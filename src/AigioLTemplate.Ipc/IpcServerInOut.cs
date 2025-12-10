#if USE_JSON_RPC
using AigioL.Common.Models;
using AigioLTemplate.Commands;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Constants;
using AigioLTemplate.Hosting;
using AigioLTemplate.Ipc;
using AigioLTemplate.Ipc.Abstractions;
using AigioLTemplate.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Reactive.Disposables;
using System.Text;
using static AigioLTemplate.Ipc.PipeHelper;

namespace AigioLTemplate.Ipc;

sealed class IpcServerInOut : IpcServerBase
{
    internal IpcServerInOut(string pipeName) : base(Log.CreateLogger<IpcServerInOut>(), pipeName)
    {
    }

    protected override async void RunCore()
    {
        var s = pipeStream;
        if (s == null || DisposedValue)
        {
            return;
        }
        var cancellationToken = disposedTokenSource.Token;
        try
        {
            // 在后台线程中等待连接
            s.WaitForConnection();

            // 开始监听 JSON-RPC 请求
            byte[] b4 = new byte[sizeof(int)];
            while (!cancellationToken.IsCancellationRequested)
            {
                string? commandName = null;
                var len = ReadInt32(s, b4, StreamMark.ReadCommandNameLengthInt32);

                // 读取函数名
                nint funcptr = default;
                var method_u8_len = len;
                var method_u8_b = ArrayPool<byte>.Shared.Rent(method_u8_len);
                var method_u8_s = method_u8_b.AsSpan(0, method_u8_len);
                try
                {
                    ReadBytes(s, method_u8_s, method_u8_len, StreamMark.ReadCommandNameBytes);
                    // 函数名 UTF-8 字节转换 UTF-16 字符
                    var method_char_len = Encoding.UTF8.GetMaxByteCount(method_u8_len);
                    var method_char_b = ArrayPool<char>.Shared.Rent(method_char_len);
                    var method_char_s = method_char_b.AsSpan(0, method_char_len);
                    try
                    {
                        if (!Encoding.UTF8.TryGetChars(method_u8_s, method_char_s, out var charsWritten))
                        {
                            throw new ArgumentOutOfRangeException(nameof(method_u8_len), method_u8_len,
                                "UTF-8 字节转换 Char 缓冲区太小，计算长度错误");
                        }
                        method_char_s = method_char_s[..charsWritten];
                        commandName = new(method_char_s);
                        funcptr = CommandHelpers.TryGetValue(commandName);
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(method_char_b);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(method_u8_b);
                }

                // 读取请求消息
                len = ReadInt32(s, b4, StreamMark.ReadReqMessageSizeInt32);

                byte[]? inputBuffer = null;
                var outputStream = m.GetStream();
                long rspMessageLen64;
                try
                {
                    try
                    {
                        var reqMessageSize = len;
                        if (reqMessageSize != 0)
                        {
                            inputBuffer = ArrayPool<byte>.Shared.Rent(reqMessageSize);
                            ReadBytes(s, inputBuffer, reqMessageSize, StreamMark.ReadReqMessageBytes);
                        }
#if DEBUG
                        var reqMessageString = inputBuffer == null ? null : Encoding.UTF8.GetString(inputBuffer, 0, reqMessageSize);
                        logger.LogDebug("接收到请求：{commandName}，消息长度：{reqMessageSize}，内容：{reqMessageString}",
                            commandName, reqMessageSize, reqMessageString);
#endif
                        if (funcptr != default)
                        {
                            // 使用池化内存创建的流不需要释放，已由池释放
                            MemoryStream? inputStream = inputBuffer == null ? null : new(inputBuffer, 0, reqMessageSize, false);

                            // 通过函数指针调用业务函数
                            ValueTask t;
                            unsafe
                            {
                                var funcptr_d = (delegate* managed<SerializableImplType, Stream?, Stream, CancellationToken, ValueTask>)funcptr;
                                t = funcptr_d(SerializableImplType.SystemTextJson, inputStream, outputStream, cancellationToken);
                            }
                            await t;
                        }
                        else
                        {
                            // 找不到业务函数，返回 NotFound
                            await ApiRsp.SerializeAsync(HttpStatusCode.NotFound, SerializableImplType.SystemTextJson, outputStream, cancellationToken);
                        }
                        rspMessageLen64 = outputStream.Length;
                        if (rspMessageLen64 > int.MaxValue)
                        {
                            outputStream.Dispose();
                            outputStream = m.GetStream();
                            // 流长度超过 int.MaxValue，返回 BadGateway
                            await ApiRsp.SerializeAsync(HttpStatusCode.BadGateway, SerializableImplType.SystemTextJson, outputStream, cancellationToken);
                        }
                    }
                    finally
                    {
                        if (inputBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(inputBuffer);
                        }
                    }

                    // 写入响应消息
                    outputStream.Position = 0;
                    rspMessageLen64 = outputStream.Length;
                    if (rspMessageLen64 > int.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException(nameof(rspMessageLen64), rspMessageLen64, "响应消息长度超过 int.MaxValue");
                    }
#if DEBUG
                    var rspMessageString = rspMessageLen64 > 0 ? Encoding.UTF8.GetString(outputStream.GetReadOnlySequence()) : null;
                    logger.LogDebug("发送响应：{commandName}，消息长度：{rspMessageLen64}，内容：{rspMessageString}",
                        commandName, rspMessageLen64, rspMessageString);
                    outputStream.Position = 0;
#endif
                    var rspMessageLen = unchecked((int)rspMessageLen64);
                    BinaryPrimitives.WriteInt32LittleEndian(b4, rspMessageLen);
                    s.Write(b4);
                    outputStream.CopyTo(s);

                    // 完成写入
                    s.Flush();
                }
                finally
                {
                    outputStream.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            CommandHelpers.OnNamedPipeServerStreamError(logger, ex, pipeName);
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(ex);
#endif
            lock (lockDispose)
            {
                if (DisposedValue)
                {
                    return;
                }

                pipeStream = PipeHelper.CreateNamedPipeServerStream(pipeName, maxNumberOfServerInstances);
                RunCore(); // 继续运行
            }
        }
    }
}
#endif
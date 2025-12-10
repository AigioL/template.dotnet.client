#if USE_JSON_RPC
using AigioLTemplate.Commands;
using AigioLTemplate.Ipc.Abstractions;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace AigioLTemplate.Ipc;

sealed class IpcServerOut : IpcServerBase
{
    internal IpcServerOut(string pipeName) : base(Log.CreateLogger<IpcServerOut>(), pipeName)
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
                (var createTime, var timeOut, var rspMessageBytes) = await OutChannel.Reader.ReadAsync(cancellationToken);
                if (timeOut.HasValue && createTime.HasValue)
                {
                    if (DateTimeOffset.Now - createTime.Value >= timeOut.Value)
                    {
                        // 超时，丢弃
                        continue;
                    }
                }

                int rspMessageLen64 = rspMessageBytes.Length;
#if DEBUG
                var rspMessageString = rspMessageBytes.Length > 0 ? Encoding.UTF8.GetString(rspMessageBytes) : null;
                logger.LogDebug("发送响应：消息长度：{rspMessageLen64}，内容：{rspMessageString}",
                    rspMessageLen64, rspMessageString);
#endif
                BinaryPrimitives.WriteInt32LittleEndian(b4, rspMessageLen64);
                s.Write(b4);
                s.Write(rspMessageBytes);

                // 完成写入
                s.Flush();
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

    Channel<(DateTimeOffset?, TimeSpan?, byte[])> OutChannel { get; } = Channel.CreateBounded<(DateTimeOffset?, TimeSpan?, byte[])>(new BoundedChannelOptions(100)
    {
        // 使用有界通道，最大容量为 100 条消息，超过时丢弃最旧的消息
        FullMode = BoundedChannelFullMode.DropOldest,
    });

    internal async void Write((DateTimeOffset?, TimeSpan?, byte[]) item)
    {
        await OutChannel.Writer.WriteAsync(item);
    }
}
#endif
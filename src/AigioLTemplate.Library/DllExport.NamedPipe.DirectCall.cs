using AigioL.Common.Models;
using AigioLTemplate.Commands;
using Microsoft.IO;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using static AigioLTemplate.DirectCall;
using ApiRspCode = AigioLTemplate.Models.ApiRspCode;

namespace AigioLTemplate;

static partial class DllExport // 直接调用
{
    /// <summary>
    /// 库模式下发送消息执行 C# 业务函数，使用回调函数指针接收响应消息（JS => C#）
    /// </summary>
    /// <param name="taskId">自增的任务 Id，用于在异步函数或队列中的请求与响应对应关系</param>
    /// <param name="commandName_len">业务函数名 UTF-16 字符串长度</param>
    /// <param name="commandName_ptr">业务函数名字符数组指针</param>
    /// <param name="reqMessage_len">请求消息字节长度</param>
    /// <param name="reqMessage_ptr">请求消息字节数组指针</param>
    /// <param name="rspMessage_callback">响应消息回调函数</param>
    /// <returns></returns>
    static unsafe int PostMessageDirectCall(int taskId, int commandName_len, char* commandName_ptr, int reqMessage_len, byte* reqMessage_ptr, delegate* unmanaged<int, int, byte*, void> rspMessage_callback)
    {
        try
        {
            string commandName = new(commandName_ptr, 0, commandName_len);
            var funcptr = CommandHelpers.TryGetValue(commandName);
            Stream reqMessage;
            if (reqMessage_len <= 0)
            {
                reqMessage = new MemoryStream([], false);
            }
            else
            {
                reqMessage = new UnmanagedMemoryStream(reqMessage_ptr, reqMessage_len);
            }
            PostMessageSendAsync(funcptr, taskId, commandName, reqMessage, (nint)rspMessage_callback);
            return (int)ApiRspCode.OK;
        }
        catch (Exception e)
        {
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(e);
#endif
            SendCatchMessage(taskId, commandName_len, commandName_ptr, ApiRspCode.Exception, e, rspMessage_callback);
            return (int)ApiRspCode.OK;
        }
    }

    static unsafe int SetOnMessageReceivedListenerDirectCall(delegate* unmanaged<int, byte*, void> onMessageReceivedListener)
    {
        CommandHelpers.OnMessageReceivedListener = onMessageReceivedListener;
        return (int)ApiRspCode.OK;
    }
}

file static class DirectCall
{
    internal static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 控制是否用数组池中租借的数组进行 GC 固定传递给非托管使用
    /// </summary>
    static readonly bool UseArrayPoolPinned = true;

    internal static async void PostMessageSendAsync(nint funcptr, int taskId, string commandName, Stream inputStream, nint rspMessage_callback, CancellationToken cancellationToken = default)
    {
        // 此函数返回值调用，回调函数指针传递，此函数执行完后必须调用仅一次的回调函数！

        var outputStream = m.GetStream(); // 创建响应流
        try
        {
            if (funcptr != default) // 找到业务函数指针
            {
                ValueTask t;
                unsafe // 指针类型转换为函数指针
                {
                    var funcptr_d = (delegate* managed<SerializableImplType, Stream?, Stream, CancellationToken, ValueTask>)funcptr;
                    t = funcptr_d(SerializableImplType.SystemTextJson, inputStream, outputStream, cancellationToken);
                }
                await t; // 执行业务函数

                // 开始向回调函数指针传入响应流中的字节数据
                bool isRentRspMessage = false;
                byte[] rspMessage;
                if (UseArrayPoolPinned)
                {
                    var rspMessageRos = outputStream.GetReadOnlySequence();
                    if (rspMessageRos.Length < int.MaxValue)
                    {
                        isRentRspMessage = true;
                        var rspMessageLen = unchecked((int)rspMessageRos.Length);
                        rspMessage = ArrayPool<byte>.Shared.Rent(rspMessageLen); // 从池中租借数组并拷贝数据
                        rspMessageRos.CopyTo(rspMessage.AsSpan(0, rspMessageLen));
                    }
                    else
                    {
                        rspMessage = outputStream.GetReadOnlySequence().ToArray(); // 创建数组并拷贝数据
                    }
                }
                else
                {
                    rspMessage = outputStream.GetReadOnlySequence().ToArray(); // 创建数组并拷贝数据
                }
                try
                {
                    // 固定数组在 GC 中
                    var handle = GCHandle.Alloc(rspMessage, GCHandleType.Pinned);
                    try
                    {
                        var rspMessagePtr = handle.AddrOfPinnedObject();
                        var rspMessage_callback_ = (delegate* unmanaged<int, int, byte*, void>)rspMessage_callback;
                        unsafe
                        {
                            // 调用非托管函数
                            rspMessage_callback_(taskId, rspMessage.Length, (byte*)rspMessagePtr);
                        }
                    }
                    finally
                    {
                        handle.Free(); // 取消 GC 固定
                    }
                }
                finally
                {
                    if (isRentRspMessage)
                    {
                        ArrayPool<byte>.Shared.Return(rspMessage); // 归还池化内存
                    }
                }
            }
            else
            {
                // 找不到业务函数，返回 NotFound
                await ApiRsp.SerializeAsync(HttpStatusCode.NotFound, SerializableImplType.SystemTextJson, outputStream, cancellationToken);
            }
        }
        catch (Exception e)
        {
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(e);
#endif
            ApiRsp rsp = new()
            {
                Code = (uint)ApiRspCode.Exception,
                Message = e.ToString(),
                Url = commandName,
            };
            await ApiRsp.SerializeAsync(rsp, SerializableImplType.SystemTextJson, outputStream, cancellationToken);
        }
    }
}
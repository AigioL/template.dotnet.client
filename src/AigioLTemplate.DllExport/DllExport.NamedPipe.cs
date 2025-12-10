using AigioLTemplate.Models;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace AigioLTemplate;

static unsafe partial class DllExport
{
    static string GetPipeNameJsonRpc(int backendProcessId, PipeDirection pipeDirection)
        => $"aigioltemplate_{backendProcessId}_{(int)pipeDirection}";

    /// <summary>
    /// 发送消息到 JSON-RPC 服务端，使用回调函数指针接收响应消息（JS => C#）
    /// </summary>
    /// <param name="taskId">自增的任务 Id，用于在异步函数或队列中的请求与响应对应关系</param>
    /// <param name="commandName_len">业务函数名 UTF-16 字符串长度</param>
    /// <param name="commandName_ptr">业务函数名字符数组指针</param>
    /// <param name="reqMessage_len">请求消息字节长度</param>
    /// <param name="reqMessage_ptr">请求消息字节数组指针</param>
    /// <param name="rspMessage_callback">响应消息回调函数</param>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate4")]
    public static int PostMessage(int taskId, int commandName_len, [NotNull] char* commandName_ptr, int reqMessage_len, byte* reqMessage_ptr, [NotNull] delegate* unmanaged<int, int, byte*, void> rspMessage_callback)
    {
        if (commandName_len <= 0 || commandName_ptr == default || rspMessage_callback == default)
        {
            // 禁止传入空指针
            SendCatchMessage(taskId, commandName_len, commandName_ptr, ApiRspCode.ArgumentNullException, null, rspMessage_callback);
            return (int)ApiRspCode.OK;
        }
        lock (lockBackend)
        {
            var checkBPHNE = CheckBackendProcessHasNotExited();
            if (checkBPHNE.HasValue)
            {
                SendCatchMessage(taskId, commandName_len, commandName_ptr, checkBPHNE.Value, null, rspMessage_callback);
                return (int)ApiRspCode.OK;
            }
            //bool is_call_rspMessage_delegate = false;
            try
            {
                IpcClientInOut ipcClientInOut;
                try
                {
                    var pipeName = GetPipeNameJsonRpc(backendProcessId, PipeDirection.InOut);
                    ipcClientInOut = IpcClientInOut.GetInstance(pipeName);
                }
                catch (TimeoutException ex)
                {
                    SendCatchMessage(taskId, commandName_len, commandName_ptr, ApiRspCode.IpcConnectTimeout, ex, rspMessage_callback);
                    return (int)ApiRspCode.OK;
                }
                ReadOnlySpan<char> commandName = new(commandName_ptr, commandName_len);
                Span<byte> commandNameU8 = stackalloc byte[Encoding.UTF8.GetMaxByteCount(commandName_len)];
                commandNameU8 = commandNameU8[..Encoding.UTF8.GetBytes(commandName, commandNameU8)];
                ReadOnlySpan<byte> reqMessage = reqMessage_len == 0 ? default : new(reqMessage_ptr, reqMessage_len);
                var result = ipcClientInOut.PostMessage(commandNameU8, reqMessage, out var rspMessage, out var rspMessageSize);
                if (rspMessage != null && rspMessage.Length != 0)
                {
                    var handle = GCHandle.Alloc(rspMessage, GCHandleType.Pinned);
                    try
                    {
                        var rspMessagePtr = handle.AddrOfPinnedObject();
                        rspMessage_callback(taskId, rspMessageSize, (byte*)rspMessagePtr);
                        //is_call_rspMessage_delegate = true;
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                return (int)result;
            }
#pragma warning disable CS0168 // 声明了变量，但从未使用过
            catch (EndOfStreamException e)
            {
                SendCatchMessage(taskId, commandName_len, commandName_ptr, ApiRspCode.EndOfStreamException, e, rspMessage_callback);
                return (int)ApiRspCode.OK;
            }
            catch (IOException e)
            {
#if DEBUG
                // 在调试模式下，让 IDE 定位到异常位置
                Debugger.BreakForUserUnhandledException(e);
#endif
                SendCatchMessage(taskId, commandName_len, commandName_ptr, ApiRspCode.IOException, e, rspMessage_callback);
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
#pragma warning restore CS0168 // 声明了变量，但从未使用过
            //finally
            //{
            //    if (!is_call_rspMessage_delegate)
            //    {
            //        // 确保回调函数被调用，即使发生异常
            //        rspMessage_callback(taskId, 0, null);
            //    }
            //}
        }
    }

    /// <summary>
    /// 设置（C# => JS）的订阅接收消息事件的回调函数指针
    /// </summary>
    /// <param name="onMessageReceivedListener"></param>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate3")]
    public static int SetOnMessageReceivedListener(delegate* unmanaged<int, byte*, void> onMessageReceivedListener)
    {
        if (onMessageReceivedListener == default)
        {
            // 禁止传入空指针
            return (int)ApiRspCode.ArgumentNullException;
        }
        lock (lockBackend)
        {
            var checkBPHNE = CheckBackendProcessHasNotExited();
            if (checkBPHNE.HasValue)
            {
                return (int)checkBPHNE.Value;
            }
            IpcClientOut.OnMessageReceivedListener = onMessageReceivedListener;
            IpcClientOut ipcClientOut;
            try
            {
                var pipeName = GetPipeNameJsonRpc(backendProcessId, PipeDirection.Out);
                ipcClientOut = IpcClientOut.GetInstance(pipeName);
            }
            catch (TimeoutException ex)
            {
                SendCatchMessage(onMessageReceivedListener, ex);
            }
        }
        return (int)ApiRspCode.OK;
    }

    /// <summary>
    /// 调用 WebApi 的 <see cref="HttpClient.Send(HttpRequestMessage)"/> 方法，使用回调函数指针接收响应消息（JS => C#）
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="methodName_len"></param>
    /// <param name="methodName_ptr"></param>
    /// <param name="reqMessage_len"></param>
    /// <param name="reqMessage_ptr"></param>
    /// <param name="rspMessage_callback"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate2")]
    public static int Send(int taskId, int methodName_len, [NotNull] char* methodName_ptr, int reqMessage_len, byte* reqMessage_ptr, [NotNull] delegate* unmanaged<int, int, byte*, void> rspMessage_callback)
    {
        if (methodName_len <= 0 || methodName_ptr == default || rspMessage_callback == default)
        {
            // 禁止传入空指针
            return (int)ApiRspCode.ArgumentNullException;
        }
        return 0;
    }
}

file abstract partial class IpcClientBase
{
    protected readonly string pipeName;
    protected readonly PipeDirection pipeDirection;

    protected NamedPipeClientStream? pipeStream;

    protected IpcClientBase(string pipeName, PipeDirection pipeDirection)
    {
        this.pipeName = pipeName;
        this.pipeDirection = pipeDirection;
    }

    protected void Run()
    {
        if (pipeStream == null)
        {
            pipeStream = new NamedPipeClientStream(".", pipeName, pipeDirection, PipeOptions.Asynchronous);
            pipeStream.Connect(TimeSpan.FromSeconds(5));
        }

        Run(pipeStream);
    }

    protected virtual void Run(NamedPipeClientStream pipeStream)
    {
    }

    protected static int ReadInt32(Stream s, Span<byte> buffer, StreamMark mark)
    {
        var span = buffer[..sizeof(int)];
        var len = s.Read(buffer);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
        var result = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        return result;
    }

    protected static void ReadBytes(Stream s, Span<byte> buffer, int bufferSize, StreamMark mark)
    {
        var span = buffer[..bufferSize];
        var len = s.Read(buffer);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
    }

    internal enum StreamMark : byte
    {
        ReadRspMessageSize,
        ReadRspMessageBytes,
    }

    protected static async void InBackground(Action action, bool longRunning = true, CancellationToken cancellationToken = default)
    {
        TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

        if (longRunning)
        {
            options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
        }

        await Task.Factory.StartNew(action, cancellationToken, options, TaskScheduler.Default).ConfigureAwait(false);
    }
}

file abstract partial class IpcClientBase : IDisposable
{
    bool disposedValue;

    protected bool DisposedValue => disposedValue;

    protected virtual void DisposeCore(bool disposing)
    {
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                pipeStream?.Dispose();
            }
            DisposeCore(disposing);

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            pipeStream = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

file sealed class IpcClientInOut : IpcClientBase
{
    internal IpcClientInOut(string pipeName) : base(pipeName, PipeDirection.InOut)
    {
    }

    static IpcClientInOut? instance;

    internal static IpcClientInOut GetInstance(string pipeName)
    {
        if (instance != null)
        {
            if (instance.pipeName == pipeName)
            {
                return instance;
            }
            else
            {
                instance.Dispose();
                instance = null;
            }
        }
        IpcClientInOut? ipcClientInOut = null;
        try
        {
            ipcClientInOut = new(pipeName);
            ipcClientInOut.Run();
            instance = ipcClientInOut;
            return instance;
        }
        catch
        {
            ipcClientInOut?.Dispose();
            instance = null;
            throw;
        }
    }

    internal ApiRspCode PostMessage(ReadOnlySpan<byte> commandName, ReadOnlySpan<byte> reqMessage, out byte[]? rspMessage, out int rspMessageSize)
    {
        rspMessage = null;
        rspMessageSize = 0;
        var s = pipeStream;
        if (s == null)
        {
            return ApiRspCode.PipeStreamIsNull;
        }

#if DEBUG
        Console.WriteLine($"PostMessage，pipeStream：{s.GetHashCode()}，IsConnected：{s.IsConnected}，CurrentThread：{Environment.CurrentManagedThreadId}");
#endif

        Span<byte> b4 = stackalloc byte[sizeof(int)];

        // 写入函数名
        BinaryPrimitives.WriteInt32LittleEndian(b4, commandName.Length);
        s.Write(b4);
        s.Write(commandName);

        // 写入请求消息
        BinaryPrimitives.WriteInt32LittleEndian(b4, reqMessage.Length);
        s.Write(b4);
        s.Write(reqMessage);

        // 完成写入
        s.Flush();

        var len = ReadInt32(s, b4, StreamMark.ReadRspMessageSize);

        // 读取响应消息
        rspMessageSize = len;
        rspMessage = ArrayPool<byte>.Shared.Rent(rspMessageSize);
        try
        {
            ReadBytes(s, rspMessage, rspMessageSize, StreamMark.ReadRspMessageBytes);
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(rspMessage);
            rspMessage = null;
            throw;
        }

        return ApiRspCode.OK;
    }
}

file sealed class IpcClientOut : IpcClientBase
{
    readonly CancellationTokenSource disposedTokenSource = new();

    internal IpcClientOut(string pipeName) : base(pipeName, PipeDirection.InOut)
    {
    }

    static IpcClientOut? instance;

    internal static IpcClientOut GetInstance(string pipeName)
    {
        if (instance != null)
        {
            if (instance.pipeName == pipeName)
            {
                return instance;
            }
            else
            {
                instance.Dispose();
                instance = null;
            }
        }
        IpcClientOut? ipcClientOut = null;
        try
        {
            ipcClientOut = new(pipeName);
            ipcClientOut.Run();
            instance = ipcClientOut;
            return instance;
        }
        catch
        {
            ipcClientOut?.Dispose();
            instance = null;
            throw;
        }
    }

    internal static unsafe delegate* unmanaged<int, byte*, void> OnMessageReceivedListener { private get; set; }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        disposedTokenSource.Cancel();
    }

    static unsafe void SendCatchMessage(Exception ex) => DllExport.SendCatchMessage(OnMessageReceivedListener, ex);

    protected override void Run(NamedPipeClientStream s)
    {
        var cancellationToken = disposedTokenSource.Token;
        InBackground(() =>
        {
            byte[]? rspMessage = null;
            int rspMessageSize = 0;
            Span<byte> b4 = stackalloc byte[sizeof(int)];
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var len = ReadInt32(s, b4, StreamMark.ReadRspMessageSize);

                    // 读取响应消息
                    rspMessageSize = len;
                    rspMessage = ArrayPool<byte>.Shared.Rent(rspMessageSize);
                    try
                    {
                        try
                        {
                            ReadBytes(s, rspMessage, rspMessageSize, StreamMark.ReadRspMessageBytes);
                        }
                        catch
                        {
                            ArrayPool<byte>.Shared.Return(rspMessage);
                            rspMessage = null;
                            throw;
                        }

                        unsafe
                        {
                            var func = OnMessageReceivedListener;
                            if (func != default)
                            {
                                if (rspMessageSize > 0)
                                {
                                    var gcHandle = GCHandle.Alloc(rspMessage, GCHandleType.Pinned);
                                    try
                                    {
                                        var lpBuffer = gcHandle.AddrOfPinnedObject();
                                        func(rspMessageSize, (byte*)lpBuffer);
                                    }
                                    finally
                                    {
                                        gcHandle.Free();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (rspMessage != null)
                            ArrayPool<byte>.Shared.Return(rspMessage);
                    }
                }
                catch (Exception ex)
                {
                    SendCatchMessage(ex);
                    Dispose();
                    return;
                }
            }
        }, cancellationToken: cancellationToken);
    }
}
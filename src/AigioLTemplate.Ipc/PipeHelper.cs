using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace AigioLTemplate.Ipc;

/// <summary>
/// 管道帮助类
/// </summary>
static partial class PipeHelper
{
    internal static string GetPipeName(string pipeUrl) // static string _3(string _0)
    {
        const string prefix = "\\\\.\\pipe\\";
        if (pipeUrl.StartsWith(prefix))
        {
            pipeUrl = pipeUrl[prefix.Length..];
        }
        return pipeUrl;
    }

    internal static ReadOnlySpan<byte> GetPipeName(ReadOnlySpan<byte> pipeUrl)
    {
        var prefix = "\\\\.\\pipe\\"u8;
        if (pipeUrl.StartsWith(prefix))
        {
            pipeUrl = pipeUrl[prefix.Length..];
        }
        return pipeUrl;
    }

    /// <summary>
    /// 根据名称创建命名管道服务流
    /// </summary>
    internal static NamedPipeServerStream CreateNamedPipeServerStream(string pipeName, int maxNumberOfServerInstances = 1)
    {
        pipeName = GetPipeName(pipeName);

        const PipeDirection direction = PipeDirection.InOut;
        const PipeTransmissionMode transmissionMode = PipeTransmissionMode.Byte;
        const PipeOptions options = PipeOptions.Asynchronous;

#if WINDOWS
        PipeSecurity pipeSecurity = new();

        // 1. 允许所有用户 (Everyone) 读写权限
        var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        pipeSecurity.AddAccessRule(new PipeAccessRule(
               everyoneSid,
               PipeAccessRights.ReadWrite,
               AccessControlType.Allow
           ));

        // 2. 允许应用容器 (ALL APPLICATION PACKAGES) 读写权限
        var appContainerSid = new SecurityIdentifier(WellKnownSidType.WinBuiltinAnyPackageSid, null);
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            appContainerSid,
            PipeAccessRights.ReadWrite,
            AccessControlType.Allow
        ));

        //// 3. 允许当前用户完全控制
        //var currentUserSid = WindowsIdentity.GetCurrent().User;
        //if (currentUserSid != null)
        //{
        //    pipeSecurity.AddAccessRule(new PipeAccessRule(
        //        currentUserSid.Value,
        //        PipeAccessRights.FullControl,
        //        AccessControlType.Allow
        //    ));
        //}

        // 4. 允许管理员组完全控制
        var adminsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            adminsSid,
            PipeAccessRights.FullControl,
            AccessControlType.Allow
        ));

        if (Environment.IsPrivilegedProcess)
        {
            // 管理员权限管道需要此配置以允许非管理员权限进程连接
            SecurityIdentifier securityIdentifier = new(WellKnownSidType.AuthenticatedUserSid, null);
            pipeSecurity.AddAccessRule(new PipeAccessRule(securityIdentifier,
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
                AccessControlType.Allow));
        }

        const int inBufferSize = 0;
        const int outBufferSize = 0;
        var result = NamedPipeServerStreamAcl.Create(pipeName,
            direction, maxNumberOfServerInstances, transmissionMode,
            options, inBufferSize, outBufferSize, pipeSecurity);
        return result;
#endif

        //var result = new NamedPipeServerStream(name, direction, maxNumberOfServerInstances, transmissionMode, options);
        //return result;
    }

    /// <summary>
    /// 从流中跳过指定长度的字节
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    /// <param name="skipLength"></param>
    /// <param name="mark"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static void SkipRead(Stream stream, byte[] buffer, int skipLength, StreamMark mark)
    {
        var span = buffer.AsSpan(0, skipLength);
        var len = stream.ReadAtLeast(span, span.Length, false);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，skipLength：{skipLength}");
        }
    }

    /// <summary>
    /// 从流中读取一个 <see cref="double"/>，使用小端 (Little Endian) 字节序
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static double ReadDouble(Stream stream, byte[] buffer, StreamMark mark)
    {
        // 读取 (8 字节, DoubleLE)
        var span = buffer.AsSpan(0, sizeof(double));
        var len = stream.ReadAtLeast(span, span.Length, false);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
        return BinaryPrimitives.ReadDoubleLittleEndian(span);
    }

    /// <summary>
    /// 从流中读取一个 <see cref="uint"/>，使用小端 (Little Endian) 字节序
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static uint ReadUInt32(Stream stream, byte[] buffer, StreamMark mark)
    {
        // 读取 (4 字节)
        var span = buffer.AsSpan(0, sizeof(uint));
        var len = stream.ReadAtLeast(span, span.Length, false);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
        return BinaryPrimitives.ReadUInt32LittleEndian(span);
    }

    internal static uint ReadUInt32SkipRead4(Stream stream, byte[] buffer, StreamMark mark)
    {
        // 读取 (4 字节) + 跳过 4 字节的保留字段
        var span = buffer.AsSpan(0, sizeof(uint));
        var len = stream.ReadAtLeast(span, span.Length, false);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
        var messageType = BinaryPrimitives.ReadUInt32LittleEndian(span[..4]);
        stream.ReadAtLeast(buffer.AsSpan(0, 4), 4, false);
        return messageType;
    }

    /// <summary>
    /// 向流中写入一个 UTF-16 编码的字符串
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    internal static void WriteUTF16String(Stream stream, string value)
    {
        var len = Encoding.Unicode.GetMaxByteCount(value.Length);
        var buffer = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            len = Encoding.Unicode.GetBytes(value, buffer);
            stream.Write(buffer, 0, len);
            stream.Flush();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    internal static int ReadInt32(Stream s, Span<byte> buffer, StreamMark mark)
    {
        var span = buffer[..sizeof(int)];
        var len = s.ReadAtLeast(span, span.Length, false);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
        var result = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        return result;
    }

    internal static void ReadBytes(Stream s, Span<byte> buffer, int bufferSize, StreamMark mark)
    {
        var span = buffer[..bufferSize];
        var len = s.ReadAtLeast(span, span.Length, false);
        if (len != span.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(len), len,
                $"读取 {mark} 失败，spanLength：{span.Length}");
        }
    }

    internal enum StreamMark : byte
    {
        /// <summary>
        /// 读取 logLevel (4 字节)
        /// </summary>
        ReadLogLevelUInt32,

        /// <summary>
        /// 跳过 4 字节的保留字段
        /// </summary>
        ReadSkipReadLogLevel4,

        /// <summary>
        /// 读取 messageLength (4 字节)
        /// </summary>
        ReadMessageLengthUInt32,

        /// <summary>
        /// 跳过 4 字节的保留字段
        /// </summary>
        ReadSkipReadMessageLength4,

        /// <summary>
        /// 读取 variableValue (8 字节, DoubleLE)
        /// </summary>
        ReadVariableValueDouble,

        /// <summary>
        /// 变量值更新尝试读取 8 字节的保留字段
        /// </summary>
        ReadSkipReadVariableValueUpdate8,

        /// <summary>
        /// 读取 messageType (4 字节) + 跳过 4 字节的保留字段，总计 8 字节
        /// </summary>
        ReadMessageTypeUInt32WithSkipRead4,

        /// <summary>
        /// 读取业务命令名称长度 (4 字节, Int32)
        /// </summary>
        ReadCommandNameLengthInt32,

        /// <summary>
        /// 读取业务命令名称 UTF-8 字节
        /// </summary>
        ReadCommandNameBytes,

        /// <summary>
        /// 读取业务命令请求参数长度 (4 字节, Int32)
        /// </summary>
        ReadReqMessageSizeInt32,

        /// <summary>
        /// 读取业务命令请求参数 UTF-8 字节
        /// </summary>
        ReadReqMessageBytes,

        ReadResponseMessageId,

        ReadResponseResult,
    }
}

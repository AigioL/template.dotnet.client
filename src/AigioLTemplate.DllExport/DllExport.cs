using AigioL.Common.Models;
using AigioLTemplate.Models;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using ApiRspCode = AigioLTemplate.Models.ApiRspCode;

namespace AigioLTemplate;

/// <summary>
/// 提供本机函数导出定义
/// </summary>
public static unsafe partial class DllExport
{
    internal static void SendCatchMessage(delegate* unmanaged<int, byte*, void> func, Exception ex)
    {
        if (func != default)
        {
            // 🧪 AigioLTemplate.UnitTest.JsonTest.MessageReceivedKeyType_Exception
            ReadOnlySpan<byte> l =
"""
{"k": 500,"v": "
"""u8; // 500 = MessageReceivedKeyType.Exception
            ReadOnlySpan<byte> r =
"""
"}
"""u8;
            var exString = ex.ToString() ?? "";
            var exBufferLength = Encoding.UTF8.GetByteCount(exString);
            var exBuffer = ArrayPool<byte>.Shared.Rent(exBufferLength);
            Span<byte> exSpan = exBuffer.AsSpan(0, exBufferLength);
            ReadOnlySpan<byte> exSpanEncoded;
            try
            {
                Encoding.UTF8.TryGetBytes(exString, exSpan, out var bytesWritten);
                exSpan = exSpan[..bytesWritten];
                exSpanEncoded = JsonEncodedText.Encode(exSpan).EncodedUtf8Bytes;

                byte[] rspMessage = new byte[l.Length + exSpanEncoded.Length + r.Length];
                l.CopyTo(rspMessage);
                exSpanEncoded.CopyTo(rspMessage.AsSpan(l.Length));
                r.CopyTo(rspMessage.AsSpan(l.Length + exSpanEncoded.Length));

                var gcHandle = GCHandle.Alloc(rspMessage, GCHandleType.Pinned);
                try
                {
                    var lpBuffer = gcHandle.AddrOfPinnedObject();
                    func(rspMessage.Length, (byte*)lpBuffer);
                }
                finally
                {
                    gcHandle.Free();
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(exBuffer);
            }
        }
    }

    internal static void SendCatchMessage(int taskId, int commandName_len, char* commandName_ptr, ApiRspCode code, Exception? ex, delegate* unmanaged<int, int, byte*, void> rspMessage_callback)
    {
        ApiRsp rsp = new()
        {
            Code = (uint)code,
            Message = ex?.Message,
            Url = new string(commandName_ptr, 0, commandName_len),
        };
        var rspMessage = JsonSerializer.SerializeToUtf8Bytes(rsp, DefaultJsonSerializerContext_.Default.ApiRsp);
        var gcHandle = GCHandle.Alloc(rspMessage, GCHandleType.Pinned);
        try
        {
            var lpBuffer = gcHandle.AddrOfPinnedObject();
            rspMessage_callback(taskId, rspMessage.Length, (byte*)lpBuffer);
        }
        finally
        {
            gcHandle.Free();
        }
    }
}

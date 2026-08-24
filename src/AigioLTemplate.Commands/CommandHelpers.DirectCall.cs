#if !USE_JSON_RPC
using System.Runtime.InteropServices;

namespace AigioLTemplate.Commands;

static partial class CommandHelpers
{
    public static class OutPool
    {
        public static void Write(byte[] content, TimeSpan? timeOut = null)
        {
            var handle = GCHandle.Alloc(content, GCHandleType.Pinned);
            try
            {
                var ptr = (byte*)handle.AddrOfPinnedObject();
                unsafe
                {
                    Write(ptr, content.Length, timeOut);
                }
            }
            finally
            {
                handle.Free();
            }
        }

#pragma warning disable IDE0060 // 删除未使用的参数
        static unsafe void Write(byte* content_ptr, int content_len, TimeSpan? timeOut = null)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            // timeOut 参数仅兼容 JSON_RPC 模式，无作用，因库模式的写入回调是即时执行的，不存在超时的情况
            var funptr = OnMessageReceivedListener;
            if (funptr != default)
            {
                funptr(content_len, content_ptr);
            }
        }
    }

    internal static unsafe delegate* unmanaged<int, byte*, void> OnMessageReceivedListener { private get; set; }
}
#endif
#if USE_JSON_RPC
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
using System.Threading.Tasks;

namespace AigioLTemplate.Ipc;

sealed class IpcServerOutPool : IDisposable
{
    readonly CompositeDisposable disposables;
    readonly IpcServerOut[] servers;
    readonly ILogger logger;
    bool disposedValue;

    internal IpcServerOutPool(string pipeName)
    {
        logger = Log.CreateLogger<IpcServerOutPool>();
        servers = new IpcServerOut[IpcServerBase.maxNumberOfServerInstances];
        for (int i = 0; i < servers.Length; i++)
        {
            var it = servers[i] = new(pipeName);
            it.Connecting += Connecting;
        }
        disposables = new(servers);
        CommandHelpers.StartNamedPipeServerStream(logger, pipeName);
    }

    public event EventHandler<IpcConnectingEventArgs>? Connecting;

    internal void Run()
    {
        for (int i = 0; i < servers.Length; i++)
        {
            servers[i].Run();
        }
    }

    static readonly RecyclableMemoryStreamManager m = new();

    internal static RecyclableMemoryStream GetStream() => m.GetStream();

    internal void Write(byte[] content, TimeSpan? timeOut = null)
    {
        (DateTimeOffset?, TimeSpan?, byte[]) t =
            (timeOut.HasValue ? DateTimeOffset.Now : null, timeOut, content);
        for (int i = 0; i < servers.Length; i++)
        {
            var it = servers[i];
            it.Write(t);
        }
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                disposables.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
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
#endif
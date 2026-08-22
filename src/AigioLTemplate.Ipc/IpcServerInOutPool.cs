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

namespace AigioLTemplate.Ipc;

/// <summary>
/// JSON-RPC 包装器池，用于管理多个 JSON-RPC 实例
/// <para>https://github.com/AArnott/StreamJsonRpc.Sample/blob/master/StreamJsonRpc.Sample.Server/Program.cs</para>
/// <para>https://github.com/dotnet/aspnetcore/blob/main/src/Servers/Kestrel/Transport.NamedPipes/src/Internal/NamedPipeConnectionListener.cs</para>
/// </summary>
sealed class IpcServerInOutPool : IDisposable
{
    readonly CompositeDisposable disposables;
    readonly IpcServerInOut[] servers;
    readonly ILogger logger;
    bool disposedValue;

    internal IpcServerInOutPool(string pipeName)
    {
        logger = Log.CreateLogger<IpcServerInOutPool>();
        servers = new IpcServerInOut[IpcServerBase.maxNumberOfServerInstances];
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
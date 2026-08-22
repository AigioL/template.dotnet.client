#if USE_JSON_RPC
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Constants;
using AigioLTemplate.Hosting;
using AigioLTemplate.Ipc;
using AigioLTemplate.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Windows.Win32;

namespace AigioLTemplate.Ipc.Abstractions;

abstract partial class IpcServerBase
{
    internal const int maxNumberOfServerInstances = 1;

    protected readonly string pipeName;
    protected readonly ILogger logger;

    protected NamedPipeServerStream? pipeStream;

    protected IpcServerBase(ILogger logger, string pipeName)
    {
        this.logger = logger;
        this.pipeName = pipeName;
        pipeStream = PipeHelper.CreateNamedPipeServerStream(pipeName, maxNumberOfServerInstances);
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

    protected static readonly RecyclableMemoryStreamManager m = new();

    protected abstract void RunCore();

    internal void Run() => InBackground(RunCore, cancellationToken: disposedTokenSource.Token);

    internal async Task WaitForConnectionAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeStream);
        await pipeStream.WaitForConnectionAsync(cancellationToken);

        if (Connecting != null)
        {
            int? clientProcessId = null;
#if !WINDOWS
            if (OperatingSystem.IsWindows())
#endif
            {
                if (PInvoke.GetNamedPipeClientProcessId(pipeStream.SafePipeHandle, out var clientProcessId2))
                {
                    clientProcessId = unchecked((int)clientProcessId2);
                }
            }
            IpcConnectingEventArgs eventArgs = new(pipeName, clientProcessId);
            Connecting(this, eventArgs);
            if (!eventArgs.AllowConnection)
            {
                if (pipeStream.IsConnected)
                {
                    pipeStream.Disconnect();
                }
                pipeStream.Dispose();
                Dispose();
            }
        }
    }

    public event EventHandler<IpcConnectingEventArgs>? Connecting;
}

partial class IpcServerBase : IDisposable
{
    bool disposedValue;
    protected readonly CancellationTokenSource disposedTokenSource = new();
    protected readonly Lock lockDispose = new();

    protected bool DisposedValue => disposedValue;

    protected virtual void DisposeCore(bool disposing)
    {
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            lock (lockDispose)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)
                    if (pipeStream != null)
                    {
                        if (pipeStream.IsConnected)
                        {
                            pipeStream.Disconnect();
                        }
                        pipeStream.Dispose();
                    }
                }
                DisposeCore(disposing);

                // 释放未托管的资源(未托管的对象)并重写终结器
                // 将大型字段设置为 null
                pipeStream = null;
                disposedValue = true;
                disposedTokenSource.Cancel();
            }
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
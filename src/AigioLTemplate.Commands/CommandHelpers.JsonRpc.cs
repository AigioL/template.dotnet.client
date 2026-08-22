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
using System.Net;
using System.Reactive.Disposables;
using System.Text;

namespace AigioLTemplate.Commands;

static partial class CommandHelpers
{
    static void InitJsonRpc()
    {
        var pipeName = HostConstants.PipeNameJsonRpc;
        if (!string.IsNullOrWhiteSpace(pipeName))
        {
            _a4865940.InitJsonRpcCore(pipeName);
        }
    }

    internal static IpcServerOutPool OutPool => _a4865940.GetIpcServerOutPool();

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "启动命名管道：{pipeName}")]
    internal static partial void StartNamedPipeServerStream(ILogger logger, string pipeName);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "命名管道：{pipeName}")]
    internal static partial void OnNamedPipeServerStreamError(ILogger logger, Exception ex, string pipeName);
}

file static class _a4865940
{
    static IpcServerOutPool? ipcServerOutPool;

    internal static IpcServerOutPool GetIpcServerOutPool()
    {
        ArgumentNullException.ThrowIfNull(ipcServerOutPool);
        return ipcServerOutPool;
    }

    internal static void InitJsonRpcCore(string pipeName)
    {
        // TODO: 实现 Pool 池中单个断开或释放，在池中重建复原并等待连接

        pipeName = PipeHelper.GetPipeName(pipeName);

        IpcServerInOutPool ipcServerInOutPool = new(pipeName + (int)PipeDirection.InOut);
        AigioLTemplateHost.AddDisposable(ipcServerInOutPool);
        ipcServerInOutPool.Connecting += IpcServerConnecting;
        ipcServerInOutPool.Run();

        ipcServerOutPool = new(pipeName + (int)PipeDirection.Out);
        AigioLTemplateHost.AddDisposable(ipcServerOutPool);
        ipcServerOutPool.Connecting += IpcServerConnecting;
        ipcServerOutPool.Run();
    }

    static void IpcServerConnecting(object? sender, IpcConnectingEventArgs e)
    {
#if !WINDOWS
        if (OperatingSystem.IsWindows()) // 非 Windows 平台目前无法验证
#endif
        {
            if (e.ClientProcessId.HasValue)
            {
                Process? clientProc = null;
                try
                {
                    Process.TryGetProcessById(e.ClientProcessId.Value, out clientProc);
                }
                catch
                {
                }
                if (clientProc == null)
                {
                    goto Disconnect;
                }
                string? clientProcPath = null;
                try
                {
                    clientProcPath = clientProc.MainModule?.FileName;
                }
                catch
                {
                }
                if (string.IsNullOrEmpty(clientProcPath))
                {
                    goto Disconnect;
                }
                var v = AssemblyInfo.ValidateRustApp(clientProcPath);
                if (!v)
                {
                    goto Disconnect;
                }
            }
            else
            {
                goto Disconnect;
            }
            return;

        Disconnect: // 验证失败，拒绝连接到命名管道服务端流
            e.AllowConnection = false;
            return;
        }
    }
}
#endif
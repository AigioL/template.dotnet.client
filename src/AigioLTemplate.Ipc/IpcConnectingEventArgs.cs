using System.Runtime.Versioning;

namespace AigioLTemplate.Ipc;

/// <summary>
/// Ipc 连接验证参数
/// </summary>
public sealed class IpcConnectingEventArgs(string pipeName, int? clientProcessId) : EventArgs
{
    /// <summary>
    /// 服务端管道名
    /// </summary>
    public string PipeName { get; } = pipeName;

    /// <summary>
    /// 客户端进程 Id，非 Windows 或无法获取时为 <see langword="null"/>
    /// </summary>
    [SupportedOSPlatform("windows6.0.6000")]
    public int? ClientProcessId { get; } = clientProcessId;

    /// <summary>
    /// 是否允许此次连接，默认允许
    /// </summary>
    public bool AllowConnection { get; set; } = true;
}
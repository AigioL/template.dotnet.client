using AigioLTemplate.Hosting;

namespace AigioLTemplate.Constants;

/// <summary>
/// Host 常量
/// </summary>
static partial class HostConstants
{
    /// <inheritdoc cref="AigioLTemplateHost.V.ModuleName"/>
    public static string? ModuleName => AigioLTemplateHost.V.ModuleName;

    /// <inheritdoc cref="AigioLTemplateHost.V.IsMainProcess"/>
    public static bool IsMainProcess => AigioLTemplateHost.V.IsMainProcess;

    /// <inheritdoc cref="AigioLTemplateHost.V.IsConsoleApp"/>
    public static bool IsConsoleApp => AigioLTemplateHost.V.IsConsoleApp;

    /// <inheritdoc cref="AigioLTemplateHost.V.IsConsoleLineToolProcess"/>
    public static bool IsConsoleLineToolProcess => AigioLTemplateHost.V.IsConsoleLineToolProcess;

    /// <inheritdoc cref="AigioLTemplateHost.V.IsMinimize"/>
    public static bool IsMinimize => AigioLTemplateHost.V.IsMinimize;

    /// <inheritdoc cref="AigioLTemplateHost.V.StartupUrl"/>
    public static string? StartupUrl => AigioLTemplateHost.V.StartupUrl;

    /// <inheritdoc cref="AigioLTemplateHost.V.EnableDevTools"/>
    public static bool EnableDevTools => AigioLTemplateHost.V.EnableDevTools;

    /// <inheritdoc cref="AigioLTemplateHost.V.UserInteractive"/>
    public static bool UserInteractive => AigioLTemplateHost.V.UserInteractive;

    /// <inheritdoc cref="AigioLTemplateHost.V.HasIPCRoot"/>
    public static bool HasIPCRoot => AigioLTemplateHost.V.HasIPCRoot;

#if USE_JSON_RPC
    /// <inheritdoc cref="AigioLTemplateHost.V.PipeNameJsonRpc"/>
    public static string? PipeNameJsonRpc => AigioLTemplateHost.V.PipeNameJsonRpc;
#endif
}

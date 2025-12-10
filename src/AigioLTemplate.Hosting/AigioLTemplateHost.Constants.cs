using AigioLTemplate.Constants;

namespace AigioLTemplate.Hosting;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1302 // Interface names should begin with I
partial interface AigioLTemplateHost
{
    internal static class V
    {
        /// <summary>
        /// 当前的进程模块名称
        /// </summary>
        public static string? ModuleName { get; internal set; }

        /// <summary>
        /// 当前是否为主进程
        /// </summary>
        public static bool IsMainProcess { get; internal set; }

        /// <summary>
        /// 当前是否有参数为 <see cref="CrashConstants.args_consoleapp"/>
        /// </summary>
        public static bool IsConsoleApp { get; internal set; }

        /// <summary>
        /// 当前是否为控制台工具进程 CLT
        /// </summary>
        public static bool IsConsoleLineToolProcess { get; internal set; }

        /// <summary>
        /// 是否最小化启动
        /// </summary>
        public static bool IsMinimize { get; internal set; }

        /// <summary>
        /// Url 协议启动参数
        /// </summary>
        public static string? StartupUrl { get; internal set; }

        /// <summary>
        /// 是否启用开发者工具
        /// </summary>
        public static bool EnableDevTools { get; internal set; }

        /// <summary>
        /// 当前应用程序是否有 UI
        /// </summary>
        public static bool UserInteractive { get; internal set; } = Environment.UserInteractive;

        /// <summary>
        /// 是否为 Ipc 管理员权限的后端进程
        /// </summary>
        public static bool HasIPCRoot => ProgramArgsConstants.IsBackend(ModuleName);

#if USE_JSON_RPC
        /// <summary>
        /// 用于 JSON-RPC 的管道名称
        /// </summary>
        public static string? PipeNameJsonRpc { get; internal set; }
#endif
    }
}
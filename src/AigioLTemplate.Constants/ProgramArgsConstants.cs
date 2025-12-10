namespace AigioLTemplate.Constants;

/// <summary>
/// 程序参数常量
/// </summary>
static partial class ProgramArgsConstants
{
    /// <summary>
    /// 静默启动程序参数
    /// </summary>
    public const string SystemBootRunArguments = "-clt c -silence";

    /// <summary>
    /// 命令行工具模式参数
    /// </summary>
    public const string clt_ = "-clt";

    /// <summary>
    /// 显示参数帮助信息
    /// </summary>
    public const string help_ = "-h";

    /// <summary>
    /// 关闭程序（参数键）
    /// </summary>
    public const string key_shutdown = "shutdown";

    /// <summary>
    /// 显示主窗口（参数键）
    /// </summary>
    public const string key_show = "show";

    /// <summary>
    /// 主进程启动程序命令参数
    /// </summary>
    public const string command_main = "main";

    /// <summary>
    /// Url 协议启动命令参数
    /// </summary>
    public const string command_url = "url";

    /// <summary>
    /// 后端进程模块名
    /// </summary>
    public const string Backend_moduleName = "Backend";

    /// <summary>
    /// 根据模块名称判断是否为后端进程
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    internal static bool IsBackend(string? moduleName) => moduleName == Backend_moduleName;
}

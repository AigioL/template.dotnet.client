namespace AigioLTemplate.Constants;

static partial class CrashConstants
{
    /// <summary>
    /// 与 Crash/Debug 相关的显示控制台窗口命令
    /// </summary>
    public const string args_consoleapp = "-consoleapp";

    /// <summary>
    /// <see cref="args_consoleapp"/> 的环境变量参数键
    /// </summary>
    public const string args_consoleapp_env = "AigioLTemplate_CONSOLE_APP";

    /// <summary>
    /// 全局异常消息
    /// </summary>
    public const string GlobalExceptionMessage = "Stopped program because of exception";

    /// <summary>
    /// 全局异常消息，args.len = 2
    /// </summary>
    public const string GlobalExceptionMessage__ =
        $"{GlobalExceptionMessage}, name: {{1}}, isTerminating: {{0}}.";

    /// <summary>
    /// 消息框标题
    /// </summary>
    public const string MBTitle =
         $"App Crash - {AssemblyInfo.Title}";

#if HAS_THE_TERM_OF_VALIDITY
    public const string THE_TERM_OF_VALIDITY = "2wCZAQBcenaQTaak0Nzkxw"; // 2025/9/1 0:00:00 +08:00
#endif
}

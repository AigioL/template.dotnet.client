using AigioL.Common.Models;
using AigioLTemplate.Commands.CommandLines;
using AigioLTemplate.Commands.CommandLines.Abstractions;
using AigioLTemplate.Models;
using System.CommandLine;
using System.Diagnostics;
using static AigioLTemplate.Constants.CrashConstants;
using static AigioLTemplate.Constants.ProgramArgsConstants;
using static AigioLTemplate.Constants.UrlConstants;
using static AigioLTemplate.Hosting.I9e4114b2;
using static AigioLTemplate.Hosting.AigioLTemplateHost;

namespace AigioLTemplate.Hosting;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1302 // Interface names should begin with I
internal partial interface AigioLTemplateHost
{
    /// <summary>
    /// 启动 AigioLTemplate 应用程序
    /// <para>使用当前线程作为主线程等待直到退出</para>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static ExitCodeStruct Start(string[]? args = null)
    {
#if DEBUG
        ExitCodeStruct? exitCode = null;
        try
        {
            exitCode = StartCore(args);
            return exitCode.Value;
        }
        finally
        {
            if (exitCode != 0)
            {
                // 0 正常退出时不需要等待键盘输入卡住控制台
                Console.WriteLine($"""

{Environment.ProcessPath} (进程 {Environment.ProcessId})已退出，代码 为 {exitCode} (0x{exitCode ?? 0:x})。
按回车键关闭此窗口. . .
""");
#if !ANDROID
                Console.ReadLine();
#endif
            }
        }
#else
        return _.StartCore(args);
#endif
    }

    /// <summary>
    /// 获取命令行参数，使用 Span 切片进行解析，并简单解析值可选设置 HostConstants
    /// </summary>
    /// <param name="args_span"></param>
    /// <param name="setHostConstants"></param>
    /// <returns></returns>
    internal static string[] GetCommandLineArgsCore(
        ReadOnlySpan<string> args_span,
        bool setHostConstants)
    {
        bool? isMainProcess = default;
        bool? isCLT = default;
        bool? isConsoleApp = default;
        string? startupUrl = default;

        try
        {
#if DEBUG
            // 输出参数显示
            Console.WriteLine($"args: {string.Join(' ', args_span)}");
#endif

            #region 启动参数解析

            // -consoleapp 由 NativeHost 启动控制台窗口
            if (!args_span.IsEmpty &&
                string.Equals(args_span[0], args_consoleapp,
                StringComparison.OrdinalIgnoreCase))
            {
                args_span = args_span[1..];
                isConsoleApp = true;
            }

            // 单参数时的解析
            if (args_span.Length == 1)
            {
                var args_span_0 = args_span[0];

                // URL 协议启动 by 命令行参数
                {
                    if (args_span_0.StartsWith(CUSTOM_URL_SCHEME,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        startupUrl = args_span_0;
                        args_span = new([command_url]);
                    }
                }

                // help 命令输出
                {
                    bool isHelp = false;
                    switch (args_span_0.FirstOrDefault())
                    {
                        case '?':
                        case '？':
                            isHelp = true;
                            break;
                        case '-':
                        case '_':
                        case '/':
                        case '\\':
                            {
                                switch (args_span_0.Length)
                                {
                                    case 2:
                                        switch (args_span_0[1])
                                        {
                                            case 'H':
                                            case 'h':
                                                {
                                                    isHelp = true;
                                                    break;
                                                }
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    if (!isHelp && string.Equals("--help", args_span_0, StringComparison.OrdinalIgnoreCase))
                    {
                        isHelp = true;
                    }

                    if (isHelp)
                    {
                        args_span = new([help_]);
                    }
                }
            }

            #endregion

            isMainProcess = args_span.IsEmpty;
            isCLT = !args_span.IsEmpty &&
                string.Equals(args_span[0], clt_, StringComparison.OrdinalIgnoreCase);

            // ------------------------------
            // 在此之后才能 return，之前仅修改 args/args_span 变量
            // ------------------------------

            // 命令行模式
            if (isCLT.Value)
            {
                if (args_span.Length == 2 &&
                    string.Equals(args_span[1], command_main,
                    StringComparison.OrdinalIgnoreCase))
                {
                    // -clt main 禁止使用此参数
                    return [help_];
                }
                else
                {
                    args_span = args_span[1..];
                    if (args_span.IsEmpty)
                    {
                        // 无参数且不为主进程的清空使用 help 参数
                        return [help_];
                    }
                    return args_span.ToArray();
                }
            }
            else
            {
                // 返回启动主进程的参数
                return [command_main];
            }
        }
        finally
        {
            if (setHostConstants)
            {
                if (isMainProcess.HasValue)
                {
                    V.IsMainProcess = isMainProcess.Value;
                }
                if (isCLT.HasValue)
                {
                    V.IsConsoleLineToolProcess = isCLT.Value;
                }
                if (isConsoleApp.HasValue)
                {
                    V.IsConsoleApp = isConsoleApp.Value;
                }
                if (startupUrl != null)
                {
                    V.StartupUrl = startupUrl;
                }
            }
        }
    }

    /// <summary>
    /// 获取命令行参数
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string[] GetCommandLineArgs(string[]? args)
    {
        string? startupUrl = default;
#if DEBUG
        string[]? args_o = args; // 传递进来的值
#endif
        try
        {
            (args, startupUrl) = OverrideCommandLineArgs(args);
            if (args != null)
            {
                args = GetCommandLineArgsCore2(
                    args,
                    null,
                    true);
            }
            else
            {
                // Environment.GetCommandLineArgs()[0] 为程序启动进程文件路径
                args = GetCommandLineArgsCore2(
                    Environment.GetCommandLineArgs(),
                    1..,
                    true);
            }
            return args;
        }
        finally
        {
            if (startupUrl != null)
            {
                V.StartupUrl = startupUrl;
            }
        }
    }
}

file interface I9e4114b2
{
    internal static ExitCodeStruct StartCore(string[]? args = null)
    {
        Exception? exception = null;
        try
        {
#if DEBUG && WINDOWS
            // Windows 上必须使用 STA 线程，仅调试模式检查
            var apartmentState = Thread.CurrentThread.GetApartmentState();
            if (apartmentState != ApartmentState.STA)
            {
                throw new ApplicationException(
                    "当前线程必须为 STA 单线程单元");
            }
#endif

            // 解析参数
            args = GetCommandLineArgs(args);

            // 初始化日志
            LogInit.InitLog(sourceName: AssemblyInfo.Trademark);

            // 初始化 GetCodeByExceptionDelegate
            ApiRspExtensions.GetCodeByExceptionDelegate = ApiRspCodeExtensions.GetCodeByException;

            var exitCode = StartByRootCommand(args);
            return exitCode;
        }
        catch (Exception ex)
        {
            ExitCodeStruct exitCode = ExitCode.Exception;
            try
            {
                Environment.ExitCode = exitCode;
            }
            catch
            {
            }
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(ex);
#endif
            exception = ex;
            return exitCode;
        }
        finally
        {
            AigioLTemplateHost.DisposeHost();
        }
    }

    /// <summary>
    /// 启动通过命令行模式
    /// <para>使用 <see cref="System.CommandLine"/> 库实现控制台命令行启动，按参数执行业务功能</para>
    /// <para>https://docs.microsoft.com/zh-cn/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline</para>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static int StartByRootCommand(string[] args)
    {
        var root = new RootCommand($"{AssemblyInfo.Product} Command Line Tools");

        // 添加业务命令

#if DEBUG
        root.AddCommand<IDebugCommand>();
        root.AddCommand<ITypesCommand>();
        root.AddCommand<ICLRCommand>();
#endif
        root.AddCommand<IMainCommand>();
        root.AddCommand<IInfoCommand>();
        root.AddCommand<IDevToolsCommand>();

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        var exitCode = root.Parse(args).InvokeAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        return exitCode;
    }

    /// <summary>
    /// 重写命令行参数值，根据平台特性或其他方式启动时修改值匹配不同的 Commands
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static (string[]? args, string? startupUrl) OverrideCommandLineArgs(string[]? args)
    {
        string? startupUrl = null;
#if DEBUG
        string[]? args_o = args; // 传递进来的值
#endif
        //if (IsDesignMode)
        //{
        //    // 当前为设计器模式，不需要解析参数
        //    args ??= [];
        //    goto RE;
        //}

#if WINDOWS
        //{
        //    if (OSHelper.IsRunningAsUwp)
        //    {
        //        IActivatedEventArgs? activatedArgs = null;
        //        try
        //        {
        //            activatedArgs = AppInstance.GetActivatedEventArgs();
        //        }
        //        catch
        //        {
        //            // 有部分用户反馈此处异常
        //            // System.Runtime.InteropServices.COMException (0x80070520): 指定的登录会话不存在。可能已被终止。
        //        }
        //        if (activatedArgs != null)
        //        {
        //            switch (activatedArgs.Kind)
        //            {
        //                // URL 协议启动 by MSIX 包标识
        //                case ActivationKind.Protocol:
        //                    {
        //                        if (activatedArgs is ProtocolActivatedEventArgs protocolActivatedEventArgs)
        //                        {
        //                            var uri = protocolActivatedEventArgs.Uri;
        //                            if (uri != null)
        //                            {
        //                                startupUrl = uri.ToString();
        //                                args = [command_url];
        //                            }
        //                        }
        //                        break;
        //                    }
        //                // MSIX 包形式的开机启动
        //                case ActivationKind.StartupTask:
        //                    {
        //                        args = SystemBootRunArguments.Split(' ');
        //                        break;
        //                    }
        //            }
        //        }
        //    }
        //}
#endif

#pragma warning disable CS0164 // 这个标签尚未被引用
    RE: return (args, startupUrl);
#pragma warning restore CS0164 // 这个标签尚未被引用
    }

    internal static string[] GetCommandLineArgsCore2(
        string[] args,
        Range? range,
        bool setHostConstants)
    {
        var args_span = range.HasValue ? args.AsSpan(range.Value) : args.AsSpan();
        var args_result = GetCommandLineArgsCore(args_span, setHostConstants);
        return args_result;
    }
}
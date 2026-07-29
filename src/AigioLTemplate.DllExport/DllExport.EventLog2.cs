using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AigioLTemplate;

static partial class DllExport
{
    static ILogger Logger => field ??= EventLog2.Provider.CreateLogger(nameof(DllExport));

#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
    /// <summary>
    /// 记录方法开始的日志
    /// </summary>
    [Conditional("DEBUG")]
    static void MethodStartLog([CallerMemberName] string memberName = "")
    {
        Logger.LogInformation("Method {memberName} started.", memberName);
    }

    /// <summary>
    /// 记录方法结束的日志
    /// </summary>
    [Conditional("DEBUG")]
    static void MethodEndLog([CallerMemberName] string memberName = "")
    {
        Logger.LogInformation("Method {memberName} ended.", memberName);
    }

    /// <summary>
    /// 记录方法抛出异常的日志
    /// </summary>
    static void MethodExceptionLog(Exception? ex, [CallerMemberName] string memberName = "")
    {
        Logger.LogError(ex, "Method {memberName} threw an exception.", memberName);
    }
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
}

static partial class EventLog2
{
    static EventLogLoggerProvider CreateDefaultEventLogLoggerProvider()
    {
        EventLogSettings settings = new()
        {
        };
        EventLogLoggerProvider p = new(settings);
        return p;
    }

    internal static EventLogLoggerProvider Provider => field ??= CreateDefaultEventLogLoggerProvider();
}
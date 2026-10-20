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
        try
        {
            Logger.LogInformation("Method {memberName} started.", memberName);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录方法结束的日志
    /// </summary>
    [Conditional("DEBUG")]
    static void MethodEndLog([CallerMemberName] string memberName = "")
    {
        try
        {
            Logger.LogInformation("Method {memberName} ended.", memberName);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录方法抛出异常的日志
    /// </summary>
    static void MethodExceptionLog(Exception? ex, [CallerMemberName] string memberName = "")
    {
        try
        {
            Logger.LogError(ex, "Method {memberName} threw an exception.", memberName);
        }
        catch
        {
        }
    }
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
}

static partial class EventLog2
{
    static ILoggerProvider CreateDefaultEventLogLoggerProvider()
    {
        EventLogSettings settings = new()
        {
        };
        EventLogLoggerProvider p = new(settings);
        return new LoggerProviderWarpper(p);
    }

    internal static ILoggerProvider Provider => field ??= CreateDefaultEventLogLoggerProvider();
}

file sealed class LoggerProviderWarpper(ILoggerProvider p) : ILoggerProvider
{
    public global::Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return new LoggerWarpper(p.CreateLogger(categoryName));
    }

    public void Dispose()
    {
        p.Dispose();
    }
}

file sealed class LoggerWarpper(global::Microsoft.Extensions.Logging.ILogger l) : global::Microsoft.Extensions.Logging.ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return l.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return l.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        try
        {
            l.Log(logLevel, eventId, state, exception, formatter);
        }
        catch
        {
            // System.AggregateException: An error occurred while writing to logger(s). (Cannot open log for source 'xxx'. You may not have write access.)
            // ---> System.InvalidOperationException: Cannot open log for source 'xxx'. You may not have write access.
            // ---> System.ComponentModel.Win32Exception (1722): RPC 服务器不可用。
        }
    }
}
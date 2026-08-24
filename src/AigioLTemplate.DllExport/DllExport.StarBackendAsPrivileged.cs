using AigioLTemplate.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AigioLTemplate;

static unsafe partial class DllExport
{
    static int backendProcessId;
    static string? backendProcessName;

    /// <summary>
    /// 检查后端进程是否存在且未退出
    /// </summary>
    /// <returns></returns>
    static ApiRspCode? CheckBackendProcessHasNotExited()
    {
#if PROJ_LIBRARY
        // 库模式下，直接返回 null，因为没有后端进程
        return null;
#else
        if (backendProcessId == 0 || backendProcessName == null)
        {
            // 后端进程 ID 为空，表示未启动
            return ApiRspCode.ProcessIsNull;
        }
        else
        {
            try
            {
                var backendProcess = Process.GetProcessById(backendProcessId);
                if (backendProcess == null ||
                    backendProcess.ProcessName != backendProcessName ||
                    backendProcess.HasExited)
                {
                    return ApiRspCode.ProcessHasExited;
                }
            }
            catch (ArgumentException)
            {
                return ApiRspCode.ProcessHasExited;
            }
            catch (InvalidOperationException)
            {
                return ApiRspCode.ProcessHasExited;
            }
        }
        return null;
#endif
    }

#if !PROJ_LIBRARY
    static readonly Lock lockBackend = new();
#endif

    /// <summary>
    /// 以管理员权限启动后端进程
    /// </summary>
    /// <param name="argc">进程启动参数字符串长度</param>
    /// <param name="argv">进程启动参数字符数组指针</param>
    /// <param name="processPath_len"></param>
    /// <param name="processPath_ptr"></param>
    /// <param name="processId">启动的进程 Id 返回值的传入指针</param>
    /// <param name="nativeApiRspCode">如果返回值为 <see cref="ApiRspCode.Win32Exception"/> 时的 Win32 错误码返回值的传入指针</param>
    /// <param name="killOrFindBackendProcess"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate5")]
    public static int StartBackendAsPrivileged(
        int argc, char* argv,
        int processPath_len, char* processPath_ptr,
        [Out] int* processId,
        [Out] int* nativeApiRspCode,
        int killOrFindBackendProcess = 0)
    {
        try
        {
            MethodStartLog();
            var r = StartBackendAsPrivilegedCore(
                argc, argv,
                processPath_len, processPath_ptr,
                processId,
                nativeApiRspCode,
                killOrFindBackendProcess);
            return r;
        }
        catch (Exception ex)
        {
            MethodExceptionLog(ex);
            throw;
        }
        finally
        {
            MethodEndLog();
        }
    }


    static int StartBackendAsPrivilegedCore(
        int argc, char* argv,
        int processPath_len, char* processPath_ptr,
        int* processId,
        int* nativeApiRspCode,
        int killOrFindBackendProcess)
    {
#if PROJ_LIBRARY
        // 库模式下，直接返回 OK，因为没有后端进程
        return (int)ApiRspCode.OK;
#else
        lock (lockBackend)
        {
            try
            {
                var args = (argc <= 0 || argv == default) ? default : new ReadOnlySpan<char>(argv, argc);
                string? processPath;
                if (processPath_len > 0 && processPath_ptr != default)
                {
                    // 如果传入了进程路径，则使用传入的路径
                    processPath = new string(processPath_ptr, 0, processPath_len);
                }
                else
                {
                    // 否则使用默认的后端进程路径
                    processPath = GetBackendProcessPath();
                }
                if (string.IsNullOrWhiteSpace(processPath))
                {
                    return (int)ApiRspCode.ProcessPathIsNull;
                }

                Process? backendProcess = null;
                if (killOrFindBackendProcess != 0)
                {
                    KillBackendProcesses();
                }
                else
                {
                    backendProcess = GetBackendProcesses().FirstOrDefault();
                }
                backendProcess ??= StarAsPrivileged(processPath, args);
                if (backendProcess == null)
                {
                    return (int)ApiRspCode.ProcessIsNull;
                }
                try
                {
                    backendProcess.ThrowIfNotWaitForInputIdle();
                }
                catch (InvalidOperationException)
                {
                }

                processId[0] = backendProcessId = backendProcess.Id;
                backendProcessName = backendProcess.ProcessName;
                return (int)ApiRspCode.OK;
            }
            catch (TimeoutException
#if DEBUG
            e
#endif
            )
            {
#if DEBUG
                // 在调试模式下，让 IDE 定位到异常位置
                Debugger.BreakForUserUnhandledException(e);
#endif
                return (int)ApiRspCode.Timeout;
            }
            catch (Win32Exception e)
            {
                nativeApiRspCode[0] = e.NativeErrorCode;
#if DEBUG
                // 在调试模式下，让 IDE 定位到异常位置
                Debugger.BreakForUserUnhandledException(e);
#endif
                return (int)ApiRspCode.Win32Exception;
            }
            catch (ObjectDisposedException
#if DEBUG
            e
#endif
            )
            {
#if DEBUG
                // 在调试模式下，让 IDE 定位到异常位置
                Debugger.BreakForUserUnhandledException(e);
#endif
                return (int)ApiRspCode.ObjectDisposedException;
            }
            catch (InvalidOperationException)
            {
                // 进程不具有图形界面
                return (int)ApiRspCode.InvalidOperationException;
            }
            catch (ArgumentNullException
#if DEBUG
            e
#endif
            )
            {
#if DEBUG
                // 在调试模式下，让 IDE 定位到异常位置
                Debugger.BreakForUserUnhandledException(e);
#endif
                return (int)ApiRspCode.ArgumentNullException;
            }
            catch (Exception
#if DEBUG
            e
#endif
            )
            {
#if DEBUG
                // 在调试模式下，让 IDE 定位到异常位置
                Debugger.BreakForUserUnhandledException(e);
#endif
                return (int)ApiRspCode.Exception;
            }
        }
#endif
    }

    static IEnumerable<Process> GetBackendProcesses()
    {
        var processes = Process.GetProcessesByName("aigioltemplate")
            .Concat(Process.GetProcessesByName("aigioltemplate.webhost"))
            .Where(static x => x.Id != Environment.ProcessId);
        return processes;
    }

    static void KillBackendProcesses()
    {
        var processes = GetBackendProcesses();
        TryKillProcesses(processes);
    }

    static void TryKillProcesses(IEnumerable<Process> processes)
    {
        foreach (var it in processes)
        {
            try
            {
                it.Kill(true);
            }
            catch
            {
            }
        }
    }

    static string GetBackendProcessPath()
    {
#if DEBUG
        var versionProps = File.ReadAllText(Path.Combine(ProjPath, @"src\Version.props"));
        const string versionPrefix = "<WinSDK_Version>";
        var index = versionProps.IndexOf(versionPrefix);
        var s = versionProps.AsSpan(index + versionPrefix.Length);
        var indexR = s.IndexOf("</WinSDK_Version>");
        var winsdkver = s[..indexR].Trim();

        var exePath = Path.Combine(ProjPath, "src", "artifacts", "bin",
            "AigioLTemplate.WebHost", $"debug_net{Environment.Version.Major}.{Environment.Version.Minor}-windows{winsdkver}", "aigioltemplate.webhost.exe");
        return exePath;
#else
        return null!;
#endif
    }

    static Process? StarAsPrivileged(string fileName, ReadOnlySpan<char> args)
    {
        ProcessStartInfo psi = new()
        {
            FileName = fileName,
            UseShellExecute = false,
        };
        if (!Environment.IsPrivilegedProcess)
        {
            // 以管理员权限运行
            psi.Verb = "runas";
            psi.UseShellExecute = true;
        }
        if (args.Length != 0)
        {
            psi.Arguments = new string(args);
        }
        var p = Process.Start(psi);
        return p;
    }

    /// <summary>
    /// 使 <see cref="Process"/> 组件周期性等待关联进程进入空闲状态，仅适用于具有用户界面并因此具有消息循环的进程
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    static void ThrowIfNotWaitForInputIdle(this Process process)
    {
        bool waitForInputIdle = false; // 如果关联进程已经达到空闲状态，则为 true
        for (short i = 0; i < sbyte.MaxValue; i++) // 周期等待，最多等待 127 次 = 127*200 毫秒 = 25.4 秒
        {
            waitForInputIdle = process.WaitForInputIdle();
            Thread.Sleep(200); // 一次周期等待 200 毫秒
            if (waitForInputIdle)
            {
                break;
            }
        }
        if (!waitForInputIdle)
        {
            throw new TimeoutException();
        }
    }
}
using AigioLTemplate.Constants;
using AigioLTemplate.Hosting;
using System.Runtime.InteropServices;

namespace AigioLTemplate;

static partial class DllExport
{
    /// <summary>
    /// 运行应用主机，类似入口点函数
    /// </summary>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate-1")]
    public static RunHostResult RunHost()
    {
        try
        {
            MethodStartLog();
            var r = RunHostCore();
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

    static RunHostResult RunHostCore()
    {
        var exitCode = AigioLTemplateHost.Start();
        Environment.ExitCode = exitCode;
        if (HostConstants.IsMainProcess)
        {
            return RunHostResult.UI;
        }
        return RunHostResult.Exit;
    }

    /// <summary>
    /// 运行应用主机的结果
    /// </summary>
    public enum RunHostResult : byte
    {
        /// <summary>
        /// 退出应用程序
        /// </summary>
        Exit = 1,

        /// <summary>
        /// 启动 UI 窗口
        /// </summary>
        UI = 2,
    }
}

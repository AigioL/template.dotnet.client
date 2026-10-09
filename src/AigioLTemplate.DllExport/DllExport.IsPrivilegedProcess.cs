using System.Runtime.InteropServices;

namespace AigioLTemplate;

static partial class DllExport
{
    /// <summary>
    /// 返回当前进程是否为管理员权限运行
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate7")]
    public static int IsPrivilegedProcess()
    {
        try
        {
            MethodStartLog();
            var r = Environment.IsPrivilegedProcess;
            return r.ToInt32();
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
}

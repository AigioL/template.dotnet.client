using System.Runtime.InteropServices;

namespace AigioLTemplate;

static partial class DllExport
{
    /// <summary>
    /// 返回一个 <see cref="ApartmentState"/> 值，该值指示当前托管线程单元状态
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate6")]
    public static int GetApartmentState()
    {
        try
        {
            MethodStartLog();
            var r = GetApartmentStateCore();
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

    static int GetApartmentStateCore()
    {
        var apartmentState = Thread.CurrentThread.GetApartmentState();
        return (int)apartmentState;
    }
}

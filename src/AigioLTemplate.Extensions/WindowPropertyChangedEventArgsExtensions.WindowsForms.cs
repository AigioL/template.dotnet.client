#if USE_WINDOWSFORMS
using AigioLTemplate.Helpers.UI;
using AigioLTemplate.Models.UI;

namespace AigioLTemplate;

static partial class WindowPropertyChangedEventArgsExtensions
{
    /// <summary>
    /// 根据模型类实例操作窗口
    /// </summary>
    /// <param name="eventArgs"></param>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool SetWindow(this WindowPropertyChangedEventArgs? eventArgs, Form? window)
    {
        if (eventArgs is null || window is null)
        {
            return false;
        }
        else if (eventArgs is WindowStatePropertyChangedEventArgs windowStatePropertyChangedEventArgs)
        {
            return WindowHelper.SetState(window, windowStatePropertyChangedEventArgs.WindowState);
        }
        else if (eventArgs is WindowBoundsPropertyChangedEventArgs windowBoundsPropertyChangedEventArgs)
        {
            return WindowHelper.SetBounds(window,
                windowBoundsPropertyChangedEventArgs.X,
                windowBoundsPropertyChangedEventArgs.Y,
                windowBoundsPropertyChangedEventArgs.Width,
                windowBoundsPropertyChangedEventArgs.Height);
        }
        else if (eventArgs is WindowMethodPropertyChangedEventArgs windowMethodPropertyChangedEventArgs)
        {
            return WindowHelper.SetMethod(window, windowMethodPropertyChangedEventArgs.Name);
        }
        else
        {
            return false;
        }
    }
}
#endif
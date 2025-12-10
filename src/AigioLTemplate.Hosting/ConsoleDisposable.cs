using Windows.Win32;

namespace AigioLTemplate.Hosting;

/// <summary>
/// 控制台窗口显示与隐藏的 <see cref="IDisposable"/> 实现
/// <para>示例：using ConsoleDisposable consoleDisposable = new();</para>
/// </summary>
public sealed class ConsoleDisposable : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleDisposable"/> class.
    /// </summary>
    public ConsoleDisposable()
    {
        ShowConsole();
    }

    void IDisposable.Dispose()
    {
        HideConsole();
    }

    /// <summary>
    /// 显示控制台窗口（仅 Windows）
    /// </summary>
    public static void ShowConsole()
    {
        //if (HostConstants.IsDesignMode)
        //{
        //    return;
        //}

#if WINDOWS
        if (!PInvoke.AttachConsole(unchecked((uint)-1)))
        {
            PInvoke.AllocConsole();
        }
#endif
    }

    /// <summary>
    /// 隐藏控制台窗口（仅 Windows）
    /// </summary>
    public static void HideConsole()
    {
        //if (HostConstants.IsDesignMode)
        //{
        //    return;
        //}

#if WINDOWS
        PInvoke.FreeConsole();
#endif
    }
}

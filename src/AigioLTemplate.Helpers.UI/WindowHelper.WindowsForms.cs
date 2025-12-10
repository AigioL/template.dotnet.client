#if USE_WINDOWSFORMS
using System.Runtime.CompilerServices;

namespace AigioLTemplate.Helpers.UI;

static partial class WindowHelper // WindowsForms
{
    /// <summary>
    /// 将 double 转换为整数像素值
    /// <para>https://learn.microsoft.com/zh-cn/windows/win32/learnwin32/dpi-and-device-independent-pixels</para>
    /// <para>Windows 缩放百分比对应 DPI 值</para>
    /// <list type="bullet">
    /// <item>100% 缩放对应 96 DPI</item>
    /// <item>125% 缩放对应 120 DPI</item>
    /// <item>150% 缩放对应 144 DPI</item>
    /// </list>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int GetDimensionPixelSize(double value, uint dpi = 96) => dpi switch
    {
        96u => (int)Math.Round(value),
        _ => (int)Math.Round(value * dpi / 96D),// 计算 DPI 缩放后的像素值
    };

    /// <summary>
    /// 设置窗口最小化、最大化还是还原窗口
    /// </summary>
    /// <param name="form"></param>
    /// <param name="windowState">值为枚举的名称或数值</param>
    /// <returns></returns>
    public static bool SetState(Form form, string? windowState)
    {
        switch (windowState)
        {
            case null or "":
                return false;
            case "0":
                form.WindowState = FormWindowState.Normal;
                return true;
            case "1":
                form.WindowState = FormWindowState.Minimized;
                return true;
            case "2":
                form.WindowState = FormWindowState.Maximized;
                return true;
            default:
                if (string.Equals("Minimized", windowState, StringComparison.InvariantCultureIgnoreCase))
                {
                    form.WindowState = FormWindowState.Minimized;
                    return true;
                }
                else if (string.Equals("Maximized", windowState, StringComparison.InvariantCultureIgnoreCase))
                {
                    form.WindowState = FormWindowState.Maximized;
                    return true;
                }
                else if (string.Equals("Normal", windowState, StringComparison.InvariantCultureIgnoreCase))
                {
                    form.WindowState = FormWindowState.Normal;
                    return true;
                }
                return false;
        }
    }

    /// <summary>
    /// 设置窗口大小
    /// </summary>
    /// <param name="form"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static bool SetSize(Form form, double? width, double? height)
    {
        if (width is null && height is null)
        {
            return false; // 不设置任何值
        }
        else if (width.HasValue)
        {
            if (height.HasValue)
            {
                var w = GetDimensionPixelSize(width.Value);
                var h = GetDimensionPixelSize(height.Value);
                form.Size = new Size(w, h);
            }
            else
            {
                var w = GetDimensionPixelSize(width.Value);
                form.Width = w;
            }
        }
        else if (height.HasValue)
        {
            var h = GetDimensionPixelSize(height.Value);
            form.Height = h;
        }
        return true;
    }

    /// <summary>
    /// 设置窗口的边界
    /// </summary>
    /// <param name="form"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static bool SetBounds(Form form, double? x, double? y, double? width, double? height)
    {
        if (x is null && y is null && width is null && height is null)
        {
            return false; // 不设置任何值
        }
        var x1 = x.HasValue ? GetDimensionPixelSize(x.Value) : form.Left;
        var y2 = y.HasValue ? GetDimensionPixelSize(y.Value) : form.Top;
        var w = width.HasValue ? GetDimensionPixelSize(width.Value) : form.Width;
        var h = height.HasValue ? GetDimensionPixelSize(height.Value) : form.Height;
        form.SetBounds(x1, y2, w, h, BoundsSpecified.All);
        return true;
    }

    /// <summary>
    /// 调用指定的方法名来操作窗口
    /// </summary>
    /// <param name="form"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static bool SetMethod(Form form, string? methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return false; // 不设置任何值
        }
        else if (string.Equals("Activate", methodName, StringComparison.InvariantCultureIgnoreCase))
        {
            form.Activate();
            return true;
        }
        else if (string.Equals("Close", methodName, StringComparison.InvariantCultureIgnoreCase))
        {
            form.Close();
            return true;
        }
        else if (string.Equals("Hide", methodName, StringComparison.InvariantCultureIgnoreCase))
        {
            form.Hide();
            return true;
        }
        else if (string.Equals("Show", methodName, StringComparison.InvariantCultureIgnoreCase))
        {
            form.Show();
            return true;
        }
        return false;
    }
}
#endif
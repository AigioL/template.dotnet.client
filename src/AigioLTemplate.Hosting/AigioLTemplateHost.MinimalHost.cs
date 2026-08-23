using System.Diagnostics;
using System.Reactive.Disposables;

namespace AigioLTemplate.Hosting;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1302 // Interface names should begin with I
partial interface AigioLTemplateHost
{
    internal static void Run() => MinimalHost.Run();

    internal static void Exit() => MinimalHost.Exit();

    /// <summary>
    /// 释放 Host 资源
    /// </summary>
    internal static void DisposeHost()
    {
        try
        {
            MinimalHost.disposables.Dispose();
        }
#if DEBUG
        catch (Exception ex)
#else
        catch
#endif
        {
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(ex);
#endif
        }

        try
        {
            MinimalHost.compositeAsyncDisposable.DisposeAsync().GetAwaiter().GetResult();
        }
#if DEBUG
        catch (Exception ex)
#else
        catch
#endif
        {
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(ex);
#endif
        }
    }

    internal static void AddDisposable(IDisposable disposable) => MinimalHost.disposables.Add(disposable);

    /// <summary>
    /// 添加一个异步可释放对象到 Host 中，在 Host 退出时会释放
    /// </summary>
    /// <param name="disposable"></param>
    /// <returns></returns>
    internal static ValueTask AddDisposableAsync(IAsyncDisposable disposable) => MinimalHost.compositeAsyncDisposable.AddAsync(disposable);

    internal static Task<SynchronizationContext> GetSynchronizationContextAsync() => MinimalHost.tcsSyncCtx.Task;
}

/// <summary>
/// 最小的轻量化 Host 实现
/// </summary>
file static class MinimalHost
{
#if USE_WINDOWSFORMS
#else
    static TaskCompletionSource? tcs;
    static readonly Lock @lock = new();
#endif
    internal static readonly CompositeDisposable disposables = new();
    internal static readonly CompositeAsyncDisposable compositeAsyncDisposable = new();
    internal static readonly TaskCompletionSource<SynchronizationContext> tcsSyncCtx = new();

    internal static void Run()
    {
#if USE_WINDOWSFORMS
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        global::System.Windows.Forms.Application.EnableVisualStyles();
        global::System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        global::System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
        var mainForm = Form1.Instance;
        // 先创建窗口才能获取 SynchronizationContext
        var windowsFormsSynchronizationContext = SynchronizationContext.Current;
        ArgumentNullException.ThrowIfNull(windowsFormsSynchronizationContext);
        tcsSyncCtx.SetResult(windowsFormsSynchronizationContext);
        global::System.Windows.Forms.Application.Run(mainForm);
#elif PROJ_LIBRARY
        // 在库项目中，直接返回，不阻塞主线程，由调用库的应用程序自行创建 UI 窗口或消息循环
#else
        lock (@lock)
        {
            tcs ??= new();
            Console.CancelKeyPress += Console_CancelKeyPress;
        }
        // 使用 TaskCompletionSource 实现阻塞主线程，等待退出信号
        // TODO: 在 Windows 上实现 Win32 消息循环方案，一些本机 API 需要在消息循环中运行获取回调函数
        tcsSyncCtx.SetResult(SynchronizationContext.Current!);
        tcs.Task.GetAwaiter().GetResult();
#endif
    }

    internal static void Exit()
    {
#if USE_WINDOWSFORMS
        global::System.Windows.Forms.Application.Exit();
#elif PROJ_LIBRARY
        // 在库项目中不需要执行任何操作
#else
        lock (@lock)
        {
            if (tcs != null)
            {
                // 触发 TaskCompletionSource 的完成状态，解除主线程的阻塞
                tcs.TrySetResult();
                Console.CancelKeyPress -= Console_CancelKeyPress;
                tcs = null;
            }
        }
#endif
    }

    static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (!e.Cancel)
        {
            // 通过键盘 Ctrl+C 触发退出
            Exit();
        }
    }
}

#if USE_WINDOWSFORMS
/// <summary>
/// 利用 WinForms 的 Form 作为后端的主窗口实现 Win32 消息循环
/// </summary>
file sealed class Form1 : Form
{
    /// <summary>
    /// WinForms 模板项目的 Program.Main 函数
    /// </summary>
    internal static void M()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        global::System.Windows.Forms.Application.EnableVisualStyles();
        global::System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        global::System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
        global::System.Windows.Forms.Application.Run(instance);
    }

    ///// <summary>
    ///// 退出当前 WinForms
    ///// </summary>
    //internal static void Exit()
    //{
    //    instance.Close();
    //}

    internal static Form Instance => instance;

    static readonly Form1 instance = new();

    Form1()
    {
        SuspendLayout();
        try
        {
            // 窗口标题文本会显示在任务管理器上
            Text = Console.Title;
        }
        catch
        {
        }
        AutoScaleMode = AutoScaleMode.Font;
        CausesValidation = false;
        ClientSize = new(1, 1);
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Opacity = 0D;
        ShowIcon = false;
        ShowInTaskbar = false;
        TransparencyKey = SystemColors.Control;
        WindowState = FormWindowState.Minimized;
        ResumeLayout(false);
        Hide();
    }
}
#endif
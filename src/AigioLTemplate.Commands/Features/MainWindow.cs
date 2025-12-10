using AigioL.Common.Models;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;
using AigioLTemplate.Models.UI;

namespace AigioLTemplate.Commands.Features;

using TArgs = WindowPropertyChangedEventArgs;
using TCommand = MainWindow;
using TResult = bool;

/// <summary>
/// 操作主窗口
/// <para>示例：</para>
/// <list type="bullet">
/// <item>POST: https://localhost:9443/c/MainWindow</item>
/// <item>Reuqest Body: {"$propertyName":"WindowMethod","name": "Hide"}</item>
/// <item>Response Body: {"Code":200,"Url": "MainWindow"}</item>
/// </list>
/// </summary>
sealed partial class MainWindow
{
#if USE_WINDOWSFORMS
    static Form? mainWindow; // TODO: WindowManager
#endif

    internal static unsafe bool Invoke(TArgs args)
    {
#if USE_WINDOWSFORMS
        var result = args.SetWindow(mainWindow);
        return result;
#else
        return default;
#endif
    }
}

partial class MainWindow :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult>> InvokeAsync(TArgs? args, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        var r = Invoke(args);
        ApiRsp<TResult> result = new()
        {
            Content = r,
        };
        result.SetIsSuccess(true);
        return new(result);
    }

    static ValueTask InvokeAsync(SerializableImplType implType, Stream? request, Stream response, CancellationToken cancellationToken = default)
    {
        var t = IV2FeatureCommand<TArgs, TResult>.InvokeCoreAsync<TCommand>(
            implType, request, response, cancellationToken: cancellationToken);
        return t;
    }

    static unsafe nint IV2FeatureCommandFunc.GetFuncPtr()
    {
        delegate* managed<SerializableImplType, Stream?, Stream, CancellationToken, ValueTask> funcptr = &InvokeAsync;
        return (nint)funcptr;
    }
}

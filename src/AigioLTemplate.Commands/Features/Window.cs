//using AigioL.Common.Models;
//using AigioLTemplate.Commands.Features.Abstractions;
//using AigioLTemplate.Models;
//using AigioLTemplate.Models.UI;

//namespace AigioLTemplate.Commands.Features;

//using TArgs = WindowCommandArgs;
//using TCommand = Window;
//using TResult = bool;

///// <summary>
///// 根据窗口句柄操作窗口
///// </summary>
//sealed partial class Window
//{
//#if USE_WINDOWSFORMS
//    /// <summary>
//    /// 根据窗口句柄获取窗口实例
//    /// </summary>
//    /// <param name="hwnd"></param>
//    /// <returns></returns>
//    static Form? GetWindow(nint hwnd)
//    {
//        // TODO: WindowManager
//        return null;
//    }
//#endif

//    internal static unsafe bool Invoke(TArgs args)
//    {
//#if USE_WINDOWSFORMS
//        var window = GetWindow((nint)args.Hwnd);
//        var result = args.EventArgs.SetWindow(window);
//        return result;
//#else
//        return default;
//#endif
//    }
//}

//partial class Window :
//    IV2FeatureCommand<TArgs, TResult>,
//    IV2FeatureCommandFunc
//{
//    public static ValueTask<ApiRsp<TResult>> InvokeAsync(TArgs? args, CancellationToken cancellationToken)
//    {
//        ArgumentNullException.ThrowIfNull(args);
//        var r = Invoke(args);
//        ApiRsp<TResult> result = new()
//        {
//            Content = r,
//        };
//        result.SetIsSuccess(true);
//        return new(result);
//    }

//    static ValueTask InvokeAsync(SerializableImplType implType, Stream? request, Stream response, CancellationToken cancellationToken = default)
//    {
//        var t = IV2FeatureCommand<TArgs, TResult>.InvokeCoreAsync<TCommand>(
//            implType, request, response, cancellationToken: cancellationToken);
//        return t;
//    }

//    static unsafe nint IV2FeatureCommandFunc.GetFuncPtr()
//    {
//        delegate* managed<SerializableImplType, Stream?, Stream, CancellationToken, ValueTask> funcptr = &InvokeAsync;
//        return (nint)funcptr;
//    }
//}

//sealed partial record WindowCommandArgs(long Hwnd, WindowPropertyChangedEventArgs EventArgs);

//partial record WindowCommandArgs : global::System.Text.Json.Serialization.IJsonSerializerContext
//{
//    /// <inheritdoc/>
//    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
//}
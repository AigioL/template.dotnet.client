#if DEBUG
using AigioL.Common.Models;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;

namespace AigioLTemplate.Commands.Features.Samples;

using TArgs = nil;
using TCommand = 无请求有响应示例;
using TResult = string;

sealed partial class 无请求有响应示例
{
    internal static string Invoke()
    {
        // 没有传入的参数
        const string result = "无请求有响应示例";
        Console.WriteLine(result);
        return result;
    }
}

partial class 无请求有响应示例 :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult?>> InvokeAsync(TArgs args, CancellationToken cancellationToken)
    {
        var r = Invoke();
        ApiRsp<TResult?> result = new()
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
#endif
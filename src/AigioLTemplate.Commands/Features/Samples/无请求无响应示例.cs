#if DEBUG
using AigioL.Common.Models;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;

namespace AigioLTemplate.Commands.Features.Samples;

using TArgs = nil;
using TCommand = 无请求无响应示例;
using TResult = nil;

sealed partial class 无请求无响应示例
{
    internal static void Invoke()
    {
        // 没有传入的参数
        Console.WriteLine("无请求无响应示例");
        // 也没有返回的参数
    }
}

partial class 无请求无响应示例 :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult>> InvokeAsync(TArgs args, CancellationToken cancellationToken)
    {
        Invoke();
        ApiRsp<TArgs> result = true;
        return new(result);
        // 返回 bool 值应用在 ApiRsp.IsSuccess 中
        // 固定会返回 ApiRsp 数据
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
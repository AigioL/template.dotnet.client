#if DEBUG
using AigioL.Common.Models;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;

namespace AigioLTemplate.Commands.Features.Samples;

using TArgs = 有请求无响应示例请求;
using TCommand = 有请求无响应示例;
using TResult = nil;

sealed partial class 有请求无响应示例
{
    internal static void Invoke(TArgs args)
    {
        Console.WriteLine(
            $"有请求无响应示例: Id={args.Id}, Name={args.Name}, Count={args.Count}, Money={args.Money}");
    }
}

partial class 有请求无响应示例 :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult>> InvokeAsync(TArgs? args, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        Invoke(args);
        ApiRsp<TResult> result = true;
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

sealed partial record 有请求无响应示例请求(long Id, string Name, int Count, decimal Money);

partial record 有请求无响应示例请求 : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
#endif
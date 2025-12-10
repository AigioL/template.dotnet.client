#if DEBUG
using AigioL.Common.Models;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;

namespace AigioLTemplate.Commands.Features.Samples;

using TArgs = 有请求有响应示例请求;
using TCommand = 有请求有响应示例;
using TResult = string;

sealed partial class 有请求有响应示例
{
    internal static string Invoke(TArgs args)
    {
        var result = $"有请求有响应示例: Id={args.Id}, Name={args.Name}, Count={args.Count}, Money={args.Money}";
        Console.WriteLine(result);
        return result;
    }
}

partial class 有请求有响应示例 :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult?>> InvokeAsync(TArgs? args, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        var r = Invoke(args);
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

sealed partial record 有请求有响应示例请求(Guid Id, string Name, float Count, double Money);

partial record 有请求有响应示例请求 : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
#endif
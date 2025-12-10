using AigioL.Common.Models;
using AigioLTemplate.Models;
using MemoryPack;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AigioLTemplate.Commands.Features.Abstractions;

/// <summary>
/// 功能业务命令接口（V2）
/// </summary>
/// <typeparam name="TCommandArgs"></typeparam>
/// <typeparam name="TResultContent"></typeparam>
interface IV2FeatureCommand<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResultContent>
    : IFeatureCommand
{
    /// <summary>
    /// 由业务类实现的命令处理方法
    /// </summary>
    internal static abstract ValueTask<ApiRsp<TResultContent?>> InvokeAsync(TCommandArgs? args, CancellationToken cancellationToken = default);

    protected static async ValueTask InvokeCoreAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFeatureCommand>(
        SerializableImplType implType,
        Stream? request,
        Stream response,
        JsonSerializerContext? requestJsonSerializerContext = null,
        JsonSerializerContext? responseJsonSerializerContext = null,
        CancellationToken cancellationToken = default)
        where TFeatureCommand : IV2FeatureCommand<TCommandArgs, TResultContent>
    {
        ApiRsp<TResultContent?>? result = null;
        try
        {
            TCommandArgs? args;
            if (request == null || nil.IsNilType(typeof(TCommandArgs)))
            {
                args = default;
            }
            else
            {
                // 反序列化请求流
                switch (implType)
                {
                    case SerializableImplType.SystemTextJson:
                        {
                            requestJsonSerializerContext ??= DefaultJsonSerializerContext_.Default;
                            var requestJsonTypeInfo = (JsonTypeInfo<TCommandArgs>?)requestJsonSerializerContext.GetTypeInfo(typeof(TCommandArgs));
                            ArgumentNullException.ThrowIfNull(requestJsonTypeInfo);
                            args = await JsonSerializer.DeserializeAsync(request, requestJsonTypeInfo, cancellationToken);
                        }
                        break;
                    case SerializableImplType.MemoryPack:
                        {
                            args = await MemoryPackSerializer.DeserializeAsync<TCommandArgs>(request, cancellationToken: cancellationToken);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
                }
            }
            // 调用业务命令
            result = await TFeatureCommand.InvokeAsync(args, cancellationToken);
        }
        catch (Exception ex)
        {
#if DEBUG
            // 在调试模式下，让 IDE 定位到异常位置
            Debugger.BreakForUserUnhandledException(ex);
#endif
            result ??= new();
            result.SetException(ex);
        }
        finally
        {
            result ??= new();
            if (string.IsNullOrWhiteSpace(result.Url))
            {
                var url = typeof(TFeatureCommand).Name;
                result.Url = url;
            }
        }
        // 序列化响应结果
        switch (implType)
        {
            case SerializableImplType.SystemTextJson:
                {
                    responseJsonSerializerContext ??= DefaultJsonSerializerContext_.Default;
                    var responseJsonTypeInfo = responseJsonSerializerContext.GetTypeInfo(typeof(ApiRsp<TResultContent>));
                    ArgumentNullException.ThrowIfNull(responseJsonTypeInfo);
                    await JsonSerializer.SerializeAsync(response, result, responseJsonTypeInfo, cancellationToken);
                }
                break;
            case SerializableImplType.MemoryPack:
                {
                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
        }
    }
}

using AigioL.Common.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;
using AigioLTemplate.ServerSdk.Services.Abstractions;
using System.Text.Json;
using ApiRspCode = AigioLTemplate.Models.ApiRspCode;

namespace AigioLTemplate.Commands.Features.WebApi;

using TArgs = AigioLTemplateFetchRequestInit;
using TCommand = AigioLTemplateFetch;
using TResult = JsonElement?;

sealed partial class AigioLTemplateFetch
{
    internal static async Task<ApiRsp<JsonElement?>> Invoke(TArgs args, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(args.Url))
        {
            return ApiRspCode.BadRequest;
        }

        var webApiService = Ioc.Default.GetRequiredService<IAigioLTemplateWebApiService>();

        var method = HttpMethod.Parse(args.Method);
        var baseAddress = new Uri(args.BaseAddress, UriKind.Absolute);
        var requestUri = new Uri(args.Url, UriKind.Relative);
        requestUri = new(baseAddress, requestUri);
        var r = await webApiService.SendAsync<JsonElement?, JsonElement?>(
            baseAddress: baseAddress,
            requestFactory: () =>
            {
                var request = new HttpRequestMessage(method, requestUri);
                return request;
            },
            requestModel: args.Body,
            isAnonymous: args.IsAnonymous,
            isSecurity: args.IsSecurity,
            cancellationToken: cancellationToken);
        if (r.IsSuccess() && r.Content.HasValue)
        {
            var urlSpan = args.Url.AsSpan().Trim();
            if (urlSpan.StartsWith('/'))
            {
                urlSpan = urlSpan[1..];
            }
            // 根据请求 URL 进行特殊业务处理
            if (urlSpan.StartsWith("identity/v5/account/loginorregister", StringComparison.InvariantCultureIgnoreCase))
            {
                //var request = args.Body.Value.Deserialize(DefaultJsonSerializerContext_.Default.LoginOrRegisterRequest);
                var response = r.Content.Value.Deserialize(DefaultJsonSerializerContext_.Default.ApiRspLoginOrRegisterResponse);
                if (response != null && response.IsSuccess() && response.Content?.User != null)
                {
                    await webApiService.OnLoginedAsync(
                        response.Content.User.PhoneNumber,
                        response.Content.User.PhoneNumberRegionCode,
                        response.Content.User.Email,
                        response.Content.User.Id,
                        response.Content.User,
                        response.Content.AuthToken);
                }
            }
        }
        return r;
    }
}

partial class AigioLTemplateFetch :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<JsonElement?>> InvokeAsync(TArgs? args, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        var result = Invoke(args, cancellationToken);
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

sealed partial record AigioLTemplateFetchRequestInit(string BaseAddress, string Url, string Method, JsonElement? Body, bool IsSecurity, bool IsAnonymous)
{
}

partial record AigioLTemplateFetchRequestInit : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
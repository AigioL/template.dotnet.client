using AigioL.Common.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using AigioLTemplate.Commands.CommandLines;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Constants;
using AigioLTemplate.Models;
using AigioLTemplate.ServerSdk.Services.Abstractions;

namespace AigioLTemplate.Commands.Features;

using TArgs = HttpClientBaseAddressModel;
using TCommand = HttpClientBaseAddress;
using TResult = HttpClientBaseAddressModel;

/// <summary>
/// 设置或获取 <see cref="HttpClient.BaseAddress"/>
/// <para>请求模型传入 <see langword="null"/> 为获取，模型字符串字段值为 <see langword="null"/> 或空字符串时忽略设置操作，值必须为 Production、Development、Localhost、Ipv6Only 其中之一，且字母大小写完全匹配</para>
/// <para>也可启动进程时设置，通过命令行参数 <see cref="IMainCommand.GetOptBaseUrlApi"/> 等</para>
/// </summary>
sealed partial class HttpClientBaseAddress
{
    internal static TResult Invoke(TArgs? args)
    {
        if (args != null)
        {
            UrlConstants_.ApiBaseUrl = args.ApiBaseUrl;
            UrlConstants_.OfficialWebsite = args.OfficialWebsite;
        }

        var webApiService = Ioc.Default.GetRequiredService<IServerSdkWebApiService>();
        var referrer = webApiService.Referrer;
        TResult r = new(referrer, UrlConstants.ApiBaseUrl, UrlConstants.OfficialWebsite);
        return r;
    }
}

partial class HttpClientBaseAddress :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult?>> InvokeAsync(TArgs? args, CancellationToken cancellationToken)
    {
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

sealed partial record HttpClientBaseAddressModel(string Referrer, string ApiBaseUrl, string OfficialWebsite);

partial record HttpClientBaseAddressModel : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
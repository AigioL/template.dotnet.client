using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;
using AigioLTemplate.ServerSdk.Models.Identity;
using AigioLTemplate.ServerSdk.Services.Abstractions;

namespace AigioLTemplate.Commands.Features.WebApi;

using TArgs = nil;
using TCommand = GetDeviceId;
using TResult = DeviceIdModel;

sealed partial class GetDeviceId
{
    internal static async Task<TResult> InvokeAsync()
    {
        var userStore = Ioc.Default.GetRequiredService<IUserStore<UserInfoModel>>();
        var currentUser = await userStore.GetCurrentUserAsync(false);
        var userInfo = await userStore.GetCurrentUserInfoAsync();
        var webApiService = Ioc.Default.GetRequiredService<IServerSdkWebApiService>();
        var authenticationHeader = webApiService.GetAuthenticationHeaderValue(currentUser?.AuthToken);
        TResult m = new()
        {
            CurrentUser = currentUser,
            UserInfo = userInfo,
            Referrer = webApiService.Referrer,
            Authentication = authenticationHeader?.ToString(),
        };
        m.SetDeviceId();
        return m;
    }
}

partial class GetDeviceId :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static async ValueTask<ApiRsp<TResult?>> InvokeAsync(TArgs args, CancellationToken cancellationToken)
    {
        var r = await InvokeAsync();
        ApiRsp<TResult?> result = new()
        {
            Content = r,
        };
        result.SetIsSuccess(true);
        return result;
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

public sealed partial record DeviceIdModel : IDeviceId
{
    public CurrentUser? CurrentUser { get; set; }

    public UserInfoModel? UserInfo { get; set; }

    public required string Referrer { get; set; }

    public string? Authentication { get; set; }

    /// <inheritdoc/>
    public Guid DeviceIdG { get; set; }

    /// <inheritdoc/>
    public string? DeviceIdR { get; set; }

    /// <inheritdoc/>
    public string? DeviceIdN { get; set; }
}

partial record DeviceIdModel : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
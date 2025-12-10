using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;

namespace AigioLTemplate.ServerSdk.Services.Abstractions;

/// <summary>
/// WebApi 服务（客户端侧调用 SDK Client）
/// </summary>
public partial interface IAigioLTemplateWebApiService
{
    /// <summary>
    /// 获取用于 HTTP 请求头的 Referrer 值
    /// </summary>
    string Referrer { get; }

    AuthenticationHeaderValue? GetAuthenticationHeaderValue(JsonWebTokenValue? jwt);

    /// <summary>
    /// 保存用户登录凭证
    /// </summary>
    Task SaveAuthTokenAsync(JsonWebTokenValue authToken);

    /// <summary>
    /// 当登录完成时
    /// </summary>
    Task OnLoginedAsync(
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? email,
        Guid? userId,
        UserInfoModel? userInfo,
        JsonWebTokenValue? authToken);

    Task<ApiRsp<TResponseModel?>> SendAsync<TRequestModel, TResponseModel>(
        Uri baseAddress,
        Func<HttpRequestMessage> requestFactory,
        TRequestModel? requestModel,
        bool isSecurity = false,
        bool isAnonymous = false,
        SerializableImplType serializableImplType = SerializableImplType.SystemTextJson,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        JsonTypeInfo<ApiRsp<TResponseModel?>>? jsonResponseModelTypeInfo = null,
        CancellationToken cancellationToken = default);
}
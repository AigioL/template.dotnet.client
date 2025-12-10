using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Essentials.Devices;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioLTemplate;
using AigioLTemplate.Constants;
using AigioLTemplate.ServerSdk.Models.Abstractions;
using AigioLTemplate.ServerSdk.Models.Identity;
using AigioLTemplate.ServerSdk.Services;
using AigioLTemplate.ServerSdk.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 <see cref="IAigioLTemplateWebApiService"/> 服务
    /// </summary>
    public static IServiceCollection AddAigioLTemplateWebApiService<
        TAppSecrets,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserInfoModel>(this IServiceCollection services)
        where TAppSecrets : class, IAppSecrets
        where TUserInfoModel : IReadOnlyId<Guid>
    {
        services.TryAddSingleton<IUserStore<TUserInfoModel>, UserStore<TUserInfoModel>>();
        services.AddSingleton<IAigioLTemplateWebApiService, S3dfab2fb<TAppSecrets>>();
        return services;
    }
}


file sealed partial class S3dfab2fb<TAppSecrets> : IAigioLTemplateWebApiService
    where TAppSecrets : class, IAppSecrets
{
    /// <summary>
    /// 默认超时时间，25 秒
    /// </summary>
    const int DefaultTimeoutMilliseconds = 25000;

    readonly RecyclableMemoryStreamManager m = new();
    readonly IUserStore<UserInfoModel> userStore;
    readonly HttpClient client;
    readonly ILogger logger;

    public S3dfab2fb(
        ILoggerFactory loggerFactory,
        IOptions<TAppSecrets> options,
        IUserStore<UserInfoModel> userStore,
        IVersionTracking versionTracking,
        IDeviceInfo deviceInfo)
    {
        logger = loggerFactory.CreateLogger("AigioLTemplateWebApiService");
        this.userStore = userStore;
        var referrer = $"{UrlConstants.CUSTOM_URL_SCHEME}{deviceInfo.Platform}/{versionTracking.CurrentVersion}";
        Referrer = new(referrer, UriKind.Absolute);
        RSAInstance = options.Value.PublicKey;
        SocketsHttpHandler handler = new()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };
        client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(DefaultTimeoutMilliseconds),
        };
    }

    string IAigioLTemplateWebApiService.Referrer => Referrer.ToString();

    RSA RSAInstance { get; }

    Uri Referrer { get; }

    /// <inheritdoc/>
    public async Task SaveAuthTokenAsync(JsonWebTokenValue authToken)
    {
        var user = await userStore.GetCurrentUserAsync(false);
        if (user != null)
        {
            user.AuthToken = authToken;
            await userStore.SetCurrentUserAsync(user);
        }
    }

    /// <inheritdoc/>
    public async Task OnLoginedAsync(
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? email,
        Guid? userId,
        UserInfoModel? userInfo,
        JsonWebTokenValue? authToken)
    {
        userId ??= userInfo?.Id;
        if (userInfo != null)
        {
            await userStore.SetCurrentUserInfoAsync(userInfo, true);
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(email) && userId.HasValue)
        {
            CurrentUser cUser = new()
            {
                UserId = userId.Value,
                AuthToken = authToken,
                PhoneNumber = phoneNumber,
                PhoneNumberRegionCode = phoneNumberRegionCode,
                Email = email,
            };
            await userStore.SetCurrentUserAsync(cUser);
        }
    }

    /// <summary>
    /// 获取请求正文
    /// </summary>
    async Task<HttpContent?> GetRequestContentAsync<TRequestModel>(
        bool isSecurity,
        Aes? aes,
        SerializableImplType serializableImplType,
        TRequestModel? requestModel,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        CancellationToken cancellationToken = default)
    {
        if (requestModel != null)
        {
            if (requestModel is IDeviceId deviceId)
            {
                deviceId.SetDeviceId();
            }

            if (isSecurity)
            {
                ArgumentNullException.ThrowIfNull(aes);
                switch (serializableImplType)
                {
                    case SerializableImplType.SystemTextJson:
                        {
                            using var serializeStream = m.GetStream(); // 创建内存流用于 Json 序列化
                            if (requestModel is JsonElement jsonElement)
                            {
                                Utf8JsonWriter writer = new((IBufferWriter<byte>)serializeStream, SerializerConstants.DefaultJsonWriterOptions);
                                jsonElement.WriteTo(writer);
                                await writer.FlushAsync(cancellationToken);
                            }
                            else
                            {
                                ArgumentNullException.ThrowIfNull(jsonRequestTypeInfo);
                                await JsonSerializer.SerializeAsync(serializeStream, requestModel, jsonRequestTypeInfo, cancellationToken);
                            }
                            serializeStream.Position = 0;

                            var cipherStream = m.GetStream(); // 创建内存流用于存储加密后的密文数据
                            using CryptoStream cryptoStream = new(cipherStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
                            await serializeStream.CopyToAsync(cryptoStream, cancellationToken);
                            await cryptoStream.FlushFinalBlockAsync(cancellationToken);
                            cipherStream.Position = 0;
                            var r = new StreamContent(cipherStream);
                            r.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.JSONSecurity);
                            return r;
                        }
                    case SerializableImplType.MemoryPack:
                        {
                            throw new NotImplementedException("尚未实现");
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializableImplType), serializableImplType, null);
                }
            }
            else
            {
                switch (serializableImplType)
                {
                    case SerializableImplType.SystemTextJson:
                        {
                            var serializeStream = m.GetStream();
                            if (requestModel is JsonElement jsonElement)
                            {
                                Utf8JsonWriter writer = new((IBufferWriter<byte>)serializeStream, SerializerConstants.DefaultJsonWriterOptions);
                                jsonElement.WriteTo(writer);
                                await writer.FlushAsync(cancellationToken);
                            }
                            else
                            {
                                ArgumentNullException.ThrowIfNull(jsonRequestTypeInfo);
                                await JsonSerializer.SerializeAsync(serializeStream, requestModel, jsonRequestTypeInfo, cancellationToken);
                            }
                            serializeStream.Position = 0;
                            var r = new StreamContent(serializeStream);
                            r.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.JSON);
                            return r;
                        }
                    case SerializableImplType.MemoryPack:
                        {
                            throw new NotImplementedException("尚未实现");
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializableImplType), serializableImplType, null);
                }
            }
        }
        return null;
    }

    const string Basic = "Bearer";

    public AuthenticationHeaderValue? GetAuthenticationHeaderValue(JsonWebTokenValue? jwt)
    {
        if (jwt.HasValue())
        {
            var authHeaderValue = new AuthenticationHeaderValue(Basic, jwt.AccessToken);
            return authHeaderValue;
        }
        return null;
    }

    /// <summary>
    /// 设置请求中的授权头
    /// </summary>
    async ValueTask<JsonWebTokenValue?> SetRequestHeaderAuthorization(HttpRequestMessage request)
    {
        var currentUser = await userStore.GetCurrentUserAsync(false);
        var authToken = currentUser?.AuthToken;
        var authHeaderValue = GetAuthenticationHeaderValue(authToken);
        if (authHeaderValue != null)
        {
            request.Headers.Authorization = authHeaderValue;
            return authToken;
        }
        return null;
    }

    void HandleHttpRequest(HttpRequestMessage request)
    {
        request.Version = HttpVersion.Version20;
        //request.VersionPolicy = HttpVersionPolicy.RequestVersionExact; // 强制使用 HTTP/2
        request.Headers.AcceptLanguage.ParseAdd(CultureInfo.CurrentUICulture.Name);
        request.Headers.Referrer = Referrer;
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAppObsolete(HttpResponseHeaders headers)
        => headers.TryGetValues(ApiConstants.Headers_AppObsolete, out var values) &&
            values.Contains(bool.TrueString, StringComparer.OrdinalIgnoreCase);

    async Task<JsonDocument?> ReadAsJsonDocumentAsync(
        HttpContent content,
        bool isSecurity,
        Aes? aes,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength.HasValue && content.Headers.ContentLength.Value == 0)
        {
            return null;
        }

        bool isJSONSecurity = false;
        if (isSecurity)
        {
            var contentType = content.Headers.ContentType;
            if (contentType != null)
            {
                if (string.Equals(contentType.MediaType, MediaTypeNames.JSONSecurity))
                {
                    isJSONSecurity = true;
                }
            }
        }

        if (isSecurity && isJSONSecurity)
        {
            ArgumentNullException.ThrowIfNull(aes);
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            using CryptoStream cryptoStream = new(stream, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);

            using var memoryStream = m.GetStream();
            await cryptoStream.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Position = 0;
            var jsonDoc = await JsonDocument.ParseAsync(memoryStream, SerializerConstants.DefaultJsonDocumentOptions, cancellationToken);
            return jsonDoc;
        }
        else
        {
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            var jsonDoc = await JsonDocument.ParseAsync(stream, SerializerConstants.DefaultJsonDocumentOptions, cancellationToken);
            return jsonDoc;
        }
    }

    async Task<ApiRsp<TResponseModel?>> ReadAsResponseModelAsync<TResponseModel>(
        HttpResponseMessage response,
        bool isSecurity,
        Aes? aes,
        JsonTypeInfo<ApiRsp<TResponseModel?>> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        ApiRsp<TResponseModel?> GetStatusCodeApiRsp()
        {
            ApiRsp<TResponseModel?> r = new()
            {
                Code = unchecked((uint)response.StatusCode),
            };
            return r;
        }

        var content = response.Content;
        if (content.Headers.ContentLength.HasValue && content.Headers.ContentLength.Value == 0)
        {
            return GetStatusCodeApiRsp();
        }

        bool isJSONSecurity = false;
        if (isSecurity)
        {
            var contentType = content.Headers.ContentType;
            if (contentType != null)
            {
                if (string.Equals(contentType.MediaType, MediaTypeNames.JSONSecurity))
                {
                    isJSONSecurity = true;
                }
            }
        }

        if (isSecurity && isJSONSecurity)
        {
            ArgumentNullException.ThrowIfNull(aes);
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            using CryptoStream cryptoStream = new(stream, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);

            using var memoryStream = m.GetStream();
            await cryptoStream.CopyToAsync(memoryStream, cancellationToken);
            await cryptoStream.FlushFinalBlockAsync(cancellationToken);

            memoryStream.Position = 0;
            var r = await JsonSerializer.DeserializeAsync(memoryStream, jsonTypeInfo, cancellationToken);
            return r ?? GetStatusCodeApiRsp();
        }
        else
        {
            var stream = await content.ReadAsStreamAsync(cancellationToken);
            var r = await JsonSerializer.DeserializeAsync(stream, jsonTypeInfo, cancellationToken);
            return r ?? GetStatusCodeApiRsp();
        }
    }

    Task<bool>? taskRefreshTokenWithSaveAsync;

    async Task<bool> RefreshTokenWithSaveCoreAsync(Uri baseAddress, JsonWebTokenValue jwt)
    {
        var requestUri = new Uri("identity/v5/account/refreshtoken", UriKind.Relative);
        requestUri = new(baseAddress, requestUri);
        RefreshTokenRequest requestModel = new()
        {
            RefreshToken = jwt.RefreshToken,
        };
        var rsp = await SendAsync<RefreshTokenRequest, JsonWebTokenValue>(
            baseAddress,
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                return request;
            },
            requestModel,
            isAnonymous: true, // 刷新 Token 必须匿名身份，否则将在客户端上递归导致死循环
            isSecurity: true);
        if (rsp.IsSuccess() && rsp.Content != null)
        {
            await SaveAuthTokenAsync(rsp.Content);
            return true;
        }
        else if (rsp.Code != unchecked((uint)ApiRspCode.Unauthorized))
        {
            logger.LogWarning("RefreshToken fail, Code: {0}", rsp.Code);
        }
        return false;
    }

    async Task<bool> RefreshTokenWithSaveAsync(Uri baseAddress, JsonWebTokenValue jwt)
    {
        taskRefreshTokenWithSaveAsync ??= RefreshTokenWithSaveCoreAsync(baseAddress, jwt);
        var r = await taskRefreshTokenWithSaveAsync;
        return r;
    }

    public Task<ApiRsp<TResponseModel?>> SendAsync<TRequestModel, TResponseModel>(
        Uri baseAddress,
        Func<HttpRequestMessage> requestFactory,
        TRequestModel? requestModel,
        bool isSecurity = false,
        bool isAnonymous = false,
        SerializableImplType serializableImplType = SerializableImplType.SystemTextJson,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        JsonTypeInfo<ApiRsp<TResponseModel?>>? jsonResponseModelTypeInfo = null,
        CancellationToken cancellationToken = default)
    {
        var r = SendCoreAsync(
            baseAddress,
            requestFactory,
            requestModel,
            isSecurity,
            isAnonymous,
            serializableImplType,
            jsonRequestTypeInfo,
            jsonResponseModelTypeInfo,
            cancellationToken: cancellationToken);
        return r;
    }

    async Task<ApiRsp<TResponseModel?>> SendCoreAsync<TRequestModel, TResponseModel>(
        Uri baseAddress,
        Func<HttpRequestMessage> requestFactory,
        TRequestModel? requestModel,
        bool isSecurity = false,
        bool isAnonymous = false,
        SerializableImplType serializableImplType = SerializableImplType.SystemTextJson,
        JsonTypeInfo<TRequestModel?>? jsonRequestTypeInfo = null,
        JsonTypeInfo<ApiRsp<TResponseModel?>>? jsonResponseModelTypeInfo = null,
        bool isRecursiveRetry = false,
        CancellationToken cancellationToken = default)
    {
        var request = requestFactory();
        Aes? aes = null;
        HttpResponseMessage? response = null;
        bool using_response = true;
        try
        {
            if (isSecurity)
            {
                // 行业标准加密
                aes = Aes.Create();
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                //request.Options.TryAdd(nameof(Aes), aes); // 将临时变量添加进请求选项，使函数外可以访问到
            }
            const AESUtils.Flags aesFlags = AESUtils.Flags.CipherMode_CBC | AESUtils.Flags.PaddingMode_PKCS7;
            request.Content ??= await GetRequestContentAsync(
                isSecurity, aes, serializableImplType,
                requestModel, jsonRequestTypeInfo, cancellationToken);
            switch (serializableImplType)
            {
                case SerializableImplType.SystemTextJson:
                    request.Headers.Accept.ParseAdd(isSecurity ?
                        MediaTypeNames.JSONSecurity :
                        MediaTypeNames.JSON);
                    break;
                //case SerializableImplType.MessagePack:
                //    request.Headers.Accept.ParseAdd(isSecurity ?
                //        MediaTypeNames.MessagePackSecurity :
                //        MediaTypeNames.MessagePack);
                //    break;
                case SerializableImplType.MemoryPack:
                    request.Headers.Accept.ParseAdd(isSecurity ?
                        MediaTypeNames.MemoryPackSecurity :
                        MediaTypeNames.MemoryPack);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializableImplType),
                        serializableImplType, null);
            }
            if (isSecurity)
            {
                ArgumentNullException.ThrowIfNull(aes);
                const int flagsLen = sizeof(ushort);
                Span<byte> skey_bytes = stackalloc byte[flagsLen + aes.IV.Length + aes.Key.Length];
                BitConverter.TryWriteBytes(skey_bytes, (ushort)aesFlags);
                aes.IV.CopyTo(skey_bytes[flagsLen..]);
                Span<byte> aesKeyReverse = stackalloc byte[aes.Key.Length];
                aes.Key.CopyTo(aesKeyReverse);
                aesKeyReverse.Reverse();
                aesKeyReverse.CopyTo(skey_bytes[(flagsLen + aes.IV.Length)..]);
                var padding = RSAUtils.GetDefaultPadding();
                var encryptData = RSAInstance.Encrypt(skey_bytes, padding);
                var skey_str = Convert.ToHexString(encryptData);
                request.Headers.Add(ApiConstants.Headers_SecurityKeyHex, skey_str);
                request.Headers.Add(ApiConstants.Headers_SecurityKeyPadding, padding.OaepHashAlgorithm.ToString() ?? string.Empty);
            }
            JsonWebTokenValue? jwt = null;
            if (!isAnonymous)
            {
                jwt = await SetRequestHeaderAuthorization(request);
            }
            HandleHttpRequest(request);
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var isAppObsolete = IsAppObsolete(response.Headers);
            if (isAppObsolete)
            {
                return ApiRspCode.AppObsolete;
            }
            var code = unchecked((ApiRspCode)response.StatusCode);
            if (!isAnonymous && code == ApiRspCode.Unauthorized && jwt != null)
            {
                if (!isRecursiveRetry) // 防止死循环递归调用
                {
                    // 401 时，调用 RefreshToken 重试
                    var isSuccessRefreshToken = await RefreshTokenWithSaveAsync(baseAddress, jwt);
                    if (isSuccessRefreshToken)
                    {
                        var r = await SendCoreAsync(
                            baseAddress,
                            requestFactory,
                            requestModel,
                            isSecurity,
                            isAnonymous,
                            serializableImplType,
                            jsonRequestTypeInfo,
                            jsonResponseModelTypeInfo,
                            isRecursiveRetry: true, // 防止死循环递归调用
                            cancellationToken: cancellationToken);
                        return r;
                    }
                }
                else
                {
                    return code;
                }
            }
            if (response.Content == null)
            {
                return code;
            }
            else if (typeof(TResponseModel) == typeof(byte[]))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return (TResponseModel)(object)bytes;
            }
            else if (typeof(TResponseModel) == typeof(string))
            {
                var str = await response.Content.ReadAsStringAsync(cancellationToken);
                return (TResponseModel)(object)str;
            }
            else if (typeof(TResponseModel) == typeof(Stream))
            {
                using_response = false;
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return (TResponseModel)(object)stream;
            }
            else if (typeof(TResponseModel) == typeof(JsonDocument))
            {
                var jsonDoc = await ReadAsJsonDocumentAsync(response.Content, isSecurity, aes, cancellationToken);
                return (TResponseModel?)(object?)jsonDoc;
            }
            else if (typeof(TResponseModel) == typeof(JsonElement))
            {
                var jsonDoc = await ReadAsJsonDocumentAsync(response.Content, isSecurity, aes, cancellationToken);
                return (TResponseModel?)(object?)jsonDoc?.RootElement;
            }
            else if (typeof(TResponseModel) == typeof(JsonElement?))
            {
                var jsonDoc = await ReadAsJsonDocumentAsync(response.Content, isSecurity, aes, cancellationToken);
                JsonElement? temp = jsonDoc?.RootElement;
                return (TResponseModel?)(object?)temp;
            }
            else
            {
                ArgumentNullException.ThrowIfNull(jsonResponseModelTypeInfo);
                var r = await ReadAsResponseModelAsync(response, isSecurity, aes, jsonResponseModelTypeInfo, cancellationToken);
                return r;
            }
        }
        catch (Exception ex)
        {
            return ex;
        }
        finally
        {
            aes?.Dispose();
            if (using_response)
            {
                response?.Dispose();
            }
        }
    }
}
using System.Net;

namespace AigioLTemplate.Models;

public enum ApiRspCode : uint
{
    #region Http 状态码，100 ~ 511

    #region 1xx 信息状态码 100~199

    /// <inheritdoc cref="HttpStatusCode.Continue"/>
    Continue = 100,

    /// <inheritdoc cref="HttpStatusCode.SwitchingProtocols"/>
    SwitchingProtocols = 101,

    /// <inheritdoc cref="HttpStatusCode.Processing"/>
    Processing = 102,

    /// <inheritdoc cref="HttpStatusCode.EarlyHints"/>
    EarlyHints = 103,

    #endregion

    #region 2xx 成功状态码 200~299 (Code >= 200u) && (Code <= 299u)

    /// <summary>
    /// 成功
    /// </summary>
    OK = 200,

    /// <inheritdoc cref="HttpStatusCode.Created"/>
    Created = 201,

    /// <inheritdoc cref="HttpStatusCode.Accepted"/>
    Accepted = 202,

    /// <inheritdoc cref="HttpStatusCode.NonAuthoritativeInformation"/>
    NonAuthoritativeInformation = 203,

    /// <inheritdoc cref="HttpStatusCode.NoContent"/>
    NoContent = 204,

    /// <inheritdoc cref="HttpStatusCode.ResetContent"/>
    ResetContent = 205,

    /// <inheritdoc cref="HttpStatusCode.PartialContent"/>
    PartialContent = 206,

    /// <inheritdoc cref="HttpStatusCode.MultiStatus"/>
    MultiStatus = 207,

    /// <inheritdoc cref="HttpStatusCode.AlreadyReported"/>
    AlreadyReported = 208,

    /// <inheritdoc cref="HttpStatusCode.IMUsed"/>
    IMUsed = 226,

    #endregion

    #region 3xx 重定向状态码 300~399

    /// <inheritdoc cref="HttpStatusCode.MultipleChoices"/>
    MultipleChoices = 300,

    /// <inheritdoc cref="HttpStatusCode.Ambiguous"/>
    Ambiguous = 300,

    /// <inheritdoc cref="HttpStatusCode.MovedPermanently"/>
    MovedPermanently = 301,

    /// <inheritdoc cref="HttpStatusCode.Moved"/>
    Moved = 301,

    /// <inheritdoc cref="HttpStatusCode.Found"/>
    Found = 302,

    /// <inheritdoc cref="HttpStatusCode.Redirect"/>
    Redirect = 302,

    /// <inheritdoc cref="HttpStatusCode.SeeOther"/>
    SeeOther = 303,

    /// <inheritdoc cref="HttpStatusCode.RedirectMethod"/>
    RedirectMethod = 303,

    /// <inheritdoc cref="HttpStatusCode.NotModified"/>
    NotModified = 304,

    /// <inheritdoc cref="HttpStatusCode.UseProxy"/>
    UseProxy = 305,

    /// <inheritdoc cref="HttpStatusCode.Unused"/>
    Unused = 306,

    /// <inheritdoc cref="HttpStatusCode.TemporaryRedirect"/>
    TemporaryRedirect = 307,

    /// <inheritdoc cref="HttpStatusCode.RedirectKeepVerb"/>
    RedirectKeepVerb = 307,

    /// <inheritdoc cref="HttpStatusCode.PermanentRedirect"/>
    PermanentRedirect = 308,

    #endregion

    #region 4xx 客户端错误状态码 400~499

    /// <inheritdoc cref="HttpStatusCode.BadRequest"/>
    BadRequest = 400,

    /// <inheritdoc cref="HttpStatusCode.Unauthorized"/>
    Unauthorized = 401,

    /// <inheritdoc cref="HttpStatusCode.PaymentRequired"/>
    PaymentRequired = 402,

    /// <inheritdoc cref="HttpStatusCode.Forbidden"/>
    Forbidden = 403,

    /// <inheritdoc cref="HttpStatusCode.NotFound"/>
    NotFound = 404,

    /// <inheritdoc cref="HttpStatusCode.MethodNotAllowed"/>
    MethodNotAllowed = 405,

    /// <inheritdoc cref="HttpStatusCode.NotAcceptable"/>
    NotAcceptable = 406,

    /// <inheritdoc cref="HttpStatusCode.ProxyAuthenticationRequired"/>
    ProxyAuthenticationRequired = 407,

    /// <inheritdoc cref="HttpStatusCode.RequestTimeout"/>
    RequestTimeout = 408,

    /// <inheritdoc cref="HttpStatusCode.Conflict"/>
    Conflict = 409,

    /// <inheritdoc cref="HttpStatusCode.Gone"/>
    Gone = 410,

    /// <inheritdoc cref="HttpStatusCode.LengthRequired"/>
    LengthRequired = 411,

    /// <inheritdoc cref="HttpStatusCode.PreconditionFailed"/>
    PreconditionFailed = 412,

    /// <inheritdoc cref="HttpStatusCode.RequestEntityTooLarge"/>
    RequestEntityTooLarge = 413,

    /// <inheritdoc cref="HttpStatusCode.RequestUriTooLong"/>
    RequestUriTooLong = 414,

    /// <inheritdoc cref="HttpStatusCode.UnsupportedMediaType"/>
    UnsupportedMediaType = 415,

    /// <inheritdoc cref="HttpStatusCode.RequestedRangeNotSatisfiable"/>
    RequestedRangeNotSatisfiable = 416,

    /// <inheritdoc cref="HttpStatusCode.ExpectationFailed"/>
    ExpectationFailed = 417,

    /// <inheritdoc cref="HttpStatusCode.MisdirectedRequest"/>
    MisdirectedRequest = 421,

    /// <inheritdoc cref="HttpStatusCode.UnprocessableEntity"/>
    UnprocessableEntity = 422,

    /// <inheritdoc cref="HttpStatusCode.UnprocessableContent"/>
    UnprocessableContent = 422,

    /// <inheritdoc cref="HttpStatusCode.Locked"/>
    Locked = 423,

    /// <inheritdoc cref="HttpStatusCode.FailedDependency"/>
    FailedDependency = 424,

    /// <inheritdoc cref="HttpStatusCode.UpgradeRequired"/>
    UpgradeRequired = 426,

    /// <inheritdoc cref="HttpStatusCode.PreconditionRequired"/>
    PreconditionRequired = 428,

    /// <inheritdoc cref="HttpStatusCode.TooManyRequests"/>
    TooManyRequests = 429,

    /// <inheritdoc cref="HttpStatusCode.RequestHeaderFieldsTooLarge"/>
    RequestHeaderFieldsTooLarge = 431,

    /// <inheritdoc cref="HttpStatusCode.UnavailableForLegalReasons"/>
    UnavailableForLegalReasons = 451,

    #endregion

    #region 5xx 服务器错误状态码 500~511

    /// <inheritdoc cref="HttpStatusCode.InternalServerError"/>
    InternalServerError = 500,

    /// <inheritdoc cref="HttpStatusCode.NotImplemented"/>
    NotImplemented = 501,

    /// <inheritdoc cref="HttpStatusCode.BadGateway"/>
    BadGateway = 502,

    /// <summary>
    /// 服务不可用，服务器停机维护或者已超载
    /// </summary>
    ServiceUnavailable = 503,

    /// <inheritdoc cref="HttpStatusCode.GatewayTimeout"/>
    GatewayTimeout = 504,

    /// <inheritdoc cref="HttpStatusCode.HttpVersionNotSupported"/>
    HttpVersionNotSupported = 505,

    /// <inheritdoc cref="HttpStatusCode.VariantAlsoNegotiates"/>
    VariantAlsoNegotiates = 506,

    /// <inheritdoc cref="HttpStatusCode.InsufficientStorage"/>
    InsufficientStorage = 507,

    /// <inheritdoc cref="HttpStatusCode.LoopDetected"/>
    LoopDetected = 508,

    /// <inheritdoc cref="HttpStatusCode.NotExtended"/>
    NotExtended = 510,

    /// <inheritdoc cref="HttpStatusCode.NetworkAuthenticationRequired"/>
    NetworkAuthenticationRequired = 511,

    #endregion

    #endregion

    Timeout = HttpStatusCode.RequestTimeout,

    Exception = HttpStatusCode.InternalServerError,

    #region 错误状态码 5000~5999

    /// <summary>
    /// 启动游戏进程中发生闪退（游戏进程启动就退出）
    /// </summary>
    StartGameProcessHasExited = 5000,

    /// <summary>
    /// 文件未找到
    /// </summary>
    FileNotFound = 5001,

    /// <summary>
    /// 等待修改器准备信号超时
    /// </summary>
    WaitTrainerReadySignalTimeout = 5002,

    #endregion

    #region Exception 状态码 6xxx

    Win32Exception = 6001,
    InvalidOperationException = 6002,
    ObjectDisposedException = 6003,
    ArgumentNullException = 6004,
    IOException = 6005,
    EndOfStreamException = 6006,
    UnauthorizedAccessException = 6007,

    #endregion

    #region 7xxx

    ProcessIsNull = 7001,
    PipeStreamIsNull = 7002,
    CommandNameIsNullOrEmpty = 7003,
    ProcessPathIsNull = 7004,
    ProcessHasExited = 7005,

    #endregion

    #region 8xxx

    IpcConnectTimeout = 8001,

    #endregion
}

public static partial class ApiRspCodeExtensions
{
    public static uint GetCodeByException(Exception ex)
    {
        try
        {
            var exTypeName = ex.GetType().Name;
            if (Enum.TryParse<ApiRspCode>(exTypeName, false, out var result))
            {
                return unchecked((uint)result);
            }
        }
        catch
        {
        }
        return unchecked((uint)ApiRspCode.Exception);
    }
}
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.Models;
using AigioLTemplate.Commands.Features;
using AigioLTemplate.Commands.Features.Essentials;
using AigioLTemplate.Commands.Features.WebApi;
using AigioLTemplate.Models.Ipc;
using AigioLTemplate.Models.Ipc.Queues.Abstractions;
using AigioLTemplate.Models.UI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace AigioLTemplate.Models;

[JsonSerializable(typeof(ApiRsp))]
[JsonSerializable(typeof(ApiRsp<bool>))]
[JsonSerializable(typeof(ApiRsp<byte>))]
[JsonSerializable(typeof(ApiRsp<sbyte>))]
[JsonSerializable(typeof(ApiRsp<ushort>))]
[JsonSerializable(typeof(ApiRsp<short>))]
[JsonSerializable(typeof(ApiRsp<uint>))]
[JsonSerializable(typeof(ApiRsp<int>))]
[JsonSerializable(typeof(ApiRsp<ulong>))]
[JsonSerializable(typeof(ApiRsp<long>))]
[JsonSerializable(typeof(ApiRsp<Guid>))]
[JsonSerializable(typeof(ApiRsp<float>))]
[JsonSerializable(typeof(ApiRsp<double>))]
[JsonSerializable(typeof(ApiRsp<decimal>))]
[JsonSerializable(typeof(ApiRsp<DateOnly>))]
[JsonSerializable(typeof(ApiRsp<DateTime>))]
[JsonSerializable(typeof(ApiRsp<DateTimeOffset>))]
[JsonSerializable(typeof(ApiRsp<bool?>))]
[JsonSerializable(typeof(ApiRsp<byte?>))]
[JsonSerializable(typeof(ApiRsp<sbyte?>))]
[JsonSerializable(typeof(ApiRsp<ushort?>))]
[JsonSerializable(typeof(ApiRsp<short?>))]
[JsonSerializable(typeof(ApiRsp<uint?>))]
[JsonSerializable(typeof(ApiRsp<int?>))]
[JsonSerializable(typeof(ApiRsp<ulong?>))]
[JsonSerializable(typeof(ApiRsp<long?>))]
[JsonSerializable(typeof(ApiRsp<Guid?>))]
[JsonSerializable(typeof(ApiRsp<float?>))]
[JsonSerializable(typeof(ApiRsp<double?>))]
[JsonSerializable(typeof(ApiRsp<decimal?>))]
[JsonSerializable(typeof(ApiRsp<DateOnly?>))]
[JsonSerializable(typeof(ApiRsp<DateTime?>))]
[JsonSerializable(typeof(ApiRsp<DateTimeOffset?>))]
[JsonSerializable(typeof(ApiRsp<string>))]
[JsonSerializable(typeof(ApiRsp<nil>))]
[JsonSerializable(typeof(ApiRsp<nil?>))]
[JsonSerializable(typeof(ApiRsp<JsonElement?>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(WindowPropertyChangedEventArgs))]
[JsonSerializable(typeof(WindowCommandArgs))]
[JsonSerializable(typeof(ApiRsp<HotkeyRegistrationResult[]>))]
//[JsonSerializable(typeof(ReplaceHotkeyArgs))]
[JsonSerializable(typeof(ApiRsp<HotkeyRegistrationResult>))]
//[JsonSerializable(typeof(HotkeyInfo))]
[JsonSerializable(typeof(HotkeyPressedEventArgs))]
[JsonSerializable(typeof(InMessageUpdateValueModel))]
#if DEBUG
[JsonSerializable(typeof(global::AigioLTemplate.Commands.Features.Samples.有请求无响应示例请求))]
[JsonSerializable(typeof(global::AigioLTemplate.Commands.Features.Samples.有请求有响应示例请求))]
#endif
[JsonSerializable(typeof(QueueItemBase))]
[JsonSerializable(typeof(ApiRsp<VersionTrackingModel>))]
[JsonSerializable(typeof(ApiRsp<DeviceInfoModel>))]
[JsonSerializable(typeof(ApiRsp<HttpClientBaseAddressModel>))]
[JsonSerializable(typeof(FetchRequestInit))]
[JsonSerializable(typeof(ApiRsp<DeviceIdModel>))]
[JsonSerializable(typeof(ApiRsp<LoginOrRegisterResponse>))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true)]
sealed partial class DefaultJsonSerializerContext_ : JsonSerializerContext
{
    static DefaultJsonSerializerContext_()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new DefaultJsonSerializerContext_(o);
    }
}

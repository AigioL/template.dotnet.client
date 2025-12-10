using AigioL.Common.Essentials.Devices;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;

namespace AigioLTemplate.Commands.Features.Essentials;

using TArgs = nil;
using TCommand = DeviceInfo;
using TResult = DeviceInfoModel;

/// <inheritdoc cref="IDeviceInfo"/>
sealed partial class DeviceInfo
{
    internal static TResult Invoke()
    {
        var deviceInfo = Ioc.Default.GetRequiredService<IDeviceInfo>();
        return TResult.Create(deviceInfo);
    }
}

partial class DeviceInfo :
    IV2FeatureCommand<TArgs, TResult>,
    IV2FeatureCommandFunc
{
    public static ValueTask<ApiRsp<TResult?>> InvokeAsync(TArgs args, CancellationToken cancellationToken)
    {
        var r = Invoke();
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

/// <inheritdoc cref="IDeviceInfo"/>
sealed partial record DeviceInfoModel(
    DeviceType DeviceType,
    DeviceIdiom Idiom,
    string Manufacturer,
    string Model,
    string Name,
    DevicePlatform2 Platform,
    string VersionString)
{
    internal static TResult Create(IDeviceInfo deviceInfo)
    {
        TResult r = new(
            deviceInfo.DeviceType,
            deviceInfo.Idiom,
            deviceInfo.Manufacturer,
            deviceInfo.Model,
            deviceInfo.Name,
            deviceInfo.Platform,
            deviceInfo.VersionString);
        return r;
    }
}

partial record DeviceInfoModel : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
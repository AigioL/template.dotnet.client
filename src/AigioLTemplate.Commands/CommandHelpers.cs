using AigioLTemplate.Commands.Features;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Commands.Features.Essentials;
using AigioLTemplate.Commands.Features.WebApi;
using System.Collections.Immutable;

namespace AigioLTemplate.Commands;

static partial class CommandHelpers
{
    internal static void ConfigureCommands()
    {
        var keyComparer = StringComparer.InvariantCultureIgnoreCase; // key 值忽略大小写

        var dict = new Dictionary<string, nint>(keyComparer);

        #region 添加业务命令

        //dict.AddCommand<RegisterHotkey>();
        //dict.AddCommand<ReplaceHotkey>();

        dict.AddCommand<MainWindow>();
        dict.AddCommand<Window>();

        dict.AddCommand<VersionTracking>();
        dict.AddCommand<DeviceInfo>();

        dict.AddCommand<HttpClientBaseAddress>();

        dict.AddCommand<AigioLTemplateFetch>();
        dict.AddCommand<GetDeviceId>();

#if DEBUG
        dict.AddCommand<global::AigioLTemplate.Commands.Features.Samples.无请求无响应示例>();
        dict.AddCommand<global::AigioLTemplate.Commands.Features.Samples.无请求有响应示例>();
        dict.AddCommand<global::AigioLTemplate.Commands.Features.Samples.有请求无响应示例>();
        dict.AddCommand<global::AigioLTemplate.Commands.Features.Samples.有请求有响应示例>();
#endif

        #endregion

        _94412478.dict = dict.ToImmutableDictionary(keyComparer);
#if USE_JSON_RPC
        InitJsonRpc();
#endif
    }

    internal static bool TryGetSerializableImplTypeByContentType(
        string contentType,
        out SerializableImplType implType)
    {
        switch (contentType)
        {
            case "application/json":
            case "application/vnd.sapi+x-json":
                implType = SerializableImplType.SystemTextJson;
                return true;
            case "application/x-memorypack":
            case "application/vnd.sapi+x-memorypack":
                implType = SerializableImplType.MemoryPack;
                return true;
            default:
                implType = default;
                return false;
        }
    }

    internal static nint TryGetValue(string commandName)
    {
        if (_94412478.dict != null && _94412478.dict.TryGetValue(commandName, out var value))
        {
            return value;
        }
        return default;
    }

    internal static async ValueTask<bool> InvokeAsync(string commandName, SerializableImplType implType, Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default)
    {
        if (_94412478.dict != null && _94412478.dict.TryGetValue(commandName, out var value))
        {
            ValueTask t;
            unsafe
            {
                var funcptr = (delegate* managed<SerializableImplType, Stream?, Stream, CancellationToken, ValueTask>)value;
                t = funcptr(implType, inputStream, outputStream, cancellationToken);
            }
            await t;
            return true;
        }

        return false;
    }

#if PROJ_WEBHOST
    internal static ImmutableDictionary<string, nint> GetCommands()
    {
        ArgumentNullException.ThrowIfNull(_94412478.dict);
        return _94412478.dict;
    }
#endif
}

file static class _94412478
{
    internal static ImmutableDictionary<string, nint>? dict;

    internal static void AddCommand<TCommand>(this Dictionary<string, nint> dict) where TCommand : IV2FeatureCommandFunc
    {
        var name = typeof(TCommand).Name;
        var funcptr = TCommand.GetFuncPtr();
        dict.Add(name, funcptr);
    }
}

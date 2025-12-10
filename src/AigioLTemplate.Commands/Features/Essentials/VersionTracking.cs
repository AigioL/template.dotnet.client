using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using AigioLTemplate.Commands.Features.Abstractions;
using AigioLTemplate.Models;
using System.Collections.Immutable;

namespace AigioLTemplate.Commands.Features.Essentials;

using TArgs = nil;
using TCommand = VersionTracking;
using TResult = VersionTrackingModel;

/// <inheritdoc cref="IVersionTracking"/>
sealed partial class VersionTracking
{
    internal static TResult Invoke()
    {
        var versionTracking = Ioc.Default.GetRequiredService<IVersionTracking>();
        return TResult.Create(versionTracking);
    }
}

partial class VersionTracking :
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

/// <inheritdoc cref="IVersionTracking"/>
sealed partial record VersionTrackingModel(
    ImmutableArray<string> BuildHistory,
    string CurrentBuild,
    string CurrentVersion,
    string? FirstInstalledBuild,
    string? FirstInstalledVersion,
    bool IsFirstLaunchEver,
    bool IsFirstLaunchForCurrentBuild,
    bool IsFirstLaunchForCurrentVersion,
    string? PreviousBuild,
    string? PreviousVersion,
    ImmutableArray<string> VersionHistory)
{
    internal static TResult Create(IVersionTracking versionTracking)
    {
        TResult r = new(
            versionTracking.BuildHistory,
            versionTracking.CurrentBuild,
            versionTracking.CurrentVersion,
            versionTracking.FirstInstalledBuild,
            versionTracking.FirstInstalledVersion,
            versionTracking.IsFirstLaunchEver,
            versionTracking.IsFirstLaunchForCurrentBuild,
            versionTracking.IsFirstLaunchForCurrentVersion,
            versionTracking.PreviousBuild,
            versionTracking.PreviousVersion,
            versionTracking.VersionHistory);
        return r;
    }
}

partial record VersionTrackingModel : global::System.Text.Json.Serialization.IJsonSerializerContext
{
    /// <inheritdoc/>
    static global::System.Text.Json.Serialization.JsonSerializerContext global::System.Text.Json.Serialization.IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}
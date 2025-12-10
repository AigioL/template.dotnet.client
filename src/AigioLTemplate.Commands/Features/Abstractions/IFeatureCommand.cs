//using AigioLTemplate.Models;
//using Newtonsoft.Json.Linq;
//using System.Diagnostics.CodeAnalysis;

//namespace AigioLTemplate.Commands.Features.Abstractions;

///// <summary>
///// 功能业务命令接口
///// </summary>
//partial interface IFeatureCommand
//{
//    /// <summary>
//    /// 使用请求流和响应流执行业务命令
//    /// </summary>
//    internal static abstract ValueTask InvokeAsync(
//        SerializableImplType implType,
//        Stream request,
//        Stream response,
//        CancellationToken cancellationToken = default);

//    //#if W1M3_COMPAT
//    //    internal static abstract Task<object?> InvokeAsync(
//    //        JToken? jToken,
//    //        IProgress<object> progress,
//    //        CancellationToken cancellationToken = default); // interface d | Task<object> _0(_89 _0, IProgress<object> _1, CancellationToken _2);
//    //#endif
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs>
//{
//    internal static abstract bool Invoke(TCommandArgs? args);
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>
//{
//    internal static abstract TResult Invoke(TCommandArgs? args);
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommandVoid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>
//{
//    internal static abstract TResult Invoke();
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommandVoid
//{
//    internal static abstract bool Invoke();
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommandVoid2
//{
//    internal static abstract ApiRsp Invoke();
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommandVoid2<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContent>
//{
//    internal static abstract ApiRsp<TContent> Invoke();
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommand3<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs>
//{
//    internal static abstract ApiRsp Invoke(TCommandArgs? args);
//}

///// <inheritdoc cref="IFeatureCommand"/>
//interface IFeatureCommand3<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommandArgs, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TContent>
//{
//    internal static abstract ApiRsp<TContent> Invoke(TCommandArgs? args);
//}
namespace AigioLTemplate.Commands.Features.Abstractions;

interface IV2FeatureCommandFunc : IFeatureCommand
{
    internal static abstract nint GetFuncPtr();

    //#if W1M3_COMPAT
    //    internal static abstract global::System.Func<global::Newtonsoft.Json.Linq.JToken?, global::System.IProgress<object>, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task<object?>> GetDelegateW1M3Compat();
    //#endif
}
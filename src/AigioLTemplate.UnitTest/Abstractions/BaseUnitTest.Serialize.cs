using AigioLTemplate.Models;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AigioLTemplate.UnitTest.Abstractions;

partial class BaseUnitTest
{
    protected static string Serialize(object? obj, bool writeIndented = true)
    {
        var o = JSO.GetOptions(writeIndented);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        var json = JsonSerializer.Serialize(obj, o);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        return json;
    }
}

file static class JSO
{
    static void AddDefaultJsonTypeInfoResolver(JsonSerializerOptions o)
    {
        if (!o.TypeInfoResolverChain.OfType<DefaultJsonTypeInfoResolver>().Any())
        {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            o.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }
    }

    static readonly Lazy<JsonSerializerOptions> lazyOptions = new(() =>
    {
        JsonSerializerOptions o = new(DefaultJsonSerializerContext_.Default.Options)
        {
            WriteIndented = false,
        };
        AddDefaultJsonTypeInfoResolver(o);
        return o;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    static readonly Lazy<JsonSerializerOptions> lazyWriteIndentedOptions = new(() =>
    {
        JsonSerializerOptions o = new(DefaultJsonSerializerContext_.Default.Options)
        {
            WriteIndented = true,
        };
        AddDefaultJsonTypeInfoResolver(o);
        return o;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static JsonSerializerOptions GetOptions(bool writeIndented = true)
    {
        if (writeIndented)
        {
            return lazyWriteIndentedOptions.Value;
        }
        return lazyOptions.Value;
    }
}
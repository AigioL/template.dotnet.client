#if !PROJ_LIBRARY
using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioLTemplate.Models;

[JsonSerializable(typeof(ApiRsp))]
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
#endif
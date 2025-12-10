using AigioLTemplate.Constants;
using System.Text.Json.Serialization;

namespace AigioLTemplate.Models.Ipc.Events.Abstractions;

#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public partial record class EventDataBase : IEventData, IJsonSerializerContext
{
    /// <inheritdoc/>
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;

    public string? Event { get; set; }

    protected virtual object? GetData() => null;

    protected virtual Type GetDataType() => typeof(nil?);

    object? IEventData.Data => GetData();

    Type IEventData.DataType => GetDataType();
}
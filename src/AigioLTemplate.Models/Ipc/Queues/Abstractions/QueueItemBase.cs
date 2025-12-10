using AigioLTemplate.Constants;
using AigioLTemplate.Models.Ipc.Events.Abstractions;
using System.Text.Json.Serialization;

namespace AigioLTemplate.Models.Ipc.Queues.Abstractions;

#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = IJsonSerializerContext.TypeDiscriminatorPropertyName)] // 自定义类型鉴别器名称
[global::MemoryPack.MemoryPackUnion((ushort)QueueItemEventName.Elevated, typeof(ElevatedQueueItem))]
[JsonDerivedType(typeof(ElevatedQueueItem), typeDiscriminator: nameof(QueueItemEventName.Elevated))]
[global::MemoryPack.MemoryPackUnion((ushort)QueueItemEventName.Hotkey, typeof(HotkeyQueueItem))]
[JsonDerivedType(typeof(HotkeyQueueItem), typeDiscriminator: nameof(QueueItemEventName.Hotkey))]
public abstract partial record class QueueItemBase : IQueueItem, IJsonSerializerContext
{
    /// <inheritdoc/>
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;

    [global::MemoryPack.MemoryPackConstructor]
    [global::System.Text.Json.Serialization.JsonConstructor]
    public QueueItemBase()
    {
    }

    internal QueueItemBase(int id, QueueItemStatus status)
    {
        Id = id;
        Status = status;
    }

    public int Id { get; set; }

    public QueueItemStatus Status { get; set; }

    protected virtual object? GetData() => null;

    protected virtual Type GetDataType() => typeof(nil?);

    object? IQueueItem.Data => GetData();

    Type IQueueItem.DataType => GetDataType();

    internal IEventData? GetEventData()
    {
        if (Status == QueueItemStatus.EventData && GetData() is IEventData eventData)
        {
            return eventData;
        }
        return null;
    }
}

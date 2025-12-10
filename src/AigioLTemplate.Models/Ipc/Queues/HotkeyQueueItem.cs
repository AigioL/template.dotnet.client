using AigioLTemplate.Constants;
using AigioLTemplate.Models.Ipc.Events;
using AigioLTemplate.Models.Ipc.Queues.Abstractions;
using TData = global::AigioLTemplate.Models.Ipc.Events.Int32ArrayEventData;

namespace AigioLTemplate.Models.Ipc.Queues;

#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public sealed partial record class HotkeyQueueItem : QueueItemBase, IQueueItem<TData>
{
    public TData? Data { get; set; }

    protected override object? GetData() => Data;

    protected override Type GetDataType() => typeof(TData);

    [global::MemoryPack.MemoryPackConstructor]
    [global::System.Text.Json.Serialization.JsonConstructor]
    public HotkeyQueueItem() : base()
    {
    }

    internal HotkeyQueueItem(int id, int[]? data = default) : base(id, QueueItemStatus.EventData)
    {
        Data = new()
        {
            Data = data,
            Event = nameof(QueueItemEventName.Hotkey),
        };
    }

    internal HotkeyQueueItem(int[]? data = default) : this(0, data)
    {
    }
}

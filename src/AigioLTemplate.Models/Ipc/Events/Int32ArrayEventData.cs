using AigioLTemplate.Constants;
using AigioLTemplate.Models.Ipc.Events.Abstractions;
using TData = System.Int32[];

namespace AigioLTemplate.Models.Ipc.Events;

#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public sealed partial record class Int32ArrayEventData : EventDataBase, IEventData<TData>
{
    public TData? Data { get; set; }

    protected override object? GetData() => Data;

    protected override Type GetDataType() => typeof(TData);
}

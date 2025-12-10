using AigioLTemplate.Constants;
using AigioLTemplate.Models.Ipc.Events.Abstractions;

namespace AigioLTemplate.Models.Ipc.Events;

#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public sealed partial record class EventNameData : EventDataBase
{
}

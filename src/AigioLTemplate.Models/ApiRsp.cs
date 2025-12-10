#if !DISABLE_MP2 && !NETFRAMEWORK
using MemoryPack;
#endif
using AigioLTemplate.Models;
using System.Text.Json;
using ApiRspCodeCustom = AigioLTemplate.Models.ApiRspCode;

namespace AigioL.Common.Models;

public partial record class ApiRsp
{
    public static implicit operator ApiRsp(ApiRspCodeCustom code) => new() { Code = unchecked((uint)code) };

#if !PROJ_DLLEXPORT && !NETFRAMEWORK
    internal static async ValueTask SerializeAsync(
        ApiRsp result,
        SerializableImplType implType,
        Stream response,
        CancellationToken cancellationToken = default)
    {
        // 序列化响应结果
        switch (implType)
        {
            case SerializableImplType.SystemTextJson:
                {
                    await JsonSerializer.SerializeAsync(response, result, DefaultJsonSerializerContext_.Default.ApiRsp, cancellationToken);
                }
                break;
            case SerializableImplType.MemoryPack:
                {
                    await MemoryPackSerializer.SerializeAsync(response, result, cancellationToken: cancellationToken);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(implType), implType, null);
        }

    }
#endif
}

public sealed partial record class ApiRsp<TContent>
{
    public static implicit operator ApiRsp<TContent>(ApiRspCodeCustom code) => new() { Code = unchecked((uint)code) };
}

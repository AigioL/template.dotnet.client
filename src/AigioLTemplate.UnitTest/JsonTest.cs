using AigioLTemplate.Commands.Features;
using AigioLTemplate.Models;
using AigioLTemplate.Models.Ipc;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace AigioLTemplate.UnitTest;

public sealed class JsonTest : BaseUnitTest
{
    readonly JsonSerializerOptions o = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
    };

    [Fact]
    public void GuidTest()
    {
        var guid = Guid.Empty;
        var typeInfo = (JsonTypeInfo<Guid>)DefaultJsonSerializerContext_.Default.GetTypeInfo(typeof(Guid))!;
        var json = JsonSerializer.Serialize(guid, typeInfo);
        Console.WriteLine(json);
        var deserializedGuid = JsonSerializer.Deserialize(json, typeInfo);
        Assert.Equal(guid, deserializedGuid);
    }

    [Fact]
    public void MessageReceivedKeyType_Exception()
    {
        var ex = new ApplicationException(
"""
测试异常字符串中出现单引号与双引号，“"';[]./#!%#@!"%@#!^"
""");

        ReadOnlySpan<byte> l =
"""
{"k": 500,"v": "
"""u8; // 500 = MessageReceivedKeyType.Exception
        ReadOnlySpan<byte> r =
"""
"}
"""u8;
        var exString = ex.ToString() ?? "";
        var exBufferLength = Encoding.UTF8.GetByteCount(exString);
        var exBuffer = ArrayPool<byte>.Shared.Rent(exBufferLength);
        Span<byte> exSpan = exBuffer.AsSpan(0, exBufferLength);
        ReadOnlySpan<byte> exSpanEncoded;
        try
        {
            Encoding.UTF8.TryGetBytes(exString, exSpan, out var bytesWritten);
            exSpan = exSpan[..bytesWritten];
            exSpanEncoded = JsonEncodedText.Encode(exSpan).EncodedUtf8Bytes;

            byte[] rspMessage = new byte[l.Length + exSpanEncoded.Length + r.Length];
            l.CopyTo(rspMessage);
            exSpanEncoded.CopyTo(rspMessage.AsSpan(l.Length));
            r.CopyTo(rspMessage.AsSpan(l.Length + exSpanEncoded.Length));

            var json = Encoding.UTF8.GetString(rspMessage);
            var n = JsonNode.Parse(json)!; // 解析 JSON 字符串以验证格式正确
            Console.WriteLine(n.ToJsonString(o));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(exBuffer);
        }
    }

    [Fact]
    public async Task MessageReceivedKeyType_InMessageUpdateValue()
    {
        var m = new InMessageUpdateValueModel
        {
            Name = "variableName",
            Value = 2,
        };
        byte[] buffer;
        using var stream = new MemoryStream();
        stream.Write(
"""
{"k": 1,"v": 
"""u8); // 2 = MessageReceivedKeyType.InMessageUpdateValue
        await JsonSerializer.SerializeAsync(stream, m, DefaultJsonSerializerContext_.Default.InMessageUpdateValueModel, TestContext.Current.CancellationToken);
        stream.Write(
"""
}
"""u8);
        stream.Position = 0;
        buffer = new byte[stream.Length];
        stream.ReadExactly(buffer, 0, buffer.Length);

        var json = Encoding.UTF8.GetString(buffer);
        var n = JsonNode.Parse(json)!; // 解析 JSON 字符串以验证格式正确
        Console.WriteLine(n.ToJsonString(o));
    }
}
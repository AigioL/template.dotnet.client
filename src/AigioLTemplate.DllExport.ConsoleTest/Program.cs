using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AigioLTemplate;

static partial class Program
{
    static int currentTaskId = 1;
    static readonly bool UseAotDll = false; // 是否使用 AOT 发布的 DLL
    static int backendProcessId;

    static async Task Main(string[] args)
    {
        try
        {
            await MainCore(args);
        }
        finally
        {
            if (backendProcessId != 0)
            {
                try
                {
                    // 退出时结束后端进程
                    Process.GetProcessById(backendProcessId)?.Kill(true);
                }
                catch
                {
                }
            }
        }
    }

    static Guid idProcessAdditional;

    static async Task MainCore(string[] args)
    {
        // 1. AOT 发布类库 AigioLTemplate.DllExport，使用 win-x64.dll.pubxml
        // 2. Debug 生成项目 AigioLTemplate.WebHost
        // 3. 启动当前项目调试

        NativeLibrary.SetDllImportResolver(typeof(Program).Assembly, DllImportResolver);

        var backendProcessPath = GetBackendProcessPath();
        var errorCode = StartBackendAsPrivileged(
            "-clt main -n true",
            backendProcessPath,
            out backendProcessId,
            out var nativeErrorCode);
        Console.WriteLine(
$"以管理员权限启动后端进程，errorCode: {errorCode}, processId: {backendProcessId}, nativeErrorCode: {nativeErrorCode}"
);
        if (errorCode == ErrorCode.Success)
        {
            unsafe
            {
                SetOnMessageReceivedListener(&OnMessageReceived);
            }
        }
        while (true)
        {
            Console.WriteLine("输入指令：");
            var line = Console.ReadLine();
            switch (line)
            {
                case "t1":
                    {
                        var req = new JsonRpcRequest
                        {
                            Method = "TODO",
                            Arguments = JsonNode.Parse("{\"TODO\":\"TODO\"}"),
                        };
                        var rsp = await PostMessageAsync(req);
                        var rspO = (JsonObject)JsonNode.Parse(rsp)!;
                        idProcessAdditional = Guid.Parse(rspO["content"]?.ToString()!);
                    }
                    break;
                case "t2":
                    {
                        if (idProcessAdditional != default)
                        {
                            string variable = "infinite_health";
                            double value = 1;
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var split = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                                if (split.Length >= 2)
                                {
                                    if (double.TryParse(split[1].Trim(), out var v))
                                    {
                                        value = v;
                                        variable = split[0].Trim();
                                    }
                                }
                            }
                            var reqArgs =
"""
{"key":"
""" + idProcessAdditional +
"""
","variable":"
""" + variable +
"""
","value":
""" + value +
"""
}
""";
                            var req = new JsonRpcRequest
                            {
                                Method = "SendProcessAdditional",
                                Arguments = JsonNode.Parse(reqArgs),
                            };
                            var rsp = await PostMessageAsync(req);
                        }
                    }
                    break;
                case "无请求无响应示例":
                case "00":
                    {
                        var req = new JsonRpcRequest { Method = "无请求无响应示例", };
                        await PostMessageAsync(req);
                    }
                    break;
                case "无请求有响应示例":
                case "01":
                    {
                        var req = new JsonRpcRequest { Method = "无请求有响应示例", };
                        await PostMessageAsync(req);
                    }
                    break;
                case "有请求无响应示例":
                case "10":
                    {
                        var req = new JsonRpcRequest
                        {
                            Method = "有请求无响应示例",
                            Arguments = new 有请求无响应示例请求(3, "aaa", 5, 35.3M)
                            {
                            }
                        };
                        await PostMessageAsync(req);
                    }
                    break;
                case "有请求有响应示例":
                case "11":
                    {
                        var req = new JsonRpcRequest
                        {
                            Method = "有请求有响应示例",
                            Arguments = new 有请求有响应示例请求(Guid.CreateVersion7(), "bbb", 2.5f, 100.0d)
                            {
                            }
                        };
                        await PostMessageAsync(req);
                    }
                    break;
                case "exit":
                case "esc":
                case "e":
                    {
                        // TODO 安全退出
                    }
                    break;
                default:
                    {
                        var req = new JsonRpcRequest { Method = line, };
                        await PostMessageAsync(req);
                    }
                    break;

            }
        }
    }

    static string GetBackendProcessPath()
    {
        var versionProps = File.ReadAllText(Path.Combine(ProjPath, @"src\Version.props"));
        const string versionPrefix = "<WinSDK_Version>";
        var index = versionProps.IndexOf(versionPrefix);
        var s = versionProps.AsSpan(index + versionPrefix.Length);
        var indexR = s.IndexOf("</WinSDK_Version>");
        var winsdkver = s[..indexR].Trim();

        var exePath = Path.Combine(ProjPath, "src", "artifacts", "bin",
            "AigioLTemplate.WebHost", $"debug_net{Environment.Version.Major}.{Environment.Version.Minor}-windows{winsdkver}", "aigioltemplate.webhost.exe");
        return exePath;
    }

    [UnmanagedCallersOnly]
    static unsafe void OnMessageReceived(int message_len, byte* message_ptr)
    {
        var message = new ReadOnlySpan<byte>(message_ptr, message_len);
        var messageString = Encoding.UTF8.GetString(message);
        Console.WriteLine($"收到【SetOnMessageReceivedListener】消息：{messageString}");
    }

    static string? GetJsonRspMessage(byte[]? rspMessage, int rspMessage_len)
    {
        if (rspMessage_len == 0 || rspMessage == null)
        {
            return null;
        }

        var jobj = JsonNode.Parse(rspMessage.AsSpan(0, rspMessage_len));
        JsonSerializerOptions o = new(JsonSerializerOptions.Web) // 这里用了弱类型 object，需要反射实现，不支持 AOT
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,

            #region JsonSerializerDefaults.Web https://github.com/dotnet/runtime/blob/v9.0.7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L172-L174

            PropertyNameCaseInsensitive = true, // 忽略大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // 允许从字符串读取数字

            #endregion

            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // 忽略 null 值
        };
        var json = JsonSerializer.Serialize(jobj, o);
        return json;
    }

    static Task<string> PostMessageAsync(string commandName, byte[]? reqMessage)
    {
        Interlocked.Increment(ref currentTaskId);
        var requestId = currentTaskId;
        return PostMessageAsync(requestId, commandName, reqMessage);
    }

    static async Task<string> PostMessageAsync(int requestId, string commandName, byte[]? reqMessage)
    {
        (int taskId, ErrorCode errorCode, byte[] rspMessage, int rspMessage_len) = await PostMessageCoreAsync(requestId, commandName, reqMessage);
        var rspJsonMessage = GetJsonRspMessage(rspMessage, rspMessage_len)!;
        Console.WriteLine(
$"""
发送【SEND】，Method: {commandName}，ReqId: {requestId}, RspId: {taskId}, errorCode: {errorCode}
请求【REQ】: {(reqMessage == null ? null : Encoding.UTF8.GetString(reqMessage))}
响应【RSP】：{rspJsonMessage}
""");
        return rspJsonMessage;
    }

    static Task<string> PostMessageAsync(JsonRpcRequest request)
    {
        request.RequestId = currentTaskId++;

        var reqMessage = request.Arguments == null ? null : JsonSerializer.SerializeToUtf8Bytes(request.Arguments, DefaultJsonSerializerContext_.Default.Options);
        return PostMessageAsync(request.RequestId, request.Method ?? "", reqMessage);
    }

    static nint? lib_ptr;

    static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == Program.libraryName)
        {
            if (!lib_ptr.HasValue)
            {
                var fileName = $"{libraryName}{((int)RuntimeInformation.ProcessArchitecture)}.dll";
                var aotDll = Path.Combine(ProjPath, @$"src\artifacts\pub\AigioLTemplate.DllExport\win-{RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()}\Shared\{fileName}");

                if (!File.Exists(aotDll))
                {
                    var baseDir = Environment.ProcessPath;
                    baseDir = string.IsNullOrWhiteSpace(baseDir) ? null : Path.GetDirectoryName(baseDir);
                    if (baseDir != null)
                    {
                        aotDll = Path.Combine(baseDir, fileName);
                    }
                }

                Console.WriteLine($"已加载本机库：{(string.IsNullOrWhiteSpace(ProjPath) ? aotDll : Path.GetRelativePath(ProjPath, aotDll))}");
                lib_ptr = NativeLibrary.Load(aotDll);
            }
            return lib_ptr.Value;
        }
        return default;
    }

    const string libraryName = "aigioltemplate_ba5ac609";

    [LibraryImport(libraryName, EntryPoint = "aigioltemplate3")]
    private static unsafe partial int SetOnMessageReceivedListener_Import(delegate* unmanaged<int, byte*, void> onMessageReceivedListener);

    static unsafe int SetOnMessageReceivedListener(delegate* unmanaged<int, byte*, void> onMessageReceivedListener)
    {
        if (UseAotDll)
        {
            return SetOnMessageReceivedListener_Import(onMessageReceivedListener);
        }
        else
        {
            delegate* unmanaged<delegate* unmanaged<int, byte*, void>, int> ptr = &DllExport.SetOnMessageReceivedListener;
            return ptr(onMessageReceivedListener);
        }
    }

    [LibraryImport(libraryName, EntryPoint = "aigioltemplate5")]
    private static unsafe partial int StartBackendAsPrivileged_Import(
        int argc, char* argv,
        int processPath_len, char* processPath_ptr,
        int* processId,
        int* nativeErrorCode,
        int killOrFindBackendProcess = 0);

    static unsafe int StartBackendAsPrivileged(
        int argc, char* argv,
        int processPath_len, char* processPath_ptr,
        int* processId,
        int* nativeErrorCode,
        int killOrFindBackendProcess = 0)
    {
        if (UseAotDll)
        {
            return StartBackendAsPrivileged_Import(argc, argv, processPath_len, processPath_ptr, processId, nativeErrorCode, killOrFindBackendProcess);
        }
        else
        {
            delegate* unmanaged<int, char*, int, char*, int*, int*, int, int> ptr = &DllExport.StartBackendAsPrivileged;
            return ptr(argc, argv, processPath_len, processPath_ptr, processId, nativeErrorCode, killOrFindBackendProcess);
        }
    }

    static unsafe ErrorCode StartBackendAsPrivileged(string args, string processPath, out int processId, out int nativeErrorCode)
    {
        fixed (int* processIdLocal = &processId)
        {
            fixed (int* nativeErrorCodeLocal = &nativeErrorCode)
            {
                int result;
                fixed (char* argv = args)
                {
                    fixed (char* processPath_ptr = processPath)
                    {
                        int argc = args.Length;
                        result = StartBackendAsPrivileged(argc, argv, processPath.Length, processPath_ptr, processIdLocal, nativeErrorCodeLocal);
                    }
                }
                return (ErrorCode)result;
            }
        }
    }

    [LibraryImport(libraryName, EntryPoint = "aigioltemplate4")]
    private static unsafe partial int PostMessage_Import(int taskId, int commandName_len, char* commandName_ptr, int reqMessage_len, byte* reqMessage_ptr, delegate* unmanaged<int, int, byte*, void> rspMessage_callback);

    static unsafe int PostMessage(int taskId, int commandName_len, char* commandName_ptr, int reqMessage_len, byte* reqMessage_ptr, delegate* unmanaged<int, int, byte*, void> rspMessage_callback)
    {
        if (UseAotDll)
        {
            return PostMessage_Import(taskId, commandName_len, commandName_ptr, reqMessage_len, reqMessage_ptr, rspMessage_callback);
        }
        else
        {
            delegate* unmanaged<int, int, char*, int, byte*, delegate* unmanaged<int, int, byte*, void>, int> ptr = &DllExport.PostMessage;
            return ptr(taskId, commandName_len, commandName_ptr, reqMessage_len, reqMessage_ptr, rspMessage_callback);
        }
    }

    static unsafe ErrorCode PostMessage(int taskId, string commandName, byte[]? reqMessage)
    {
        GCHandle? handle = null;
        int reqMessage_len = default;
        byte* reqMessage_ptr = default;
        if (reqMessage != null && reqMessage.Length != 0)
        {
            handle = GCHandle.Alloc(reqMessage, GCHandleType.Pinned);
            reqMessage_ptr = (byte*)handle.Value.AddrOfPinnedObject();
            reqMessage_len = reqMessage.Length;
        }
        try
        {
            fixed (char* commandName_ptr = commandName)
            {
                int commandName_len = commandName.Length;
                delegate* unmanaged<int, int, byte*, void> rspMessage_delegate = &OnRspMessageReceived;
                var result = PostMessage(taskId, commandName_len, commandName_ptr, reqMessage_len, reqMessage_ptr, rspMessage_delegate);
                return (ErrorCode)result;
            }
        }
        finally
        {
            if (handle.HasValue)
            {
                handle.Value.Free();
            }
        }
    }

    static readonly Dictionary<int, TaskCompletionSource<(int taskId, byte[] rspMessage, int rspMessage_len)>> tcsOnRspMessageReceived = new();

    static async Task<(int taskId, ErrorCode code, byte[] rspMessage, int rspMessage_len)> PostMessageCoreAsync(int taskId, string commandName, byte[]? reqMessage)
    {
        var tcs = new TaskCompletionSource<(int taskId, byte[] rspMessage, int rspMessage_len)>();
        tcsOnRspMessageReceived.Add(taskId, tcs);
        var errCode = PostMessage(taskId, commandName, reqMessage);
        var result = await tcs.Task;
        return (result.taskId, errCode, result.rspMessage, result.rspMessage_len);
    }

    [UnmanagedCallersOnly]
    static unsafe void OnRspMessageReceived(int taskId, int rspMessage_len, byte* rspMessage_ptr)
    {
        if (tcsOnRspMessageReceived.Remove(taskId, out var tcs))
        {
            var rspMessage = ArrayPool<byte>.Shared.Rent(rspMessage_len);
            new ReadOnlySpan<byte>(rspMessage_ptr, rspMessage_len).CopyTo(rspMessage);
            tcs.TrySetResult((taskId, rspMessage, rspMessage_len));
        }
    }

    public enum ErrorCode
    {
        Success = HttpStatusCode.OK,
        Timeout = HttpStatusCode.RequestTimeout,
        Exception = HttpStatusCode.InternalServerError,

        Win32Exception = 5001,
        InvalidOperationException = 5002,
        ObjectDisposedException = 5003,
        ArgumentNullException = 5004,
        IOException = 5005,
        EndOfStreamException = 5006,

        ProcessIsNull = 7001,
        PipeStreamIsNull = 7002,
        CommandNameIsNullOrEmpty = 7003,
        ProcessPathIsNull = 7004,

        IpcConnectTimeout = 8001,
    }
}

public class JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public string Version { get; set; } = "2.0";
}

public sealed class JsonRpcRequest : JsonRpcMessage
{
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("params")]
    public object? Arguments { get; set; }

    [JsonPropertyName("id")]
    public int RequestId { get; set; }
}

[JsonSerializable(typeof(有请求无响应示例请求))]
[JsonSerializable(typeof(有请求有响应示例请求))]
[JsonSerializable(typeof(JsonRpcRequest))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(nil))]
[JsonSerializable(typeof(nil?))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true)]
sealed partial class DefaultJsonSerializerContext_ : JsonSerializerContext
{
    static DefaultJsonSerializerContext_()
    {
        JsonSerializerOptions o = new(JsonSerializerOptions.Web) // 这里用了弱类型 object，需要反射实现，不支持 AOT
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,

            #region JsonSerializerDefaults.Web https://github.com/dotnet/runtime/blob/v9.0.7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L172-L174

            PropertyNameCaseInsensitive = true, // 忽略大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // 允许从字符串读取数字

            #endregion

            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // 忽略 null 值

        };
        Default = new DefaultJsonSerializerContext_(o);
    }
}

sealed partial record 有请求无响应示例请求(long Id, string Name, int Count, decimal Money);

sealed partial record 有请求有响应示例请求(Guid Id, string Name, float Count, double Money);

#pragma warning disable CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
public readonly partial struct nil
#pragma warning restore CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
{
}
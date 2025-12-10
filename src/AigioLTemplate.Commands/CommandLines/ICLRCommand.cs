using AigioL.Common.Models;
using AigioLTemplate.Commands.CommandLines.Abstractions;
using AigioLTemplate.Constants;
using AigioLTemplate.Models;
using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Windows.Win32;
using static AigioLTemplate.Commands.CommandLines.C40b122bb;
using static AigioLTemplate.Hosting.AigioLTemplateHost;
using ApiRspCode = AigioLTemplate.Models.ApiRspCode;

namespace AigioLTemplate.Commands.CommandLines;

/// <summary>
/// 加载 CLR 程序集并执行函数
/// </summary>
interface ICLRCommand : IConsoleCommand
{
    const string commandName = "rlc";
    const string optionName_pwzArgument = "-argv";

    static Command IConsoleCommand.GetCommand()
    {
        var pwzArgument = new Option<string>(optionName_pwzArgument);
        var command = new Command(commandName)
        {
            pwzArgument,
        };
        command.SetAction(parseResult => Handler(
            parseResult.GetValue(pwzArgument)
        ));
        return command;
    }

    private static unsafe ExitCodeStruct Handler(
        string? pwzArgument)
    {
        var pwzAssemblyPath = GetAssemblyPath();

        if (string.IsNullOrWhiteSpace(pwzAssemblyPath)
            || string.IsNullOrWhiteSpace(pwzArgument))
        {
            return ExitCode.Failure_BadArguments;
        }
        var valid = AssemblyInfo.ValidateAssembly(pwzAssemblyPath);
        if (!valid)
        {
            return ExitCode.Failure;
        }
        var exitCode = PInvoke.RunDotNetFxDllMain(
            pwzAssemblyPath,
            pwzTypeName,
            pwzMethodName,
            pwzArgument);
        return unchecked((int)exitCode);
    }

    /// <summary>
    /// 加载程序集项目为 AigioLTemplate.DotNetFx，并调用其 Program.ComponentEntryPoint 方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static ApiRsp<T?> RunDotNetFxDllMain<T>(string[] args) where T : notnull
    {
        //if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 16299))
        //{
        //    // TODO: 检查 .NET Framework 4.7.1 是否安装
        //    // https://learn.microsoft.com/zh-cn/dotnet/framework/install/versions-and-dependencies#net-framework-471
        //}

        Process? process = null;
        try
        {
            var argv = JsonSerializer.Serialize(args, DefaultJsonSerializerContext_.Default.StringArray);
            ProcessStartInfo psi = new()
            {
                FileName = Environment.ProcessPath,
                Arguments = $"{ProgramArgsConstants.clt_} {commandName} {optionName_pwzArgument} {argv}",
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
            };
            process = Process.Start(psi);
            ArgumentNullException.ThrowIfNull(process);

            var strStandardOutput = process.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(strStandardOutput))
            {
                var result = JsonSerializer.Deserialize(
                    strStandardOutput,
                    (JsonTypeInfo<ApiRsp<T?>>)DefaultJsonSerializerContext_.Default.GetTypeInfo(typeof(ApiRsp<T?>))!);
                if (result != null)
                {
                    return result;
                }
            }

            var exitCode = (ExitCode)process.ExitCode;
            switch (exitCode)
            {
                case ExitCode.Ok:
                    return ApiRspCode.OK;
                case ExitCode.Exception:
                    return ApiRspCode.Exception;
                case ExitCode.Failure_NotFound:
                    return ApiRspCode.NotFound;
                case ExitCode.Elevated:
                case ExitCode.Failure:
                case ExitCode.Failure_Mutex:
                case ExitCode.Failure_IncompatibleOS:
                case ExitCode.Failure_UserInteractiveFalse:
                case ExitCode.Failure_StartUnauthorizedAccess:
                case ExitCode.Failure_BadArguments:
                    return ApiRspCode.BadRequest;
                default:
#pragma warning disable CA2208 // 正确实例化参数异常
                    throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, null);
#pragma warning restore CA2208 // 正确实例化参数异常
            }
        }
        catch (Exception ex)
        {
            return ex;
        }
        finally
        {
            if (process != null)
            {
                if (!process.WaitForExit(TimeSpan.FromSeconds(waitTimeoutSeconds)))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                    }
                }
                process.Dispose();
            }
        }

    }
}

file static class C40b122bb
{
    const string dllFileName = "aigioltemplate_86e16c4e.dll";

    // src\AigioLTemplate.DotNetFx\Program.cs
    internal const string pwzTypeName = "AigioLTemplate.Program";
    internal const string pwzMethodName = "ComponentEntryPoint";

    internal const int waitTimeoutSeconds = 29;

    /// <summary>
    /// 获取 .NET Framework 程序集路径
    /// </summary>
    /// <returns></returns>
    internal static string GetAssemblyPath()
    {
        var processPath = Environment.ProcessPath;
        ArgumentNullException.ThrowIfNull(processPath);
        var result = Path.GetFullPath(Path.Combine(processPath, "..", dllFileName));
#if DEBUG
        if (!File.Exists(result))
        {
            var result2 = GetAssemblyPathByProjPath();
            if (result2 != null)
            {
                return result2;
            }
        }
#endif
        return result;
    }

#if DEBUG
    static string? GetAssemblyPathByProjPath()
    {
        var path = Path.Combine(ProjPath, @"src\artifacts\bin\AigioLTemplate.DotNetFx");
        var query = from m in Directory.EnumerateDirectories(path, "debug_net4*")
                    let dir = new DirectoryInfo(m)
                    orderby dir.LastWriteTime descending
                    select dir;
        foreach (var it in query)
        {
            var dllPath = Path.Combine(it.FullName, dllFileName);
            if (File.Exists(dllPath))
            {
                return dllPath;
            }
        }
        return null;
    }
#endif
}
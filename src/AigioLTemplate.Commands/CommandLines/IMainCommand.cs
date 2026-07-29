using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using AigioLTemplate.Commands.CommandLines.Abstractions;
using AigioLTemplate.Constants;
using AigioLTemplate.Hosting;
using AigioLTemplate.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using static AigioLTemplate.Hosting.AigioLTemplateHost;

namespace AigioLTemplate.Commands.CommandLines;

/// <summary>
/// 主要启动应用程序命令
/// <item>-clt main</item>
/// </summary>
interface IMainCommand : IConsoleCommand
{
    const string commandName = ProgramArgsConstants.command_main;

    static Option<bool> GetOptEnablePipeJsonRpc() => new("-n");

    static Option<string> GetOptBaseUrlApi() => new("-base_url_api");

    static Option<string> GetOptBaseUrlOw() => new("-base_url_ow");

    static Command IConsoleCommand.GetCommand()
    {
        var base_url_api = GetOptBaseUrlApi();
        var base_url_ow = GetOptBaseUrlOw();
        var enablePipeJsonRpc = GetOptEnablePipeJsonRpc();
        var command = new Command(commandName)
        {
            base_url_api,
            base_url_ow,
            enablePipeJsonRpc,
        };
        command.SetAction(parseResult => Handler(
            parseResult.GetValue(base_url_api),
            parseResult.GetValue(base_url_ow),
            parseResult.GetValue(enablePipeJsonRpc)
        ));
        return command;
    }

    static ExitCodeStruct Handler(
        string? base_url_api,
        string? base_url_ow,
        bool enablePipeJsonRpc)
    {
#if USE_JSON_RPC
#if DEBUG
        enablePipeJsonRpc = true;
#endif
        if (enablePipeJsonRpc)
        {
            V.PipeNameJsonRpc = $"aigioltemplate_{Environment.ProcessId}_";
        }
#endif

        // 设置 HttpClient.BaseAddress
        if (base_url_api != null)
        {
            UrlConstants_.ApiBaseUrl = base_url_api;
        }
        if (base_url_ow != null)
        {
            UrlConstants_.OfficialWebsite = base_url_ow;
        }

        ExitCodeStruct exitCode;

        V.IsMainProcess = true;

        // 配置依赖注入服务
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);

        CommandHelpers.ConfigureCommands();

#if USE_ASPNETCORE
        unsafe
        {
            var func = FunctionPointerRunWebApp;
            if (func != null)
            {
                func();
                FunctionPointerRunWebApp = null;
            }
        }
#endif

        AigioLTemplateHost.Run();

#if USE_ASPNETCORE
        unsafe
        {
            var func = FunctionPointerShutdownWebApp;
            if (func != null)
            {
                func();
                FunctionPointerShutdownWebApp = null;
            }
        }
#endif

        exitCode = ExitCode.Ok;
        return exitCode;
    }

#if USE_ASPNETCORE
    internal unsafe static delegate* managed<void> FunctionPointerRunWebApp;
    internal unsafe static delegate* managed<void> FunctionPointerShutdownWebApp;
#endif

    internal static void ConfigureServices(IServiceCollection services)
    {
        // 添加日志服务
        services.Add(ServiceDescriptor.Singleton(Log.Factory));
        services.AddLogging();

        services.AddSingleton(Options.Create(AppSecrets.Instance));

        // 添加 Essential 服务
        services.AddEssential();

        // 添加 WebApi 服务
        services.AddServerSdkWebApiService<AppSecrets, UserInfoModel>();
    }
}

file static partial class I1c5cf9e8
{
    static string? GetFileVersionByAssemblyAttribute(Assembly? assembly = null)
    {
        assembly ??= typeof(I1c5cf9e8).Assembly;
        var v = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        return v;
    }

    static string? GetFileVersionByFileVersionInfo(string? processPath = null)
    {
        processPath ??= Environment.ProcessPath;
        if (processPath != null)
        {
            var v = FileVersionInfo.GetVersionInfo(processPath).FileVersion;
            return v;
        }
        return null;
    }

    static readonly Lazy<string> lazyFileVersion = new(() =>
    {
        try
        {
            var v = GetFileVersionByAssemblyAttribute();
            if (v != null)
            {
                return v;
            }
        }
        catch
        {
        }
        try
        {
            var v = GetFileVersionByFileVersionInfo();
            if (v != null)
            {
                return v;
            }
        }
        catch
        {
        }
        throw new ApplicationException("Failed to retrieve the application file version number.");
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static string FileVersion => lazyFileVersion.Value;
}
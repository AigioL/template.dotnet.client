using AigioLTemplate.Commands.CommandLines.Abstractions;
using AigioLTemplate.Hosting;
using System.CommandLine;
using System.Management.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static AigioLTemplate.Hosting.AigioLTemplateHost;

namespace AigioLTemplate.Commands.CommandLines;

/// <summary>
/// 显示设备信息命令
/// <para>-clt info</para>
/// </summary>
interface IInfoCommand : IConsoleCommand
{
    const string commandName = "info";

    static Command IConsoleCommand.GetCommand()
    {
        var command = new Command(commandName)
        {
        };
        command.SetAction(parseResult => Handler());
        return command;
    }

    static ExitCodeStruct Handler()
    {
        using ConsoleDisposable consoleDisposable = new();

        StringBuilder b = new();
        ShowInfo(b);
        Console.Write(b);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        return ExitCode.Ok;
    }

    internal static void ShowInfo(StringBuilder b)
    {
        b.Append("BaseDirectory: ");
        b.AppendLine(AppContext.BaseDirectory);

        b.Append("CurrentDirectory: ");
        b.AppendLine(Environment.CurrentDirectory);

        b.Append("AppDataDirectory: ");
        b.AppendLine(IOPath.AppDataDirectory);

        b.Append("CacheDirectory: ");
        b.AppendLine(IOPath.CacheDirectory);

        b.Append("OSArchitecture: ");
        b.AppendLineEx(RuntimeInformation.OSArchitecture);

        b.Append("ProcessArchitecture: ");
        b.AppendLineEx(RuntimeInformation.ProcessArchitecture);

        b.Append("ProcessId: ");
        b.AppendLineEx(Environment.ProcessId);

        b.Append("ProcessorCount: ");
        b.AppendLineEx(Environment.ProcessorCount);

        b.Append("CurrentManagedThreadId: ");
        b.AppendLineEx(Environment.CurrentManagedThreadId);

        b.Append("RuntimeVersion: ");
        b.AppendLineEx(Environment.Version);

        b.Append("OSVersion: ");
        b.AppendLineEx(Environment.OSVersion.Version);

        b.Append("OSVersionString: ");
        b.AppendLineEx(Environment.OSVersion.VersionString);

        b.Append("UserInteractive: ");
        b.AppendLineEx(Environment.UserInteractive);

        b.Append("MachineName: ");
        b.AppendLine(Environment.MachineName);

        b.Append("UserName: ");
        b.AppendLine(Environment.UserName);

        b.Append("UserDomainName: ");
        b.AppendLine(Environment.UserDomainName);

        b.Append("IsPrivilegedProcess: ");
        b.AppendLineEx(Environment.IsPrivilegedProcess);

        b.Append("Is64BitOperatingSystem: ");
        b.AppendLineEx(Environment.Is64BitOperatingSystem);

        b.Append("Is64BitProcess: ");
        b.AppendLineEx(Environment.Is64BitProcess);

        b.Append("SystemPageSize: ");
        b.AppendLineEx(Environment.SystemPageSize);

        b.Append("IsDynamicCodeCompiled: ");
        b.AppendLineEx(RuntimeFeature.IsDynamicCodeCompiled);

        b.Append("IsDynamicCodeSupported: ");
        b.AppendLineEx(RuntimeFeature.IsDynamicCodeSupported);

        b.Append("MacAddressHash: ");
        b.AppendLineEx(NetAdapterHelper.GetMacAddressHash());
    }
}

file static class _4e9d1ea8
{
    internal static StringBuilder AppendLineEx<T>(this StringBuilder b, T? value) where T : notnull
    {
        if (value is null)
        {
            return b.AppendLine();
        }
        else if (value is bool b1)
        {
            return b.AppendLine(b1 ? "true" : "false");
        }
        else
        {
            return b.AppendLine(value.ToString());
        }
    }
}
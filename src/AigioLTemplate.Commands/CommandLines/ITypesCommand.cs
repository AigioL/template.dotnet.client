using AigioLTemplate.Commands.CommandLines.Abstractions;
using System.CommandLine;
using System.Reflection;
using System.Text;
using static AigioLTemplate.Hosting.AigioLTemplateHost;

namespace AigioLTemplate.Commands.CommandLines;

/// <summary>
/// 打印所有类型命令
/// <para>-clt types</para>
/// </summary>
partial interface ITypesCommand
{
    static void WriteUTF8BOM(Stream stream)
    {
        // 写入 UTF-8 BOM 头
        stream.WriteByte(0xEF);
        stream.WriteByte(0xBB);
        stream.WriteByte(0xBF);
    }

    internal static async Task HandlerCoreAsync(Stream assemblies_stream, Stream types_stream, Stream pipe_stream, CancellationToken cancellationToken = default)
    {
        try
        {
            WriteUTF8BOM(assemblies_stream);
            WriteUTF8BOM(types_stream);
            WriteUTF8BOM(pipe_stream);

            var assemblies_ = AppDomain.CurrentDomain.GetAssemblies();
            HashSet<Assembly> assemblies = [.. assemblies_];
            AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
            {
                var assembly = args.LoadedAssembly;
                if (assemblies.Add(assembly))
                {
                    ShowAssembly(assembly);
                }
            };
            foreach (var assembly in assemblies_)
            {
                ShowAssembly(assembly);
            }

            void ShowAssembly(Assembly assembly)
            {
                lock (types_stream)
                {
                    var assemblyName = assembly.FullName;
                    if (!string.IsNullOrWhiteSpace(assemblyName))
                    {
                        assemblies_stream.Write(Encoding.UTF8.GetBytes(assemblyName));
                        assemblies_stream.Write("\r\n"u8);
                    }
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    var types = assembly.GetTypes();
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    foreach (var type in types)
                    {
                        var fullName = type.FullName;
                        if (!string.IsNullOrWhiteSpace(fullName))
                        {
                            types_stream.Write(Encoding.UTF8.GetBytes(fullName));
                            types_stream.Write("\r\n"u8);
                        }
                    }
                }
            }

            var pipes = Directory.GetFiles(@"\\.\pipe\");
            if (pipes != null)
            {
                var strC01 = Encoding.UTF8.GetString("Support_"u8);
                var strC02 = Encoding.UTF8.GetString("Trainer_"u8);
                var g = pipes.GroupBy(x => (x.Contains(strC01) || x.Contains(strC02)) ? 1u : 2u)
                    .OrderBy(static x => x.Key).ToArray();
                StreamWriter w = new(pipe_stream, Encoding.UTF8);
                foreach (var it in g)
                {
                    foreach (var pipeUrl in it)
                    {
                        w.WriteLine(pipeUrl);
                    }
                    w.WriteLine("--------------------------------------------------------------------------------");
                }
            }
        }
        finally
        {
            await FlushAsync(types_stream, cancellationToken);
            await FlushAsync(assemblies_stream, cancellationToken);
            await FlushAsync(pipe_stream, cancellationToken);
        }
    }

    static Task FlushAsync(Stream s, CancellationToken cancellationToken = default)
    {
        s.SetLength(s.Position);
        return s.FlushAsync(cancellationToken);
    }
}

#if DEBUG
partial interface ITypesCommand : IConsoleCommand
{
    const string commandName = "types";

    static Command IConsoleCommand.GetCommand()
    {
        var command = new Command(commandName)
        {
        };
        command.SetAction(parseResult => Handler());
        return command;
    }

    private static async Task<int> Handler()
    {
        ExitCodeStruct exitCode = -1;

        using var assemblies_stream = new FileStream(Path.Combine(
            AppContext.BaseDirectory, "assemblies.txt"),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);
        using var types_stream = new FileStream(Path.Combine(
            AppContext.BaseDirectory, "types.txt"),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);
        using var pipe_stream = new FileStream(Path.Combine(
            AppContext.BaseDirectory, "pipes.txt"),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);
        try
        {
            await HandlerCoreAsync(assemblies_stream, types_stream, pipe_stream);
            return exitCode = ExitCode.Ok;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return exitCode = ExitCode.Exception;
        }
        finally
        {
            await types_stream.DisposeAsync();
            await assemblies_stream.DisposeAsync();
            Console.WriteLine($"ExitCode: {exitCode}");
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
#endif
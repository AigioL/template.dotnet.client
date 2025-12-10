using System.CommandLine;
using System.Runtime.CompilerServices;

namespace AigioLTemplate.Commands.CommandLines.Abstractions;

/// <summary>
/// 控制台业务命令接口定义
/// </summary>
interface IConsoleCommand
{
    /// <summary>
    /// 由业务实现的自定义命令
    /// </summary>
    /// <returns></returns>
    internal static abstract Command GetCommand();
}

static partial class ConsoleCommandExtensions
{
    /// <summary>
    /// 将业务命令添加到 <see cref="RootCommand"/>
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="rootCommand"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddCommand<TCommand>(this RootCommand rootCommand)
        where TCommand : IConsoleCommand
    {
        var command = TCommand.GetCommand();
        rootCommand.Subcommands.Add(command);
    }
}
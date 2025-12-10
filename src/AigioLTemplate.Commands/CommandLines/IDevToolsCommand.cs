using AigioLTemplate.Commands.CommandLines.Abstractions;
using System.CommandLine;
using static AigioLTemplate.Hosting.AigioLTemplateHost;

namespace AigioLTemplate.Commands.CommandLines;

/// <summary>
/// 开发工具命令
/// <list type="bullet">
/// <item>-clt devtools</item>
/// </list>
/// </summary>
interface IDevToolsCommand : IConsoleCommand
{
    const string commandName = "devtools";

    static Command IConsoleCommand.GetCommand()
    {
        var base_url_api = IMainCommand.GetOptBaseUrlApi();
        var base_url_ow = IMainCommand.GetOptBaseUrlOw();
        var enablePipeJsonRpc = IMainCommand.GetOptEnablePipeJsonRpc();
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
        var exitCode = IMainCommand.Handler(
            base_url_api,
            base_url_ow,
            enablePipeJsonRpc);
        return exitCode;
    }
}

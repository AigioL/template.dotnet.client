using AigioLTemplate.Commands.Features.Samples;
using AigioLTemplate.Hosting;

namespace AigioLTemplate;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            // 无参直接退出
            return 0;
        }

        //args = ["-clt", "info"];

        if (args.Length >= 2 && string.Equals("-clt", args[0], StringComparison.InvariantCultureIgnoreCase)
            && string.Equals("demo", args[1], StringComparison.InvariantCultureIgnoreCase))
        {
            // 初始化日志
            LogInit.InitLog(sourceName: AssemblyInfo.Trademark);

            // TODO: demo 模式下的逻辑

            return 0;
        }

        var exitCode = AigioLTemplateHost.Start(args);
        return exitCode;
    }
}
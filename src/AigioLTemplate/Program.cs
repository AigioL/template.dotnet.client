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

        var exitCode = AigioLTemplateHost.Start(args);
        return exitCode;
    }
}
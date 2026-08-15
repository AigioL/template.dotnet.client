using System.Diagnostics;

namespace AigioLTemplate.UnitTest;

public sealed class Async2Test
{
    [Fact]
    public async Task StackTraceTest()
    {
        // https://learn.microsoft.com/zh-cn/dotnet/core/whats-new/dotnet-11/runtime#cleaner-live-stack-traces
        await OuterAsync();
    }

    static async Task OuterAsync()
    {
        await Task.CompletedTask;
        await MiddleAsync();
    }

    static async Task MiddleAsync()
    {
        await Task.CompletedTask;
        await InnerAsync();
    }

    static async Task InnerAsync()
    {
        await Task.CompletedTask;
        Console.WriteLine(new StackTrace(fNeedFileInfo: true));
    }
}

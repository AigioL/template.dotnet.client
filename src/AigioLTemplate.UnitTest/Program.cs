using AigioL.Common.Models;
using AigioLTemplate.Models;

namespace AigioLTemplate.UnitTest;

partial class Program
{
    internal partial int Main(string[] args)
    {
        // 初始化日志
        LogInit.InitLog("AigioLTemplate.UnitTest");

        // 初始化 GetCodeByExceptionDelegate
        ApiRspExtensions.GetCodeByExceptionDelegate = ApiRspCodeExtensions.GetCodeByException;

        return 0;
    }
}

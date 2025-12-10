using System.Diagnostics;

namespace AigioLTemplate.Models.Exceptions.Abstractions;

abstract class ApiRspCodeException : global::AigioL.Common.Models.Abstractions.ApiRspCodeException
{
    public abstract ApiRspCode Code { get; }

    public sealed override uint GetCode() => unchecked((uint)Code);

    public ApiRspCodeException(string? message) : base(message)
    {
    }

    public ApiRspCodeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public static string? TryGetFileName(string exePath)
    {
        try
        {
            return Path.GetFileName(exePath);
        }
        catch
        {
        }
        return exePath;
    }

    public static int? TryGetExitCode(Process process)
    {
        try
        {
            return process.ExitCode;
        }
        catch
        {
            // 无法检索进程的退出代码。
            return null;
        }
    }
}

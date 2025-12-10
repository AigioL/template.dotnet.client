using AigioLTemplate.Constants;

namespace System.IO;

static partial class IOPath
{
    /// <summary>
    /// 获取应用程序数据的位置
    /// </summary>
    public static string AppDataDirectory
    {
        get
        {
            if (field == null)
            {
                var appDataDirectory = _c914ceb2.GetAppDataDirectory();
                ArgumentNullException.ThrowIfNull(appDataDirectory);
                field = appDataDirectory;
            }
            return field;
        }
    }

    public static string GetAigioLTemplateLibFilePath(string? moduleFileName)
    {
        var libFilePath = moduleFileName == null ?
            Path.Combine(AppDataDirectory, "bin", "31d6dca4") :
            Path.Combine(AppDataDirectory, "bin", "31d6dca4", moduleFileName);
        return libFilePath;
    }

    /// <summary>
    /// 获取缓存数据的位置
    /// </summary>
    public static string CacheDirectory
    {
        get
        {
            if (field == null)
            {
                var cacheDirectory = _c914ceb2.GetCacheDirectory();
                ArgumentNullException.ThrowIfNull(cacheDirectory);
                field = cacheDirectory;
            }
            return field;
        }
    }
}

file static class _c914ceb2
{
    internal static string GetAppDataDirectory()
    {
        var value = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(value, UrlConstants.HARDCODED_APP_NAME__PATH);
    }

    internal static string GetCacheDirectory()
    {
        var value = GetTempPath();
        return Path.Combine(value, UrlConstants.HARDCODED_APP_NAME__PATH);
    }

    static string GetTempPathByUserName(string? windowsPathRoot, string userName)
    {
        ArgumentNullException.ThrowIfNull(windowsPathRoot);
        var result = Path.Combine(windowsPathRoot,
            "Users",
            userName,
            "AppData",
            "Local",
            "Temp");
        return result;
    }

    /// <inheritdoc cref="Path.GetTempPath"/>
    static string GetTempPath()
    {
        var result = Path.GetTempPath();
#if WINDOWS || NETFRAMEWORK
        var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (result.StartsWith(windowsPath, StringComparison.OrdinalIgnoreCase))
        {
            // Win 上在某些情况下返回 C:\Windows\Temp，然后没有权限写入抛出异常
            // 疑似环境变量被修改
            var windowsPathRoot = Path.GetPathRoot(windowsPath);
            result = GetTempPathByUserName(windowsPathRoot, Environment.UserName);
        }
#endif
        return result;
    }
}
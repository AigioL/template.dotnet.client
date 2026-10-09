using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AigioLTemplate;

static unsafe partial class DllExport
{
    /// <summary>
    /// 获取特殊路径的值长度
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate0")]
    public static int GetSpecialPathLength(int type)
    {
        try
        {
            MethodStartLog();
            var r = GetSpecialPathLengthCore(type);
            return r;
        }
        catch (Exception ex)
        {
            MethodExceptionLog(ex);
            throw;
        }
        finally
        {
            MethodEndLog();
        }
    }

    static int GetSpecialPathLengthCore(int type) => type switch
    {
        1 => IOPath.AppDataDirectory.Length,
        2 => IOPath.CacheDirectory.Length,
        3 => IOPath.GetAigioLTemplateLibFilePath(null).Length,
        _ => 0,
    };

    /// <summary>
    /// 先获取长度，创建数组后调用此函数写入特殊路径的值，返回是否写入字符数据成功
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value_len"></param>
    /// <param name="value_ptr"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "aigioltemplate1")]
    public static int GetSpecialPath(int type, int value_len, [NotNull][In] char* value_ptr)
    {
        try
        {
            MethodStartLog();
            var r = GetSpecialPathCore(type, value_len, value_ptr);
            return r.ToInt32();
        }
        catch (Exception ex)
        {
            MethodExceptionLog(ex);
            throw;
        }
        finally
        {
            MethodEndLog();
        }
    }

    static bool GetSpecialPathCore(int type, int value_len, char* value_ptr)
    {
        if (value_len <= 0 || value_ptr == default)
        {
            // 禁止传入空指针
            return false;
        }
        var value = type switch
        {
            1 => IOPath.AppDataDirectory,
            2 => IOPath.CacheDirectory,
            3 => IOPath.GetAigioLTemplateLibFilePath(null),
            _ => null,
        };

        if (value != null && value_len != 0 && value_len == value.Length)
        {
            Span<char> span = new(value_ptr, value_len);
            value.AsSpan().CopyTo(span);
            return true;
        }
        return false;
    }
}


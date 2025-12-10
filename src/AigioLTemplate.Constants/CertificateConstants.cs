using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace AigioLTemplate.Constants;

/// <summary>
/// 证书常量
/// </summary>
public static partial class CertificateConstants
{
    /// <summary>
    /// X509 证书的可分辨名称字符串，<see cref="X500DistinguishedName"/>
    /// </summary>
    internal const string X500DistinguishedNameValue = $"C=CN, O=TODO, OU=技术部, CN=TODO"; // CRC32 占位符，需要替换为代码签名证书的值

    /// <summary>
    /// 将 <see cref="X509Certificate.Subject"/> 字符串使用逗号与等于符号分割成字典
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    static Dictionary<string, string> SplitSubject(string subject)
    {
        Dictionary<string, string> dict = new(StringComparer.InvariantCultureIgnoreCase);
        // 使用 Span 的 Trim 与 Split 方法避免了字符串分割时的内存分配，以提高性能
        var span_l = subject.AsSpan().Trim();
        var split_l = span_l.Split(',');
        while (split_l.MoveNext())
        {
            var it_r = split_l.Current;
            var it = span_l[it_r].Trim();
            var split_it_l = it.Split('=');

            // 固定 Key=Value 格式
            if (!split_it_l.MoveNext())
            {
                continue;
            }
            string k = new(it[split_it_l.Current].Trim());
            if (!split_it_l.MoveNext())
            {
                continue;
            }
            string v = new(it[split_it_l.Current].Trim());

            if (!dict.TryAdd(k, v))
            {
                dict[k] = v;
            }
        }
        return dict;
    }

    /// <summary>
    /// 验证证书的可分辨名称是否符合预期
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    internal static bool IsValidSubject(string subject)
    {
        var constDict = SplitSubject(X500DistinguishedNameValue);
        var inputDict = SplitSubject(subject);
        foreach (var it in constDict)
        {
            if (inputDict.TryGetValue(it.Key, out var value))
            {
                if (!string.Equals(value, it.Value, StringComparison.InvariantCulture))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }
}
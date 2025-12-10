using WmiLight;

namespace AigioLTemplate;

/// <summary>
/// <see cref="WmiObject"/> 的扩展函数
/// </summary>
public static partial class WmiObjectExtensions
{
    /// <inheritdoc cref="WmiObject.GetPropertyValue(string)"/>
    public static object? TryGetValue(this WmiObject o, string key)
    {
        try
        {
            var result = o.GetPropertyValue(key);
            return result;
        }
        catch
        {
            // WmiLight key 不存在时抛出 COM 异常 0x80041002
            return null;
        }
    }
}

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

public static partial class BooleanExtensions
{
    public static int ToInt32(this bool b) => b ? 1 : 0; // 将 bool 类型转换为 int 类型，true 转换为 1，false 转换为 0，避免 UnmanagedCallersOnly 不支持 bool 类型参数
}

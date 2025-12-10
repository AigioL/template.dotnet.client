using AigioLTemplate.Constants;
using System.Text.Json.Serialization;

namespace AigioLTemplate.Models.UI;

/// <summary>
/// UI 窗口属性变更参数模型类
/// <para>使用 JSON 序列化派生类（多态），在 JSON 源生成中仅需要标注基类</para>
/// </summary>
#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = IJsonSerializerContext.TypeDiscriminatorPropertyName)] // 自定义类型鉴别器名称
#region 0 = WindowStatePropertyChangedEventArgs = WindowState
[global::MemoryPack.MemoryPackUnion(0, typeof(WindowStatePropertyChangedEventArgs))]
[JsonDerivedType(typeof(WindowStatePropertyChangedEventArgs), typeDiscriminator: "WindowState")]
#endregion
#region 1 = WindowBoundsPropertyChangedEventArgs = WindowBounds || WindowSize
[JsonDerivedType(typeof(WindowBoundsPropertyChangedEventArgs), typeDiscriminator: "WindowSize")]
[global::MemoryPack.MemoryPackUnion(1, typeof(WindowBoundsPropertyChangedEventArgs))]
[JsonDerivedType(typeof(WindowBoundsPropertyChangedEventArgs), typeDiscriminator: "WindowBounds")]
#endregion
#region 2 = WindowMethodPropertyChangedEventArgs = WindowMethod
[global::MemoryPack.MemoryPackUnion(2, typeof(WindowMethodPropertyChangedEventArgs))]
[JsonDerivedType(typeof(WindowMethodPropertyChangedEventArgs), typeDiscriminator: "WindowMethod")]
#endregion
public abstract partial class WindowPropertyChangedEventArgs : IJsonSerializerContext
{
    /// <inheritdoc/>
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => DefaultJsonSerializerContext_.Default;
}

/// <summary>
/// UI 窗口 WindowState 属性变更参数模型类
/// </summary>
#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public sealed partial class WindowStatePropertyChangedEventArgs : WindowPropertyChangedEventArgs
{
    /// <summary>
    /// 指定是最小化、最大化还是还原窗口，忽略大小写，值为 "Minimized"、"Maximized" 或 "Normal"，或者数值 "0"、"1"、"2"
    /// </summary>
    public string? WindowState { get; set; }
}

#if DEBUG
/// <summary>
/// UI 窗口 Size 属性变更参数模型类
/// </summary>
[Obsolete("use WindowBoundsPropertyChangedEventArgs", true)]
public sealed partial class WindowSizePropertyChangedEventArgs : WindowPropertyChangedEventArgs
{
    /// <summary>
    /// 窗口宽度，传递 <see langword="null"/> 时不设置值
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// 窗口高度，传递 <see langword="null"/> 时不设置值
    /// </summary>
    public double? Height { get; set; }
}
#endif

/// <summary>
/// UI 窗口 Bounds 属性变更参数模型类
/// </summary>
#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public sealed partial class WindowBoundsPropertyChangedEventArgs : WindowPropertyChangedEventArgs
{
    /// <summary>
    /// 窗口左侧离屏幕的距离，传递 <see langword="null"/> 时不设置值
    /// </summary>
    public double? X { get; set; }

    /// <summary>
    /// 窗口顶部离屏幕的距离，传递 <see langword="null"/> 时不设置值
    /// </summary>
    public double? Y { get; set; }

    /// <summary>
    /// 窗口宽度，传递 <see langword="null"/> 时不设置值
    /// </summary>
    public double? Width { get; set; }

    /// <summary>
    /// 窗口高度，传递 <see langword="null"/> 时不设置值
    /// </summary>
    public double? Height { get; set; }
}

/// <summary>
/// UI 窗口调用函数，无返回值或返回 <see langword="bool"/> 的函数调用参数模型类
/// </summary>
#if MP2_GENERATE_TS
[global::MemoryPack.GenerateTypeScript]
#endif
[global::MemoryPack.MemoryPackable(SerializerConstants.MP2GenerateType, global::MemoryPack.SerializeLayout.Sequential)]
public sealed partial class WindowMethodPropertyChangedEventArgs : WindowPropertyChangedEventArgs
{
    /// <summary>
    /// 窗口函数名，忽略大小写，例如 "Show"、"Hide"、"Close"、"Activate" 等
    /// </summary>
    public string? Name { get; set; }
}
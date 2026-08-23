using System.Diagnostics;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

/// <summary>
/// 使用 Win32 API 的 BOOL 类型表示布尔值，其中 0 表示 false，非零值表示 true，解决 UnmanagedCallersOnlyAttribute 不支持 C# 的 bool 类型的问题
/// </summary>
[DebuggerDisplay("{Value}")]
public readonly partial struct BOOL : IEquatable<BOOL>
{
    internal readonly int Value;

    internal BOOL(int value) => this.Value = value;

    public static implicit operator int(BOOL value) => value.Value;

    public static explicit operator BOOL(int value) => new BOOL(value);

    public static bool operator ==(BOOL left, BOOL right) => left.Value == right.Value;

    public static bool operator !=(BOOL left, BOOL right) => !(left == right);

    public bool Equals(BOOL other) => this.Value == other.Value;

    public override bool Equals(object? obj) => obj is BOOL other && this.Equals(other);

    public override int GetHashCode() => this.Value.GetHashCode();

    public override string ToString() => $"0x{this.Value:x}";

    internal BOOL(bool value) => this.Value = value ? 1 : 0;

    public static implicit operator bool(BOOL value) => value.Value != 0;

    public static implicit operator BOOL(bool value) => new BOOL(value);
}
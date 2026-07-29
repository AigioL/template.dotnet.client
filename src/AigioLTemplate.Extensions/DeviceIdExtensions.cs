using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Essentials.Storage;
using AigioL.Common.Primitives.Columns;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Win32;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;

namespace AigioLTemplate;

public static partial class DeviceIdExtensions
{
    public static partial void SetDeviceId(this IDeviceId deviceId)
    {
        deviceId.DeviceIdG = DeviceIdHelper.lazy.Value.g;
        deviceId.DeviceIdR = DeviceIdHelper.lazy.Value.r;
        deviceId.DeviceIdN = DeviceIdHelper.lazy.Value.n;
    }
}

file static class DeviceIdHelper
{
    internal static readonly Lazy<(Guid g, string r, string n)> lazy = new(() =>
    {
        var preferences = Ioc.Default.GetRequiredService<IPreferences>();
        var deviceIdG = GetDeviceIdG(preferences);
        var deviceIdR = GetDeviceIdR(preferences);
        var deviceIdN = GetDeviceIdN();
        return (deviceIdG, deviceIdR, deviceIdN);
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    static Guid GetGuid(IPreferences preferences, string key)
    {
        var value = preferences.Get<string>(key, null);
        if (!(!string.IsNullOrWhiteSpace(value) && ShortGuid.TryParse(value, out Guid guid)))
        {
            guid = Guid.NewGuid();
            preferences.Set(key, ShortGuid.Encode(guid));
        }
        return guid;
    }

    static string Get(IPreferences preferences, string key)
    {
        var value = preferences.Get<string>(key, null);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = GenerateRandomString(MaxLengths.DeviceIdR);
            preferences.Set(key, value);
        }
        return value;
    }

    static Guid GetDeviceIdG(IPreferences preferences) => GetGuid(preferences, "KEY_DEVICE_ID_G");

    static string GetDeviceIdR(IPreferences preferences) => Get(preferences, "KEY_DEVICE_ID_R");

    static string GetDeviceIdN(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv)
    {
        Span<byte> bytes = stackalloc byte[key.Length + iv.Length];
        if (key[0] % 2 == 0)
        {
            key.CopyTo(bytes);
            iv.CopyTo(bytes[key.Length..]);
        }
        else
        {
            iv.CopyTo(bytes);
            key.CopyTo(bytes[iv.Length..]);
        }
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(bytes, hash);
        var r = Convert.ToHexString(hash);
        return r;
    }

    static string GetMachineSecretKey()
    {
        using var rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
        if (rk != null)
        {
            var value = rk.GetValue("MachineGuid")?.ToString();
            return value ?? string.Empty;
        }
        return string.Empty;
    }

    static string GetDeviceIdN()
    {
        var msk = GetMachineSecretKey();
        var bytes = Encoding.UTF8.GetBytes(msk);

        Span<byte> key = stackalloc byte[SHA1.HashSizeInBytes + 4];
        Span<byte> iv = stackalloc byte[MD5.HashSizeInBytes];
        SHA1.HashData(bytes, key);
        Crc32.TryHash(bytes, key[SHA1.HashSizeInBytes..], out _);
        MD5.HashData(bytes, iv);

        var r = GetDeviceIdN(key, iv);
        return r;
    }

    /// <summary>
    /// 生成随机字符串，长度为固定传入字符串
    /// </summary>
    /// <param name="length">要生成的字符串长度</param>
    /// <param name="randomChars">随机字符串字符集</param>
    /// <returns></returns>
    static string GenerateRandomString(int length = 6, string randomChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")
    {
        var random = Random.Shared;
        var result = new char[length];
        if (random.Next(256) % 2 == 0)
            for (var i = length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(i);
        else
            for (var i = 0; i < length; i++) // 0 1 2 3 4 5
                EachGenerate(i);
        return new string(result);
        void EachGenerate(int i)
        {
            var index = random.Next(0, randomChars.Length);
            var temp = RandomCharAt(randomChars, index);
            static char RandomCharAt(string s, int index)
            {
                if (index == s.Length) index = 0;
                else if (index > s.Length) index %= s.Length;
                return s[index];
            }
            result[i] = temp;
        }
    }
}
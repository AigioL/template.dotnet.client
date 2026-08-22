namespace AigioLTemplate.UnitTest;

public sealed class AppTest : BaseUnitTest
{
    readonly Dictionary<string, string?> paths = new()
    {
        { "BaseDirectory", AppContext.BaseDirectory },
        { "ProcessPath", Environment.ProcessPath },
        { "CurrentDirectory", Directory.GetCurrentDirectory() },
        { "AppDataDirectory", IOPath.AppDataDirectory },
        { "CacheDirectory", IOPath.CacheDirectory },
    };

    static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    static string ReplaceEnvValue(string value)
    {
        if (value.StartsWith(UserProfile, StringComparison.InvariantCultureIgnoreCase))
        {
            return "%USERPROFILE%" + value[UserProfile.Length..];
        }
        else if (value.StartsWith(ProjPath, StringComparison.InvariantCultureIgnoreCase))
        {
            return "{ProjPath}" + value[ProjPath.Length..];
        }
        return value;
    }

    [Fact]
    public void GetSpecialPath()
    {
        foreach (var it in paths)
        {
            Assert.NotNull(it.Value);
            Console.WriteLine($"{it.Key}: {ReplaceEnvValue(it.Value)}");
        }
    }

    [Fact]
    public void ValidateAssembly()
    {
#pragma warning disable IL3000 // Avoid accessing Assembly file path when publishing as a single file
        var valid = AssemblyInfo.ValidateAssembly(GetType().Assembly.Location!);
#pragma warning restore IL3000 // Avoid accessing Assembly file path when publishing as a single file
        Assert.True(valid);
    }

    [Fact]
    public void ValidateRustApp()
    {
        //var appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TBD", "TBD.exe");
        //if (File.Exists(appPath))
        //{
        //    var valid = AssemblyInfo.ValidateRustApp(appPath);
        //    Assert.True(valid);
        //}
    }
}

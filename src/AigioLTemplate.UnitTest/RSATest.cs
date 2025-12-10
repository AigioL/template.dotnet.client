using System.Buffers.Text;
using System.Security.Cryptography;
using R = AigioLTemplate.Constants.Properties.Resources;

namespace AigioLTemplate.UnitTest;

public sealed class RSATest : BaseUnitTest
{
    [Theory]
    [InlineData(2048)]
    public void Create(int keySize)
    {
        using var rsa = RSA.Create();
        rsa.KeySize = keySize;

        var privateKey = rsa.ExportParameters(true);
        var publicKey = rsa.ExportParameters(false);

#pragma warning disable IL3000 // Avoid accessing Assembly file path when publishing as a single file
        var cacheDirectory = Path.GetFullPath(Path.Combine(typeof(RSATest).Assembly.Location!, "..", "Cache", $"rsa_{keySize}"));
#pragma warning restore IL3000 // Avoid accessing Assembly file path when publishing as a single file

        Dictionary<string, RSAParameters> d = new()
        {
            { nameof(privateKey), privateKey },
            { nameof(publicKey), publicKey },
        };
        foreach (var it in d)
        {
            var filePath = Path.Combine(cacheDirectory, Path.Combine(cacheDirectory, it.Key));
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null)
            {
                Directory.CreateDirectory(dirPath);
            }
            using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            RSAUtils.WriteParameters(fileStream, it.Value);
            fileStream.Flush();
            fileStream.SetLength(fileStream.Position);

            Console.WriteLine(filePath);
        }
    }

    [Fact]
    public void GetPublicKey()
    {
        var publicKey = R.GetPublicKey();
        using var stream = new MemoryStream();
        RSAUtils.WriteParameters(stream, publicKey);
        Console.WriteLine($"PublicKey: {Base64Url.EncodeToString(stream.ToArray())}");
    }
}

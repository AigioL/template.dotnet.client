using AigioLTemplate.ServerSdk.Models.Abstractions;
using System.Security.Cryptography;
using R = AigioLTemplate.Constants.Properties.Resources;

namespace AigioLTemplate.Models;

/// <summary>
/// 应用程序机密项模型类
/// </summary>
public sealed partial record class AppSecrets : IAppSecrets
{
    public required RSA PublicKey { get; init; }
}

partial record class AppSecrets
{
    internal static AppSecrets Instance
    {
        get
        {
            if (field == null)
            {
                var rsa = RSA.Create();
                var rsaParameters = R.GetPublicKey();
                rsa.ImportParameters(rsaParameters);
                return field = new()
                {
                    PublicKey = rsa,
                };
            }
            return field;
        }
    }
}
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Oidc;

public class SigningKeyService
{
    private readonly RsaSecurityKey _signingKey;

    public SigningKeyService()
    {
        var keyPath = Path.Combine("App_Data", "oidc-signing-key.json");
        var directory = Path.GetDirectoryName(keyPath)!;

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        RSA rsa;

        if (File.Exists(keyPath))
        {
            var json = File.ReadAllText(keyPath);
            var keyData = JsonSerializer.Deserialize<RsaKeyData>(json)!;
            rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(keyData.PrivateKey), out _);
        }
        else
        {
            rsa = RSA.Create(2048);
            var keyData = new RsaKeyData
            {
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            };
            var json = JsonSerializer.Serialize(keyData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(keyPath, json);
        }

        _signingKey = new RsaSecurityKey(rsa)
        {
            KeyId = GenerateKeyId(rsa)
        };
    }

    public RsaSecurityKey GetSigningKey()
    {
        return _signingKey;
    }

    private static string GenerateKeyId(RSA rsa)
    {
        var publicKey = rsa.ExportRSAPublicKey();
        var hash = SHA256.HashData(publicKey);
        return Convert.ToBase64String(hash)[..16].Replace("+", "-").Replace("/", "_");
    }

    private class RsaKeyData
    {
        public string PrivateKey { get; set; } = string.Empty;
    }
}
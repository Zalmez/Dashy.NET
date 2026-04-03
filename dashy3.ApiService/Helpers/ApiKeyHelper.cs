using System.Security.Cryptography;
using System.Text;

namespace dashy3.ApiService.Helpers;

public static class ApiKeyHelper
{
    public static (string RawKey, string Hash, string Prefix) GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var encoded = Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var rawKey = $"dk_{encoded}";
        var hash = ComputeHash(rawKey);
        var prefix = rawKey[..Math.Min(11, rawKey.Length)];
        return (rawKey, hash, prefix);
    }

    public static string ComputeHash(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

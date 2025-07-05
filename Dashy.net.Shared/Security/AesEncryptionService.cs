using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Dashy.Net.Shared.Security;

public static class AesEncryptionService
{
    private static readonly byte[] Key = GetKey();
    private static readonly byte[] IV = GetIV();

    private static byte[] GetKey()
    {
        var env = Environment.GetEnvironmentVariable("DASHY_AES_KEY");
        if (!string.IsNullOrEmpty(env) && env.Length == 32)
            return Encoding.UTF8.GetBytes(env);
        throw new InvalidOperationException("DASHY_AES_KEY environment variable must be set to a 32-character string for AES-256 encryption.");
    }
    private static byte[] GetIV()
    {
        var env = Environment.GetEnvironmentVariable("DASHY_AES_IV");
        if (!string.IsNullOrEmpty(env) && env.Length == 16)
            return Encoding.UTF8.GetBytes(env);
        throw new InvalidOperationException("DASHY_AES_IV environment variable must be set to a 16-character string for AES encryption.");
    }

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var buffer = Convert.FromBase64String(cipherText);
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}

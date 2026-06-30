using System.Security.Cryptography;

namespace FinalLabSystem.Infrastructure.Security;

public static class AesEncryptionHelper
{
    private const int KeySize = 32;
    private const int SaltSize = 16;
    private const int IvSize = 16;
    private const int DefaultIterations = 100_000;

    public static byte[] Encrypt(byte[] plaintext, string password)
    {
        if (plaintext == null || plaintext.Length == 0)
            return Array.Empty<byte>();

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] iv = RandomNumberGenerator.GetBytes(IvSize);
        byte[] key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

        byte[] result = new byte[SaltSize + IvSize + ciphertext.Length];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
        Buffer.BlockCopy(ciphertext, 0, result, SaltSize + IvSize, ciphertext.Length);

        return result;
    }

    public static byte[] Decrypt(byte[] cipherData, string password)
    {
        if (cipherData == null || cipherData.Length < SaltSize + IvSize)
            throw new CryptographicException("Invalid cipher data format.");

        byte[] salt = new byte[SaltSize];
        byte[] iv = new byte[IvSize];
        byte[] ciphertext = new byte[cipherData.Length - SaltSize - IvSize];

        Buffer.BlockCopy(cipherData, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(cipherData, SaltSize, iv, 0, IvSize);
        Buffer.BlockCopy(cipherData, SaltSize + IvSize, ciphertext, 0, ciphertext.Length);

        byte[] key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }

    public static byte[] DeriveKey(string password, byte[] salt, int iterations = DefaultIterations)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(KeySize);
    }
}

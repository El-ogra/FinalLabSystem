using System.Security.Cryptography;
using FinalLabSystem.Infrastructure.Security;

namespace FinalLabSystem.Tests.Infrastructure;

public class AesEncryptionHelperTests
{
    [Fact]
    public void Encrypt_Decrypt_Roundtrip_RestoresOriginal()
    {
        var original = System.Text.Encoding.UTF8.GetBytes("test data unicode أبجد");
        var password = "StrongP@ssw0rd!";

        var encrypted = AesEncryptionHelper.Encrypt(original, password);
        var decrypted = AesEncryptionHelper.Decrypt(encrypted, password);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void Encrypt_Twice_SamePassword_ProducesDifferentCiphertext()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("identical data");
        var password = "SamePassword123!";

        var encrypted1 = AesEncryptionHelper.Encrypt(data, password);
        var encrypted2 = AesEncryptionHelper.Encrypt(data, password);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Decrypt_WithWrongPassword_ThrowsCryptographicException()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("sensitive data");
        var encrypted = AesEncryptionHelper.Encrypt(data, "CorrectPassword!");

        Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionHelper.Decrypt(encrypted, "WrongPassword!"));
    }

    [Fact]
    public void Encrypt_EmptyBytes_DoesNotThrow()
    {
        var empty = Array.Empty<byte>();
        var result = AesEncryptionHelper.Encrypt(empty, "password");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void DeriveKey_SamePasswordAndSalt_ProducesSameKey()
    {
        var password = "MySecretKey";
        var salt = new byte[16];
        Array.Fill(salt, (byte)0xAB);

        var key1 = AesEncryptionHelper.DeriveKey(password, salt);
        var key2 = AesEncryptionHelper.DeriveKey(password, salt);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void Decrypt_CorruptedFile_ThrowsCryptographicException()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("original data");
        var encrypted = AesEncryptionHelper.Encrypt(data, "password");

        encrypted[32] ^= 0xFF;

        Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionHelper.Decrypt(encrypted, "password"));
    }

    [Fact]
    public void Decrypt_EmptyFile_ThrowsCryptographicException()
    {
        Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionHelper.Decrypt(Array.Empty<byte>(), "password"));
    }

    [Fact]
    public void Decrypt_TruncatedFile_ThrowsCryptographicException()
    {
        var truncated = new byte[16];

        Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionHelper.Decrypt(truncated, "password"));
    }

    [Fact]
    public void Decrypt_WrongFormatFile_ThrowsCryptographicException()
    {
        var plainText = System.Text.Encoding.UTF8.GetBytes("This is not encrypted data at all");

        Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionHelper.Decrypt(plainText, "password"));
    }

    [Fact]
    public void Decrypt_InvalidSaltLength_ThrowsCryptographicException()
    {
        var invalidData = new byte[8];

        Assert.ThrowsAny<CryptographicException>(() =>
            AesEncryptionHelper.Decrypt(invalidData, "password"));
    }
}

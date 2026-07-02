using System;
using System.Security.Cryptography;
using System.Text;

namespace FinalLabSystem.Infrastructure.Security;

public sealed class OtpGenerator : IOtpGenerator
{
    private static readonly byte[] HmacKey = RandomNumberGenerator.GetBytes(32);

    public string Generate(int digits = 6)
    {
        int max = (int)Math.Pow(10, digits);
        int code = RandomNumberGenerator.GetInt32(0, max);
        return code.ToString($"D{digits}");
    }

    public string Hash(string otp)
    {
        if (string.IsNullOrEmpty(otp))
            throw new ArgumentException("OTP cannot be null or empty.", nameof(otp));

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        using var hmac = new HMACSHA256(HmacKey);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp).Concat(salt).ToArray());
        return $"hmac256${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string otp, string hash)
    {
        if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(hash))
            return false;

        var parts = hash.Split('$');
        if (parts.Length != 3 || parts[0] != "hmac256")
            return false;

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedHash = Convert.FromBase64String(parts[2]);

        using var hmac = new HMACSHA256(HmacKey);
        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp).Concat(salt).ToArray());

        return CryptographicOperations.FixedTimeEquals(expectedHash, computedHash);
    }

    public bool IsExpired(DateTime generatedAt, int expiryMinutes = 10)
    {
        return DateTime.UtcNow - generatedAt > TimeSpan.FromMinutes(expiryMinutes);
    }
}

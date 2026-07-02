using System;
using FinalLabSystem.Infrastructure.Security;
using Xunit;

namespace FinalLabSystem.Tests.Infrastructure;

public class OtpGeneratorTests
{
    private readonly IOtpGenerator _generator = new OtpGenerator();

    [Fact]
    public void Generate_Returns6Digits_ByDefault()
    {
        var otp = _generator.Generate();
        Assert.Matches(@"^\d{6}$", otp);
    }

    [Fact]
    public void Generate_ReturnsSpecifiedLength()
    {
        var otp = _generator.Generate(8);
        Assert.Matches(@"^\d{8}$", otp);
    }

    [Fact]
    public void Hash_DifferentFromPlaintext()
    {
        string otp = "123456";
        string hash = _generator.Hash(otp);
        Assert.NotEqual(otp, hash);
        Assert.Contains("$", hash);
    }

    [Fact]
    public void Verify_CorrectOtp_ReturnsTrue()
    {
        string otp = "123456";
        string hash = _generator.Hash(otp);
        Assert.True(_generator.Verify(otp, hash));
    }

    [Fact]
    public void Verify_WrongOtp_ReturnsFalse()
    {
        string otp = "123456";
        string hash = _generator.Hash(otp);
        Assert.False(_generator.Verify("654321", hash));
    }

    [Fact]
    public void Verify_EmptyOtp_ReturnsFalse()
    {
        string hash = _generator.Hash("123456");
        Assert.False(_generator.Verify("", hash));
    }

    [Fact]
    public void Verify_NullOtp_ReturnsFalse()
    {
        string hash = _generator.Hash("123456");
        Assert.False(_generator.Verify(null!, hash));
    }

    [Fact]
    public void Verify_EmptyHash_ReturnsFalse()
    {
        Assert.False(_generator.Verify("123456", ""));
    }

    [Fact]
    public void Verify_NullHash_ReturnsFalse()
    {
        Assert.False(_generator.Verify("123456", null!));
    }

    [Fact]
    public void Verify_InvalidHashFormat_ReturnsFalse()
    {
        Assert.False(_generator.Verify("123456", "invalid_hash_format"));
    }

    [Fact]
    public void IsExpired_BeforeExpiry_ReturnsFalse()
    {
        var generatedAt = DateTime.UtcNow.AddMinutes(-5);
        Assert.False(_generator.IsExpired(generatedAt, 10));
    }

    [Fact]
    public void IsExpired_AfterExpiry_ReturnsTrue()
    {
        var generatedAt = DateTime.UtcNow.AddMinutes(-11);
        Assert.True(_generator.IsExpired(generatedAt, 10));
    }

    [Fact]
    public void IsExpired_AtExactExpiry_ReturnsTrue()
    {
        var generatedAt = DateTime.UtcNow.AddMinutes(-10);
        Assert.True(_generator.IsExpired(generatedAt, 10));
    }

    [Fact]
    public void Hash_SameOtp_ProducesDifferentHashes()
    {
        string otp = "123456";
        string hash1 = _generator.Hash(otp);
        string hash2 = _generator.Hash(otp);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Generate_DifferentCalls_ProduceDifferentOtps()
    {
        var otp1 = _generator.Generate();
        var otp2 = _generator.Generate();
        Assert.NotEqual(otp1, otp2);
    }
}

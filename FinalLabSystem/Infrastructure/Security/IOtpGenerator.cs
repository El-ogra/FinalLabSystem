namespace FinalLabSystem.Infrastructure.Security;

public interface IOtpGenerator
{
    string Generate(int digits = 6);
    string Hash(string otp);
    bool Verify(string otp, string hash);
    bool IsExpired(DateTime generatedAt, int expiryMinutes = 10);
}

using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IDeliveryConfirmationService
{
    Task SaveSignatureAsync(int visitId, byte[] signatureImage, string receivedByName, int staffId);
    Task<string> GenerateOtpAsync(int visitId, int staffId);
    Task<bool> VerifyOtpAsync(int visitId, string otp, int staffId);
    Task<bool> IsDeliveredAsync(int visitId);
}

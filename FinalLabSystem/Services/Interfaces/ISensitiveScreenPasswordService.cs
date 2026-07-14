using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface ISensitiveScreenPasswordService
{
    Task<bool> IsPasswordSetAsync(string screenType);
    Task<bool> VerifyAsync(string screenType, string password);
    Task SetPasswordAsync(string screenType, string newPassword, int staffId);
    Task ChangePasswordAsync(string screenType, string currentPassword, string newPassword, int staffId);
}

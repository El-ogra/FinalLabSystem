using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface ICashDrawerService
{
    Task<CashDrawerSummaryDto> GetDailySummaryAsync(DateOnly date);
    Task<CashDrawerSummaryDto> GetSummaryByFilterAsync(CashDrawerFilterDto filter);
    Task<bool> IsPasswordSetAsync();
    Task<bool> UnlockAsync(string password);
    Task SetPasswordAsync(string newPassword);
    Task ChangePasswordAsync(string currentPassword, string newPassword);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAuthService
{
    Task<Staff?> LoginAsync(string username, string password);
    Task<bool> HasPermissionAsync(int staffId, string permissionCode);
    Task UpdateLastLoginAsync(int staffId);
    Task<Staff> CreateUserAsync(Staff staff, List<int> permissionIds);

    Task<bool> HasAnyAdministratorAsync();
    Task<Staff> CreateInitialAdministratorAsync(string username, string password, string? displayName = null);
}

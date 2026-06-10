using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Authenticates a staff member using a username and password.
    /// </summary>
    /// <param name="username">The staff member's login name.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>The authenticated staff member, or <c>null</c> when the credentials are invalid.</returns>
    Task<Staff?> LoginAsync(string username, string password);

    /// <summary>
    /// Checks whether a staff member has a specific permission.
    /// </summary>
    /// <param name="staffId">The staff member identifier.</param>
    /// <param name="permissionCode">The permission code to check.</param>
    /// <returns><c>true</c> when the staff member has the permission; otherwise, <c>false</c>.</returns>
    Task<bool> HasPermissionAsync(int staffId, string permissionCode);

    /// <summary>
    /// Updates the last-login timestamp for a staff member.
    /// </summary>
    /// <param name="staffId">The staff member identifier.</param>
    Task UpdateLastLoginAsync(int staffId);

    /// <summary>
    /// Creates a staff user with the supplied permissions.
    /// </summary>
    /// <param name="staff">The staff record to create.</param>
    /// <param name="permissionIds">The permission identifiers to assign.</param>
    /// <returns>The created staff record.</returns>
    Task<Staff> CreateUserAsync(Staff staff, List<int> permissionIds);

    /// <summary>
    /// Determines whether any administrator account exists.
    /// </summary>
    /// <returns><c>true</c> when at least one administrator exists; otherwise, <c>false</c>.</returns>
    Task<bool> HasAnyAdministratorAsync();

    /// <summary>
    /// Creates the first administrator account during initial setup.
    /// </summary>
    /// <param name="username">The administrator login name.</param>
    /// <param name="password">The plain-text password to hash and store.</param>
    /// <param name="displayName">The optional display name for the administrator.</param>
    /// <returns>The created administrator staff record.</returns>
    Task<Staff> CreateInitialAdministratorAsync(string username, string password, string? displayName = null);
}

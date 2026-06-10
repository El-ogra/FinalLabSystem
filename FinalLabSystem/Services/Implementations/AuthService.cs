using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class AuthService : IAuthService
{
    private const string FullAccessPermissionCode = "SYSTEM.FULL_ACCESS";
    private const string FullAccessPermissionName = "Full System Access";
    private const string FullAccessPermissionGroup = "SYSTEM";
    private const string FullAccessPermissionDescription = "Unrestricted access to every module and operation in the system.";

    private readonly FinalLabDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(FinalLabDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Staff?> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var staff = await _context.Staff
            .FirstOrDefaultAsync(s => s.Username == username && s.IsActive);

        if (staff is null)
            return null;

        return PasswordHasher.Verify(password, staff.PasswordHash) ? staff : null;
    }

    public async Task<bool> HasPermissionAsync(int staffId, string permissionCode)
    {
        var staff = await _context.Staff.AsNoTracking().FirstOrDefaultAsync(s => s.StaffId == staffId);
        if (staff is null)
            return false;

        if (staff.IsAdmin)
            return true;

        return await _context.StaffPermissions
            .Include(sp => sp.Permission)
            .AnyAsync(sp => sp.StaffId == staffId
                            && sp.Permission.PermissionCode == permissionCode
                            && sp.IsGranted);
    }

    public async Task UpdateLastLoginAsync(int staffId)
    {
        var staff = await _context.Staff.FindAsync(staffId);
        if (staff != null)
        {
            staff.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Staff> CreateUserAsync(Staff staff, List<int> permissionIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            var permissions = permissionIds.Select(id => new StaffPermission
            {
                StaffId = staff.StaffId,
                PermissionId = id,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow
            }).ToList();

            _context.StaffPermissions.AddRange(permissions);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return staff;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<bool> HasAnyAdministratorAsync()
        => _context.Staff.AsNoTracking().AnyAsync(s => s.IsAdmin && s.IsActive);

    public async Task<Staff> CreateInitialAdministratorAsync(string username, string password, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        username = username.Trim();
        displayName = string.IsNullOrWhiteSpace(displayName) ? username : displayName.Trim();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (await _context.Staff.AnyAsync(s => s.IsAdmin && s.IsActive))
                throw new InvalidOperationException("An administrator account already exists.");

            if (await _context.Staff.AnyAsync(s => s.Username == username))
                throw new InvalidOperationException($"Username '{username}' is already taken.");

            var fullAccess = await _context.Permissions
                .FirstOrDefaultAsync(p => p.PermissionCode == FullAccessPermissionCode);

            if (fullAccess is null)
            {
                fullAccess = new Permission
                {
                    PermissionCode = FullAccessPermissionCode,
                    PermissionName = FullAccessPermissionName,
                    PermissionGroup = FullAccessPermissionGroup,
                    Description = FullAccessPermissionDescription
                };
                _context.Permissions.Add(fullAccess);
                await _context.SaveChangesAsync();
            }

            var admin = new Staff
            {
                Username = username,
                DisplayName = displayName,
                PasswordHash = PasswordHasher.Hash(password),
                IsAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                JobTitle = "System Administrator"
            };

            _context.Staff.Add(admin);
            await _context.SaveChangesAsync();

            _context.StaffPermissions.Add(new StaffPermission
            {
                StaffId = admin.StaffId,
                PermissionId = fullAccess.PermissionId,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = admin.StaffId
            });
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return admin;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

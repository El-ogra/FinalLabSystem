using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly FinalLabDbContext _context;

    public AuthService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<Staff?> LoginAsync(string username, string password)
    {
        return await _context.Staff
            .FirstOrDefaultAsync(s => s.Username == username && s.PasswordHash == password && s.IsActive);
    }

    public async Task<bool> HasPermissionAsync(int staffId, string permissionCode)
    {
        return await _context.StaffPermissions
            .Include(sp => sp.Permission)
            .AnyAsync(sp => sp.StaffId == staffId && sp.Permission.PermissionCode == permissionCode && sp.IsGranted);
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
}

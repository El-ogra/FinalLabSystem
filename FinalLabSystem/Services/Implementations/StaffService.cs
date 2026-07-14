using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<StaffService> _logger;
    private readonly IAuditService _auditService;

    public StaffService(FinalLabDbContext context, ILogger<StaffService> logger, IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<List<Staff>> GetAllAsync()
    {
        return await _context.Staff
            .OrderBy(s => s.DisplayName)
            .ToListAsync();
    }

    public async Task<Staff?> GetByIdAsync(int staffId)
    {
        return await _context.Staff.FindAsync(staffId);
    }

    public async Task UpdateDiscountLimitAsync(int staffId, double discountLimit, int modifiedByStaffId)
    {
        var staff = await _context.Staff.FindAsync(staffId)
            ?? throw new InvalidOperationException($"Staff with ID {staffId} not found.");

        var oldValue = staff.DiscountLimit;
        staff.DiscountLimit = discountLimit;

        await _context.SaveChangesAsync();

        await _auditService.LogActionAsync(
            tableName: "Staff",
            recordId: staffId,
            action: "DISCOUNT_LIMIT_UPDATED",
            staffId: modifiedByStaffId,
            notes: $"تغيير حد الخصم من {oldValue}% إلى {discountLimit}% للموظف {staff.DisplayName}");
    }

    public async Task UpdateStaffAsync(Staff staff, int modifiedByStaffId)
    {
        var existing = await _context.Staff.FindAsync(staff.StaffId)
            ?? throw new InvalidOperationException($"Staff with ID {staff.StaffId} not found.");

        var oldDiscountLimit = existing.DiscountLimit;

        existing.DisplayName = staff.DisplayName;
        existing.DisplayNameAr = staff.DisplayNameAr;
        existing.JobTitle = staff.JobTitle;
        existing.Phone = staff.Phone;
        existing.Email = staff.Email;
        existing.IsActive = staff.IsActive;
        existing.DiscountLimit = staff.DiscountLimit;

        await _context.SaveChangesAsync();

        if (Math.Abs(oldDiscountLimit - staff.DiscountLimit) > 0.001)
        {
            await _auditService.LogActionAsync(
                tableName: "Staff",
                recordId: staff.StaffId,
                action: "DISCOUNT_LIMIT_UPDATED",
                staffId: modifiedByStaffId,
                notes: $"تغيير حد الخصم من {oldDiscountLimit}% إلى {staff.DiscountLimit}% للموظف {staff.DisplayName}");
        }
    }
}

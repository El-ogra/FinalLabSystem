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

public class AttendanceService : IAttendanceService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(FinalLabDbContext context, ILogger<AttendanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<WorkShift>> GetActiveShiftsAsync()
    {
        return await _context.WorkShifts
            .Where(ws => ws.IsActive)
            .ToListAsync();
    }

    public async Task RecordClockInAsync(int staffId, int shiftId)
    {
        var shift = await _context.WorkShifts
            .FirstOrDefaultAsync(ws => ws.ShiftId == shiftId);

        if (shift == null)
            throw new InvalidOperationException($"WorkShift with ID {shiftId} not found.");

        var now = DateTime.UtcNow;
        var attendanceDate = DateOnly.FromDateTime(now);
        var actualTimeOfDay = now.TimeOfDay;

        int? lateMinutes;
        if (actualTimeOfDay > shift.ClockInTime)
            lateMinutes = (int)(actualTimeOfDay - shift.ClockInTime).TotalMinutes;
        else
            lateMinutes = 0;

        var attendance = new Attendance
        {
            StaffId = staffId,
            ShiftId = shiftId,
            ClockIn = now,
            AttendanceDate = attendanceDate,
            LateMinutes = lateMinutes
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task RecordClockOutAsync(int staffId)
    {
        var attendance = await _context.Attendances
            .Where(a => a.StaffId == staffId && a.ClockOut == null)
            .OrderByDescending(a => a.ClockIn)
            .FirstOrDefaultAsync();

        if (attendance == null)
            throw new InvalidOperationException($"No active clock-in record found for staff ID {staffId}.");

        attendance.ClockOut = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Attendance>> GetAttendanceByDateRangeAsync(DateOnly from, DateOnly to, int? staffId = null)
    {
        var query = _context.Attendances
            .Include(a => a.Staff)
            .Include(a => a.Shift)
            .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to);

        if (staffId.HasValue)
            query = query.Where(a => a.StaffId == staffId.Value);

        return await query.OrderByDescending(a => a.AttendanceDate).ThenByDescending(a => a.ClockIn).ToListAsync();
    }

    public async Task<TimeSpan> GetTotalHoursWorkedAsync(int staffId, DateOnly from, DateOnly to)
    {
        var records = await _context.Attendances
            .Where(a => a.StaffId == staffId
                     && a.AttendanceDate >= from
                     && a.AttendanceDate <= to
                     && a.ClockOut != null)
            .ToListAsync();

        var total = TimeSpan.Zero;
        foreach (var r in records)
            total += r.ClockOut!.Value - r.ClockIn;

        return total;
    }

    public async Task<Attendance?> GetActiveAttendanceAsync(int staffId)
    {
        return await _context.Attendances
            .Include(a => a.Shift)
            .Where(a => a.StaffId == staffId && a.ClockOut == null)
            .OrderByDescending(a => a.ClockIn)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WorkShift>> GetAllShiftsAsync()
    {
        return await _context.WorkShifts
            .OrderBy(s => s.ShiftName)
            .ToListAsync();
    }

    public async Task<WorkShift> CreateShiftAsync(WorkShift shift)
    {
        _context.WorkShifts.Add(shift);
        await _context.SaveChangesAsync();
        return shift;
    }

    public async Task UpdateShiftAsync(WorkShift shift)
    {
        _context.WorkShifts.Update(shift);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Staff>> GetAllActiveStaffAsync()
    {
        return await _context.Staff
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayName)
            .ToListAsync();
    }
}

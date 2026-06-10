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
}

using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class AttendanceWorkflowEndToEndTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static AttendanceService CreateService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<AttendanceService>>();
        return new AttendanceService(ctx, logger.Object);
    }

    [Fact]
    public async Task EndToEnd_ClockInAndClockOut_Success()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_ClockInAndClockOut_Success)));

        var staff = new Staff { StaffId = 1, DisplayName = "Test Staff", IsActive = true, Username = "testuser", PasswordHash = "hash" };
        ctx.Staff.Add(staff);

        var shift = new WorkShift
        {
            ShiftId = 1,
            ShiftName = "Morning",
            ClockInTime = new TimeSpan(8, 0, 0),
            ClockOutTime = new TimeSpan(16, 0, 0),
            IsActive = true
        };
        ctx.WorkShifts.Add(shift);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        await service.RecordClockInAsync(1, 1);

        var active = await service.GetActiveAttendanceAsync(1);
        Assert.NotNull(active);
        Assert.Null(active.ClockOut);
        Assert.Equal(1, active.StaffId);
        Assert.Equal(1, active.ShiftId);

        await service.RecordClockOutAsync(1);

        var after = await service.GetActiveAttendanceAsync(1);
        Assert.Null(after);

        var records = await service.GetAttendanceByDateRangeAsync(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        Assert.Single(records);
        Assert.NotNull(records[0].ClockOut);
    }

    [Fact]
    public async Task EndToEnd_GetAttendanceByDateRange_ReturnsFiltered()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_GetAttendanceByDateRange_ReturnsFiltered)));

        var staff = new Staff { StaffId = 1, DisplayName = "Staff A", IsActive = true, Username = "staffA", PasswordHash = "hash" };
        ctx.Staff.Add(staff);

        var shift = new WorkShift
        {
            ShiftId = 1,
            ShiftName = "Morning",
            ClockInTime = new TimeSpan(8, 0, 0),
            ClockOutTime = new TimeSpan(16, 0, 0),
            IsActive = true
        };
        ctx.WorkShifts.Add(shift);

        var today = DateTime.UtcNow.Date;
        ctx.Attendances.AddRange(
            new Attendance
            {
                StaffId = 1,
                ShiftId = 1,
                ClockIn = today.AddDays(-2).AddHours(8),
                ClockOut = today.AddDays(-2).AddHours(16),
                AttendanceDate = DateOnly.FromDateTime(today.AddDays(-2)),
                LateMinutes = 0
            },
            new Attendance
            {
                StaffId = 1,
                ShiftId = 1,
                ClockIn = today.AddDays(-1).AddHours(8),
                ClockOut = today.AddDays(-1).AddHours(16),
                AttendanceDate = DateOnly.FromDateTime(today.AddDays(-1)),
                LateMinutes = 0
            },
            new Attendance
            {
                StaffId = 1,
                ShiftId = 1,
                ClockIn = today.AddHours(8),
                ClockOut = today.AddHours(16),
                AttendanceDate = DateOnly.FromDateTime(today),
                LateMinutes = 0
            });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        var from = DateOnly.FromDateTime(today.AddDays(-1));
        var to = DateOnly.FromDateTime(today);
        var result = await service.GetAttendanceByDateRangeAsync(from, to);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(1, a.StaffId));
    }

    [Fact]
    public async Task EndToEnd_GetTotalHoursWorked_CalculatesCorrectly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_GetTotalHoursWorked_CalculatesCorrectly)));

        var staff = new Staff { StaffId = 1, DisplayName = "Staff B", IsActive = true, Username = "staffB", PasswordHash = "hash" };
        ctx.Staff.Add(staff);

        var shift = new WorkShift
        {
            ShiftId = 1,
            ShiftName = "Morning",
            ClockInTime = new TimeSpan(8, 0, 0),
            ClockOutTime = new TimeSpan(16, 0, 0),
            IsActive = true
        };
        ctx.WorkShifts.Add(shift);

        var today = DateTime.UtcNow.Date;
        ctx.Attendances.AddRange(
            new Attendance
            {
                StaffId = 1,
                ShiftId = 1,
                ClockIn = today.AddHours(8),
                ClockOut = today.AddHours(12),
                AttendanceDate = DateOnly.FromDateTime(today),
                LateMinutes = 0
            },
            new Attendance
            {
                StaffId = 1,
                ShiftId = 1,
                ClockIn = today.AddHours(13),
                ClockOut = today.AddHours(17),
                AttendanceDate = DateOnly.FromDateTime(today),
                LateMinutes = 0
            });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        var date = DateOnly.FromDateTime(today);
        var total = await service.GetTotalHoursWorkedAsync(1, date, date);

        Assert.Equal(TimeSpan.FromHours(8), total);
    }

    [Fact]
    public async Task EndToEnd_CreateAndRetrieveShift_Success()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_CreateAndRetrieveShift_Success)));

        var service = CreateService(ctx);

        var newShift = new WorkShift
        {
            ShiftName = "Evening",
            ClockInTime = new TimeSpan(14, 0, 0),
            ClockOutTime = new TimeSpan(22, 0, 0),
            IsActive = true
        };

        var created = await service.CreateShiftAsync(newShift);
        Assert.True(created.ShiftId > 0);
        Assert.Equal("Evening", created.ShiftName);

        var allShifts = await service.GetAllShiftsAsync();
        Assert.Contains(allShifts, s => s.ShiftName == "Evening" && s.ShiftId == created.ShiftId);

        var activeShifts = await service.GetActiveShiftsAsync();
        Assert.Contains(activeShifts, s => s.ShiftId == created.ShiftId);
    }
}

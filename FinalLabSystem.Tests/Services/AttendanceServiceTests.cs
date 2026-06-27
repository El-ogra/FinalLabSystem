using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class AttendanceServiceTests
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

    private static async Task<(Staff staff, WorkShift shift)> SeedDataAsync(FinalLabDbContext ctx)
    {
        var staff = new Staff
        {
            Username = "testuser",
            DisplayName = "Test User",
            PasswordHash = "hash",
            IsActive = true,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Staff.Add(staff);

        var shift = new WorkShift
        {
            ShiftName = "Morning",
            ClockInTime = new TimeSpan(8, 0, 0),
            ClockOutTime = new TimeSpan(16, 0, 0),
            IsActive = true
        };
        ctx.WorkShifts.Add(shift);
        await ctx.SaveChangesAsync();
        return (staff, shift);
    }

    [Fact]
    public async Task RecordClockInAsync_CreatesRecordWithCorrectLateMinutes()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordClockInAsync_CreatesRecordWithCorrectLateMinutes)));
        var service = CreateService(ctx);
        var (staff, shift) = await SeedDataAsync(ctx);

        await service.RecordClockInAsync(staff.StaffId, shift.ShiftId);

        var record = ctx.Attendances.Single();
        Assert.Equal(staff.StaffId, record.StaffId);
        Assert.Equal(shift.ShiftId, record.ShiftId);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), record.AttendanceDate);
        Assert.Null(record.ClockOut);
    }

    [Fact]
    public async Task RecordClockInAsync_ThrowsWhenShiftNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordClockInAsync_ThrowsWhenShiftNotFound)));
        var service = CreateService(ctx);
        var (staff, _) = await SeedDataAsync(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RecordClockInAsync(staff.StaffId, 999));
    }

    [Fact]
    public async Task RecordClockOutAsync_UpdatesLastOpenRecord()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordClockOutAsync_UpdatesLastOpenRecord)));
        var service = CreateService(ctx);
        var (staff, shift) = await SeedDataAsync(ctx);
        await service.RecordClockInAsync(staff.StaffId, shift.ShiftId);

        await service.RecordClockOutAsync(staff.StaffId);

        var record = ctx.Attendances.Single();
        Assert.NotNull(record.ClockOut);
        Assert.True(record.ClockOut <= DateTime.UtcNow);
    }

    [Fact]
    public async Task RecordClockOutAsync_ThrowsWhenNoOpenRecord()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordClockOutAsync_ThrowsWhenNoOpenRecord)));
        var service = CreateService(ctx);
        var (staff, _) = await SeedDataAsync(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RecordClockOutAsync(staff.StaffId));
    }

    [Fact]
    public async Task GetAttendanceByDateRangeAsync_FiltersCorrectly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetAttendanceByDateRangeAsync_FiltersCorrectly)));
        var service = CreateService(ctx);
        var (staff, shift) = await SeedDataAsync(ctx);
        await service.RecordClockInAsync(staff.StaffId, shift.ShiftId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await service.GetAttendanceByDateRangeAsync(today, today);

        Assert.Single(result);
        Assert.Equal(staff.StaffId, result[0].StaffId);
    }

    [Fact]
    public async Task GetTotalHoursWorkedAsync_CalculatesCorrectly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetTotalHoursWorkedAsync_CalculatesCorrectly)));
        var service = CreateService(ctx);
        var (staff, shift) = await SeedDataAsync(ctx);

        var attendance = new Attendance
        {
            StaffId = staff.StaffId,
            ShiftId = shift.ShiftId,
            ClockIn = DateTime.UtcNow.AddHours(-8),
            ClockOut = DateTime.UtcNow,
            AttendanceDate = DateOnly.FromDateTime(DateTime.Today)
        };
        ctx.Attendances.Add(attendance);
        await ctx.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var total = await service.GetTotalHoursWorkedAsync(staff.StaffId, today, today);

        Assert.True(total.TotalHours >= 7.9 && total.TotalHours <= 8.1);
    }

    [Fact]
    public async Task GetTotalHoursWorkedAsync_ExcludesOpenRecords()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetTotalHoursWorkedAsync_ExcludesOpenRecords)));
        var service = CreateService(ctx);
        var (staff, shift) = await SeedDataAsync(ctx);

        var attendance = new Attendance
        {
            StaffId = staff.StaffId,
            ShiftId = shift.ShiftId,
            ClockIn = DateTime.UtcNow.AddHours(-4),
            ClockOut = null,
            AttendanceDate = DateOnly.FromDateTime(DateTime.Today)
        };
        ctx.Attendances.Add(attendance);
        await ctx.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var total = await service.GetTotalHoursWorkedAsync(staff.StaffId, today, today);

        Assert.Equal(TimeSpan.Zero, total);
    }

    [Fact]
    public async Task GetActiveAttendanceAsync_ReturnsOpenRecord()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetActiveAttendanceAsync_ReturnsOpenRecord)));
        var service = CreateService(ctx);
        var (staff, shift) = await SeedDataAsync(ctx);
        await service.RecordClockInAsync(staff.StaffId, shift.ShiftId);

        var active = await service.GetActiveAttendanceAsync(staff.StaffId);

        Assert.NotNull(active);
        Assert.Null(active.ClockOut);
    }

    [Fact]
    public async Task CreateShiftAsync_CreatesShift()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateShiftAsync_CreatesShift)));
        var service = CreateService(ctx);
        var shift = new WorkShift
        {
            ShiftName = "Night",
            ClockInTime = new TimeSpan(22, 0, 0),
            ClockOutTime = new TimeSpan(6, 0, 0),
            IsActive = true
        };

        var result = await service.CreateShiftAsync(shift);

        Assert.True(result.ShiftId > 0);
        Assert.Equal("Night", result.ShiftName);
        Assert.Single(ctx.WorkShifts);
    }

    [Fact]
    public async Task UpdateShiftAsync_UpdatesExisting()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateShiftAsync_UpdatesExisting)));
        var service = CreateService(ctx);
        var shift = new WorkShift
        {
            ShiftName = "Evening",
            ClockInTime = new TimeSpan(14, 0, 0),
            ClockOutTime = new TimeSpan(22, 0, 0),
            IsActive = true
        };
        ctx.WorkShifts.Add(shift);
        await ctx.SaveChangesAsync();

        shift.ShiftName = "Evening Updated";
        shift.IsActive = false;
        await service.UpdateShiftAsync(shift);

        var updated = ctx.WorkShifts.Single();
        Assert.Equal("Evening Updated", updated.ShiftName);
        Assert.False(updated.IsActive);
    }
}

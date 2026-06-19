using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class ReceiptServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static Staff CreateStaff(string displayName, bool isAdmin)
        => new()
        {
            Username = $"user_{Guid.NewGuid():N}",
            DisplayName = displayName,
            PasswordHash = "hash",
            IsActive = true,
            IsAdmin = isAdmin,
            CreatedAt = DateTime.UtcNow
        };

    private static Patient CreatePatient()
        => new()
        {
            FullNameAr = "مريض تجريبي",
            FullNameEn = "Test Patient",
            PatientCode = $"P{DateTime.Now:yyyyMMddHHmmss}",
            Sex = "M",
            CreatedAt = DateTime.UtcNow
        };

    private static TestGroup CreateTestGroup(FinalLabDbContext ctx, string name)
    {
        var category = new TestCategory
        {
            CategoryCode = $"CAT_{Guid.NewGuid().ToString("N")[..6]}",
            CategoryNameEn = "Test Category",
            CategoryNameAr = "فئة تجريبية",
            IsActive = true,
            SortOrder = 1
        };
        ctx.TestCategories.Add(category);
        ctx.SaveChanges();

        var group = new TestGroup
        {
            GroupCode = $"GRP_{Guid.NewGuid().ToString("N")[..6]}",
            GroupNameEn = name,
            GroupNameAr = name,
            CategoryId = category.CategoryId,
            IsActive = true,
            SortOrder = 1
        };
        ctx.TestGroups.Add(group);
        ctx.SaveChanges();
        return group;
    }

    private static TestType CreateTestType(FinalLabDbContext ctx, TestGroup group, string name, decimal price)
    {
        var testType = new TestType
        {
            GroupId = group.GroupId,
            TypeCode = $"T_{Guid.NewGuid().ToString("N")[..6]}",
            TypeNameEn = name,
            TypeNameAr = name,
            DefaultPrice = price,
            IsActive = true,
            SortOrder = 1
        };
        ctx.TestTypes.Add(testType);
        ctx.SaveChanges();
        return testType;
    }

    private static async Task<(Visit visit, Staff staff)> CreateVisitWithPayment(
        FinalLabDbContext ctx, decimal subtotal, decimal discount, decimal paid)
    {
        var staff = CreateStaff("Staff1", false);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();

        var patient = CreatePatient();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.PatientId,
            VisitCode = $"V{DateTime.Now:yyyyMMdd}0001",
            VisitDate = DateTime.UtcNow,
            Subtotal = subtotal,
            DiscountAmount = discount,
            DiscountPercent = subtotal > 0 ? Math.Round(discount / subtotal * 100, 2) : 0,
            TotalAfterDiscount = subtotal - discount,
            TotalPaid = paid,
            BalanceDue = subtotal - discount - paid,
            PaymentStatus = paid >= subtotal - discount ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid,
            VisitStatus = VisitStatus.Open,
            ReceptionistId = staff.StaffId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        return (visit, staff);
    }

    // ========== CanPrintReceiptAsync Tests ==========

    [Fact]
    public async Task CanPrintReceiptAsync_FirstPrint_ReturnsTrue()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var (visit, staff) = await CreateVisitWithPayment(ctx, 500, 100, 400);

        var result = await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanPrintReceiptAsync_AfterPrint_SameFinancialState_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var (visit, staff) = await CreateVisitWithPayment(ctx, 500, 100, 400);

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "A4",
            ShowBreakdown = true,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 400,
            BalanceDue = 0
        });

        var result = await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId);

        Assert.False(result);
    }

    [Fact]
    public async Task CanPrintReceiptAsync_AfterPaymentChange_ReturnsTrue()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var (visit, staff) = await CreateVisitWithPayment(ctx, 500, 100, 300);

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "A4",
            ShowBreakdown = true,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 300,
            BalanceDue = 100
        });

        var visitEntity = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(visitEntity);
        visitEntity.TotalPaid = 400;
        visitEntity.BalanceDue = 0;
        await ctx.SaveChangesAsync();

        var result = await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanPrintReceiptAsync_AdminAlwaysCanPrint_ReturnsTrue()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var admin = CreateStaff("Admin", true);
        ctx.Staff.Add(admin);

        var patient = CreatePatient();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.PatientId,
            VisitCode = $"V{DateTime.Now:yyyyMMdd}0002",
            VisitDate = DateTime.UtcNow,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 400,
            BalanceDue = 0,
            PaymentStatus = PaymentStatus.Paid,
            VisitStatus = VisitStatus.Open,
            ReceptionistId = admin.StaffId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = admin.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "A4",
            ShowBreakdown = true,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 400,
            BalanceDue = 0
        });

        var result = await service.CanPrintReceiptAsync(visit.VisitId, admin.StaffId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanPrintReceiptAsync_NonExistentVisit_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var staff = CreateStaff("Staff1", false);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();

        var result = await service.CanPrintReceiptAsync(99999, staff.StaffId);

        Assert.False(result);
    }

    // ========== LogPrintEventAsync Tests ==========

    [Fact]
    public async Task LogPrintEventAsync_ValidEntry_PersistsToDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var (visit, staff) = await CreateVisitWithPayment(ctx, 200, 0, 200);

        var entry = new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "Thermal",
            ShowBreakdown = false,
            Subtotal = 200,
            DiscountAmount = 0,
            TotalAfterDiscount = 200,
            TotalPaid = 200,
            BalanceDue = 0
        };

        await service.LogPrintEventAsync(entry);

        var saved = await ctx.ReceiptPrintLogs.FirstOrDefaultAsync(l => l.VisitId == visit.VisitId);
        Assert.NotNull(saved);
        Assert.Equal("Thermal", saved.Format);
        Assert.False(saved.ShowBreakdown);
        Assert.Equal(200m, saved.Subtotal);
        Assert.Equal(200m, saved.TotalPaid);
        Assert.Equal(0m, saved.BalanceDue);
    }

    [Fact]
    public async Task LogPrintEventAsync_MultipleLogs_AllPersisted()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var (visit, staff) = await CreateVisitWithPayment(ctx, 100, 10, 90);

        for (int i = 0; i < 3; i++)
        {
            await service.LogPrintEventAsync(new ReceiptPrintLog
            {
                VisitId = visit.VisitId,
                StaffId = staff.StaffId,
                PrintedAt = DateTime.UtcNow.AddMinutes(i),
                Format = i % 2 == 0 ? "A4" : "Thermal",
                ShowBreakdown = true,
                Subtotal = 100,
                DiscountAmount = 10,
                TotalAfterDiscount = 90,
                TotalPaid = 90,
                BalanceDue = 0
            });
        }

        var count = await ctx.ReceiptPrintLogs.CountAsync(l => l.VisitId == visit.VisitId);
        Assert.Equal(3, count);
    }

    // ========== GetLastPrintLogAsync Tests ==========

    [Fact]
    public async Task GetLastPrintLogAsync_NoLogs_ReturnsNull()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var result = await service.GetLastPrintLogAsync(99999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLastPrintLogAsync_MultipleLogs_ReturnsLatest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var (visit, staff) = await CreateVisitWithPayment(ctx, 300, 0, 300);

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow.AddHours(-2),
            Format = "A4",
            ShowBreakdown = true,
            Subtotal = 300, DiscountAmount = 0,
            TotalAfterDiscount = 300, TotalPaid = 300, BalanceDue = 0
        });

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "Thermal",
            ShowBreakdown = false,
            Subtotal = 300, DiscountAmount = 0,
            TotalAfterDiscount = 300, TotalPaid = 300, BalanceDue = 0
        });

        var last = await service.GetLastPrintLogAsync(visit.VisitId);

        Assert.NotNull(last);
        Assert.Equal("Thermal", last.Format);
    }

    // ========== GetGroupedTestsForReceiptAsync Tests ==========

    [Fact]
    public async Task GetGroupedTestsForReceiptAsync_AllTestsInGroup_Summarized()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var patient = CreatePatient();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var group = CreateTestGroup(ctx, "Kidney Functions");
        var t1 = CreateTestType(ctx, group, "Creatinine", 15);
        var t2 = CreateTestType(ctx, group, "BUN", 12);
        var t3 = CreateTestType(ctx, group, "Uric Acid", 10);

        var staff = CreateStaff("Staff", false);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.PatientId,
            VisitCode = $"V{DateTime.Now:yyyyMMdd}0003",
            VisitDate = DateTime.UtcNow,
            Subtotal = 37, TotalAfterDiscount = 37,
            TotalPaid = 37, BalanceDue = 0,
            PaymentStatus = PaymentStatus.Paid,
            VisitStatus = VisitStatus.Open,
            ReceptionistId = staff.StaffId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        ctx.VisitTests.AddRange(
            new VisitTest { VisitId = visit.VisitId, TesttypeId = t1.TesttypeId, PriceCharged = 15, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow },
            new VisitTest { VisitId = visit.VisitId, TesttypeId = t2.TesttypeId, PriceCharged = 12, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow },
            new VisitTest { VisitId = visit.VisitId, TesttypeId = t3.TesttypeId, PriceCharged = 10, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var result = await service.GetGroupedTestsForReceiptAsync(visit.VisitId);

        Assert.Single(result);
        Assert.True(result[0].IsSummarized);
        Assert.Equal("Kidney Functions", result[0].GroupName);
        Assert.Equal(3, result[0].TestCount);
        Assert.Equal(37m, result[0].TotalPrice);
        Assert.NotNull(result[0].DetailLine);
    }

    [Fact]
    public async Task GetGroupedTestsForReceiptAsync_SomeTestsInGroup_IndividualLines()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var patient = CreatePatient();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var group = CreateTestGroup(ctx, "Lipids");
        var t1 = CreateTestType(ctx, group, "Cholesterol", 20);
        var t2 = CreateTestType(ctx, group, "Triglycerides", 15);
        var t3 = CreateTestType(ctx, group, "HDL", 10);

        var staff = CreateStaff("Staff", false);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.PatientId,
            VisitCode = $"V{DateTime.Now:yyyyMMdd}0004",
            VisitDate = DateTime.UtcNow,
            Subtotal = 35, TotalAfterDiscount = 35,
            TotalPaid = 35, BalanceDue = 0,
            PaymentStatus = PaymentStatus.Paid,
            VisitStatus = VisitStatus.Open,
            ReceptionistId = staff.StaffId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        ctx.VisitTests.AddRange(
            new VisitTest { VisitId = visit.VisitId, TesttypeId = t1.TesttypeId, PriceCharged = 20, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow },
            new VisitTest { VisitId = visit.VisitId, TesttypeId = t2.TesttypeId, PriceCharged = 15, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var result = await service.GetGroupedTestsForReceiptAsync(visit.VisitId);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.False(r.IsSummarized));
    }

    [Fact]
    public async Task GetGroupedTestsForReceiptAsync_EmptyVisit_ReturnsEmpty()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var result = await service.GetGroupedTestsForReceiptAsync(99999);

        Assert.Empty(result);
    }

    // ========== Print-Once Integration Test ==========

    [Fact]
    public async Task PrintOnce_PatientPaysPartial_PaysMore_CanPrintAgain()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<ReceiptService>>();
        var service = new ReceiptService(ctx, logger);

        var staff = CreateStaff("Staff1", false);
        ctx.Staff.Add(staff);
        var patient = CreatePatient();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.PatientId,
            VisitCode = $"V{DateTime.Now:yyyyMMdd}0005",
            VisitDate = DateTime.UtcNow,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 300,
            BalanceDue = 100,
            PaymentStatus = PaymentStatus.PartiallyPaid,
            VisitStatus = VisitStatus.Open,
            ReceptionistId = staff.StaffId,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        Assert.True(await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId));

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "A4",
            ShowBreakdown = true,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 300,
            BalanceDue = 100
        });

        Assert.False(await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId));

        var v = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(v);
        v!.TotalPaid = 400;
        v.BalanceDue = 0;
        v.PaymentStatus = PaymentStatus.Paid;
        await ctx.SaveChangesAsync();

        Assert.True(await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId));

        await service.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = visit.VisitId,
            StaffId = staff.StaffId,
            PrintedAt = DateTime.UtcNow,
            Format = "Thermal",
            ShowBreakdown = false,
            Subtotal = 500,
            DiscountAmount = 100,
            TotalAfterDiscount = 400,
            TotalPaid = 400,
            BalanceDue = 0
        });

        Assert.False(await service.CanPrintReceiptAsync(visit.VisitId, staff.StaffId));
    }
}

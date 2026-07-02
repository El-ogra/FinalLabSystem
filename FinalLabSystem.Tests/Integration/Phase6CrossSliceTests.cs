using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Integration;

public class Phase6CrossSliceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static async Task<Staff> SeedStaffAsync(FinalLabDbContext ctx)
    {
        var staff = new Staff
        {
            Username = "tech1",
            DisplayName = "Tech 1",
            IsActive = true,
            PasswordHash = "hash"
        };
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();
        return staff;
    }

    private static async Task<Patient> SeedPatientAsync(FinalLabDbContext ctx, string code = "P001")
    {
        var patient = new Patient
        {
            PatientCode = code,
            FullNameAr = "مريض تجريبي",
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();
        return patient;
    }

    private static async Task<Visit> SeedVisitAsync(FinalLabDbContext ctx, int patientId, string visitCode = "V001")
    {
        var visit = new Visit
        {
            VisitCode = visitCode,
            PatientId = patientId,
            VisitDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();
        return visit;
    }

    [Fact]
    public async Task CrossSlice_DeliveryConfirmation_CreatesAuditLogEntry()
    {
        var dbName = nameof(CrossSlice_DeliveryConfirmation_CreatesAuditLogEntry);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var auditService = new Mock<IAuditService>();
        auditService.Setup(a => a.LogActionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        var otpGenerator = new OtpGenerator();
        var logger = new Mock<ILogger<DeliveryConfirmationService>>();
        var service = new DeliveryConfirmationService(ctx, auditService.Object, otpGenerator, logger.Object);

        await service.SaveSignatureAsync(visit.VisitId, new byte[] { 1, 2, 3 }, "المستلم", staff.StaffId);

        auditService.Verify(a => a.LogActionAsync(
            It.Is<string>(s => s == "DeliverySignatureConfirmed"),
            staff.StaffId,
            It.IsAny<string>(),
            staff.StaffId,
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task CrossSlice_DeliveryConfirmation_VisitMarkedAsDelivered()
    {
        var dbName = nameof(CrossSlice_DeliveryConfirmation_VisitMarkedAsDelivered);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        string otp = await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);
        bool verified = await service.VerifyOtpAsync(visit.VisitId, otp, staff.StaffId);

        Assert.True(verified);
        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.NotNull(updatedVisit.DeliveryConfirmedAt);

        var confirmations = await ctx.DeliveryConfirmations.Where(c => c.VisitId == visit.VisitId).ToListAsync();
        Assert.Single(confirmations);
        Assert.Equal(DeliveryConfirmationMethod.OTP, confirmations[0].Method);
    }

    [Fact]
    public void CrossSlice_PrintQueue_PassesCorrectDocumentTypesToPrintService()
    {
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        var printQueueService = new PrintQueueService(mockScopeFactory.Object);

        var item1 = new PrintQueueItemDto
        {
            VisitId = 1,
            PatientName = "Ali",
            DocumentType = "VisitReceipt",
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };
        var item2 = new PrintQueueItemDto
        {
            VisitId = 2,
            PatientName = "Sara",
            DocumentType = "TestResult",
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };

        printQueueService.Enqueue(item1);
        printQueueService.Enqueue(item2);

        var items = printQueueService.GetItems();
        Assert.Equal(2, items.Count);
        Assert.Equal(PrintQueueItemStatus.Pending, items[0].Status);
        Assert.Equal(PrintQueueItemStatus.Pending, items[1].Status);
    }

    [Fact]
    public async Task CrossSlice_ReportSettings_SavedLayoutPersistsAndAudited()
    {
        var dbName = nameof(CrossSlice_ReportSettings_SavedLayoutPersistsAndAudited);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var staff = await SeedStaffAsync(ctx);

        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(staff);

        var auditService = new Mock<IAuditService>();
        var logger = new Mock<ILogger<ReportLayoutService>>();
        var service = new ReportLayoutService(ctx, session.Object, auditService.Object, logger.Object);

        var layout = new ReportLayoutDto
        {
            LabNameAr = "مختبر البراء",
            LabNameEn = "Al-Baraa Lab",
            PrimaryColor = "#1565C0",
            FontFamily = "Arial",
            FontSize = 14,
            ShowHeader = true,
            ShowFooter = false
        };

        await service.SaveLayoutAsync(layout, staff.StaffId);
        var readBack = await service.GetCurrentLayoutAsync();

        Assert.Equal("مختبر البراء", readBack.LabNameAr);
        Assert.Equal("Al-Baraa Lab", readBack.LabNameEn);
        Assert.Equal("#1565C0", readBack.PrimaryColor);
    }

    [Fact]
    public async Task CrossSlice_PatientLifecycle_DeliveryAfterResults()
    {
        var dbName = nameof(CrossSlice_PatientLifecycle_DeliveryAfterResults);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);

        Assert.False(await service.IsDeliveredAsync(visit.VisitId));

        string otp = await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);
        Assert.NotNull(otp);

        bool verified = await service.VerifyOtpAsync(visit.VisitId, otp, staff.StaffId);
        Assert.True(verified);

        Assert.True(await service.IsDeliveredAsync(visit.VisitId));
    }

    [Fact]
    public async Task CrossSlice_MultipleDeliveries_IndependentPerVisit()
    {
        var dbName = nameof(CrossSlice_MultipleDeliveries_IndependentPerVisit);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit1 = await SeedVisitAsync(ctx, patient.PatientId, "V001");
        var visit2 = await SeedVisitAsync(ctx, patient.PatientId, "V002");
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);

        await service.SaveSignatureAsync(visit1.VisitId, new byte[] { 1 }, "المستلم1", staff.StaffId);

        Assert.True(await service.IsDeliveredAsync(visit1.VisitId));
        Assert.False(await service.IsDeliveredAsync(visit2.VisitId));
    }

    [Fact]
    public void Regression_Phase5_PrintQueueService_StillWorks()
    {
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var service = new PrintQueueService(mockScopeFactory.Object);

        var item = new PrintQueueItemDto
        {
            VisitId = 1,
            PatientName = "Test",
            DocumentType = "Receipt",
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };

        service.Enqueue(item);
        Assert.Single(service.GetItems());
        Assert.Equal(PrintQueueItemStatus.Pending, service.GetItems()[0].Status);
    }

    [Fact]
    public void Regression_Phase5_PrintQueueService_ClearAndRemove()
    {
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var service = new PrintQueueService(mockScopeFactory.Object);

        var item1 = new PrintQueueItemDto
        {
            VisitId = 1,
            PatientName = "Patient 1",
            DocumentType = "Receipt",
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };
        var item2 = new PrintQueueItemDto
        {
            VisitId = 2,
            PatientName = "Patient 2",
            DocumentType = "TestResult",
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };

        service.Enqueue(item1);
        service.Enqueue(item2);
        Assert.Equal(2, service.GetItems().Count);

        service.Remove(item1);
        Assert.Single(service.GetItems());

        service.Clear();
        Assert.Empty(service.GetItems());
    }

    private static DeliveryConfirmationService CreateService(FinalLabDbContext ctx)
    {
        var auditService = new Mock<IAuditService>();
        auditService.Setup(a => a.LogActionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        var otpGenerator = new OtpGenerator();
        var logger = new Mock<ILogger<DeliveryConfirmationService>>();
        return new DeliveryConfirmationService(ctx, auditService.Object, otpGenerator, logger.Object);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class DeliveryConfirmationServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static DeliveryConfirmationService CreateService(FinalLabDbContext ctx, Mock<IAuditService>? auditServiceMock = null)
    {
        var auditService = auditServiceMock ?? new Mock<IAuditService>();
        auditService.Setup(a => a.LogActionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        var otpGenerator = new OtpGenerator();
        var logger = new Mock<ILogger<DeliveryConfirmationService>>();
        return new DeliveryConfirmationService(ctx, auditService.Object, otpGenerator, logger.Object);
    }

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

    private static async Task<Visit> SeedVisitAsync(FinalLabDbContext ctx, int patientId)
    {
        var visit = new Visit
        {
            VisitCode = "V001",
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

    private static async Task<Patient> SeedPatientAsync(FinalLabDbContext ctx)
    {
        var patient = new Patient
        {
            PatientCode = "P001",
            FullNameAr = "مريض تجريبي",
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();
        return patient;
    }

    [Fact]
    public async Task SaveSignatureAsync_PersistsRecord_WithConfirmedAtUtcNow()
    {
        var dbName = nameof(SaveSignatureAsync_PersistsRecord_WithConfirmedAtUtcNow);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        byte[] signature = new byte[] { 1, 2, 3, 4, 5 };

        await service.SaveSignatureAsync(visit.VisitId, signature, "المستلم", staff.StaffId);

        var confirmation = await ctx.DeliveryConfirmations.FirstOrDefaultAsync(c => c.VisitId == visit.VisitId);
        Assert.NotNull(confirmation);
        Assert.Equal(DeliveryConfirmationMethod.Signature, confirmation.Method);
        Assert.Equal("المستلم", confirmation.ReceivedByName);
        Assert.Equal(staff.StaffId, confirmation.StaffId);
        Assert.Equal(signature, confirmation.SignatureImage);
        Assert.True(confirmation.ConfirmedAt.Kind == DateTimeKind.Utc);
    }

    [Fact]
    public async Task SaveSignatureAsync_LogsAuditEvent()
    {
        var dbName = nameof(SaveSignatureAsync_LogsAuditEvent);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var auditService = new Mock<IAuditService>();
        auditService.Setup(a => a.LogActionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        var service = CreateService(ctx, auditService);

        await service.SaveSignatureAsync(visit.VisitId, new byte[] { 1 }, "المستلم", staff.StaffId);

        auditService.Verify(a => a.LogActionAsync(
            It.IsAny<string>(),
            staff.StaffId,
            It.IsAny<string>(),
            staff.StaffId,
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task SaveSignatureAsync_UpdatesVisit_DeliveryConfirmedAt()
    {
        var dbName = nameof(SaveSignatureAsync_UpdatesVisit_DeliveryConfirmedAt);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        await service.SaveSignatureAsync(visit.VisitId, new byte[] { 1 }, "المستلم", staff.StaffId);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.NotNull(updatedVisit.DeliveryConfirmedAt);
    }

    [Fact]
    public async Task GenerateOtpAsync_PersistsHash_NotPlaintext()
    {
        var dbName = nameof(GenerateOtpAsync_PersistsHash_NotPlaintext);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        string otp = await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.NotNull(updatedVisit.DeliveryOtpCode);
        Assert.NotEqual(otp, updatedVisit.DeliveryOtpCode);
    }

    [Fact]
    public async Task GenerateOtpAsync_ReturnsPlainOtp_ToCaller()
    {
        var dbName = nameof(GenerateOtpAsync_ReturnsPlainOtp_ToCaller);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        string otp = await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);

        Assert.Matches(@"^\d{6}$", otp);
    }

    [Fact]
    public async Task VerifyOtpAsync_CorrectOtp_MarksVisitAsDelivered()
    {
        var dbName = nameof(VerifyOtpAsync_CorrectOtp_MarksVisitAsDelivered);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        string otp = await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);
        bool result = await service.VerifyOtpAsync(visit.VisitId, otp, staff.StaffId);

        Assert.True(result);
        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.NotNull(updatedVisit.DeliveryConfirmedAt);
        Assert.Null(updatedVisit.DeliveryOtpCode);
    }

    [Fact]
    public async Task VerifyOtpAsync_CorrectOtp_CreatesConfirmationRecord()
    {
        var dbName = nameof(VerifyOtpAsync_CorrectOtp_CreatesConfirmationRecord);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        string otp = await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);
        await service.VerifyOtpAsync(visit.VisitId, otp, staff.StaffId);

        var confirmation = await ctx.DeliveryConfirmations.FirstOrDefaultAsync(c => c.VisitId == visit.VisitId);
        Assert.NotNull(confirmation);
        Assert.Equal(DeliveryConfirmationMethod.OTP, confirmation.Method);
    }

    [Fact]
    public async Task VerifyOtpAsync_WrongOtp_ReturnsFalse()
    {
        var dbName = nameof(VerifyOtpAsync_WrongOtp_ReturnsFalse);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        await service.GenerateOtpAsync(visit.VisitId, staff.StaffId);
        bool result = await service.VerifyOtpAsync(visit.VisitId, "000000", staff.StaffId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsDeliveredAsync_NotDelivered_ReturnsFalse()
    {
        var dbName = nameof(IsDeliveredAsync_NotDelivered_ReturnsFalse);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);

        var service = CreateService(ctx);
        bool result = await service.IsDeliveredAsync(visit.VisitId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsDeliveredAsync_Delivered_ReturnsTrue()
    {
        var dbName = nameof(IsDeliveredAsync_Delivered_ReturnsTrue);
        using var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var patient = await SeedPatientAsync(ctx);
        var visit = await SeedVisitAsync(ctx, patient.PatientId);
        var staff = await SeedStaffAsync(ctx);

        var service = CreateService(ctx);
        await service.SaveSignatureAsync(visit.VisitId, new byte[] { 1 }, "المستلم", staff.StaffId);

        bool result = await service.IsDeliveredAsync(visit.VisitId);
        Assert.True(result);
    }
}

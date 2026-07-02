using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public sealed class DeliveryConfirmationService : IDeliveryConfirmationService
{
    private readonly FinalLabDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IOtpGenerator _otpGenerator;
    private readonly ILogger<DeliveryConfirmationService> _logger;

    public DeliveryConfirmationService(
        FinalLabDbContext context,
        IAuditService auditService,
        IOtpGenerator otpGenerator,
        ILogger<DeliveryConfirmationService> logger)
    {
        _context = context;
        _auditService = auditService;
        _otpGenerator = otpGenerator;
        _logger = logger;
    }

    public async Task SaveSignatureAsync(int visitId, byte[] signatureImage, string receivedByName, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId)
            ?? throw new InvalidOperationException($"Visit {visitId} not found.");

        var confirmation = new DeliveryConfirmation
        {
            VisitId = visitId,
            Method = DeliveryConfirmationMethod.Signature,
            ConfirmedAt = DateTime.UtcNow,
            SignatureImage = signatureImage,
            ReceivedByName = receivedByName,
            StaffId = staffId
        };

        _context.DeliveryConfirmations.Add(confirmation);
        visit.DeliveryConfirmedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("DeliverySignatureConfirmed", staffId, "DeliverySignatureConfirmed", staffId, "Signature confirmed");

        _logger.LogInformation("Delivery signature confirmed for visit {VisitId} at {Time}", visitId, DateTime.UtcNow);
    }

    public async Task<string> GenerateOtpAsync(int visitId, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId)
            ?? throw new InvalidOperationException($"Visit {visitId} not found.");

        string otp = _otpGenerator.Generate(6);
        string hash = _otpGenerator.Hash(otp);

        visit.DeliveryOtpCode = hash;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP generated for visit {VisitId}", visitId);
        return otp;
    }

    public async Task<bool> VerifyOtpAsync(int visitId, string otp, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null || string.IsNullOrEmpty(visit.DeliveryOtpCode))
            return false;

        if (!_otpGenerator.Verify(otp, visit.DeliveryOtpCode))
            return false;

        var confirmation = new DeliveryConfirmation
        {
            VisitId = visitId,
            Method = DeliveryConfirmationMethod.OTP,
            ConfirmedAt = DateTime.UtcNow,
            OtpCodeHash = visit.DeliveryOtpCode,
            ReceivedByName = "OTP Verified",
            StaffId = staffId
        };

        _context.DeliveryConfirmations.Add(confirmation);
        visit.DeliveryConfirmedAt = DateTime.UtcNow;
        visit.DeliveryOtpCode = null;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("DeliveryOtpConfirmed", staffId, "DeliveryOtpConfirmed", staffId, "OTP confirmed");

        _logger.LogInformation("Delivery OTP confirmed for visit {VisitId} at {Time}", visitId, DateTime.UtcNow);
        return true;
    }

    public async Task<bool> IsDeliveredAsync(int visitId)
    {
        return await _context.Visits
            .AnyAsync(v => v.VisitId == visitId && v.DeliveryConfirmedAt != null);
    }
}

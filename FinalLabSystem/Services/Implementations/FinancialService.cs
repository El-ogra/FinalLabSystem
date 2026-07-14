using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class FinancialService : IFinancialService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<FinancialService> _logger;
    private readonly IVisitService _visitService;
    private readonly IAuditService _auditService;

    public FinancialService(FinalLabDbContext context, ILogger<FinancialService> logger, IVisitService visitService, IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _visitService = visitService;
        _auditService = auditService;
    }

    public async Task RecordPatientPaymentAsync(Payment payment)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            payment.PaymentDate = DateTime.UtcNow;
            payment.PaymentType = "PAYMENT";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var visit = await _context.Visits.FindAsync(payment.VisitId);
            if (visit != null)
            {
                await _context.Entry(visit).ReloadAsync();
                // VS-03: تحديث الأعلام
                await _visitService.UpdateVisitFlagsAsync(visit.VisitId);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ApplyDiscountAsync(int visitId, decimal discount, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
            throw new InvalidOperationException($"Visit with ID {visitId} not found.");

        var staff = await _context.Staff.FindAsync(staffId);
        if (staff == null)
            throw new InvalidOperationException($"Staff with ID {staffId} not found.");

        // VS-06: التحقق من حد الخصم المسموح للموظف
        if (!staff.IsAdmin && discount > (decimal)staff.DiscountLimit)
        {
            await _auditService.LogActionAsync(
                tableName: "Visit",
                recordId: visitId,
                action: "DISCOUNT_REJECTED",
                staffId: staffId,
                notes: $"محاولة خصم {discount}% تجاوزت الحد المسموح {staff.DiscountLimit}% للموظف {staff.DisplayName}");

            throw new InvalidOperationException(
                $"الخصم {discount}% يتجاوز الحد المسموح {staff.DiscountLimit}%. لا يمكن تطبيق الخصم.");
        }

        visit.DiscountPercent = discount;
        visit.DiscountAmount = visit.Subtotal * discount / 100m;
        visit.TotalAfterDiscount = visit.Subtotal - visit.DiscountAmount;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // VS-03: تحديث الأعلام
        await _visitService.UpdateVisitFlagsAsync(visitId);
    }

    public async Task ApplyFullPaymentAsync(int visitId, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
            throw new InvalidOperationException($"Visit with ID {visitId} not found.");

        var amountToPay = Math.Max(0, visit.TotalAfterDiscount - visit.TotalPaid);
        if (amountToPay > 0)
        {
            _context.Payments.Add(new Payment
            {
                VisitId = visit.VisitId,
                PaymentDate = DateTime.UtcNow,
                Amount = amountToPay,
                PaymentMethod = PaymentMethod.Cash,
                PaymentType = "PAYMENT",
                ReceivedBy = staffId,
                Notes = "Full payment confirmation"
            });
        }

        visit.TotalPaid = visit.TotalAfterDiscount;
        visit.BalanceDue = 0;
        visit.PaymentStatus = PaymentStatus.Paid;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // VS-03: تحديث الأعلام
        await _visitService.UpdateVisitFlagsAsync(visitId);
    }

    public async Task<bool> ApplyClearancePaymentAsync(int visitId, decimal balanceDue)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var visit = await _context.Visits
                .Include(v => v.Payments)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit is null)
                return false;

            var amountToPay = Math.Max(0m, balanceDue);
            if (amountToPay <= 0m)
                amountToPay = Math.Max(0m, visit.TotalAfterDiscount - visit.TotalPaid);

            var receivedBy = visit.ReceptionistId
                ?? visit.Payments.OrderByDescending(payment => payment.PaymentDate).Select(payment => payment.ReceivedBy).FirstOrDefault();

            if (receivedBy <= 0)
                receivedBy = await _context.Staff.Select(staff => staff.StaffId).FirstOrDefaultAsync();

            if (receivedBy <= 0)
                throw new InvalidOperationException("No staff member is available to receive the clearance payment.");

            if (amountToPay > 0)
            {
                _context.Payments.Add(new Payment
                {
                    VisitId = visit.VisitId,
                    PaymentDate = DateTime.UtcNow,
                    Amount = amountToPay,
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentType = "PAYMENT",
                    ReceivedBy = receivedBy,
                    Notes = "Clearance payment"
                });
            }

            visit.TotalPaid = visit.TotalAfterDiscount;
            visit.BalanceDue = 0;
            visit.PaymentStatus = PaymentStatus.Paid;
            visit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // VS-03: تحديث الأعلام
            await _visitService.UpdateVisitFlagsAsync(visitId);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RevertClearanceAsync(int visitId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var visit = await _context.Visits
                .Include(v => v.Payments)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit is null || visit.PaymentStatus == PaymentStatus.Paid)
                return false;

            var lastPayment = visit.Payments
                .OrderByDescending(payment => payment.PaymentDate)
                .ThenByDescending(payment => payment.PaymentId)
                .FirstOrDefault();

            if (lastPayment is not null)
                _context.Payments.Remove(lastPayment);

            var remainingPaid = visit.Payments
                .Where(payment => lastPayment is null || payment.PaymentId != lastPayment.PaymentId)
                .Sum(payment => payment.Amount);

            visit.TotalPaid = remainingPaid;
            visit.BalanceDue = Math.Max(0, visit.TotalAfterDiscount - remainingPaid);
            visit.PaymentStatus = visit.BalanceDue <= 0 ? PaymentStatus.Paid : remainingPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Pending;
            visit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // VS-03: تحديث الأعلام
            await _visitService.UpdateVisitFlagsAsync(visitId);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RevertPaymentAsync(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.Payments)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit == null)
            throw new InvalidOperationException($"Visit with ID {visitId} not found.");

        _context.Payments.RemoveRange(visit.Payments);
        visit.TotalPaid = 0;
        visit.BalanceDue = visit.TotalAfterDiscount;
        visit.PaymentStatus = PaymentStatus.Pending;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // VS-03: تحديث الأعلام
        await _visitService.UpdateVisitFlagsAsync(visitId);
    }

    public async Task<decimal> CalculateSubtotalAsync(List<int> testTypeIds, int? schemeId)
    {
        if (testTypeIds.Count == 0)
            return 0m;

        var uniqueIds = testTypeIds.Distinct().ToList();

        if (schemeId.HasValue)
        {
            var schemePrices = await _context.TestTypePrices
                .Where(p => p.SchemeId == schemeId.Value && uniqueIds.Contains(p.TesttypeId))
                .ToDictionaryAsync(p => p.TesttypeId, p => p.Price);

            var missingIds = uniqueIds.Where(id => !schemePrices.ContainsKey(id)).ToList();
            var defaultPrices = await _context.TestTypes
                .Where(t => missingIds.Contains(t.TesttypeId))
                .ToDictionaryAsync(t => t.TesttypeId, t => t.DefaultPrice);

            return uniqueIds.Sum(id =>
                Convert.ToDecimal(schemePrices.TryGetValue(id, out var schemePrice)
                    ? schemePrice
                    : defaultPrices.GetValueOrDefault(id)));
        }

        var prices = await _context.TestTypes
            .Where(t => uniqueIds.Contains(t.TesttypeId))
            .Select(t => t.DefaultPrice)
            .ToListAsync();

        return prices.Sum(p => Convert.ToDecimal(p));
    }
}

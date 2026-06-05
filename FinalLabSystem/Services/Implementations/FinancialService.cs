using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class FinancialService : IFinancialService
{
    private readonly FinalLabDbContext _context;

    public FinancialService(FinalLabDbContext context)
    {
        _context = context;
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
                await _context.Entry(visit).ReloadAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ApplyDiscountAsync(int visitId, double discount, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
            throw new InvalidOperationException($"Visit with ID {visitId} not found.");

        visit.DiscountPercent = discount;
        visit.DiscountAmount = visit.Subtotal * discount / 100.0;
        visit.TotalAfterDiscount = visit.Subtotal - visit.DiscountAmount;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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
                PaymentMethod = "CASH",
                PaymentType = "PAYMENT",
                ReceivedBy = staffId,
                Notes = "Full payment confirmation"
            });
        }

        visit.TotalPaid = visit.TotalAfterDiscount;
        visit.BalanceDue = 0;
        visit.PaymentStatus = "PAID";
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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
        visit.PaymentStatus = "PENDING";
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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

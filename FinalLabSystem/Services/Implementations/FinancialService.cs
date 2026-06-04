using System;
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
}

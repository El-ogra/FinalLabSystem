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

public class InvoiceService : IInvoiceService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(FinalLabDbContext context, ILogger<InvoiceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ContractInvoice> GenerateInvoiceAsync(int companyId, int year, int month, int staffId)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);

        var totalAmount = await _context.Visits
            .Where(v => v.CompanyId == companyId
                && v.VisitDate >= start
                && v.VisitDate <= end)
            .SumAsync(v => (decimal)v.TotalAfterDiscount);

        var company = await _context.Companies.FindAsync(companyId);
        if (company is not null && company.DiscountRate > 0)
        {
            totalAmount = totalAmount * (1 - (decimal)company.DiscountRate / 100);
        }

        var invoice = new ContractInvoice
        {
            CompanyId = companyId,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.FromDateTime(start),
            PeriodEnd = DateOnly.FromDateTime(end),
            TotalAmount = totalAmount,
            PaidAmount = 0,
            Status = "Pending",
            CreatedBy = staffId
        };

        _context.ContractInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }

    public async Task RecordPaymentAsync(int invoiceId, decimal amount, string method, string? reference, int staffId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var invoice = await _context.ContractInvoices.FindAsync(invoiceId);
            if (invoice is null)
                throw new InvalidOperationException($"Invoice {invoiceId} not found.");

            var payment = new ContractPayment
            {
                ContractInvoiceId = invoiceId,
                PaymentDate = DateTime.UtcNow,
                Amount = amount,
                PaymentMethod = method,
                ReferenceNumber = reference
            };

            _context.ContractPayments.Add(payment);

            invoice.PaidAmount += amount;

            if (invoice.PaidAmount >= invoice.TotalAmount)
                invoice.Status = "Paid";
            else
                invoice.Status = "Partial";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<ContractInvoice>> GetInvoicesAsync(int companyId)
    {
        return await _context.ContractInvoices
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<ContractPayment>> GetPaymentsAsync(int invoiceId)
    {
        return await _context.ContractPayments
            .Where(p => p.ContractInvoiceId == invoiceId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int invoiceId, string status)
    {
        var invoice = await _context.ContractInvoices.FindAsync(invoiceId);
        if (invoice is null)
            return;

        invoice.Status = status;
        await _context.SaveChangesAsync();
    }
}

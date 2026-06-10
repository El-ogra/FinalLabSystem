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

public class ContractService : IContractService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<ContractService> _logger;

    public ContractService(FinalLabDbContext context, ILogger<ContractService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId)
    {
        var totalAmount = await _context.Visits
            .Where(v => v.CompanyId == companyId && v.VisitDate >= start && v.VisitDate <= end)
            .SumAsync(v => (decimal)v.TotalAfterDiscount);

        var invoice = new ContractInvoice
        {
            CompanyId = companyId,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.FromDateTime(start),
            PeriodEnd = DateOnly.FromDateTime(end),
            TotalAmount = totalAmount,
            PaidAmount = 0,
            Status = "DRAFT",
            CreatedBy = staffId
        };

        _context.ContractInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }

    public async Task RecordCorporatePaymentAsync(ContractPayment payment)
    {
        payment.PaymentDate = DateTime.UtcNow;

        _context.ContractPayments.Add(payment);
        await _context.SaveChangesAsync();
    }
}

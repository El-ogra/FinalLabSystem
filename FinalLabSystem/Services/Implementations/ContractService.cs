using System;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class ContractService : IContractService
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<ContractService> _logger;

    public ContractService(IInvoiceService invoiceService, ILogger<ContractService> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    public async Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId)
    {
        return await _invoiceService.GenerateInvoiceAsync(companyId, start.Year, start.Month, staffId);
    }

    public async Task RecordCorporatePaymentAsync(ContractPayment payment)
    {
        await _invoiceService.RecordPaymentAsync(
            payment.ContractInvoiceId,
            payment.Amount,
            payment.PaymentMethod,
            payment.ReferenceNumber,
            0);
    }
}

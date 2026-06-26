using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IInvoiceService
{
    Task<ContractInvoice> GenerateInvoiceAsync(int companyId, int year, int month, int staffId);

    Task RecordPaymentAsync(int invoiceId, decimal amount, string method, string? reference, int staffId);

    Task<List<ContractInvoice>> GetInvoicesAsync(int companyId);

    Task<List<ContractPayment>> GetPaymentsAsync(int invoiceId);

    Task UpdateStatusAsync(int invoiceId, string status);
}

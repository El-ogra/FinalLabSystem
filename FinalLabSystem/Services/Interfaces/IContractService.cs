using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IContractService
{
    Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId);
    Task RecordCorporatePaymentAsync(ContractPayment payment);
}

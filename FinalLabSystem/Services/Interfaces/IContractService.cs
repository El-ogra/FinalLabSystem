using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IContractService
{
    /// <summary>
    /// Generates a monthly invoice for a corporate account.
    /// </summary>
    /// <param name="companyId">The corporate company identifier.</param>
    /// <param name="start">The inclusive invoice period start date.</param>
    /// <param name="end">The inclusive invoice period end date.</param>
    /// <param name="staffId">The staff member generating the invoice.</param>
    /// <returns>The generated contract invoice.</returns>
    Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId);

    /// <summary>
    /// Records a corporate contract payment.
    /// </summary>
    /// <param name="payment">The contract payment to record.</param>
    Task RecordCorporatePaymentAsync(ContractPayment payment);
}

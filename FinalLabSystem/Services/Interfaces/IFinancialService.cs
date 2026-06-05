using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IFinancialService
{
    Task RecordPatientPaymentAsync(Payment payment);
    Task ApplyDiscountAsync(int visitId, double discount, int staffId);
    Task ApplyFullPaymentAsync(int visitId, int staffId);
    Task<bool> ApplyClearancePaymentAsync(int visitId, decimal balanceDue);
    Task<bool> RevertClearanceAsync(int visitId);
    Task RevertPaymentAsync(int visitId);
    Task<decimal> CalculateSubtotalAsync(List<int> testTypeIds, int? schemeId);
}

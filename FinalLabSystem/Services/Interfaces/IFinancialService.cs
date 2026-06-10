using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IFinancialService
{
    /// <summary>
    /// Records a patient payment.
    /// </summary>
    /// <param name="payment">The payment record to save.</param>
    Task RecordPatientPaymentAsync(Payment payment);

    /// <summary>
    /// Applies a discount to a visit.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <param name="discount">The discount amount to apply.</param>
    /// <param name="staffId">The staff member applying the discount.</param>
    Task ApplyDiscountAsync(int visitId, decimal discount, int staffId);

    /// <summary>
    /// Marks a visit as fully paid.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <param name="staffId">The staff member applying the payment.</param>
    Task ApplyFullPaymentAsync(int visitId, int staffId);

    /// <summary>
    /// Applies a clearance payment for a visit balance.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <param name="balanceDue">The balance amount being cleared.</param>
    /// <returns><c>true</c> when the clearance payment was applied; otherwise, <c>false</c>.</returns>
    Task<bool> ApplyClearancePaymentAsync(int visitId, decimal balanceDue);

    /// <summary>
    /// Reverts a previously applied clearance payment.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <returns><c>true</c> when clearance was reverted; otherwise, <c>false</c>.</returns>
    Task<bool> RevertClearanceAsync(int visitId);

    /// <summary>
    /// Reverts payment state for a visit.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    Task RevertPaymentAsync(int visitId);

    /// <summary>
    /// Calculates the subtotal for selected tests under an optional pricing scheme.
    /// </summary>
    /// <param name="testTypeIds">The selected test type identifiers.</param>
    /// <param name="schemeId">The optional pricing scheme identifier.</param>
    /// <returns>The calculated subtotal.</returns>
    Task<decimal> CalculateSubtotalAsync(List<int> testTypeIds, int? schemeId);
}

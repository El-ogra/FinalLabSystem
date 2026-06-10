using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IBloodBankService
{
    /// <summary>
    /// Saves a blood-bank cross-match result and its donor rows.
    /// </summary>
    /// <param name="test">The cross-match test result to save.</param>
    /// <param name="donors">The donor compatibility rows.</param>
    /// <param name="staffId">The staff member saving the result.</param>
    Task SaveCrossMatchResultAsync(CrossMatchTest test, List<CrossMatchDonor> donors, int staffId);

    /// <summary>
    /// Gets cross-match details for a visit test.
    /// </summary>
    /// <param name="visitTestId">The visit-test identifier.</param>
    /// <returns>The cross-match details, or <c>null</c> when none exist.</returns>
    Task<CrossMatchTest?> GetCrossMatchDetailsAsync(int visitTestId);
}

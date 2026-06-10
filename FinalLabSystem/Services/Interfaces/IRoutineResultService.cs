using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IRoutineResultService
{
    /// <summary>
    /// Saves numeric or text routine test results.
    /// </summary>
    /// <param name="results">The result rows to save.</param>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="staffId">The staff member saving the results.</param>
    Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId);

    /// <summary>
    /// Gets result rows for a visit test.
    /// </summary>
    /// <param name="visitTestId">The visit-test identifier.</param>
    /// <returns>The result rows for the visit test.</returns>
    Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId);
}

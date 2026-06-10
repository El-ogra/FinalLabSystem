using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface ISampleTrackingService
{
    /// <summary>
    /// Generates sample tube barcodes for a visit.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <param name="staffId">The staff member generating the barcodes.</param>
    /// <returns>The generated sample tubes.</returns>
    Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId);

    /// <summary>
    /// Gets sample tubes for a visit.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <returns>The sample tubes linked to the visit.</returns>
    Task<List<SampleTube>> GetTubesForVisitAsync(int visitId);

    /// <summary>
    /// Updates the workflow stage for a visit test.
    /// </summary>
    /// <param name="visitTestId">The visit-test identifier.</param>
    /// <param name="newStage">The new workflow stage.</param>
    /// <param name="staffId">The staff member making the change.</param>
    Task UpdateTestStageAsync(int visitTestId, TestStage newStage, int staffId);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IExternalLabService
{
    /// <summary>
    /// Creates an outsource manifest for visit tests sent to an external lab.
    /// </summary>
    /// <param name="externalLabId">The external lab identifier.</param>
    /// <param name="visitTestIds">The visit-test identifiers to include.</param>
    /// <param name="staffId">The staff member creating the manifest.</param>
    /// <returns>The created external shipment.</returns>
    Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId);

    /// <summary>
    /// Updates the status of an external shipment.
    /// </summary>
    /// <param name="shipmentId">The external shipment identifier.</param>
    /// <param name="status">The new shipment status.</param>
    Task UpdateShipmentStatusAsync(int shipmentId, string status);
}

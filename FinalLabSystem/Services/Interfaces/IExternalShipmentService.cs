using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IExternalShipmentService
{
    Task<ExternalShipment> CreateManifestAsync(int externalLabId, List<int> visitTestIds, int staffId);

    Task<List<ExternalShipment>> GetShipmentsAsync(int externalLabId);

    Task ReceiveResultsAsync(int shipmentItemId, string resultValue, int staffId);

    Task UpdateStatusAsync(int shipmentId, string status);
}

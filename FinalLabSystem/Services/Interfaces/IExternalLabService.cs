using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IExternalLabService
{
    Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId);
    Task UpdateShipmentStatusAsync(int shipmentId, string status);
}

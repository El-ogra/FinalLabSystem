using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

/// <summary>
/// Deprecated adapter: delegates to IExternalShipmentService.
/// Will be removed in Phase 7.
/// </summary>
public class ExternalLabService : IExternalLabService
{
    private readonly IExternalShipmentService _shipmentService;
    private readonly ILogger<ExternalLabService> _logger;

    public ExternalLabService(IExternalShipmentService shipmentService, ILogger<ExternalLabService> logger)
    {
        _shipmentService = shipmentService;
        _logger = logger;
    }

    public async Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId)
    {
        return await _shipmentService.CreateManifestAsync(externalLabId, visitTestIds, staffId);
    }

    public async Task UpdateShipmentStatusAsync(int shipmentId, string status)
    {
        await _shipmentService.UpdateStatusAsync(shipmentId, status);
    }
}

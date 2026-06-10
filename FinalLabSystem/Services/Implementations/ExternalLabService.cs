using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class ExternalLabService : IExternalLabService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<ExternalLabService> _logger;

    public ExternalLabService(FinalLabDbContext context, ILogger<ExternalLabService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var lab = await _context.ExternalLabs
                .FirstOrDefaultAsync(el => el.ExternalLabId == externalLabId);

            if (lab == null)
                throw new InvalidOperationException($"ExternalLab with ID {externalLabId} not found.");

            var shipment = new ExternalShipment
            {
                ExternalLabId = externalLabId,
                ShipmentDate = DateTime.UtcNow,
                Status = "PENDING",
                CreatedBy = staffId,
                TrackingNumber = $"SHIP-{externalLabId}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"
            };

            _context.ExternalShipments.Add(shipment);
            await _context.SaveChangesAsync();

            foreach (var visitTestId in visitTestIds)
            {
                var visitTest = await _context.VisitTests
                    .FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);

                if (visitTest == null)
                    throw new InvalidOperationException($"VisitTest with ID {visitTestId} not found.");

                visitTest.IsOutsourced = true;
                visitTest.ExternalLabId = externalLabId;
                visitTest.OutsourceSentAt = DateTime.UtcNow;
                visitTest.OutsourceSentBy = staffId;
                visitTest.CurrentStage = "SENT_OUT";

                var item = new ExternalShipmentItem
                {
                    ShipmentId = shipment.ShipmentId,
                    VisitTestId = visitTestId,
                    Status = "SENT"
                };

                _context.ExternalShipmentItems.Add(item);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await _context.ExternalShipments
                .Include(s => s.ExternalLab)
                .Include(s => s.ExternalShipmentItems)
                    .ThenInclude(i => i.VisitTest)
                .FirstOrDefaultAsync(s => s.ShipmentId == shipment.ShipmentId))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateShipmentStatusAsync(int shipmentId, string status)
    {
        var shipment = await _context.ExternalShipments
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

        if (shipment == null)
            return;

        shipment.Status = status;
        await _context.SaveChangesAsync();
    }
}

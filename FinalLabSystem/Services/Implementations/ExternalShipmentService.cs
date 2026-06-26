using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class ExternalShipmentService : IExternalShipmentService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<ExternalShipmentService> _logger;

    public ExternalShipmentService(FinalLabDbContext context, ILogger<ExternalShipmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExternalShipment> CreateManifestAsync(int externalLabId, List<int> visitTestIds, int staffId)
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
                visitTest.CurrentStage = TestStage.SentOut;

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

    public async Task<List<ExternalShipment>> GetShipmentsAsync(int externalLabId)
    {
        return await _context.ExternalShipments
            .Include(s => s.ExternalLab)
            .Include(s => s.ExternalShipmentItems)
                .ThenInclude(i => i.VisitTest)
            .Where(s => s.ExternalLabId == externalLabId)
            .OrderByDescending(s => s.ShipmentDate)
            .ToListAsync();
    }

    public async Task ReceiveResultsAsync(int shipmentItemId, string resultValue, int staffId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var item = await _context.ExternalShipmentItems
                .Include(i => i.VisitTest)
                    .ThenInclude(vt => vt.Testtype)
                        .ThenInclude(tt => tt.TestComponents)
                .FirstOrDefaultAsync(i => i.ShipmentItemId == shipmentItemId);

            if (item == null)
                throw new InvalidOperationException($"ExternalShipmentItem {shipmentItemId} not found.");

            var visitTest = item.VisitTest;

            foreach (var component in visitTest.Testtype.TestComponents)
            {
                var result = new TestResult
                {
                    VisitTestId = visitTest.VisitTestId,
                    ComponentId = component.ComponentId,
                    ResultValue = resultValue,
                    ResultStatus = "Final",
                    EnteredBy = staffId,
                    EnteredAt = DateTime.UtcNow,
                    ValidationStatus = ResultValidationStatus.Entered
                };

                _context.TestResults.Add(result);
            }

            item.Status = "Received";

            visitTest.CurrentStage = TestStage.ResultEntered;
            visitTest.OutsourceResultReceivedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateStatusAsync(int shipmentId, string status)
    {
        var shipment = await _context.ExternalShipments
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

        if (shipment == null)
            return;

        shipment.Status = status;
        await _context.SaveChangesAsync();
    }
}

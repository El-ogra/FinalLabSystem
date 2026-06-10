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

public class SampleTrackingService : ISampleTrackingService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<SampleTrackingService> _logger;

    public SampleTrackingService(FinalLabDbContext context, ILogger<SampleTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId)
    {
        var existing = await GetTubesForVisitAsync(visitId);
        if (existing.Count > 0)
            return existing;

        var visitTests = await _context.VisitTests
            .Include(vt => vt.Testtype)
            .Where(vt => vt.VisitId == visitId)
            .ToListAsync();

        var groups = visitTests
            .GroupBy(vt => new { vt.Testtype.DefaultTubeType, vt.Testtype.DefaultTubeColor });

        var tubes = new List<SampleTube>();

        foreach (var group in groups)
        {
            var tube = new SampleTube
            {
                VisitId = visitId,
                TubeType = group.Key.DefaultTubeType!,
                TubeColor = group.Key.DefaultTubeColor,
                BarcodeValue = $"TUBE-{visitId}-{Guid.NewGuid():N}",
                PrintedAt = DateTime.UtcNow,
                PrintedBy = staffId
            };

            _context.SampleTubes.Add(tube);

            foreach (var vt in group)
            {
                vt.Tube = tube;
            }

            tubes.Add(tube);
        }

        await _context.SaveChangesAsync();
        return tubes;
    }

    public async Task<List<SampleTube>> GetTubesForVisitAsync(int visitId)
    {
        return await _context.SampleTubes
            .Include(t => t.Visit)
                .ThenInclude(v => v.Patient)
            .Include(t => t.VisitTests)
                .ThenInclude(vt => vt.Testtype)
            .Where(t => t.VisitId == visitId)
            .OrderBy(t => t.TubeId)
            .ToListAsync();
    }

    public async Task UpdateTestStageAsync(int visitTestId, TestStage newStage, int staffId)
    {
        var visitTest = await _context.VisitTests
            .FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);

        if (visitTest == null)
            return;

        var workflow = new TestWorkflow
        {
            VisitTestId = visitTestId,
            Stage = newStage.ToString(),
            PerformedBy = staffId,
            PerformedAt = DateTime.UtcNow
        };

        _context.TestWorkflows.Add(workflow);
        visitTest.CurrentStage = newStage;

        await _context.SaveChangesAsync();
    }
}

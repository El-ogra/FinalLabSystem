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
    private readonly IBarcodeGenerator _barcodeGenerator;

    public SampleTrackingService(FinalLabDbContext context, ILogger<SampleTrackingService> logger, IBarcodeGenerator barcodeGenerator)
    {
        _context = context;
        _logger = logger;
        _barcodeGenerator = barcodeGenerator;
    }

    public async Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId)
    {
        var existing = await GetTubesForVisitAsync(visitId);
        if (existing.Count > 0)
            return existing;

        var visitTests = await _context.VisitTests
            .Include(vt => vt.Testtype)
                .ThenInclude(tt => tt.TestTypeSampleTubes)
            .Include(vt => vt.Testtype)
                .ThenInclude(tt => tt.CollectionType)
            .Include(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
            .Where(vt => vt.VisitId == visitId)
            .ToListAsync();

        if (visitTests.Count == 0)
            return new List<SampleTube>();

        var patientCode = visitTests[0].Visit.Patient.PatientCode;
        var patientId = visitTests[0].Visit.PatientId;

        var caseCode = await _barcodeGenerator.GenerateCaseCodeAsync(visitId);
        var fileCode = await _barcodeGenerator.GenerateFileCodeAsync(visitId);
        var labId = await _barcodeGenerator.GetOrCreateLabIdAsync(patientId);

        var hasLabBarcode = await _context.PatientBarcodes
            .AnyAsync(pb => pb.PatientId == patientId
                         && pb.CodeType == BarcodeCodeType.Lab);

        var ordinal = await _context.PatientBarcodes
            .CountAsync(pb => pb.PatientId == patientId
                           && pb.IssueDate.Date == DateTime.Today
                           && pb.CodeType == BarcodeCodeType.Case) + 1;

        var patientBarcodes = new List<PatientBarcode>
        {
            new PatientBarcode
            {
                PatientId = patientId,
                VisitId = visitId,
                CodeType = BarcodeCodeType.Case,
                BarcodeValue = caseCode,
                IssueDate = DateTime.Now,
                SortOrdinal = ordinal,
                CreatedBy = staffId
            },
            new PatientBarcode
            {
                PatientId = patientId,
                VisitId = visitId,
                CodeType = BarcodeCodeType.File,
                BarcodeValue = fileCode,
                IssueDate = DateTime.Now,
                SortOrdinal = ordinal,
                CreatedBy = staffId
            }
        };

        if (!hasLabBarcode)
        {
            patientBarcodes.Add(new PatientBarcode
            {
                PatientId = patientId,
                VisitId = null,
                CodeType = BarcodeCodeType.Lab,
                BarcodeValue = labId,
                IssueDate = DateTime.Now,
                SortOrdinal = 0,
                CreatedBy = staffId
            });
        }

        _context.PatientBarcodes.AddRange(patientBarcodes);

        var groups = visitTests
            .GroupBy(vt => TubeResolver.ResolvePrimaryTubeIdentity(vt.Testtype));

        var tubes = new List<SampleTube>();
        var tubeOrdinal = 1;

        foreach (var group in groups)
        {
            var tube = new SampleTube
            {
                VisitId = visitId,
                TubeType = group.Key,
                TubeColor = null,
                BarcodeValue = $"{patientCode}-{tubeOrdinal:D2}",
                PrintedAt = DateTime.UtcNow,
                PrintedBy = staffId
            };

            _context.SampleTubes.Add(tube);

            foreach (var vt in group)
            {
                vt.Tube = tube;
            }

            tubes.Add(tube);
            tubeOrdinal++;
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

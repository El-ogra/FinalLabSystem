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

public class RoutineResultService : IRoutineResultService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<RoutineResultService> _logger;

    public RoutineResultService(FinalLabDbContext context, ILogger<RoutineResultService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null)
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");

        int patientAgeInDays;
        if (patient.DateOfBirth.HasValue)
        {
            patientAgeInDays = (int)(DateTime.UtcNow - patient.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)).TotalDays;
        }
        else
        {
            var years = patient.ApproxAge ?? 0;
            patientAgeInDays = (int)(years * 365);
        }

        var visitTestIds = results.Select(r => r.VisitTestId).Distinct().ToList();
        var visitTests = await _context.VisitTests
            .Include(vt => vt.Visit)
            .Where(vt => visitTestIds.Contains(vt.VisitTestId))
            .ToDictionaryAsync(vt => vt.VisitTestId);

        var componentIds = results
            .Where(r => r.ResultNumeric.HasValue)
            .Select(r => r.ComponentId)
            .Distinct()
            .ToList();

        var allRanges = componentIds.Count > 0
            ? await _context.NormalRanges
                .Include(nr => nr.Component)
                .Where(nr => componentIds.Contains(nr.ComponentId))
                .ToListAsync()
            : new List<NormalRange>();

        var rangesByComponent = allRanges
            .GroupBy(nr => nr.ComponentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var existingResults = await _context.TestResults
            .Where(r => visitTestIds.Contains(r.VisitTestId) && componentIds.Contains(r.ComponentId))
            .ToListAsync();

        var existingLookup = existingResults
            .GroupBy(r => (r.VisitTestId, r.ComponentId))
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var result in results)
        {
            result.EnteredBy = staffId;
            result.EnteredAt = DateTime.UtcNow;

            if (result.ResultNumeric.HasValue && rangesByComponent.TryGetValue(result.ComponentId, out var ranges))
            {
                var isPregnant = visitTests.TryGetValue(result.VisitTestId, out var vt) && vt.Visit.IsPregnant;

                var matchingRange = ranges
                    .Where(nr =>
                        (nr.Sex == "B" || nr.Sex == patient.Sex) &&
                        patientAgeInDays >= nr.AgeFromDays &&
                        patientAgeInDays <= nr.AgeToDays &&
                        (!nr.ForPregnantOnly.HasValue || nr.ForPregnantOnly.Value == isPregnant))
                    .OrderBy(nr => nr.RangeId)
                    .FirstOrDefault();

                if (matchingRange != null)
                {
                    result.SnapLowNormal = matchingRange.LowNormal;
                    result.SnapHighNormal = matchingRange.HighNormal;
                    result.SnapLowCritical = matchingRange.LowCritical;
                    result.SnapHighCritical = matchingRange.HighCritical;
                    result.SnapNormalText = matchingRange.NormalRangeText;
                    result.SnapUnit = matchingRange.Component?.Unit;

                    var value = result.ResultNumeric.Value;

                    if (matchingRange.HighCritical.HasValue && value > matchingRange.HighCritical.Value)
                        result.ResultStatus = "HIGH_CRITICAL";
                    else if (matchingRange.LowCritical.HasValue && value < matchingRange.LowCritical.Value)
                        result.ResultStatus = "LOW_CRITICAL";
                    else if (matchingRange.HighNormal.HasValue && value > matchingRange.HighNormal.Value)
                        result.ResultStatus = "HIGH";
                    else if (matchingRange.LowNormal.HasValue && value < matchingRange.LowNormal.Value)
                        result.ResultStatus = "LOW";
                    else
                        result.ResultStatus = "NORMAL";
                }
            }

            var key = (result.VisitTestId, result.ComponentId);
            if (existingLookup.TryGetValue(key, out var existing))
            {
                _context.Entry(existing).CurrentValues.SetValues(result);
                existing.EnteredBy = staffId;
                existing.EnteredAt = DateTime.UtcNow;
            }
            else
            {
                _context.TestResults.Add(result);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId)
    {
        return await _context.TestResults
            .Include(r => r.Component)
            .Where(r => r.VisitTestId == visitTestId)
            .OrderBy(r => r.Component.SortOrder)
            .ToListAsync();
    }
}

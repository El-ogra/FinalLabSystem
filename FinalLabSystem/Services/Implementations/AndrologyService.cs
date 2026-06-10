using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class AndrologyService : IAndrologyService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<AndrologyService> _logger;

    public AndrologyService(FinalLabDbContext context, ILogger<AndrologyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveSemenAnalysisAsync(SemenAnalysis analysis, int staffId)
    {
        analysis.AnalyzedBy = staffId;

        var existing = await _context.SemenAnalyses
            .FirstOrDefaultAsync(sa => sa.VisitTestId == analysis.VisitTestId);

        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(analysis);
            existing.AnalyzedBy = staffId;
        }
        else
        {
            _context.SemenAnalyses.Add(analysis);
        }

        await _context.SaveChangesAsync();
    }
}

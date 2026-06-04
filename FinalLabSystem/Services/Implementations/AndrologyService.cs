using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class AndrologyService : IAndrologyService
{
    private readonly FinalLabDbContext _context;

    public AndrologyService(FinalLabDbContext context)
    {
        _context = context;
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

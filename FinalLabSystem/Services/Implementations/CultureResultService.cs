using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class CultureResultService : ICultureResultService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<CultureResultService> _logger;

    public CultureResultService(FinalLabDbContext context, ILogger<CultureResultService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild)
    {
        var query = _context.AntibioticCatalogs.Where(a => a.IsActive);

        if (isPregnant)
            query = query.Where(a => a.IsSafePregnancy);

        if (isChild)
            query = query.Where(a => a.IsSafeChildren);

        return await query.OrderBy(a => a.AntibioticName).ToListAsync();
    }

    public async Task SaveCultureAsync(MicrobiologyCulture culture)
    {
        _context.MicrobiologyCultures.Add(culture);
        await _context.SaveChangesAsync();
    }

    public async Task AddOrganismsAndSensitivitiesAsync(int cultureId, List<MicrobiologyOrganism> organisms)
    {
        var culture = await _context.MicrobiologyCultures
            .Include(c => c.MicrobiologyOrganisms)
            .FirstOrDefaultAsync(c => c.CultureId == cultureId);

        foreach (var organism in organisms)
        {
            organism.CultureId = cultureId;
            _context.MicrobiologyOrganisms.Add(organism);
        }

        await _context.SaveChangesAsync();
    }
}

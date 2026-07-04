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

    public async Task<MicrobiologyCulture?> GetByVisitTestIdAsync(int visitTestId)
    {
        return await _context.MicrobiologyCultures
            .Include(c => c.MicrobiologyOrganisms)
                .ThenInclude(o => o.OrganismAntibiotics)
            .FirstOrDefaultAsync(c => c.VisitTestId == visitTestId);
    }

    public async Task SaveFullCultureAsync(MicrobiologyCulture culture, List<MicrobiologyOrganism> organisms)
    {
        bool isNewCulture = culture.CultureId == 0;

        foreach (var organism in organisms)
        {
            bool isNewOrganism = organism.OrganismId == 0;

            if (isNewCulture && isNewOrganism)
            {
                if (!culture.MicrobiologyOrganisms.Contains(organism))
                    culture.MicrobiologyOrganisms.Add(organism);
            }
            else if (isNewOrganism)
            {
                organism.CultureId = culture.CultureId;
                _context.MicrobiologyOrganisms.Add(organism);
            }

            foreach (var antibiotic in organism.OrganismAntibiotics.ToList())
            {
                bool isNewAntibiotic = antibiotic.AntibioticResultId == 0;

                if (isNewOrganism && isNewAntibiotic)
                {
                    continue;
                }

                if (isNewAntibiotic)
                {
                    antibiotic.OrganismId = organism.OrganismId;
                    _context.OrganismAntibiotics.Add(antibiotic);
                }
                else
                {
                    antibiotic.OrganismId = organism.OrganismId;
                    _context.OrganismAntibiotics.Update(antibiotic);
                }
            }
        }

        if (isNewCulture)
            _context.MicrobiologyCultures.Add(culture);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateSensitivityAsync(int antibioticResultId, AntibioticSensitivity value)
    {
        var entity = await _context.OrganismAntibiotics.FindAsync(antibioticResultId);
        if (entity != null)
        {
            entity.Sensitivity = value;
            await _context.SaveChangesAsync();
        }
    }
}

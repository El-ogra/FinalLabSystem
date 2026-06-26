using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Implementations;

public class PricingService : IPricingService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<PricingService> _logger;

    public PricingService(FinalLabDbContext context, ILogger<PricingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PriceScheme>> GetAllSchemesAsync()
    {
        return await _context.PriceSchemes
            .OrderBy(s => s.SchemeName)
            .ToListAsync();
    }

    public async Task<decimal> GetTestPriceAsync(int testTypeId, int schemeId)
    {
        var price = await _context.TestTypePrices
            .FirstOrDefaultAsync(tp => tp.TesttypeId == testTypeId && tp.SchemeId == schemeId);

        return price?.Price ?? 0m;
    }

    public async Task UpdateSchemePricesAsync(int schemeId, List<TestTypePrice> prices)
    {
        var existing = await _context.TestTypePrices
            .Where(tp => tp.SchemeId == schemeId)
            .ToListAsync();

        _context.TestTypePrices.RemoveRange(existing);

        foreach (var price in prices)
        {
            price.SchemeId = schemeId;
        }

        _context.TestTypePrices.AddRange(prices);

        await _context.SaveChangesAsync();
    }

    public async Task<PriceScheme?> GetSchemeByIdAsync(int id)
    {
        return await _context.PriceSchemes.FindAsync(id);
    }

    public async Task<PriceScheme> CreateSchemeAsync(PriceScheme scheme)
    {
        scheme.CreatedAt = System.DateTime.UtcNow;
        _context.PriceSchemes.Add(scheme);
        await _context.SaveChangesAsync();
        return scheme;
    }

    public async Task UpdateSchemeAsync(PriceScheme scheme)
    {
        var existing = await _context.PriceSchemes.FindAsync(scheme.SchemeId);
        if (existing is null)
            return;

        existing.SchemeName = scheme.SchemeName;
        existing.Description = scheme.Description;
        existing.IsDefault = scheme.IsDefault;
        existing.IsActive = scheme.IsActive;

        await _context.SaveChangesAsync();
    }
}

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

    public async Task<double> GetTestPriceAsync(int testTypeId, int schemeId)
    {
        var price = await _context.TestTypePrices
            .FirstOrDefaultAsync(tp => tp.TesttypeId == testTypeId && tp.SchemeId == schemeId);

        return price?.Price ?? 0.0;
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
}

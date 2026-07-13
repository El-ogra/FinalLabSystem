using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
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

    /// <summary>
    /// [القرار 12 - VS-01 + VS-02] يرجع سعر التحليل لزيارة معينة وفق الأولوية:
    /// 1) SchemeId (إن وجد) ← تجاوز للأسعار الأساسية (قائمة أسعار مخصصة لجهة معينة).
    /// 2) BillingType على الزيارة (ثنائية التسعير على مستوى TestType - VS-02):
    ///    - Individual ⇒ TestType.PatientDefaultPrice.
    ///    - LabToLab   ⇒ TestType.LabToLabDefaultPrice.
    ///    - Free       ⇒ 0.
    /// </summary>
    public async Task<decimal> GetPriceForTestAsync(int testTypeId, int visitId)
    {
        var visit = await _context.Visits
            .AsNoTracking()
            .Where(v => v.VisitId == visitId)
            .Select(v => new { v.SchemeId, v.BillingType })
            .FirstOrDefaultAsync();

        if (visit is null)
        {
            _logger.LogWarning("GetPriceForTestAsync: Visit {VisitId} not found; falling back to 0.", visitId);
            return 0m;
        }

        // 1) الأولوية لـ SchemeId إن وجد (يتجاوز الأسعار الأساسية الثنائية).
        if (visit.SchemeId.HasValue)
        {
            var schemedPrice = await _context.TestTypePrices
                .AsNoTracking()
                .Where(tp => tp.TesttypeId == testTypeId && tp.SchemeId == visit.SchemeId.Value)
                .Select(tp => (decimal?)tp.Price)
                .FirstOrDefaultAsync();

            if (schemedPrice.HasValue)
                return schemedPrice.Value;
            // إن لم يوجد سعر مخصّص للتحليل في القائمة نتراجع لأساس BillingType.
        }

        // 2) [VS-02] وفق BillingType: يُقرأ السعر المناسب من الثنائية الجديدة في TestType.
        var prices = await _context.TestTypes
            .AsNoTracking()
            .Where(t => t.TesttypeId == testTypeId)
            .Select(t => new { t.PatientDefaultPrice, t.LabToLabDefaultPrice })
            .FirstOrDefaultAsync();

        if (prices is null)
        {
            _logger.LogWarning("GetPriceForTestAsync: TestType {TestTypeId} not found; falling back to 0.", testTypeId);
            return 0m;
        }

        return visit.BillingType switch
        {
            BillingType.Individual => prices.PatientDefaultPrice,
            BillingType.LabToLab => prices.LabToLabDefaultPrice,
            BillingType.Free => 0m,
            _ => prices.PatientDefaultPrice
        };
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

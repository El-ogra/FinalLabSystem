using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Infrastructure;

public sealed class TestPricingEngine
{
    private readonly IPricingService _pricingService;

    public TestPricingEngine(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    public async Task<TestPricingResultDto> ResolvePriceAsync(int testTypeId, int? schemeId)
    {
        if (schemeId is null)
            return new TestPricingResultDto { TestTypeId = testTypeId, Price = 0m, IsFallback = true };

        var price = await _pricingService.GetTestPriceAsync(testTypeId, schemeId.Value);

        if (price == 0m)
            return new TestPricingResultDto { TestTypeId = testTypeId, Price = 0m, IsFallback = true };

        return new TestPricingResultDto { TestTypeId = testTypeId, Price = price, IsFallback = false };
    }

    public async Task<List<TestPricingResultDto>> GetPricingSummaryAsync(IEnumerable<int> testTypeIds, int? schemeId)
    {
        var results = new List<TestPricingResultDto>();
        foreach (var testTypeId in testTypeIds)
        {
            results.Add(await ResolvePriceAsync(testTypeId, schemeId));
        }
        return results;
    }
}

using FinalLabSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IPricingService
{
    Task<List<PriceScheme>> GetAllSchemesAsync();
    Task<decimal> GetTestPriceAsync(int testTypeId, int schemeId);
    Task UpdateSchemePricesAsync(int schemeId, List<TestTypePrice> prices);
}

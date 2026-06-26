using FinalLabSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IPricingService
{
    /// <summary>
    /// Gets all pricing schemes.
    /// </summary>
    /// <returns>The configured pricing schemes.</returns>
    Task<List<PriceScheme>> GetAllSchemesAsync();

    /// <summary>
    /// Gets a test price under a pricing scheme.
    /// </summary>
    /// <param name="testTypeId">The test type identifier.</param>
    /// <param name="schemeId">The pricing scheme identifier.</param>
    /// <returns>The price for the test in the scheme.</returns>
    Task<decimal> GetTestPriceAsync(int testTypeId, int schemeId);

    /// <summary>
    /// Updates prices for a pricing scheme.
    /// </summary>
    /// <param name="schemeId">The pricing scheme identifier.</param>
    /// <param name="prices">The test prices to save.</param>
    Task UpdateSchemePricesAsync(int schemeId, List<TestTypePrice> prices);

    Task<PriceScheme?> GetSchemeByIdAsync(int id);

    Task<PriceScheme> CreateSchemeAsync(PriceScheme scheme);

    Task UpdateSchemeAsync(PriceScheme scheme);
}

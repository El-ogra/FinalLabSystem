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
    /// [القرار 12 - VS-01] يُحدِّد سعر التحليل لزيارة معينة وفق الترتيب:
    /// 1) إن كان للزيارة SchemeId ⇒ ابحث في TestTypePrice.
    /// 2) وإلا: استخدم BillingType على الزيارة:
    ///    - Individual ⇒ سعر المريض (DefaultPrice حاليًا؛ سيصبح PatientDefaultPrice في VS-02).
    ///    - LabToLab   ⇒ سعر معمل-لمعمل (DefaultPrice حاليًا؛ سيصبح LabToLabDefaultPrice في VS-02).
    ///    - Free       ⇒ 0.
    /// </summary>
    /// <param name="testTypeId">معرّف نوع التحليل.</param>
    /// <param name="visitId">معرّف الزيارة.</param>
    /// <returns>السعر المُطبَّق.</returns>
    Task<decimal> GetPriceForTestAsync(int testTypeId, int visitId);

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

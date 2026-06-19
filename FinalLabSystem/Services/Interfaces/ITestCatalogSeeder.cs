using System.Threading;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

/// <summary>
/// Performs an idempotent, upsert-based import of the test catalog
/// (categories, groups, test types, components, normal ranges) from the seed CSV file.
/// </summary>
/// <remarks>
/// The actual seeding logic (CSV parsing, per-entity upsert, NormalRange insert-if-missing
/// handling) is implemented in a later phase. This interface is defined now as scaffolding
/// so that DI wiring and dependent code can reference the contract.
/// </remarks>
public interface ITestCatalogSeeder
{
    /// <summary>
    /// Seeds the test catalog from the embedded CSV file.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous seed operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken = default);
}

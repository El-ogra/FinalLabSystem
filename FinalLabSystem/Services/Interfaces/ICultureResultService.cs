using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface ICultureResultService
{
    /// <summary>
    /// Gets antibiotics considered safe for the supplied patient conditions.
    /// </summary>
    /// <param name="isPregnant">Whether the patient is pregnant.</param>
    /// <param name="isChild">Whether the patient is a child.</param>
    /// <returns>The safe antibiotic catalog entries.</returns>
    Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);

    /// <summary>
    /// Saves a microbiology culture result.
    /// </summary>
    /// <param name="culture">The culture result to save.</param>
    Task SaveCultureAsync(MicrobiologyCulture culture);

    /// <summary>
    /// Adds organisms and sensitivity rows to a culture.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="organisms">The organisms and sensitivities to add.</param>
    Task AddOrganismsAndSensitivitiesAsync(int cultureId, List<MicrobiologyOrganism> organisms);
}

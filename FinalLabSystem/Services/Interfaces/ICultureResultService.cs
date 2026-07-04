using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface ICultureResultService
{
    /// <summary>
    /// Gets antibiotics considered safe for the supplied patient conditions.
    /// </summary>
    Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);

    /// <summary>
    /// Loads an existing culture result with its organisms and antibiotics for a given VisitTest.
    /// Returns null if no culture exists for this VisitTest.
    /// </summary>
    Task<MicrobiologyCulture?> GetByVisitTestIdAsync(int visitTestId);

    /// <summary>
    /// Saves a complete culture result: the culture record, its organisms, and their antibiotic
    /// sensitivity rows — all within a single SaveChangesAsync call (transaction).
    /// </summary>
    Task SaveFullCultureAsync(MicrobiologyCulture culture, List<MicrobiologyOrganism> organisms);

    /// <summary>
    /// Updates a single antibiotic sensitivity value.
    /// </summary>
    Task UpdateSensitivityAsync(int antibioticResultId, AntibioticSensitivity value);
}

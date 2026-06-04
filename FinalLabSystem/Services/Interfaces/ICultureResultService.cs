using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface ICultureResultService
{
    Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);
    Task SaveCultureAsync(MicrobiologyCulture culture);
    Task AddOrganismsAndSensitivitiesAsync(int cultureId, List<MicrobiologyOrganism> organisms);
}

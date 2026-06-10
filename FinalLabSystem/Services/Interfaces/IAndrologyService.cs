using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAndrologyService
{
    /// <summary>
    /// Saves a semen analysis result.
    /// </summary>
    /// <param name="analysis">The semen analysis data to save.</param>
    /// <param name="staffId">The staff member saving the result.</param>
    Task SaveSemenAnalysisAsync(SemenAnalysis analysis, int staffId);
}

using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAndrologyService
{
    Task SaveSemenAnalysisAsync(SemenAnalysis analysis, int staffId);
}

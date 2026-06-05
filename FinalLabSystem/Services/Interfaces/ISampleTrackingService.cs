using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface ISampleTrackingService
{
    Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId);
    Task<List<SampleTube>> GetTubesForVisitAsync(int visitId);
    Task UpdateTestStageAsync(int visitTestId, string newStage, int staffId);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IRoutineResultService
{
    Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId);

    Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId);

    Task SaveSingleComponentResultAsync(int visitTestId, int componentId, string? resultValue, int patientId, int staffId);

    Task<bool> TogglePrintStatusAsync(int visitTestId, int staffId);

    Task<bool> ToggleExportStatusAsync(int visitTestId, int staffId);

    Task<bool> ToggleReviewStatusAsync(int visitTestId, int staffId);
}

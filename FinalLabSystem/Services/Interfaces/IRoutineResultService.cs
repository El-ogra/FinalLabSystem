using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IRoutineResultService
{
    Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId);
    Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IVisitService
{
    Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges);
    Task CancelVisitTestAsync(int visitTestId);
    Task<Visit?> GetVisitSummaryAsync(int visitId);
}

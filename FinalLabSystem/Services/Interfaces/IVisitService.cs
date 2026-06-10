using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IVisitService
{
    Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges);
    Task<Visit> SavePatientVisitAsync(Patient patient, Visit visit, List<int> testTypeIds, decimal amountPaid, int staffId, List<PatientMedicalHistory> medicalHistories, ReferralSource? referralToSave);
    Task<VisitFullDto> GetVisitFullDataAsync(int visitId);
    Task<List<TodayPatientDto>> GetTodayPatientListAsync();
    Task<bool> CancelVisitAsync(int visitId);
    Task<List<Visit>> GetTodayVisitsWithPatientsAsync();
    Task UpdateVisitTestsAsync(int visitId, List<int> testTypeIds);
    Task<string> GenerateVisitCodeAsync();
    Task CancelVisitTestAsync(int visitTestId);
    Task<Visit?> GetVisitSummaryAsync(int visitId);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IVisitService
{
    /// <summary>
    /// Creates a visit with selected tests, profiles, and additional charges.
    /// </summary>
    /// <param name="visit">The visit header to create.</param>
    /// <param name="testIds">The selected test type identifiers.</param>
    /// <param name="profileIds">The selected profile identifiers.</param>
    /// <param name="charges">The additional visit charges.</param>
    /// <returns>The created visit.</returns>
    Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges);

    /// <summary>
    /// Saves a patient and visit together with tests, payment, medical history, and referral data.
    /// </summary>
    /// <param name="patient">The patient to create or update.</param>
    /// <param name="visit">The visit to save.</param>
    /// <param name="testTypeIds">The selected test type identifiers.</param>
    /// <param name="amountPaid">The amount paid at registration.</param>
    /// <param name="staffId">The staff member performing the save.</param>
    /// <param name="medicalHistories">The medical-history records to attach.</param>
    /// <param name="referralToSave">The optional referral source to save or link.</param>
    /// <returns>The saved visit.</returns>
    Task<Visit> SavePatientVisitAsync(Patient patient, Visit visit, List<int> testTypeIds, decimal amountPaid, int staffId, List<PatientMedicalHistory> medicalHistories, ReferralSource? referralToSave);

    /// <summary>
    /// Gets the complete visit data needed by visit workflows.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <returns>The full visit data transfer object.</returns>
    Task<VisitFullDto> GetVisitFullDataAsync(int visitId);

    /// <summary>
    /// Gets the patient list for visits registered today.
    /// </summary>
    /// <returns>The list of today's patient visit summaries.</returns>
    Task<List<TodayPatientDto>> GetTodayPatientListAsync();

    /// <summary>
    /// Cancels a visit.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <returns><c>true</c> when the visit was cancelled; otherwise, <c>false</c>.</returns>
    Task<bool> CancelVisitAsync(int visitId);

    /// <summary>
    /// Gets today's visits including patient information.
    /// </summary>
    /// <returns>The list of today's visits with patients.</returns>
    Task<List<Visit>> GetTodayVisitsWithPatientsAsync();

    /// <summary>
    /// Replaces the tests assigned to a visit.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <param name="testTypeIds">The test type identifiers that should remain assigned.</param>
    Task UpdateVisitTestsAsync(int visitId, List<int> testTypeIds);

    /// <summary>
    /// Generates the next visit code.
    /// </summary>
    /// <returns>A unique visit code.</returns>
    Task<string> GenerateVisitCodeAsync();

    /// <summary>
    /// Cancels a single test within a visit.
    /// </summary>
    /// <param name="visitTestId">The visit-test identifier.</param>
    Task CancelVisitTestAsync(int visitTestId);

    /// <summary>
    /// Gets a lightweight visit summary.
    /// </summary>
    /// <param name="visitId">The visit identifier.</param>
    /// <returns>The visit summary, or <c>null</c> when no visit exists.</returns>
    Task<Visit?> GetVisitSummaryAsync(int visitId);
}

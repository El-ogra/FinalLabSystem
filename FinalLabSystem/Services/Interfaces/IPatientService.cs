using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Services.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IPatientService
{
    /// <summary>
    /// Registers a new patient record.
    /// </summary>
    /// <param name="patient">The patient details to register.</param>
    /// <returns>The created patient record.</returns>
    Task<Patient> RegisterPatientAsync(Patient patient);

    /// <summary>
    /// Updates an existing patient record.
    /// </summary>
    /// <param name="patient">The patient record containing updated values.</param>
    /// <returns>The updated patient record.</returns>
    Task<Patient> UpdatePatientAsync(Patient patient);

    /// <summary>
    /// Gets a patient by identifier.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <returns>The matching patient, or <c>null</c> when no patient exists.</returns>
    Task<Patient?> GetPatientByIdAsync(int patientId);

    /// <summary>
    /// Generates the next patient code.
    /// </summary>
    /// <returns>A unique patient code.</returns>
    Task<string> GeneratePatientCodeAsync();

    /// <summary>
    /// Gets visits registered for the current day with patient details.
    /// </summary>
    /// <returns>The list of today's visits.</returns>
    Task<List<Visit>> GetTodayPatientsAsync();

    /// <summary>
    /// Gets the configured patient title values.
    /// </summary>
    /// <returns>The available patient titles.</returns>
    Task<List<string>> GetPatientTitlesAsync();

    /// <summary>
    /// Searches patients by name, phone, code, or other supported text fields.
    /// </summary>
    /// <param name="searchTerm">The search text.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of patients to return.</param>
    /// <returns>A paged result containing matching patients.</returns>
    Task<PagedResult<Patient>> SearchPatientsAsync(string searchTerm, int page = 1, int pageSize = 50);

    /// <summary>
    /// Adds a medical-history entry for a patient.
    /// </summary>
    /// <param name="history">The medical-history record to add.</param>
    Task AddMedicalHistoryAsync(PatientMedicalHistory history);

    /// <summary>
    /// Gets the active medical-history entries for a patient.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <returns>The active medical-history records.</returns>
    Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId);
}

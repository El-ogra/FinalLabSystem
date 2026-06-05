using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IPatientService
{
    Task<Patient> RegisterPatientAsync(Patient patient);
    Task<Patient> UpdatePatientAsync(Patient patient);
    Task<Patient?> GetPatientByIdAsync(int patientId);
    Task<string> GeneratePatientCodeAsync();
    Task<List<Visit>> GetTodayPatientsAsync();
    Task<List<string>> GetPatientTitlesAsync();
    Task<List<Patient>> SearchPatientsAsync(string searchTerm);
    Task AddMedicalHistoryAsync(PatientMedicalHistory history);
    Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId);
}

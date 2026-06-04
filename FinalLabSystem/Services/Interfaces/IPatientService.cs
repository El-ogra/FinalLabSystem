using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IPatientService
{
    Task<Patient> RegisterPatientAsync(Patient patient);
    Task<List<Patient>> SearchPatientsAsync(string searchTerm);
    Task AddMedicalHistoryAsync(PatientMedicalHistory history);
    Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId);
}

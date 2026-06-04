using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly FinalLabDbContext _context;

    public PatientService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<Patient> RegisterPatientAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.FullNameAr.Contains(searchTerm)
                || (p.FullNameEn != null && p.FullNameEn.Contains(searchTerm))
                || p.PatientCode.Contains(searchTerm)
                || (p.Phone != null && p.Phone.Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task AddMedicalHistoryAsync(PatientMedicalHistory history)
    {
        _context.PatientMedicalHistories.Add(history);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId)
    {
        return await _context.PatientMedicalHistories
            .Where(h => h.PatientId == patientId && h.IsActive)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}

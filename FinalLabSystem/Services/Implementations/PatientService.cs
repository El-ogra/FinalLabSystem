using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(FinalLabDbContext context, ILogger<PatientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Patient> RegisterPatientAsync(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.PatientCode))
            patient.PatientCode = await GeneratePatientCodeAsync();

        if (string.IsNullOrWhiteSpace(patient.PatientType))
            patient.PatientType = "Individual";

        patient.CreatedAt = patient.CreatedAt == default ? DateTime.UtcNow : patient.CreatedAt;
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> UpdatePatientAsync(Patient patient)
    {
        var existing = await _context.Patients.FindAsync(patient.PatientId);
        if (existing is null)
            throw new InvalidOperationException($"Patient with ID {patient.PatientId} was not found.");

        existing.PatientCode = patient.PatientCode;
        existing.NationalId = patient.NationalId;
        existing.Title = patient.Title;
        existing.FullNameAr = patient.FullNameAr;
        existing.FullNameEn = patient.FullNameEn;
        existing.Sex = patient.Sex;
        existing.DateOfBirth = patient.DateOfBirth;
        existing.ApproxAge = patient.ApproxAge;
        existing.ApproxAgeUnit = patient.ApproxAgeUnit;
        existing.Phone = patient.Phone;
        existing.Phone2 = patient.Phone2;
        existing.Email = patient.Email;
        existing.Address = patient.Address;
        existing.BloodType = patient.BloodType;
        existing.Notes = patient.Notes;
        existing.IsVip = patient.IsVip;
        existing.PatientType = string.IsNullOrWhiteSpace(patient.PatientType) ? "Individual" : patient.PatientType;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        return await _context.Patients
            .Include(p => p.PatientMedicalHistories)
            .Include(p => p.Visits)
                .ThenInclude(v => v.VisitTests)
                    .ThenInclude(vt => vt.Testtype)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<string> GeneratePatientCodeAsync()
    {
        var todayPrefix = $"P{DateTime.UtcNow:yyyyMMdd}";
        var lastCode = await _context.Patients
            .Where(p => p.PatientCode.StartsWith(todayPrefix))
            .OrderByDescending(p => p.PatientCode)
            .Select(p => p.PatientCode)
            .FirstOrDefaultAsync();

        var next = 1;
        if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.Length > todayPrefix.Length)
        {
            var suffix = lastCode[todayPrefix.Length..];
            if (int.TryParse(suffix, out var parsed))
                next = parsed + 1;
        }

        string candidate;
        do
        {
            candidate = $"{todayPrefix}{next:0000}";
            next++;
        }
        while (await _context.Patients.AnyAsync(p => p.PatientCode == candidate));

        return candidate;
    }

    public async Task<List<Visit>> GetTodayPatientsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.Testtype)
            .Where(v => v.VisitDate >= today && v.VisitDate < tomorrow)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<List<string>> GetPatientTitlesAsync()
    {
        return await _context.Patients
            .Where(p => p.Title != null && p.Title != "")
            .Select(p => p.Title!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await _context.Patients.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync();

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

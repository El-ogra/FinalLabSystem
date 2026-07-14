using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class BarcodeGenerator : IBarcodeGenerator
{
    private readonly FinalLabDbContext _context;

    public BarcodeGenerator(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateCaseCodeAsync(int visitId)
    {
        return await GenerateCodeAsync(visitId, BarcodeCodeType.Case);
    }

    public async Task<string> GenerateFileCodeAsync(int visitId)
    {
        return await GenerateCodeAsync(visitId, BarcodeCodeType.File);
    }

    public async Task<string> GetOrCreateLabIdAsync(int patientId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null)
            throw new ArgumentException($"Patient {patientId} not found");

        if (!string.IsNullOrEmpty(patient.LabId))
            return patient.LabId;

        var visit = await _context.Visits
            .Where(v => v.PatientId == patientId)
            .OrderBy(v => v.VisitDate)
            .FirstOrDefaultAsync();

        var visitDate = visit?.VisitDate ?? DateTime.Today;
        var labId = await GenerateLabIdAsync(patientId, visitDate);

        patient.LabId = labId;
        await _context.SaveChangesAsync();

        return labId;
    }

    private async Task<string> GenerateCodeAsync(int visitId, BarcodeCodeType codeType)
    {
        var visit = await _context.Visits
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit == null)
            throw new ArgumentException($"Visit {visitId} not found");

        var patientId = visit.PatientId;
        var visitDate = visit.VisitDate;
        var ordinal = await GetDailyOrdinalAsync(patientId, visitDate);

        return BuildBarcodeValue((byte)codeType, visitDate, ordinal);
    }

    private Task<string> GenerateLabIdAsync(int patientId, DateTime visitDate)
    {
        return Task.FromResult(BuildBarcodeValue((byte)BarcodeCodeType.Lab, visitDate, 0));
    }

    private string BuildBarcodeValue(byte typeDigit, DateTime date, int ordinal)
    {
        var datePart = date.ToString("yyMMdd");
        var weekday = (int)date.DayOfWeek + 1;
        var ordinalPart = ordinal.ToString("D4");
        var partial = $"{typeDigit}{datePart}{weekday}{ordinalPart}";
        var checkDigit = CalculateLuhnCheckDigit(partial);
        return $"{partial}{checkDigit}";
    }

    private async Task<int> GetDailyOrdinalAsync(int patientId, DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _context.PatientBarcodes
            .Where(pb => pb.PatientId == patientId
                      && pb.IssueDate >= dayStart
                      && pb.IssueDate < dayEnd
                      && pb.CodeType == BarcodeCodeType.Case)
            .CountAsync() + 1;
    }

    internal static int CalculateLuhnCheckDigit(string code)
    {
        int sum = 0;
        bool alternate = true;
        for (int i = code.Length - 1; i >= 0; i--)
        {
            int digit = code[i] - '0';
            if (alternate)
                digit *= 2;
            if (digit > 9)
                digit -= 9;
            sum += digit;
            alternate = !alternate;
        }
        int remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }
}

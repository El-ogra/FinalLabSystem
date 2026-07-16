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

    /// <summary>
    /// Builds a 12-digit barcode value with a Luhn check digit appended.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Barcode structure (post-VS-15): {weekday(1)}{date(6)}{ordinal(3)}{type(1)}{luhn(1)}
    /// </para>
    /// <para>
    /// Positions 0–10 form the partial barcode; position 11 is the Luhn check digit.
    /// The branch digit from the original reference has been removed (Decision 23).
    /// </para>
    /// </remarks>
    /// <param name="typeDigit">Type code: 1=Case, 3=File, 5=Lab.</param>
    /// <param name="date">The visit date (used to derive weekday and YYMMDD).</param>
    /// <param name="ordinal">Daily patient ordinal (zero-padded to 3 digits).</param>
    /// <returns>A 12-digit barcode string.</returns>
    internal string BuildBarcodeValue(byte typeDigit, DateTime date, int ordinal)
    {
        var datePart = date.ToString("yyMMdd");
        var weekday = (int)date.DayOfWeek + 1;
        var ordinalPart = ordinal.ToString("D3");
        var partial = $"{weekday}{datePart}{ordinalPart}{typeDigit}";
        var checkDigit = CalculateLuhnCheckDigit(partial);
        return $"{partial}{checkDigit}";
    }

    private async Task<int> GetDailyOrdinalAsync(int patientId, DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _context.PatientBarcodes
            .Where(pb => pb.IssueDate >= dayStart
                      && pb.IssueDate < dayEnd
                      && pb.CodeType == BarcodeCodeType.Case)
            .CountAsync() + 1;
    }

    /// <summary>
    /// Calculates the Luhn check digit for a barcode string.
    /// </summary>
    /// <remarks>
    /// <para><strong>INTENTIONAL DEVIATION — NOT in original Real Lab System reference.</strong></para>
    /// <para>
    /// The Luhn check digit is an added data-integrity layer. The original Real Lab System
    /// uses a 12-digit barcode without any check digit. This implementation appends a 13th
    /// digit computed via the Luhn algorithm (mod-10) to detect single-digit transcription
    /// errors and adjacent transpositions.
    /// </para>
    /// <para>
    /// Current barcode structure (post-VS-15, 12 digits before Luhn):
    /// <list type="number">
    ///   <item>Position 0: Weekday (1=Saturday..7=Friday)</item>
    ///   <item>Positions 1–6: Date YYMMDD</item>
    ///   <item>Positions 7–9: Daily patient ordinal (D3, max 999)</item>
    ///   <item>Position 10: Type code (1=Case, 3=File, 5=Lab)</item>
    /// </list>
    /// The Luhn check digit is appended at position 11, yielding a 12-digit final barcode.
    /// </para>
    /// <para>
    /// Other intentional deviations from the reference (Decision 23): the branch digit
    /// has been permanently removed because this installation serves a single lab facility.
    /// </para>
    /// </remarks>
    /// <param name="code">The partial barcode string (11 digits, positions 0–10) to compute the check digit for.</param>
    /// <returns>A single digit (0–9) that should be appended to <paramref name="code"/> to form a valid barcode.</returns>
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

using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IReceiptService
{
    /// <summary>
    /// Determines whether a receipt can be printed for the given visit.
    /// Admins can always print (unlimited reprints).
    /// Regular staff can only print once per financial state.
    /// </summary>
    Task<bool> CanPrintReceiptAsync(int visitId, int staffId);

    /// <summary>
    /// Logs a receipt print event with financial snapshot.
    /// </summary>
    Task LogPrintEventAsync(ReceiptPrintLog logEntry);

    /// <summary>
    /// Gets the last receipt print log for a visit.
    /// </summary>
    Task<ReceiptPrintLog?> GetLastPrintLogAsync(int visitId);

    /// <summary>
    /// Groups visit tests by their TestGroup, applying the grouping rule:
    /// If ALL tests in a group are present → summarized single line.
    /// If only SOME tests are present → individual lines per test.
    /// </summary>
    Task<List<ReceiptGroupedTest>> GetGroupedTestsForReceiptAsync(int visitId);
}

public sealed class ReceiptGroupedTest
{
    public string GroupName { get; set; } = string.Empty;

    public int TestCount { get; set; }

    public decimal TotalPrice { get; set; }

    public bool IsSummarized { get; set; }

    public string? DetailLine { get; set; }
}

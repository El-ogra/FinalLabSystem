namespace FinalLabSystem.Services.Interfaces;

/// <summary>
/// Abstraction for document printing and export operations.
/// Replace <see cref="FinalLabSystem.Services.Implementations.NullPrintService"/> with a real implementation when printing is required.
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Prints or exports a document of the specified type.
    /// </summary>
    /// <param name="documentType">
    /// The document template identifier, e.g. "VisitReceipt", "TestResult", "DailyReport".
    /// </param>
    /// <param name="data">The data object to render into the document.</param>
    Task PrintAsync(string documentType, object data);
}

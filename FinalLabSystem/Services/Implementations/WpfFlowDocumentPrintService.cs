using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Services.Printing;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class WpfFlowDocumentPrintService : IPrintService
{
    private readonly ILogger<WpfFlowDocumentPrintService> _logger;
    private readonly IFeatureToggleService _featureToggleService;

    public WpfFlowDocumentPrintService(
        ILogger<WpfFlowDocumentPrintService> logger,
        IFeatureToggleService featureToggleService)
    {
        _logger = logger;
        _featureToggleService = featureToggleService;
    }

    public async Task PrintAsync(string documentType, object data)
    {
        if (string.IsNullOrEmpty(documentType))
            throw new ArgumentNullException(nameof(documentType));
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var enablePrinting = await _featureToggleService.IsEnabledAsync("EnableServerPrinting", false);
        if (!enablePrinting)
        {
            _logger.LogInformation("Printing disabled by EnableServerPrinting toggle. Document type: {DocType}", documentType);
            return;
        }

        DocumentTemplateBase template = documentType switch
        {
            "ResultReport" => new ResultReportTemplate(),
            "Receipt" => new ReceiptTemplate(),
            "CompositeReport" => new CompositeReportTemplate(),
            "Worksheet" => new WorksheetTemplate(),
            "Envelope" => new EnvelopeTemplate(),
            "MedicalHistory" => new MedicalHistoryTemplate(),
            "BlankReport" => new BlankReportTemplate(),
            "CashDrawerSummary" => new CashDrawerSummaryTemplate(),
            "CommissionReport" => new CommissionReportTemplate(),
            "OutstandingBalance" => new OutstandingBalanceReportTemplate(),
            _ => throw new NotSupportedException($"Document type '{documentType}' is not supported.")
        };

        var document = template.BuildDocument(data);

        await PrintDocumentAsync(documentType, document);
    }

    protected virtual async Task PrintDocumentAsync(string documentType, FlowDocument document)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                var docSource = (IDocumentPaginatorSource)document;
                dlg.PrintDocument(docSource.DocumentPaginator, documentType);
                _logger.LogInformation("Printed document: {DocType}", documentType);
            }
        });
    }
}

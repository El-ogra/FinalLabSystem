using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Views.Patients;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class ReceiptDialogViewModel : ViewModelBase
{
    private string _selectedTemplate = "Detailed";
    private VisitFullDto? _visitData;

    public ReceiptDialogViewModel()
    {
        Templates = new ObservableCollection<string> { "Detailed", "Summary" };
        PreviewAndPrintCommand = new RelayCommand(_ => PreviewAndPrint());
    }

    public ObservableCollection<string> Templates { get; }

    public string SelectedTemplate
    {
        get => _selectedTemplate;
        set => SetProperty(ref _selectedTemplate, string.IsNullOrWhiteSpace(value) ? "Detailed" : value);
    }

    public VisitFullDto? VisitData
    {
        get => _visitData;
        set => SetProperty(ref _visitData, value);
    }

    public ICommand PreviewAndPrintCommand { get; }

    public void Initialize(VisitFullDto dto)
    {
        VisitData = dto;
    }

    public void LoadVisit(Visit visit)
    {
        VisitData = new VisitFullDto
        {
            VisitId = visit.VisitId,
            PatientCode = visit.Patient.PatientCode,
            FullNameAr = visit.Patient.FullNameAr,
            EntryDate = visit.VisitDate,
            Subtotal = Convert.ToDecimal(visit.Subtotal),
            DiscountAmount = Convert.ToDecimal(visit.DiscountAmount),
            TotalAfterDiscount = Convert.ToDecimal(visit.TotalAfterDiscount),
            TotalPaid = Convert.ToDecimal(visit.TotalPaid),
            BalanceDue = Convert.ToDecimal(visit.BalanceDue),
            SelectedTests = visit.VisitTests
                .Select(vt => new SelectedTestDto
                {
                    TestTypeId = vt.TesttypeId,
                    TestCode = vt.Testtype.TypeCode,
                    TestName = vt.Testtype.TypeNameAr ?? vt.Testtype.TypeNameEn,
                    BillNameLine1 = vt.Testtype.BillNameLine1,
                    BillNameLine2 = vt.Testtype.BillNameLine2,
                    Price = Convert.ToDecimal(vt.PriceCharged),
                    SampleType = vt.Testtype.SampleType
                })
                .ToList()
        };
    }

    private void PreviewAndPrint()
    {
        if (VisitData is null)
            return;

        var document = SelectedTemplate == "Summary"
            ? CreateSummaryDocument(VisitData)
            : CreateDetailedDocument(VisitData);

        var preview = new PrintPreviewWindow(document)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        };
        preview.ShowDialog();
    }

    private static FlowDocument CreateDetailedDocument(VisitFullDto dto)
    {
        var document = CreateBaseDocument(dto, "إيصال مفصل");
        var table = new Table();
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });

        var group = new TableRowGroup();
        group.Rows.Add(CreateRow("التحليل", "السعر", true));
        foreach (var row in BuildReceiptRows(dto.SelectedTests))
            group.Rows.Add(CreateRow(row.Name, row.Price.ToString("N2"), false, row.SubLine));

        table.RowGroups.Add(group);
        document.Blocks.Add(table);
        AddFinancialLines(document, dto);
        return document;
    }

    private static FlowDocument CreateSummaryDocument(VisitFullDto dto)
    {
        var document = CreateBaseDocument(dto, "إيصال مختصر");
        AddFinancialLines(document, dto);
        return document;
    }

    private static FlowDocument CreateBaseDocument(VisitFullDto dto, string title)
    {
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(32)
        };

        document.Blocks.Add(new Paragraph(new Run(title))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });
        document.Blocks.Add(new Paragraph(new Run($"اسم المريض: {dto.FullNameAr}")));
        document.Blocks.Add(new Paragraph(new Run($"كود المريض: {dto.PatientCode}")));
        document.Blocks.Add(new Paragraph(new Run($"تاريخ الزيارة: {dto.EntryDate:yyyy-MM-dd HH:mm}")));
        return document;
    }

    private static void AddFinancialLines(FlowDocument document, VisitFullDto dto)
    {
        document.Blocks.Add(new Paragraph(new Run($"الإجمالي: {dto.Subtotal:N2}")));
        document.Blocks.Add(new Paragraph(new Run($"الخصم: {dto.DiscountAmount:N2}")));
        document.Blocks.Add(new Paragraph(new Run($"المجموع بعد الخصم: {dto.TotalAfterDiscount:N2}")));
        document.Blocks.Add(new Paragraph(new Run($"المدفوع: {dto.TotalPaid:N2}")));
        document.Blocks.Add(new Paragraph(new Run($"المتبقي: {dto.BalanceDue:N2}")));
    }

    private static IEnumerable<(string Name, string? SubLine, decimal Price)> BuildReceiptRows(IEnumerable<SelectedTestDto> tests)
    {
        var materialized = tests.ToList();
        foreach (var group in materialized
                     .Where(t => !string.IsNullOrWhiteSpace(t.BillNameLine1))
                     .GroupBy(t => t.BillNameLine1!.Trim()))
        {
            yield return (
                group.Key,
                group.Select(t => t.BillNameLine2).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)),
                group.Sum(t => t.Price));
        }

        foreach (var test in materialized.Where(t => string.IsNullOrWhiteSpace(t.BillNameLine1)))
            yield return (test.TestName, null, test.Price);
    }

    private static TableRow CreateRow(string first, string second, bool isHeader, string? subLine = null)
    {
        var row = new TableRow();
        var firstParagraph = new Paragraph();
        firstParagraph.Inlines.Add(new Run(first));
        if (!string.IsNullOrWhiteSpace(subLine))
        {
            firstParagraph.Inlines.Add(new LineBreak());
            firstParagraph.Inlines.Add(new Run(subLine)
            {
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.DimGray
            });
        }

        row.Cells.Add(new TableCell(firstParagraph)
        {
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            BorderBrush = System.Windows.Media.Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(4)
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(second)))
        {
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            BorderBrush = System.Windows.Media.Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(4)
        });
        return row;
    }
}

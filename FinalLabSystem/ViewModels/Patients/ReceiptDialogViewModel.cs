using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class ReceiptDialogViewModel : ViewModelBase
{
    private readonly IReceiptService _receiptService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IDialogService _dialogService;
    private readonly IPrintPreviewDialogService _printPreviewDialogService;

    private VisitFullDto? _visitData;
    private string _selectedFormat = "A4";
    private bool _showBreakdown = true;
    private bool _canPrint;
    private List<ReceiptGroupedTest> _groupedTests = new();

    public ReceiptDialogViewModel(
        IReceiptService receiptService,
        ICurrentUserSession currentUserSession,
        IDialogService dialogService,
        IPrintPreviewDialogService printPreviewDialogService)
    {
        _receiptService = receiptService;
        _currentUserSession = currentUserSession;
        _dialogService = dialogService;
        _printPreviewDialogService = printPreviewDialogService;

        Formats = new ObservableCollection<string> { "A4", "Thermal" };
        PrintCommand = new AsyncRelayCommand(PrintAsync, () => CanPrint && VisitData is not null);
    }

    public ObservableCollection<string> Formats { get; }

    public string SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            if (SetProperty(ref _selectedFormat, value))
                OnPropertyChanged(nameof(IsA4Selected));
        }
    }

    public bool IsA4Selected => SelectedFormat == "A4";

    public bool ShowBreakdown
    {
        get => _showBreakdown;
        set => SetProperty(ref _showBreakdown, value);
    }

    public bool CanPrint
    {
        get => _canPrint;
        set
        {
            if (SetProperty(ref _canPrint, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public VisitFullDto? VisitData
    {
        get => _visitData;
        set => SetProperty(ref _visitData, value);
    }

    public List<ReceiptGroupedTest> GroupedTests
    {
        get => _groupedTests;
        set => SetProperty(ref _groupedTests, value);
    }

    public ICommand PrintCommand { get; }

    public async Task InitializeAsync(VisitFullDto dto)
    {
        VisitData = dto;

        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        CanPrint = await _receiptService.CanPrintReceiptAsync(dto.VisitId, staffId);

        GroupedTests = await _receiptService.GetGroupedTestsForReceiptAsync(dto.VisitId);
    }

    private async Task PrintAsync()
    {
        if (VisitData is null)
            return;

        var document = SelectedFormat == "Thermal"
            ? BuildThermalDocument(VisitData, GroupedTests, ShowBreakdown)
            : BuildA4Document(VisitData, GroupedTests, ShowBreakdown);

        _printPreviewDialogService.Show(document, "إيصال المريض");

        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        await _receiptService.LogPrintEventAsync(new ReceiptPrintLog
        {
            VisitId = VisitData.VisitId,
            StaffId = staffId,
            PrintedAt = DateTime.UtcNow,
            Format = SelectedFormat,
            ShowBreakdown = ShowBreakdown,
            Subtotal = VisitData.Subtotal,
            DiscountAmount = VisitData.DiscountAmount,
            TotalAfterDiscount = VisitData.TotalAfterDiscount,
            TotalPaid = VisitData.TotalPaid,
            BalanceDue = VisitData.BalanceDue
        });

        CanPrint = await _receiptService.CanPrintReceiptAsync(VisitData.VisitId, staffId);
    }

    private static FlowDocument BuildA4Document(
        VisitFullDto dto,
        List<ReceiptGroupedTest> groupedTests,
        bool showBreakdown)
    {
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(32)
        };

        document.Blocks.Add(new Paragraph(new Run("ايصال المعمل"))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });

        document.Blocks.Add(new Paragraph(new Run($"طباعه: {DateTime.Now:yyyy-MM-dd HH:mm}"))
        {
            FontSize = 10,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center
        });

        AddPatientInfoSection(document, dto);

        if (showBreakdown && groupedTests.Count > 0)
        {
            AddBreakdownTable(document, groupedTests);
        }

        AddFinancialSummary(document, dto);

        return document;
    }

    private static FlowDocument BuildThermalDocument(
        VisitFullDto dto,
        List<ReceiptGroupedTest> groupedTests,
        bool showBreakdown)
    {
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Courier New"),
            FontSize = 11,
            PagePadding = new Thickness(8),
            ColumnWidth = 280
        };

        document.Blocks.Add(new Paragraph(new Run("═══════════════════════"))
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 9
        });

        document.Blocks.Add(new Paragraph(new Run("ايصال المعمل"))
        {
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });

        document.Blocks.Add(new Paragraph(new Run($"{DateTime.Now:dd/MM/yyyy HH:mm}"))
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 9
        });

        document.Blocks.Add(new Paragraph(new Run("═══════════════════════"))
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 9
        });

        AddThermalPatientInfo(document, dto);

        if (showBreakdown && groupedTests.Count > 0)
        {
            AddThermalBreakdown(document, groupedTests);
        }

        AddThermalFinancialSummary(document, dto);

        document.Blocks.Add(new Paragraph(new Run("═══════════════════════"))
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 9
        });

        return document;
    }

    private static void AddPatientInfoSection(FlowDocument document, VisitFullDto dto)
    {
        document.Blocks.Add(new Paragraph(new Run($"المريض: {dto.FullNameAr}"))
        {
            FontSize = 14
        });
        document.Blocks.Add(new Paragraph(new Run($"الكود: {dto.PatientCode}")));
        document.Blocks.Add(new Paragraph(new Run($"التاريخ: {dto.EntryDate:yyyy-MM-dd HH:mm}")));

        if (!string.IsNullOrWhiteSpace(dto.Sex) && dto.Sex != "U")
        {
            var sexText = dto.Sex == "M" ? "ذكر" : "انثى";
            var ageText = dto.ApproxAge.HasValue ? $" - {dto.ApproxAge} {dto.ApproxAgeUnit}" : "";
            document.Blocks.Add(new Paragraph(new Run($"النوع: {sexText}{ageText}")));
        }
    }

    private static void AddBreakdownTable(FlowDocument document, List<ReceiptGroupedTest> tests)
    {
        document.Blocks.Add(new Paragraph(new Run("───────────────────────"))
        {
            FontSize = 9,
            Foreground = Brushes.Gray
        });

        var table = new Table();
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(60) });
        table.Columns.Add(new TableColumn { Width = new GridLength(90) });

        var headerGroup = new TableRowGroup();
        headerGroup.Rows.Add(CreateTableRow("التحليل", "العدد", "السعر", true));
        table.RowGroups.Add(headerGroup);

        var bodyGroup = new TableRowGroup();
        foreach (var test in tests)
        {
            if (test.IsSummarized)
            {
                var row = CreateTableRow(
                    test.GroupName,
                    test.TestCount.ToString(),
                    test.TotalPrice.ToString("N2"),
                    false);

                if (!string.IsNullOrWhiteSpace(test.DetailLine))
                {
                    var firstCell = row.Cells[0];
                    var para = firstCell.Blocks.FirstBlock as Paragraph;
                    if (para is not null)
                    {
                        para.Inlines.Add(new LineBreak());
                        para.Inlines.Add(new Run(test.DetailLine)
                        {
                            FontSize = 10,
                            Foreground = Brushes.DimGray
                        });
                    }
                }

                bodyGroup.Rows.Add(row);
            }
            else
            {
                bodyGroup.Rows.Add(CreateTableRow(
                    test.GroupName,
                    test.TestCount.ToString(),
                    test.TotalPrice.ToString("N2"),
                    false));
            }
        }
        table.RowGroups.Add(bodyGroup);

        document.Blocks.Add(table);

        document.Blocks.Add(new Paragraph(new Run("───────────────────────"))
        {
            FontSize = 9,
            Foreground = Brushes.Gray
        });
    }

    private static void AddFinancialSummary(FlowDocument document, VisitFullDto dto)
    {
        var table = new Table();
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });

        var group = new TableRowGroup();
        group.Rows.Add(CreateFinancialRow("الاجمالي", dto.Subtotal.ToString("N2")));
        group.Rows.Add(CreateFinancialRow("الخصم", dto.DiscountAmount.ToString("N2")));
        group.Rows.Add(CreateFinancialRow("الاجمالي بعد الخصم", dto.TotalAfterDiscount.ToString("N2")));
        group.Rows.Add(CreateFinancialRow("المدفوع", dto.TotalPaid.ToString("N2")));
        group.Rows.Add(CreateFinancialRow("المتبقي", dto.BalanceDue.ToString("N2"), true));

        table.RowGroups.Add(group);
        document.Blocks.Add(table);
    }

    private static void AddThermalPatientInfo(FlowDocument document, VisitFullDto dto)
    {
        document.Blocks.Add(new Paragraph(new Run($"المريض: {dto.FullNameAr}") { FontSize = 11 }));
        document.Blocks.Add(new Paragraph(new Run($"الكود: {dto.PatientCode}") { FontSize = 10 }));
        document.Blocks.Add(new Paragraph(new Run($"التاريخ: {dto.EntryDate:dd/MM/yyyy HH:mm}") { FontSize = 10 }));
    }

    private static void AddThermalBreakdown(FlowDocument document, List<ReceiptGroupedTest> tests)
    {
        document.Blocks.Add(new Paragraph(new Run("───────────────────────"))
        {
            FontSize = 9
        });

        foreach (var test in tests)
        {
            if (test.IsSummarized)
            {
                var line = $"{test.GroupName} ({test.TestCount})";
                var para = new Paragraph();
                para.Inlines.Add(new Run(line) { FontSize = 10, FontWeight = FontWeights.Bold });
                para.Inlines.Add(new Run($"  {test.TotalPrice:N2}") { FontSize = 10 });
                document.Blocks.Add(para);

                if (!string.IsNullOrWhiteSpace(test.DetailLine))
                {
                    document.Blocks.Add(new Paragraph(
                        new Run($"  >> {test.DetailLine}") { FontSize = 9, Foreground = Brushes.DimGray }));
                }
            }
            else
            {
                var para = new Paragraph();
                para.Inlines.Add(new Run(test.GroupName) { FontSize = 10 });
                para.Inlines.Add(new Run($"  {test.TotalPrice:N2}") { FontSize = 10 });
                document.Blocks.Add(para);
            }
        }

        document.Blocks.Add(new Paragraph(new Run("───────────────────────"))
        {
            FontSize = 9
        });
    }

    private static void AddThermalFinancialSummary(FlowDocument document, VisitFullDto dto)
    {
        var p1 = new Paragraph();
        p1.Inlines.Add(new Run("الاجمالي: ") { FontSize = 10 });
        p1.Inlines.Add(new Run($"{dto.Subtotal:N2}") { FontSize = 10, FontWeight = FontWeights.Bold });
        document.Blocks.Add(p1);

        var p2 = new Paragraph();
        p2.Inlines.Add(new Run("الخصم: ") { FontSize = 10 });
        p2.Inlines.Add(new Run($"{dto.DiscountAmount:N2}") { FontSize = 10 });
        document.Blocks.Add(p2);

        var p3 = new Paragraph();
        p3.Inlines.Add(new Run("الاجمالي بعد الخصم: ") { FontSize = 10, FontWeight = FontWeights.Bold });
        p3.Inlines.Add(new Run($"{dto.TotalAfterDiscount:N2}") { FontSize = 10, FontWeight = FontWeights.Bold });
        document.Blocks.Add(p3);

        var p4 = new Paragraph();
        p4.Inlines.Add(new Run("المدفوع: ") { FontSize = 10 });
        p4.Inlines.Add(new Run($"{dto.TotalPaid:N2}") { FontSize = 10 });
        document.Blocks.Add(p4);

        var p5 = new Paragraph();
        p5.Inlines.Add(new Run("المتبقي: ") { FontSize = 10 });
        p5.Inlines.Add(new Run($"{dto.BalanceDue:N2}") { FontSize = 10, FontWeight = dto.BalanceDue > 0 ? FontWeights.Bold : FontWeights.Normal });
        document.Blocks.Add(p5);
    }

    private static TableRow CreateTableRow(string col1, string col2, string col3, bool isHeader)
    {
        var row = new TableRow();
        var fontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal;

        row.Cells.Add(new TableCell(new Paragraph(new Run(col1)))
        {
            FontWeight = fontWeight,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(4)
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(col2)))
        {
            FontWeight = fontWeight,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(4),
            TextAlignment = TextAlignment.Center
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(col3)))
        {
            FontWeight = fontWeight,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(4),
            TextAlignment = TextAlignment.Left
        });
        return row;
    }

    private static TableRow CreateFinancialRow(string label, string value, bool isBold = false)
    {
        var row = new TableRow();
        var fontWeight = isBold ? FontWeights.Bold : FontWeights.Normal;

        row.Cells.Add(new TableCell(new Paragraph(new Run(label)))
        {
            FontWeight = fontWeight,
            Padding = new Thickness(4)
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(value)))
        {
            FontWeight = fontWeight,
            Padding = new Thickness(4),
            TextAlignment = TextAlignment.Left
        });
        return row;
    }
}

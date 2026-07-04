using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class BarcodeDialogViewModel : ViewModelBase
{
    private readonly ISampleTrackingService _sampleTrackingService;
    private readonly ILabelPrintService _labelPrintService;
    private readonly IInventoryService _inventoryService;
    private readonly IDialogService _dialogService;
    private readonly FinalLabDbContext _context;
    private int _visitId;
    private int _patientId;
    private BarcodeLabel? _selectedLabel;

    public BarcodeDialogViewModel(
        ISampleTrackingService sampleTrackingService,
        ILabelPrintService labelPrintService,
        IInventoryService inventoryService,
        IDialogService dialogService,
        FinalLabDbContext context)
    {
        _sampleTrackingService = sampleTrackingService;
        _labelPrintService = labelPrintService;
        _inventoryService = inventoryService;
        _dialogService = dialogService;
        _context = context;
        Labels = new ObservableCollection<BarcodeLabel>();
        CaseLabels = new ObservableCollection<BarcodePatientLabel>();
        FileLabels = new ObservableCollection<BarcodePatientLabel>();
        LabIdLabels = new ObservableCollection<BarcodePatientLabel>();
        PrintBarcodeCommand = new AsyncRelayCommand<BarcodeLabel>(parameter => PrintLabelAsync(parameter));
        PrintAllCommand = new AsyncRelayCommand(_ => PrintAllAsync());
        PrintCaseLabelsCommand = new AsyncRelayCommand(_ => PrintCaseLabelsAsync());
        PrintFileLabelsCommand = new AsyncRelayCommand(_ => PrintFileLabelsAsync());
        PrintLabIdCommand = new AsyncRelayCommand(_ => PrintLabIdAsync());
    }

    public ObservableCollection<BarcodeLabel> Labels { get; }
    public ObservableCollection<BarcodePatientLabel> CaseLabels { get; }
    public ObservableCollection<BarcodePatientLabel> FileLabels { get; }
    public ObservableCollection<BarcodePatientLabel> LabIdLabels { get; }

    public int VisitId
    {
        get => _visitId;
        private set => SetProperty(ref _visitId, value);
    }

    public BarcodeLabel? SelectedLabel
    {
        get => _selectedLabel;
        set => SetProperty(ref _selectedLabel, value);
    }

    public ICommand PrintBarcodeCommand { get; }
    public ICommand PrintAllCommand { get; }
    public ICommand PrintCaseLabelsCommand { get; }
    public ICommand PrintFileLabelsCommand { get; }
    public ICommand PrintLabIdCommand { get; }

    public async Task LoadBarcodesAsync(int visitId, int patientId)
    {
        VisitId = visitId;
        _patientId = patientId;

        await LoadTubesAsync(visitId);

        var barcodes = await _context.PatientBarcodes
            .Where(pb => pb.VisitId == visitId
                      || (pb.CodeType == BarcodeCodeType.Lab
                          && pb.PatientId == patientId))
            .ToListAsync();

        CaseLabels.Clear();
        FileLabels.Clear();
        LabIdLabels.Clear();

        foreach (var barcode in barcodes)
        {
            var label = ProjectBarcodePatientLabel(barcode);
            switch (barcode.CodeType)
            {
                case BarcodeCodeType.Case:
                    CaseLabels.Add(label);
                    break;
                case BarcodeCodeType.File:
                    FileLabels.Add(label);
                    break;
                case BarcodeCodeType.Lab:
                    LabIdLabels.Add(label);
                    break;
            }
        }
    }

    public async Task LoadTubesAsync(int visitId)
    {
        VisitId = visitId;
        var tubes = await _sampleTrackingService.GetTubesForVisitAsync(visitId);
        Labels.Clear();
        foreach (var tube in tubes)
            Labels.Add(ProjectLabel(tube));
    }

    private async Task PrintLabelAsync(BarcodeLabel? label)
    {
        if (label is null)
            return;

        await CheckStockAndWarnAsync(label.SourceTube.TubeType);
        await _labelPrintService.PrintLabelsAsync(new[] { label });
    }

    private async Task PrintAllAsync()
    {
        if (Labels.Count == 0)
            return;

        await CheckStockAndWarnForAllAsync();
        await _labelPrintService.PrintLabelsAsync(Labels);
    }

    private async Task PrintCaseLabelsAsync()
    {
        if (CaseLabels.Count == 0) return;
        var labels = CaseLabels.ToList();
        await _labelPrintService.PrintLabelsAsync(labels.Select(l => l.ToBarcodeLabel()).ToList());
    }

    private async Task PrintFileLabelsAsync()
    {
        if (FileLabels.Count == 0) return;
        var labels = FileLabels.ToList();
        await _labelPrintService.PrintLabelsAsync(labels.Select(l => l.ToBarcodeLabel()).ToList());
    }

    private async Task PrintLabIdAsync()
    {
        if (LabIdLabels.Count == 0) return;
        var labels = LabIdLabels.ToList();
        await _labelPrintService.PrintLabelsAsync(labels.Select(l => l.ToBarcodeLabel()).ToList());
    }

    private async Task CheckStockAndWarnAsync(string tubeType)
    {
        var material = await _inventoryService.GetByTubeTypeAsync(tubeType);
        if (material is not null && material.MinimumStock > 0 && material.CurrentStock <= material.MinimumStock)
        {
            _dialogService.ShowWarning(
                $"تنبيه: مخزون '{material.MaterialNameAr ?? material.MaterialName}' منخفض ({material.CurrentStock} متبقي من {material.MinimumStock}). يُرجى الشراء.",
                "تنبيه مخزون منخفض");
        }
    }

    private async Task CheckStockAndWarnForAllAsync()
    {
        var lowStockMessages = new List<string>();
        foreach (var label in Labels)
        {
            var material = await _inventoryService.GetByTubeTypeAsync(label.SourceTube.TubeType);
            if (material is not null && material.MinimumStock > 0 && material.CurrentStock <= material.MinimumStock)
            {
                lowStockMessages.Add(
                    $"'{material.MaterialNameAr ?? material.MaterialName}' ({material.CurrentStock} متبقي من {material.MinimumStock})");
            }
        }

        if (lowStockMessages.Count > 0)
        {
            _dialogService.ShowWarning(
                $"تنبيه: المخزون منخفض للأنواع التالية:\n{string.Join("\n", lowStockMessages)}\n\nيُرجى الشراء.",
                "تنبيه مخزون منخفض");
        }
    }

    private static BarcodeLabel ProjectLabel(SampleTube tube)
    {
        var patient = tube.Visit?.Patient;
        var sexLine = (patient?.Sex) switch
        {
            "M" => "Male",
            "F" => "Female",
            _ => ""
        };

        if (patient is not null && patient.ApproxAge.HasValue && !string.IsNullOrWhiteSpace(patient.ApproxAgeUnit))
        {
            if (sexLine.Length > 0)
                sexLine += " - ";
            sexLine += $"{patient.ApproxAge} {patient.ApproxAgeUnit}";
        }

        var testCodes = string.Join(", ", tube.VisitTests
            .Select(vt => ResolveAbbreviation(vt.Testtype))
            .Where(c => !string.IsNullOrWhiteSpace(c)));

        var patientCode = patient?.PatientCode ?? string.Empty;
        var tubeName = tube.TubeType;

        return new BarcodeLabel
        (
            patient?.FullNameAr ?? string.Empty,
            sexLine,
            testCodes,
            tube.BarcodeValue,
            patientCode,
            tubeName,
            tube
        );
    }

    private static BarcodePatientLabel ProjectBarcodePatientLabel(PatientBarcode barcode)
    {
        var patient = barcode.Patient;
        var sexLine = (patient?.Sex) switch
        {
            "M" => "Male",
            "F" => "Female",
            _ => ""
        };

        if (patient is not null && patient.ApproxAge.HasValue && !string.IsNullOrWhiteSpace(patient.ApproxAgeUnit))
        {
            if (sexLine.Length > 0)
                sexLine += " - ";
            sexLine += $"{patient.ApproxAge} {patient.ApproxAgeUnit}";
        }

        var typeName = barcode.CodeType switch
        {
            BarcodeCodeType.Case => "كود الحالة (Case)",
            BarcodeCodeType.File => "كود الملف (File)",
            BarcodeCodeType.Lab => "كود المعمل (Lab ID)",
            _ => ""
        };

        return new BarcodePatientLabel
        (
            patient?.FullNameAr ?? string.Empty,
            sexLine,
            typeName,
            barcode.BarcodeValue,
            patient?.PatientCode ?? string.Empty
        );
    }

    private static string ResolveAbbreviation(TestType test)
    {
        if (!string.IsNullOrWhiteSpace(test.TypeCode))
            return test.TypeCode;
        if (!string.IsNullOrWhiteSpace(test.TypeAbbrev))
            return test.TypeAbbrev;
        if (!string.IsNullOrWhiteSpace(test.TypeNameEn))
            return test.TypeNameEn;
        return string.Empty;
    }
}

public sealed record BarcodeLabel(
    string PatientNameAr,
    string SexAgeLine,
    string TestCodesLine,
    string BarcodePayload,
    string PatientIdentifierLine,
    string TubeName,
    SampleTube SourceTube
);

public sealed record BarcodePatientLabel(
    string PatientNameAr,
    string SexAgeLine,
    string TypeName,
    string BarcodePayload,
    string PatientIdentifierLine
)
{
    public BarcodeLabel ToBarcodeLabel()
    {
        var dummyTube = new SampleTube
        {
            TubeId = 0,
            VisitId = 0,
            TubeType = TypeName,
            BarcodeValue = BarcodePayload
        };
        return new BarcodeLabel(PatientNameAr, SexAgeLine, TypeName, BarcodePayload, PatientIdentifierLine, TypeName, dummyTube);
    }
}

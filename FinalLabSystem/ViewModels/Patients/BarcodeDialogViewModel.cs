using System;
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
    private readonly ISettingsService _settingsService;
    private readonly FinalLabDbContext _context;
    private int _visitId;
    private int _patientId;
    private BarcodeLabel? _selectedLabel;
    private bool _includeLabIdInPrintAll;
    private double _previewOffsetX;
    private double _previewOffsetY;
    private string _newExtraHeader = string.Empty;
    private string _newExtraDescription = string.Empty;
    private BarcodeLabel? _draggedLabel;

    public BarcodeDialogViewModel(
        ISampleTrackingService sampleTrackingService,
        ILabelPrintService labelPrintService,
        IInventoryService inventoryService,
        IDialogService dialogService,
        ISettingsService settingsService,
        FinalLabDbContext context)
    {
        _sampleTrackingService = sampleTrackingService;
        _labelPrintService = labelPrintService;
        _inventoryService = inventoryService;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _context = context;
        Labels = new ObservableCollection<BarcodeLabel>();
        CaseLabels = new ObservableCollection<BarcodePatientLabel>();
        FileLabels = new ObservableCollection<BarcodePatientLabel>();
        LabIdLabels = new ObservableCollection<BarcodePatientLabel>();
        ExtraLabels = new ObservableCollection<ExtraBarcodeLabel>();
        PrintBarcodeCommand = new AsyncRelayCommand<BarcodeLabel>(parameter => PrintLabelAsync(parameter));
        PrintAllCommand = new AsyncRelayCommand(_ => PrintAllAsync());
        AddExtraLabelCommand = new AsyncRelayCommand(_ => { AddExtraLabel(); return Task.CompletedTask; });
        RemoveExtraLabelCommand = new AsyncRelayCommand<ExtraBarcodeLabel>(parameter => { RemoveExtraLabel(parameter); return Task.CompletedTask; });
        PrintExtraLabelCommand = new AsyncRelayCommand<ExtraBarcodeLabel>(parameter => PrintExtraLabelAsync(parameter));
        RemoveTestFromLabelCommand = new AsyncRelayCommand<BarcodeLabel>(parameter => RemoveTestFromLabelAsync(parameter));
        SavePrintOffsetsCommand = new AsyncRelayCommand(_ => SavePrintOffsetsAsync());
    }

    public ObservableCollection<BarcodeLabel> Labels { get; }
    public ObservableCollection<BarcodePatientLabel> CaseLabels { get; }
    public ObservableCollection<BarcodePatientLabel> FileLabels { get; }
    public ObservableCollection<BarcodePatientLabel> LabIdLabels { get; }
    public ObservableCollection<ExtraBarcodeLabel> ExtraLabels { get; }

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

    public bool IncludeLabIdInPrintAll
    {
        get => _includeLabIdInPrintAll;
        set => SetProperty(ref _includeLabIdInPrintAll, value);
    }

    public double PreviewOffsetX
    {
        get => _previewOffsetX;
        set => SetProperty(ref _previewOffsetX, value);
    }

    public double PreviewOffsetY
    {
        get => _previewOffsetY;
        set => SetProperty(ref _previewOffsetY, value);
    }

    public string NewExtraHeader
    {
        get => _newExtraHeader;
        set => SetProperty(ref _newExtraHeader, value);
    }

    public string NewExtraDescription
    {
        get => _newExtraDescription;
        set => SetProperty(ref _newExtraDescription, value);
    }

    public BarcodeLabel? DraggedLabel
    {
        get => _draggedLabel;
        set => SetProperty(ref _draggedLabel, value);
    }

    public ICommand PrintBarcodeCommand { get; }
    public ICommand PrintAllCommand { get; }
    public ICommand AddExtraLabelCommand { get; }
    public ICommand RemoveExtraLabelCommand { get; }
    public ICommand PrintExtraLabelCommand { get; }
    public ICommand RemoveTestFromLabelCommand { get; }
    public ICommand SavePrintOffsetsCommand { get; }

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

        await LoadPrintOffsetsAsync();
    }

    public async Task LoadTubesAsync(int visitId)
    {
        VisitId = visitId;
        var tubes = await _sampleTrackingService.GetTubesForVisitAsync(visitId);
        Labels.Clear();
        foreach (var tube in tubes)
            Labels.Add(ProjectLabel(tube));
    }

    public async Task MoveTestToLabelAsync(BarcodeLabel sourceLabel, BarcodeLabel destLabel)
    {
        if (sourceLabel == destLabel) return;
        if (sourceLabel.SourceTube.TubeId == 0 || destLabel.SourceTube.TubeId == 0) return;

        var sourceTests = sourceLabel.SourceTube.VisitTests.ToList();
        var destTube = destLabel.SourceTube;

        foreach (var vt in sourceTests)
        {
            await _sampleTrackingService.MoveTestToTubeAsync(vt.VisitTestId, destTube.TubeId);
        }

        await LoadTubesAsync(VisitId);
    }

    private async Task RemoveTestFromLabelAsync(BarcodeLabel? label)
    {
        if (label == null) return;
        if (label.SourceTube.TubeId == 0) return;

        var tests = label.SourceTube.VisitTests.ToList();
        foreach (var vt in tests)
        {
            await _sampleTrackingService.RemoveTestFromTubeAsync(vt.VisitTestId);
        }

        await LoadTubesAsync(VisitId);
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
        var allLabels = new List<BarcodeLabel>(Labels);
        allLabels.AddRange(CaseLabels.Select(l => l.ToBarcodeLabel()));
        allLabels.AddRange(FileLabels.Select(l => l.ToBarcodeLabel()));

        if (IncludeLabIdInPrintAll)
            allLabels.AddRange(LabIdLabels.Select(l => l.ToBarcodeLabel()));

        if (allLabels.Count == 0)
            return;

        await CheckStockAndWarnForAllAsync();
        await _labelPrintService.PrintLabelsAsync(allLabels);
    }

    private void AddExtraLabel()
    {
        if (string.IsNullOrWhiteSpace(NewExtraHeader) && string.IsNullOrWhiteSpace(NewExtraDescription))
            return;

        var payload = $"EXT-{DateTime.Now:yyyyMMddHHmmss}-{ExtraLabels.Count + 1:D3}";
        var label = new ExtraBarcodeLabel(NewExtraHeader.Trim(), NewExtraDescription.Trim(), payload);
        ExtraLabels.Add(label);
        NewExtraHeader = string.Empty;
        NewExtraDescription = string.Empty;
    }

    private void RemoveExtraLabel(ExtraBarcodeLabel? label)
    {
        if (label != null)
            ExtraLabels.Remove(label);
    }

    private async Task PrintExtraLabelAsync(ExtraBarcodeLabel? label)
    {
        if (label == null) return;

        var barcodeLabel = label.ToBarcodeLabel();
        await _labelPrintService.PrintLabelsAsync(new[] { barcodeLabel });
    }

    private async Task LoadPrintOffsetsAsync()
    {
        var labSetting = await _settingsService.GetLabSettingAsync();
        if (labSetting != null)
        {
            PreviewOffsetX = labSetting.LabelPrintOffsetXmm;
            PreviewOffsetY = labSetting.LabelPrintOffsetYmm;
        }
    }

    private async Task SavePrintOffsetsAsync()
    {
        var labSetting = await _settingsService.GetLabSettingAsync();
        if (labSetting == null) return;
        labSetting.LabelPrintOffsetXmm = PreviewOffsetX;
        labSetting.LabelPrintOffsetYmm = PreviewOffsetY;
        await _settingsService.UpsertSettingAsync(labSetting, 0);
        _dialogService.ShowMessage("تم حفظ إزاحة الطباعة بنجاح.", "حفظ");
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

public sealed record ExtraBarcodeLabel(
    string Header,
    string Description,
    string BarcodePayload
)
{
    public BarcodeLabel ToBarcodeLabel()
    {
        var typeName = !string.IsNullOrWhiteSpace(Header) ? Header : "باركود إضافي";
        var dummyTube = new SampleTube
        {
            TubeId = 0,
            VisitId = 0,
            TubeType = typeName,
            BarcodeValue = BarcodePayload
        };
        return new BarcodeLabel(
            Header,
            Description,
            string.Empty,
            BarcodePayload,
            string.Empty,
            typeName,
            dummyTube
        );
    }
}

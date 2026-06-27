using System.Collections.ObjectModel;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class BarcodeDialogViewModel : ViewModelBase
{
    private readonly ISampleTrackingService _sampleTrackingService;
    private readonly ILabelPrintService _labelPrintService;
    private readonly IInventoryService _inventoryService;
    private readonly IDialogService _dialogService;
    private int _visitId;
    private BarcodeLabel? _selectedLabel;

    public BarcodeDialogViewModel(
        ISampleTrackingService sampleTrackingService,
        ILabelPrintService labelPrintService,
        IInventoryService inventoryService,
        IDialogService dialogService)
    {
        _sampleTrackingService = sampleTrackingService;
        _labelPrintService = labelPrintService;
        _inventoryService = inventoryService;
        _dialogService = dialogService;
        Labels = new ObservableCollection<BarcodeLabel>();
        PrintBarcodeCommand = new AsyncRelayCommand<BarcodeLabel>(parameter => PrintLabelAsync(parameter));
        PrintAllCommand = new AsyncRelayCommand(_ => PrintAllAsync());
    }

    public ObservableCollection<BarcodeLabel> Labels { get; }

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

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
    private int _visitId;
    private BarcodeLabel? _selectedLabel;

    public BarcodeDialogViewModel(ISampleTrackingService sampleTrackingService, ILabelPrintService labelPrintService)
    {
        _sampleTrackingService = sampleTrackingService;
        _labelPrintService = labelPrintService;
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

        await _labelPrintService.PrintLabelsAsync(new[] { label });
    }

    private async Task PrintAllAsync()
    {
        if (Labels.Count == 0)
            return;

        await _labelPrintService.PrintLabelsAsync(Labels);
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

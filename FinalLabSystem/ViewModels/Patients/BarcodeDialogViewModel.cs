using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class BarcodeDialogViewModel : ViewModelBase
{
    private readonly ISampleTrackingService _sampleTrackingService;
    private int _visitId;
    private SampleTube? _selectedTube;

    public BarcodeDialogViewModel(ISampleTrackingService sampleTrackingService)
    {
        _sampleTrackingService = sampleTrackingService;
        Tubes = new ObservableCollection<SampleTube>();
        PrintBarcodeCommand = new RelayCommand(parameter => PrintTube(parameter as SampleTube ?? SelectedTube));
        PrintAllCommand = new RelayCommand(_ => PrintAll());
    }

    public ObservableCollection<SampleTube> Tubes { get; }

    public int VisitId
    {
        get => _visitId;
        private set => SetProperty(ref _visitId, value);
    }

    public SampleTube? SelectedTube
    {
        get => _selectedTube;
        set => SetProperty(ref _selectedTube, value);
    }

    public ICommand PrintBarcodeCommand { get; }

    public ICommand PrintAllCommand { get; }

    public async Task LoadTubesAsync(int visitId)
    {
        VisitId = visitId;
        var tubes = await _sampleTrackingService.GetTubesForVisitAsync(visitId);
        Tubes.Clear();
        foreach (var tube in tubes)
            Tubes.Add(tube);
    }

    private static void PrintTube(SampleTube? tube)
    {
        if (tube is null)
            return;

        MessageBox.Show($"تم إرسال الباركود للطباعة: {tube.BarcodeValue}", "طباعة الباركود", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintAll()
    {
        foreach (var tube in Tubes)
            PrintTube(tube);
    }
}

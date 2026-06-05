using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

        PrintTubes(new[] { tube }, "Lab barcode");
    }

    private void PrintAll()
    {
        if (Tubes.Count == 0)
            return;

        PrintTubes(Tubes, "Lab barcodes");
    }

    private static void PrintTubes(IEnumerable<SampleTube> tubes, string jobName)
    {
        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.LeftToRight,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 12,
            PagePadding = new Thickness(24)
        };

        foreach (var tube in tubes)
        {
            var sampleType = string.Join(", ", tube.VisitTests
                .Select(vt => vt.Testtype.SampleType)
                .Where(sample => !string.IsNullOrWhiteSpace(sample))
                .Distinct());

            var patientCode = tube.Visit?.Patient?.PatientCode ?? string.Empty;
            var patientName = tube.Visit?.Patient?.FullNameAr ?? string.Empty;
            var visitDate = tube.Visit?.VisitDate.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

            var block = new Paragraph
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 10)
            };
            block.Inlines.Add(new Run($"{patientCode} - {patientName}") { FontWeight = FontWeights.Bold });
            block.Inlines.Add(new LineBreak());
            block.Inlines.Add(new Run(tube.BarcodeValue) { FontSize = 18, FontWeight = FontWeights.Bold });
            block.Inlines.Add(new LineBreak());
            block.Inlines.Add(new Run($"{tube.TubeType} - {sampleType}"));
            block.Inlines.Add(new LineBreak());
            block.Inlines.Add(new Run(visitDate));
            document.Blocks.Add(block);
        }

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, jobName);
    }

    private static string BuildZplStub(SampleTube tube)
    {
        var patientCode = tube.Visit?.Patient?.PatientCode ?? string.Empty;
        var patientName = tube.Visit?.Patient?.FullNameAr ?? string.Empty;
        var builder = new StringBuilder();
        builder.AppendLine("^XA");
        builder.AppendLine("^FO30,20^A0N,25,25^FD" + patientCode + "^FS");
        builder.AppendLine("^FO30,50^A0N,20,20^FD" + patientName + "^FS");
        builder.AppendLine("^FO30,80^BCN,60,Y,N,N^FD" + tube.BarcodeValue + "^FS");
        builder.AppendLine("^FO30,160^A0N,20,20^FD" + tube.TubeType + "^FS");
        builder.AppendLine("^XZ");
        return builder.ToString();
    }
}

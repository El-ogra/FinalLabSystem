using System.Windows.Documents;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class PrintPreviewViewModel : ViewModelBase
{
    private readonly IPrintService _printService;
    private FlowDocument _document = new();
    private string _description = "Lab document";

    public PrintPreviewViewModel(IPrintService printService)
    {
        _printService = printService;
        PrintCommand = new AsyncRelayCommand(async () => await _printService.PrintFlowDocumentAsync(Document, Description), () => Document is not null);
        CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
    }

    public FlowDocument Document
    {
        get => _document;
        set => SetProperty(ref _document, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public ICommand PrintCommand { get; }

    public ICommand CloseCommand { get; }

    public Action? RequestClose { get; set; }
}

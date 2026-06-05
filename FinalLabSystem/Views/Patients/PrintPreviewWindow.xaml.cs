using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FinalLabSystem.Views.Patients;

public partial class PrintPreviewWindow : Window
{
    private readonly FlowDocument _document;

    public PrintPreviewWindow(FlowDocument document)
    {
        InitializeComponent();
        _document = document;
        PreviewViewer.Document = document;
    }

    private void PrintButton_OnClick(object sender, RoutedEventArgs e)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
            printDialog.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, "Lab receipt");
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

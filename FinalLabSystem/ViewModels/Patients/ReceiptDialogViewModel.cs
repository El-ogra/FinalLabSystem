using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class ReceiptDialogViewModel : ViewModelBase
{
    private string _selectedTemplate = "Detailed";
    private ReceiptVisitData? _visitData;

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

    public ReceiptVisitData? VisitData
    {
        get => _visitData;
        set => SetProperty(ref _visitData, value);
    }

    public ICommand PreviewAndPrintCommand { get; }

    public void LoadVisit(Visit visit)
    {
        VisitData = new ReceiptVisitData(
            visit.VisitCode,
            visit.Patient.FullNameAr,
            Convert.ToDecimal(visit.Subtotal),
            Convert.ToDecimal(visit.DiscountAmount),
            Convert.ToDecimal(visit.TotalPaid),
            Convert.ToDecimal(visit.BalanceDue),
            visit.VisitTests
                .Select(vt => new ReceiptTestLine(vt.Testtype.TypeNameAr ?? vt.Testtype.TypeNameEn, Convert.ToDecimal(vt.PriceCharged)))
                .ToList());
    }

    private void PreviewAndPrint()
    {
        if (VisitData is null)
            return;

        var details = SelectedTemplate == "Detailed"
            ? string.Join(Environment.NewLine, VisitData.Tests.Select(test => $"{test.Name}: {test.Price:N2}"))
            : "قالب مختصر";

        MessageBox.Show(
            $"{VisitData.PatientName}{Environment.NewLine}{details}{Environment.NewLine}الإجمالي: {VisitData.Subtotal:N2}{Environment.NewLine}الخصم: {VisitData.Discount:N2}{Environment.NewLine}المدفوع: {VisitData.Paid:N2}{Environment.NewLine}المتبقي: {VisitData.Balance:N2}",
            "معاينة الإيصال",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

public sealed record ReceiptVisitData(
    string VisitCode,
    string PatientName,
    decimal Subtotal,
    decimal Discount,
    decimal Paid,
    decimal Balance,
    IReadOnlyList<ReceiptTestLine> Tests);

public sealed record ReceiptTestLine(string Name, decimal Price);

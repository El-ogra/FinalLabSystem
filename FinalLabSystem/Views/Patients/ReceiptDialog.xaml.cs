using System.Windows;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class ReceiptDialog : Window
{
    public ReceiptDialog(ReceiptDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

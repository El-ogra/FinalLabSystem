using System.Windows;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class BarcodeDialog : Window
{
    public BarcodeDialog(BarcodeDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

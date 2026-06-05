using System.Windows;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class TestResultsWindow : Window
{
    public TestResultsWindow(TestResultsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

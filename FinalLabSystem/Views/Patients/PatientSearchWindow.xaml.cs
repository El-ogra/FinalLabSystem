using System.Windows;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class PatientSearchWindow : Window
{
    public PatientSearchWindow(PatientSearchViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

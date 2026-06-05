using System.Windows;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class PatientRegistrationWindow : Window
{
    public PatientRegistrationWindow(PatientRegistrationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

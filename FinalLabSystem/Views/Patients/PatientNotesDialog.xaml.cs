using System.Windows;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class PatientNotesDialog : Window
{
    public PatientNotesDialog(PatientNotesDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
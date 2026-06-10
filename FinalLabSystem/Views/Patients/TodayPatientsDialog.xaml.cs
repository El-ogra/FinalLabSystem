using System.Windows;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class TodayPatientsDialog : Window
{
    public TodayPatientsDialog(TodayPatientsDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ViewModel = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    public TodayPatientsDialogViewModel ViewModel { get; }

    public TodayPatientDto? SelectedPatient => ViewModel.SelectedPatient;

    private void OnCloseRequested(object? sender, bool result)
    {
        DialogResult = result;
        Close();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}

using System.Windows;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Views.Patients;

public partial class PatientRegistrationWindow : Window
{
    public PatientRegistrationWindow(PatientRegistrationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is IAsyncInitializable vm)
            await vm.InitializeAsync();
    }
}

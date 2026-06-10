using System.Windows;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class TestDataManagementWindow : Window
{
    public TestDataManagementWindow(TestDataManagementViewModel viewModel)
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

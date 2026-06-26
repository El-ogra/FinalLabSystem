using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class ExternalLabsWindow : Window
{
    public ExternalLabsWindow(ExternalLabsWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ExternalLabsWindowViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

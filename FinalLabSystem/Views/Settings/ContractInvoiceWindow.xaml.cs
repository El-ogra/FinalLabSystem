using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class ContractInvoiceWindow : Window
{
    public ContractInvoiceWindow(ContractInvoiceWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContractInvoiceWindowViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

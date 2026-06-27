using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class CashDrawerWindow : Window
{
    private readonly CashDrawerWindowViewModel _viewModel;

    public CashDrawerWindow(CashDrawerWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var canAccess = await _viewModel.TryUnlockAsync();
        if (!canAccess)
        {
            var dlg = new CashDrawerUnlockDialog { Owner = this };
            if (dlg.ShowDialog() == true && dlg.EnteredPassword is not null)
            {
                await _viewModel.UnlockWithPasswordAsync(dlg.EnteredPassword);
            }
        }
    }
}

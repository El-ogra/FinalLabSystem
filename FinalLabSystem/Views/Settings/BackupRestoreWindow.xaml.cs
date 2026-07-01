using System.Windows;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class BackupRestoreWindow : Window
{
    public BackupRestoreWindow(BackupRestoreWindowViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestShutdown = () => navigationService.Shutdown();
    }
}

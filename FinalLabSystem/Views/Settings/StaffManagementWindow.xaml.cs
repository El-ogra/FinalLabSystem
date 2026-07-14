using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class StaffManagementWindow : Window
{
    public StaffManagementWindow(StaffManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.InitializeAsync();
    }
}

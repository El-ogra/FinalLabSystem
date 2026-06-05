using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class TestDataManagementWindow : Window
{
    public TestDataManagementWindow(TestDataManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

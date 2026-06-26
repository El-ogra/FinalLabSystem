using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class CompaniesWindow : Window
{
    public CompaniesWindow(CompaniesWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

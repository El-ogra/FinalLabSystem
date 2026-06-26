using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class PriceSchemeWindow : Window
{
    public PriceSchemeWindow(PriceSchemeWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

using System.Windows;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class NormalRangesWindow : Window
{
    public NormalRangesWindow(NormalRangeWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

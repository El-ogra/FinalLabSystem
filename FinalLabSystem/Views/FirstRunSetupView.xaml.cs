using System.Windows.Controls;
using FinalLabSystem.ViewModels;

namespace FinalLabSystem.Views;

public partial class FirstRunSetupView : UserControl
{
    public FirstRunSetupView(FirstRunSetupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

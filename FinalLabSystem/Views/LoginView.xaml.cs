using System.Windows.Controls;
using FinalLabSystem.ViewModels;

namespace FinalLabSystem.Views;

public partial class LoginView : UserControl
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

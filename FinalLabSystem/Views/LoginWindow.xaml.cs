using System.Windows;

namespace FinalLabSystem.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginView loginView)
    {
        InitializeComponent();
        LoginHost.Content = loginView;
    }
}

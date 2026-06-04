using System.Windows;

namespace FinalLabSystem.Views;

public partial class FirstRunSetupWindow : Window
{
    public FirstRunSetupWindow(FirstRunSetupView setupView)
    {
        InitializeComponent();
        SetupHost.Content = setupView;
    }
}

using System.Windows;

namespace FinalLabSystem.Views.Settings;

public partial class SecuritySettingsWindow : Window
{
    public SecuritySettingsWindow()
    {
        InitializeComponent();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

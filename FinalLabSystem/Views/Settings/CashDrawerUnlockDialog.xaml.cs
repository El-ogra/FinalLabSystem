using System.Windows;
using System.Windows.Input;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Views.Settings;

public partial class CashDrawerUnlockDialog : Window
{
    public CashDrawerUnlockDialog()
    {
        InitializeComponent();
        PasswordBox.Focus();
    }

    public string? EnteredPassword { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ErrorText.Text = "يُرجى إدخال كلمة المرور";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        EnteredPassword = PasswordBox.Password;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OK_Click(sender, e);
    }
}

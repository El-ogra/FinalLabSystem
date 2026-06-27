using System.Windows;
using System.Windows.Input;

namespace FinalLabSystem.Views.Settings;

public partial class CashDrawerChangePasswordDialog : Window
{
    public CashDrawerChangePasswordDialog()
    {
        InitializeComponent();
        CurrentPasswordBox.Focus();
    }

    public string? CurrentPassword { get; private set; }
    public string? NewPassword { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(CurrentPasswordBox.Password))
        {
            ShowError("يُرجى إدخال كلمة المرور الحالية");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
        {
            ShowError("يُرجى إدخال كلمة المرور الجديدة");
            return;
        }

        if (NewPasswordBox.Password.Length < 4)
        {
            ShowError("كلمة المرور الجديدة يجب أن تكون 4 أحرف على الأقل");
            return;
        }

        if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
        {
            ShowError("كلمتا المرور الجديدتان غير متطابقتين");
            return;
        }

        CurrentPassword = CurrentPasswordBox.Password;
        NewPassword = NewPasswordBox.Password;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void Password_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OK_Click(sender, e);
    }
}

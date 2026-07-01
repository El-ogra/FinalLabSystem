using System.Windows;
using System.Windows.Input;

namespace FinalLabSystem.Views.Settings;

public partial class BackupPasswordDialog : Window
{
    public BackupPasswordDialog()
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

        if (PasswordBox.Password.Length < 8)
        {
            ErrorText.Text = "كلمة المرور يجب أن تكون 8 أحرف على الأقل";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            ErrorText.Text = "كلمتا المرور غير متطابقتين";
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

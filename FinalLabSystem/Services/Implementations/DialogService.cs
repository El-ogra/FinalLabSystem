using System.Windows;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public sealed class DialogService : IDialogService
{
    public void ShowMessage(string message, string title = "")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "خطأ")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "تنبيه")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool ShowConfirmation(string message, string title = "تأكيد")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}

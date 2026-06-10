namespace FinalLabSystem.Services.Interfaces;

public interface IDialogService
{
    void ShowMessage(string message, string title = "");
    void ShowError(string message, string title = "خطأ");
    void ShowWarning(string message, string title = "تنبيه");
    bool ShowConfirmation(string message, string title = "تأكيد");
}

namespace FinalLabSystem.Services.Interfaces;

public interface IDialogService
{
    /// <summary>
    /// Shows an informational message to the user.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="title">The dialog title.</param>
    void ShowMessage(string message, string title = "");

    /// <summary>
    /// Shows an error message to the user.
    /// </summary>
    /// <param name="message">The error message text to display.</param>
    /// <param name="title">The dialog title.</param>
    void ShowError(string message, string title = "خطأ");

    /// <summary>
    /// Shows a warning message to the user.
    /// </summary>
    /// <param name="message">The warning message text to display.</param>
    /// <param name="title">The dialog title.</param>
    void ShowWarning(string message, string title = "تنبيه");

    /// <summary>
    /// Shows a confirmation prompt to the user.
    /// </summary>
    /// <param name="message">The confirmation message text to display.</param>
    /// <param name="title">The dialog title.</param>
    /// <returns><c>true</c> when the user confirms; otherwise, <c>false</c>.</returns>
    bool ShowConfirmation(string message, string title = "تأكيد");

    /// <summary>
    /// Shows a modal dialog resolved from the DI container.
    /// </summary>
    /// <typeparam name="T">The Window type to show.</typeparam>
    /// <returns>The dialog result, or <c>null</c> when cancelled.</returns>
    T? ShowCustomDialog<T>() where T : System.Windows.Window;
}

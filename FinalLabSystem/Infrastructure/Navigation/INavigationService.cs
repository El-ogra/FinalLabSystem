namespace FinalLabSystem.Infrastructure.Navigation;

public interface INavigationService
{
    void ShowLogin();

    void ShowFirstRunSetup();

    void ShowMain();

    void OpenTaskWindow<TViewModel>() where TViewModel : class;

    void OpenTaskWindow<TViewModel>(Action<TViewModel>? configure) where TViewModel : class;

    void RegisterWindow<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : System.Windows.Window;

    void ReturnToMain();

    void Shutdown();
}

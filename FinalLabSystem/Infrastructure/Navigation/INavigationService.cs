namespace FinalLabSystem.Infrastructure.Navigation;

public interface INavigationService
{
    void ShowLogin();

    void ShowFirstRunSetup();

    void ShowMain();

    void OpenTaskWindow<TViewModel>() where TViewModel : class;

    void ReturnToMain();

    void Shutdown();
}

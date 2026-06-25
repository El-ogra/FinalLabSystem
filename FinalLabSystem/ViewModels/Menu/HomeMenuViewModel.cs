using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class HomeMenuViewModel : ViewModelBase
{
    public HomeMenuViewModel()
    {
        WelcomeMessage = "مرحباً بكم في نظام FinalLab";
    }

    public string WelcomeMessage { get; }
}

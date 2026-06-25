using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class NormalRangesMenuViewModel : ViewModelBase
{
    public NormalRangesMenuViewModel(INavigationService navigationService)
    {
        NavigateToNormalRangesCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<NormalRangeWindowViewModel>());
    }

    public ICommand NavigateToNormalRangesCommand { get; }
}

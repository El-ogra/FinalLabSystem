using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Infrastructure.Settings;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IUserSettingsService _userSettings;
    private readonly ICurrentUserSession _session;

    private readonly AsyncRelayCommand _loginCommand;
    private readonly RelayCommand _togglePasswordVisibilityCommand;

    private string? _username;
    private string? _password;
    private bool _isRememberMe;
    private bool _isPasswordVisible;
    private string? _errorMessage;
    private bool _isBusy;

    public LoginViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IUserSettingsService userSettings,
        ICurrentUserSession session)
    {
        _authService = authService;
        _navigationService = navigationService;
        _userSettings = userSettings;
        _session = session;

        var rememberedUsername = _userSettings.RememberedUsername;
        if (!string.IsNullOrWhiteSpace(rememberedUsername))
        {
            _username = rememberedUsername;
            _isRememberMe = true;
        }

        _loginCommand = new AsyncRelayCommand(OnLoginAsync, CanLogin);
        _loginCommand.ErrorOccurred += (_, ex) => ErrorMessage = ex.Message;

        _togglePasswordVisibilityCommand = new RelayCommand(_ => IsPasswordVisible = !IsPasswordVisible);
    }

    public string? Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
                _loginCommand.RaiseCanExecuteChanged();
        }
    }

    public string? Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
                _loginCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsRememberMe
    {
        get => _isRememberMe;
        set => SetProperty(ref _isRememberMe, value);
    }

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand LoginCommand => _loginCommand;

    public ICommand TogglePasswordVisibilityCommand => _togglePasswordVisibilityCommand;

    private bool CanLogin(object? _)
        => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    private async Task OnLoginAsync(object? _)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var staff = await _authService.LoginAsync(Username!, Password!);

            if (staff is null)
            {
                ErrorMessage = "Invalid username or password.";
                return;
            }

            await _authService.UpdateLastLoginAsync(staff.StaffId);
            _userSettings.SetRememberedUsername(IsRememberMe ? Username : null);
            _session.SignIn(staff);

            _session.StartIdleTimer(() =>
            {
                _session.SignOut();
                _session.StopIdleTimer();
                _navigationService.ShowLogin();
            });

            _navigationService.ShowMain();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

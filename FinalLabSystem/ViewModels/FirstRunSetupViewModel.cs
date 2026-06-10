using System.Collections;
using System.ComponentModel;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels;

public class FirstRunSetupViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private const int MinUsernameLength = 3;
    private const int MinPasswordLength = 6;

    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly ICurrentUserSession _session;

    private readonly AsyncRelayCommand _createAdministratorCommand;
    private readonly Dictionary<string, List<string>> _errors = new();

    private string? _username;
    private string? _password;
    private string? _confirmPassword;
    private string? _displayName;
    private string? _statusMessage;
    private bool _isBusy;

    public FirstRunSetupViewModel(
        IAuthService authService,
        INavigationService navigationService,
        ICurrentUserSession session)
    {
        _authService = authService;
        _navigationService = navigationService;
        _session = session;

        _createAdministratorCommand = new AsyncRelayCommand(OnCreateAdministratorAsync, _ => !HasErrors && HasAllRequiredFields());
        _createAdministratorCommand.ErrorOccurred += (_, ex) => StatusMessage = ex.Message;
    }

        public new event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public string? Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ValidateUsername();
                _createAdministratorCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string? Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ValidatePassword();
                ValidateConfirmPassword();
                _createAdministratorCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
            {
                ValidateConfirmPassword();
                _createAdministratorCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand CreateAdministratorCommand => _createAdministratorCommand;

    public new bool HasErrors => _errors.Count > 0;

    public new IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(kv => kv.Value);

        return _errors.TryGetValue(propertyName, out var list) ? list : Array.Empty<string>();
    }

    private bool HasAllRequiredFields()
        => !string.IsNullOrWhiteSpace(Username)
           && !string.IsNullOrWhiteSpace(Password)
           && !string.IsNullOrWhiteSpace(ConfirmPassword);

    private void ValidateUsername()
    {
        ClearError(nameof(Username));

        if (string.IsNullOrWhiteSpace(Username))
            AddError(nameof(Username), "Username is required.");
        else if (Username.Trim().Length < MinUsernameLength)
            AddError(nameof(Username), $"Username must be at least {MinUsernameLength} characters.");
    }

    private void ValidatePassword()
    {
        ClearError(nameof(Password));

        if (string.IsNullOrEmpty(Password))
            AddError(nameof(Password), "Password is required.");
        else if (Password.Length < MinPasswordLength)
            AddError(nameof(Password), $"Password must be at least {MinPasswordLength} characters.");
    }

    private void ValidateConfirmPassword()
    {
        ClearError(nameof(ConfirmPassword));

        if (string.IsNullOrEmpty(ConfirmPassword))
            AddError(nameof(ConfirmPassword), "Please confirm the password.");
        else if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            AddError(nameof(ConfirmPassword), "Passwords do not match.");
    }

    private new void AddError(string propertyName, string message)
    {
        if (!_errors.TryGetValue(propertyName, out var list))
        {
            list = new List<string>();
            _errors[propertyName] = list;
        }

        if (!list.Contains(message))
        {
            list.Add(message);
            OnErrorsChanged(propertyName);
        }
    }

    private void ClearError(string propertyName)
    {
        if (_errors.Remove(propertyName))
            OnErrorsChanged(propertyName);
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    private async Task OnCreateAdministratorAsync(object? _)
    {
        ValidateUsername();
        ValidatePassword();
        ValidateConfirmPassword();

        if (HasErrors)
            return;

        IsBusy = true;
        StatusMessage = null;

        try
        {
            var admin = await _authService.CreateInitialAdministratorAsync(Username!, Password!, DisplayName);
            await _authService.UpdateLastLoginAsync(admin.StaffId);
            _session.SignIn(admin);

            _navigationService.ShowMain();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

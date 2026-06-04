namespace FinalLabSystem.Infrastructure.Settings;

public interface IUserSettingsService
{
    string? RememberedUsername { get; }

    void SetRememberedUsername(string? username);
}

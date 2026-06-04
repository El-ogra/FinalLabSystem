using System.IO;
using System.Text.Json;

namespace FinalLabSystem.Infrastructure.Settings;

public sealed class JsonUserSettingsService : IUserSettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _settingsFilePath;
    private readonly object _syncRoot = new();
    private UserSettingsModel _settings;

    public JsonUserSettingsService()
    {
        var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataRoot, "FinalLabSystem");
        Directory.CreateDirectory(appFolder);

        _settingsFilePath = Path.Combine(appFolder, "user.settings.json");
        _settings = Load();
    }

    public string? RememberedUsername => _settings.RememberedUsername;

    public void SetRememberedUsername(string? username)
    {
        lock (_syncRoot)
        {
            _settings.RememberedUsername = string.IsNullOrWhiteSpace(username) ? null : username;
            Save();
        }
    }

    private UserSettingsModel Load()
    {
        if (!File.Exists(_settingsFilePath))
            return new UserSettingsModel();

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<UserSettingsModel>(json) ?? new UserSettingsModel();
        }
        catch
        {
            return new UserSettingsModel();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_settings, SerializerOptions);
        File.WriteAllText(_settingsFilePath, json);
    }

    private sealed class UserSettingsModel
    {
        public string? RememberedUsername { get; set; }
    }
}

using FinalLabSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value, or <c>null</c> when the key is not found.</returns>
    Task<string?> GetSettingValueAsync(string key);

    /// <summary>
    /// Creates or updates a laboratory setting.
    /// </summary>
    /// <param name="setting">The setting to save.</param>
    /// <param name="staffId">The staff member making the change.</param>
    Task UpsertSettingAsync(LabSetting setting, int staffId);

    /// <summary>
    /// Gets settings that belong to a group.
    /// </summary>
    /// <param name="groupName">The setting group name.</param>
    /// <returns>A dictionary of setting keys and values.</returns>
    Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName);
}

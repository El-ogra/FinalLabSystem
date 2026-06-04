using FinalLabSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface ISettingsService
{
    Task<string?> GetSettingValueAsync(string key);
    Task UpsertSettingAsync(LabSetting setting, int staffId);
    Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName);
}

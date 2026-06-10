using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Implementations;

public class SettingsService : ISettingsService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(FinalLabDbContext context, ILogger<SettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);

        return setting?.SettingValue;
    }

    public async Task UpsertSettingAsync(LabSetting setting, int staffId)
    {
        var existing = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == setting.SettingKey);

        if (existing != null)
        {
            existing.SettingValue = setting.SettingValue;
            existing.SettingDescription = setting.SettingDescription;
            existing.SettingGroup = setting.SettingGroup;
            existing.LastUpdatedBy = staffId;
            existing.LastUpdatedAt = DateTime.UtcNow;
        }
        else
        {
            setting.LastUpdatedBy = staffId;
            setting.LastUpdatedAt = DateTime.UtcNow;
            _context.LabSettings.Add(setting);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName)
    {
        return await _context.LabSettings
            .Where(s => s.SettingGroup == groupName)
            .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue ?? string.Empty);
    }
}

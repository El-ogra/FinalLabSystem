using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public sealed class BackupScheduler : IBackupScheduler
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<BackupScheduler> _logger;

    public BackupScheduler(FinalLabDbContext context, ILogger<BackupScheduler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> GetScheduledHourAsync()
    {
        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "BackupScheduleHour");
        return setting?.BackupScheduleHour;
    }

    public async Task<int?> GetRetentionDaysAsync()
    {
        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "BackupRetentionDays");
        return setting?.BackupRetentionDays;
    }

    public async Task SetScheduledHourAsync(int hour, int staffId)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "The scheduled hour must be between 0 and 23.");

        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "BackupScheduleHour");

        if (setting is null)
        {
            setting = new Models.LabSetting
            {
                SettingKey = "BackupScheduleHour",
                BackupScheduleHour = hour,
                LastUpdatedBy = staffId,
                LastUpdatedAt = DateTime.UtcNow
            };
            _context.LabSettings.Add(setting);
        }
        else
        {
            setting.BackupScheduleHour = hour;
            setting.LastUpdatedBy = staffId;
            setting.LastUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetRetentionDaysAsync(int days, int staffId)
    {
        if (days < 0)
            throw new ArgumentOutOfRangeException(nameof(days), "Retention days cannot be negative.");

        var setting = await _context.LabSettings
            .FirstOrDefaultAsync(s => s.SettingKey == "BackupRetentionDays");

        if (setting is null)
        {
            setting = new Models.LabSetting
            {
                SettingKey = "BackupRetentionDays",
                BackupRetentionDays = days,
                LastUpdatedBy = staffId,
                LastUpdatedAt = DateTime.UtcNow
            };
            _context.LabSettings.Add(setting);
        }
        else
        {
            setting.BackupRetentionDays = days;
            setting.LastUpdatedBy = staffId;
            setting.LastUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public Task CleanupOldBackupsAsync(string backupFolder, int retentionDays)
    {
        if (retentionDays <= 0 || !Directory.Exists(backupFolder))
            return Task.CompletedTask;

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        var files = Directory.GetFiles(backupFolder, "*.bak")
            .Concat(Directory.GetFiles(backupFolder, "*.bak.enc"));

        foreach (var file in files)
        {
            var info = new FileInfo(file);
            if (info.CreationTimeUtc < cutoff)
            {
                try
                {
                    info.Delete();
                    _logger.LogInformation("Deleted old backup: {FilePath}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old backup: {FilePath}", file);
                }
            }
        }

        return Task.CompletedTask;
    }
}

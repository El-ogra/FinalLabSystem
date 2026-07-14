using System;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IBackupScheduler
{
    Task<int?> GetScheduledHourAsync();
    Task<int?> GetRetentionDaysAsync();
    Task SetScheduledHourAsync(int hour, int staffId);
    Task SetRetentionDaysAsync(int days, int staffId);
    Task CleanupOldBackupsAsync(string backupFolder, int retentionDays);
}

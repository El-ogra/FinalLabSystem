using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type);
    Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword);
    Task<List<BackupMetadataDto>> ListBackupsAsync(string folder);
    Task<bool> ValidateBackupFileAsync(string backupFilePath, string adminPassword);
}

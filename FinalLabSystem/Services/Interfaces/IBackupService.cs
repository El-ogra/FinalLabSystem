using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type);
    Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword);
    Task<bool> RestoreLegacyJsonBackupAsync(string backupFilePath, string adminPassword);
    Task<List<BackupMetadataDto>> ListBackupsAsync(string folder);
    Task<string> GetBackupOutputFolderAsync();
    Task SaveBackupOutputFolderAsync(string folderPath, int staffId);
}

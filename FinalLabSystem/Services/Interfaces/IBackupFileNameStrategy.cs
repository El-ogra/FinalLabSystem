using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface IBackupFileNameStrategy
{
    string GenerateFileName(BackupType type);
    string GenerateLegacyFileName();
}

using System;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public sealed class BackupFileNameStrategy : IBackupFileNameStrategy
{
    public string GenerateFileName(BackupType type)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        var typeShort = type switch
        {
            BackupType.Full => "FULL",
            BackupType.Incremental => "DIFF",
            _ => "FULL"
        };

        return $"FinalLab_{timestamp}_{typeShort}.bak";
    }

    public string GenerateLegacyFileName()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
        return $"FinalLabSystem_{timestamp}.bak.enc";
    }
}

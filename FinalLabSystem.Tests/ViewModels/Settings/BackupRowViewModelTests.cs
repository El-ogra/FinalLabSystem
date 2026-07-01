using FinalLabSystem.Models.DTOs;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class BackupRowViewModelTests
{
    private static BackupMetadataDto CreateDto(
        string fileName = "backup.bak",
        string filePath = @"C:\Backups\backup.bak",
        long fileSizeBytes = 1048576,
        DateTime? createdAt = null)
    {
        return new BackupMetadataDto
        {
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            IsEncrypted = true,
            SchemaVersion = "1.0"
        };
    }

    [Fact]
    public void WrapsDto_ExposesAllFields()
    {
        var utcNow = DateTime.UtcNow;
        var dto = CreateDto(
            fileName: "test_backup.bak",
            filePath: @"D:\Backups\test_backup.bak",
            fileSizeBytes: 2048,
            createdAt: utcNow);

        var vm = new BackupRowViewModel(dto);

        Assert.Equal("test_backup.bak", vm.FileName);
        Assert.Equal(@"D:\Backups\test_backup.bak", vm.FilePath);
        Assert.Equal("2.0 KB", vm.DisplaySize);
        Assert.Equal(utcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), vm.DisplayCreatedAt);
    }

    [Fact]
    public void DisplaySize_FormatsBytes_HumanReadable()
    {
        var dto = CreateDto(fileSizeBytes: 1048576);

        var vm = new BackupRowViewModel(dto);

        Assert.Equal("1.0 MB", vm.DisplaySize);
    }

    [Fact]
    public void DisplayCreatedAt_ConvertsUTC_ToLocal()
    {
        var utcTime = new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        var dto = CreateDto(createdAt: utcTime);

        var vm = new BackupRowViewModel(dto);

        var expected = utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        Assert.Equal(expected, vm.DisplayCreatedAt);
    }
}

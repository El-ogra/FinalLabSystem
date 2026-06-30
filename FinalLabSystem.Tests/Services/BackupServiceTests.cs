using System.Text.RegularExpressions;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class BackupServiceTests
{
    private static readonly string TestPassword = "TestP@ssw0rd!";

    private static FinalLabDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new FinalLabDbContext(options);
    }

    private static Staff CreateAdminStaff() => new()
    {
        StaffId = 1,
        Username = "admin",
        DisplayName = "Admin User",
        IsAdmin = true,
        IsActive = true,
        PasswordHash = "hash"
    };

    private static Staff CreateNonAdminStaff() => new()
    {
        StaffId = 2,
        Username = "user",
        DisplayName = "Regular User",
        IsAdmin = false,
        IsActive = true,
        PasswordHash = "hash"
    };

    private static (BackupService service, FinalLabDbContext context, string tempDir, Mock<IAuditService> audit)
        CreateBackupService(string dbName, Staff? user = null)
    {
        var context = CreateContext(dbName);
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(user ?? CreateAdminStaff());
        session.Setup(s => s.IsAuthenticated).Returns(user != null);

        var audit = new Mock<IAuditService>();
        var logger = Mock.Of<ILogger<BackupService>>();

        var tempDir = Path.Combine(Path.GetTempPath(), "BackupTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var service = new BackupService(context, session.Object, audit.Object, logger);
        return (service, context, tempDir, audit);
    }

    private static void SeedTestData(FinalLabDbContext context)
    {
        context.Patients.Add(new Patient
        {
            PatientCode = "P001",
            FullNameAr = "أحمد محمد",
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateBackupAsync_NonAdmin_ThrowsUnauthorized()
    {
        var (service, context, tempDir, _) = CreateBackupService(
            Guid.NewGuid().ToString(), CreateNonAdminStaff());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_NullSession_ThrowsUnauthorized()
    {
        var context = CreateContext(Guid.NewGuid().ToString());
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns((Staff?)null);
        session.Setup(s => s.IsAuthenticated).Returns(false);

        var audit = Mock.Of<IAuditService>();
        var logger = Mock.Of<ILogger<BackupService>>();
        var tempDir = Path.Combine(Path.GetTempPath(), "BackupTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var service = new BackupService(context, session.Object, audit, logger);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_AdminUser_WritesFile_ToTargetFolder()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        Assert.True(File.Exists(filePath));
        var fileInfo = new FileInfo(filePath);
        Assert.True(fileInfo.Length > 0);
        Assert.EndsWith(".bak.enc", filePath);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_AdminUser_LogsAuditEvent_CorrectMapping()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, audit) = CreateBackupService(dbName);
        SeedTestData(context);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        audit.Verify(a => a.LogActionAsync(
            "Backup",
            0,
            "B",
            It.IsAny<int>(),
            It.IsAny<string?>()),
            Times.Once);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_FileName_FollowsTimestampPattern()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var fileName = Path.GetFileName(filePath);
        Assert.Matches(@"^FinalLabSystem_\d{4}-\d{2}-\d{2}_\d{6}\.bak\.enc$", fileName);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_EncryptedFile_DoesNotContain_PlaintextMarker()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var content = await File.ReadAllBytesAsync(filePath);
        var text = System.Text.Encoding.UTF8.GetString(content);
        Assert.DoesNotContain("PatientCode", text);
        Assert.DoesNotContain("FullNameAr", text);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_ExcludesViews()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var encryptedBytes = await File.ReadAllBytesAsync(filePath);
        var jsonBytes = FinalLabSystem.Infrastructure.Security.AesEncryptionHelper.Decrypt(encryptedBytes, TestPassword);
        var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

        Assert.DoesNotContain("VOutstandingBalance", json);
        Assert.DoesNotContain("VPatientHistory", json);
        Assert.DoesNotContain("VPendingTest", json);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_InvalidFolder_ThrowsIOException()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);

        var invalidPath = @"C:\Windows\System32\not_exist_folder\backup.bak";

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.CreateBackupAsync(invalidPath, TestPassword, BackupType.Full));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_CreatesPreRestoreBackup_First()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var preRestorePath = Path.Combine(tempDir, "pre_restore.bak.enc");
        var preRestoreFile = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);
        File.Copy(preRestoreFile, preRestorePath, true);

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        Assert.True(result);
        Assert.True(File.Exists(preRestorePath));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_WrongPassword_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var result = await service.RestoreBackupAsync(backupPath, "WrongPassword!");

        Assert.False(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_NonAdmin_ThrowsUnauthorized()
    {
        var dbName = Guid.NewGuid().ToString();
        var (adminService, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await adminService.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var (nonAdminService, _, _, _) = CreateBackupService(dbName, CreateNonAdminStaff());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => nonAdminService.RestoreBackupAsync(backupPath, TestPassword));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_OnException_RollsBackTransaction()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var patientCountBefore = await context.Patients.CountAsync();

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        var patientCountAfter = await context.Patients.CountAsync();
        Assert.Equal(patientCountBefore, patientCountAfter);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_PreservesFkRelationships()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        Assert.True(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_PreservesTimestampsAsUtc()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);

        var patient = new Patient
        {
            PatientCode = "P002",
            FullNameAr = "سارة علي",
            Sex = "F",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        Assert.True(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_PreservesSoftDeletedRecords()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);

        var patient = new Patient
        {
            PatientCode = "P003",
            FullNameAr = "محمد أحمد",
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        Assert.True(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogsAuditEvent_CorrectMapping()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, audit) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        await service.RestoreBackupAsync(backupPath, TestPassword);

        audit.Verify(a => a.LogActionAsync(
            "Backup",
            0,
            "R",
            It.IsAny<int>(),
            It.IsAny<string?>()),
            Times.Once);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ListBackupsAsync_EmptyFolder_ReturnsEmpty()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, _, tempDir, _) = CreateBackupService(dbName);

        var result = await service.ListBackupsAsync(tempDir);

        Assert.Empty(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ListBackupsAsync_ReturnsCorrectMetadata()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var result = await service.ListBackupsAsync(tempDir);

        Assert.Single(result);
        Assert.Contains(".bak.enc", result[0].FileName);
        Assert.True(result[0].FileSizeBytes > 0);
        Assert.True(result[0].IsEncrypted);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ValidateBackupFileAsync_ValidFile_ReturnsTrue()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var result = await service.ValidateBackupFileAsync(backupPath, TestPassword);

        Assert.True(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ValidateBackupFileAsync_CorruptedFile_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _) = CreateBackupService(dbName);

        var filePath = Path.Combine(tempDir, "corrupted.bak.enc");
        await File.WriteAllBytesAsync(filePath, new byte[] { 1, 2, 3, 4, 5 });

        var result = await service.ValidateBackupFileAsync(filePath, TestPassword);

        Assert.False(result);

        Directory.Delete(tempDir, true);
    }
}

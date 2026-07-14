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

    private sealed class TestableBackupService : BackupService
    {
        private readonly string _databaseName;

        public TestableBackupService(
            FinalLabDbContext context,
            ICurrentUserSession currentUserSession,
            IAuditService auditService,
            ISensitiveScreenPasswordService sensitivePasswordService,
            ISqlServerBackupExecutor backupExecutor,
            ISqlServerRestoreExecutor restoreExecutor,
            IBackupFileNameStrategy fileNameStrategy,
            ILogger<BackupService> logger,
            string databaseName)
            : base(context, currentUserSession, auditService, sensitivePasswordService,
                  backupExecutor, restoreExecutor, fileNameStrategy, logger)
        {
            _databaseName = databaseName;
        }

        protected override string GetDatabaseName() => _databaseName;
    }

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

    private static (BackupService service, FinalLabDbContext context, string tempDir, Mock<IAuditService> audit, Mock<ISqlServerBackupExecutor> backupExecutor, Mock<ISqlServerRestoreExecutor> restoreExecutor)
        CreateBackupService(string dbName, Staff? user = null)
    {
        var context = CreateContext(dbName);
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(user ?? CreateAdminStaff());
        session.Setup(s => s.IsAuthenticated).Returns(user != null);

        var audit = new Mock<IAuditService>();
        var backupExecutor = new Mock<ISqlServerBackupExecutor>();
        var restoreExecutor = new Mock<ISqlServerRestoreExecutor>();
        var fileNameStrategy = new BackupFileNameStrategy();
        var logger = Mock.Of<ILogger<BackupService>>();

        var tempDir = Path.Combine(Path.GetTempPath(), "BackupTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var service = new TestableBackupService(
            context,
            session.Object,
            audit.Object,
            Mock.Of<ISensitiveScreenPasswordService>(),
            backupExecutor.Object,
            restoreExecutor.Object,
            fileNameStrategy,
            logger,
            "TestDb");
        return (service, context, tempDir, audit, backupExecutor, restoreExecutor);
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
        var (service, context, tempDir, _, _, _) = CreateBackupService(
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
        var backupExecutor = new Mock<ISqlServerBackupExecutor>();
        var restoreExecutor = new Mock<ISqlServerRestoreExecutor>();
        var fileNameStrategy = new BackupFileNameStrategy();
        var logger = Mock.Of<ILogger<BackupService>>();
        var tempDir = Path.Combine(Path.GetTempPath(), "BackupTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var service = new TestableBackupService(
            context,
            session.Object,
            audit,
            Mock.Of<ISensitiveScreenPasswordService>(),
            backupExecutor.Object,
            restoreExecutor.Object,
            fileNameStrategy,
            logger,
            "TestDb");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_AdminUser_CallsBackupExecutor()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, backupExecutor, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        Assert.NotNull(filePath);
        Assert.EndsWith(".bak", filePath);
        backupExecutor.Verify(e => e.FullBackupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_AdminUser_LogsAuditEvent_CorrectMapping()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, audit, _, _) = CreateBackupService(dbName);
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
        var (service, context, tempDir, _, _, _) = CreateBackupService(dbName);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Full);

        var fileName = Path.GetFileName(filePath);
        Assert.Matches(@"^FinalLab_\d{4}-\d{2}-\d{2}_\d{6}_FULL\.bak$", fileName);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_DifferentialBackup_CallsDifferentialExecutor()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, backupExecutor, _) = CreateBackupService(dbName);

        var filePath = await service.CreateBackupAsync(tempDir, TestPassword, BackupType.Incremental);

        Assert.EndsWith(".bak", filePath);
        Assert.Contains("DIFF", filePath);
        backupExecutor.Verify(e => e.DifferentialBackupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task CreateBackupAsync_InvalidFolder_ThrowsIOException()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, _, _) = CreateBackupService(dbName);

        var invalidPath = @"X:\NonExistentDrive\backup";

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.CreateBackupAsync(invalidPath, TestPassword, BackupType.Full));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_AdminUser_CallsRestoreExecutors()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, _, restoreExecutor) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = Path.Combine(tempDir, "test_backup.bak");
        await File.WriteAllBytesAsync(backupPath, new byte[] { 1, 2, 3 });

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        Assert.True(result);
        restoreExecutor.Verify(e => e.SetSingleUserAsync(It.IsAny<string>()), Times.Once);
        restoreExecutor.Verify(e => e.RestoreAsync(It.IsAny<string>(), backupPath), Times.Once);
        restoreExecutor.Verify(e => e.SetMultiUserAsync(It.IsAny<string>()), Times.Once);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_NonAdmin_ThrowsUnauthorized()
    {
        var dbName = Guid.NewGuid().ToString();
        var (adminService, context, tempDir, _, _, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = Path.Combine(tempDir, "test_backup.bak");
        await File.WriteAllBytesAsync(backupPath, new byte[] { 1, 2, 3 });

        var (nonAdminService, _, _, _, _, _) = CreateBackupService(dbName, CreateNonAdminStaff());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => nonAdminService.RestoreBackupAsync(backupPath, TestPassword));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_OnException_SetsMultiUser()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, _, restoreExecutor) = CreateBackupService(dbName);
        SeedTestData(context);

        restoreExecutor.Setup(e => e.RestoreAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Restore failed"));

        var backupPath = Path.Combine(tempDir, "test_backup.bak");
        await File.WriteAllBytesAsync(backupPath, new byte[] { 1, 2, 3 });

        var result = await service.RestoreBackupAsync(backupPath, TestPassword);

        Assert.False(result);
        restoreExecutor.Verify(e => e.SetMultiUserAsync(It.IsAny<string>()), Times.Once);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogsAuditEvent_CorrectMapping()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, audit, _, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var backupPath = Path.Combine(tempDir, "test_backup.bak");
        await File.WriteAllBytesAsync(backupPath, new byte[] { 1, 2, 3 });

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
        var (service, _, tempDir, _, _, _) = CreateBackupService(dbName);

        var result = await service.ListBackupsAsync(tempDir);

        Assert.Empty(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ListBackupsAsync_ReturnsCorrectMetadata_ForNewFormat()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, _, _) = CreateBackupService(dbName);
        SeedTestData(context);

        var dummyFile = Path.Combine(tempDir, "FinalLab_2026-01-01_120000_FULL.bak");
        await File.WriteAllBytesAsync(dummyFile, new byte[] { 1, 2, 3, 4, 5 });

        var result = await service.ListBackupsAsync(tempDir);

        Assert.Single(result);
        Assert.Contains(".bak", result[0].FileName);
        Assert.DoesNotContain(".enc", result[0].FileName);
        Assert.True(result[0].FileSizeBytes > 0);
        Assert.False(result[0].IsEncrypted);
        Assert.Equal("2.0", result[0].SchemaVersion);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ListBackupsAsync_DetectsLegacyFormat()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, _, tempDir, _, _, _) = CreateBackupService(dbName);

        var legacyFile = Path.Combine(tempDir, "FinalLabSystem_2026-01-01_120000.bak.enc");
        await File.WriteAllBytesAsync(legacyFile, new byte[] { 1, 2, 3 });

        var result = await service.ListBackupsAsync(tempDir);

        Assert.Single(result);
        Assert.Contains(".bak.enc", result[0].FileName);
        Assert.True(result[0].IsEncrypted);
        Assert.Equal("1.0", result[0].SchemaVersion);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task RestoreBackupAsync_FileNotFound_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, _, tempDir, _, _, _) = CreateBackupService(dbName);

        var result = await service.RestoreBackupAsync(Path.Combine(tempDir, "nonexistent.bak"), TestPassword);

        Assert.False(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetBackupOutputFolderAsync_ReturnsDefault_WhenNoSetting()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, _, tempDir, _, _, _) = CreateBackupService(dbName);

        var result = await service.GetBackupOutputFolderAsync();

        Assert.NotNull(result);
        Assert.Contains("FinalLabBackups", result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task SaveBackupOutputFolderAsync_ThenGet_ReturnsSavedPath()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, context, tempDir, _, _, _) = CreateBackupService(dbName);

        var testPath = Path.Combine(tempDir, "TestBackups");
        await service.SaveBackupOutputFolderAsync(testPath, 1);

        var result = await service.GetBackupOutputFolderAsync();

        Assert.Equal(testPath, result);

        Directory.Delete(tempDir, true);
    }
}

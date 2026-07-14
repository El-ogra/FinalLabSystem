using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class BackupServiceIntegrationTests
{
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

    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static void SeedFullTestData(FinalLabDbContext context)
    {
        for (int i = 1; i <= 5; i++)
        {
            context.Patients.Add(new Patient
            {
                PatientCode = $"P{i:D3}",
                FullNameAr = $"مريض تجريبي {i}",
                Sex = i % 2 == 0 ? "F" : "M",
                PatientType = "Individual",
                CreatedAt = DateTime.UtcNow
            });
        }
        context.SaveChanges();

        for (int i = 1; i <= 3; i++)
        {
            context.Visits.Add(new Visit
            {
                VisitCode = $"V{i:D3}",
                PatientId = i,
                VisitDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
        context.SaveChanges();
    }

    private static Staff CreateAdminStaff() => new()
    {
        StaffId = 1,
        Username = "admin",
        DisplayName = "Admin",
        IsAdmin = true,
        IsActive = true,
        PasswordHash = "hash"
    };

    private static (BackupService service, FinalLabDbContext context, string tempDir, Mock<ISqlServerBackupExecutor> backupExecutor, Mock<ISqlServerRestoreExecutor> restoreExecutor)
        CreateTestService(string dbName)
    {
        var context = new FinalLabDbContext(CreateOptions(dbName));
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(CreateAdminStaff());
        session.Setup(s => s.IsAuthenticated).Returns(true);
        var audit = Mock.Of<IAuditService>();
        var backupExecutor = new Mock<ISqlServerBackupExecutor>();
        var restoreExecutor = new Mock<ISqlServerRestoreExecutor>();
        var fileNameStrategy = new BackupFileNameStrategy();
        var logger = Mock.Of<ILogger<BackupService>>();
        var tempDir = Path.Combine(Path.GetTempPath(), "E2E_" + Guid.NewGuid().ToString("N")[..8]);
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
            dbName);
        return (service, context, tempDir, backupExecutor, restoreExecutor);
    }

    [Fact]
    public async Task FullBackupRestoreCycle_CallsAllExecutors()
    {
        var dbName = Guid.NewGuid().ToString();
        var password = "E2ETestP@ss!";

        using (var seedContext = new FinalLabDbContext(CreateOptions(dbName)))
        {
            SeedFullTestData(seedContext);
        }

        string backupPath;
        var (service, context, tempDir, backupExecutor, restoreExecutor) = CreateTestService(dbName);
        try
        {
            backupPath = await service.CreateBackupAsync(tempDir, password, BackupType.Full);
            Assert.NotNull(backupPath);
            Assert.EndsWith(".bak", backupPath);

            backupExecutor.Verify(e => e.FullBackupAsync(It.IsAny<string>(), backupPath), Times.Once);

            var dummyFile = Path.Combine(tempDir, "restore_test.bak");
            await File.WriteAllBytesAsync(dummyFile, new byte[] { 1, 2, 3 });
            var result = await service.RestoreBackupAsync(dummyFile, password);
            Assert.True(result);

            restoreExecutor.Verify(e => e.SetSingleUserAsync(It.IsAny<string>()), Times.Once);
            restoreExecutor.Verify(e => e.RestoreAsync(It.IsAny<string>(), dummyFile), Times.Once);
            restoreExecutor.Verify(e => e.SetMultiUserAsync(It.IsAny<string>()), Times.Once);
        }
        finally
        {
            context.Dispose();
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task BackupRestore_WithCircularNavigationProperties_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var tempDir = Path.Combine(Path.GetTempPath(), "E2E_Circular_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            var password = "CircularTestP@ss!";

            using (var context = new FinalLabDbContext(CreateOptions(dbName)))
            {
                context.Patients.Add(new Patient
                {
                    PatientCode = "P001",
                    FullNameAr = "Patient With Navigation",
                    Sex = "M",
                    PatientType = "Individual",
                    CreatedAt = DateTime.UtcNow
                });
                context.LabSettings.Add(new LabSetting
                {
                    SettingKey = "TestSetting",
                    SettingValue = "TestValue",
                    EnforceStageGating = true,
                    EnableServerPrinting = false
                });
                await context.SaveChangesAsync();
            }

            var session = new Mock<ICurrentUserSession>();
            session.Setup(s => s.CurrentUser).Returns(CreateAdminStaff());
            session.Setup(s => s.IsAuthenticated).Returns(true);
            var audit = Mock.Of<IAuditService>();
            var backupExecutor = new Mock<ISqlServerBackupExecutor>();
            var restoreExecutor = new Mock<ISqlServerRestoreExecutor>();
            var fileNameStrategy = new BackupFileNameStrategy();
            var logger = Mock.Of<ILogger<BackupService>>();

            using (var context = new FinalLabDbContext(CreateOptions(dbName)))
            {
                var service = new TestableBackupService(
                    context,
                    session.Object,
                    audit,
                    Mock.Of<ISensitiveScreenPasswordService>(),
                    backupExecutor.Object,
                    restoreExecutor.Object,
                    fileNameStrategy,
                    logger,
                    dbName);
                var backupPath = await service.CreateBackupAsync(tempDir, password, BackupType.Full);
                Assert.NotNull(backupPath);
                Assert.EndsWith(".bak", backupPath);
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BackupRestore_DifferentPasswords_ProducesDifferentKeys()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("sensitive backup data");

        var enc1 = AesEncryptionHelper.Encrypt(data, "Password1!");
        var enc2 = AesEncryptionHelper.Encrypt(data, "Password2!");

        Assert.NotEqual(enc1, enc2);

        var dec1 = AesEncryptionHelper.Decrypt(enc1, "Password1!");
        var dec2 = AesEncryptionHelper.Decrypt(enc2, "Password2!");

        Assert.Equal(data, dec1);
        Assert.Equal(data, dec2);

        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(
            () => AesEncryptionHelper.Decrypt(enc1, "Password2!"));
        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(
            () => AesEncryptionHelper.Decrypt(enc2, "Password1!"));
    }

    [Fact]
    public Task BackupFileNameStrategy_GeneratesCorrectFormat()
    {
        var strategy = new BackupFileNameStrategy();

        var fullFileName = strategy.GenerateFileName(BackupType.Full);
        var diffFileName = strategy.GenerateFileName(BackupType.Incremental);
        var legacyFileName = strategy.GenerateLegacyFileName();

        Assert.Matches(@"^FinalLab_\d{4}-\d{2}-\d{2}_\d{6}_FULL\.bak$", fullFileName);
        Assert.Matches(@"^FinalLab_\d{4}-\d{2}-\d{2}_\d{6}_DIFF\.bak$", diffFileName);
        Assert.Matches(@"^FinalLabSystem_\d{4}-\d{2}-\d{2}_\d{6}\.bak\.enc$", legacyFileName);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task RestoreLegacyJsonBackup_DecryptsAndRestores()
    {
        var dbName = Guid.NewGuid().ToString();
        var tempDir = Path.Combine(Path.GetTempPath(), "E2E_Legacy_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            var password = "LegacyTestP@ss!";

            using (var seedContext = new FinalLabDbContext(CreateOptions(dbName)))
            {
                seedContext.Patients.Add(new Patient
                {
                    PatientCode = "LEG001",
                    FullNameAr = "Legacy Patient",
                    Sex = "M",
                    PatientType = "Individual",
                    CreatedAt = DateTime.UtcNow
                });
                await seedContext.SaveChangesAsync();
            }

            var session = new Mock<ICurrentUserSession>();
            session.Setup(s => s.CurrentUser).Returns(CreateAdminStaff());
            session.Setup(s => s.IsAuthenticated).Returns(true);
            var audit = Mock.Of<IAuditService>();
            var backupExecutor = new Mock<ISqlServerBackupExecutor>();
            var restoreExecutor = new Mock<ISqlServerRestoreExecutor>();
            var fileNameStrategy = new BackupFileNameStrategy();
            var logger = Mock.Of<ILogger<BackupService>>();

            var legacyData = new Dictionary<string, List<Dictionary<string, object?>>>
            {
                ["Patients"] = new List<Dictionary<string, object?>>
                {
                    new()
                    {
                        ["PatientCode"] = "LEG002",
                        ["FullNameAr"] = "Restored Legacy",
                        ["Sex"] = "F",
                        ["PatientType"] = "Individual",
                        ["CreatedAt"] = DateTime.UtcNow
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(legacyData);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            var encryptedBytes = AesEncryptionHelper.Encrypt(jsonBytes, password);
            var legacyFile = Path.Combine(tempDir, "legacy_backup.bak.enc");
            await File.WriteAllBytesAsync(legacyFile, encryptedBytes);

            using (var context = new FinalLabDbContext(CreateOptions(dbName)))
            {
                var service = new TestableBackupService(
                    context,
                    session.Object,
                    audit,
                    Mock.Of<ISensitiveScreenPasswordService>(),
                    backupExecutor.Object,
                    restoreExecutor.Object,
                    fileNameStrategy,
                    logger,
                    dbName);

                var result = await service.RestoreLegacyJsonBackupAsync(legacyFile, password);
                Assert.True(result);
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

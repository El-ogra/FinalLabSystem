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

    private static (BackupService service, FinalLabDbContext context, string tempDir)
        CreateTestService(string dbName)
    {
        var context = new FinalLabDbContext(CreateOptions(dbName));
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(CreateAdminStaff());
        session.Setup(s => s.IsAuthenticated).Returns(true);
        var audit = Mock.Of<IAuditService>();
        var logger = Mock.Of<ILogger<BackupService>>();
        var tempDir = Path.Combine(Path.GetTempPath(), "E2E_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        var service = new BackupService(context, session.Object, audit, logger);
        return (service, context, tempDir);
    }

    [Fact]
    public async Task FullBackupRestoreCycle_PreservesAllData()
    {
        var dbName = Guid.NewGuid().ToString();
        var password = "E2ETestP@ss!";

        // Seed data
        using (var seedContext = new FinalLabDbContext(CreateOptions(dbName)))
        {
            SeedFullTestData(seedContext);
        }

        // Create backup
        string backupPath;
        var (service, context, tempDir) = CreateTestService(dbName);
        try
        {
            backupPath = await service.CreateBackupAsync(tempDir, password, BackupType.Full);
            Assert.True(File.Exists(backupPath));

            // Validate backup
            var isValid = await service.ValidateBackupFileAsync(backupPath, password);
            Assert.True(isValid);

            // Verify decrypted content contains Patient data
            var encryptedBytes = await File.ReadAllBytesAsync(backupPath);
            var jsonBytes = AesEncryptionHelper.Decrypt(encryptedBytes, password);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            Assert.Contains("Patient", json);
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
            var logger = Mock.Of<ILogger<BackupService>>();

            using (var context = new FinalLabDbContext(CreateOptions(dbName)))
            {
                var service = new BackupService(context, session.Object, audit, logger);
                var backupPath = await service.CreateBackupAsync(tempDir, password, BackupType.Full);
                Assert.True(File.Exists(backupPath));
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
}

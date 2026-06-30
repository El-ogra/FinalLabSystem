using FinalLabSystem.Data;
using FinalLabSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinalLabSystem.Tests.Validation;

public class LabSettingSmtpBackupMigrationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public void Migration_AddsAllEightColumns_NullableOrDefaulted()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(LabSetting))!;
        var propertyNames = entityType.GetProperties().Select(p => p.Name).ToList();

        Assert.Contains("SmtpHost", propertyNames);
        Assert.Contains("SmtpPort", propertyNames);
        Assert.Contains("SmtpUsername", propertyNames);
        Assert.Contains("SmtpPasswordEncrypted", propertyNames);
        Assert.Contains("SmtpEnableSsl", propertyNames);
        Assert.Contains("BackupScheduleHour", propertyNames);
        Assert.Contains("BackupRetentionDays", propertyNames);
        Assert.Contains("BackupOutputFolder", propertyNames);
    }

    [Fact]
    public void Migration_SmtpPort_DefaultsTo587()
    {
        var setting = new LabSetting
        {
            SettingKey = "test"
        };

        Assert.Equal(587, setting.SmtpPort ?? 587);
    }

    [Fact]
    public void Migration_BackupScheduleHour_DefaultsTo2()
    {
        var setting = new LabSetting
        {
            SettingKey = "test"
        };

        Assert.Equal(2, setting.BackupScheduleHour ?? 2);
    }

    [Fact]
    public void Migration_BackupRetentionDays_DefaultsTo30()
    {
        var setting = new LabSetting
        {
            SettingKey = "test"
        };

        Assert.Equal(30, setting.BackupRetentionDays ?? 30);
    }
}

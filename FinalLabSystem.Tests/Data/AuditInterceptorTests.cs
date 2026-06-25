using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace FinalLabSystem.Tests.Data;

public class AuditInterceptorTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static Mock<ICurrentUserSession> CreateSession(int staffId = 1)
    {
        var mock = new Mock<ICurrentUserSession>();
        mock.Setup(s => s.CurrentUser).Returns(new Staff
        {
            StaffId = staffId,
            DisplayName = "Test User",
            Username = "testuser"
        });
        return mock;
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAddingAuditableEntity_CreatesAuditLog()
    {
        var dbName = Guid.NewGuid().ToString();
        var session = CreateSession();
        using var context = new FinalLabDbContext(CreateOptions(dbName), session.Object);

        context.Visits.Add(new Visit
        {
            VisitCode = "V001",
            PatientId = 1,
            VisitDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs.ToListAsync();
        Assert.NotEmpty(logs);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenModifyingAuditableEntity_CapturesOldAndNewValues()
    {
        var dbName = Guid.NewGuid().ToString();
        var session = CreateSession();
        using var context = new FinalLabDbContext(CreateOptions(dbName), session.Object);

        context.Visits.Add(new Visit
        {
            VisitCode = "V002",
            PatientId = 1,
            VisitDate = DateTime.UtcNow,
            Notes = "Original"
        });
        await context.SaveChangesAsync();

        var visit = await context.Visits.FirstAsync();
        visit.Notes = "Updated";
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs
            .Where(l => l.TableName == "Visit" && l.FieldName == "Notes")
            .ToListAsync();
        Assert.Contains(logs, l => l.OldValue == "Original" && l.NewValue == "Updated");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDeletingAuditableEntity_CreatesDeleteLog()
    {
        var dbName = Guid.NewGuid().ToString();
        var session = CreateSession();
        using var context = new FinalLabDbContext(CreateOptions(dbName), session.Object);

        context.Visits.Add(new Visit
        {
            VisitCode = "V003",
            PatientId = 1,
            VisitDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var visit = await context.Visits.FirstAsync();
        context.Visits.Remove(visit);
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs
            .Where(l => l.Action == "D")
            .ToListAsync();
        Assert.NotEmpty(logs);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNonAuditableEntity_NoAuditLog()
    {
        var dbName = Guid.NewGuid().ToString();
        var session = CreateSession();
        using var context = new FinalLabDbContext(CreateOptions(dbName), session.Object);

        context.AuditLogs.Add(new AuditLog
        {
            TableName = "Test",
            RecordId = 1,
            Action = "A",
            FieldName = "Test",
            ChangedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var count = await context.AuditLogs.CountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenSessionIsNull_DoesNotCreateAuditLog()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));

        context.Visits.Add(new Visit
        {
            VisitCode = "V004",
            PatientId = 1,
            VisitDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs.ToListAsync();
        Assert.Empty(logs);
    }
}

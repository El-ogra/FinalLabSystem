using System.Linq;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalLabSystem.Tests.Validation;

public class DeliveryConfirmationMigrationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public void Migration_AddsDeliveryConfirmedAt_DeliverySignature_DeliveryOtpCode_ToVisit()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Migration_AddsDeliveryConfirmedAt_DeliverySignature_DeliveryOtpCode_ToVisit)));
        ctx.Database.EnsureCreated();

        var entityType = ctx.Model.FindEntityType(typeof(Visit));
        Assert.NotNull(entityType);

        var propertyNames = entityType.GetProperties().Select(p => p.Name).ToList();
        Assert.Contains("DeliveryConfirmedAt", propertyNames);
        Assert.Contains("DeliverySignature", propertyNames);
        Assert.Contains("DeliveryOtpCode", propertyNames);
    }

    [Fact]
    public void Migration_CreatesDeliveryConfirmationTable_With_AllColumns()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Migration_CreatesDeliveryConfirmationTable_With_AllColumns)));
        ctx.Database.EnsureCreated();

        var entityType = ctx.Model.FindEntityType(typeof(DeliveryConfirmation));
        Assert.NotNull(entityType);

        var propertyNames = entityType.GetProperties().Select(p => p.Name).ToList();
        Assert.Contains("DeliveryConfirmationId", propertyNames);
        Assert.Contains("VisitId", propertyNames);
        Assert.Contains("Method", propertyNames);
        Assert.Contains("ConfirmedAt", propertyNames);
        Assert.Contains("SignatureImage", propertyNames);
        Assert.Contains("OtpCodeHash", propertyNames);
        Assert.Contains("ReceivedByName", propertyNames);
        Assert.Contains("StaffId", propertyNames);
    }

    [Fact]
    public void Migration_No_NatighRelated_Columns_Added_To_Visit()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Migration_No_NatighRelated_Columns_Added_To_Visit)));
        ctx.Database.EnsureCreated();

        var entityType = ctx.Model.FindEntityType(typeof(Visit));
        Assert.NotNull(entityType);

        var propertyNames = entityType.GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain(propertyNames, p => p.Contains("Natigh"));
    }

    [Fact]
    public void DeliveryConfirmation_HasCorrectRelationships()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(DeliveryConfirmation_HasCorrectRelationships)));
        ctx.Database.EnsureCreated();

        var entityType = ctx.Model.FindEntityType(typeof(DeliveryConfirmation));
        Assert.NotNull(entityType);

        var fkProperties = entityType.GetProperties().Select(p => p.Name).ToList();
        Assert.Contains("VisitId", fkProperties);
        Assert.Contains("StaffId", fkProperties);
    }

    [Fact]
    public void DeliveryConfirmation_IsAuditable()
    {
        var hasAuditable = typeof(DeliveryConfirmation)
            .GetCustomAttributes(typeof(AuditableAttribute), false)
            .Length > 0;
        Assert.True(hasAuditable);
    }
}

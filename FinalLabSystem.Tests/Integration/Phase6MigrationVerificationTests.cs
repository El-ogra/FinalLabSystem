using System.Linq;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace FinalLabSystem.Tests.Integration;

public class Phase6MigrationVerificationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public void AllPhase6Models_Exist_InModel()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(AllPhase6Models_Exist_InModel)));
        ctx.Database.EnsureCreated();

        var deliveryConfirmationType = ctx.Model.FindEntityType(typeof(DeliveryConfirmation));
        Assert.NotNull(deliveryConfirmationType);

        var visitType = ctx.Model.FindEntityType(typeof(Visit));
        Assert.NotNull(visitType);

        var deliveryProp = visitType!.FindProperty("DeliveryConfirmedAt");
        Assert.NotNull(deliveryProp);

        var signatureProp = visitType.FindProperty("DeliverySignature");
        Assert.NotNull(signatureProp);

        var otpProp = visitType.FindProperty("DeliveryOtpCode");
        Assert.NotNull(otpProp);
    }

    [Fact]
    public void DeliveryConfirmation_HasCorrectTableStructure()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(DeliveryConfirmation_HasCorrectTableStructure)));
        ctx.Database.EnsureCreated();

        var entityType = ctx.Model.FindEntityType(typeof(DeliveryConfirmation));
        Assert.NotNull(entityType);
        Assert.Equal("DeliveryConfirmation", entityType!.GetTableName());

        var pk = entityType.FindPrimaryKey();
        Assert.NotNull(pk);
        Assert.Equal("DeliveryConfirmationId", pk!.Properties.First().Name);
    }

    [Fact]
    public void Visit_DeliveryColumns_AreNullable()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Visit_DeliveryColumns_AreNullable)));
        ctx.Database.EnsureCreated();

        var entityType = ctx.Model.FindEntityType(typeof(Visit));
        Assert.NotNull(entityType);

        var confirmedAt = entityType!.FindProperty("DeliveryConfirmedAt");
        Assert.NotNull(confirmedAt);
        Assert.True(confirmedAt!.IsNullable);

        var signature = entityType.FindProperty("DeliverySignature");
        Assert.NotNull(signature);
        Assert.True(signature!.IsNullable);

        var otpCode = entityType.FindProperty("DeliveryOtpCode");
        Assert.NotNull(otpCode);
        Assert.True(otpCode!.IsNullable);
    }
}

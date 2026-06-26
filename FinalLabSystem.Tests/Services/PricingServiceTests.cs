using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class PricingServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static PricingService CreateService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<PricingService>>();
        return new PricingService(ctx, logger.Object);
    }

    [Fact]
    public async Task CreateSchemeAsync_CreatesSchemeInDatabase()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateSchemeAsync_CreatesSchemeInDatabase)));
        var service = CreateService(ctx);
        var scheme = new PriceScheme { SchemeName = "Test Scheme", IsActive = true };

        var result = await service.CreateSchemeAsync(scheme);

        Assert.True(result.SchemeId > 0);
        Assert.Equal("Test Scheme", result.SchemeName);
        Assert.Single(ctx.PriceSchemes);
    }

    [Fact]
    public async Task GetSchemeByIdAsync_ReturnsScheme_WhenExists()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetSchemeByIdAsync_ReturnsScheme_WhenExists)));
        var service = CreateService(ctx);
        var scheme = new PriceScheme { SchemeName = "Existing Scheme", IsActive = true };
        ctx.PriceSchemes.Add(scheme);
        await ctx.SaveChangesAsync();

        var result = await service.GetSchemeByIdAsync(scheme.SchemeId);

        Assert.NotNull(result);
        Assert.Equal("Existing Scheme", result!.SchemeName);
    }

    [Fact]
    public async Task UpdateSchemeAsync_UpdatesExistingScheme()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateSchemeAsync_UpdatesExistingScheme)));
        var service = CreateService(ctx);
        var scheme = new PriceScheme { SchemeName = "Original", IsActive = true, IsDefault = false };
        ctx.PriceSchemes.Add(scheme);
        await ctx.SaveChangesAsync();

        scheme.SchemeName = "Updated";
        scheme.IsDefault = true;
        await service.UpdateSchemeAsync(scheme);

        var updated = await ctx.PriceSchemes.FindAsync(scheme.SchemeId);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.SchemeName);
        Assert.True(updated.IsDefault);
    }

    [Fact]
    public async Task GetTestPriceAsync_ReturnsPrice_WhenExists()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetTestPriceAsync_ReturnsPrice_WhenExists)));
        var service = CreateService(ctx);

        var scheme = new PriceScheme { SchemeName = "S1", IsActive = true };
        ctx.PriceSchemes.Add(scheme);
        await ctx.SaveChangesAsync();

        var cat = new TestCategory { CategoryCode = "C1", CategoryNameEn = "Cat", IsActive = true, SortOrder = 1 };
        ctx.TestCategories.Add(cat);
        await ctx.SaveChangesAsync();

        var group = new TestGroup { CategoryId = cat.CategoryId, GroupCode = "G1", GroupNameEn = "Grp", IsActive = true, SortOrder = 1 };
        ctx.TestGroups.Add(group);
        await ctx.SaveChangesAsync();

        var testType = new TestType { TypeCode = "T1", TypeNameEn = "Test", GroupId = group.GroupId, IsActive = true, SortOrder = 1, TurnaroundHours = 24 };
        ctx.TestTypes.Add(testType);
        await ctx.SaveChangesAsync();

        var price = new TestTypePrice { SchemeId = scheme.SchemeId, TesttypeId = testType.TesttypeId, Price = 75.50m };
        ctx.TestTypePrices.Add(price);
        await ctx.SaveChangesAsync();

        var result = await service.GetTestPriceAsync(testType.TesttypeId, scheme.SchemeId);

        Assert.Equal(75.50m, result);
    }
}

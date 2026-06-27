using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class InventoryAlertEndToEndTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static InventoryService CreateService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<InventoryService>>();
        return new InventoryService(ctx, logger.Object);
    }

    [Fact]
    public async Task EndToEnd_UpdateStock_ChangesQuantity()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_UpdateStock_ChangesQuantity)));

        var material = new TubeMaterial
        {
            TubeMaterialId = 1,
            MaterialName = "EDTA",
            MaterialNameAr = "EDTA",
            TubeColor = "Purple",
            IsActive = true,
            SortOrder = 1,
            CurrentStock = 50,
            MinimumStock = 10
        };
        ctx.TubeMaterials.Add(material);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        await service.UpdateStockAsync(1, -20, "Test adjustment");

        var updated = await ctx.TubeMaterials.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal(30, updated.CurrentStock);
    }

    [Fact]
    public async Task EndToEnd_GetLowStock_ReturnsOnlyLowItems()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_GetLowStock_ReturnsOnlyLowItems)));

        ctx.TubeMaterials.AddRange(
            new TubeMaterial
            {
                TubeMaterialId = 1,
                MaterialName = "EDTA",
                TubeColor = "Purple",
                IsActive = true,
                SortOrder = 1,
                CurrentStock = 5,
                MinimumStock = 10
            },
            new TubeMaterial
            {
                TubeMaterialId = 2,
                MaterialName = "SST",
                TubeColor = "Gold",
                IsActive = true,
                SortOrder = 2,
                CurrentStock = 100,
                MinimumStock = 10
            },
            new TubeMaterial
            {
                TubeMaterialId = 3,
                MaterialName = "Citrate",
                TubeColor = "Blue",
                IsActive = true,
                SortOrder = 3,
                CurrentStock = 3,
                MinimumStock = 15
            });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var lowStock = await service.GetLowStockAsync();

        Assert.Equal(2, lowStock.Count);
        Assert.All(lowStock, m => Assert.True(m.CurrentStock <= m.MinimumStock));
        Assert.Contains(lowStock, m => m.TubeMaterialId == 1);
        Assert.Contains(lowStock, m => m.TubeMaterialId == 3);
    }

    [Fact]
    public async Task EndToEnd_IsLowStock_ReturnsCorrectly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_IsLowStock_ReturnsCorrectly)));

        ctx.TubeMaterials.AddRange(
            new TubeMaterial
            {
                TubeMaterialId = 1,
                MaterialName = "EDTA",
                MaterialNameAr = "أنبوب EDTA",
                TubeColor = "Purple",
                IsActive = true,
                SortOrder = 1,
                CurrentStock = 5,
                MinimumStock = 10
            },
            new TubeMaterial
            {
                TubeMaterialId = 2,
                MaterialName = "SST",
                MaterialNameAr = "أنبوب SST",
                TubeColor = "Gold",
                IsActive = true,
                SortOrder = 2,
                CurrentStock = 100,
                MinimumStock = 10
            });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        var isLowEdta = await service.IsLowStockAsync("EDTA");
        Assert.True(isLowEdta);

        var isLowSst = await service.IsLowStockAsync("SST");
        Assert.False(isLowSst);

        var isLowArabic = await service.IsLowStockAsync("أنبوب EDTA");
        Assert.True(isLowArabic);
    }

    [Fact]
    public async Task EndToEnd_GetLowStockCount_ReturnsAccurateCount()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_GetLowStockCount_ReturnsAccurateCount)));

        ctx.TubeMaterials.AddRange(
            new TubeMaterial
            {
                TubeMaterialId = 1,
                MaterialName = "EDTA",
                TubeColor = "Purple",
                IsActive = true,
                SortOrder = 1,
                CurrentStock = 5,
                MinimumStock = 10
            },
            new TubeMaterial
            {
                TubeMaterialId = 2,
                MaterialName = "SST",
                TubeColor = "Gold",
                IsActive = true,
                SortOrder = 2,
                CurrentStock = 100,
                MinimumStock = 10
            },
            new TubeMaterial
            {
                TubeMaterialId = 3,
                MaterialName = "Citrate",
                TubeColor = "Blue",
                IsActive = true,
                SortOrder = 3,
                CurrentStock = 3,
                MinimumStock = 15
            });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var count = await service.GetLowStockCountAsync();

        Assert.Equal(2, count);
    }
}

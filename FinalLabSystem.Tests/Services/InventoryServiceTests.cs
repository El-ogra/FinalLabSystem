using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class InventoryServiceTests : IDisposable
{
    private readonly FinalLabDbContext _context;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase($"InventoryTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new FinalLabDbContext(options);
        var logger = new Mock<ILogger<InventoryService>>();
        _service = new InventoryService(_context, logger.Object);
    }

    public void Dispose() => _context.Dispose();

    private static TubeMaterial CreateMaterial(string name, string nameAr, int currentStock, int minimumStock, bool isActive = true)
    {
        return new TubeMaterial
        {
            MaterialName = name,
            MaterialNameAr = nameAr,
            TubeColor = "Red",
            CurrentStock = currentStock,
            MinimumStock = minimumStock,
            IsActive = isActive,
            SortOrder = 1
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsActiveMaterials()
    {
        var inactive = CreateMaterial("Inactive", "غير نشط", 20, 5);
        inactive.IsActive = false;
        _context.TubeMaterials.AddRange(
            CreateMaterial("Red Top", "أنابيب حمراء", 50, 10),
            CreateMaterial("EDTA", "EDTA", 30, 5),
            inactive
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsLowStockItems()
    {
        _context.TubeMaterials.AddRange(
            CreateMaterial("Red Top", "أنابيب حمراء", 5, 10),
            CreateMaterial("EDTA", "EDTA", 50, 10),
            CreateMaterial("Fluoride", "Fluoride", 30, 10)
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetLowStockAsync();

        Assert.Single(result);
        Assert.Equal("Red Top", result[0].MaterialName);
    }

    [Fact]
    public async Task GetLowStockAsync_ExcludesZeroMinimum()
    {
        _context.TubeMaterials.AddRange(
            CreateMaterial("Red Top", "أنابيب حمراء", 5, 0),
            CreateMaterial("EDTA", "EDTA", 0, 0)
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetLowStockAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTubeTypeAsync_MatchesArabicName()
    {
        _context.TubeMaterials.Add(CreateMaterial("Red Top", "أنابيب حمراء", 50, 10));
        await _context.SaveChangesAsync();

        var result = await _service.GetByTubeTypeAsync("أنابيب حمراء");

        Assert.NotNull(result);
        Assert.Equal("Red Top", result.MaterialName);
    }

    [Fact]
    public async Task GetByTubeTypeAsync_MatchesEnglishName()
    {
        _context.TubeMaterials.Add(CreateMaterial("Red Top", "أنابيب حمراء", 50, 10));
        await _context.SaveChangesAsync();

        var result = await _service.GetByTubeTypeAsync("Red Top");

        Assert.NotNull(result);
        Assert.Equal("Red Top", result.MaterialName);
    }

    [Fact]
    public async Task GetByTubeTypeAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByTubeTypeAsync("NonExistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task IsLowStockAsync_ReturnsTrue_WhenBelowMinimum()
    {
        _context.TubeMaterials.Add(CreateMaterial("Red Top", "أنابيب حمراء", 5, 10));
        await _context.SaveChangesAsync();

        var result = await _service.IsLowStockAsync("Red Top");

        Assert.True(result);
    }

    [Fact]
    public async Task IsLowStockAsync_ReturnsFalse_WhenAboveMinimum()
    {
        _context.TubeMaterials.Add(CreateMaterial("Red Top", "أنابيب حمراء", 50, 10));
        await _context.SaveChangesAsync();

        var result = await _service.IsLowStockAsync("Red Top");

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStockAsync_AdjustsCurrentStockCorrectly()
    {
        _context.TubeMaterials.Add(CreateMaterial("Red Top", "أنابيب حمراء", 50, 10));
        await _context.SaveChangesAsync();

        var material = _context.TubeMaterials.Single();
        await _service.UpdateStockAsync(material.TubeMaterialId, 10);

        var updated = await _context.TubeMaterials.FindAsync(material.TubeMaterialId);
        Assert.Equal(60, updated!.CurrentStock);
    }

    [Fact]
    public async Task UpdateStockAsync_PreventsNegativeStock()
    {
        _context.TubeMaterials.Add(CreateMaterial("Red Top", "أنابيب حمراء", 5, 10));
        await _context.SaveChangesAsync();

        var material = _context.TubeMaterials.Single();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateStockAsync(material.TubeMaterialId, -10));
    }
}

using FinalLabSystem.Data;
using FinalLabSystem.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalLabSystem.Tests.Validation;

public class TubeMaterialStockMigrationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

    [Fact]
    public void TubeMaterial_HasCurrentStock_DefaultsToZero()
    {
        var material = new TubeMaterial();
        Assert.Equal(0, material.CurrentStock);
    }

    [Fact]
    public void TubeMaterial_HasMinimumStock_DefaultsToZero()
    {
        var material = new TubeMaterial();
        Assert.Equal(0, material.MinimumStock);
    }

    [Fact]
    public void TubeMaterial_CanSetStockValues()
    {
        var material = new TubeMaterial
        {
            MaterialName = "Red Top",
            MaterialNameAr = "أنابيب حمراء",
            CurrentStock = 100,
            MinimumStock = 20
        };

        Assert.Equal(100, material.CurrentStock);
        Assert.Equal(20, material.MinimumStock);
    }
}

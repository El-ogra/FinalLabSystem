using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Tests.Services;

public class FeatureToggleServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

    [Fact]
    public async Task IsEnabledAsync_WithUnknownToggleAndNoDefault_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = new FeatureToggleService(context);

        var result = await service.IsEnabledAsync("UnknownToggle");

        Assert.False(result);
    }
}
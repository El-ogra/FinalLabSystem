using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinalLabSystem.Tests.Services;

public class ExternalLabRegistryServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static ExternalLabRegistryService CreateService(FinalLabDbContext ctx)
        => new ExternalLabRegistryService(ctx);

    private static ExternalLab CreateSampleLab(string name = "Lab Alpha")
        => new()
        {
            LabName = name,
            ContactPerson = "Dr. Smith",
            Phone = "123456",
            Email = "lab@test.com",
            Address = "123 Main St",
            IsActive = true
        };

    [Fact]
    public async Task CreateAsync_ShouldAddLab()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateAsync_ShouldAddLab)));
        var service = CreateService(ctx);

        var lab = await service.CreateAsync(CreateSampleLab("New Lab"));

        Assert.True(lab.ExternalLabId > 0);
        Assert.Equal("New Lab", lab.LabName);
        Assert.True(lab.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnActiveLabsOnly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetAllAsync_ShouldReturnActiveLabsOnly)));
        ctx.ExternalLabs.AddRange(
            CreateSampleLab("Active Lab"),
            new ExternalLab { LabName = "Inactive Lab", IsActive = false }
        );
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var labs = await service.GetAllAsync();

        Assert.Single(labs);
        Assert.Equal("Active Lab", labs[0].LabName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLab()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetByIdAsync_ShouldReturnLab)));
        var lab = CreateSampleLab("Find Me");
        ctx.ExternalLabs.Add(lab);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var result = await service.GetByIdAsync(lab.ExternalLabId);

        Assert.NotNull(result);
        Assert.Equal("Find Me", result!.LabName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetByIdAsync_ShouldReturnNull_WhenNotFound)));
        var service = CreateService(ctx);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyLab()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateAsync_ShouldModifyLab)));
        var lab = CreateSampleLab("Old Name");
        ctx.ExternalLabs.Add(lab);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        lab.LabName = "New Name";
        lab.Phone = "999999";
        await service.UpdateAsync(lab);

        var updated = await ctx.ExternalLabs.FindAsync(lab.ExternalLabId);
        Assert.Equal("New Name", updated!.LabName);
        Assert.Equal("999999", updated.Phone);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSoftDelete_WhenIsActiveSetToFalse()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateAsync_ShouldSoftDelete_WhenIsActiveSetToFalse)));
        var lab = CreateSampleLab("To Delete");
        ctx.ExternalLabs.Add(lab);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        lab.IsActive = false;
        await service.UpdateAsync(lab);

        var updated = await ctx.ExternalLabs.FindAsync(lab.ExternalLabId);
        Assert.False(updated!.IsActive);

        var activeLabs = await service.GetAllAsync();
        Assert.Empty(activeLabs);
    }
}

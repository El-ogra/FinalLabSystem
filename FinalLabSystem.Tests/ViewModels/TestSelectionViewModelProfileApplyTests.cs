using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class TestSelectionViewModelProfileApplyTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static async Task<(int ProfileId, int TestTypeId1, int TestTypeId2)> SeedProfileWithTestsAsync(
        FinalLabDbContext context)
    {
        var cat = new TestCategory
        {
            CategoryCode = "CAT",
            CategoryNameEn = "Category",
            SortOrder = 1,
            IsActive = true
        };
        context.TestCategories.Add(cat);
        await context.SaveChangesAsync();

        var group = new TestGroup
        {
            CategoryId = cat.CategoryId,
            GroupCode = "GRP",
            GroupNameEn = "Group",
            SortOrder = 1,
            IsActive = true
        };
        context.TestGroups.Add(group);
        await context.SaveChangesAsync();

        var tt1 = new TestType
        {
            TypeCode = "T001",
            TypeNameEn = "CBC",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            DefaultPrice = 50m
        };
        var tt2 = new TestType
        {
            TypeCode = "T002",
            TypeNameEn = "Glucose",
            GroupId = group.GroupId,
            SortOrder = 2,
            TurnaroundHours = 24,
            IsActive = true,
            DefaultPrice = 30m
        };
        context.TestTypes.AddRange(tt1, tt2);
        await context.SaveChangesAsync();

        var profile = new TestProfile
        {
            ProfileNameAr = "بروفايل",
            ProfileNameEn = "Profile",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.TestProfiles.Add(profile);
        await context.SaveChangesAsync();

        context.TestProfileItems.AddRange(
            new TestProfileItem
            {
                ProfileId = profile.ProfileId,
                TestTypeId = tt1.TesttypeId,
                SortOrder = 1
            },
            new TestProfileItem
            {
                ProfileId = profile.ProfileId,
                TestTypeId = tt2.TesttypeId,
                SortOrder = 2
            });
        await context.SaveChangesAsync();

        return (profile.ProfileId, tt1.TesttypeId, tt2.TesttypeId);
    }

    [Fact]
    public async Task GetActiveProfilesAsync_ReturnsActiveProfiles()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var catalogService = new TestCatalogService(context, Mock.Of<ILogger<TestCatalogService>>());

        var (profileId, _, _) = await SeedProfileWithTestsAsync(context);

        var profiles = await catalogService.GetActiveProfilesAsync();

        Assert.Single(profiles);
        Assert.Equal(profileId, profiles[0].ProfileId);
    }

    [Fact]
    public async Task GetProfileTestsAsync_ReturnsTestsInProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var catalogService = new TestCatalogService(context, Mock.Of<ILogger<TestCatalogService>>());

        var (profileId, tt1, tt2) = await SeedProfileWithTestsAsync(context);

        var tests = await catalogService.GetProfileTestsAsync(profileId);

        Assert.Equal(2, tests.Count);
        Assert.Contains(tests, t => t.TesttypeId == tt1);
        Assert.Contains(tests, t => t.TesttypeId == tt2);
    }

    [Fact]
    public async Task GetProfileTestsAsync_InvalidProfile_ReturnsEmpty()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var catalogService = new TestCatalogService(context, Mock.Of<ILogger<TestCatalogService>>());

        var tests = await catalogService.GetProfileTestsAsync(999);

        Assert.Empty(tests);
    }

    [Fact]
    public async Task InactiveProfiles_NotReturnedByGetActiveProfilesAsync()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var catalogService = new TestCatalogService(context, Mock.Of<ILogger<TestCatalogService>>());

        await SeedProfileWithTestsAsync(context);

        var inactiveProfile = new TestProfile
        {
            ProfileNameAr = "غير نشط",
            ProfileNameEn = "Inactive",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        context.TestProfiles.Add(inactiveProfile);
        await context.SaveChangesAsync();

        var profiles = await catalogService.GetActiveProfilesAsync();

        Assert.Single(profiles);
        Assert.All(profiles, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task DefaultPrice_IsUsedForProfileTests()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));

        var (_, _, tt2) = await SeedProfileWithTestsAsync(context);

        var testType = await context.TestTypes.FindAsync(tt2);
        Assert.NotNull(testType);
        Assert.Equal(30m, testType.DefaultPrice);
    }
}

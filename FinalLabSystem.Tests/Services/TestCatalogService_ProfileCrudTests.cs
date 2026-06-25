using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class TestCatalogService_ProfileCrudTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static TestCatalogService CreateService(string dbName)
    {
        var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        return new TestCatalogService(context, logger);
    }

    private static async Task<(int TestTypeId1, int TestTypeId2)> SeedTestTypesAsync(FinalLabDbContext context)
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
            TypeNameEn = "Test One",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        var tt2 = new TestType
        {
            TypeCode = "T002",
            TypeNameEn = "Test Two",
            GroupId = group.GroupId,
            SortOrder = 2,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.AddRange(tt1, tt2);
        await context.SaveChangesAsync();

        return (tt1.TesttypeId, tt2.TesttypeId);
    }

    [Fact]
    public async Task CreateProfileAsync_CreatesProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);

        var profile = new TestProfile
        {
            ProfileNameAr = "بروفايل اختبار",
            ProfileNameEn = "Test Profile",
            Description = "Description",
            IsActive = true
        };

        var created = await service.CreateProfileAsync(profile);

        Assert.True(created.ProfileId > 0);
        Assert.Equal("بروفايل اختبار", created.ProfileNameAr);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task GetAllProfilesAsync_ReturnsAllProfiles()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);

        await service.CreateProfileAsync(new TestProfile
        {
            ProfileNameAr = "البروفايل 1",
            ProfileNameEn = "Profile 1",
            IsActive = true
        });
        await service.CreateProfileAsync(new TestProfile
        {
            ProfileNameAr = "البروفايل 2",
            ProfileNameEn = "Profile 2",
            IsActive = true
        });

        var profiles = await service.GetAllProfilesAsync();

        Assert.Equal(2, profiles.Count);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(dbName);

        var profile = new TestProfile
        {
            ProfileNameAr = "الأصلي",
            ProfileNameEn = "Original",
            IsActive = true
        };
        var created = await service.CreateProfileAsync(profile);

        created.ProfileNameAr = "المحدّث";
        await service.UpdateProfileAsync(created);

        var updated = await context.TestProfiles.FindAsync(created.ProfileId);
        Assert.NotNull(updated);
        Assert.Equal("المحدّث", updated.ProfileNameAr);
    }

    [Fact]
    public async Task DeleteProfileAsync_SoftDeletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(dbName);

        var profile = new TestProfile
        {
            ProfileNameAr = "للحذف",
            ProfileNameEn = "To Delete",
            IsActive = true
        };
        var created = await service.CreateProfileAsync(profile);

        await service.DeleteProfileAsync(created.ProfileId);

        var deleted = await context.TestProfiles.FindAsync(created.ProfileId);
        Assert.NotNull(deleted);
        Assert.False(deleted.IsActive);
    }

    [Fact]
    public async Task AddProfileItemAsync_AddsItemToProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(dbName);

        var (tt1, _) = await SeedTestTypesAsync(context);

        var profile = new TestProfile
        {
            ProfileNameAr = "بروفايل",
            ProfileNameEn = "Profile",
            IsActive = true
        };
        var created = await service.CreateProfileAsync(profile);

        await service.AddProfileItemAsync(created.ProfileId, tt1, 1);

        var items = await context.TestProfileItems
            .Where(i => i.ProfileId == created.ProfileId)
            .ToListAsync();

        Assert.Single(items);
        Assert.Equal(tt1, items[0].TestTypeId);
    }

    [Fact]
    public async Task RemoveProfileItemAsync_RemovesItem()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(dbName);

        var (tt1, _) = await SeedTestTypesAsync(context);

        var profile = new TestProfile
        {
            ProfileNameAr = "بروفايل",
            ProfileNameEn = "Profile",
            IsActive = true
        };
        var created = await service.CreateProfileAsync(profile);

        await service.AddProfileItemAsync(created.ProfileId, tt1, 1);

        var item = await context.TestProfileItems.FirstAsync(i => i.ProfileId == created.ProfileId);
        await service.RemoveProfileItemAsync(item.ProfileItemId);

        var items = await context.TestProfileItems
            .Where(i => i.ProfileId == created.ProfileId)
            .ToListAsync();

        Assert.Empty(items);
    }

    [Fact]
    public async Task UpdateProfileItemSortOrderAsync_UpdatesSortOrder()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(dbName);

        var (tt1, _) = await SeedTestTypesAsync(context);

        var profile = new TestProfile
        {
            ProfileNameAr = "بروفايل",
            ProfileNameEn = "Profile",
            IsActive = true
        };
        var created = await service.CreateProfileAsync(profile);

        await service.AddProfileItemAsync(created.ProfileId, tt1, 1);

        var item = await context.TestProfileItems.FirstAsync(i => i.ProfileId == created.ProfileId);
        await service.UpdateProfileItemSortOrderAsync(item.ProfileItemId, 5);

        context.Entry(item).State = EntityState.Detached;
        var updated = await context.TestProfileItems.FindAsync(item.ProfileItemId);
        Assert.NotNull(updated);
        Assert.Equal(5, updated.SortOrder);
    }

    [Fact]
    public async Task GetActiveProfilesAsync_ReturnsOnlyActive()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(dbName);

        await service.CreateProfileAsync(new TestProfile
        {
            ProfileNameAr = "نشط",
            ProfileNameEn = "Active",
            IsActive = true
        });

        var inactiveProfile = new TestProfile
        {
            ProfileNameAr = "غير نشط",
            ProfileNameEn = "Inactive",
            IsActive = false
        };
        context.TestProfiles.Add(inactiveProfile);
        await context.SaveChangesAsync();

        var profiles = await service.GetActiveProfilesAsync();

        Assert.Single(profiles);
        Assert.All(profiles, p => Assert.True(p.IsActive));
    }
}

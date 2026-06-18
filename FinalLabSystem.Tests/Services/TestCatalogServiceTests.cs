using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class TestCatalogServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static async Task SeedSchemesAsync(FinalLabDbContext context)
    {
        context.PriceSchemes.Add(new PriceScheme
        {
            SchemeName = "Patient Price",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        context.PriceSchemes.Add(new PriceScheme
        {
            SchemeName = "Lab-to-Lab Price",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private static async Task<TestGroup> SeedGroupAsync(FinalLabDbContext context)
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
        return group;
    }

    private TestCatalogService CreateService(string dbName)
    {
        var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        return new TestCatalogService(context, logger);
    }

    [Fact]
    public async Task CreateTestTypeAsync_WithValidData_CreatesTestTypeWithPricesTubesAndComponent()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedSchemesAsync(context);
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var entity = new TestType
        {
            TypeCode = "T001",
            TypeNameEn = "Test One",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            PrintWithOther = true,
            AddWithGroup = true
        };

        var tubes = new List<TestTypeSampleTube>
        {
            new() { SampleType = "Serum", Quantity = 1, SortOrder = 1, IsActive = true, TubeType = "Default" }
        };

        var id = await service.CreateTestTypeAsync(entity, 50m, 25m, tubes);

        Assert.True(id > 0);
        var saved = await context.TestTypes
            .Include(tt => tt.TestTypePrices)
            .Include(tt => tt.TestTypeSampleTubes)
            .Include(tt => tt.TestComponents)
            .FirstAsync(tt => tt.TesttypeId == id);

        Assert.Equal("T001", saved.TypeCode);
        Assert.Equal("Test One", saved.TypeNameEn);
        Assert.Equal(2, saved.TestTypePrices.Count);
        Assert.Single(saved.TestTypeSampleTubes);
        Assert.Single(saved.TestComponents);
        Assert.Equal("T001", saved.TestComponents.First().ComponentCode);
        Assert.Equal("NUMERIC", saved.TestComponents.First().ResultType);
    }

    [Fact]
    public async Task CreateTestTypeAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateTestTypeAsync(null!, 0, 0, new List<TestTypeSampleTube>()));
    }

    [Fact]
    public async Task GetTestTypeDetailsAsync_ReturnsEagerLoadedData()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedSchemesAsync(context);
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var entity = new TestType
        {
            TypeCode = "T002",
            TypeNameEn = "Test Two",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 48,
            IsActive = true
        };
        context.TestTypes.Add(entity);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = entity.TesttypeId,
            ComponentCode = "C001",
            ComponentNameEn = "Component One",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 0.5,
            HighNormal = 1.2,
            NormalRangeText = "0.5 - 1.2",
            FastingState = "A",
            Unit = "mg/dL"
        };
        context.NormalRanges.Add(range);
        await context.SaveChangesAsync();

        var details = await service.GetTestTypeDetailsAsync(entity.TesttypeId);

        Assert.NotNull(details);
        Assert.Equal("T002", details.TypeCode);
        Assert.Single(details.TestComponents);
        Assert.Single(details.TestComponents.First().NormalRanges);
    }

    [Fact]
    public async Task UpdateTestTypeAsync_UpdatesFieldsAndPricesAndTubes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedSchemesAsync(context);
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var entity = new TestType
        {
            TypeCode = "T003",
            TypeNameEn = "Original",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(entity);
        await context.SaveChangesAsync();

        entity.TypeNameEn = "Updated";
        entity.SortOrder = 5;

        var tubes = new List<TestTypeSampleTube>
        {
            new() { SampleType = "Plasma", Quantity = 1, SortOrder = 1, IsActive = true, TubeType = "Default" }
        };

        await service.UpdateTestTypeAsync(entity, 100m, 50m, tubes);

        var updated = await context.TestTypes
            .Include(tt => tt.TestTypePrices)
            .Include(tt => tt.TestTypeSampleTubes)
            .FirstAsync(tt => tt.TesttypeId == entity.TesttypeId);

        Assert.Equal("Updated", updated.TypeNameEn);
        Assert.Equal(5, updated.SortOrder);
        Assert.Single(updated.TestTypeSampleTubes);
    }

    [Fact]
    public async Task DeleteTestTypeAsync_WithNoVisits_RemovesEntity()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var entity = new TestType
        {
            TypeCode = "T004",
            TypeNameEn = "Deletable",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(entity);
        await context.SaveChangesAsync();

        await service.DeleteTestTypeAsync(entity.TesttypeId);

        var deleted = await context.TestTypes.FirstOrDefaultAsync(tt => tt.TesttypeId == entity.TesttypeId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteTestTypeAsync_WithVisits_SoftDeletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var entity = new TestType
        {
            TypeCode = "T005",
            TypeNameEn = "WithVisits",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(entity);
        await context.SaveChangesAsync();

        var patient = new Patient
        {
            PatientCode = "P001",
            FullNameAr = "مريض",
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var visit = new Visit
        {
            VisitCode = "V001",
            PatientId = patient.PatientId,
            VisitDate = DateTime.UtcNow
        };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        context.VisitTests.Add(new VisitTest
        {
            VisitId = visit.VisitId,
            TesttypeId = entity.TesttypeId,
            PriceCharged = 50m
        });
        await context.SaveChangesAsync();

        await service.DeleteTestTypeAsync(entity.TesttypeId);

        var softDeleted = await context.TestTypes.FirstAsync(tt => tt.TesttypeId == entity.TesttypeId);
        Assert.False(softDeleted.IsActive);
    }

    [Fact]
    public async Task AddComponentAsync_AddsComponentToTestType()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T006",
            TypeNameEn = "WithComponent",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            ComponentCode = "GLU",
            ComponentNameEn = "Glucose",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };

        var id = await service.AddComponentAsync(testType.TesttypeId, component);

        Assert.True(id > 0);
        var saved = await context.TestComponents.FirstAsync(c => c.ComponentId == id);
        Assert.Equal("GLU", saved.ComponentCode);
        Assert.Equal(testType.TesttypeId, saved.TesttypeId);
    }

    [Fact]
    public async Task UpdateComponentAsync_UpdatesComponentFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T007",
            TypeNameEn = "CompUpdate",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "OLD",
            ComponentNameEn = "Old Name",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        component.ComponentNameEn = "New Name";
        component.Unit = "mg/dL";

        await service.UpdateComponentAsync(component);

        var updated = await context.TestComponents.FirstAsync(c => c.ComponentId == component.ComponentId);
        Assert.Equal("New Name", updated.ComponentNameEn);
        Assert.Equal("mg/dL", updated.Unit);
    }

    [Fact]
    public async Task DeleteComponentAsync_DeletesComponentAndRanges()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T008",
            TypeNameEn = "CompDelete",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "DEL",
            ComponentNameEn = "To Delete",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        context.NormalRanges.Add(new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            FastingState = "A"
        });
        await context.SaveChangesAsync();

        await service.DeleteComponentAsync(component.ComponentId);

        Assert.False(await context.TestComponents.AnyAsync(c => c.ComponentId == component.ComponentId));
        Assert.False(await context.NormalRanges.AnyAsync(r => r.ComponentId == component.ComponentId));
    }

    [Fact]
    public async Task AddRangeAsync_CreatesNewRangeWithVersion1()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T009",
            TypeNameEn = "RangeCreate",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "RNG",
            ComponentNameEn = "Ranges",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 100,
            LowNormal = 1.0,
            HighNormal = 10.0,
            NormalRangeText = "1.0 - 10.0",
            FastingState = "A",
            Unit = "mg/dL"
        };

        var id = await service.AddRangeAsync(range);

        Assert.True(id > 0);
        var saved = await context.NormalRanges.FirstAsync(r => r.RangeId == id);
        Assert.Equal(1.0, saved.LowNormal);
        Assert.Equal(10.0, saved.HighNormal);
        Assert.Equal(1, saved.Version);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task SaveRangeAsync_WithNewRange_CreatesWithVersion1()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T010",
            TypeNameEn = "NewRange",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "NEW",
            ComponentNameEn = "New Range",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "M",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 0.5,
            HighNormal = 1.5,
            NormalRangeText = "0.5 - 1.5",
            FastingState = "A",
            Unit = "mg/dL"
        };

        var saved = await service.SaveRangeAsync(range);

        Assert.True(saved.RangeId > 0);
        Assert.Equal("M", saved.Sex);
        Assert.Equal(1, saved.Version);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task SaveRangeAsync_WithExistingRange_CreatesNewVersionAndSupersedesOld()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T011",
            TypeNameEn = "VersionTest",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "VER",
            ComponentNameEn = "Version Test",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 1.0,
            HighNormal = 10.0,
            NormalRangeText = "1.0 - 10.0",
            FastingState = "A",
            Unit = "mg/dL",
            Version = 1,
            IsActive = true
        };
        context.NormalRanges.Add(range);
        await context.SaveChangesAsync();

        var editRange = new NormalRange
        {
            RangeId = range.RangeId,
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 2.0,
            HighNormal = 8.0,
            NormalRangeText = "2.0 - 8.0",
            FastingState = "A",
            Unit = "mg/dL",
            Version = 1,
            IsActive = true
        };

        var saved = await service.SaveRangeAsync(editRange);

        var allRanges = await context.NormalRanges
            .Where(r => r.ComponentId == component.ComponentId)
            .OrderBy(r => r.RangeId)
            .ToListAsync();

        Assert.Equal(2, allRanges.Count);

        var oldRange = allRanges[0];
        var newRange = allRanges[1];

        Assert.False(oldRange.IsActive);
        Assert.Equal(newRange.RangeId, oldRange.SupersededById);
        Assert.Equal(1, oldRange.Version);
        Assert.Equal(1.0, oldRange.LowNormal);

        Assert.True(newRange.IsActive);
        Assert.Equal(2, newRange.Version);
        Assert.Equal(2.0, newRange.LowNormal);
        Assert.Equal(8.0, newRange.HighNormal);
        Assert.Equal("2.0 - 8.0", newRange.NormalRangeText);
    }

    [Fact]
    public async Task SaveRangeAsync_ThrowsWhenSupersedingInactiveRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T012",
            TypeNameEn = "InactiveRange",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "INA",
            ComponentNameEn = "Inactive",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 1.0,
            HighNormal = 10.0,
            FastingState = "A",
            Version = 1,
            IsActive = false
        };
        context.NormalRanges.Add(range);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveRangeAsync(range));
    }

    [Fact]
    public async Task SaveRangeAsync_OldRangeRetainsSupersededByIdForHistoricalReference()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T013",
            TypeNameEn = "HistRef",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "HST",
            ComponentNameEn = "History",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 1.0,
            HighNormal = 10.0,
            FastingState = "A",
            Version = 1,
            IsActive = true
        };
        context.NormalRanges.Add(range);
        await context.SaveChangesAsync();

        var editRange = new NormalRange
        {
            RangeId = range.RangeId,
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = 2.0,
            HighNormal = 10.0,
            FastingState = "A",
            Version = 1,
            IsActive = true
        };
        var saved = await service.SaveRangeAsync(editRange);

        var oldRange = await context.NormalRanges.FirstAsync(r => r.RangeId == range.RangeId);
        Assert.False(oldRange.IsActive);
        Assert.NotNull(oldRange.SupersededById);
        Assert.Equal(saved.RangeId, oldRange.SupersededById.Value);
    }

    [Fact]
    public async Task GetAllTubeMaterialsAsync_ReturnsOrderedList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        context.TubeMaterials.Add(new TubeMaterial { MaterialName = "Plasma", SortOrder = 2, IsActive = true });
        context.TubeMaterials.Add(new TubeMaterial { MaterialName = "Serum", SortOrder = 1, IsActive = true });
        await context.SaveChangesAsync();

        var result = await service.GetAllTubeMaterialsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Serum", result[0].MaterialName);
        Assert.Equal("Plasma", result[1].MaterialName);
    }

    [Fact]
    public async Task CreateTubeMaterialAsync_AddsMaterial()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var material = new TubeMaterial { MaterialName = "Urine" };
        var created = await service.CreateTubeMaterialAsync(material);

        Assert.True(created.TubeMaterialId > 0);
        Assert.Equal("Urine", created.MaterialName);
        Assert.True(created.IsActive);
        Assert.Equal(1, created.SortOrder);

        var second = new TubeMaterial { MaterialName = "CSF" };
        var created2 = await service.CreateTubeMaterialAsync(second);
        Assert.Equal(2, created2.SortOrder);
    }

    [Fact]
    public async Task DeleteTubeMaterialAsync_WhenNotUsed_Deletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var material = new TubeMaterial { MaterialName = "Urine", SortOrder = 1, IsActive = true };
        context.TubeMaterials.Add(material);
        await context.SaveChangesAsync();

        var result = await service.DeleteTubeMaterialAsync(material.TubeMaterialId);

        Assert.True(result);
        Assert.Null(await context.TubeMaterials.FindAsync(material.TubeMaterialId));
    }

    [Fact]
    public async Task DeleteTubeMaterialAsync_WhenUsed_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedSchemesAsync(context);
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var material = new TubeMaterial { MaterialName = "Serum", SortOrder = 1, IsActive = true };
        context.TubeMaterials.Add(material);
        await context.SaveChangesAsync();

        var testType = new TestType
        {
            TypeCode = "TUBETEST",
            TypeNameEn = "TubeTest",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        context.TestTypeSampleTubes.Add(new TestTypeSampleTube
        {
            TestTypeId = testType.TesttypeId,
            TubeType = "Serum",
            SampleType = "Serum",
            Quantity = 1,
            SortOrder = 1,
            IsActive = true
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteTubeMaterialAsync(material.TubeMaterialId));

        Assert.NotNull(await context.TubeMaterials.FindAsync(material.TubeMaterialId));
    }

    [Fact]
    public async Task DeleteRangeAsync_RemovesRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T014",
            TypeNameEn = "DelRange",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "DELR",
            ComponentNameEn = "Delete Range",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            FastingState = "A"
        };
        context.NormalRanges.Add(range);
        await context.SaveChangesAsync();

        await service.DeleteRangeAsync(range.RangeId);

        Assert.False(await context.NormalRanges.AnyAsync(r => r.RangeId == range.RangeId));
    }

    [Fact]
    public async Task GetRangesForComponentAsync_ReturnsOrderedRanges()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T015",
            TypeNameEn = "GetRanges",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "GETR",
            ComponentNameEn = "Get Ranges",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        context.NormalRanges.Add(new NormalRange { ComponentId = component.ComponentId, Sex = "F", AgeFromDays = 0, AgeToDays = 100, FastingState = "A" });
        context.NormalRanges.Add(new NormalRange { ComponentId = component.ComponentId, Sex = "M", AgeFromDays = 0, AgeToDays = 100, FastingState = "A" });
        context.NormalRanges.Add(new NormalRange { ComponentId = component.ComponentId, Sex = "B", AgeFromDays = 0, AgeToDays = 100, FastingState = "A" });
        await context.SaveChangesAsync();

        var ranges = await service.GetRangesForComponentAsync(component.ComponentId);

        Assert.Equal(3, ranges.Count);
    }

    [Fact]
    public async Task GetRangesForTestTypeAsync_ReturnsRangesWithComponents()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var group = await SeedGroupAsync(context);
        var logger = Mock.Of<ILogger<TestCatalogService>>();
        var service = new TestCatalogService(context, logger);

        var testType = new TestType
        {
            TypeCode = "T016",
            TypeNameEn = "CrossEntity",
            GroupId = group.GroupId,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };
        context.TestTypes.Add(testType);
        await context.SaveChangesAsync();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "XENT",
            ComponentNameEn = "Cross Entity",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        context.NormalRanges.Add(new NormalRange { ComponentId = component.ComponentId, Sex = "B", AgeFromDays = 0, AgeToDays = 36500, FastingState = "A" });
        await context.SaveChangesAsync();

        var ranges = await service.GetRangesForTestTypeAsync(testType.TesttypeId);

        Assert.Single(ranges);
        Assert.NotNull(ranges[0].Component);
        Assert.Equal("XENT", ranges[0].Component.ComponentCode);
    }
}

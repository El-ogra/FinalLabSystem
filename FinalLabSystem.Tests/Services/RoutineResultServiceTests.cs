using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class RoutineResultServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static async Task<(TestComponent Component, NormalRange Range)> SeedRangeAsync(
        FinalLabDbContext context, string sex = "B", int ageFrom = 0, int ageTo = 36500,
        double? lowNormal = 0.5, double? highNormal = 1.5,
        double? lowCritical = null, double? highCritical = null,
        bool? forPregnantOnly = null)
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

        var testType = new TestType
        {
            TypeCode = "T001",
            TypeNameEn = "Test",
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
            ComponentCode = "GLU",
            ComponentNameEn = "Glucose",
            ResultType = "NUMERIC",
            Unit = "mg/dL",
            IsActive = true,
            SortOrder = 1
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        var range = new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = sex,
            AgeFromDays = ageFrom,
            AgeToDays = ageTo,
            LowNormal = lowNormal,
            HighNormal = highNormal,
            LowCritical = lowCritical,
            HighCritical = highCritical,
            ForPregnantOnly = forPregnantOnly,
            NormalRangeText = $"{lowNormal} - {highNormal}",
            FastingState = "A",
            Unit = "mg/dL",
            IsActive = true
        };
        context.NormalRanges.Add(range);
        await context.SaveChangesAsync();

        return (component, range);
    }

    private static async Task<(int PatientId, int VisitTestId)> SeedPatientAndVisitAsync(
        FinalLabDbContext context, string sex = "M", DateOnly? dob = null,
        int? approxAge = null, bool isPregnant = false)
    {
        var patient = new Patient
        {
            PatientCode = "P001",
            FullNameAr = "مريض",
            Sex = sex,
            PatientType = "Individual",
            DateOfBirth = dob,
            ApproxAge = approxAge,
            CreatedAt = DateTime.UtcNow
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var visit = new Visit
        {
            VisitCode = "V001",
            PatientId = patient.PatientId,
            VisitDate = DateTime.UtcNow,
            IsPregnant = isPregnant
        };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var testType = await context.TestTypes.FirstAsync();

        var visitTest = new VisitTest
        {
            VisitId = visit.VisitId,
            TesttypeId = testType.TesttypeId,
            PriceCharged = 50m
        };
        context.VisitTests.Add(visitTest);
        await context.SaveChangesAsync();

        return (patient.PatientId, visitTest.VisitTestId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithNormalValue_FlagsNormal()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "1.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("NORMAL", saved.ResultStatus);
        Assert.Equal("1.0", saved.ResultValue);
        Assert.Equal(1.0m, saved.ResultNumeric);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithHighValue_FlagsHigh()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "2.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("HIGH", saved.ResultStatus);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithLowValue_FlagsLow()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "0.1"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("LOW", saved.ResultStatus);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithHighCriticalValue_FlagsHighCritical()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, highCritical: 2.5);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "3.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("HIGH_CRITICAL", saved.ResultStatus);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithLowCriticalValue_FlagsLowCritical()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, lowCritical: 0.2);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "0.1"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("LOW_CRITICAL", saved.ResultStatus);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithSexFilter_SelectsCorrectRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "M");
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "M");

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "0.8"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("NORMAL", saved.ResultStatus);
        Assert.NotNull(saved.NormalRangeId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithSexMismatch_NoMatchingRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "F");
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "M");

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "0.8"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Null(saved.NormalRangeId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithBothSex_MatchesAnySex()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "B");
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "F");

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "0.8"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("NORMAL", saved.ResultStatus);
        Assert.NotNull(saved.NormalRangeId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithAgeRange_SelectsCorrectRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var component = new TestComponent
        {
            TesttypeId = (await SeedRangeAsync(context)).Component.TesttypeId,
            ComponentCode = "AGE",
            ComponentNameEn = "Age Range",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = 2
        };
        context.TestComponents.Add(component);
        await context.SaveChangesAsync();

        context.NormalRanges.Add(new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 365,
            LowNormal = 10.0,
            HighNormal = 20.0,
            NormalRangeText = "10 - 20",
            FastingState = "A",
            Unit = "mg/dL"
        });
        context.NormalRanges.Add(new NormalRange
        {
            ComponentId = component.ComponentId,
            Sex = "B",
            AgeFromDays = 366,
            AgeToDays = 36500,
            LowNormal = 15.0,
            HighNormal = 25.0,
            NormalRangeText = "15 - 25",
            FastingState = "A",
            Unit = "mg/dL"
        });
        await context.SaveChangesAsync();

        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "M", dob: dob);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "18.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync(r => r.ComponentId == component.ComponentId);
        Assert.Equal("NORMAL", saved.ResultStatus);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithPregnantFlag_SelectsCorrectRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "B", forPregnantOnly: true,
            lowNormal: 5.0, highNormal: 10.0);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "F", isPregnant: true);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "7.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("NORMAL", saved.ResultStatus);
        Assert.NotNull(saved.NormalRangeId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithNonPregnantPatient_DoesNotMatchPregnantOnlyRange()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "B", forPregnantOnly: true,
            lowNormal: 5.0, highNormal: 10.0);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "F", isPregnant: false);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "7.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Null(saved.NormalRangeId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithApproxAge_CalculatesAgeInDays()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "B",
            ageFrom: 0, ageTo: 3650);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context, sex: "M",
            approxAge: 5);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "0.8"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("NORMAL", saved.ResultStatus);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_WithInactiveRange_DoesNotMatch()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, _) = await SeedRangeAsync(context, sex: "B",
            lowNormal: 0.5, highNormal: 1.5);

        var inactive = await context.NormalRanges.FirstAsync();
        inactive.IsActive = false;
        await context.SaveChangesAsync();

        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "1.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Null(saved.NormalRangeId);
    }

    [Fact]
    public async Task SaveNumericOrTextResultsAsync_SnapshotsRangeFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<RoutineResultService>>();
        var service = new RoutineResultService(context, logger);

        var (component, range) = await SeedRangeAsync(context, sex: "B",
            lowNormal: 0.5, highNormal: 1.5);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "1.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.NotNull(saved.NormalRangeId);
        Assert.Equal(0.5, saved.SnapLowNormal);
        Assert.Equal(1.5, saved.SnapHighNormal);
        Assert.Equal("0.5 - 1.5", saved.SnapNormalText);
        Assert.Equal("mg/dL", saved.SnapUnit);
    }
}

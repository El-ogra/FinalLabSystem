using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class RoutineResultServiceLiveValidationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static RoutineResultService CreateService(FinalLabDbContext context)
    {
        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(false);

        return new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object,
            Mock.Of<IReportCommentEngine>());
    }

    private static async Task<(TestComponent Component, NormalRange Range)> SeedRangeAsync(
        FinalLabDbContext context,
        double? lowNormal = 0.5, double? highNormal = 1.5,
        double? lowCritical = null, double? highCritical = null)
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
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            LowNormal = lowNormal,
            HighNormal = highNormal,
            LowCritical = lowCritical,
            HighCritical = highCritical,
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
        FinalLabDbContext context)
    {
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
    public async Task Save_WithNormalValue_SnapshotsNormalRangeFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var (component, _) = await SeedRangeAsync(context, lowNormal: 0.5, highNormal: 1.5);
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
        Assert.Equal(0.5, saved.SnapLowNormal);
        Assert.Equal(1.5, saved.SnapHighNormal);
        Assert.Equal("0.5 - 1.5", saved.SnapNormalText);
        Assert.Equal("mg/dL", saved.SnapUnit);
    }

    [Fact]
    public async Task Save_WithCriticalRange_SnapshotsCriticalFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var (component, _) = await SeedRangeAsync(context,
            lowNormal: 0.5, highNormal: 1.5, lowCritical: 0.2, highCritical: 2.5);
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
        Assert.Equal(0.2, saved.SnapLowCritical);
        Assert.Equal(2.5, saved.SnapHighCritical);
    }

    [Fact]
    public async Task Save_TextResult_DoesNotSetResultNumeric()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var (component, _) = await SeedRangeAsync(context);
        var (patientId, visitTestId) = await SeedPatientAndVisitAsync(context);

        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "Positive"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Null(saved.ResultNumeric);
        Assert.Null(saved.ResultStatus);
    }

    [Fact]
    public async Task ToggleReviewStatusAsync_WithEnteredResults_SetsToReviewed()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

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

        var toggleResult = await service.ToggleReviewStatusAsync(visitTestId, 1);

        Assert.True(toggleResult);
        var saved = await context.TestResults.FirstAsync();
        Assert.Equal(ResultValidationStatus.Reviewed, saved.ValidationStatus);
    }

    [Fact]
    public async Task ToggleReviewStatusAsync_WithNoResults_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var result = await service.ToggleReviewStatusAsync(999, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task ToggleReviewStatusAsync_AlreadyReviewed_ReturnsFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

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
        await service.ToggleReviewStatusAsync(visitTestId, 1);

        var secondToggle = await service.ToggleReviewStatusAsync(visitTestId, 1);

        Assert.False(secondToggle);
    }
}

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

public class AutoCommentEndToEndTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static async Task SeedFullSetupAsync(FinalLabDbContext context)
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
            TypeNameEn = "CBC",
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
            ComponentCode = "WBC",
            ComponentNameEn = "White Blood Cells",
            ResultType = "NUMERIC",
            Unit = "10^3/uL",
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
            LowNormal = 4.0,
            HighNormal = 11.0,
            LowCritical = 2.0,
            HighCritical = 20.0,
            NormalRangeText = "4.0 - 11.0",
            FastingState = "A",
            Unit = "10^3/uL",
            IsActive = true
        });
        await context.SaveChangesAsync();

        context.ReportCommentTemplates.Add(new ReportCommentTemplate
        {
            Title = "Low WBC",
            CommentText = "Leukopenia detected - follow up recommended",
            CommentLang = "EN",
            TriggerCondition = "Low",
            TesttypeId = testType.TesttypeId,
            IsActive = true,
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow
        });
        context.ReportCommentTemplates.Add(new ReportCommentTemplate
        {
            Title = "High WBC",
            CommentText = "Leukocytosis detected - consider infection workup",
            CommentLang = "EN",
            TriggerCondition = "High",
            TesttypeId = testType.TesttypeId,
            IsActive = true,
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow
        });
        context.ReportCommentTemplates.Add(new ReportCommentTemplate
        {
            Title = "Critical",
            CommentText = "Critical value - immediate attention required",
            CommentLang = "EN",
            TriggerCondition = "Critical",
            TesttypeId = testType.TesttypeId,
            IsActive = true,
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private static async Task<(int PatientId, int VisitTestId)> SeedPatientAsync(FinalLabDbContext context)
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

    private static ReportCommentTemplateService CreateTemplateService(FinalLabDbContext context)
        => new ReportCommentTemplateService(context);

    [Fact]
    public async Task EndToEnd_LowResult_AutoCommentInjected()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedFullSetupAsync(context);
        var (patientId, visitTestId) = await SeedPatientAsync(context);

        var templateService = CreateTemplateService(context);
        var commentEngine = new ReportCommentEngine(templateService);

        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(false);

        var service = new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object,
            commentEngine);

        var component = await context.TestComponents.FirstAsync();
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
        Assert.Equal("LOW", saved.ResultStatus);
        Assert.Equal("Leukopenia detected - follow up recommended", saved.Comment);
    }

    [Fact]
    public async Task EndToEnd_HighResult_AutoCommentInjected()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedFullSetupAsync(context);
        var (patientId, visitTestId) = await SeedPatientAsync(context);

        var templateService = CreateTemplateService(context);
        var commentEngine = new ReportCommentEngine(templateService);

        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(false);

        var service = new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object,
            commentEngine);

        var component = await context.TestComponents.FirstAsync();
        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "15.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("HIGH", saved.ResultStatus);
        Assert.Equal("Leukocytosis detected - consider infection workup", saved.Comment);
    }

    [Fact]
    public async Task EndToEnd_NormalResult_NoCommentInjected()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedFullSetupAsync(context);
        var (patientId, visitTestId) = await SeedPatientAsync(context);

        var templateService = CreateTemplateService(context);
        var commentEngine = new ReportCommentEngine(templateService);

        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(false);

        var service = new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object,
            commentEngine);

        var component = await context.TestComponents.FirstAsync();
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
        Assert.Null(saved.Comment);
    }

    [Fact]
    public async Task EndToEnd_HighCriticalResult_CriticalCommentInjected()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedFullSetupAsync(context);
        var (patientId, visitTestId) = await SeedPatientAsync(context);

        var templateService = CreateTemplateService(context);
        var commentEngine = new ReportCommentEngine(templateService);

        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(false);

        var service = new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object,
            commentEngine);

        var component = await context.TestComponents.FirstAsync();
        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "25.0"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("HIGH_CRITICAL", saved.ResultStatus);
        Assert.Equal("Critical value - immediate attention required", saved.Comment);
    }

    [Fact]
    public async Task EndToEnd_ManualComment_NotOverwritten()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        await SeedFullSetupAsync(context);
        var (patientId, visitTestId) = await SeedPatientAsync(context);

        var templateService = CreateTemplateService(context);
        var commentEngine = new ReportCommentEngine(templateService);

        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(false);

        var service = new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object,
            commentEngine);

        var component = await context.TestComponents.FirstAsync();
        var results = new List<TestResult>
        {
            new()
            {
                VisitTestId = visitTestId,
                ComponentId = component.ComponentId,
                ResultValue = "3.0",
                Comment = "Manual clinical note"
            }
        };

        await service.SaveNumericOrTextResultsAsync(results, patientId, 1);

        var saved = await context.TestResults.FirstAsync();
        Assert.Equal("Manual clinical note", saved.Comment);
    }
}

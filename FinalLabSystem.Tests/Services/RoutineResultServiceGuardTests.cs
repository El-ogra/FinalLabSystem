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

public class RoutineResultServiceGuardTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static RoutineResultService CreateService(FinalLabDbContext context, bool enforceStageGating = true)
    {
        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService
            .Setup(s => s.IsEnabledAsync(FeatureToggles.EnforceStageGating, false))
            .ReturnsAsync(enforceStageGating);

        return new RoutineResultService(
            context,
            Mock.Of<ILogger<RoutineResultService>>(),
            featureToggleService.Object);
    }
    private static async Task<int> SeedVisitTestWithResultsAsync(FinalLabDbContext context, params ResultValidationStatus[] statuses)
    {
        var vt = new VisitTest
        {
            Visit = new Visit
            {
                VisitCode = "V001",
                VisitDate = DateTime.UtcNow
            },
            Testtype = new TestType
            {
                TypeCode = "T001",
                TypeNameEn = "Test",
                SortOrder = 1,
                TurnaroundHours = 24,
                IsActive = true
            },
            PriceCharged = 50m
        };
        context.VisitTests.Add(vt);
        await context.SaveChangesAsync();

        foreach (var status in statuses)
        {
            context.TestResults.Add(new TestResult
            {
                VisitTestId = vt.VisitTestId,
                Component = new TestComponent
                {
                    ComponentCode = $"C_{Guid.NewGuid():N}",
                    ComponentNameEn = "Component",
                    ResultType = "NUMERIC",
                    IsActive = true,
                    SortOrder = 1
                },
                ValidationStatus = status
            });
        }
        await context.SaveChangesAsync();

        return vt.VisitTestId;
    }

    [Fact]
    public async Task TogglePrintStatusAsync_WhenNotReviewed_ThrowsInvalidOperationException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var visitTestId = await SeedVisitTestWithResultsAsync(context, ResultValidationStatus.Entered);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.TogglePrintStatusAsync(visitTestId, 1));
        Assert.Contains("يجب مراجعة النتائج قبل الطباعة", ex.Message);
    }

    [Fact]
    public async Task TogglePrintStatusAsync_WhenReviewed_PrintsSuccessfully()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var visitTestId = await SeedVisitTestWithResultsAsync(context, ResultValidationStatus.Reviewed);

        var result = await service.TogglePrintStatusAsync(visitTestId, 1);

        Assert.True(result);

        var vt = await context.VisitTests.FindAsync(visitTestId);
        Assert.NotNull(vt);
        Assert.True(vt.IsPrinted);
    }

    [Fact]
    public async Task ToggleExportStatusAsync_WhenNotPrinted_ThrowsInvalidOperationException()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var visitTestId = await SeedVisitTestWithResultsAsync(context, ResultValidationStatus.Reviewed);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ToggleExportStatusAsync(visitTestId, 1));
        Assert.Contains("يجب طباعة النتائج قبل التسليم", ex.Message);
    }

    [Fact]
    public async Task ToggleExportStatusAsync_WhenPrinted_ExportsSuccessfully()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var visitTestId = await SeedVisitTestWithResultsAsync(context, ResultValidationStatus.Reviewed);

        var vt = await context.VisitTests.FindAsync(visitTestId);
        Assert.NotNull(vt);
        vt.IsPrinted = true;
        vt.PrintedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var result = await service.ToggleExportStatusAsync(visitTestId, 1);

        Assert.True(result);

        var updated = await context.VisitTests.FindAsync(visitTestId);
        Assert.NotNull(updated);
        Assert.True(updated.IsExported);
    }
    [Fact]
    public async Task TogglePrintStatusAsync_WhenGatingDisabled_PrintsWithoutReviewedResults()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context, enforceStageGating: false);

        var visitTestId = await SeedVisitTestWithResultsAsync(context, ResultValidationStatus.Entered);

        var result = await service.TogglePrintStatusAsync(visitTestId, 1);

        Assert.True(result);

        var vt = await context.VisitTests.FindAsync(visitTestId);
        Assert.NotNull(vt);
        Assert.True(vt.IsPrinted);
    }

    [Fact]
    public async Task ToggleExportStatusAsync_WhenGatingDisabled_ExportsWithoutPrintedResults()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context, enforceStageGating: false);

        var visitTestId = await SeedVisitTestWithResultsAsync(context, ResultValidationStatus.Reviewed);

        var result = await service.ToggleExportStatusAsync(visitTestId, 1);

        Assert.True(result);

        var vt = await context.VisitTests.FindAsync(visitTestId);
        Assert.NotNull(vt);
        Assert.True(vt.IsExported);
    }
}

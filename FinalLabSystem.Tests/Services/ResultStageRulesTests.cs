using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Tests.Services;

public class ResultStageRulesTests
{
    private static VisitTest CreateVisitTest(params ResultValidationStatus[] statuses)
    {
        var vt = new VisitTest();
        foreach (var status in statuses)
        {
            vt.TestResults.Add(new TestResult { ValidationStatus = status });
        }
        return vt;
    }

    [Fact]
    public void CanPrint_WhenAllResultsReviewed_ReturnsTrue()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Reviewed, ResultValidationStatus.Reviewed);
        Assert.True(ResultStageRules.CanPrint(vt));
    }

    [Fact]
    public void CanPrint_WhenAnyResultEntered_ReturnsFalse()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Entered, ResultValidationStatus.Reviewed);
        Assert.False(ResultStageRules.CanPrint(vt));
    }

    [Fact]
    public void CanExport_WhenPrinted_ReturnsTrue()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Reviewed, ResultValidationStatus.Reviewed);
        vt.IsPrinted = true;
        Assert.True(ResultStageRules.CanExport(vt));
    }

    [Fact]
    public void CanExport_WhenNotPrinted_ReturnsFalse()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Reviewed, ResultValidationStatus.Reviewed);
        Assert.False(ResultStageRules.CanExport(vt));
    }

    [Fact]
    public void CanDeliver_WhenNotPrinted_ReturnsFalse()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Reviewed);
        Assert.False(ResultStageRules.CanDeliver(vt));
    }

    [Fact]
    public void CanDeliver_WhenPrinted_ReturnsTrue()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Reviewed);
        vt.IsPrinted = true;
        Assert.True(ResultStageRules.CanDeliver(vt));
    }

    [Fact]
    public void GetMinimumValidationStatus_ReturnsLowestAcrossResults()
    {
        var vt = CreateVisitTest(ResultValidationStatus.Validated, ResultValidationStatus.Entered, ResultValidationStatus.Reviewed);
        Assert.Equal(ResultValidationStatus.Entered, ResultStageRules.GetMinimumValidationStatus(vt));
    }

    [Fact]
    public void GetMinimumValidationStatus_WithNoResults_ReturnsEntered()
    {
        var vt = new VisitTest();
        Assert.Equal(ResultValidationStatus.Entered, ResultStageRules.GetMinimumValidationStatus(vt));
    }

    [Fact]
    public void CanPrint_WithNoResults_ReturnsFalse()
    {
        var vt = new VisitTest();
        Assert.False(ResultStageRules.CanPrint(vt));
    }
}

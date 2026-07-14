using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels;

public class PatientStatusComputationTests
{
    private static TodayPatientWithStatusDto CreateDto(VisitDisplayStatus status)
    {
        return new TodayPatientWithStatusDto
        {
            PatientId = 1,
            VisitId = 1,
            PatientCode = "P001",
            FullNameAr = "مريض تجريبي",
            ComputedStatus = status,
            StatusIcon = GetExpectedIcon(status),
            StatusColor = GetExpectedColor(status)
        };
    }

    private static string GetExpectedIcon(VisitDisplayStatus status) => status switch
    {
        VisitDisplayStatus.NewNoResults => "\U0001F6D2",
        VisitDisplayStatus.ResultsNotWritten => "\U0001F6D2",
        VisitDisplayStatus.ResultsNotReviewed => "\U0001F3C5",
        VisitDisplayStatus.ResultsNotPrinted => "\U0001F4C4",
        VisitDisplayStatus.NotDelivered => "\U0001F5A8",
        VisitDisplayStatus.DeliveredWithBalance => "\U0001F4B2",
        VisitDisplayStatus.FullyComplete => "\u2705",
        _ => "\U0001F6D2"
    };

    private static string GetExpectedColor(VisitDisplayStatus status) => status switch
    {
        VisitDisplayStatus.NewNoResults => "#808080",
        VisitDisplayStatus.ResultsNotWritten => "#FF8C00",
        VisitDisplayStatus.ResultsNotReviewed => "#FFD700",
        VisitDisplayStatus.ResultsNotPrinted => "#4FC3F7",
        VisitDisplayStatus.NotDelivered => "#9C27B0",
        VisitDisplayStatus.DeliveredWithBalance => "#F44336",
        VisitDisplayStatus.FullyComplete => "#4CAF50",
        _ => "#808080"
    };

    [Fact]
    public void NewNoResults_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.NewNoResults);
        Assert.Equal(VisitDisplayStatus.NewNoResults, dto.ComputedStatus);
        Assert.Equal("\U0001F6D2", dto.StatusIcon);
        Assert.Equal("#808080", dto.StatusColor);
    }

    [Fact]
    public void ResultsNotWritten_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.ResultsNotWritten);
        Assert.Equal(VisitDisplayStatus.ResultsNotWritten, dto.ComputedStatus);
        Assert.Equal("\U0001F6D2", dto.StatusIcon);
        Assert.Equal("#FF8C00", dto.StatusColor);
    }

    [Fact]
    public void ResultsNotReviewed_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.ResultsNotReviewed);
        Assert.Equal(VisitDisplayStatus.ResultsNotReviewed, dto.ComputedStatus);
        Assert.Equal("\U0001F3C5", dto.StatusIcon);
        Assert.Equal("#FFD700", dto.StatusColor);
    }

    [Fact]
    public void ResultsNotPrinted_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.ResultsNotPrinted);
        Assert.Equal(VisitDisplayStatus.ResultsNotPrinted, dto.ComputedStatus);
        Assert.Equal("\U0001F4C4", dto.StatusIcon);
        Assert.Equal("#4FC3F7", dto.StatusColor);
    }

    [Fact]
    public void NotDelivered_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.NotDelivered);
        Assert.Equal(VisitDisplayStatus.NotDelivered, dto.ComputedStatus);
        Assert.Equal("\U0001F5A8", dto.StatusIcon);
        Assert.Equal("#9C27B0", dto.StatusColor);
    }

    [Fact]
    public void DeliveredWithBalance_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.DeliveredWithBalance);
        Assert.Equal(VisitDisplayStatus.DeliveredWithBalance, dto.ComputedStatus);
        Assert.Equal("\U0001F4B2", dto.StatusIcon);
        Assert.Equal("#F44336", dto.StatusColor);
    }

    [Fact]
    public void FullyComplete_HasCorrectIconAndColor()
    {
        var dto = CreateDto(VisitDisplayStatus.FullyComplete);
        Assert.Equal(VisitDisplayStatus.FullyComplete, dto.ComputedStatus);
        Assert.Equal("\u2705", dto.StatusIcon);
        Assert.Equal("#4CAF50", dto.StatusColor);
    }
}

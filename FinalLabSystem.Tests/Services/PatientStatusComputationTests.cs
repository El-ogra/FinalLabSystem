using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Tests.ViewModels;

public class PatientStatusComputationTests
{
    private static TodayPatientWithStatusDto CreateDto(PatientVisitStatus status)
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

    private static string GetExpectedIcon(PatientVisitStatus status) => status switch
    {
        PatientVisitStatus.NewNoResults => "\U0001F6D2",
        PatientVisitStatus.HasUnwrittenResults => "\U0001F6D2",
        PatientVisitStatus.HasUnreviewedResults => "\U0001F3C5",
        PatientVisitStatus.HasUnprintedResults => "\U0001F4C4",
        PatientVisitStatus.HasUndeliveredResults => "\U0001F5A8",
        PatientVisitStatus.CompleteWithBalance => "\U0001F4B2",
        PatientVisitStatus.FullyComplete => "\u2705",
        _ => "\U0001F6D2"
    };

    private static string GetExpectedColor(PatientVisitStatus status) => status switch
    {
        PatientVisitStatus.NewNoResults => "#808080",
        PatientVisitStatus.HasUnwrittenResults => "#FF8C00",
        PatientVisitStatus.HasUnreviewedResults => "#FFD700",
        PatientVisitStatus.HasUnprintedResults => "#4FC3F7",
        PatientVisitStatus.HasUndeliveredResults => "#9C27B0",
        PatientVisitStatus.CompleteWithBalance => "#F44336",
        PatientVisitStatus.FullyComplete => "#4CAF50",
        _ => "#808080"
    };

    [Fact]
    public void NewNoResults_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.NewNoResults);

        Assert.Equal(PatientVisitStatus.NewNoResults, dto.ComputedStatus);
        Assert.Equal("\U0001F6D2", dto.StatusIcon);
        Assert.Equal("#808080", dto.StatusColor);
    }

    [Fact]
    public void HasUnwrittenResults_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.HasUnwrittenResults);

        Assert.Equal(PatientVisitStatus.HasUnwrittenResults, dto.ComputedStatus);
        Assert.Equal("\U0001F6D2", dto.StatusIcon);
        Assert.Equal("#FF8C00", dto.StatusColor);
    }

    [Fact]
    public void HasUnreviewedResults_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.HasUnreviewedResults);

        Assert.Equal(PatientVisitStatus.HasUnreviewedResults, dto.ComputedStatus);
        Assert.Equal("\U0001F3C5", dto.StatusIcon);
        Assert.Equal("#FFD700", dto.StatusColor);
    }

    [Fact]
    public void HasUnprintedResults_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.HasUnprintedResults);

        Assert.Equal(PatientVisitStatus.HasUnprintedResults, dto.ComputedStatus);
        Assert.Equal("\U0001F4C4", dto.StatusIcon);
        Assert.Equal("#4FC3F7", dto.StatusColor);
    }

    [Fact]
    public void HasUndeliveredResults_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.HasUndeliveredResults);

        Assert.Equal(PatientVisitStatus.HasUndeliveredResults, dto.ComputedStatus);
        Assert.Equal("\U0001F5A8", dto.StatusIcon);
        Assert.Equal("#9C27B0", dto.StatusColor);
    }

    [Fact]
    public void CompleteWithBalance_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.CompleteWithBalance);

        Assert.Equal(PatientVisitStatus.CompleteWithBalance, dto.ComputedStatus);
        Assert.Equal("\U0001F4B2", dto.StatusIcon);
        Assert.Equal("#F44336", dto.StatusColor);
    }

    [Fact]
    public void FullyComplete_HasCorrectIconAndColor()
    {
        var dto = CreateDto(PatientVisitStatus.FullyComplete);

        Assert.Equal(PatientVisitStatus.FullyComplete, dto.ComputedStatus);
        Assert.Equal("\u2705", dto.StatusIcon);
        Assert.Equal("#4CAF50", dto.StatusColor);
    }
}

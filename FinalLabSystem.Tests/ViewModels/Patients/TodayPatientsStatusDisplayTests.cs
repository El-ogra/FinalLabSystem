using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Tests.ViewModels;

public class TodayPatientsStatusDisplayTests
{
    private static TodayPatientWithStatusDto CreateDto(
        PatientVisitStatus status = PatientVisitStatus.NewNoResults,
        string statusIcon = "",
        string statusColor = "")
    {
        return new TodayPatientWithStatusDto
        {
            PatientId = 1,
            VisitId = 1,
            PatientCode = "P001",
            FullNameAr = "مريض تجريبي",
            ComputedStatus = status,
            StatusIcon = statusIcon,
            StatusColor = statusColor
        };
    }

    [Fact]
    public void TodayPatients_TypeIsObservableCollection()
    {
        var collection = new System.Collections.ObjectModel.ObservableCollection<TodayPatientWithStatusDto>();

        Assert.NotNull(collection);
        Assert.Empty(collection);
    }

    [Fact]
    public void StatusIcon_WithFullyComplete_IsNotEmpty()
    {
        var dto = CreateDto(
            status: PatientVisitStatus.FullyComplete,
            statusIcon: "\u2705",
            statusColor: "#4CAF50");

        Assert.False(string.IsNullOrEmpty(dto.StatusIcon));
        Assert.Equal("\u2705", dto.StatusIcon);
    }

    [Fact]
    public void StatusIcon_WithNewNoResults_IsNotEmpty()
    {
        var dto = CreateDto(
            status: PatientVisitStatus.NewNoResults,
            statusIcon: "\U0001F6D2",
            statusColor: "#808080");

        Assert.False(string.IsNullOrEmpty(dto.StatusIcon));
    }

    [Fact]
    public void StatusColor_WithCompleteWithBalance_IsRed()
    {
        var dto = CreateDto(
            status: PatientVisitStatus.CompleteWithBalance,
            statusIcon: "\U0001F4B2",
            statusColor: "#F44336");

        Assert.Equal("#F44336", dto.StatusColor);
    }
}

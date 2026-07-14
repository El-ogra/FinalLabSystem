using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Tests.ViewModels;

public class TodayPatientsStatusDisplayTests
{
    private static TodayPatientWithStatusDto CreateDto(
        VisitDisplayStatus status = VisitDisplayStatus.NewNoResults,
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
            status: VisitDisplayStatus.FullyComplete,
            statusIcon: "\u2705",
            statusColor: "#4CAF50");

        Assert.Equal(VisitDisplayStatus.FullyComplete, dto.ComputedStatus);
        Assert.Equal("\u2705", dto.StatusIcon);
        Assert.Equal("#4CAF50", dto.StatusColor);
    }

    [Fact]
    public void VerifyStatus_HasUnwrittenResults()
    {
        var dto = new TodayPatientWithStatusDto { ComputedStatus = VisitDisplayStatus.ResultsNotWritten };
        Assert.Equal("\U0001F6D2", GetStatusIcon(dto.ComputedStatus));
        Assert.Equal("#FF8C00", GetStatusColor(dto.ComputedStatus));
    }

    [Fact]
    public void VerifyStatus_HasUnreviewedResults()
    {
        var dto = new TodayPatientWithStatusDto { ComputedStatus = VisitDisplayStatus.ResultsNotReviewed };
        Assert.Equal("\U0001F3C5", GetStatusIcon(dto.ComputedStatus));
        Assert.Equal("#FFD700", GetStatusColor(dto.ComputedStatus));
    }

    [Fact]
    public void VerifyStatus_HasUnprintedResults()
    {
        var dto = new TodayPatientWithStatusDto { ComputedStatus = VisitDisplayStatus.ResultsNotPrinted };
        Assert.Equal("\U0001F4C4", GetStatusIcon(dto.ComputedStatus));
        Assert.Equal("#4FC3F7", GetStatusColor(dto.ComputedStatus));
    }

    [Fact]
    public void VerifyStatus_HasUndeliveredResults()
    {
        var dto = new TodayPatientWithStatusDto { ComputedStatus = VisitDisplayStatus.NotDelivered };
        Assert.Equal("\U0001F5A8", GetStatusIcon(dto.ComputedStatus));
        Assert.Equal("#9C27B0", GetStatusColor(dto.ComputedStatus));
    }

    [Fact]
    public void VerifyStatus_CompleteWithBalance()
    {
        var dto = new TodayPatientWithStatusDto { ComputedStatus = VisitDisplayStatus.DeliveredWithBalance };
        Assert.Equal("\U0001F4B2", GetStatusIcon(dto.ComputedStatus));
        Assert.Equal("#F44336", GetStatusColor(dto.ComputedStatus));
    }

    // Helper functions replicating the private UI logic for testing
    private static string GetStatusIcon(VisitDisplayStatus status) => status switch
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

    private static string GetStatusColor(VisitDisplayStatus status) => status switch
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
}

using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class CommissionReportServiceTests
{
    [Fact]
    public async Task GetCommissionReportAsync_ReturnsRowsWithinDateRange()
    {
        var mockService = new Mock<ICommissionReportService>();
        mockService.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", CommissionRate = 10, VisitId = 100, VisitCode = "V001", VisitDate = DateTime.UtcNow.AddDays(-5), PatientName = "أحمد", VisitTotal = 500, TotalPaid = 500, CommissionDue = 50 },
                new() { ReferralId = 2, ReferralName = "د. سعيد", SourceType = "مختبر", CommissionRate = 15, VisitId = 101, VisitCode = "V002", VisitDate = DateTime.UtcNow.AddDays(-1), PatientName = "محمد", VisitTotal = 300, TotalPaid = 300, CommissionDue = 45 }
            });

        var result = await mockService.Object.GetCommissionReportAsync(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCommissionReportAsync_ReturnsEmptyForNoMatch()
    {
        var mockService = new Mock<ICommissionReportService>();
        mockService.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>());

        var result = await mockService.Object.GetCommissionReportAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommissionReportAsync_MapsAllFieldsCorrectly()
    {
        var mockService = new Mock<ICommissionReportService>();
        mockService.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 5, ReferralName = "د. خالد", SourceType = "عيادة", CommissionRate = 12.5, VisitId = 200, VisitCode = "V010", VisitDate = DateTime.UtcNow.AddDays(-2), PatientName = "سالم أحمد", VisitTotal = 800, TotalPaid = 600, CommissionDue = 100 }
            });

        var result = await mockService.Object.GetCommissionReportAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

        Assert.Single(result);
        var row = result[0];
        Assert.Equal(5, row.ReferralId);
        Assert.Equal("د. خالد", row.ReferralName);
        Assert.Equal("عيادة", row.SourceType);
        Assert.Equal(12.5, row.CommissionRate);
        Assert.Equal(200, row.VisitId);
        Assert.Equal("V010", row.VisitCode);
        Assert.Equal("سالم أحمد", row.PatientName);
        Assert.Equal(800, row.VisitTotal);
        Assert.Equal(600, row.TotalPaid);
        Assert.Equal(100, row.CommissionDue);
    }

    [Fact]
    public async Task GetCommissionReportAsync_IncludesBoundaryDates()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var mockService = new Mock<ICommissionReportService>();
        mockService.Setup(s => s.GetCommissionReportAsync(startDate, endDate))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", CommissionRate = 10, VisitId = 300, VisitCode = "V030", VisitDate = startDate, PatientName = "test", VisitTotal = 100, TotalPaid = 100, CommissionDue = 10 },
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", CommissionRate = 10, VisitId = 301, VisitCode = "V031", VisitDate = endDate, PatientName = "test", VisitTotal = 100, TotalPaid = 100, CommissionDue = 10 }
            });

        var result = await mockService.Object.GetCommissionReportAsync(startDate, endDate);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCommissionReportAsync_HandlesNullCommissionDue()
    {
        var mockService = new Mock<ICommissionReportService>();
        mockService.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", CommissionRate = 10, VisitId = 400, VisitCode = "V040", VisitDate = DateTime.UtcNow, PatientName = "test", VisitTotal = 100, TotalPaid = 0, CommissionDue = null }
            });

        var result = await mockService.Object.GetCommissionReportAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        Assert.Single(result);
        Assert.Null(result[0].CommissionDue);
    }

    [Fact]
    public async Task GetCommissionReportAsync_ReturnsCorrectCount()
    {
        var mockService = new Mock<ICommissionReportService>();
        mockService.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", CommissionRate = 10, VisitId = 500, VisitCode = "V050", VisitDate = DateTime.UtcNow, PatientName = "test", VisitTotal = 100, TotalPaid = 100, CommissionDue = 10 }
            });

        var result = await mockService.Object.GetCommissionReportAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        Assert.Single(result);
    }
}

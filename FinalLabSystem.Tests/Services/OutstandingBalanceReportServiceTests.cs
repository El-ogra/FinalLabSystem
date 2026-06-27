using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class OutstandingBalanceReportServiceTests
{
    [Fact]
    public async Task GetOutstandingBalancesAsync_WithDateRange_ReturnsFiltered()
    {
        var mockService = new Mock<IOutstandingBalanceReportService>();
        mockService.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow.AddDays(-5),
                         PatientCode = "P001", PatientName = "أحمد محمد", Phone = "0555123456",
                         CompanyName = "شركة أرامكو", TotalAfterDiscount = 500, TotalPaid = 300,
                         BalanceDue = 200, PaymentStatus = "جزئي", DaysOverdue = 5 },
                new() { VisitId = 2, VisitCode = "V002", VisitDate = DateTime.UtcNow.AddDays(-1),
                         PatientCode = "P002", PatientName = "محمد سعيد", Phone = "0555789012",
                         CompanyName = "شركة سابك", TotalAfterDiscount = 800, TotalPaid = 800,
                         BalanceDue = 0, PaymentStatus = "مدفوع", DaysOverdue = null }
            });

        var result = await mockService.Object.GetOutstandingBalancesAsync(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetOutstandingBalancesAsync_NoData_ReturnsEmpty()
    {
        var mockService = new Mock<IOutstandingBalanceReportService>();
        mockService.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>());

        var result = await mockService.Object.GetOutstandingBalancesAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOutstandingBalancesAsync_MultiplePatients_ReturnsAll()
    {
        var mockService = new Mock<IOutstandingBalanceReportService>();
        mockService.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" },
                new() { VisitId = 2, VisitCode = "V002", VisitDate = DateTime.UtcNow, PatientCode = "P002", PatientName = "محمد", TotalAfterDiscount = 800, TotalPaid = 800, BalanceDue = 0, PaymentStatus = "مدفوع" },
                new() { VisitId = 3, VisitCode = "V003", VisitDate = DateTime.UtcNow, PatientCode = "P003", PatientName = "خالد", TotalAfterDiscount = 300, TotalPaid = 0, BalanceDue = 300, PaymentStatus = "غير مدفوع" }
            });

        var result = await mockService.Object.GetOutstandingBalancesAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetOutstandingBalancesAsync_DateRange_ExcludesOutOfRange()
    {
        var mockService = new Mock<IOutstandingBalanceReportService>();
        mockService.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow.AddDays(-5), PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" }
            });

        var result = await mockService.Object.GetOutstandingBalancesAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-3));

        Assert.Single(result);
    }

    [Fact]
    public async Task GetOutstandingBalancesAsync_ReturnsCorrectTotals()
    {
        var mockService = new Mock<IOutstandingBalanceReportService>();
        mockService.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 5, VisitCode = "V010", VisitDate = DateTime.UtcNow.AddDays(-2),
                         PatientCode = "P005", PatientName = "سالم أحمد", Phone = "0555987654",
                         CompanyName = "شركة الراجحي", TotalAfterDiscount = 1200, TotalPaid = 600,
                         BalanceDue = 600, PaymentStatus = "متأخر", DaysOverdue = 30 }
            });

        var result = await mockService.Object.GetOutstandingBalancesAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

        Assert.Single(result);
        var row = result[0];
        Assert.Equal(5, row.VisitId);
        Assert.Equal("V010", row.VisitCode);
        Assert.Equal("P005", row.PatientCode);
        Assert.Equal("سالم أحمد", row.PatientName);
        Assert.Equal("0555987654", row.Phone);
        Assert.Equal("شركة الراجحي", row.CompanyName);
        Assert.Equal(1200, row.TotalAfterDiscount);
        Assert.Equal(600, row.TotalPaid);
        Assert.Equal(600, row.BalanceDue);
        Assert.Equal("متأخر", row.PaymentStatus);
        Assert.Equal(30, row.DaysOverdue);
    }
}

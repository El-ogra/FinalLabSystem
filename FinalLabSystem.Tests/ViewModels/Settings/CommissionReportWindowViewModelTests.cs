using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class CommissionReportWindowViewModelTests
{
    private static (CommissionReportWindowViewModel VM, Mock<ICommissionReportService> ServiceMock, Mock<IPrintService> PrintMock, Mock<IDialogService> DialogMock) CreateVM()
    {
        var serviceMock = new Mock<ICommissionReportService>();
        var printMock = new Mock<IPrintService>();
        var dialogMock = new Mock<IDialogService>();
        var vm = new CommissionReportWindowViewModel(serviceMock.Object, printMock.Object, dialogMock.Object);
        return (vm, serviceMock, printMock, dialogMock);
    }

    [Fact]
    public async Task LoadAsync_PopulatesRows()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientName = "أحمد", VisitTotal = 500, TotalPaid = 500, CommissionDue = 50 },
                new() { ReferralId = 2, ReferralName = "د. سعيد", SourceType = "مختبر", VisitCode = "V002", VisitDate = DateTime.UtcNow, PatientName = "محمد", VisitTotal = 300, TotalPaid = 300, CommissionDue = 45 }
            });

        await vm.LoadAsync();

        Assert.Equal(2, vm.Rows.Count);
    }

    [Fact]
    public async Task LoadAsync_SetsStatusMessage()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientName = "أحمد", VisitTotal = 500, TotalPaid = 500, CommissionDue = 50 }
            });

        await vm.LoadAsync();

        Assert.Contains("1", vm.StatusMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyResult_SetsZeroStatus()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>());

        await vm.LoadAsync();

        Assert.Contains("0", vm.StatusMessage);
    }

    [Fact]
    public async Task PrintAsync_CallsPrintService()
    {
        var (vm, serviceMock, printMock, _) = CreateVM();
        serviceMock.Setup(s => s.GetCommissionReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CommissionReportRow>
            {
                new() { ReferralId = 1, ReferralName = "د. علي", SourceType = "طبيب", VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientName = "أحمد", VisitTotal = 500, TotalPaid = 500, CommissionDue = 50 }
            });

        await vm.LoadAsync();
        await vm.PrintAsync();

        printMock.Verify(p => p.PrintAsync("CommissionReport", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task PrintAsync_EmptyRows_ShowsWarning()
    {
        var (vm, _, printMock, dialogMock) = CreateVM();

        await vm.PrintAsync();

        dialogMock.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        printMock.Verify(p => p.PrintAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}

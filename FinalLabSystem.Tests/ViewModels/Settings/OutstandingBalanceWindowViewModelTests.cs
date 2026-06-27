using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class OutstandingBalanceWindowViewModelTests
{
    private static (OutstandingBalanceWindowViewModel VM, Mock<IOutstandingBalanceReportService> ServiceMock, Mock<IPrintService> PrintMock, Mock<IDialogService> DialogMock) CreateVM()
    {
        var serviceMock = new Mock<IOutstandingBalanceReportService>();
        var printMock = new Mock<IPrintService>();
        var dialogMock = new Mock<IDialogService>();
        var vm = new OutstandingBalanceWindowViewModel(serviceMock.Object, printMock.Object, dialogMock.Object);
        return (vm, serviceMock, printMock, dialogMock);
    }

    [Fact]
    public async Task LoadAsync_PopulatesReports()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" },
                new() { VisitId = 2, VisitCode = "V002", VisitDate = DateTime.UtcNow, PatientCode = "P002", PatientName = "محمد", TotalAfterDiscount = 800, TotalPaid = 800, BalanceDue = 0, PaymentStatus = "مدفوع" }
            });

        await vm.LoadAsync();

        Assert.Equal(2, vm.Reports.Count);
    }

    [Fact]
    public async Task LoadAsync_SetsTotalOutstanding()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" },
                new() { VisitId = 2, VisitCode = "V002", VisitDate = DateTime.UtcNow, PatientCode = "P002", PatientName = "محمد", TotalAfterDiscount = 800, TotalPaid = 800, BalanceDue = 0, PaymentStatus = "مدفوع" }
            });

        await vm.LoadAsync();

        Assert.Equal(200, vm.TotalOutstanding);
    }

    [Fact]
    public async Task RefreshCommand_ReloadsData()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" }
            });

        await vm.LoadAsync();
        Assert.Single(vm.Reports);

        serviceMock.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" },
                new() { VisitId = 2, VisitCode = "V002", VisitDate = DateTime.UtcNow, PatientCode = "P002", PatientName = "محمد", TotalAfterDiscount = 800, TotalPaid = 800, BalanceDue = 0, PaymentStatus = "مدفوع" },
                new() { VisitId = 3, VisitCode = "V003", VisitDate = DateTime.UtcNow, PatientCode = "P003", PatientName = "خالد", TotalAfterDiscount = 300, TotalPaid = 0, BalanceDue = 300, PaymentStatus = "غير مدفوع" }
            });

        await vm.LoadAsync();
        Assert.Equal(3, vm.Reports.Count);
    }

    [Fact]
    public async Task PrintCommand_CallsPrintService()
    {
        var (vm, serviceMock, printMock, _) = CreateVM();
        serviceMock.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" }
            });

        await vm.LoadAsync();
        await vm.PrintAsync();

        printMock.Verify(p => p.PrintAsync("OutstandingBalance", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task StatusMessage_ShowsAfterLoad()
    {
        var (vm, serviceMock, _, _) = CreateVM();
        serviceMock.Setup(s => s.GetOutstandingBalancesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<OutstandingBalanceReportRow>
            {
                new() { VisitId = 1, VisitCode = "V001", VisitDate = DateTime.UtcNow, PatientCode = "P001", PatientName = "أحمد", TotalAfterDiscount = 500, TotalPaid = 300, BalanceDue = 200, PaymentStatus = "جزئي" }
            });

        await vm.LoadAsync();

        Assert.Contains("1", vm.StatusMessage);
    }
}

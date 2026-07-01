using System.Windows.Input;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class TestResultsFKeyRemappingTests
{
    private static TestResultsViewModel CreateViewModel()
    {
        var mockVisitService = new Mock<IVisitService>();
        var mockRoutineResult = new Mock<IRoutineResultService>();
        var mockAudit = new Mock<IAuditService>();
        var mockReporting = new Mock<IReportingService>();
        var mockAuth = new Mock<IAuthService>();
        var mockDialog = new Mock<IDialogService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockNavigation = new Mock<INavigationService>();
        var mockPrint = new Mock<IPrintService>();
        var mockReceipt = new Mock<IReceiptService>();
        var mockAuditTrail = new Mock<IAuditTrailDialogService>();
        var mockResultEntry = new Mock<IResultEntryDialogService>();
        var mockPrintQueue = new Mock<IPrintQueueService>();

        return new TestResultsViewModel(
            mockVisitService.Object,
            mockRoutineResult.Object,
            mockAudit.Object,
            mockReporting.Object,
            mockAuth.Object,
            mockDialog.Object,
            mockSession.Object,
            mockNavigation.Object,
            mockPrint.Object,
            mockReceipt.Object,
            mockAuditTrail.Object,
            mockResultEntry.Object,
            mockPrintQueue.Object);
    }

    // ========== Ctrl+R / Ctrl+F / Ctrl+P = Legacy toggles ==========

    [Fact]
    public void ToggleReviewStatusCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.ToggleReviewStatusCommand);
    }

    [Fact]
    public void ToggleFinishStatusCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.ToggleFinishStatusCommand);
    }

    [Fact]
    public void TogglePrintStatusCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.TogglePrintStatusCommand);
    }

    // ========== F8 / F12 / F4 / F7 = New semantics ==========

    [Fact]
    public void EditSelectedPatientCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.EditSelectedPatientCommand);
    }

    [Fact]
    public void PrintReceiptCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.PrintReceiptCommand);
    }

    [Fact]
    public void NavigateToResultEntryCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToResultEntryCommand);
    }

    [Fact]
    public void NavigateToExternalSamplesCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToExternalSamplesCommand);
    }
}

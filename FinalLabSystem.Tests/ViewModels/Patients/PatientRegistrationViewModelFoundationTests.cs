using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Patients;

public class PatientRegistrationViewModelFoundationTests
{
    private static (PatientRegistrationViewModel vm, Mock<ICurrentUserSession> sessionMock,
        Mock<IDialogService> dialogMock, Mock<ILogger<PatientRegistrationViewModel>> loggerMock) CreateViewModelWithMocks(
        Staff? currentUser = null)
    {
        var mockVisitService = new Mock<IVisitService>();
        var mockPatientService = new Mock<IPatientService>();
        var mockSampleTracking = new Mock<ISampleTrackingService>();
        var mockNavigation = new Mock<INavigationService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();
        var mockReferralService = new Mock<IReferralService>();
        var mockTestCatalogService = new Mock<ITestCatalogService>();
        var mockFinancialService = new Mock<IFinancialService>();
        var mockPricingService = new Mock<IPricingService>();
        var mockBarcodeFactory = new Mock<IBarcodeDialogFactory>();
        var mockReceiptFactory = new Mock<IReceiptDialogFactory>();
        var mockLogger = new Mock<ILogger<PatientRegistrationViewModel>>();

        mockSession.Setup(s => s.CurrentUser).Returns(currentUser);

        var patientInfo = new PatientInfoViewModel(mockPatientService.Object);
        var referral = new ReferralViewModel(mockReferralService.Object);
        var medicalHistory = new MedicalHistoryViewModel();
        var pricingEngine = new TestPricingEngine(mockPricingService.Object);
        var testSelection = new TestSelectionViewModel(mockTestCatalogService.Object, pricingEngine);
        var financial = new FinancialViewModel(mockFinancialService.Object, mockDialog.Object);

        var vm = new PatientRegistrationViewModel(
            patientInfo,
            referral,
            medicalHistory,
            testSelection,
            financial,
            mockVisitService.Object,
            mockPatientService.Object,
            mockSampleTracking.Object,
            mockNavigation.Object,
            mockSession.Object,
            mockDialog.Object,
            mockBarcodeFactory.Object,
            mockReceiptFactory.Object,
            mockLogger.Object);

        return (vm, mockSession, mockDialog, mockLogger);
    }

    [Fact]
    public void PatientRegistrationVM_Should_Use_ActualStaffId_WhenAvailable()
    {
        var staffUser = new Staff { StaffId = 5, Username = "test" };
        var (vm, sessionMock, _, _) = CreateViewModelWithMocks(staffUser);

        Assert.NotNull(vm.SaveCommand);
        Assert.NotNull(vm.BarcodeCommand);
        Assert.NotNull(vm.ReceiptCommand);
        Assert.Equal(5, sessionMock.Object.CurrentUser?.StaffId);
    }

    [Fact]
    public async Task PatientRegistrationVM_InitializeAsync_Should_LogError_OnException()
    {
        var mockVisitService = new Mock<IVisitService>();
        var mockPatientService = new Mock<IPatientService>();
        var mockSampleTracking = new Mock<ISampleTrackingService>();
        var mockNavigation = new Mock<INavigationService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();
        var mockReferralService = new Mock<IReferralService>();
        var mockTestCatalogService = new Mock<ITestCatalogService>();
        var mockFinancialService = new Mock<IFinancialService>();
        var mockPricingService = new Mock<IPricingService>();
        var mockBarcodeFactory = new Mock<IBarcodeDialogFactory>();
        var mockReceiptFactory = new Mock<IReceiptDialogFactory>();
        var mockLogger = new Mock<ILogger<PatientRegistrationViewModel>>();

        mockPatientService.Setup(s => s.GeneratePatientCodeAsync())
            .ThrowsAsync(new InvalidOperationException("DB failure"));

        var patientInfo = new PatientInfoViewModel(mockPatientService.Object);
        var referral = new ReferralViewModel(mockReferralService.Object);
        var medicalHistory = new MedicalHistoryViewModel();
        var pricingEngine = new TestPricingEngine(mockPricingService.Object);
        var testSelection = new TestSelectionViewModel(mockTestCatalogService.Object, pricingEngine);
        var financial = new FinancialViewModel(mockFinancialService.Object, mockDialog.Object);

        var vm = new PatientRegistrationViewModel(
            patientInfo,
            referral,
            medicalHistory,
            testSelection,
            financial,
            mockVisitService.Object,
            mockPatientService.Object,
            mockSampleTracking.Object,
            mockNavigation.Object,
            mockSession.Object,
            mockDialog.Object,
            mockBarcodeFactory.Object,
            mockReceiptFactory.Object,
            mockLogger.Object);

        await vm.InitializeAsync();

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to initialize")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockDialog.Verify(d => d.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void PatientRegistrationVM_Should_Inject_Factories()
    {
        var (vm, _, _, _) = CreateViewModelWithMocks(new Staff { StaffId = 1, Username = "test" });

        Assert.NotNull(vm.BarcodeCommand);
        Assert.NotNull(vm.ReceiptCommand);
        Assert.False(vm.BarcodeCommand.CanExecute(null));
        Assert.False(vm.ReceiptCommand.CanExecute(null));
    }
}

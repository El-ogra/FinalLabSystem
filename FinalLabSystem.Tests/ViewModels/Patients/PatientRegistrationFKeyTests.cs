using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class PatientRegistrationFKeyTests
{
    private static PatientRegistrationViewModel CreateViewModel()
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

        var patientInfo = new PatientInfoViewModel(mockPatientService.Object);
        var referral = new ReferralViewModel(mockReferralService.Object);
        var medicalHistory = new MedicalHistoryViewModel();
        var pricingEngine = new TestPricingEngine(mockPricingService.Object);
        var testSelection = new TestSelectionViewModel(mockTestCatalogService.Object, pricingEngine);
        var financial = new FinancialViewModel(mockFinancialService.Object, mockDialog.Object);

        return new PatientRegistrationViewModel(
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
    }

    [Fact]
    public void F1_AddNewCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.AddNewCommand);
    }

    [Fact]
    public void F2_NavigateToPatientDataCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToPatientDataCommand);
    }

    [Fact]
    public void F3_NavigateToSearchCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToSearchCommand);
    }

    [Fact]
    public void F4_NavigateToResultEntryCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToResultEntryCommand);
    }

    [Fact]
    public void F5_LoadTodayPatientsCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.LoadTodayPatientsCommand);
    }

    [Fact]
    public void F6_NavigateToDeliveryCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToDeliveryCommand);
    }

    [Fact]
    public void F7_NavigateToExternalSamplesCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.NavigateToExternalSamplesCommand);
    }

    [Fact]
    public void F8_EditCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.EditCommand);
    }

    [Fact]
    public void F9_SaveCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.SaveCommand);
    }

    [Fact]
    public void F10_DeleteCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.DeleteCommand);
    }

    [Fact]
    public void F11_BarcodeCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.BarcodeCommand);
    }

    [Fact]
    public void F12_ReceiptCommand_IsNotNull()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.ReceiptCommand);
    }
}

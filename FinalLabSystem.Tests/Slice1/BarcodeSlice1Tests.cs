using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.ViewModels.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Slice1;

public class BarcodeGeneratorTests
{
    private static FinalLabDbContext CreateInMemoryDbContext(Action<FinalLabDbContext>? seed = null)
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new FinalLabDbContext(options);
        seed?.Invoke(context);
        return context;
    }

    // --- Unit Tests: BarcodeGenerator (5 tests) ---

    [Fact]
    public async Task T01_GenerateCaseCodeAsync_Returns13Chars_StartsWith1()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" };
        var visit = new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.Visits.Add(visit);
            ctx.SaveChanges();
        });

        var generator = new BarcodeGenerator(context);
        var code = await generator.GenerateCaseCodeAsync(1);

        Assert.Equal(13, code.Length);
        Assert.StartsWith("1", code);
        var datePart = DateTime.Today.ToString("yyMMdd");
        Assert.Contains(datePart, code);
    }

    [Fact]
    public async Task T02_GenerateFileCodeAsync_Returns13Chars_StartsWith3()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" };
        var visit = new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.Visits.Add(visit);
            ctx.SaveChanges();
        });

        var generator = new BarcodeGenerator(context);
        var code = await generator.GenerateFileCodeAsync(1);

        Assert.Equal(13, code.Length);
        Assert.StartsWith("3", code);
        var datePart = DateTime.Today.ToString("yyMMdd");
        Assert.Contains(datePart, code);
    }

    [Fact]
    public async Task T03_GetOrCreateLabIdAsync_NewPatient_CreatesAndPersists()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.SaveChanges();
        });

        var generator = new BarcodeGenerator(context);
        var labId = await generator.GetOrCreateLabIdAsync(1);

        Assert.Equal(13, labId.Length);
        Assert.StartsWith("5", labId);

        var updatedPatient = await context.Patients.FindAsync(1);
        Assert.NotNull(updatedPatient!.LabId);
        Assert.Equal(labId, updatedPatient.LabId);
    }

    [Fact]
    public async Task T04_GetOrCreateLabIdAsync_ExistingPatient_ReturnsSame()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M", LabId = "5260714100005" };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.SaveChanges();
        });

        var generator = new BarcodeGenerator(context);
        var labId = await generator.GetOrCreateLabIdAsync(1);

        Assert.Equal("5260714100005", labId);
    }

    [Fact]
    public void T05_CalculateCheckDigit_ProducesValidLuhn()
    {
        var code1 = "126071410000";
        var digit1 = BarcodeGenerator.CalculateLuhnCheckDigit(code1);
        Assert.Equal(4, digit1);

        var fullCode1 = code1 + digit1;
        int sum1 = 0;
        bool alt1 = true;
        for (int i = fullCode1.Length - 1; i >= 0; i--)
        {
            int d = fullCode1[i] - '0';
            if (alt1) d *= 2;
            if (d > 9) d -= 9;
            sum1 += d;
            alt1 = !alt1;
        }
        Assert.Equal(0, sum1 % 10);

        var code2 = "326071410000";
        var digit2 = BarcodeGenerator.CalculateLuhnCheckDigit(code2);
        Assert.Equal(2, digit2);

        var fullCode2 = code2 + digit2;
        int sum2 = 0;
        bool alt2 = true;
        for (int i = fullCode2.Length - 1; i >= 0; i--)
        {
            int d = fullCode2[i] - '0';
            if (alt2) d *= 2;
            if (d > 9) d -= 9;
            sum2 += d;
            alt2 = !alt2;
        }
        Assert.Equal(0, sum2 % 10);
    }

    // --- Integration Tests: SampleTrackingService (2 tests) ---

    [Fact]
    public async Task T06_GenerateBarcodesForVisit_FirstVisit_CreatesAll3Codes()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" };
        var visit = new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" };
        var staff = new Staff { StaffId = 1, Username = "admin", DisplayName = "Admin", PasswordHash = "x", IsActive = true };
        var testType = new TestType { TesttypeId = 1, TypeNameEn = "CBC", TypeCode = "CBC" };
        var visitTest = new VisitTest { VisitTestId = 1, VisitId = 1, TesttypeId = 1, CurrentStage = TestStage.Pending };
        var collectionType = new CollectionType { CollectionTypeId = 1, TypeNameEn = "Blood", SortOrder = 1, IsActive = true };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.Visits.Add(visit);
            ctx.Staff.Add(staff);
            ctx.TestTypes.Add(testType);
            ctx.VisitTests.Add(visitTest);
            ctx.CollectionTypes.Add(collectionType);
            ctx.SaveChanges();
        });

        var barcodeGenerator = new BarcodeGenerator(context);
        var logger = new Mock<ILogger<SampleTrackingService>>();
        var service = new SampleTrackingService(context, logger.Object, barcodeGenerator);

        var tubes = await service.GenerateBarcodesForVisitAsync(1, 1);

        Assert.NotEmpty(tubes);

        var barcodes = await context.PatientBarcodes.ToListAsync();
        Assert.Equal(3, barcodes.Count);

        Assert.Contains(barcodes, b => b.CodeType == BarcodeCodeType.Case);
        Assert.Contains(barcodes, b => b.CodeType == BarcodeCodeType.File);
        Assert.Contains(barcodes, b => b.CodeType == BarcodeCodeType.Lab);

        var labBarcode = barcodes.First(b => b.CodeType == BarcodeCodeType.Lab);
        Assert.Null(labBarcode.VisitId);

        var patientAfter = await context.Patients.FindAsync(1);
        Assert.NotNull(patientAfter!.LabId);
        Assert.StartsWith("5", patientAfter.LabId);
    }

    [Fact]
    public async Task T07_GenerateBarcodesForVisit_SecondVisit_ReusesLabId()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" };
        var visit1 = new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" };
        var visit2 = new Visit { VisitId = 2, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V002" };
        var staff = new Staff { StaffId = 1, Username = "admin", DisplayName = "Admin", PasswordHash = "x", IsActive = true };
        var testType = new TestType { TesttypeId = 1, TypeNameEn = "CBC", TypeCode = "CBC" };
        var vt1 = new VisitTest { VisitTestId = 1, VisitId = 1, TesttypeId = 1, CurrentStage = TestStage.Pending };
        var vt2 = new VisitTest { VisitTestId = 2, VisitId = 2, TesttypeId = 1, CurrentStage = TestStage.Pending };
        var ct = new CollectionType { CollectionTypeId = 1, TypeNameEn = "Blood", SortOrder = 1, IsActive = true };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.Visits.AddRange(visit1, visit2);
            ctx.Staff.Add(staff);
            ctx.TestTypes.Add(testType);
            ctx.VisitTests.AddRange(vt1, vt2);
            ctx.CollectionTypes.Add(ct);
            ctx.SaveChanges();
        });

        var barcodeGenerator = new BarcodeGenerator(context);
        var logger = new Mock<ILogger<SampleTrackingService>>();
        var service = new SampleTrackingService(context, logger.Object, barcodeGenerator);

        await service.GenerateBarcodesForVisitAsync(1, 1);
        var labId1 = (await context.Patients.FindAsync(1))!.LabId;

        await service.GenerateBarcodesForVisitAsync(2, 1);
        var labId2 = (await context.Patients.FindAsync(1))!.LabId;

        Assert.Equal(labId1, labId2);

        var allBarcodes = await context.PatientBarcodes.ToListAsync();
        var caseBarcodes = allBarcodes.Where(b => b.CodeType == BarcodeCodeType.Case).ToList();
        Assert.Equal(2, caseBarcodes.Count);
        Assert.NotEqual(caseBarcodes[0].BarcodeValue, caseBarcodes[1].BarcodeValue);

        var fileBarcodes = allBarcodes.Where(b => b.CodeType == BarcodeCodeType.File).ToList();
        Assert.Equal(2, fileBarcodes.Count);
        Assert.NotEqual(fileBarcodes[0].BarcodeValue, fileBarcodes[1].BarcodeValue);

        var labBarcodes = allBarcodes.Where(b => b.CodeType == BarcodeCodeType.Lab).ToList();
        Assert.All(labBarcodes, b => Assert.Equal(labId1, b.BarcodeValue));
    }

    // --- ViewModel Tests (2 tests) ---

    [Fact]
    public async Task T08_BarcodeDialog_LabIdSection_ShowsOnlyOneLabel()
    {
        var patient = new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" };
        var labBarcode = new PatientBarcode
        {
            PatientBarcodeId = 1,
            PatientId = 1,
            CodeType = BarcodeCodeType.Lab,
            BarcodeValue = "5123070410012",
            IssueDate = DateTime.Now,
            SortOrdinal = 0
        };

        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(patient);
            ctx.PatientBarcodes.Add(labBarcode);
            ctx.SaveChanges();
        });

        var mockSampleTracking = new Mock<ISampleTrackingService>();
        mockSampleTracking.Setup(s => s.GetTubesForVisitAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SampleTube>());

        var vm = new BarcodeDialogViewModel(
            mockSampleTracking.Object,
            Mock.Of<ILabelPrintService>(),
            Mock.Of<IInventoryService>(),
            Mock.Of<IDialogService>(),
            context);

        await vm.LoadBarcodesAsync(1, 1);

        Assert.Single(vm.LabIdLabels);
        Assert.Empty(vm.CaseLabels);
        Assert.Empty(vm.FileLabels);
    }

    [Fact]
    public void T09_PatientRegistration_PrintLabIdCommand_EnabledOnlyAfterSave()
    {
        var mockVisitService = new Mock<IVisitService>();
        var mockPatientService = new Mock<IPatientService>();
        var mockSampleTracking = new Mock<ISampleTrackingService>();
        var mockNavigation = new Mock<INavigationService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();
        var mockBarcodeFactory = new Mock<IBarcodeDialogFactory>();
        var mockReceiptFactory = new Mock<IReceiptDialogFactory>();
        var mockBarcodeGenerator = new Mock<IBarcodeGenerator>();
        var mockLabelPrintService = new Mock<ILabelPrintService>();
        var mockLogger = new Mock<ILogger<PatientRegistrationViewModel>>();
        var mockReferralService = new Mock<IReferralService>();
        var mockTestCatalogService = new Mock<ITestCatalogService>();
        var mockFinancialService = new Mock<IFinancialService>();
        var mockPricingService = new Mock<IPricingService>();

        var patientInfo = new PatientInfoViewModel(mockPatientService.Object);
        var referral = new ReferralViewModel(mockReferralService.Object);
        var medicalHistory = new MedicalHistoryViewModel();
        var pricingEngine = new TestPricingEngine(mockPricingService.Object);
        var testSelection = new TestSelectionViewModel(mockTestCatalogService.Object, pricingEngine);
        var financial = new FinancialViewModel(mockFinancialService.Object, mockDialog.Object);

        var vm = new PatientRegistrationViewModel(
            patientInfo, referral, medicalHistory, testSelection, financial,
            mockVisitService.Object, mockPatientService.Object, mockSampleTracking.Object,
            mockNavigation.Object, mockSession.Object, mockDialog.Object,
            mockBarcodeFactory.Object, mockReceiptFactory.Object,
            mockBarcodeGenerator.Object, mockLabelPrintService.Object, mockLogger.Object);

        Assert.False(vm.PrintLabIdCommand.CanExecute(null));
    }
}

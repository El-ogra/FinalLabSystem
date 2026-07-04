using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Services.Printing;
using FinalLabSystem.ViewModels.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Slice2;

public class CultureSlice2Tests
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

    private static (CultureEntryViewModel vm, Mock<ICultureResultService> mockCulture,
        Mock<IDialogService> mockDialog)
        CreateViewModel(
            int visitTestId = 1,
            int patientId = 1,
            bool isPregnant = false,
            int patientAgeDays = 3650,
            List<AntibioticCatalog>? antibiotics = null)
    {
        var mockCulture = new Mock<ICultureResultService>();
        var mockDialog = new Mock<IDialogService>();
        var context = CreateInMemoryDbContext();

        antibiotics ??= new List<AntibioticCatalog>
        {
            new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
            new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true }
        };

        mockCulture.Setup(s => s.GetSafeAntibioticsAsync(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(antibiotics);
        mockCulture.Setup(s => s.GetByVisitTestIdAsync(It.IsAny<int>()))
            .ReturnsAsync((MicrobiologyCulture?)null);
        mockCulture.Setup(s => s.SaveFullCultureAsync(It.IsAny<MicrobiologyCulture>(), It.IsAny<List<MicrobiologyOrganism>>()))
            .Returns(Task.CompletedTask);

        var vm = new CultureEntryViewModel(
            mockCulture.Object,
            mockDialog.Object,
            context,
            visitTestId,
            patientId,
            isPregnant,
            patientAgeDays);

        return (vm, mockCulture, mockDialog);
    }

    // --- Unit Tests: CultureResultService (3 tests) ---

    [Fact]
    public async Task T01_GetSafeAntibioticsAsync_Pregnant_FiltersOutUnsafe()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Tetracycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true },
                new AntibioticCatalog { AntibioticId = 3, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var result = await service.GetSafeAntibioticsAsync(isPregnant: true, isChild: false);

        Assert.Single(result);
        Assert.Equal("Amoxicillin", result[0].AntibioticName);
    }

    [Fact]
    public async Task T02_GetSafeAntibioticsAsync_Child_FiltersOutUnsafe()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Tetracycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true },
                new AntibioticCatalog { AntibioticId = 3, AntibioticName = "Azithromycin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var result = await service.GetSafeAntibioticsAsync(isPregnant: false, isChild: true);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.IsSafeChildren));
    }

    [Fact]
    public async Task T03_GetSafeAntibioticsAsync_PregnantAndChild_AppliesBothFilters()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Tetracycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true },
                new AntibioticCatalog { AntibioticId = 3, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 4, AntibioticName = "Erythromycin", IsSafePregnancy = true, IsSafeChildren = false, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var result = await service.GetSafeAntibioticsAsync(isPregnant: true, isChild: true);

        Assert.Single(result);
        Assert.Equal("Amoxicillin", result[0].AntibioticName);
    }

    // --- ViewModel Tests: CultureEntryViewModel (3 tests) ---

    [Fact]
    public async Task T04_AddOrganism_MaxThree_DisablesAddCommand()
    {
        var (vm, _, _) = CreateViewModel();

        await vm.LoadAsync();

        Assert.True(vm.AddOrganismCommand.CanExecute(null));

        vm.AddOrganismCommand.Execute(null);
        await Task.Delay(100);
        Assert.True(vm.AddOrganismCommand.CanExecute(null));

        vm.AddOrganismCommand.Execute(null);
        await Task.Delay(100);
        Assert.True(vm.AddOrganismCommand.CanExecute(null));

        vm.AddOrganismCommand.Execute(null);
        await Task.Delay(100);
        Assert.False(vm.AddOrganismCommand.CanExecute(null));
    }

    [Fact]
    public async Task T05_SaveWithoutOrganisms_PendingCulture_ShowsWarningAndDoesNotSave()
    {
        var (vm, mockCulture, mockDialog) = CreateViewModel();
        await vm.LoadAsync();

        vm.CultureResult = "PENDING";
        Assert.False(vm.CanSaveWithoutOrganisms);

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        mockDialog.Verify(d => d.ShowWarning(
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
        mockCulture.Verify(s => s.SaveFullCultureAsync(
            It.IsAny<MicrobiologyCulture>(),
            It.IsAny<List<MicrobiologyOrganism>>()), Times.Never);
    }

    [Fact]
    public async Task T05b_SaveWithNoGrowth_EmptyOrganisms_AllowsSave()
    {
        var (vm, mockCulture, _) = CreateViewModel();
        await vm.LoadAsync();

        vm.CultureResult = "No Growth";
        Assert.True(vm.CanSaveWithoutOrganisms);

        var closeRequested = false;
        vm.RequestClose = () => closeRequested = true;

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        mockCulture.Verify(s => s.SaveFullCultureAsync(
            It.IsAny<MicrobiologyCulture>(),
            It.IsAny<List<MicrobiologyOrganism>>()), Times.Once);
        mockCulture.Verify(s => s.SaveFullCultureAsync(
            It.IsAny<MicrobiologyCulture>(),
            It.Is<List<MicrobiologyOrganism>>(l => l.Count == 0)), Times.Once);
        Assert.True(closeRequested);
    }

    [Fact]
    public async Task T05c_SaveWithNoGrowth_ExtraSpacesLowerCase_AllowsSave()
    {
        var (vm, mockCulture, _) = CreateViewModel();
        await vm.LoadAsync();

        vm.CultureResult = "  no growth  ";
        Assert.True(vm.CanSaveWithoutOrganisms);

        var closeRequested = false;
        vm.RequestClose = () => closeRequested = true;

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        mockCulture.Verify(s => s.SaveFullCultureAsync(
            It.IsAny<MicrobiologyCulture>(),
            It.IsAny<List<MicrobiologyOrganism>>()), Times.Once);
        mockCulture.Verify(s => s.SaveFullCultureAsync(
            It.IsAny<MicrobiologyCulture>(),
            It.Is<List<MicrobiologyOrganism>>(l => l.Count == 0)), Times.Once);
        Assert.True(closeRequested);
    }

    [Fact]
    public async Task T06_LoadAsync_PregnantPatient_CallsGetSafeAntibioticsWithTrue()
    {
        var antibiotics = new List<AntibioticCatalog>
        {
            new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
            new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Azithromycin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true }
        };
        var (vm, mockCulture, _) = CreateViewModel(isPregnant: true, antibiotics: antibiotics);

        await vm.LoadAsync();

        mockCulture.Verify(s => s.GetSafeAntibioticsAsync(true, It.IsAny<bool>()), Times.Once);
        Assert.Equal(2, vm.AvailableAntibiotics.Count);
    }

    // --- Integration Test (1 test) ---

    [Fact]
    public async Task T07_FullCultureWorkflow_SaveAndReload_Roundtrip()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" });
            ctx.Visits.Add(new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" });
            ctx.TestTypes.Add(new TestType { TesttypeId = 1, TypeNameEn = "Urine Culture", TypeCode = "UC" });
            ctx.VisitTests.Add(new VisitTest { VisitTestId = 1, VisitId = 1, TesttypeId = 1, CurrentStage = TestStage.Pending });
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var culture = new MicrobiologyCulture
        {
            VisitTestId = 1,
            CultureResult = "POSITIVE",
            SpecimenSource = "Urine",
            CultureCondition = "Aerobic, 37°C, 24h",
            ColonyCount = "10^5 CFU/mL",
            IncubationHours = 24,
            ReceivedAt = DateTime.Now
        };

        var organisms = new List<MicrobiologyOrganism>
        {
            new MicrobiologyOrganism
            {
                OrganismName = "E. coli",
                GramStain = "Gram-negative rods",
                ColonyCount = "Heavy growth",
                Morphology = "Round, smooth",
                SortOrder = 1,
                OrganismAntibiotics = new List<OrganismAntibiotic>
                {
                    new OrganismAntibiotic { AntibioticName = "Amoxicillin", Sensitivity = AntibioticSensitivity.Highly },
                    new OrganismAntibiotic { AntibioticName = "Ciprofloxacin", Sensitivity = AntibioticSensitivity.Resistant }
                }
            }
        };

        await service.SaveFullCultureAsync(culture, organisms);
        Assert.True(culture.CultureId > 0, "Culture should have been assigned an ID");

        var reloaded = await service.GetByVisitTestIdAsync(1);
        Assert.NotNull(reloaded);
        Assert.Equal("POSITIVE", reloaded!.CultureResult);
        Assert.Equal("Urine", reloaded.SpecimenSource);
        Assert.Equal("Aerobic, 37°C, 24h", reloaded.CultureCondition);
        Assert.Equal("10^5 CFU/mL", reloaded.ColonyCount);
        Assert.Single(reloaded.MicrobiologyOrganisms);

        var org = reloaded.MicrobiologyOrganisms.First();
        Assert.Equal("E. coli", org.OrganismName);
        Assert.Equal(2, org.OrganismAntibiotics.Count);

        var amox = org.OrganismAntibiotics.First(a => a.AntibioticName == "Amoxicillin");
        Assert.Equal(AntibioticSensitivity.Highly, amox.Sensitivity);
    }

    // --- Data Migration Test (1 test) ---

    [Fact]
    public void T08_DataMigration_SensitivityCharToEnum_MapsCorrectly()
    {
        Assert.Equal((byte)0, (byte)AntibioticSensitivity.Highly);
        Assert.Equal((byte)1, (byte)AntibioticSensitivity.Moderate);
        Assert.Equal((byte)2, (byte)AntibioticSensitivity.Low);
        Assert.Equal((byte)3, (byte)AntibioticSensitivity.Resistant);
    }

    // --- Report Tests (2 tests) ---

    [Fact]
    public void T09_CultureReport_NullFinalComment_OmitsFooter()
    {
        var culture = new MicrobiologyCulture
        {
            CultureId = 1,
            VisitTestId = 1,
            CultureResult = "No Growth",
            SpecimenSource = "Urine",
            CultureCondition = "Aerobic",
            ColonyCount = "Nil",
            IncubationHours = 24,
            FinalComment = null
        };

        var patient = new Patient
        {
            PatientId = 1,
            FullNameAr = "أحمد محمد",
            Sex = "M",
            ApproxAge = 5,
            ApproxAgeUnit = "سنوات"
        };

        var template = new CultureReportTemplate(culture, patient, new List<MicrobiologyOrganism>());
        var doc = template.BuildDocument();

        var allText = string.Join(" ",
            doc.Blocks
                .OfType<System.Windows.Documents.Paragraph>()
                .SelectMany(p => p.Inlines.OfType<System.Windows.Documents.Run>())
                .Select(r => r.Text));

        Assert.DoesNotContain("ملاحظات", allText);
    }

    [Fact]
    public void T10_CultureReport_FourSensitivityGroups_AllEnumValues()
    {
        var culture = new MicrobiologyCulture
        {
            CultureId = 1,
            VisitTestId = 1,
            CultureResult = "POSITIVE",
            SpecimenSource = "Urine"
        };

        var patient = new Patient
        {
            PatientId = 1,
            FullNameAr = "سارة",
            Sex = "F",
            ApproxAge = 30,
            ApproxAgeUnit = "سنة"
        };

        var organism = new MicrobiologyOrganism
        {
            OrganismId = 1,
            CultureId = 1,
            OrganismName = "Klebsiella",
            SortOrder = 1,
            OrganismAntibiotics = new List<OrganismAntibiotic>
            {
                new OrganismAntibiotic { AntibioticResultId = 1, OrganismId = 1, AntibioticName = "Amoxicillin", Sensitivity = AntibioticSensitivity.Highly },
                new OrganismAntibiotic { AntibioticResultId = 2, OrganismId = 1, AntibioticName = "Gentamicin", Sensitivity = AntibioticSensitivity.Moderate },
                new OrganismAntibiotic { AntibioticResultId = 3, OrganismId = 1, AntibioticName = "Tetracycline", Sensitivity = AntibioticSensitivity.Low },
                new OrganismAntibiotic { AntibioticResultId = 4, OrganismId = 1, AntibioticName = "Ciprofloxacin", Sensitivity = AntibioticSensitivity.Resistant }
            }
        };

        var template = new CultureReportTemplate(culture, patient, new List<MicrobiologyOrganism> { organism });
        var doc = template.BuildDocument();

        var allText = string.Join(" ",
            doc.Blocks
                .OfType<System.Windows.Documents.Paragraph>()
                .SelectMany(p => p.Inlines.OfType<System.Windows.Documents.Run>())
                .Select(r => r.Text));

        Assert.Contains("Highly Sensitive", allText);
        Assert.Contains("Moderately Sensitive", allText);
        Assert.Contains("Low Sensitivity", allText);
        Assert.Contains("Resistant", allText);
        Assert.Contains("Amoxicillin", allText);
        Assert.Contains("Gentamicin", allText);
        Assert.Contains("Tetracycline", allText);
        Assert.Contains("Ciprofloxacin", allText);
    }

    // --- T11: FK correctness test for the SaveFullCultureAsync fix ---

    [Fact]
    public async Task T11_SaveFullCultureAsync_NewHierarchy_AllForeignKeysPersistCorrectly()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" });
            ctx.Visits.Add(new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" });
            ctx.TestTypes.Add(new TestType { TesttypeId = 1, TypeNameEn = "Urine Culture", TypeCode = "UC" });
            ctx.VisitTests.Add(new VisitTest { VisitTestId = 1, VisitId = 1, TesttypeId = 1, CurrentStage = TestStage.Pending });
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var culture = new MicrobiologyCulture
        {
            CultureId = 0,
            VisitTestId = 1,
            CultureResult = "POSITIVE",
            SpecimenSource = "Urine"
        };

        var organisms = new List<MicrobiologyOrganism>
        {
            new MicrobiologyOrganism
            {
                OrganismId = 0,
                OrganismName = "E. coli",
                GramStain = "Gram-negative",
                SortOrder = 1,
                OrganismAntibiotics = new List<OrganismAntibiotic>
                {
                    new OrganismAntibiotic
                    {
                        AntibioticResultId = 0,
                        AntibioticName = "Amoxicillin",
                        Sensitivity = AntibioticSensitivity.Highly
                    },
                    new OrganismAntibiotic
                    {
                        AntibioticResultId = 0,
                        AntibioticName = "Ciprofloxacin",
                        Sensitivity = AntibioticSensitivity.Resistant
                    }
                }
            }
        };

        await service.SaveFullCultureAsync(culture, organisms);

        Assert.True(culture.CultureId > 0, $"CultureId should be assigned by DB, but was {culture.CultureId}");

        var reloaded = await service.GetByVisitTestIdAsync(1);
        Assert.NotNull(reloaded);
        Assert.Equal(culture.CultureId, reloaded!.CultureId);

        var org = reloaded.MicrobiologyOrganisms.First();
        Assert.True(org.OrganismId > 0, $"OrganismId should be assigned by DB, but was {org.OrganismId}");
        Assert.Equal(culture.CultureId, org.CultureId);

        foreach (var abx in org.OrganismAntibiotics)
        {
            Assert.True(abx.AntibioticResultId > 0, $"AntibioticResultId should be assigned by DB, but was {abx.AntibioticResultId}");
            Assert.Equal(org.OrganismId, abx.OrganismId);
        }
    }
}

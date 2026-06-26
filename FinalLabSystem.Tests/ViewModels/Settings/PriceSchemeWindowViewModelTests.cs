using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class PriceSchemeWindowViewModelTests
{
    private static (PriceSchemeWindowViewModel VM, Mock<IPricingService> PricingMock, Mock<ITestCatalogService> CatalogMock) CreateVM()
    {
        var pricingMock = new Mock<IPricingService>();
        var catalogMock = new Mock<ITestCatalogService>();
        var vm = new PriceSchemeWindowViewModel(pricingMock.Object, catalogMock.Object);
        return (vm, pricingMock, catalogMock);
    }

    [Fact]
    public async Task LoadAsync_ShouldPopulateSchemes()
    {
        var (vm, pricingMock, catalogMock) = CreateVM();
        pricingMock.Setup(p => p.GetAllSchemesAsync()).ReturnsAsync(new List<PriceScheme>
        {
            new() { SchemeId = 1, SchemeName = "Default", IsDefault = true, IsActive = true },
            new() { SchemeId = 2, SchemeName = "VIP", IsDefault = false, IsActive = true }
        });
        catalogMock.Setup(c => c.GetAllTestTypesAsync()).ReturnsAsync(new List<TestType>());

        await vm.LoadAsync();

        Assert.Equal(2, vm.Schemes.Count);
    }

    [Fact]
    public void AddCommand_ShouldSetIsEditing()
    {
        var (vm, _, _) = CreateVM();

        vm.AddCommand.Execute(null);

        Assert.True(vm.IsEditing);
        Assert.Contains("إضافة", vm.Title);
    }

    [Fact]
    public void SaveCommand_ShouldCallCreate_WhenNewScheme()
    {
        var (vm, pricingMock, catalogMock) = CreateVM();
        catalogMock.Setup(c => c.GetAllTestTypesAsync()).ReturnsAsync(new List<TestType>());
        vm.AddCommand.Execute(null);
        vm.EditModel.SchemeName = "New Scheme";
        pricingMock.Setup(p => p.CreateSchemeAsync(It.IsAny<PriceScheme>()))
            .ReturnsAsync((PriceScheme s) => { s.SchemeId = 10; return s; });

        vm.SaveCommand.Execute(null);

        pricingMock.Verify(p => p.CreateSchemeAsync(It.IsAny<PriceScheme>()), Times.Once);
        Assert.False(vm.IsEditing);
    }

    [Fact]
    public async Task SaveCommand_ShouldCallUpdate_WhenExistingScheme()
    {
        var (vm, pricingMock, catalogMock) = CreateVM();
        pricingMock.Setup(p => p.GetAllSchemesAsync()).ReturnsAsync(new List<PriceScheme>
        {
            new() { SchemeId = 1, SchemeName = "Old", IsDefault = true, IsActive = true }
        });
        catalogMock.Setup(c => c.GetAllTestTypesAsync()).ReturnsAsync(new List<TestType>());
        await vm.LoadAsync();

        vm.EditModel.SchemeId = 1;
        vm.EditModel.SchemeName = "Updated";
        vm.AddCommand.Execute(null);
        vm.EditModel.SchemeId = 1;
        vm.EditModel.SchemeName = "Updated";
        vm.SaveCommand.Execute(null);

        pricingMock.Verify(p => p.UpdateSchemeAsync(It.IsAny<PriceScheme>()), Times.Once);
    }

    [Fact]
    public void CancelCommand_ShouldResetIsEditing()
    {
        var (vm, _, _) = CreateVM();

        vm.CancelCommand.Execute(null);

        Assert.False(vm.IsEditing);
    }

    [Fact]
    public async Task SelectedScheme_ShouldLoadPrices()
    {
        var (vm, pricingMock, catalogMock) = CreateVM();
        pricingMock.Setup(p => p.GetAllSchemesAsync()).ReturnsAsync(new List<PriceScheme>
        {
            new() { SchemeId = 1, SchemeName = "S1", IsActive = true }
        });
        catalogMock.Setup(c => c.GetAllTestTypesAsync()).ReturnsAsync(new List<TestType>
        {
            new() { TesttypeId = 10, TypeCode = "T01", TypeNameEn = "CBC", IsActive = true }
        });
        pricingMock.Setup(p => p.GetTestPriceAsync(10, 1)).ReturnsAsync(50m);
        await vm.LoadAsync();

        vm.SelectedScheme = vm.Schemes[0];

        Assert.Single(vm.TestPrices);
        Assert.Equal(50m, vm.TestPrices[0].Price);
    }
}

using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Patients;

public class TestSelectionViewModelPricingTests
{
    private static (TestSelectionViewModel VM, Mock<ITestCatalogService> CatalogMock, Mock<IPricingService> PricingMock) CreateVM(int? schemeId = null)
    {
        var catalogMock = new Mock<ITestCatalogService>();
        var pricingMock = new Mock<IPricingService>();
        var engine = new TestPricingEngine(pricingMock.Object);
        var vm = new TestSelectionViewModel(catalogMock.Object, engine);
        if (schemeId.HasValue)
            vm.SchemeId = schemeId.Value;
        return (vm, catalogMock, pricingMock);
    }

    [Fact]
    public void AddTest_ShouldUsePriceFromPricingEngine()
    {
        var (vm, _, pricingMock) = CreateVM(schemeId: 5);
        pricingMock.Setup(p => p.GetTestPriceAsync(10, 5)).ReturnsAsync(75m);

        var item = new TestDisplayItem(10, "T01", "CBC", "CBC", 75m, "Blood", "Group1", "Cat1", "RoutineTests");
        vm.AddTestCommand.Execute(item);

        Assert.Single(vm.SelectedTests);
        Assert.Equal(75m, vm.SelectedTests[0].Price);
    }

    [Fact]
    public void AddTest_ShouldMarkFallback_WhenItemIsFallback()
    {
        var (vm, _, pricingMock) = CreateVM(schemeId: 5);
        pricingMock.Setup(p => p.GetTestPriceAsync(10, 5)).ReturnsAsync(0m);

        var item = new TestDisplayItem(10, "T01", "CBC", "CBC", 0m, "Blood", "Group1", "Cat1", "RoutineTests", IsFallback: true);
        vm.AddTestCommand.Execute(item);

        Assert.True(vm.SelectedTests[0].IsFallback);
    }

    [Fact]
    public void AddTest_ShouldNotDuplicate_WhenSameTestAddedTwice()
    {
        var (vm, _, pricingMock) = CreateVM();
        pricingMock.Setup(p => p.GetTestPriceAsync(10, It.IsAny<int>())).ReturnsAsync(50m);

        var item = new TestDisplayItem(10, "T01", "CBC", "CBC", 50m, "Blood", "Group1", "Cat1", "RoutineTests");
        vm.AddTestCommand.Execute(item);
        vm.AddTestCommand.Execute(item);

        Assert.Single(vm.SelectedTests);
    }

    [Fact]
    public void RemoveTest_ShouldDecreaseCount()
    {
        var (vm, _, pricingMock) = CreateVM();
        pricingMock.Setup(p => p.GetTestPriceAsync(10, It.IsAny<int>())).ReturnsAsync(50m);

        var item = new TestDisplayItem(10, "T01", "CBC", "CBC", 50m, "Blood", "Group1", "Cat1", "RoutineTests");
        vm.AddTestCommand.Execute(item);
        Assert.Single(vm.SelectedTests);

        vm.RemoveTestCommand.Execute(vm.SelectedTests[0]);

        Assert.Empty(vm.SelectedTests);
    }

    [Fact]
    public void GetSelectedPrices_ShouldReturnAllPrices()
    {
        var (vm, _, pricingMock) = CreateVM();
        pricingMock.Setup(p => p.GetTestPriceAsync(10, It.IsAny<int>())).ReturnsAsync(50m);
        pricingMock.Setup(p => p.GetTestPriceAsync(20, It.IsAny<int>())).ReturnsAsync(80m);

        vm.AddTestCommand.Execute(new TestDisplayItem(10, "T01", "CBC", "CBC", 50m, "Blood", "G", "C", "R"));
        vm.AddTestCommand.Execute(new TestDisplayItem(20, "T02", "ESR", "ESR", 80m, "Blood", "G", "C", "R"));

        var prices = vm.GetSelectedPrices();

        Assert.Equal(2, prices.Count);
        Assert.Contains(50m, prices);
        Assert.Contains(80m, prices);
    }

    [Fact]
    public void TestsChanged_ShouldFire_OnAdd()
    {
        var (vm, _, pricingMock) = CreateVM();
        pricingMock.Setup(p => p.GetTestPriceAsync(10, It.IsAny<int>())).ReturnsAsync(50m);
        bool fired = false;
        vm.TestsChanged += (_, _) => fired = true;

        vm.AddTestCommand.Execute(new TestDisplayItem(10, "T01", "CBC", "CBC", 50m, "Blood", "G", "C", "R"));

        Assert.True(fired);
    }
}

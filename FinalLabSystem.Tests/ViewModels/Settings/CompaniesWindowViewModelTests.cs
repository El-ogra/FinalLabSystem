using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class CompaniesWindowViewModelTests
{
    private static (CompaniesWindowViewModel VM, Mock<ICompanyService> Mock) CreateVM()
    {
        var mock = new Mock<ICompanyService>();
        var vm = new CompaniesWindowViewModel(mock.Object);
        return (vm, mock);
    }

    [Fact]
    public async Task LoadAsync_ShouldPopulateCompanies()
    {
        var (vm, mock) = CreateVM();
        mock.Setup(c => c.GetAllAsync()).ReturnsAsync(new List<Company>
        {
            new() { CompanyId = 1, CompanyName = "Corp A", CompanyType = "CORPORATE" },
            new() { CompanyId = 2, CompanyName = "Corp B", CompanyType = "CORPORATE" }
        });

        await vm.LoadAsync();

        Assert.Equal(2, vm.Companies.Count);
    }

    [Fact]
    public void AddCommand_ShouldSetIsEditing()
    {
        var (vm, _) = CreateVM();

        vm.AddCommand.Execute(null);

        Assert.True(vm.IsEditing);
        Assert.Contains("إضافة", vm.Title);
    }

    [Fact]
    public void SaveCommand_ShouldCallCreate_WhenNewCompany()
    {
        var (vm, mock) = CreateVM();
        vm.AddCommand.Execute(null);
        vm.EditModel.CompanyName = "New Corp";
        vm.EditModel.CompanyType = "CORPORATE";
        mock.Setup(c => c.CreateAsync(It.IsAny<Company>()))
            .ReturnsAsync((Company c) => { c.CompanyId = 10; return c; });

        vm.SaveCommand.Execute(null);

        mock.Verify(c => c.CreateAsync(It.IsAny<Company>()), Times.Once);
        Assert.False(vm.IsEditing);
    }

    [Fact]
    public async Task SaveCommand_ShouldCallUpdate_WhenExistingCompany()
    {
        var (vm, mock) = CreateVM();
        mock.Setup(c => c.GetAllAsync()).ReturnsAsync(new List<Company>
        {
            new() { CompanyId = 1, CompanyName = "Old Name", CompanyType = "CORPORATE" }
        });
        await vm.LoadAsync();
        vm.SelectedCompany = vm.Companies[0];

        vm.EditModel.CompanyName = "Updated Name";
        vm.SaveCommand.Execute(null);

        mock.Verify(c => c.UpdateAsync(It.IsAny<Company>()), Times.Once);
    }

    [Fact]
    public void CancelCommand_ShouldResetIsEditing()
    {
        var (vm, _) = CreateVM();
        vm.EditModel.CompanyName = "Test";

        vm.CancelCommand.Execute(null);

        Assert.False(vm.IsEditing);
    }

    [Fact]
    public void CompanyRowViewModel_ToModel_ShouldMapAllFields()
    {
        var row = new CompanyRowViewModel
        {
            CompanyId = 5,
            CompanyName = "Test Corp",
            CompanyType = "CORPORATE",
            ContactPerson = "John",
            Phone = "123",
            DiscountRate = 10,
            CreditLimit = 5000,
            ContractStartDate = new DateOnly(2026, 1, 1),
            ContractEndDate = new DateOnly(2026, 12, 31),
            BillingPeriodicity = "Monthly",
            IsActive = true
        };

        var model = row.ToModel();

        Assert.Equal(5, model.CompanyId);
        Assert.Equal("Test Corp", model.CompanyName);
        Assert.Equal(10, model.DiscountRate);
        Assert.Equal(new DateOnly(2026, 1, 1), model.ContractStartDate);
    }
}

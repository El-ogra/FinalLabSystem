using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class ContractInvoiceWindowViewModelTests
{
    private static (ContractInvoiceWindowViewModel VM, Mock<IInvoiceService> InvoiceMock, Mock<ICompanyService> CompanyMock) CreateVM()
    {
        var invoiceMock = new Mock<IInvoiceService>();
        var companyMock = new Mock<ICompanyService>();
        var vm = new ContractInvoiceWindowViewModel(invoiceMock.Object, companyMock.Object);
        return (vm, invoiceMock, companyMock);
    }

    [Fact]
    public void CanGenerateInvoice_ShouldBeFalse_WhenNoCompanySelected()
    {
        var (vm, _, _) = CreateVM();
        Assert.False(vm.CanGenerateInvoice);
    }

    [Fact]
    public void CanRecordPayment_ShouldBeFalse_WhenNoInvoiceSelected()
    {
        var (vm, _, _) = CreateVM();
        Assert.False(vm.CanRecordPayment);
    }

    [Fact]
    public void CanRecordPayment_ShouldBeFalse_WhenAmountIsZero()
    {
        var (vm, _, _) = CreateVM();
        vm.SelectedInvoice = new InvoiceRowViewModel();
        vm.PaymentAmount = 0;
        Assert.False(vm.CanRecordPayment);
    }

    [Fact]
    public void CanRecordPayment_ShouldBeTrue_WhenInvoiceSelectedAndAmountPositive()
    {
        var (vm, _, _) = CreateVM();
        vm.SelectedInvoice = new InvoiceRowViewModel();
        vm.PaymentAmount = 100;
        vm.PaymentMethod = "CASH";
        Assert.True(vm.CanRecordPayment);
    }

    [Fact]
    public void StatusMessage_ShouldUpdate_AfterInitialize()
    {
        var (vm, _, companyMock) = CreateVM();
        companyMock.Setup(c => c.GetAllAsync()).ReturnsAsync(new List<Company>());

        _ = vm.InitializeAsync();

        Assert.Contains("تم تحميل", vm.StatusMessage);
    }

    [Fact]
    public void InvoiceRowViewModel_Balance_ShouldCalculateCorrectly()
    {
        var row = new InvoiceRowViewModel
        {
            TotalAmount = 1000,
            PaidAmount = 400
        };
        Assert.Equal(600, row.Balance);
    }
}

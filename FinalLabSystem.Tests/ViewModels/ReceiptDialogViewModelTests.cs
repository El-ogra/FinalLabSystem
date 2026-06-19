using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class ReceiptDialogViewModelTests
{
    private static (ReceiptDialogViewModel vm, Mock<IReceiptService> mockReceiptService)
        CreateViewModel(bool canPrint = true, List<ReceiptGroupedTest>? groupedTests = null)
    {
        var mockReceiptService = new Mock<IReceiptService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();

        var staff = new Staff
        {
            StaffId = 1,
            Username = "testuser",
            DisplayName = "Test User",
            IsAdmin = false,
            IsActive = true
        };
        mockSession.Setup(s => s.CurrentUser).Returns(staff);

        mockReceiptService.Setup(s => s.CanPrintReceiptAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(canPrint);

        mockReceiptService.Setup(s => s.GetGroupedTestsForReceiptAsync(It.IsAny<int>()))
            .ReturnsAsync(groupedTests ?? new List<ReceiptGroupedTest>());

        mockReceiptService.Setup(s => s.LogPrintEventAsync(It.IsAny<ReceiptPrintLog>()))
            .Returns(Task.CompletedTask);

        var vm = new ReceiptDialogViewModel(
            mockReceiptService.Object,
            mockSession.Object,
            mockDialog.Object);

        return (vm, mockReceiptService);
    }

    private static VisitFullDto CreateVisitDto(int visitId = 1, decimal subtotal = 500,
        decimal discount = 100, decimal paid = 400, decimal balance = 100)
        => new()
        {
            VisitId = visitId,
            PatientId = 1,
            PatientCode = "P001",
            FullNameAr = "مريض تجريبي",
            EntryDate = DateTime.Now,
            Sex = "M",
            ApproxAge = 35,
            ApproxAgeUnit = "Years",
            Subtotal = subtotal,
            DiscountAmount = discount,
            DiscountPercent = subtotal > 0 ? Math.Round(discount / subtotal * 100, 2) : 0,
            TotalAfterDiscount = subtotal - discount,
            TotalPaid = paid,
            BalanceDue = balance,
            PaymentStatus = balance <= 0 ? "Paid" : "PartiallyPaid"
        };

    // ========== Initialization Tests ==========

    [Fact]
    public async Task InitializeAsync_SetsVisitData()
    {
        var (vm, _) = CreateViewModel();
        var dto = CreateVisitDto();

        await vm.InitializeAsync(dto);

        Assert.NotNull(vm.VisitData);
        Assert.Equal(dto.VisitId, vm.VisitData.VisitId);
        Assert.Equal(dto.FullNameAr, vm.VisitData.FullNameAr);
    }

    [Fact]
    public async Task InitializeAsync_CallsCanPrintReceipt()
    {
        var (vm, mock) = CreateViewModel(canPrint: true);
        var dto = CreateVisitDto();

        await vm.InitializeAsync(dto);

        mock.Verify(s => s.CanPrintReceiptAsync(dto.VisitId, 1), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_CallsGetGroupedTests()
    {
        var (vm, mock) = CreateViewModel();
        var dto = CreateVisitDto();

        await vm.InitializeAsync(dto);

        mock.Verify(s => s.GetGroupedTestsForReceiptAsync(dto.VisitId), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_CanPrintTrue_SetsCanPrint()
    {
        var (vm, _) = CreateViewModel(canPrint: true);
        var dto = CreateVisitDto();

        await vm.InitializeAsync(dto);

        Assert.True(vm.CanPrint);
    }

    [Fact]
    public async Task InitializeAsync_CanPrintFalse_SetsCanPrintFalse()
    {
        var (vm, _) = CreateViewModel(canPrint: false);
        var dto = CreateVisitDto();

        await vm.InitializeAsync(dto);

        Assert.False(vm.CanPrint);
    }

    // ========== Format Selection Tests ==========

    [Fact]
    public void DefaultFormat_IsA4()
    {
        var (vm, _) = CreateViewModel();

        Assert.Equal("A4", vm.SelectedFormat);
        Assert.True(vm.IsA4Selected);
    }

    [Fact]
    public void SelectedFormat_ChangeToThermal_UpdatesIsA4Selected()
    {
        var (vm, _) = CreateViewModel();

        vm.SelectedFormat = "Thermal";

        Assert.False(vm.IsA4Selected);
    }

    [Fact]
    public void SelectedFormat_ChangeToA4_UpdatesIsA4Selected()
    {
        var (vm, _) = CreateViewModel();
        vm.SelectedFormat = "Thermal";

        vm.SelectedFormat = "A4";

        Assert.True(vm.IsA4Selected);
    }

    // ========== ShowBreakdown Toggle Tests ==========

    [Fact]
    public void DefaultShowBreakdown_IsTrue()
    {
        var (vm, _) = CreateViewModel();

        Assert.True(vm.ShowBreakdown);
    }

    [Fact]
    public void ShowBreakdown_Toggle_Works()
    {
        var (vm, _) = CreateViewModel();

        vm.ShowBreakdown = false;

        Assert.False(vm.ShowBreakdown);
    }

    // ========== GroupedTests Tests ==========

    [Fact]
    public async Task InitializeAsync_SetsGroupedTests()
    {
        var grouped = new List<ReceiptGroupedTest>
        {
            new() { GroupName = "Kidney", TestCount = 3, TotalPrice = 37, IsSummarized = true, DetailLine = "Creatinine, BUN, Uric Acid" },
            new() { GroupName = "Glucose", TestCount = 1, TotalPrice = 6, IsSummarized = false }
        };
        var (vm, _) = CreateViewModel(groupedTests: grouped);
        var dto = CreateVisitDto();

        await vm.InitializeAsync(dto);

        Assert.Equal(2, vm.GroupedTests.Count);
        Assert.Equal("Kidney", vm.GroupedTests[0].GroupName);
        Assert.True(vm.GroupedTests[0].IsSummarized);
        Assert.False(vm.GroupedTests[1].IsSummarized);
    }

    // ========== LogPrintEventAsync Tests ==========

    [Fact]
    public async Task InitializeAsync_DoesNotLogPrintEvent()
    {
        var (vm, mock) = CreateViewModel();
        var dto = CreateVisitDto(subtotal: 1000, discount: 200, paid: 800, balance: 0);

        await vm.InitializeAsync(dto);

        mock.Verify(s => s.LogPrintEventAsync(It.IsAny<ReceiptPrintLog>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_StoresFinancialSnapshotForPrint()
    {
        var (vm, _) = CreateViewModel();
        var dto = CreateVisitDto(subtotal: 1000, discount: 200, paid: 800, balance: 0);

        await vm.InitializeAsync(dto);

        Assert.NotNull(vm.VisitData);
        Assert.Equal(1000m, vm.VisitData.Subtotal);
        Assert.Equal(200m, vm.VisitData.DiscountAmount);
        Assert.Equal(800m, vm.VisitData.TotalAfterDiscount);
        Assert.Equal(800m, vm.VisitData.TotalPaid);
        Assert.Equal(0m, vm.VisitData.BalanceDue);
    }

    // ========== Formats Collection Tests ==========

    [Fact]
    public void Formats_ContainsA4AndThermal()
    {
        var (vm, _) = CreateViewModel();

        Assert.Equal(2, vm.Formats.Count);
        Assert.Contains("A4", vm.Formats);
        Assert.Contains("Thermal", vm.Formats);
    }
}

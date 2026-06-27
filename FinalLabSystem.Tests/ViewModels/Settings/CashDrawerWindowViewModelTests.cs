using System;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class CashDrawerWindowViewModelTests
{
    private static (CashDrawerWindowViewModel VM, Mock<ICashDrawerService> CashMock, Mock<IDialogService> DialogMock, Mock<IPrintService> PrintMock) CreateVM()
    {
        var cashMock = new Mock<ICashDrawerService>();
        var dialogMock = new Mock<IDialogService>();
        var printMock = new Mock<IPrintService>();
        var vm = new CashDrawerWindowViewModel(cashMock.Object, dialogMock.Object, printMock.Object);
        return (vm, cashMock, dialogMock, printMock);
    }

    [Fact]
    public async Task TryUnlockAsync_NoPasswordSet_ShowsMessageAndReturnsFalse()
    {
        var (vm, cashMock, dialogMock, _) = CreateVM();
        cashMock.Setup(c => c.IsPasswordSetAsync()).ReturnsAsync(false);

        var result = await vm.TryUnlockAsync();

        Assert.False(result);
        dialogMock.Verify(d => d.ShowMessage(
            It.Is<string>(s => s.Contains("كلمة مرور")),
            It.Is<string>(s => s.Contains("در"))), Times.Once);
    }

    [Fact]
    public async Task TryUnlockAsync_PasswordSet_ReturnsTrue()
    {
        var (vm, cashMock, _, _) = CreateVM();
        cashMock.Setup(c => c.IsPasswordSetAsync()).ReturnsAsync(true);

        var result = await vm.TryUnlockAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task UnlockWithPasswordAsync_CorrectPassword_UnlocksAndLoads()
    {
        var (vm, cashMock, _, _) = CreateVM();
        cashMock.Setup(c => c.UnlockAsync("correct")).ReturnsAsync(true);
        cashMock.Setup(c => c.GetDailySummaryAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(new CashDrawerSummaryDto { GrandTotal = 100, PaymentCount = 1 });

        var result = await vm.UnlockWithPasswordAsync("correct");

        Assert.True(result);
        Assert.True(vm.IsUnlocked);
        Assert.NotNull(vm.Summary);
    }

    [Fact]
    public async Task UnlockWithPasswordAsync_WrongPassword_ShowsErrorAndReturnsFalse()
    {
        var (vm, cashMock, dialogMock, _) = CreateVM();
        cashMock.Setup(c => c.UnlockAsync("wrong")).ReturnsAsync(false);

        var result = await vm.UnlockWithPasswordAsync("wrong");

        Assert.False(result);
        Assert.False(vm.IsUnlocked);
        dialogMock.Verify(d => d.ShowError(It.Is<string>(s => s.Contains("غير صحيحة")), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void IsUnlocked_PropertyChanged_RaisesCanAccessDrawer()
    {
        var (vm, _, _, _) = CreateVM();
        bool canAccessChanged = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CashDrawerWindowViewModel.CanAccessDrawer))
                canAccessChanged = true;
        };

        vm.IsUnlocked = true;

        Assert.True(canAccessChanged);
        Assert.True(vm.CanAccessDrawer);
    }

    [Fact]
    public async Task ChangePasswordCommand_ExecutesWithoutError()
    {
        var (vm, cashMock, _, _) = CreateVM();
        cashMock.Setup(c => c.SetPasswordAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        vm.ChangePasswordCommand.Execute(null);

        await Task.Delay(100);
        cashMock.Verify(c => c.SetPasswordAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PrintCommand_CallsPrintService()
    {
        var (vm, cashMock, _, printMock) = CreateVM();
        cashMock.Setup(c => c.GetDailySummaryAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(new CashDrawerSummaryDto { GrandTotal = 50 });
        await vm.LoadSummaryAsync();
        vm.IsUnlocked = true;

        vm.PrintCommand.Execute(null);

        await Task.Delay(100);
        printMock.Verify(p => p.PrintAsync("CashDrawerSummary", It.IsAny<CashDrawerSummaryDto>()), Times.Once);
    }

    [Fact]
    public void Summary_PropertyChanged_IsRaised()
    {
        var (vm, _, _, _) = CreateVM();
        bool changed = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CashDrawerWindowViewModel.Summary))
                changed = true;
        };

        vm.Summary = new CashDrawerSummaryDto();

        Assert.True(changed);
    }
}

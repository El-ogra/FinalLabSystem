using System.Reflection;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.ViewModels.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class Phase5BuildVerificationTests
{
    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseInMemoryDatabase(nameof(Phase5BuildVerificationTests)));

        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ICashDrawerService, CashDrawerService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICommissionReportService, CommissionReportService>();
        services.AddScoped<IOutstandingBalanceReportService, OutstandingBalanceReportService>();

        services.AddSingleton<IDialogService>(new Mock<IDialogService>().Object);
        services.AddSingleton<IPrintService>(new Mock<IPrintService>().Object);

        services.AddLogging();

        services.AddTransient<HomeMenuViewModel>();
        services.AddTransient<AttendanceWindowViewModel>();
        services.AddTransient<CashDrawerWindowViewModel>();
        services.AddTransient<InventoryWindowViewModel>();
        services.AddTransient<CommissionReportWindowViewModel>();
        services.AddTransient<OutstandingBalanceWindowViewModel>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Phase5_AllServicesAreResolvable()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
        Assert.NotNull(attendance);
        Assert.IsType<AttendanceService>(attendance);

        var cashDrawer = scope.ServiceProvider.GetRequiredService<ICashDrawerService>();
        Assert.NotNull(cashDrawer);
        Assert.IsType<CashDrawerService>(cashDrawer);

        var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        Assert.NotNull(inventory);
        Assert.IsType<InventoryService>(inventory);

        var commission = scope.ServiceProvider.GetRequiredService<ICommissionReportService>();
        Assert.NotNull(commission);
        Assert.IsType<CommissionReportService>(commission);

        var outstanding = scope.ServiceProvider.GetRequiredService<IOutstandingBalanceReportService>();
        Assert.NotNull(outstanding);
        Assert.IsType<OutstandingBalanceReportService>(outstanding);
    }

    [Fact]
    public void Phase5_AllViewModelsAreResolvable()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var homeMenu = scope.ServiceProvider.GetRequiredService<HomeMenuViewModel>();
        Assert.NotNull(homeMenu);

        var attendance = scope.ServiceProvider.GetRequiredService<AttendanceWindowViewModel>();
        Assert.NotNull(attendance);

        var cashDrawer = scope.ServiceProvider.GetRequiredService<CashDrawerWindowViewModel>();
        Assert.NotNull(cashDrawer);

        var inventory = scope.ServiceProvider.GetRequiredService<InventoryWindowViewModel>();
        Assert.NotNull(inventory);

        var commission = scope.ServiceProvider.GetRequiredService<CommissionReportWindowViewModel>();
        Assert.NotNull(commission);

        var outstanding = scope.ServiceProvider.GetRequiredService<OutstandingBalanceWindowViewModel>();
        Assert.NotNull(outstanding);
    }

    [Fact]
    public void Phase5_AllWindowsAreRegistered()
    {
        var assembly = Assembly.GetAssembly(typeof(AttendanceWindowViewModel))!;

        var windowNames = new[]
        {
            "FinalLabSystem.Views.Settings.AttendanceWindow",
            "FinalLabSystem.Views.Settings.CashDrawerWindow",
            "FinalLabSystem.Views.Settings.InventoryWindow",
            "FinalLabSystem.Views.Settings.CommissionReportWindow",
            "FinalLabSystem.Views.Settings.OutstandingBalanceWindow"
        };

        foreach (var fullName in windowNames)
        {
            var type = assembly.GetType(fullName);
            Assert.NotNull(type);
        }
    }
}

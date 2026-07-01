using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using FinalLabSystem.Views.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class BackupRestoreWindowRegistrationTests
{
    [Fact]
    public void DI_Resolves_BackupRestoreWindowViewModel()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddSingleton<IDialogService>(Mock.Of<IDialogService>());
        services.AddSingleton<IProcessService>(Mock.Of<IProcessService>());
        services.AddTransient<BackupRestoreWindowViewModel>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var vm = scope.ServiceProvider.GetRequiredService<BackupRestoreWindowViewModel>();

        Assert.NotNull(vm);
    }

    [Fact]
    public void DI_Resolves_BackupRestoreWindow()
    {
        var serviceDescriptors = new ServiceCollection();
        serviceDescriptors.AddTransient<BackupRestoreWindow>();

        var hasRegistration = serviceDescriptors.Any(
            sd => sd.ServiceType == typeof(BackupRestoreWindow)
                && sd.Lifetime == ServiceLifetime.Transient);

        Assert.True(hasRegistration);
    }

    [Fact]
    public void DI_Resolves_BackupPasswordDialog()
    {
        var serviceDescriptors = new ServiceCollection();
        serviceDescriptors.AddTransient<BackupPasswordDialog>();

        var hasRegistration = serviceDescriptors.Any(
            sd => sd.ServiceType == typeof(BackupPasswordDialog)
                && sd.Lifetime == ServiceLifetime.Transient);

        Assert.True(hasRegistration);
    }

    [Fact]
    public void BackupRestoreWindow_SetsRequestShutdownCallback()
    {
        var mockBackup = new Mock<IBackupService>();
        var mockDialog = new Mock<IDialogService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockProcess = new Mock<IProcessService>();

        mockBackup.Setup(s => s.GetBackupOutputFolderAsync())
            .ReturnsAsync(@"C:\Test");
        mockBackup.Setup(s => s.ListBackupsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Models.DTOs.BackupMetadataDto>());

        var viewModel = new BackupRestoreWindowViewModel(
            mockBackup.Object,
            mockDialog.Object,
            mockSession.Object,
            mockProcess.Object);

        Assert.Null(viewModel.RequestShutdown);

        var mockNav = new Mock<INavigationService>();
        viewModel.RequestShutdown = () => mockNav.Object.Shutdown();

        viewModel.RequestShutdown.Invoke();

        mockNav.Verify(n => n.Shutdown(), Times.Once);
    }
}

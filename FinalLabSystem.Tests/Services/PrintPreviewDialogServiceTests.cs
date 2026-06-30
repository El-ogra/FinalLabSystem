using System.Windows.Documents;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.Views.Patients;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class PrintPreviewDialogServiceTests
{
    [Fact]
    public void Constructor_StoresServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var service = new PrintPreviewDialogService(mockServiceProvider.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public void Class_ImplementsIPrintPreviewDialogService()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var service = new PrintPreviewDialogService(mockServiceProvider.Object);

        Assert.IsAssignableFrom<IPrintPreviewDialogService>(service);
    }

    [Fact]
    public void Show_Throws_When_VM_Not_Registered()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceProvider>();

        mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);
        mockScopeFactory.Setup(sf => sf.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeProvider.Object);
        mockScopeProvider.Setup(sp => sp.GetService(typeof(PrintPreviewViewModel)))
            .Returns((object?)null);

        var service = new PrintPreviewDialogService(mockServiceProvider.Object);

        Assert.Throws<InvalidOperationException>(() =>
            service.Show(new FlowDocument(), "test"));
    }

    [Fact]
    public void Show_Throws_When_Window_Not_Registered()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeProvider = new Mock<IServiceProvider>();

        mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);
        mockScopeFactory.Setup(sf => sf.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeProvider.Object);
        mockScopeProvider.Setup(sp => sp.GetService(typeof(PrintPreviewViewModel)))
            .Returns(new PrintPreviewViewModel(Mock.Of<IPrintService>()));
        mockScopeProvider.Setup(sp => sp.GetService(typeof(PrintPreviewWindow)))
            .Returns((object?)null);

        var service = new PrintPreviewDialogService(mockServiceProvider.Object);

        Assert.Throws<InvalidOperationException>(() =>
            service.Show(new FlowDocument(), "test"));
    }

    [Fact]
    public void DI_Registration_PrintPreviewDialogService_Should_Be_Singleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPrintPreviewDialogService, PrintPreviewDialogService>();

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IPrintPreviewDialogService>();
        var second = provider.GetRequiredService<IPrintPreviewDialogService>();

        Assert.Same(first, second);
    }

    [Fact]
    public void DI_Registration_PrintPreviewViewModel_Should_Be_Transient()
    {
        var services = new ServiceCollection();
        services.AddTransient<PrintPreviewViewModel>();
        services.AddSingleton<IPrintService>(Mock.Of<IPrintService>());

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<PrintPreviewViewModel>();
        var second = provider.GetRequiredService<PrintPreviewViewModel>();

        Assert.NotSame(first, second);
    }

    [Fact]
    public void DI_Registration_PrintPreviewWindow_Has_Transient_Descriptor()
    {
        var services = new ServiceCollection();
        services.AddTransient<PrintPreviewWindow>();

        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(PrintPreviewWindow));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor!.Lifetime);
    }
}

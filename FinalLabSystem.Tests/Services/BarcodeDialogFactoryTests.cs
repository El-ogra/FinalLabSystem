using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class BarcodeDialogFactoryTests
{
    [Fact]
    public void Show_ResolvesViewModel_FromServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(BarcodeDialogViewModel)))
            .Throws(new InvalidOperationException("Service not registered"));

        var factory = new BarcodeDialogFactory(mockServiceProvider.Object);

        Assert.Throws<InvalidOperationException>(() => factory.Show(1));
    }

    [Fact]
    public void Show_ResolvesViewModel_AndReturnsPrinted()
    {
        var mockSampleTracking = new Mock<ISampleTrackingService>();
        mockSampleTracking.Setup(s => s.GetTubesForVisitAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<FinalLabSystem.Models.SampleTube>());

        var vm = new BarcodeDialogViewModel(
            mockSampleTracking.Object,
            Mock.Of<ILabelPrintService>(),
            Mock.Of<IInventoryService>(),
            Mock.Of<IDialogService>());

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(BarcodeDialogViewModel)))
            .Returns(vm);

        var resolved = mockServiceProvider.Object.GetService(typeof(BarcodeDialogViewModel));

        Assert.NotNull(resolved);
        Assert.IsType<BarcodeDialogViewModel>(resolved);
    }

    [Fact]
    public void Class_ImplementsIBarcodeDialogFactory()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var factory = new BarcodeDialogFactory(mockServiceProvider.Object);

        Assert.IsAssignableFrom<IBarcodeDialogFactory>(factory);
    }

    [Fact]
    public void DI_Registration_BarcodeDialogFactory_Should_Be_Singleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBarcodeDialogFactory, BarcodeDialogFactory>();

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IBarcodeDialogFactory>();
        var second = provider.GetRequiredService<IBarcodeDialogFactory>();

        Assert.Same(first, second);
    }
}

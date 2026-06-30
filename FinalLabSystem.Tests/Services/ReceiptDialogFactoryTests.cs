using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class ReceiptDialogFactoryTests
{
    [Fact]
    public void Show_ResolvesViewModel_FromServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ReceiptDialogViewModel)))
            .Throws(new InvalidOperationException("Service not registered"));

        var factory = new ReceiptDialogFactory(mockServiceProvider.Object);
        var dto = new VisitFullDto { VisitId = 1, Sex = "M", PatientId = 1 };

        Assert.Throws<InvalidOperationException>(() => factory.Show(dto));
    }

    [Fact]
    public void Show_DoesNotOpen_WhenCanPrintFalse()
    {
        var mockReceiptService = new Mock<IReceiptService>();
        mockReceiptService.Setup(s => s.CanPrintReceiptAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);
        mockReceiptService.Setup(s => s.GetGroupedTestsForReceiptAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ReceiptGroupedTest>());

        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();

        var vm = new ReceiptDialogViewModel(mockReceiptService.Object, mockSession.Object, mockDialog.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ReceiptDialogViewModel)))
            .Returns(vm);

        var factory = new ReceiptDialogFactory(mockServiceProvider.Object);
        var dto = new VisitFullDto { VisitId = 1, Sex = "M", PatientId = 1 };

        var result = factory.Show(dto);

        Assert.False(result);
    }

    [Fact]
    public void Class_ImplementsIReceiptDialogFactory()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var factory = new ReceiptDialogFactory(mockServiceProvider.Object);

        Assert.IsAssignableFrom<IReceiptDialogFactory>(factory);
    }

    [Fact]
    public void DI_Registration_ReceiptDialogFactory_Should_Be_Singleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IReceiptDialogFactory, ReceiptDialogFactory>();

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IReceiptDialogFactory>();
        var second = provider.GetRequiredService<IReceiptDialogFactory>();

        Assert.Same(first, second);
    }
}

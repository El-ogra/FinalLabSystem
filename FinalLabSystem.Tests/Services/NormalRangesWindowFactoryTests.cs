using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class NormalRangesWindowFactoryTests
{
    [Fact]
    public void Open_ResolvesWindow_FromServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(FinalLabSystem.Views.Settings.NormalRangesWindow)))
            .Throws(new InvalidOperationException("Service not registered"));

        var factory = new NormalRangesWindowFactory(mockServiceProvider.Object);
        var editableTest = new FinalLabSystem.Models.TestType { TesttypeId = 1 };

        Assert.Throws<InvalidOperationException>(() => factory.Open(editableTest));
    }

    [Fact]
    public void Class_ImplementsINormalRangesWindowFactory()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var factory = new NormalRangesWindowFactory(mockServiceProvider.Object);

        Assert.IsAssignableFrom<INormalRangesWindowFactory>(factory);
    }

    [Fact]
    public void DI_Registration_NormalRangesWindowFactory_Should_Be_Singleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INormalRangesWindowFactory, NormalRangesWindowFactory>();

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<INormalRangesWindowFactory>();
        var second = provider.GetRequiredService<INormalRangesWindowFactory>();

        Assert.Same(first, second);
    }
}

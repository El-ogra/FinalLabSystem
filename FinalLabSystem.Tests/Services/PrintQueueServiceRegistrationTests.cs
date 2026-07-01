using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class PrintQueueServiceRegistrationTests
{
    [Fact]
    public void DI_Resolves_IPrintQueueService_AsSingleton()
    {
        var services = new ServiceCollection();

        // Register the same services as App.xaml.cs would
        services.AddSingleton<IPrintQueueService, PrintQueueService>();
        services.AddSingleton<IServiceScopeFactory, MockScopeFactory>();

        var provider = services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<IPrintQueueService>();
        var instance2 = provider.GetRequiredService<IPrintQueueService>();

        Assert.Same(instance1, instance2);
    }
}

// Minimal mock for IServiceScopeFactory to satisfy DI resolution
internal class MockScopeFactory : IServiceScopeFactory
{
    public IServiceScope CreateScope() => throw new NotImplementedException();
}

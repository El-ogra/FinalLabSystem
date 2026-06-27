using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class InventoryServiceRegistrationTests
{
    [Fact]
    public void IInventoryService_IsRegisteredInDI()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(opt => opt.UseInMemoryDatabase("InventoryDI_Test"));
        services.AddLogging();
        services.AddScoped<IInventoryService, InventoryService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IInventoryService>();

        Assert.NotNull(service);
        Assert.IsType<InventoryService>(service);
    }

    [Fact]
    public void InventoryService_ImplementsIInventoryService()
    {
        Assert.True(typeof(IInventoryService).IsAssignableFrom(typeof(InventoryService)));
    }
}

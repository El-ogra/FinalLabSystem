using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class CashDrawerServiceRegistrationTests
{
    [Fact]
    public void ICashDrawerService_IsRegisteredInDI()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(opt => opt.UseInMemoryDatabase("CashDrawerDI_Test"));
        services.AddLogging();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ICashDrawerService, CashDrawerService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ICashDrawerService>();

        Assert.NotNull(service);
        Assert.IsType<CashDrawerService>(service);
    }
}

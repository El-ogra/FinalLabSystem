using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class OutstandingBalanceReportServiceRegistrationTests
{
    [Fact]
    public void IOutstandingBalanceReportService_IsRegisteredInDI()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(opt => opt.UseInMemoryDatabase("OutstandingBalanceDI_Test"));
        services.AddLogging();
        services.AddScoped<IOutstandingBalanceReportService, OutstandingBalanceReportService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IOutstandingBalanceReportService>();

        Assert.NotNull(service);
        Assert.IsType<OutstandingBalanceReportService>(service);
    }

    [Fact]
    public void OutstandingBalanceReportService_ImplementsIOutstandingBalanceReportService()
    {
        Assert.True(typeof(IOutstandingBalanceReportService).IsAssignableFrom(typeof(OutstandingBalanceReportService)));
    }
}

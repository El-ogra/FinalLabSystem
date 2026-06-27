using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class CommissionReportServiceRegistrationTests
{
    [Fact]
    public void ICommissionReportService_IsRegisteredInDI()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(opt => opt.UseInMemoryDatabase("CommissionReportDI_Test"));
        services.AddLogging();
        services.AddScoped<ICommissionReportService, CommissionReportService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ICommissionReportService>();

        Assert.NotNull(service);
        Assert.IsType<CommissionReportService>(service);
    }

    [Fact]
    public void CommissionReportService_ImplementsICommissionReportService()
    {
        Assert.True(typeof(ICommissionReportService).IsAssignableFrom(typeof(CommissionReportService)));
    }
}

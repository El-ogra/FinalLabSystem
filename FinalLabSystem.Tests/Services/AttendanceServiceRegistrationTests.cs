using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class AttendanceServiceRegistrationTests
{
    [Fact]
    public void IAttendanceService_IsRegisteredInDI()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(opt => opt.UseInMemoryDatabase("DI_Test"));
        services.AddLogging();
        services.AddScoped<IAttendanceService, AttendanceService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IAttendanceService>();

        Assert.NotNull(service);
        Assert.IsType<AttendanceService>(service);
    }
}

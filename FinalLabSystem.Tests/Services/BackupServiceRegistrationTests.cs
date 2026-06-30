using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Tests.Services;

public class BackupServiceRegistrationTests
{
    [Fact]
    public void BackupService_DI_Registration_Should_Be_Scoped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddScoped<IBackupService, BackupService>();

        var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<IBackupService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<IBackupService>();

        Assert.NotSame(service1, service2);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Integration;

public class Phase6BuildVerificationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseInMemoryDatabase("Phase6BuildVerificationTests"), ServiceLifetime.Scoped);

        services.AddLogging();
        services.AddScoped<IAuditService>(sp => new Mock<IAuditService>().Object);
        services.AddScoped<IDeliveryConfirmationService, DeliveryConfirmationService>();
        services.AddSingleton<IOtpGenerator, OtpGenerator>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllPhase6Services_Resolve_FromDI()
    {
        var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryConfirmationService>();
        Assert.NotNull(deliveryService);

        var otpGenerator = scope.ServiceProvider.GetRequiredService<IOtpGenerator>();
        Assert.NotNull(otpGenerator);
    }

    [Fact]
    public void IOtpGenerator_CanGenerate()
    {
        var generator = new OtpGenerator();
        var otp = generator.Generate();
        Assert.Matches(@"^\d{6}$", otp);
    }

    [Fact]
    public void DeliveryConfirmationService_CanBeCreated()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(DeliveryConfirmationService_CanBeCreated)));
        var auditService = new Mock<IAuditService>();
        var otpGenerator = new OtpGenerator();
        var logger = new Mock<ILogger<DeliveryConfirmationService>>();

        var service = new DeliveryConfirmationService(ctx, auditService.Object, otpGenerator, logger.Object);
        Assert.NotNull(service);
    }
}

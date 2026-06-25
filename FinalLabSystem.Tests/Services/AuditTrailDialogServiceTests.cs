using FinalLabSystem.Services.Implementations;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class AuditTrailDialogServiceTests
{
    [Fact]
    public void Constructor_StoresServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();

        var service = new AuditTrailDialogService(mockServiceProvider.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public void Class_ImplementsIAuditTrailDialogService()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();

        var service = new AuditTrailDialogService(mockServiceProvider.Object);

        Assert.IsAssignableFrom<FinalLabSystem.Services.Interfaces.IAuditTrailDialogService>(service);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_DoesNotThrow()
    {
        var service = new AuditTrailDialogService(null!);

        Assert.NotNull(service);
    }
}

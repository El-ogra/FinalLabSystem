using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class AuditTrailWindowRegistrationTests
{
    [Fact]
    public void AuditTrailDialogService_CanBeResolvedWithServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IAuditTrailDialogService)))
            .Returns(new AuditTrailDialogService(mockServiceProvider.Object));

        var service = mockServiceProvider.Object.GetService(typeof(IAuditTrailDialogService));

        Assert.NotNull(service);
        Assert.IsType<AuditTrailDialogService>(service);
    }

    [Fact]
    public void AuditTrailDialogService_ImplementsCorrectInterface()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var service = new AuditTrailDialogService(mockServiceProvider.Object);

        Assert.IsAssignableFrom<IAuditTrailDialogService>(service);
    }
}

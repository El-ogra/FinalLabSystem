using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class ResultEntryWindowRegistrationTests
{
    [Fact]
    public void ResultEntryDialogService_CanBeResolvedWithServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IResultEntryDialogService)))
            .Returns(new ResultEntryDialogService(mockServiceProvider.Object));

        var service = mockServiceProvider.Object.GetService(typeof(IResultEntryDialogService));

        Assert.NotNull(service);
        Assert.IsType<ResultEntryDialogService>(service);
    }

    [Fact]
    public void ResultEntryDialogService_ImplementsCorrectInterface()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var service = new ResultEntryDialogService(mockServiceProvider.Object);

        Assert.IsAssignableFrom<IResultEntryDialogService>(service);
    }
}

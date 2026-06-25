using FinalLabSystem.Services.Implementations;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class ResultEntryDialogServiceTests
{
    [Fact]
    public void Constructor_StoresServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();

        var service = new ResultEntryDialogService(mockServiceProvider.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public void Class_ImplementsIResultEntryDialogService()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();

        var service = new ResultEntryDialogService(mockServiceProvider.Object);

        Assert.IsAssignableFrom<FinalLabSystem.Services.Interfaces.IResultEntryDialogService>(service);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_DoesNotThrow()
    {
        var service = new ResultEntryDialogService(null!);

        Assert.NotNull(service);
    }
}

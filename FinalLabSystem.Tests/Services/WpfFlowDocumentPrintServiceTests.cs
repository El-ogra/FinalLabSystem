using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Windows.Documents;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class WpfFlowDocumentPrintServiceTests
{
    private readonly Mock<ILogger<WpfFlowDocumentPrintService>> _loggerMock = new();
    private readonly Mock<IFeatureToggleService> _toggleMock = new();

    [Fact]
    public async Task PrintAsync_WhenToggleDisabled_ReturnsSilently()
    {
        _toggleMock.Setup(t => t.IsEnabledAsync("EnableServerPrinting", false)).ReturnsAsync(false);
        var service = new WpfFlowDocumentPrintService(_loggerMock.Object, _toggleMock.Object);

        await service.PrintAsync("ResultReport", new object());
        // Should not throw
    }

    [Fact]
    public async Task PrintAsync_WhenDocumentTypeUnknown_ThrowsNotSupportedException()
    {
        _toggleMock.Setup(t => t.IsEnabledAsync("EnableServerPrinting", false)).ReturnsAsync(true);
        var service = new WpfFlowDocumentPrintService(_loggerMock.Object, _toggleMock.Object);

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            service.PrintAsync("UnknownType", new object()));
    }

    [Fact]
    public async Task PrintAsync_WithNullDocumentType_ThrowsArgumentNullException()
    {
        var service = new WpfFlowDocumentPrintService(_loggerMock.Object, _toggleMock.Object);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.PrintAsync(null!, new object()));
    }

    [Fact]
    public async Task PrintAsync_WithNullData_ThrowsArgumentNullException()
    {
        var service = new WpfFlowDocumentPrintService(_loggerMock.Object, _toggleMock.Object);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.PrintAsync("ResultReport", null!));
    }
    [Theory]
    [InlineData("CompositeReport")]
    [InlineData("Worksheet")]
    [InlineData("Envelope")]
    [InlineData("MedicalHistory")]
    [InlineData("BlankReport")]
    public async Task PrintAsync_WithSupportedStubDocumentType_WhenToggleEnabled_DoesNotThrowNotSupportedException(string documentType)
    {
        _toggleMock.Setup(t => t.IsEnabledAsync("EnableServerPrinting", false)).ReturnsAsync(true);
        var service = new TestableWpfFlowDocumentPrintService(_loggerMock.Object, _toggleMock.Object);

        var exception = await Record.ExceptionAsync(() => service.PrintAsync(documentType, new object()));

        Assert.False(exception is NotSupportedException);
    }

    private sealed class TestableWpfFlowDocumentPrintService : WpfFlowDocumentPrintService
    {
        public TestableWpfFlowDocumentPrintService(
            ILogger<WpfFlowDocumentPrintService> logger,
            IFeatureToggleService featureToggleService)
            : base(logger, featureToggleService)
        {
        }

        protected override Task PrintDocumentAsync(string documentType, FlowDocument document)
        {
            return Task.CompletedTask;
        }
    }
}

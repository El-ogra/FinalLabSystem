using System.Reflection;
using System.Windows.Documents;
using FinalLabSystem.Services;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class IPrintServiceExtensionTests
{
    [Fact]
    public void IPrintService_Should_Expose_PrintFlowDocumentAsync_Method()
    {
        var method = typeof(IPrintService).GetMethod(nameof(IPrintService.PrintFlowDocumentAsync));
        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Contains(parameters, p => p.ParameterType == typeof(FlowDocument) && p.Name == "document");
        Assert.Contains(parameters, p => p.ParameterType == typeof(string) && p.Name == "description");
    }

    [Fact]
    public async Task WpfFlowDocumentPrintService_PrintFlowDocumentAsync_Should_Use_PrintDialog_AndPrintDocument()
    {
        var loggerMock = new Mock<ILogger<WpfFlowDocumentPrintService>>();
        var toggleMock = new Mock<IFeatureToggleService>();
        toggleMock.Setup(t => t.IsEnabledAsync(FeatureToggles.EnableServerPrinting, false)).ReturnsAsync(true);

        var service = new PrintFlowDocumentTestableService(loggerMock.Object, toggleMock.Object);
        bool printDialogCalled = false;
        service.OnPrintAction = _ => printDialogCalled = true;

        await service.PrintFlowDocumentAsync(new FlowDocument(), "test description");

        Assert.True(printDialogCalled);
    }

    [Fact]
    public async Task WpfFlowDocumentPrintService_PrintFlowDocumentAsync_Should_Throw_When_Document_Null()
    {
        var loggerMock = new Mock<ILogger<WpfFlowDocumentPrintService>>();
        var toggleMock = new Mock<IFeatureToggleService>();

        var service = new WpfFlowDocumentPrintService(loggerMock.Object, toggleMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.PrintFlowDocumentAsync(null!, "test"));
    }

    [Fact]
    public async Task WpfFlowDocumentPrintService_PrintFlowDocumentAsync_WhenToggleDisabled_ReturnsSilently_DoesNotCallPrint()
    {
        var loggerMock = new Mock<ILogger<WpfFlowDocumentPrintService>>();
        var toggleMock = new Mock<IFeatureToggleService>();
        toggleMock.Setup(t => t.IsEnabledAsync(FeatureToggles.EnableServerPrinting, false)).ReturnsAsync(false);

        var service = new PrintFlowDocumentTestableService(loggerMock.Object, toggleMock.Object);
        bool printDialogCalled = false;
        service.OnPrintAction = _ => printDialogCalled = true;

        await service.PrintFlowDocumentAsync(new FlowDocument(), "test");

        Assert.False(printDialogCalled);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private sealed class PrintFlowDocumentTestableService : WpfFlowDocumentPrintService
    {
        public Action<string>? OnPrintAction { get; set; }

        public PrintFlowDocumentTestableService(
            ILogger<WpfFlowDocumentPrintService> logger,
            IFeatureToggleService featureToggleService)
            : base(logger, featureToggleService)
        {
        }

        protected override Task ShowPrintDialogAndPrintAsync(FlowDocument document, string description)
        {
            OnPrintAction?.Invoke(description);
            return Task.CompletedTask;
        }
    }
}

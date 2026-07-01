using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class PrintQueueServiceTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IPrintService> _printServiceMock;
    private readonly Mock<IServiceProvider> _providerMock;
    private readonly PrintQueueService _sut;

    public PrintQueueServiceTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _printServiceMock = new Mock<IPrintService>();
        _providerMock = new Mock<IServiceProvider>();

        _providerMock.Setup(p => p.GetService(typeof(IPrintService)))
                     .Returns(_printServiceMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_providerMock.Object);
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

        _sut = new PrintQueueService(_scopeFactoryMock.Object);
    }

    private static PrintQueueItemDto CreateItem(int visitId = 1, string docType = "Receipt")
    {
        return new PrintQueueItemDto
        {
            VisitId = visitId,
            PatientName = "Test Patient",
            DocumentType = docType,
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Enqueue_AddsItem_WithDateTimeUtcNow()
    {
        var item = CreateItem();
        var before = DateTime.UtcNow.AddSeconds(-1);

        _sut.Enqueue(item);

        var items = _sut.GetItems();
        Assert.Single(items);
        Assert.True(items[0].AddedAt >= before);
        Assert.Equal(PrintQueueItemStatus.Pending, items[0].Status);
    }

    [Fact]
    public void Remove_RemovesSpecificItem()
    {
        var item1 = CreateItem(1, "Receipt");
        var item2 = CreateItem(2, "Report");
        var item3 = CreateItem(3, "Invoice");

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        _sut.Remove(item2);

        var items = _sut.GetItems();
        Assert.Equal(2, items.Count);
        Assert.DoesNotContain(items, i => i.VisitId == 2);
    }

    [Fact]
    public void Clear_EmptiesQueue()
    {
        _sut.Enqueue(CreateItem(1));
        _sut.Enqueue(CreateItem(2));
        _sut.Enqueue(CreateItem(3));

        _sut.Clear();

        Assert.Empty(_sut.GetItems());
    }

    [Fact]
    public async Task PrintAllAsync_CallsPrintService_ForEachItem()
    {
        _sut.Enqueue(CreateItem(1));
        _sut.Enqueue(CreateItem(2));
        _sut.Enqueue(CreateItem(3));

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        await _sut.PrintAllAsync(null, CancellationToken.None);

        _printServiceMock.Verify(
            s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task PrintAllAsync_ReportsProgress_AfterEachItem()
    {
        _sut.Enqueue(CreateItem(1));
        _sut.Enqueue(CreateItem(2));
        _sut.Enqueue(CreateItem(3));

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var progressMock = new Mock<IProgress<double>>();
        var reportedValues = new List<double>();
        progressMock.Setup(p => p.Report(It.IsAny<double>()))
                    .Callback<double>(v => reportedValues.Add(v));

        await _sut.PrintAllAsync(progressMock.Object, CancellationToken.None);

        Assert.Equal(3, reportedValues.Count);
        Assert.Equal(33.3, reportedValues[0], 1);
        Assert.Equal(66.7, reportedValues[1], 1);
        Assert.Equal(100.0, reportedValues[2], 1);
    }

    [Fact]
    public async Task PrintAllAsync_OneItemFails_ContinuesOthers_MarksFailed()
    {
        var item1 = CreateItem(1);
        var item2 = CreateItem(2);
        var item3 = CreateItem(3);

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        int callCount = 0;
        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 2)
                    throw new InvalidOperationException("Printer jam");
                return Task.CompletedTask;
            });

        await _sut.PrintAllAsync(null, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Done, item1.Status);
        Assert.Equal(PrintQueueItemStatus.Failed, item2.Status);
        Assert.Equal("Printer jam", item2.Error);
        Assert.Equal(PrintQueueItemStatus.Done, item3.Status);
    }

    [Fact]
    public async Task PrintAllAsync_RespectsCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel before calling

        _sut.Enqueue(CreateItem(1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.PrintAllAsync(null, cts.Token));

        _printServiceMock.Verify(
            s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }

    [Fact]
    public async Task PrintAllAsync_EmptyQueue_CompletesImmediately()
    {
        var progressMock = new Mock<IProgress<double>>();

        await _sut.PrintAllAsync(progressMock.Object, CancellationToken.None);

        progressMock.Verify(p => p.Report(It.IsAny<double>()), Times.Never);
        _printServiceMock.Verify(
            s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }

    [Fact]
    public async Task PrintAllAsync_CancelledMidBatch_StopsAfterCurrentItem()
    {
        var cts = new CancellationTokenSource();
        var item1 = CreateItem(1);
        var item2 = CreateItem(2);
        var item3 = CreateItem(3);

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        int callCount = 0;
        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 2)
                    cts.Cancel();
                return Task.CompletedTask;
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.PrintAllAsync(null, cts.Token));

        Assert.Equal(PrintQueueItemStatus.Done, item1.Status);
        Assert.Equal(PrintQueueItemStatus.Done, item2.Status);
        Assert.Equal(PrintQueueItemStatus.Pending, item3.Status);
    }

    [Fact]
    public async Task PrintAllAsync_AllItemsFail_MarksAllFailed()
    {
        var item1 = CreateItem(1);
        var item2 = CreateItem(2);
        var item3 = CreateItem(3);

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("Test failure"));

        var progressMock = new Mock<IProgress<double>>();

        await _sut.PrintAllAsync(progressMock.Object, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Failed, item1.Status);
        Assert.Equal(PrintQueueItemStatus.Failed, item2.Status);
        Assert.Equal(PrintQueueItemStatus.Failed, item3.Status);
        progressMock.Verify(p => p.Report(100.0), Times.Once);
    }

    [Fact]
    public void Enqueue_SetsStatusToPending()
    {
        var item = CreateItem();
        item.Status = PrintQueueItemStatus.Printing; // intentionally wrong

        _sut.Enqueue(item);

        var items = _sut.GetItems();
        Assert.Equal(PrintQueueItemStatus.Pending, items[0].Status);
        // Enqueue resets Status to Pending regardless of the item's
        // prior status, ensuring every queued item starts as Pending.
    }

    [Fact]
    public async Task PrintAllAsync_SetsPrintingStatus_DuringProcessing()
    {
        var item = CreateItem(1);
        _sut.Enqueue(item);

        PrintQueueItemStatus? capturedStatus = null;
        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Callback<string, object>((dt, data) =>
            {
                var dto = (PrintQueueItemDto)data;
                capturedStatus = dto.Status;
            })
            .Returns(Task.CompletedTask);

        await _sut.PrintAllAsync(null, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Printing, capturedStatus);
        Assert.Equal(PrintQueueItemStatus.Done, item.Status);
    }

    [Fact]
    public async Task PrintAllAsync_ErrorPropertySet_OnFailure()
    {
        var item = CreateItem(1);
        _sut.Enqueue(item);

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("Specific error message"));

        await _sut.PrintAllAsync(null, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Failed, item.Status);
        Assert.Equal("Specific error message", item.Error);
    }
}

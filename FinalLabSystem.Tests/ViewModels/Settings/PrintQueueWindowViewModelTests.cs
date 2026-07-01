using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class PrintQueueWindowViewModelTests
{
    private readonly Mock<IPrintQueueService> _printQueueServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly PrintQueueWindowViewModel _sut;

    public PrintQueueWindowViewModelTests()
    {
        _printQueueServiceMock = new Mock<IPrintQueueService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _sut = new PrintQueueWindowViewModel(
            _printQueueServiceMock.Object,
            _dialogServiceMock.Object);
    }

    private static PrintQueueItemDto CreateItem(int id, PrintQueueItemStatus status = PrintQueueItemStatus.Pending)
    {
        return new PrintQueueItemDto
        {
            VisitId = id,
            PatientName = $"Patient {id}",
            DocumentType = "Receipt",
            Status = status,
            AddedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task LoadCommand_PopulatesItems_FromService()
    {
        var items = new List<PrintQueueItemDto>
        {
            CreateItem(1), CreateItem(2), CreateItem(3), CreateItem(4), CreateItem(5)
        };
        _printQueueServiceMock.Setup(s => s.GetItems()).Returns(items);

        await _sut.LoadCommand.ExecuteAsync(null);

        Assert.Equal(5, _sut.Items.Count);
    }

    [Fact]
    public async Task PrintAllCommand_TogglesIsRunning_DuringExecution()
    {
        var tcs = new TaskCompletionSource();
        _printQueueServiceMock
            .Setup(s => s.PrintAllAsync(It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        _sut.Items.Add(CreateItem(1));

        var task = _sut.PrintAllCommand.ExecuteAsync(null);

        Assert.True(_sut.IsRunning);

        tcs.SetResult();
        await task;

        Assert.False(_sut.IsRunning);
    }

    [Fact]
    public async Task CancelCommand_TriggersCancellationTokenSource_Cancel()
    {
        CancellationToken? capturedToken = null;
        var tcs = new TaskCompletionSource();

        _printQueueServiceMock
            .Setup(s => s.PrintAllAsync(It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()))
            .Callback<IProgress<double>, CancellationToken>((p, ct) =>
            {
                capturedToken = ct;
            })
            .Returns(tcs.Task);

        _sut.Items.Add(CreateItem(1));

        var printTask = _sut.PrintAllCommand.ExecuteAsync(null);
        await Task.Delay(50); // Let the async operation start

        _sut.CancelCommand.Execute(null);

        Assert.NotNull(capturedToken);
        Assert.True(capturedToken.Value.IsCancellationRequested);

        tcs.SetCanceled(); // Complete the task to avoid hanging
        await printTask;
    }
}

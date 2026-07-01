using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class PrintQueueWindowViewModel : FinalLabSystem.Infrastructure.ViewModelBase
{
    private readonly IPrintQueueService _printQueueService;
    private readonly IDialogService _dialogService;

    private CancellationTokenSource? _cancellationTokenSource;

    public PrintQueueWindowViewModel(
        IPrintQueueService printQueueService,
        IDialogService dialogService)
    {
        _printQueueService = printQueueService;
        _dialogService = dialogService;

        Items = new ObservableCollection<PrintQueueItemDto>();

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        PrintAllCommand = new AsyncRelayCommand(PrintAllAsync, () => !IsRunning && Items.Count > 0);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, () => SelectedItem != null);
    }

    public ObservableCollection<PrintQueueItemDto> Items { get; }

    private double _progress;
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            SetProperty(ref _isRunning, value);
            PrintAllCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private PrintQueueItemDto? _selectedItem;
    public PrintQueueItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            RemoveSelectedCommand.NotifyCanExecuteChanged();
        }
    }

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand PrintAllCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand RemoveSelectedCommand { get; }

    private async Task LoadAsync()
    {
        Items.Clear();
        foreach (var item in _printQueueService.GetItems())
        {
            Items.Add(item);
        }
        StatusText = $"{Items.Count} items in queue.";
        await System.Threading.Tasks.Task.CompletedTask;
    }

    private async Task PrintAllAsync()
    {
        IsRunning = true;
        Progress = 0;
        StatusText = "Printing...";
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(value =>
            {
                Progress = value;
                StatusText = $"Printing... {value:F0}%";
            });

            await _printQueueService.PrintAllAsync(progress, _cancellationTokenSource.Token);

            int failed = Items.Count(i => i.Status == PrintQueueItemStatus.Failed);
            int done = Items.Count(i => i.Status == PrintQueueItemStatus.Done);
            StatusText = failed > 0
                ? $"Completed. {done} printed, {failed} failed."
                : $"All {done} items printed successfully.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Print cancelled by user.";
        }
        finally
        {
            IsRunning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void RemoveSelected()
    {
        if (SelectedItem == null) return;
        _printQueueService.Remove(SelectedItem);
        Items.Remove(SelectedItem);
        SelectedItem = null;
    }
}

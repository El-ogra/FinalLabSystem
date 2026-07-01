using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.Services.Implementations;

public class PrintQueueService : IPrintQueueService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<PrintQueueItemDto> _items = new();
    private readonly object _lock = new();

    public PrintQueueService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Enqueue(PrintQueueItemDto item)
    {
        lock (_lock)
        {
            item.Status = PrintQueueItemStatus.Pending;
            _items.Add(item);
        }
    }

    public void Remove(PrintQueueItemDto item)
    {
        lock (_lock)
        {
            _items.Remove(item);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }

    public List<PrintQueueItemDto> GetItems()
    {
        lock (_lock)
        {
            return new List<PrintQueueItemDto>(_items);
        }
    }

    public async Task PrintAllAsync(IProgress<double>? progress, CancellationToken cancellationToken)
    {
        PrintQueueItemDto[] snapshot;
        lock (_lock)
        {
            snapshot = _items.ToArray();
        }

        int total = snapshot.Length;
        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PrintQueueItemDto item = snapshot[i];
            item.Status = PrintQueueItemStatus.Printing;

            using IServiceScope scope = _scopeFactory.CreateScope();
            IPrintService printService = scope.ServiceProvider.GetRequiredService<IPrintService>();

            try
            {
                await printService.PrintAsync(item.DocumentType, item);
                item.Status = PrintQueueItemStatus.Done;
            }
            catch (Exception ex)
            {
                item.Status = PrintQueueItemStatus.Failed;
                item.Error = ex.Message;
            }

            double percent = total > 0 ? (double)(i + 1) / total * 100.0 : 100.0;
            progress?.Report(percent);
        }
    }
}

using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IPrintQueueService
{
    void Enqueue(PrintQueueItemDto item);
    void Remove(PrintQueueItemDto item);
    void Clear();
    List<PrintQueueItemDto> GetItems();
    Task PrintAllAsync(IProgress<double>? progress, CancellationToken cancellationToken);
}

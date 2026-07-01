using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models.DTOs;

public class PrintQueueItemDto
{
    public int VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public PrintQueueItemStatus Status { get; set; } = PrintQueueItemStatus.Pending;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
}

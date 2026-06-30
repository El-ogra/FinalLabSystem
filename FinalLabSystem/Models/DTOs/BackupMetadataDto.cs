namespace FinalLabSystem.Models.DTOs;

public class BackupMetadataDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public int? CreatedByStaffId { get; set; }
    public bool IsEncrypted { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;
}

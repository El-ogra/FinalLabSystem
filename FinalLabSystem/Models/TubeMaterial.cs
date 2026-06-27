using System.ComponentModel.DataAnnotations;

namespace FinalLabSystem.Models;

public class TubeMaterial
{
    public int TubeMaterialId { get; set; }

    [Required]
    [MaxLength(50)]
    public string MaterialName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? MaterialNameAr { get; set; }

    [MaxLength(20)]
    public string? TubeColor { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public int CurrentStock { get; set; }

    public int MinimumStock { get; set; }
}

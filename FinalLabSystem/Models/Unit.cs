using System.ComponentModel.DataAnnotations;

namespace FinalLabSystem.Models;

public class Unit
{
    public int UnitId { get; set; }

    [Required]
    [MaxLength(30)]
    public string UnitName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? UnitNameAr { get; set; }

    [MaxLength(10)]
    public string? Abbreviation { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}

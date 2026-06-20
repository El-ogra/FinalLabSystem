using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models.DTOs;

public sealed class TestComponentResultDto
{
    public int ComponentId { get; set; }

    public string ComponentCode { get; set; } = string.Empty;

    public string ComponentName { get; set; } = string.Empty;

    public string? Unit { get; set; }

    public string ResultType { get; set; } = "Numeric";

    public byte DecimalPlaces { get; set; }

    public int? ResultId { get; set; }

    public string? ResultValue { get; set; }

    public decimal? ResultNumeric { get; set; }

    public string? ResultStatus { get; set; }

    public string? Comment { get; set; }

    public string? SnapUnit { get; set; }

    public double? SnapLowNormal { get; set; }

    public double? SnapHighNormal { get; set; }

    public string? SnapNormalText { get; set; }

    public ResultValidationStatus ValidationStatus { get; set; }

    public string? EnteredByName { get; set; }

    public DateTime? EnteredAt { get; set; }

    public string? LastModifiedByName { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public int SortOrder { get; set; }
}

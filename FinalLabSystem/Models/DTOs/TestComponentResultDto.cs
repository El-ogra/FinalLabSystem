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

    public string LOrH
    {
        get
        {
            if (ResultNumeric == null || SnapLowNormal == null || SnapHighNormal == null)
                return string.Empty;
            var val = (double)ResultNumeric.Value;
            var low = SnapLowNormal.Value;
            var high = SnapHighNormal.Value;
            if (val >= high * 2) return "HH";
            if (val <= low * 2) return "LL";
            if (val > high) return "H";
            if (val < low) return "L";
            return string.Empty;
        }
    }

    public bool IsVerified => ValidationStatus >= ResultValidationStatus.Reviewed;

    public bool IsPrintEnabled { get; set; }
}

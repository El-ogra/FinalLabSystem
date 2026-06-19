using CsvHelper.Configuration.Attributes;

namespace FinalLabSystem.Services.Implementations;

/// <summary>
/// Flat DTO representing a single row from the seed CSV file.
/// Maps 1:1 to every column in the header row. No parsing, validation,
/// or business logic — pure data transfer.
/// </summary>
public sealed class CsvSeedRow
{
    // ── TestCategory columns ──

    [Name("category_code")]
    public string CategoryCode { get; set; } = string.Empty;

    [Name("category_name_en")]
    public string CategoryNameEn { get; set; } = string.Empty;

    [Name("category_name_ar")]
    public string? CategoryNameAr { get; set; }

    [Name("category_sort_order")]
    public int CategorySortOrder { get; set; }

    [Name("category_is_active")]
    public bool CategoryIsActive { get; set; }

    // ── TestGroup columns ──

    [Name("group_code")]
    public string GroupCode { get; set; } = string.Empty;

    [Name("group_name_en")]
    public string GroupNameEn { get; set; } = string.Empty;

    [Name("group_name_ar")]
    public string? GroupNameAr { get; set; }

    [Name("group_sort_order")]
    public int GroupSortOrder { get; set; }

    [Name("group_is_active")]
    public bool GroupIsActive { get; set; }

    // ── TestType columns ──

    [Name("type_code")]
    public string TypeCode { get; set; } = string.Empty;

    [Name("type_name_en")]
    public string TypeNameEn { get; set; } = string.Empty;

    [Name("type_name_ar")]
    public string? TypeNameAr { get; set; }

    [Name("type_abbrev")]
    public string? TypeAbbrev { get; set; }

    [Name("type_default_price")]
    public decimal TypeDefaultPrice { get; set; }

    [Name("type_sample_type")]
    public string? TypeSampleType { get; set; }

    [Name("type_default_tube_type")]
    public string? TypeDefaultTubeType { get; set; }

    [Name("type_default_tube_color")]
    public string? TypeDefaultTubeColor { get; set; }

    [Name("type_turnaround_hours")]
    public short? TypeTurnaroundHours { get; set; }

    [Name("type_special_type")]
    public string TypeSpecialType { get; set; } = string.Empty;

    [Name("type_sort_order")]
    public int TypeSortOrder { get; set; }

    [Name("type_is_active")]
    public bool TypeIsActive { get; set; }

    [Name("type_notes")]
    public string? TypeNotes { get; set; }

    [Name("type_report_name_line1")]
    public string? TypeReportNameLine1 { get; set; }

    [Name("type_report_name_line2")]
    public string? TypeReportNameLine2 { get; set; }

    [Name("type_bill_name_line1")]
    public string? TypeBillNameLine1 { get; set; }

    [Name("type_bill_name_line2")]
    public string? TypeBillNameLine2 { get; set; }

    [Name("type_history_name")]
    public string? TypeHistoryName { get; set; }

    [Name("type_collection_notes")]
    public string? TypeCollectionNotes { get; set; }

    [Name("type_collection_type_id")]
    public int? TypeCollectionTypeId { get; set; }

    [Name("type_outside_lab_name")]
    public string? TypeOutsideLabName { get; set; }

    [Name("type_outside_cost_price")]
    public decimal? TypeOutsideCostPrice { get; set; }

    [Name("type_patient_question")]
    public string? TypePatientQuestion { get; set; }

    [Name("type_reference_type")]
    public string? TypeReferenceType { get; set; }

    [Name("type_barcode_name")]
    public string? TypeBarcodeName { get; set; }

    [Name("type_is_routine_test")]
    public bool TypeIsRoutineTest { get; set; }

    [Name("type_see_report")]
    public bool TypeSeeReport { get; set; }

    [Name("type_print_with_other")]
    public bool TypePrintWithOther { get; set; }

    [Name("type_add_with_group")]
    public bool TypeAddWithGroup { get; set; }

    [Name("type_is_main_test")]
    public bool TypeIsMainTest { get; set; }

    [Name("type_is_send_outside")]
    public bool TypeIsSendOutside { get; set; }

    [Name("type_behavior")]
    public int TypeBehavior { get; set; }

    // ── TestComponent columns ──

    [Name("component_code")]
    public string ComponentCode { get; set; } = string.Empty;

    [Name("component_name_en")]
    public string ComponentNameEn { get; set; } = string.Empty;

    [Name("component_name_ar")]
    public string? ComponentNameAr { get; set; }

    [Name("component_unit")]
    public string ComponentUnit { get; set; } = string.Empty;

    [Name("component_result_type")]
    public string ComponentResultType { get; set; } = string.Empty;

    [Name("component_decimal_places")]
    public int ComponentDecimalPlaces { get; set; }

    [Name("component_sort_order")]
    public int ComponentSortOrder { get; set; }

    [Name("component_is_active")]
    public bool ComponentIsActive { get; set; }

    // ── NormalRange columns ──

    [Name("range_sex")]
    public string RangeSex { get; set; } = string.Empty;

    [Name("range_age_from_days")]
    public int? RangeAgeFromDays { get; set; }

    [Name("range_age_to_days")]
    public int? RangeAgeToDays { get; set; }

    [Name("range_age_from_value")]
    public int? RangeAgeFromValue { get; set; }

    [Name("range_age_to_value")]
    public int? RangeAgeToValue { get; set; }

    [Name("range_age_description")]
    public string? RangeAgeDescription { get; set; }

    [Name("range_for_pregnant_only")]
    public bool? RangeForPregnantOnly { get; set; }

    [Name("range_age_unit")]
    public string? RangeAgeUnit { get; set; }

    [Name("range_low_flag")]
    public string? RangeLowFlag { get; set; }

    [Name("range_high_flag")]
    public string? RangeHighFlag { get; set; }

    [Name("range_low_comment")]
    public string? RangeLowComment { get; set; }

    [Name("range_high_comment")]
    public string? RangeHighComment { get; set; }

    [Name("range_critical_range_text")]
    public string? RangeCriticalRangeText { get; set; }

    [Name("range_critical_flag")]
    public string? RangeCriticalFlag { get; set; }

    [Name("range_critical_comment")]
    public string? RangeCriticalComment { get; set; }

    [Name("range_fasting_state")]
    public string RangeFastingState { get; set; } = string.Empty;

    [Name("range_low_normal")]
    public double? RangeLowNormal { get; set; }

    [Name("range_high_normal")]
    public double? RangeHighNormal { get; set; }

    [Name("range_low_critical")]
    public double? RangeLowCritical { get; set; }

    [Name("range_high_critical")]
    public double? RangeHighCritical { get; set; }

    [Name("range_normal_range_text")]
    public string? RangeNormalRangeText { get; set; }

    [Name("range_note")]
    public string? RangeNote { get; set; }

    [Name("range_unit")]
    public string? RangeUnit { get; set; }

    [Name("range_version")]
    public int RangeVersion { get; set; }

    [Name("range_is_active")]
    public bool RangeIsActive { get; set; }

    // ── Metadata-only columns (not mapped to entities) ──

    [Name("source_status")]
    public string? SourceStatus { get; set; }

    [Name("seed_decision")]
    public string? SeedDecision { get; set; }
}

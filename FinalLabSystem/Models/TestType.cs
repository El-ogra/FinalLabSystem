using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalLabSystem.Models;

[Flags]
public enum TestTypeBehavior
{
    None = 0,
    IsRoutineTest = 1 << 0,
    SeeReport = 1 << 1,
    PrintWithOther = 1 << 2,
    AddWithGroup = 1 << 3,
    IsMainTest = 1 << 4,
    IsSendOutside = 1 << 5,
    IsOutsourceable = 1 << 6,
}

public partial class TestType
{
    public int TesttypeId { get; set; }

    public int GroupId { get; set; }

    [Required(ErrorMessage = "كود التحليل مطلوب")]
    [StringLength(30)]
    public string TypeCode { get; set; } = null!;

    [Required(ErrorMessage = "اسم التحليل بالانجليزية مطلوب")]
    [StringLength(200)]
    public string TypeNameEn { get; set; } = null!;

    [StringLength(200)]
    public string? TypeNameAr { get; set; }

    [StringLength(20)]
    public string? TypeAbbrev { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون 0 أو أكثر")]
    public decimal DefaultPrice { get; set; }

    public string? SampleType { get; set; }

    public string? DefaultTubeType { get; set; }

    public string? DefaultTubeColor { get; set; }

    public short TurnaroundHours { get; set; }

    public string? SpecialType { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public string? ReportNameLine1 { get; set; }

    public string? ReportNameLine2 { get; set; }

    public string? BillNameLine1 { get; set; }

    public string? BillNameLine2 { get; set; }

    public string? HistoryName { get; set; }

    public string? CollectionNotes { get; set; }

    public int? CollectionTypeId { get; set; }

    public string? OutsideLabName { get; set; }

    public decimal? OutsideCostPrice { get; set; }

    public string? PatientQuestion { get; set; }

    public string? ReferenceType { get; set; }

    public string? BarcodeName { get; set; }

    public TestTypeBehavior Behavior { get; set; } = TestTypeBehavior.None;

    public bool IsOutsourceable
    {
        get => Behavior.HasFlag(TestTypeBehavior.IsOutsourceable);
        set
        {
            if (value) Behavior |= TestTypeBehavior.IsOutsourceable;
            else Behavior &= ~TestTypeBehavior.IsOutsourceable;
        }
    }

    public bool IsRoutineTest
    {
        get => Behavior.HasFlag(TestTypeBehavior.IsRoutineTest);
        set
        {
            if (value) Behavior |= TestTypeBehavior.IsRoutineTest;
            else Behavior &= ~TestTypeBehavior.IsRoutineTest;
        }
    }

    public bool SeeReport
    {
        get => Behavior.HasFlag(TestTypeBehavior.SeeReport);
        set
        {
            if (value) Behavior |= TestTypeBehavior.SeeReport;
            else Behavior &= ~TestTypeBehavior.SeeReport;
        }
    }

    public bool PrintWithOther
    {
        get => Behavior.HasFlag(TestTypeBehavior.PrintWithOther);
        set
        {
            if (value) Behavior |= TestTypeBehavior.PrintWithOther;
            else Behavior &= ~TestTypeBehavior.PrintWithOther;
        }
    }

    public bool AddWithGroup
    {
        get => Behavior.HasFlag(TestTypeBehavior.AddWithGroup);
        set
        {
            if (value) Behavior |= TestTypeBehavior.AddWithGroup;
            else Behavior &= ~TestTypeBehavior.AddWithGroup;
        }
    }

    public bool IsMainTest
    {
        get => Behavior.HasFlag(TestTypeBehavior.IsMainTest);
        set
        {
            if (value) Behavior |= TestTypeBehavior.IsMainTest;
            else Behavior &= ~TestTypeBehavior.IsMainTest;
        }
    }

    public bool IsSendOutside
    {
        get => Behavior.HasFlag(TestTypeBehavior.IsSendOutside);
        set
        {
            if (value) Behavior |= TestTypeBehavior.IsSendOutside;
            else Behavior &= ~TestTypeBehavior.IsSendOutside;
        }
    }

    public virtual TestGroup Group { get; set; } = null!;

    public virtual CollectionType? CollectionType { get; set; }

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestComponent> TestComponents { get; set; } = new List<TestComponent>();

    public virtual ICollection<TestTypePrice> TestTypePrices { get; set; } = new List<TestTypePrice>();

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();

    public virtual ICollection<TestProfileItem> TestProfileItems { get; set; } = new List<TestProfileItem>();

    public virtual ICollection<TestTypeSampleTube> TestTypeSampleTubes { get; set; } = new List<TestTypeSampleTube>();
}

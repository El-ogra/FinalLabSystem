using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestType
{
    public int TesttypeId { get; set; }

    public int GroupId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeNameEn { get; set; } = null!;

    public string? TypeNameAr { get; set; }

    public string? TypeAbbrev { get; set; }

    public double DefaultPrice { get; set; }

    public string? SampleType { get; set; }

    public string? DefaultTubeType { get; set; }

    public string? DefaultTubeColor { get; set; }

    public short TurnaroundHours { get; set; }

    public bool IsOutsourceable { get; set; }

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

    public bool IsRoutineTest { get; set; }

    public bool SeeReport { get; set; }

    public bool PrintWithOther { get; set; }

    public bool AddWithGroup { get; set; }

    public bool IsMainTest { get; set; }

    public bool IsSendOutside { get; set; }

    public string? OutsideLabName { get; set; }

    public decimal? OutsideCostPrice { get; set; }

    public string? PatientQuestion { get; set; }

    public string? ReferenceType { get; set; }

    public string? BarcodeName { get; set; }

    public virtual TestGroup Group { get; set; } = null!;

    public virtual CollectionType? CollectionType { get; set; }

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestComponent> TestComponents { get; set; } = new List<TestComponent>();

    public virtual ICollection<TestTypePrice> TestTypePrices { get; set; } = new List<TestTypePrice>();

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();

    public virtual ICollection<TestProfileItem> TestProfileItems { get; set; } = new List<TestProfileItem>();

    public virtual ICollection<TestTypeSampleTube> TestTypeSampleTubes { get; set; } = new List<TestTypeSampleTube>();
}

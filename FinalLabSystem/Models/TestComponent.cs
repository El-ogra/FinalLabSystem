using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestComponent
{
    public int ComponentId { get; set; }

    public int TesttypeId { get; set; }

    public string ComponentCode { get; set; } = null!;

    public string ComponentNameEn { get; set; } = null!;

    public string? ComponentNameAr { get; set; }

    public string? Unit { get; set; }

    public string ResultType { get; set; } = null!;

    public byte DecimalPlaces { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<NormalRange> NormalRanges { get; set; } = new List<NormalRange>();

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    public virtual TestType Testtype { get; set; } = null!;
}

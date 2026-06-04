using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestCategory
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryNameEn { get; set; } = null!;

    public string? CategoryNameAr { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestGroup> TestGroups { get; set; } = new List<TestGroup>();
}

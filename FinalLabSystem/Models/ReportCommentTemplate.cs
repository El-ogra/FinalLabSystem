using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class ReportCommentTemplate
{
    public int TemplateId { get; set; }

    public int? CategoryId { get; set; }

    public int? TesttypeId { get; set; }

    public int? ComponentId { get; set; }

    public string Title { get; set; } = null!;

    public string CommentText { get; set; } = null!;

    public string CommentLang { get; set; } = null!;

    public string? TriggerCondition { get; set; }

    public bool IsActive { get; set; }

    public short SortOrder { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual TestCategory? Category { get; set; }

    public virtual TestComponent? Component { get; set; }

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual Staff? ModifiedByNavigation { get; set; }

    public virtual TestType? Testtype { get; set; }
}

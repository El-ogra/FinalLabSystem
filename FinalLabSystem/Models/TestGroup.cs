using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestGroup
{
    public int GroupId { get; set; }

    public int CategoryId { get; set; }

    public string GroupCode { get; set; } = null!;

    public string GroupNameEn { get; set; } = null!;

    public string? GroupNameAr { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual TestCategory Category { get; set; } = null!;

    public virtual ICollection<TestType> TestTypes { get; set; } = new List<TestType>();
}

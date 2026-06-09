using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class CollectionType
{
    public int CollectionTypeId { get; set; }

    public string TypeNameEn { get; set; } = null!;

    public string? TypeNameAr { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TestType> TestTypes { get; set; } = new List<TestType>();
}

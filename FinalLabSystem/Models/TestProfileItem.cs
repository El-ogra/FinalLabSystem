using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Test Profile Item - Maps individual tests to test profiles/packages
/// V4.0 New Table
/// </summary>
public partial class TestProfileItem
{
    public int ProfileItemId { get; set; }

    public int ProfileId { get; set; }

    public int TestTypeId { get; set; }

    public int? SortOrder { get; set; }

    // Navigation properties
    public virtual TestProfile Profile { get; set; } = null!;

    public virtual TestType TestType { get; set; } = null!;
}

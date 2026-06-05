using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestTypeSampleTube
{
    public int TestTypeTubeId { get; set; }

    public int TestTypeId { get; set; }

    public string TubeType { get; set; } = null!;

    public string? TubeColor { get; set; }

    public string? SampleType { get; set; }

    public int Quantity { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public virtual TestType Testtype { get; set; } = null!;
}

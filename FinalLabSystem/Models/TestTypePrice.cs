using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestTypePrice
{
    public int PriceId { get; set; }

    public int SchemeId { get; set; }

    public int TesttypeId { get; set; }

    public decimal Price { get; set; }

    public virtual PriceScheme Scheme { get; set; } = null!;

    public virtual TestType Testtype { get; set; } = null!;
}

namespace FinalLabSystem.Models.DTOs;

public sealed class TestPricingResultDto
{
    public int TestTypeId { get; set; }

    public decimal Price { get; set; }

    public bool IsFallback { get; set; }
}

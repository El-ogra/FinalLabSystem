namespace FinalLabSystem.Models.DTOs;

public sealed class SelectedTestDto
{
    public int TestTypeId { get; set; }

    public string TestCode { get; set; } = string.Empty;

    public string TestName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? SampleType { get; set; }
}

using System;

namespace FinalLabSystem.Models.DTOs;

public sealed class CashDrawerFilterDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? StaffId { get; set; }
}

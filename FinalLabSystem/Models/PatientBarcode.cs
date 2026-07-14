using System;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

public class PatientBarcode
{
    public int PatientBarcodeId { get; set; }
    public int PatientId { get; set; }
    public int? VisitId { get; set; }
    public BarcodeCodeType CodeType { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public int SortOrdinal { get; set; }
    public int? CreatedBy { get; set; }

    public Patient Patient { get; set; } = null!;
    public Visit? Visit { get; set; }
    public Staff? Staff { get; set; }
}

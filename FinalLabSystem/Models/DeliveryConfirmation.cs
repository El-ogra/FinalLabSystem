using System;
using System.ComponentModel.DataAnnotations;
using FinalLabSystem.Data;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

[Auditable]
public partial class DeliveryConfirmation
{
    public int DeliveryConfirmationId { get; set; }

    public int VisitId { get; set; }

    public DeliveryConfirmationMethod Method { get; set; }

    public DateTime ConfirmedAt { get; set; }

    public byte[]? SignatureImage { get; set; }

    [StringLength(500)]
    public string? OtpCodeHash { get; set; }

    [Required]
    [StringLength(100)]
    public string ReceivedByName { get; set; } = string.Empty;

    public int StaffId { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}

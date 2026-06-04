using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string CompanyType { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public int? SchemeId { get; set; }

    public double DiscountRate { get; set; }

    public double CreditLimit { get; set; }

    public string? PaymentTerms { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PriceScheme? Scheme { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public virtual ICollection<ContractInvoice> ContractInvoices { get; set; } = new List<ContractInvoice>();
}

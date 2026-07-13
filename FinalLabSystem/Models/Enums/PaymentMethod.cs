namespace FinalLabSystem.Models.Enums;

/// <summary>
/// وسيلة سداد سطر الدفع (Payment) — قيم صرفة لوسائل السداد فقط.
/// [القرار 12] أُزيلت Contract (نُقلت لـ BillingType) وأُضيفت Card و Check.
/// </summary>
public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    Check = 2,
    Insurance = 3,
    Other = 4
}

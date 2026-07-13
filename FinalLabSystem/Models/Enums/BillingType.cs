namespace FinalLabSystem.Models.Enums;

/// <summary>
/// نوع الفوترة على مستوى الزيارة (Visit) — يحدد مصدر التسعير الأساسي.
/// [القرار 12] فُصل عن PaymentMethod و ReferringEntityCategory.
/// </summary>
public enum BillingType
{
    Individual = 0,
    LabToLab = 1,
    Free = 2
}

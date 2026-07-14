namespace FinalLabSystem.Models.Enums;

/// <summary>
/// الحالة البصرية المستنتجة من الأعلام الخمسة للزيارة (القرار 11).
/// </summary>
public enum VisitDisplayStatus
{
    NewNoResults = 0,         // S1 🔴
    ResultsNotWritten = 1,    // S2 📝 (Partial entry)
    ResultsNotReviewed = 2,   // S3 ↔️ (Fully entered, not reviewed)
    ResultsNotPrinted = 3,    // S4 🖨️
    NotDelivered = 4,         // S5 🛒
    DeliveredWithBalance = 5, // S6 £
    FullyComplete = 6         // S7 🎖️
}

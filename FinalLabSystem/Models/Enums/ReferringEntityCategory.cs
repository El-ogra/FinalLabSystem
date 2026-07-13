namespace FinalLabSystem.Models.Enums;

/// <summary>
/// تصنيف الجهة المُحوِّلة (ReferralSource) — يستخدم لتقارير العمولات والتحليل.
/// [القرار 12] فُصل كمحور مستقل عن SourceType الحر.
/// </summary>
public enum ReferringEntityCategory
{
    ReferringDoctor = 0,
    OutsourcedSample = 1,
    ReferralOrContractEntity = 2
}

namespace FinalLabSystem.Models.Enums;

/// <summary>
/// مستويات حساسية المضادات الحيوية (VS-04 / القرار 22).
/// تسمية القيم بلاحقة "For" مطابقة حرفياً لتسميات المرجع الأصلي
/// (Real Lab System) لضمان مطابقة نصية للتقارير الطبية.
/// القيم الرقمية (0..3) لم تتغير عن الإصدار السابق (Highly=0, Moderate=1,
/// Low=2, Resistant=3) — لا حاجة لترحيل بيانات فعلي، إعادة تسمية على مستوى الكود فقط.
/// </summary>
public enum AntibioticSensitivity : byte
{
    HighlyFor = 0,
    ModerateFor = 1,
    LowFor = 2,
    ResistantFor = 3
}

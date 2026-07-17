using System.Text;
using System.Text.RegularExpressions;

namespace FinalLabSystem.Infrastructure.Text;

/// <summary>
/// يُطبّع النص العربي بإزالة الهمزات وتحويل التاء المربوطة إلى هاء وتحويل الألف المقصورة إلى ياء.
/// </summary>
public static partial class ArabicTextNormalizer
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var result = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            switch (c)
            {
                case '\u0623': // أ
                case '\u0621': // إ
                case '\u0622': // آ
                    result.Append('\u0627'); // ا
                    break;
                case '\u0624': // ؤ
                    result.Append('\u0648'); // و
                    break;
                case '\u0626': // ئ
                    result.Append('\u064A'); // ي
                    break;
                case '\u0629': // ة
                    result.Append('\u0647'); // ه
                    break;
                case '\u0649': // ى
                    result.Append('\u064A'); // ي
                    break;
                default:
                    result.Append(c);
                    break;
            }
        }

        var normalized = result.ToString().Trim();
        return MultipleSpacesRegex().Replace(normalized, " ");
    }

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultipleSpacesRegex();
}

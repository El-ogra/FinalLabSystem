using ZXing;

namespace FinalLabSystem.Infrastructure.Barcoding;

internal static class BarcodeFormatOptions
{
    public const int DefaultWidth = 200;
    public const int DefaultHeight = 60;
    public const int DefaultMargin = 2;
    public static readonly BarcodeFormat DefaultFormat = BarcodeFormat.CODE_128;
}

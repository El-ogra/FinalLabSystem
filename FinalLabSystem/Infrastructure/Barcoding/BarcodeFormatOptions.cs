using ZXing;

namespace FinalLabSystem.Infrastructure.Barcoding;

internal static class BarcodeFormatOptions
{
    public const int DefaultWidth = 200;
    public const int DefaultHeight = 60;
    public const int DefaultMargin = 2;
    public static readonly BarcodeFormat DefaultFormat = BarcodeFormat.CODE_128;

    public const double LabelWidthMm = 38.0;
    public const double LabelHeightMm = 25.0;
    public const double DotsPerInch = 96.0;
    public const double MmPerInch = 25.4;
}

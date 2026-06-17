using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace FinalLabSystem.Infrastructure.Barcoding;

[ValueConversion(typeof(string), typeof(BitmapSource))]
public sealed class BarcodeImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return null!;

        var writer = new BarcodeWriter<BitMatrix>
        {
            Format = BarcodeFormatOptions.DefaultFormat,
            Options = new EncodingOptions
            {
                Width = BarcodeFormatOptions.DefaultWidth,
                Height = BarcodeFormatOptions.DefaultHeight,
                Margin = BarcodeFormatOptions.DefaultMargin,
                PureBarcode = true
            },
            Renderer = new BitMatrixRenderer()
        };

        return RenderBitMatrix(writer.Write(text));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static WriteableBitmap RenderBitMatrix(BitMatrix matrix)
    {
        var width = matrix.Width;
        var height = matrix.Height;
        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.BlackWhite, null);
        var pixels = new byte[width * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                pixels[y * width + x] = matrix[x, y] ? (byte)0 : (byte)255;
            }
        }

        bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width, 0);
        return bitmap;
    }

    private sealed class BitMatrixRenderer : IBarcodeRenderer<BitMatrix>
    {
        public BitMatrix Render(BitMatrix matrix, BarcodeFormat format, string content)
        {
            return matrix;
        }

        public BitMatrix Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
        {
            return matrix;
        }
    }
}

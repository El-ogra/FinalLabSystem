using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FinalLabSystem.Infrastructure.Barcoding;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.Services.Interfaces;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace FinalLabSystem.Services.Implementations;

public sealed class WpfLabelPrintService : ILabelPrintService
{
    private static readonly double PageWidth = BarcodeFormatOptions.LabelWidthMm * BarcodeFormatOptions.DotsPerInch / BarcodeFormatOptions.MmPerInch;
    private static readonly double PageHeight = BarcodeFormatOptions.LabelHeightMm * BarcodeFormatOptions.DotsPerInch / BarcodeFormatOptions.MmPerInch;
    private static readonly int BarcodeWidth = (int)(PageWidth - 14);
    private const int BarcodeHeight = 28;

    /// <summary>
    /// Set by a future settings module to skip the dialog and send directly to a configured printer.
    /// When null (default), the standard PrintDialog is shown so the user can pick a printer.
    /// </summary>
    public string? PreferredPrinterName { get; set; }

    public Task PrintLabelsAsync(IEnumerable<BarcodeLabel> labels)
    {
        var list = labels.ToList();
        if (list.Count == 0)
            return Task.CompletedTask;

        var document = BuildFixedDocument(list);

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
            printDialog.PrintDocument(document.DocumentPaginator, "Barcode Labels");

        return Task.CompletedTask;
    }

    private static FixedDocument BuildFixedDocument(List<BarcodeLabel> labels)
    {
        var doc = new FixedDocument();
        doc.DocumentPaginator.PageSize = new Size(PageWidth, PageHeight);

        foreach (var label in labels)
        {
            var page = new FixedPage { Width = PageWidth, Height = PageHeight };
            page.Children.Add(BuildLabelGrid(label));

            var pageContent = new PageContent();
            pageContent.Child = page;
            doc.Pages.Add(pageContent);
        }

        return doc;
    }

    private static Grid BuildLabelGrid(BarcodeLabel label)
    {
        var grid = new Grid
        {
            Width = PageWidth,
            Height = PageHeight,
            Margin = new Thickness(2)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var nameText = new TextBlock
        {
            Text = label.PatientNameAr,
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Right,
            FlowDirection = FlowDirection.RightToLeft
        };
        Grid.SetRow(nameText, 0);
        grid.Children.Add(nameText);

        var sexAgeText = new TextBlock
        {
            Text = label.SexAgeLine,
            FontSize = 7
        };
        Grid.SetRow(sexAgeText, 1);
        grid.Children.Add(sexAgeText);

        var codesText = new TextBlock
        {
            Text = label.TestCodesLine,
            FontSize = 6
        };
        Grid.SetRow(codesText, 2);
        grid.Children.Add(codesText);

        var barcodeImage = new Image
        {
            Source = GenerateBarcodeImage(label.BarcodePayload),
            HorizontalAlignment = HorizontalAlignment.Center,
            Stretch = Stretch.None,
            Margin = new Thickness(0, 1, 0, 1)
        };
        Grid.SetRow(barcodeImage, 3);
        grid.Children.Add(barcodeImage);

        var footerText = new TextBlock
        {
            FontSize = 6
        };
        footerText.Inlines.Add(new Run(label.PatientIdentifierLine));
        footerText.Inlines.Add(new Run("   "));
        footerText.Inlines.Add(new Run(label.TubeName));
        Grid.SetRow(footerText, 4);
        grid.Children.Add(footerText);

        return grid;
    }

    private static BitmapSource GenerateBarcodeImage(string payload)
    {
        var writer = new BarcodeWriter<BitMatrix>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = BarcodeWidth,
                Height = BarcodeHeight,
                Margin = 1,
                PureBarcode = true
            },
            Renderer = new BitMatrixRenderer()
        };

        return RenderBitMatrix(writer.Write(payload));
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

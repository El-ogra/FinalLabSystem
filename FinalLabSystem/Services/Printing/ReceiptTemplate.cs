using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class ReceiptTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("إيصال استلام"));
        doc.Blocks.Add(new Paragraph(new Run("جاري إنشاء الإيصال...")));
        return doc;
    }

    internal override void ApplyLayout(FlowDocument document, ReportLayoutDto? layout)
    {
        if (layout is null || document is null) return;

        // Typography
        if (!string.IsNullOrWhiteSpace(layout.FontFamily))
            document.FontFamily = new FontFamily(layout.FontFamily);

        if (layout.FontSize > 0)
            document.FontSize = layout.FontSize;

        // Margins (cm → WPF points: 1 cm ≈ 28.35 points)
        document.PagePadding = new System.Windows.Thickness(
            (double)(layout.MarginLeft * 28.35m),
            (double)(layout.MarginTop * 28.35m),
            (double)(layout.MarginRight * 28.35m),
            (double)(layout.MarginBottom * 28.35m));

        // Colors on header paragraphs
        var primaryBrush = !string.IsNullOrWhiteSpace(layout.PrimaryColor)
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(layout.PrimaryColor))
            : null;

        if (primaryBrush is not null)
        {
            foreach (var block in document.Blocks)
            {
                if (block is Paragraph para)
                    para.Foreground = primaryBrush;
            }
        }
    }
}

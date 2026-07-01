using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class ResultReportTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("تقرير نتائج"));
        doc.Blocks.Add(new Paragraph(new Run("جاري إنشاء التقرير...")));
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

        // Page margins
        document.PagePadding = new System.Windows.Thickness(
            (double)(layout.MarginLeft * 28.35m),
            (double)(layout.MarginTop * 28.35m),
            (double)(layout.MarginRight * 28.35m),
            (double)(layout.MarginBottom * 28.35m));

        // Primary color on all paragraphs
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

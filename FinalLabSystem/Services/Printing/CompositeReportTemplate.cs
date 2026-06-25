using System.Windows.Documents;

namespace FinalLabSystem.Services.Printing;

public class CompositeReportTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("تقرير مجمع"));
        doc.Blocks.Add(new Paragraph(new Run("تقرير مجمع - جاري إنشاء التقرير...")));
        return doc;
    }
}
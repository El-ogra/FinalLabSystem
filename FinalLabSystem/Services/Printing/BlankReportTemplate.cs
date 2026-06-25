using System.Windows.Documents;

namespace FinalLabSystem.Services.Printing;

public class BlankReportTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("تقرير فارغ"));
        doc.Blocks.Add(new Paragraph(new Run("تقرير فارغ - جاري إنشاء النموذج...")));
        return doc;
    }
}
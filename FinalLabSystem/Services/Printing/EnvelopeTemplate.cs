using System.Windows.Documents;

namespace FinalLabSystem.Services.Printing;

public class EnvelopeTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("ظرف المريض"));
        doc.Blocks.Add(new Paragraph(new Run("ظرف المريض - جاري إنشاء الظرف...")));
        return doc;
    }
}
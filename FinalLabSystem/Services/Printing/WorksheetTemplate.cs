using System.Windows.Documents;

namespace FinalLabSystem.Services.Printing;

public class WorksheetTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("أمر شغل"));
        doc.Blocks.Add(new Paragraph(new Run("أمر شغل - جاري إنشاء أمر الشغل...")));
        return doc;
    }
}
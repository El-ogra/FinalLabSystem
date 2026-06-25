using System.Windows.Documents;

namespace FinalLabSystem.Services.Printing;

public class ReceiptTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("إيصال استلام"));
        doc.Blocks.Add(new Paragraph(new Run("جاري إنشاء الإيصال...")));
        return doc;
    }
}

using System.Windows.Documents;

namespace FinalLabSystem.Services.Printing;

public class MedicalHistoryTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("السجل المرضي"));
        doc.Blocks.Add(new Paragraph(new Run("السجل المرضي - جاري إنشاء المقارنة التاريخية...")));
        return doc;
    }
}
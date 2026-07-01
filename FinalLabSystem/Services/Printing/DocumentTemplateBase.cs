using System.Windows.Documents;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public abstract class DocumentTemplateBase
{
    public abstract FlowDocument BuildDocument(object data);

    protected virtual Paragraph CreateHeader(string title)
    {
        return new Paragraph(new Run(title))
        {
            FontSize = 16,
            FontWeight = System.Windows.FontWeights.Bold,
            TextAlignment = System.Windows.TextAlignment.Center
        };
    }

    protected virtual Paragraph CreateFooter(string text)
    {
        return new Paragraph(new Run(text))
        {
            FontSize = 10,
            TextAlignment = System.Windows.TextAlignment.Center,
            Margin = new System.Windows.Thickness(0, 20, 0, 0)
        };
    }

    /// <summary>
    /// يُستدعى بعد BuildDocument وليس قبله.
    /// الـ default: no-op (لا يُغيّر المستند).
    /// الـ overrides تُعدّل الـ FlowDocument المُمرَّر مباشرة.
    /// </summary>
    internal virtual void ApplyLayout(FlowDocument document, ReportLayoutDto? layout)
    {
        // Default no-op: لا يُغيّر سلوك أي template قائمة.
    }
}

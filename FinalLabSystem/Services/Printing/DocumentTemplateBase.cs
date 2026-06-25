using System.Windows.Documents;

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
}

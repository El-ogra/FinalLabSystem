using System.Windows;
using System.Windows.Documents;

namespace FinalLabSystem.Services.Interfaces;

public interface IPrintPreviewDialogService
{
    void Show(FlowDocument document, string description, Window? owner = null);
}

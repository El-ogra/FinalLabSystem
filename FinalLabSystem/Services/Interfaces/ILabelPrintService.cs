using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Interfaces;

public interface ILabelPrintService
{
    Task PrintLabelsAsync(IEnumerable<BarcodeLabel> labels);
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models.DTOs;

public sealed class VisitTestItemDto : INotifyPropertyChanged
{
    public int VisitTestId { get; set; }

    public string TestTypeName { get; set; } = string.Empty;

    public string TestTypeCode { get; set; } = string.Empty;

    public string? SpecialType { get; set; }

    public TestStage CurrentStage { get; set; }

    public bool IsOutsourced { get; set; }

    public string? ExternalLabName { get; set; }

    public int TotalComponents { get; set; }

    public int EnteredCount { get; set; }

    public int ReviewedCount { get; set; }

    public ResultValidationStatus OverallValidationStatus { get; set; }

    public bool IsManuallyOverridden { get; set; }

    public bool IsPrinted { get; set; }

    public bool IsExported { get; set; }

    public bool IsSingleComponent => TotalComponents == 1;

    public bool IsMultiComponent => TotalComponents > 1;

    public bool IsAllReviewed => TotalComponents > 0 &&
        ComponentResults.Count >= TotalComponents &&
        ComponentResults.All(c => c.ValidationStatus >= ResultValidationStatus.Reviewed);

    public string SingleComponentResultValue
    {
        get => ComponentResults.Count == 1
            ? ComponentResults[0].ResultValue ?? string.Empty
            : string.Empty;
        set
        {
            if (ComponentResults.Count == 1 && IsSingleComponent)
            {
                ComponentResults[0].ResultValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleComponentResultValue)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleComponentStatus)));
            }
        }
    }

    public void SetSingleComponentResultValue(string value)
    {
        if (ComponentResults.Count == 1)
        {
            ComponentResults[0].ResultValue = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleComponentResultValue)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleComponentStatus)));
        }
    }

    public string SingleComponentStatus
    {
        get => ComponentResults.Count == 1 ? ComponentResults[0].ResultStatus ?? string.Empty : string.Empty;
    }

    public ObservableCollection<TestComponentResultDto> ComponentResults { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyResultChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleComponentResultValue)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleComponentStatus)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAllReviewed)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPrinted)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExported)));
    }
}

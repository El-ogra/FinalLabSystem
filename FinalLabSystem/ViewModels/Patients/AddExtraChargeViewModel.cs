using System.Collections.Generic;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class AddExtraChargeViewModel : ViewModelBase
{
    private string _chargeDescription = "";
    private decimal _amount;
    private string _selectedChargeType = "HomeCollection";
    private bool _isCustomDescription;

    public AddExtraChargeViewModel()
    {
        SaveCommand = new RelayCommand(_ => Save(), _ => Amount > 0);
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public List<ChargeTypeOption> ChargeTypes { get; } = new()
    {
        new("HomeCollection", "خدمة السحب المنزلي"),
        new("RushFee", "رسوم الاستعجال"),
        new("Other", "أخرى")
    };

    public string SelectedChargeType
    {
        get => _selectedChargeType;
        set
        {
            if (SetProperty(ref _selectedChargeType, value))
            {
                IsCustomDescription = value == "Other";
                if (!IsCustomDescription)
                {
                    var match = ChargeTypes.Find(ct => ct.Key == value);
                    if (match is not null)
                        ChargeDescription = match.DisplayName;
                }
            }
        }
    }

    public string ChargeDescription
    {
        get => _chargeDescription;
        set => SetProperty(ref _chargeDescription, value);
    }

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, Math.Max(0, value)))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsCustomDescription
    {
        get => _isCustomDescription;
        private set
        {
            if (SetProperty(ref _isCustomDescription, value))
                OnPropertyChanged(nameof(IsDescriptionReadOnly));
        }
    }

    public bool IsDescriptionReadOnly => !IsCustomDescription;

    public VisitCharge? Result { get; private set; }

    public bool? DialogResult { get; private set; }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    private void Save()
    {
        if (Amount <= 0) return;
        Result = new VisitCharge
        {
            ChargeDescription = ChargeDescription,
            Amount = Amount,
            ChargeType = SelectedChargeType
        };
        DialogResult = true;
    }

    private void Cancel()
    {
        DialogResult = false;
    }

    public record ChargeTypeOption(string Key, string DisplayName);
}

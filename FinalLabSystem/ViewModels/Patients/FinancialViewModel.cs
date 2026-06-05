using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class FinancialViewModel : ViewModelBase
{
    private bool _isUpdating;
    private decimal _subtotal;
    private decimal _discountAmount;
    private decimal _discountPercent;
    private decimal _totalAfterDiscount;
    private decimal _amountPaid;
    private decimal _balanceDue;
    private decimal _previouslyPaid;
    private bool _isPaymentConfirmed;

    public FinancialViewModel()
    {
        ConfirmPaymentCommand = new RelayCommand(_ => IsPaymentConfirmed = true);
        RevertCommand = new RelayCommand(_ => Revert(), _ => !IsPaymentConfirmed);
    }

    public decimal Subtotal
    {
        get => _subtotal;
        private set
        {
            if (SetProperty(ref _subtotal, value))
                RecalculateTotals();
        }
    }

    public decimal DiscountAmount
    {
        get => _discountAmount;
        set
        {
            WarnIfConfirmed();
            if (SetProperty(ref _discountAmount, Math.Clamp(value, 0, Subtotal)) && !_isUpdating)
            {
                _isUpdating = true;
                DiscountPercent = Subtotal <= 0 ? 0 : Math.Round(DiscountAmount / Subtotal * 100, 2);
                _isUpdating = false;
                RecalculateTotals();
            }
        }
    }

    public decimal DiscountPercent
    {
        get => _discountPercent;
        set
        {
            WarnIfConfirmed();
            if (SetProperty(ref _discountPercent, Math.Clamp(value, 0, 100)) && !_isUpdating)
            {
                _isUpdating = true;
                DiscountAmount = Math.Round(Subtotal * DiscountPercent / 100, 2);
                _isUpdating = false;
                RecalculateTotals();
            }
        }
    }

    public decimal TotalAfterDiscount
    {
        get => _totalAfterDiscount;
        private set => SetProperty(ref _totalAfterDiscount, value);
    }

    public decimal AmountPaid
    {
        get => _amountPaid;
        set
        {
            WarnIfConfirmed();
            if (SetProperty(ref _amountPaid, Math.Max(0, value)))
                RecalculateTotals();
        }
    }

    public decimal BalanceDue
    {
        get => _balanceDue;
        private set => SetProperty(ref _balanceDue, value);
    }

    public decimal PreviouslyPaid
    {
        get => _previouslyPaid;
        set => SetProperty(ref _previouslyPaid, Math.Max(0, value));
    }

    public bool IsPaymentConfirmed
    {
        get => _isPaymentConfirmed;
        set => SetProperty(ref _isPaymentConfirmed, value);
    }

    public ICommand ConfirmPaymentCommand { get; }

    public ICommand RevertCommand { get; }

    public void RecalculateFromTests(List<decimal> prices)
    {
        WarnIfConfirmed();
        Subtotal = prices.Sum();
        RecalculateTotals();
    }

    public void LoadFinancials(decimal subtotal, decimal discountAmount, decimal amountPaid, decimal previouslyPaid)
    {
        _subtotal = subtotal;
        OnPropertyChanged(nameof(Subtotal));
        _discountAmount = discountAmount;
        _discountPercent = subtotal <= 0 ? 0 : Math.Round(discountAmount / subtotal * 100, 2);
        _amountPaid = amountPaid;
        _previouslyPaid = previouslyPaid;
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(DiscountPercent));
        OnPropertyChanged(nameof(AmountPaid));
        OnPropertyChanged(nameof(PreviouslyPaid));
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalAfterDiscount = Math.Max(0, Subtotal - DiscountAmount);
        BalanceDue = TotalAfterDiscount - AmountPaid;
    }

    private void Revert()
    {
        if (IsPaymentConfirmed)
            return;

        DiscountAmount = 0;
        AmountPaid = 0;
        RecalculateTotals();
    }

    private void WarnIfConfirmed()
    {
        if (!IsPaymentConfirmed)
            return;

        MessageBox.Show("تم تعديل الحسابات بعد تأكيد الدفع، سيتم إلغاء التأكيد.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        IsPaymentConfirmed = false;
    }
}

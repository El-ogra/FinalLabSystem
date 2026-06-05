using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class FinancialViewModel : ViewModelBase
{
    private readonly IFinancialService _financialService;
    private bool _isUpdating;
    private int _currentVisitId;
    private decimal _subtotal;
    private decimal _discountAmount;
    private decimal _discountPercent;
    private decimal _totalAfterDiscount;
    private decimal _amountPaid;
    private decimal _balanceDue;
    private decimal _previouslyPaid;
    private bool _isPaymentConfirmed;
    private bool _isClearanceRequested;
    private string _paymentStatus = "PENDING";

    public FinancialViewModel(IFinancialService financialService)
    {
        _financialService = financialService;
        ConfirmPaymentCommand = new AsyncRelayCommand(ConfirmPaymentAsync);
        RevertCommand = new AsyncRelayCommand(RevertAsync, () => !IsPaymentConfirmed);
        ClearanceCommand = new RelayCommand(_ => RequestClearance());
    }

    public int CurrentVisitId
    {
        get => _currentVisitId;
        private set => SetProperty(ref _currentVisitId, value);
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
        set
        {
            if (SetProperty(ref _isPaymentConfirmed, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool IsClearanceRequested
    {
        get => _isClearanceRequested;
        private set => SetProperty(ref _isClearanceRequested, value);
    }

    public string PaymentStatus
    {
        get => _paymentStatus;
        set => SetProperty(ref _paymentStatus, string.IsNullOrWhiteSpace(value) ? "PENDING" : value);
    }

    public ICommand ConfirmPaymentCommand { get; }

    public ICommand RevertCommand { get; }

    public ICommand ClearanceCommand { get; }

    public void SetCurrentVisitId(int visitId)
    {
        CurrentVisitId = visitId;
    }

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
        IsClearanceRequested = false;
        PaymentStatus = amountPaid >= Math.Max(0, subtotal - discountAmount) && subtotal > 0 ? "PAID" : amountPaid > 0 ? "PARTIAL" : "PENDING";
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(DiscountPercent));
        OnPropertyChanged(nameof(AmountPaid));
        OnPropertyChanged(nameof(PreviouslyPaid));
        RecalculateTotals();
    }

    public void LoadFromDto(VisitFullDto dto)
    {
        CurrentVisitId = dto.VisitId;
        _subtotal = dto.Subtotal;
        _discountAmount = dto.DiscountAmount;
        _discountPercent = dto.DiscountPercent;
        _totalAfterDiscount = dto.TotalAfterDiscount;
        _amountPaid = dto.TotalPaid;
        _previouslyPaid = dto.TotalPaid;
        _balanceDue = dto.BalanceDue;
        _paymentStatus = dto.PaymentStatus;
        _isPaymentConfirmed = string.Equals(dto.PaymentStatus, "PAID", StringComparison.OrdinalIgnoreCase);
        _isClearanceRequested = false;
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(DiscountPercent));
        OnPropertyChanged(nameof(TotalAfterDiscount));
        OnPropertyChanged(nameof(AmountPaid));
        OnPropertyChanged(nameof(PreviouslyPaid));
        OnPropertyChanged(nameof(BalanceDue));
        OnPropertyChanged(nameof(PaymentStatus));
        OnPropertyChanged(nameof(IsPaymentConfirmed));
        OnPropertyChanged(nameof(IsClearanceRequested));
        CommandManager.InvalidateRequerySuggested();
    }

    public void ClearAllFields()
    {
        CurrentVisitId = 0;
        _subtotal = 0;
        _discountAmount = 0;
        _discountPercent = 0;
        _totalAfterDiscount = 0;
        _amountPaid = 0;
        _balanceDue = 0;
        _previouslyPaid = 0;
        _isPaymentConfirmed = false;
        _isClearanceRequested = false;
        _paymentStatus = "PENDING";
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(DiscountPercent));
        OnPropertyChanged(nameof(TotalAfterDiscount));
        OnPropertyChanged(nameof(AmountPaid));
        OnPropertyChanged(nameof(BalanceDue));
        OnPropertyChanged(nameof(PreviouslyPaid));
        OnPropertyChanged(nameof(IsPaymentConfirmed));
        OnPropertyChanged(nameof(IsClearanceRequested));
        OnPropertyChanged(nameof(PaymentStatus));
        CommandManager.InvalidateRequerySuggested();
    }

    private async Task ConfirmPaymentAsync()
    {
        if (IsClearanceRequested)
        {
            if (CurrentVisitId <= 0)
            {
                AmountPaid = TotalAfterDiscount;
                IsPaymentConfirmed = true;
                PaymentStatus = "PAID";
                IsClearanceRequested = false;
                return;
            }

            var applied = await _financialService.ApplyClearancePaymentAsync(CurrentVisitId, BalanceDue);
            if (applied)
            {
                AmountPaid = TotalAfterDiscount;
                BalanceDue = 0;
                PreviouslyPaid = TotalAfterDiscount;
                PaymentStatus = "PAID";
                IsPaymentConfirmed = true;
                IsClearanceRequested = false;
            }

            return;
        }

        IsPaymentConfirmed = true;
    }

    private async Task RevertAsync()
    {
        if (IsPaymentConfirmed)
            return;

        if (CurrentVisitId > 0)
            await _financialService.RevertClearanceAsync(CurrentVisitId);

        DiscountAmount = 0;
        AmountPaid = 0;
        IsClearanceRequested = false;
        PaymentStatus = "PENDING";
        RecalculateTotals();
    }

    private void RequestClearance()
    {
        WarnIfConfirmed();
        AmountPaid = TotalAfterDiscount;
        IsClearanceRequested = true;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalAfterDiscount = Math.Max(0, Subtotal - DiscountAmount);
        BalanceDue = TotalAfterDiscount - AmountPaid;
    }

    private void WarnIfConfirmed()
    {
        if (!IsPaymentConfirmed)
            return;

        MessageBox.Show("تم تعديل الحسابات بعد تأكيد الدفع، سيتم إلغاء التأكيد.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        IsPaymentConfirmed = false;
    }
}

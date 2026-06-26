using System;
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class PaymentRowViewModel : ViewModelBase
{
    private int _contractPaymentId;
    private int _contractInvoiceId;
    private decimal _amount;
    private DateTime _paymentDate;
    private string _paymentMethod = string.Empty;
    private string? _referenceNumber;
    private string? _notes;

    public int ContractPaymentId
    {
        get => _contractPaymentId;
        set => SetProperty(ref _contractPaymentId, value);
    }

    public int ContractInvoiceId
    {
        get => _contractInvoiceId;
        set => SetProperty(ref _contractInvoiceId, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public DateTime PaymentDate
    {
        get => _paymentDate;
        set => SetProperty(ref _paymentDate, value);
    }

    public string PaymentMethod
    {
        get => _paymentMethod;
        set => SetProperty(ref _paymentMethod, value);
    }

    public string? ReferenceNumber
    {
        get => _referenceNumber;
        set => SetProperty(ref _referenceNumber, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }
}

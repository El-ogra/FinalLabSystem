using System;
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class InvoiceRowViewModel : ViewModelBase
{
    private int _contractInvoiceId;
    private int _companyId;
    private string _companyName = string.Empty;
    private DateTime _invoiceDate;
    private DateOnly _periodStart;
    private DateOnly _periodEnd;
    private decimal _totalAmount;
    private decimal _paidAmount;
    private string _status = string.Empty;

    public int ContractInvoiceId
    {
        get => _contractInvoiceId;
        set => SetProperty(ref _contractInvoiceId, value);
    }

    public int CompanyId
    {
        get => _companyId;
        set => SetProperty(ref _companyId, value);
    }

    public string CompanyName
    {
        get => _companyName;
        set => SetProperty(ref _companyName, value);
    }

    public DateTime InvoiceDate
    {
        get => _invoiceDate;
        set => SetProperty(ref _invoiceDate, value);
    }

    public DateOnly PeriodStart
    {
        get => _periodStart;
        set => SetProperty(ref _periodStart, value);
    }

    public DateOnly PeriodEnd
    {
        get => _periodEnd;
        set => SetProperty(ref _periodEnd, value);
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    public decimal PaidAmount
    {
        get => _paidAmount;
        set => SetProperty(ref _paidAmount, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public decimal Balance => TotalAmount - PaidAmount;
}

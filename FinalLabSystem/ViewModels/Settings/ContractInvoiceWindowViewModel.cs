using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ContractInvoiceWindowViewModel : ViewModelBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICompanyService _companyService;

    public ContractInvoiceWindowViewModel(
        IInvoiceService invoiceService,
        ICompanyService companyService)
    {
        _invoiceService = invoiceService;
        _companyService = companyService;

        Invoices = new ObservableCollection<InvoiceRowViewModel>();
        Payments = new ObservableCollection<PaymentRowViewModel>();
        Companies = new ObservableCollection<Company>();

        GenerateInvoiceCommand = new AsyncRelayCommand(GenerateInvoiceAsync, () => CanGenerateInvoice);
        RecordPaymentCommand = new AsyncRelayCommand(RecordPaymentAsync, () => CanRecordPayment);
        RefreshCommand = new AsyncRelayCommand(LoadInvoicesAsync);
    }

    public ObservableCollection<InvoiceRowViewModel> Invoices { get; }
    public ObservableCollection<PaymentRowViewModel> Payments { get; }
    public ObservableCollection<Company> Companies { get; }

    public ICommand GenerateInvoiceCommand { get; }
    public ICommand RecordPaymentCommand { get; }
    public ICommand RefreshCommand { get; }

    private Company? _selectedCompany;
    public Company? SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            SetProperty(ref _selectedCompany, value);
            OnPropertyChanged(nameof(CanGenerateInvoice));
            _ = LoadInvoicesAsync();
        }
    }

    private InvoiceRowViewModel? _selectedInvoice;
    public InvoiceRowViewModel? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            SetProperty(ref _selectedInvoice, value);
            OnPropertyChanged(nameof(CanRecordPayment));
            _ = LoadPaymentsAsync();
        }
    }

    private DateOnly _periodStart = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
    public DateOnly PeriodStart
    {
        get => _periodStart;
        set => SetProperty(ref _periodStart, value);
    }

    private DateOnly _periodEnd = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly PeriodEnd
    {
        get => _periodEnd;
        set => SetProperty(ref _periodEnd, value);
    }

    private decimal _paymentAmount;
    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set
        {
            SetProperty(ref _paymentAmount, value);
            OnPropertyChanged(nameof(CanRecordPayment));
        }
    }

    private string _paymentMethod = string.Empty;
    public string PaymentMethod
    {
        get => _paymentMethod;
        set
        {
            SetProperty(ref _paymentMethod, value);
            OnPropertyChanged(nameof(CanRecordPayment));
        }
    }

    private string? _paymentReference;
    public string? PaymentReference
    {
        get => _paymentReference;
        set => SetProperty(ref _paymentReference, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool CanGenerateInvoice => SelectedCompany is not null;
    public bool CanRecordPayment => SelectedInvoice is not null && PaymentAmount > 0 && !string.IsNullOrWhiteSpace(PaymentMethod);

    public async Task InitializeAsync()
    {
        var companies = await _companyService.GetAllAsync();
        Companies.Clear();
        foreach (var company in companies)
            Companies.Add(company);

        await LoadInvoicesAsync();
    }

    private async Task LoadInvoicesAsync()
    {
        IsLoading = true;
        try
        {
            var invoices = SelectedCompany is not null
                ? await _invoiceService.GetInvoicesAsync(SelectedCompany.CompanyId)
                : new System.Collections.Generic.List<ContractInvoice>();

            Invoices.Clear();
            foreach (var inv in invoices)
            {
                Invoices.Add(new InvoiceRowViewModel
                {
                    ContractInvoiceId = inv.ContractInvoiceId,
                    CompanyId = inv.CompanyId,
                    CompanyName = inv.Company?.CompanyName ?? "",
                    InvoiceDate = inv.InvoiceDate,
                    PeriodStart = inv.PeriodStart,
                    PeriodEnd = inv.PeriodEnd,
                    TotalAmount = inv.TotalAmount,
                    PaidAmount = inv.PaidAmount,
                    Status = inv.Status
                });
            }
            StatusMessage = $"تم تحميل {Invoices.Count} فاتورة";
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ في التحميل: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPaymentsAsync()
    {
        if (SelectedInvoice is null) return;

        try
        {
            var payments = await _invoiceService.GetPaymentsAsync(SelectedInvoice.ContractInvoiceId);
            Payments.Clear();
            foreach (var p in payments)
            {
                Payments.Add(new PaymentRowViewModel
                {
                    ContractPaymentId = p.ContractPaymentId,
                    ContractInvoiceId = p.ContractInvoiceId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ في تحميل الدفعات: {ex.Message}";
        }
    }

    private async Task GenerateInvoiceAsync()
    {
        if (SelectedCompany is null) return;

        IsLoading = true;
        try
        {
            var invoice = await _invoiceService.GenerateInvoiceAsync(
                SelectedCompany.CompanyId,
                PeriodStart.Year,
                PeriodStart.Month,
                0);

            StatusMessage = $"تم إنشاء الفاتورة رقم {invoice.ContractInvoiceId} بقيمة {invoice.TotalAmount:N2}";
            await LoadInvoicesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ في إنشاء الفاتورة: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RecordPaymentAsync()
    {
        if (SelectedInvoice is null) return;

        IsLoading = true;
        try
        {
            await _invoiceService.RecordPaymentAsync(
                SelectedInvoice.ContractInvoiceId,
                PaymentAmount,
                PaymentMethod,
                PaymentReference,
                0);

            StatusMessage = $"تم تسجيل الدفعة بقيمة {PaymentAmount:N2}";
            PaymentAmount = 0;
            PaymentMethod = string.Empty;
            PaymentReference = null;
            await LoadInvoicesAsync();
            await LoadPaymentsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ في تسجيل الدفعة: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

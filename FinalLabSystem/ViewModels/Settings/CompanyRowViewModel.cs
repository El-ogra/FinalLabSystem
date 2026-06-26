using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CompanyRowViewModel : ViewModelBase
{
    private int _companyId;
    private string _companyName = string.Empty;
    private string _companyType = string.Empty;
    private string? _contactPerson;
    private string? _phone;
    private string? _phone2;
    private string? _email;
    private string? _address;
    private int? _schemeId;
    private double _discountRate;
    private double _creditLimit;
    private string? _paymentTerms;
    private DateOnly? _contractStartDate;
    private DateOnly? _contractEndDate;
    private string? _billingPeriodicity;
    private bool _isActive;
    private string? _notes;

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

    public string CompanyType
    {
        get => _companyType;
        set => SetProperty(ref _companyType, value);
    }

    public string? ContactPerson
    {
        get => _contactPerson;
        set => SetProperty(ref _contactPerson, value);
    }

    public string? Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string? Phone2
    {
        get => _phone2;
        set => SetProperty(ref _phone2, value);
    }

    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public int? SchemeId
    {
        get => _schemeId;
        set => SetProperty(ref _schemeId, value);
    }

    public double DiscountRate
    {
        get => _discountRate;
        set => SetProperty(ref _discountRate, value);
    }

    public double CreditLimit
    {
        get => _creditLimit;
        set => SetProperty(ref _creditLimit, value);
    }

    public string? PaymentTerms
    {
        get => _paymentTerms;
        set => SetProperty(ref _paymentTerms, value);
    }

    public DateOnly? ContractStartDate
    {
        get => _contractStartDate;
        set => SetProperty(ref _contractStartDate, value);
    }

    public DateOnly? ContractEndDate
    {
        get => _contractEndDate;
        set => SetProperty(ref _contractEndDate, value);
    }

    public string? BillingPeriodicity
    {
        get => _billingPeriodicity;
        set => SetProperty(ref _billingPeriodicity, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public Company ToModel() => new()
    {
        CompanyId = CompanyId,
        CompanyName = CompanyName,
        CompanyType = CompanyType,
        ContactPerson = ContactPerson,
        Phone = Phone,
        Phone2 = Phone2,
        Email = Email,
        Address = Address,
        SchemeId = SchemeId,
        DiscountRate = DiscountRate,
        CreditLimit = CreditLimit,
        PaymentTerms = PaymentTerms,
        ContractStartDate = ContractStartDate,
        ContractEndDate = ContractEndDate,
        BillingPeriodicity = BillingPeriodicity,
        IsActive = IsActive,
        Notes = Notes
    };

    public void LoadFromModel(Company company)
    {
        CompanyId = company.CompanyId;
        CompanyName = company.CompanyName;
        CompanyType = company.CompanyType;
        ContactPerson = company.ContactPerson;
        Phone = company.Phone;
        Phone2 = company.Phone2;
        Email = company.Email;
        Address = company.Address;
        SchemeId = company.SchemeId;
        DiscountRate = company.DiscountRate;
        CreditLimit = company.CreditLimit;
        PaymentTerms = company.PaymentTerms;
        ContractStartDate = company.ContractStartDate;
        ContractEndDate = company.ContractEndDate;
        BillingPeriodicity = company.BillingPeriodicity;
        IsActive = company.IsActive;
        Notes = company.Notes;
    }
}

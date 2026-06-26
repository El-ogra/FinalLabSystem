using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ExternalLabRowViewModel : ViewModelBase
{
    private int _externalLabId;
    private string _labName = string.Empty;
    private string? _contactPerson;
    private string? _phone;
    private string? _email;
    private string? _address;
    private string? _notes;
    private bool _isActive;

    public int ExternalLabId
    {
        get => _externalLabId;
        set => SetProperty(ref _externalLabId, value);
    }

    public string LabName
    {
        get => _labName;
        set => SetProperty(ref _labName, value);
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

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}

using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestProfileRowViewModel : Infrastructure.ViewModelBase
{
    private string _profileNameAr = string.Empty;
    private string? _profileNameEn;
    private string? _description;
    private bool _isActive;
    private int _itemCount;

    public int ProfileId { get; }

    public string ProfileNameAr
    {
        get => _profileNameAr;
        set => SetProperty(ref _profileNameAr, value);
    }

    public string? ProfileNameEn
    {
        get => _profileNameEn;
        set => SetProperty(ref _profileNameEn, value);
    }

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public int ItemCount
    {
        get => _itemCount;
        set => SetProperty(ref _itemCount, value);
    }

    public TestProfileRowViewModel(TestProfile profile)
    {
        ProfileId = profile.ProfileId;
        _profileNameAr = profile.ProfileNameAr;
        _profileNameEn = profile.ProfileNameEn;
        _description = profile.Description;
        _isActive = profile.IsActive;
        _itemCount = profile.TestProfileItems?.Count ?? 0;
    }
}

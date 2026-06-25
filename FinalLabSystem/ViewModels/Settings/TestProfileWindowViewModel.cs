using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestProfileWindowViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IDialogService _dialogService;

    private ObservableCollection<TestProfileRowViewModel> _profiles = new();
    private TestProfileRowViewModel? _selectedProfile;
    private ObservableCollection<TestProfileItemRowViewModel> _profileItems = new();

    private string _editableProfileNameAr = string.Empty;
    private string? _editableProfileNameEn;
    private string? _editableDescription;
    private bool _isEditing;
    private bool _isNewProfile;

    public TestProfileWindowViewModel(
        ITestCatalogService testCatalogService,
        ICurrentUserSession currentUserSession,
        IDialogService dialogService)
    {
        _testCatalogService = testCatalogService;
        _currentUserSession = currentUserSession;
        _dialogService = dialogService;

        NewProfileCommand = new RelayCommand(_ => StartNewProfile());
        SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync, () => IsEditing && !string.IsNullOrWhiteSpace(EditableProfileNameAr));
        DeleteProfileCommand = new AsyncRelayCommand(DeleteProfileAsync, () => SelectedProfile != null);
        AddTestCommand = new AsyncRelayCommand(AddTestAsync, () => SelectedProfile != null);
        RemoveTestCommand = new AsyncRelayCommand(RemoveTestAsync, () => SelectedItem != null);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CloseCommand = new RelayCommand(parameter =>
        {
            if (parameter is Window window)
                window.Close();
        });
    }

    public ObservableCollection<TestProfileRowViewModel> Profiles
    {
        get => _profiles;
        set => SetProperty(ref _profiles, value);
    }

    public TestProfileRowViewModel? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value) && value != null)
                LoadProfileDetail(value);
        }
    }

    public ObservableCollection<TestProfileItemRowViewModel> ProfileItems
    {
        get => _profileItems;
        set => SetProperty(ref _profileItems, value);
    }

    public TestProfileItemRowViewModel? SelectedItem { get; set; }

    public string EditableProfileNameAr
    {
        get => _editableProfileNameAr;
        set => SetProperty(ref _editableProfileNameAr, value);
    }

    public string? EditableProfileNameEn
    {
        get => _editableProfileNameEn;
        set => SetProperty(ref _editableProfileNameEn, value);
    }

    public string? EditableDescription
    {
        get => _editableDescription;
        set => SetProperty(ref _editableDescription, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public ObservableCollection<TestType> AvailableTests { get; } = new();
    public TestType? SelectedAvailableTest { get; set; }

    public ICommand NewProfileCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand AddTestCommand { get; }
    public ICommand RemoveTestCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }

    public async Task InitializeAsync()
    {
        await LoadProfilesAsync();
        await LoadAvailableTestsAsync();
    }

    private async Task LoadProfilesAsync()
    {
        var profiles = await _testCatalogService.GetAllProfilesAsync();
        Profiles = new ObservableCollection<TestProfileRowViewModel>(
            profiles.Select(p => new TestProfileRowViewModel(p)));
    }

    private async Task LoadAvailableTestsAsync()
    {
        AvailableTests.Clear();
        var tests = await _testCatalogService.GetAllTestTypesAsync();
        foreach (var t in tests)
            AvailableTests.Add(t);
    }

    private void LoadProfileDetail(TestProfileRowViewModel profile)
    {
        EditableProfileNameAr = profile.ProfileNameAr;
        EditableProfileNameEn = profile.ProfileNameEn;
        EditableDescription = profile.Description;
        _isNewProfile = false;
        IsEditing = true;

        var items = new ObservableCollection<TestProfileItemRowViewModel>();
        if (SelectedProfile != null)
        {
            var profileData = Profiles.FirstOrDefault(p => p.ProfileId == profile.ProfileId);
            if (profileData != null)
            {
                _ = LoadProfileItemsAsync(profile.ProfileId);
            }
        }
    }

    private async Task LoadProfileItemsAsync(int profileId)
    {
        var tests = await _testCatalogService.GetProfileTestsAsync(profileId);
        var profile = await _testCatalogService.GetAllProfilesAsync();
        var found = profile.FirstOrDefault(p => p.ProfileId == profileId);

        var items = new ObservableCollection<TestProfileItemRowViewModel>();
        if (found?.TestProfileItems != null)
        {
            foreach (var item in found.TestProfileItems)
                items.Add(new TestProfileItemRowViewModel(item));
        }
        ProfileItems = items;
    }

    private void StartNewProfile()
    {
        SelectedProfile = null;
        EditableProfileNameAr = string.Empty;
        EditableProfileNameEn = null;
        EditableDescription = null;
        ProfileItems.Clear();
        _isNewProfile = true;
        IsEditing = true;
    }

    private async Task SaveProfileAsync()
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId;

        if (_isNewProfile)
        {
            var newProfile = new TestProfile
            {
                ProfileNameAr = EditableProfileNameAr,
                ProfileNameEn = EditableProfileNameEn,
                Description = EditableDescription,
                CreatedBy = staffId
            };
            await _testCatalogService.CreateProfileAsync(newProfile);
        }
        else if (SelectedProfile != null)
        {
            var profile = new TestProfile
            {
                ProfileId = SelectedProfile.ProfileId,
                ProfileNameAr = EditableProfileNameAr,
                ProfileNameEn = EditableProfileNameEn,
                Description = EditableDescription,
                IsActive = true
            };
            await _testCatalogService.UpdateProfileAsync(profile);
        }

        await LoadProfilesAsync();
        IsEditing = false;
        _dialogService.ShowMessage("تم الحفظ بنجاح", "حفظ");
    }

    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile == null) return;

        if (!_dialogService.ShowConfirmation("هل أنت متأكد من حذف هذا البروفايل؟", "تأكيد الحذف"))
            return;

        await _testCatalogService.DeleteProfileAsync(SelectedProfile.ProfileId);
        await LoadProfilesAsync();
        IsEditing = false;
        _dialogService.ShowMessage("تم الحذف بنجاح", "حذف");
    }

    private async Task AddTestAsync()
    {
        if (SelectedProfile == null || SelectedAvailableTest == null) return;

        var nextOrder = ProfileItems.Count > 0
            ? ProfileItems.Max(i => i.SortOrder ?? 0) + 1
            : 1;

        await _testCatalogService.AddProfileItemAsync(
            SelectedProfile.ProfileId,
            SelectedAvailableTest.TesttypeId,
            nextOrder);

        await LoadProfileItemsAsync(SelectedProfile.ProfileId);
        SelectedProfile.ItemCount = ProfileItems.Count;
    }

    private async Task RemoveTestAsync()
    {
        if (SelectedItem == null) return;

        await _testCatalogService.RemoveProfileItemAsync(SelectedItem.ProfileItemId);

        if (SelectedProfile != null)
        {
            await LoadProfileItemsAsync(SelectedProfile.ProfileId);
            SelectedProfile.ItemCount = ProfileItems.Count;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadProfilesAsync();
        await LoadAvailableTestsAsync();
        IsEditing = false;
    }
}

using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class ReferralViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IReferralService _referralService;
    private string? _referralTitle;
    private string? _searchText;
    private ReferralSource? _selectedReferral;
    private bool _shouldSaveReferral;
    private bool _printReferralOnReport;
    private string? _referralAddress;

    public ReferralViewModel(IReferralService referralService)
    {
        _referralService = referralService;
        ReferralSuggestions = new ObservableCollection<ReferralSource>();
        ReferralTitles = new ObservableCollection<string>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var titles = await _referralService.GetReferralTitlesAsync();
            ReferralTitles.Clear();
            foreach (var title in titles)
                ReferralTitles.Add(title);

            await SearchAsync(null);
        }
        catch
        {
            // TODO F-07: _dialogService.ShowError("حدث خطأ أثناء تحميل البيانات.");
        }
    }

    public ObservableCollection<ReferralSource> ReferralSuggestions { get; }

    public ObservableCollection<string> ReferralTitles { get; }

    public string? ReferralTitle
    {
        get => _referralTitle;
        set => SetProperty(ref _referralTitle, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                _ = SearchAsync(value);
        }
    }

    public ReferralSource? SelectedReferral
    {
        get => _selectedReferral;
        set
        {
            if (SetProperty(ref _selectedReferral, value) && value is not null)
                LoadReferral(value);
        }
    }

    public bool ShouldSaveReferral
    {
        get => _shouldSaveReferral;
        set => SetProperty(ref _shouldSaveReferral, value);
    }

    public bool PrintReferralOnReport
    {
        get => _printReferralOnReport;
        set => SetProperty(ref _printReferralOnReport, value);
    }

    public string? ReferralAddress
    {
        get => _referralAddress;
        set => SetProperty(ref _referralAddress, value);
    }

    public void LoadReferral(ReferralSource referral)
    {
        ReferralTitle = referral.Title;
        SearchText = referral.SourceName;
        ReferralAddress = referral.Address;
        SelectedReferral = referral;
    }

    public void LoadFromDto(VisitFullDto dto)
    {
        ReferralTitle = dto.ReferralTitle;
        SearchText = dto.ReferralName;
        ReferralAddress = dto.ReferralAddress;
        SelectedReferral = dto.ReferralId.HasValue
            ? new ReferralSource
            {
                ReferralId = dto.ReferralId.Value,
                SourceType = "DOCTOR",
                Title = dto.ReferralTitle,
                SourceName = string.IsNullOrWhiteSpace(dto.ReferralName) ? "Referral Source" : dto.ReferralName,
                Address = dto.ReferralAddress,
                IsActive = true
            }
            : null;
        ShouldSaveReferral = false;
        PrintReferralOnReport = false;
    }

    public void ClearAllFields()
    {
        ReferralTitle = null;
        SearchText = null;
        SelectedReferral = null;
        ShouldSaveReferral = false;
        PrintReferralOnReport = false;
        ReferralAddress = null;
    }

    public ReferralSource ToReferralSource()
    {
        return new ReferralSource
        {
            SourceType = "DOCTOR",
            Title = ReferralTitle,
            SourceName = string.IsNullOrWhiteSpace(SearchText) ? "Referral Source" : SearchText.Trim(),
            Address = ReferralAddress,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task SearchAsync(string? term)
    {
        var results = await _referralService.SearchReferralSourcesAsync(term ?? string.Empty);
        ReferralSuggestions.Clear();
        foreach (var referral in results)
            ReferralSuggestions.Add(referral);
    }
}

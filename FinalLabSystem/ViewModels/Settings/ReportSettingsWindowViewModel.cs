using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ReportSettingsWindowViewModel : ViewModelBase
{
    private readonly IReportLayoutService _reportLayoutService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserSession _currentUserSession;

    private ReportLayoutDto _currentLayout = new();
    private bool _isBusy;

    public ReportSettingsWindowViewModel(
        IReportLayoutService reportLayoutService,
        IDialogService dialogService,
        ICurrentUserSession currentUserSession)
    {
        _reportLayoutService = reportLayoutService;
        _dialogService = dialogService;
        _currentUserSession = currentUserSession;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
        ResetToDefaultsCommand = new AsyncRelayCommand(ResetToDefaultsAsync, () => !IsBusy);
        BrowseLogoCommand = new RelayCommand(_ => BrowseLogo());
        PreviewCommand = new AsyncRelayCommand(PreviewAsync);
    }

    public ReportLayoutDto CurrentLayout
    {
        get => _currentLayout;
        set { _currentLayout = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand BrowseLogoCommand { get; }
    public ICommand PreviewCommand { get; }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            CurrentLayout = await _reportLayoutService.GetCurrentLayoutAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            var staffId = _currentUserSession.CurrentUser?.StaffId
                ?? throw new InvalidOperationException("لا يمكن الحفظ بدون جلسة مستخدم نشطة.");
            await _reportLayoutService.SaveLayoutAsync(CurrentLayout, staffId);
            _dialogService.ShowMessage("تم حفظ الإعدادات بنجاح.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResetToDefaultsAsync()
    {
        if (!_dialogService.ShowConfirmation("هل أنت متأكد من إعادة جميع الإعدادات إلى القيم الافتراضية؟"))
            return;

        IsBusy = true;
        try
        {
            await _reportLayoutService.ResetToDefaultsAsync();
            CurrentLayout = _reportLayoutService.GetDefaults();
            _dialogService.ShowMessage("تمت إعادة الإعدادات إلى القيم الافتراضية.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BrowseLogo()
    {
        // No file dialog available in IDialogService - placeholder for future implementation
        _dialogService.ShowMessage("ميزة استعراض الشعار متاحة في الإصدارات القادمة.");
    }

    private async Task PreviewAsync()
    {
        await Task.CompletedTask;
        _dialogService.ShowMessage("معاينة التقرير: الإعدادات الحالية ستُطبَّع على التقارير.");
    }
}

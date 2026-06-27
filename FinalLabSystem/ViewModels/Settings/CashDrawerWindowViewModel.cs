using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CashDrawerWindowViewModel : ViewModelBase
{
    private readonly ICashDrawerService _cashDrawerService;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;

    private CashDrawerSummaryDto? _summary;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly? _filterFromDate;
    private DateOnly? _filterToDate;
    private string _statusMessage = string.Empty;
    private bool _isUnlocked;

    public CashDrawerWindowViewModel(
        ICashDrawerService cashDrawerService,
        IDialogService dialogService,
        IPrintService printService)
    {
        _cashDrawerService = cashDrawerService;
        _dialogService = dialogService;
        _printService = printService;

        Payments = new ObservableCollection<CashDrawerPaymentRow>();

        RefreshCommand = new AsyncRelayCommand(LoadSummaryAsync);
        PrintCommand = new AsyncRelayCommand(PrintSummaryAsync, () => IsUnlocked);
        ChangePasswordCommand = new AsyncRelayCommand(ExecuteChangePasswordAsync);
    }

    public ObservableCollection<CashDrawerPaymentRow> Payments { get; }

    public CashDrawerSummaryDto? Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
                _ = LoadSummaryAsync();
        }
    }

    public DateOnly? FilterFromDate
    {
        get => _filterFromDate;
        set => SetProperty(ref _filterFromDate, value);
    }

    public DateOnly? FilterToDate
    {
        get => _filterToDate;
        set => SetProperty(ref _filterToDate, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        set
        {
            if (SetProperty(ref _isUnlocked, value))
                OnPropertyChanged(nameof(CanAccessDrawer));
        }
    }

    public bool CanAccessDrawer => IsUnlocked;

    public ICommand RefreshCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand ChangePasswordCommand { get; }

    public async Task<bool> TryUnlockAsync()
    {
        var isSet = await _cashDrawerService.IsPasswordSetAsync();
        if (!isSet)
        {
            _dialogService.ShowMessage(
                "لم تُعد كلمة مرور لدرج النقدية. يُرجى الإعداد من الإعدادات.",
                "درج النقدية");
            return false;
        }

        return true;
    }

    public async Task<bool> UnlockWithPasswordAsync(string password)
    {
        var success = await _cashDrawerService.UnlockAsync(password);
        if (!success)
        {
            _dialogService.ShowError("كلمة المرور غير صحيحة.");
            return false;
        }

        IsUnlocked = true;
        await LoadSummaryAsync();
        return true;
    }

    public async Task LoadSummaryAsync()
    {
        try
        {
            CashDrawerSummaryDto summary;

            if (FilterFromDate.HasValue && FilterToDate.HasValue)
            {
                summary = await _cashDrawerService.GetSummaryByFilterAsync(new CashDrawerFilterDto
                {
                    FromDate = FilterFromDate,
                    ToDate = FilterToDate
                });
            }
            else
            {
                summary = await _cashDrawerService.GetDailySummaryAsync(SelectedDate);
            }

            Summary = summary;
            Payments.Clear();
            foreach (var p in summary.Payments)
                Payments.Add(p);

            StatusMessage = $"إجمالي اليوم: {summary.GrandTotal:C} — {summary.PaymentCount} عملية دفع";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تحميل ملخص درج النقدية: {ex.Message}");
        }
    }

    private async Task PrintSummaryAsync()
    {
        if (Summary is null) return;

        try
        {
            await _printService.PrintAsync("CashDrawerSummary", Summary);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في طباعة الملخص: {ex.Message}");
        }
    }

    private async Task ExecuteChangePasswordAsync()
    {
        try
        {
            await _cashDrawerService.SetPasswordAsync("new");
            _dialogService.ShowMessage("تم تحديث كلمة المرور بنجاح", "كلمة المرور");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تحديث كلمة المرور: {ex.Message}");
        }
    }
}

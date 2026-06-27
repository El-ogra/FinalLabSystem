using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CommissionReportWindowViewModel : ViewModelBase
{
    private readonly ICommissionReportService _commissionReportService;
    private readonly IPrintService _printService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<CommissionReportRow> _rows = new();
    private DateTime _startDate;
    private DateTime _endDate;
    private string _statusMessage = string.Empty;
    private bool _isLoading;

    public CommissionReportWindowViewModel(
        ICommissionReportService commissionReportService,
        IPrintService printService,
        IDialogService dialogService)
    {
        _commissionReportService = commissionReportService;
        _printService = printService;
        _dialogService = dialogService;

        _startDate = DateTime.Today.AddMonths(-1);
        _endDate = DateTime.Today;

        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        PrintCommand = new RelayCommand(async _ => await PrintAsync());
    }

    public ObservableCollection<CommissionReportRow> Rows
    {
        get => _rows;
        set { _rows = value; OnPropertyChanged(); }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set { _startDate = value; OnPropertyChanged(); }
    }

    public DateTime EndDate
    {
        get => _endDate;
        set { _endDate = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }
    public ICommand PrintCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = "جاري التحميل...";

        try
        {
            var data = await _commissionReportService.GetCommissionReportAsync(StartDate, EndDate);
            Rows = new ObservableCollection<CommissionReportRow>(data);
            StatusMessage = $"تم تحميل {Rows.Count} سجل";
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task PrintAsync()
    {
        if (Rows.Count == 0)
        {
            _dialogService.ShowWarning("لا توجد بيانات للطباعة");
            return;
        }

        try
        {
            await _printService.PrintAsync("CommissionReport", Rows);
        }
        catch (Exception ex)
        {
            _dialogService.ShowWarning($"خطأ في الطباعة: {ex.Message}");
        }
    }
}

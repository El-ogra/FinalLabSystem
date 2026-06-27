using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class OutstandingBalanceWindowViewModel : ViewModelBase
{
    private readonly IOutstandingBalanceReportService _reportService;
    private readonly IPrintService _printService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<OutstandingBalanceReportRow> _reports = new();
    private DateTime _fromDate;
    private DateTime _toDate;
    private double _totalOutstanding;
    private string _statusMessage = string.Empty;
    private bool _isLoading;

    public OutstandingBalanceWindowViewModel(
        IOutstandingBalanceReportService reportService,
        IPrintService printService,
        IDialogService dialogService)
    {
        _reportService = reportService;
        _printService = printService;
        _dialogService = dialogService;

        _fromDate = DateTime.Today.AddMonths(-1);
        _toDate = DateTime.Today;

        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        PrintCommand = new RelayCommand(async _ => await PrintAsync());
    }

    public ObservableCollection<OutstandingBalanceReportRow> Reports
    {
        get => _reports;
        set { _reports = value; OnPropertyChanged(); }
    }

    public DateTime FromDate
    {
        get => _fromDate;
        set { _fromDate = value; OnPropertyChanged(); }
    }

    public DateTime ToDate
    {
        get => _toDate;
        set { _toDate = value; OnPropertyChanged(); }
    }

    public double TotalOutstanding
    {
        get => _totalOutstanding;
        set { _totalOutstanding = value; OnPropertyChanged(); }
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
            var data = await _reportService.GetOutstandingBalancesAsync(FromDate, ToDate);
            Reports = new ObservableCollection<OutstandingBalanceReportRow>(data);
            TotalOutstanding = data.Sum(r => r.BalanceDue);
            StatusMessage = $"تم تحميل {Reports.Count} سجل — إجمالي المستحق: {TotalOutstanding:N2}";
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
        if (Reports.Count == 0)
        {
            _dialogService.ShowWarning("لا توجد بيانات للطباعة");
            return;
        }

        try
        {
            await _printService.PrintAsync("OutstandingBalance", Reports);
        }
        catch (Exception ex)
        {
            _dialogService.ShowWarning($"خطأ في الطباعة: {ex.Message}");
        }
    }
}

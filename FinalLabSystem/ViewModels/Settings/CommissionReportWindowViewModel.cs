using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private ObservableCollection<CommissionReportRow> _doctorRows = new();
    private ObservableCollection<CommissionReportRow> _outsourcedRows = new();
    private ObservableCollection<CommissionReportRow> _contractRows = new();
    private DateTime _startDate;
    private DateTime _endDate;
    private string _statusMessage = string.Empty;
    private bool _isLoading;
    private double _doctorTotal;
    private double _outsourcedTotal;
    private double _contractTotal;
    private double _grandTotal;

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

    public ObservableCollection<CommissionReportRow> DoctorRows
    {
        get => _doctorRows;
        set { _doctorRows = value; OnPropertyChanged(); }
    }

    public ObservableCollection<CommissionReportRow> OutsourcedRows
    {
        get => _outsourcedRows;
        set { _outsourcedRows = value; OnPropertyChanged(); }
    }

    public ObservableCollection<CommissionReportRow> ContractRows
    {
        get => _contractRows;
        set { _contractRows = value; OnPropertyChanged(); }
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

    public double DoctorTotal
    {
        get => _doctorTotal;
        set { _doctorTotal = value; OnPropertyChanged(); }
    }

    public double OutsourcedTotal
    {
        get => _outsourcedTotal;
        set { _outsourcedTotal = value; OnPropertyChanged(); }
    }

    public double ContractTotal
    {
        get => _contractTotal;
        set { _contractTotal = value; OnPropertyChanged(); }
    }

    public double GrandTotal
    {
        get => _grandTotal;
        set { _grandTotal = value; OnPropertyChanged(); }
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

            // [VS-17] تقسيم البيانات حسب التصنيف
            DoctorRows = new ObservableCollection<CommissionReportRow>(
                data.Where(r => r.Category == "ReferringDoctor"));
            OutsourcedRows = new ObservableCollection<CommissionReportRow>(
                data.Where(r => r.Category == "OutsourcedSample"));
            ContractRows = new ObservableCollection<CommissionReportRow>(
                data.Where(r => r.Category == "ReferralOrContractEntity"));

            // [VS-17] حساب الإجماليات لكل قسم
            DoctorTotal = DoctorRows.Sum(r => r.CommissionDue ?? 0);
            OutsourcedTotal = OutsourcedRows.Sum(r => r.CommissionDue ?? 0);
            ContractTotal = ContractRows.Sum(r => r.CommissionDue ?? 0);
            GrandTotal = DoctorTotal + OutsourcedTotal + ContractTotal;

            StatusMessage = $"تم تحميل {Rows.Count} سجل | أطباء: {DoctorRows.Count} | عينات خارجية: {OutsourcedRows.Count} | جهات تعاقد: {ContractRows.Count}";
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

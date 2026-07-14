using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class StaffManagementViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IStaffService _staffService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IDialogService _dialogService;
    private Staff? _selectedStaff;

    public StaffManagementViewModel(IStaffService staffService, ICurrentUserSession currentUserSession, IDialogService dialogService)
    {
        _staffService = staffService;
        _currentUserSession = currentUserSession;
        _dialogService = dialogService;
        StaffList = new ObservableCollection<Staff>();
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public ObservableCollection<Staff> StaffList { get; }

    public Staff? SelectedStaff
    {
        get => _selectedStaff;
        set
        {
            if (SetProperty(ref _selectedStaff, value))
            {
                OnPropertyChanged(nameof(SelectedDiscountLimit));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public double SelectedDiscountLimit
    {
        get => _selectedStaff?.DiscountLimit ?? 0;
        set
        {
            if (_selectedStaff is not null)
            {
                _selectedStaff.DiscountLimit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDiscountLimitDisplay));
            }
        }
    }

    public string SelectedDiscountLimitDisplay => _selectedStaff is not null
        ? $"حد الخصم: {_selectedStaff.DiscountLimit}%"
        : "";

    public ICommand SaveCommand { get; }

    public async Task InitializeAsync()
    {
        await LoadStaffAsync();
    }

    private async Task LoadStaffAsync()
    {
        var staffList = await _staffService.GetAllAsync();
        StaffList.Clear();
        foreach (var staff in staffList)
            StaffList.Add(staff);
    }

    private async Task SaveAsync()
    {
        if (_selectedStaff is null)
            return;

        var currentStaffId = _currentUserSession.CurrentUser?.StaffId;
        if (currentStaffId is null or 0)
        {
            _dialogService.ShowError("لا يمكن الحفظ بدون جلسة مستخدم نشطة.");
            return;
        }

        await _staffService.UpdateStaffAsync(_selectedStaff, currentStaffId.Value);
        _dialogService.ShowMessage("تم حفظ التعديلات بنجاح.", "حفظ");
    }
}

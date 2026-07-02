using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients.Delivery;
using FinalLabSystem.Views.Patients.Delivery;

namespace FinalLabSystem.ViewModels.Patients.Delivery;

public sealed class DeliveryViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IDeliveryConfirmationService _deliveryConfirmationService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IOtpGenerator _otpGenerator;

    private int _visitId;
    private string _patientName = string.Empty;
    private bool _isDelivered;
    private string _statusMessage = string.Empty;

    public DeliveryViewModel(
        INavigationService navigationService,
        IDeliveryConfirmationService deliveryConfirmationService,
        IDialogService dialogService,
        ICurrentUserSession currentUserSession,
        IOtpGenerator otpGenerator)
    {
        _navigationService = navigationService;
        _deliveryConfirmationService = deliveryConfirmationService;
        _dialogService = dialogService;
        _currentUserSession = currentUserSession;
        _otpGenerator = otpGenerator;

        ReturnToMainCommand = new RelayCommand(_ => navigationService.ReturnToMain());
        ConfirmWithSignatureCommand = new AsyncRelayCommand(async _ => await ConfirmWithSignatureAsync(), _ => !IsDelivered && _visitId > 0);
        ConfirmWithOtpCommand = new AsyncRelayCommand(async _ => await ConfirmWithOtpAsync(), _ => !IsDelivered && _visitId > 0);
    }

    public ICommand ReturnToMainCommand { get; }
    public ICommand ConfirmWithSignatureCommand { get; }
    public ICommand ConfirmWithOtpCommand { get; }

    public string PatientName
    {
        get => _patientName;
        set { _patientName = value; OnPropertyChanged(); }
    }

    public bool IsDelivered
    {
        get => _isDelivered;
        set { _isDelivered = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public void Initialize(int visitId, string patientName)
    {
        _visitId = visitId;
        PatientName = patientName;
        _ = LoadDeliveryStatusAsync();
    }

    private async Task LoadDeliveryStatusAsync()
    {
        IsDelivered = await _deliveryConfirmationService.IsDeliveredAsync(_visitId);
        if (IsDelivered)
            StatusMessage = "تم التسليم مسبقاً.";
    }

    private Task ConfirmWithSignatureAsync()
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0)
        {
            _dialogService.ShowError("لا يمكن التسليم بدون جلسة مستخدم نشطة.");
            return Task.CompletedTask;
        }

        var vm = new SignatureConfirmationDialogViewModel(_deliveryConfirmationService, _visitId, staffId);
        var dialog = new SignatureConfirmationDialog { DataContext = vm };
        vm.RequestClose = result =>
        {
            dialog.DialogResult = result;
            dialog.Close();
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            IsDelivered = true;
            StatusMessage = "تم التسليم بالتوقيع بنجاح.";
            _dialogService.ShowMessage("تم تأكيد التسليم بالتوقيع.");
        }

        return Task.CompletedTask;
    }

    private async Task ConfirmWithOtpAsync()
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0)
        {
            _dialogService.ShowError("لا يمكن التسليم بدون جلسة مستخدم نشطة.");
            return;
        }

        try
        {
            string otp = await _deliveryConfirmationService.GenerateOtpAsync(_visitId, staffId);
            _dialogService.ShowMessage($"رمز التحقق: {otp}\n\nأرسل هذا الرمز إلى المستلم untuk التحقق من الهوية.");

            var vm = new OtpVerificationDialogViewModel(_deliveryConfirmationService, _visitId, staffId);
            var dialog = new OtpVerificationDialog { DataContext = vm };
            vm.RequestClose = result =>
            {
                dialog.DialogResult = result;
                dialog.Close();
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                IsDelivered = true;
                StatusMessage = "تم التسليم بالتحقق بنجاح.";
                _dialogService.ShowMessage("تم تأكيد التسليم بالتحقق.");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في إنشاء رمز التحقق: {ex.Message}");
        }
    }
}

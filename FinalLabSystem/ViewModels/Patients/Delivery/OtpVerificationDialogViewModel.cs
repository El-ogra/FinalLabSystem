using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients.Delivery;

public sealed class OtpVerificationDialogViewModel : ViewModelBase
{
    private readonly IDeliveryConfirmationService _deliveryConfirmationService;
    private readonly int _visitId;
    private readonly int _staffId;

    private string _enteredOtp = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public OtpVerificationDialogViewModel(
        IDeliveryConfirmationService deliveryConfirmationService,
        int visitId,
        int staffId)
    {
        _deliveryConfirmationService = deliveryConfirmationService;
        _visitId = visitId;
        _staffId = staffId;

        VerifyCommand = new AsyncRelayCommand(async _ => await VerifyAsync(), _ => CanVerify);
        ResendOtpCommand = new AsyncRelayCommand(async _ => await ResendOtpAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    public string EnteredOtp
    {
        get => _enteredOtp;
        set { _enteredOtp = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanVerify)); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public bool CanVerify => EnteredOtp.Length == 6 && !IsBusy;

    public ICommand VerifyCommand { get; }
    public ICommand ResendOtpCommand { get; }
    public ICommand CancelCommand { get; }

    public Action<bool?>? RequestClose { get; set; }

    private async Task VerifyAsync()
    {
        if (!CanVerify) return;

        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            bool success = await _deliveryConfirmationService.VerifyOtpAsync(_visitId, EnteredOtp, _staffId);
            if (success)
            {
                RequestClose?.Invoke(true);
            }
            else
            {
                StatusMessage = "رمز التحقق غير صحيح. حاول مرة أخرى.";
                EnteredOtp = string.Empty;
            }
        }
        catch (Exception)
        {
            StatusMessage = "حدث خطأ أثناء التحقق.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResendOtpAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            await _deliveryConfirmationService.GenerateOtpAsync(_visitId, _staffId);
            StatusMessage = "تم إعادة إرسال رمز التحقق.";
        }
        catch (Exception)
        {
            StatusMessage = "حدث خطأ أثناء إعادة الإرسال.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients.Delivery;

public sealed class SignatureConfirmationDialogViewModel : ViewModelBase
{
    private readonly IDeliveryConfirmationService _deliveryConfirmationService;
    private readonly int _visitId;
    private readonly int _staffId;

    private byte[]? _capturedSignature;
    private string _receivedByName = string.Empty;
    private bool _isBusy;

    public SignatureConfirmationDialogViewModel(
        IDeliveryConfirmationService deliveryConfirmationService,
        int visitId,
        int staffId)
    {
        _deliveryConfirmationService = deliveryConfirmationService;
        _visitId = visitId;
        _staffId = staffId;

        ClearCommand = new RelayCommand(_ => CapturedSignature = null);
        ConfirmCommand = new AsyncRelayCommand(async _ => await ConfirmAsync(), _ => CanConfirm);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    public byte[]? CapturedSignature
    {
        get => _capturedSignature;
        set { _capturedSignature = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConfirm)); }
    }

    public string ReceivedByName
    {
        get => _receivedByName;
        set { _receivedByName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConfirm)); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public bool CanConfirm => CapturedSignature != null && CapturedSignature.Length > 0 && !string.IsNullOrWhiteSpace(ReceivedByName) && !IsBusy;

    public ICommand ClearCommand { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public Action<bool?>? RequestClose { get; set; }

    private async Task ConfirmAsync()
    {
        if (!CanConfirm) return;

        IsBusy = true;
        try
        {
            await _deliveryConfirmationService.SaveSignatureAsync(_visitId, CapturedSignature!, ReceivedByName.Trim(), _staffId);
            RequestClose?.Invoke(true);
        }
        catch (Exception)
        {
            RequestClose?.Invoke(false);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

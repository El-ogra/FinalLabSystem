using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ExternalLabsWindowViewModel : ViewModelBase
{
    private readonly IExternalLabRegistryService _labRegistryService;
    private readonly IExternalShipmentService _shipmentService;

    public ExternalLabsWindowViewModel(
        IExternalLabRegistryService labRegistryService,
        IExternalShipmentService shipmentService)
    {
        _labRegistryService = labRegistryService;
        _shipmentService = shipmentService;

        Labs = new ObservableCollection<ExternalLabRowViewModel>();
        Shipments = new ObservableCollection<ShipmentRowViewModel>();
        PendingItems = new ObservableCollection<ShipmentItemRowViewModel>();
        SelectedVisitTestIds = new List<int>();

        RefreshLabsCommand = new AsyncRelayCommand(LoadLabsAsync);
        CreateLabCommand = new AsyncRelayCommand(CreateLabAsync, () => CanCreateLab);
        SaveLabCommand = new AsyncRelayCommand(SaveLabAsync);
        DeleteLabCommand = new AsyncRelayCommand(DeleteLabAsync, () => SelectedLab is not null);
        RefreshShipmentsCommand = new AsyncRelayCommand(LoadShipmentsAsync);
        LoadPendingCommand = new AsyncRelayCommand(LoadPendingItemsAsync);
        SendManifestCommand = new AsyncRelayCommand(SendManifestAsync, () => CanSendManifest);
        ReceiveResultsCommand = new AsyncRelayCommand(ReceiveResultsAsync, () => CanReceiveResults);
    }

    public ObservableCollection<ExternalLabRowViewModel> Labs { get; }
    public ObservableCollection<ShipmentRowViewModel> Shipments { get; }
    public ObservableCollection<ShipmentItemRowViewModel> PendingItems { get; }

    public ICommand RefreshLabsCommand { get; }
    public ICommand CreateLabCommand { get; }
    public ICommand SaveLabCommand { get; }
    public ICommand DeleteLabCommand { get; }
    public ICommand RefreshShipmentsCommand { get; }
    public ICommand LoadPendingCommand { get; }
    public ICommand SendManifestCommand { get; }
    public ICommand ReceiveResultsCommand { get; }

    private ExternalLabRowViewModel? _selectedLab;
    public ExternalLabRowViewModel? SelectedLab
    {
        get => _selectedLab;
        set
        {
            SetProperty(ref _selectedLab, value);
            OnPropertyChanged(nameof(CanDeleteLab));
            _ = LoadShipmentsAsync();
        }
    }

    private ShipmentRowViewModel? _selectedShipment;
    public ShipmentRowViewModel? SelectedShipment
    {
        get => _selectedShipment;
        set => SetProperty(ref _selectedShipment, value);
    }

    private ShipmentItemRowViewModel? _selectedPendingItem;
    public ShipmentItemRowViewModel? SelectedPendingItem
    {
        get => _selectedPendingItem;
        set
        {
            SetProperty(ref _selectedPendingItem, value);
            OnPropertyChanged(nameof(CanReceiveResults));
        }
    }

    private string _editLabName = string.Empty;
    public string EditLabName
    {
        get => _editLabName;
        set
        {
            SetProperty(ref _editLabName, value);
            OnPropertyChanged(nameof(CanCreateLab));
        }
    }

    private string? _editContactPerson;
    public string? EditContactPerson
    {
        get => _editContactPerson;
        set => SetProperty(ref _editContactPerson, value);
    }

    private string? _editPhone;
    public string? EditPhone
    {
        get => _editPhone;
        set => SetProperty(ref _editPhone, value);
    }

    private string? _editEmail;
    public string? EditEmail
    {
        get => _editEmail;
        set => SetProperty(ref _editEmail, value);
    }

    private string? _editAddress;
    public string? EditAddress
    {
        get => _editAddress;
        set => SetProperty(ref _editAddress, value);
    }

    private string? _editNotes;
    public string? EditNotes
    {
        get => _editNotes;
        set => SetProperty(ref _editNotes, value);
    }

    private string _receiveResultValue = string.Empty;
    public string ReceiveResultValue
    {
        get => _receiveResultValue;
        set
        {
            SetProperty(ref _receiveResultValue, value);
            OnPropertyChanged(nameof(CanReceiveResults));
        }
    }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool CanCreateLab => !string.IsNullOrWhiteSpace(EditLabName);
    public bool CanDeleteLab => SelectedLab is not null;
    public bool CanSendManifest => SelectedLab is not null && PendingItems.Any(i => i.IsSelected);
    public bool CanReceiveResults => SelectedPendingItem is not null && !string.IsNullOrWhiteSpace(ReceiveResultValue);

    private List<int> SelectedVisitTestIds { get; set; }

    public async Task InitializeAsync()
    {
        await LoadLabsAsync();
    }

    private async Task LoadLabsAsync()
    {
        IsLoading = true;
        try
        {
            var labs = await _labRegistryService.GetAllAsync();
            Labs.Clear();
            foreach (var lab in labs)
            {
                Labs.Add(new ExternalLabRowViewModel
                {
                    ExternalLabId = lab.ExternalLabId,
                    LabName = lab.LabName,
                    ContactPerson = lab.ContactPerson,
                    Phone = lab.Phone,
                    Email = lab.Email,
                    Address = lab.Address,
                    Notes = lab.Notes,
                    IsActive = lab.IsActive
                });
            }
            StatusMessage = $"تم تحميل {Labs.Count} مختبر";
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

    private async Task LoadShipmentsAsync()
    {
        if (SelectedLab is null) return;

        IsLoading = true;
        try
        {
            var shipments = await _shipmentService.GetShipmentsAsync(SelectedLab.ExternalLabId);
            Shipments.Clear();
            foreach (var s in shipments)
            {
                var vm = new ShipmentRowViewModel
                {
                    ShipmentId = s.ShipmentId,
                    ExternalLabId = s.ExternalLabId,
                    LabName = s.ExternalLab?.LabName ?? "",
                    ShipmentDate = s.ShipmentDate,
                    Status = s.Status,
                    TrackingNumber = s.TrackingNumber
                };

                foreach (var item in s.ExternalShipmentItems)
                {
                    vm.Items.Add(new ShipmentItemRowViewModel
                    {
                        ShipmentItemId = item.ShipmentItemId,
                        VisitTestId = item.VisitTestId,
                        Status = item.Status,
                        TestName = item.VisitTest?.Testtype?.TypeNameEn,
                        PatientName = item.VisitTest?.Visit?.Patient?.FullNameEn ?? item.VisitTest?.Visit?.Patient?.FullNameAr
                    });
                }

                Shipments.Add(vm);
            }
            StatusMessage = $"تم تحميل {Shipments.Count} شحنة";
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

    private async Task LoadPendingItemsAsync()
    {
        if (SelectedLab is null) return;

        IsLoading = true;
        try
        {
            var shipments = await _shipmentService.GetShipmentsAsync(SelectedLab.ExternalLabId);
            PendingItems.Clear();

            foreach (var shipment in shipments.Where(s => s.Status != "COMPLETED"))
            {
                foreach (var item in shipment.ExternalShipmentItems.Where(i => i.Status != "Received"))
                {
                    PendingItems.Add(new ShipmentItemRowViewModel
                    {
                        ShipmentItemId = item.ShipmentItemId,
                        VisitTestId = item.VisitTestId,
                        Status = item.Status,
                        TestName = item.VisitTest?.Testtype?.TypeNameEn,
                        PatientName = item.VisitTest?.Visit?.Patient?.FullNameEn ?? item.VisitTest?.Visit?.Patient?.FullNameAr
                    });
                }
            }
            StatusMessage = $"تم تحميل {PendingItems.Count} عنصر معلق";
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

    private async Task CreateLabAsync()
    {
        IsLoading = true;
        try
        {
            var lab = new ExternalLab
            {
                LabName = EditLabName,
                ContactPerson = EditContactPerson,
                Phone = EditPhone,
                Email = EditEmail,
                Address = EditAddress,
                Notes = EditNotes
            };

            await _labRegistryService.CreateAsync(lab);
            StatusMessage = $"تم إنشاء المختبر: {lab.LabName}";

            EditLabName = string.Empty;
            EditContactPerson = null;
            EditPhone = null;
            EditEmail = null;
            EditAddress = null;
            EditNotes = null;

            await LoadLabsAsync();
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

    private async Task SaveLabAsync()
    {
        if (SelectedLab is null) return;

        IsLoading = true;
        try
        {
            var lab = new ExternalLab
            {
                ExternalLabId = SelectedLab.ExternalLabId,
                LabName = SelectedLab.LabName,
                ContactPerson = SelectedLab.ContactPerson,
                Phone = SelectedLab.Phone,
                Email = SelectedLab.Email,
                Address = SelectedLab.Address,
                Notes = SelectedLab.Notes,
                IsActive = SelectedLab.IsActive
            };

            await _labRegistryService.UpdateAsync(lab);
            StatusMessage = $"تم حفظ المختبر: {lab.LabName}";
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

    private async Task DeleteLabAsync()
    {
        if (SelectedLab is null) return;

        IsLoading = true;
        try
        {
            var lab = new ExternalLab
            {
                ExternalLabId = SelectedLab.ExternalLabId,
                LabName = SelectedLab.LabName,
                ContactPerson = SelectedLab.ContactPerson,
                Phone = SelectedLab.Phone,
                Email = SelectedLab.Email,
                Address = SelectedLab.Address,
                Notes = SelectedLab.Notes,
                IsActive = false
            };

            await _labRegistryService.UpdateAsync(lab);
            StatusMessage = $"تم حذف المختبر: {lab.LabName}";
            await LoadLabsAsync();
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

    private async Task SendManifestAsync()
    {
        if (SelectedLab is null) return;

        var selectedIds = PendingItems.Where(i => i.IsSelected).Select(i => i.VisitTestId).ToList();
        if (!selectedIds.Any())
        {
            StatusMessage = "اختر عناصر للإرسال";
            return;
        }

        IsLoading = true;
        try
        {
            var shipment = await _shipmentService.CreateManifestAsync(
                SelectedLab.ExternalLabId, selectedIds, 0);

            StatusMessage = $"تم إنشاء الشحنة رقم {shipment.ShipmentId} — {shipment.TrackingNumber}";
            await LoadShipmentsAsync();
            await LoadPendingItemsAsync();
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

    private async Task ReceiveResultsAsync()
    {
        if (SelectedPendingItem is null) return;

        IsLoading = true;
        try
        {
            await _shipmentService.ReceiveResultsAsync(
                SelectedPendingItem.ShipmentItemId,
                ReceiveResultValue,
                0);

            StatusMessage = $"تم استلام النتيجة للعنصر {SelectedPendingItem.VisitTestId}";
            ReceiveResultValue = string.Empty;
            await LoadShipmentsAsync();
            await LoadPendingItemsAsync();
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
}

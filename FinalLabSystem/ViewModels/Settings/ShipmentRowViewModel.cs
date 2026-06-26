using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ShipmentRowViewModel : ViewModelBase
{
    private int _shipmentId;
    private int _externalLabId;
    private string _labName = string.Empty;
    private DateTime _shipmentDate;
    private string _status = string.Empty;
    private string? _trackingNumber;
    private ObservableCollection<ShipmentItemRowViewModel> _items = new();

    public int ShipmentId
    {
        get => _shipmentId;
        set => SetProperty(ref _shipmentId, value);
    }

    public int ExternalLabId
    {
        get => _externalLabId;
        set => SetProperty(ref _externalLabId, value);
    }

    public string LabName
    {
        get => _labName;
        set => SetProperty(ref _labName, value);
    }

    public DateTime ShipmentDate
    {
        get => _shipmentDate;
        set => SetProperty(ref _shipmentDate, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string? TrackingNumber
    {
        get => _trackingNumber;
        set => SetProperty(ref _trackingNumber, value);
    }

    public ObservableCollection<ShipmentItemRowViewModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
}

public sealed class ShipmentItemRowViewModel : ViewModelBase
{
    private int _shipmentItemId;
    private int _visitTestId;
    private string? _status;
    private string? _testName;
    private string? _patientName;
    private bool _isSelected;

    public int ShipmentItemId
    {
        get => _shipmentItemId;
        set => SetProperty(ref _shipmentItemId, value);
    }

    public int VisitTestId
    {
        get => _visitTestId;
        set => SetProperty(ref _visitTestId, value);
    }

    public string? Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string? TestName
    {
        get => _testName;
        set => SetProperty(ref _testName, value);
    }

    public string? PatientName
    {
        get => _patientName;
        set => SetProperty(ref _patientName, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

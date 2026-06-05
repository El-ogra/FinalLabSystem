using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestTypeSampleTubeRowViewModel : ViewModelBase
{
    private int _testTypeTubeId;
    private int _testTypeId;
    private string _tubeType = string.Empty;
    private string? _tubeColor;
    private string? _sampleType;
    private int _quantity = 1;
    private short _sortOrder;
    private bool _isActive = true;
    private string? _notes;

    public TestTypeSampleTubeRowViewModel()
    {
    }

    public TestTypeSampleTubeRowViewModel(TestTypeSampleTube tube)
    {
        _testTypeTubeId = tube.TestTypeTubeId;
        _testTypeId = tube.TestTypeId;
        _tubeType = tube.TubeType;
        _tubeColor = tube.TubeColor;
        _sampleType = tube.SampleType;
        _quantity = tube.Quantity;
        _sortOrder = tube.SortOrder;
        _isActive = tube.IsActive;
        _notes = tube.Notes;
    }

    public int TestTypeTubeId
    {
        get => _testTypeTubeId;
        set => SetProperty(ref _testTypeTubeId, value);
    }

    public int TestTypeId
    {
        get => _testTypeId;
        set => SetProperty(ref _testTypeId, value);
    }

    public string TubeType
    {
        get => _tubeType;
        set => SetProperty(ref _tubeType, value);
    }

    public string? TubeColor
    {
        get => _tubeColor;
        set => SetProperty(ref _tubeColor, value);
    }

    public string? SampleType
    {
        get => _sampleType;
        set => SetProperty(ref _sampleType, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value < 1 ? 1 : value);
    }

    public short SortOrder
    {
        get => _sortOrder;
        set => SetProperty(ref _sortOrder, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public TestTypeSampleTube ToEntity()
    {
        return new TestTypeSampleTube
        {
            TestTypeTubeId = TestTypeTubeId,
            TestTypeId = TestTypeId,
            TubeType = string.IsNullOrWhiteSpace(TubeType) ? "Default" : TubeType.Trim(),
            TubeColor = string.IsNullOrWhiteSpace(TubeColor) ? null : TubeColor.Trim(),
            SampleType = string.IsNullOrWhiteSpace(SampleType) ? null : SampleType.Trim(),
            Quantity = Quantity,
            SortOrder = SortOrder,
            IsActive = IsActive,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
        };
    }
}

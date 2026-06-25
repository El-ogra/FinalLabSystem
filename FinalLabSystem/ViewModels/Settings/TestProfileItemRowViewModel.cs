using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestProfileItemRowViewModel : Infrastructure.ViewModelBase
{
    private int? _sortOrder;

    public int ProfileItemId { get; }

    public int TestTypeId { get; }

    public string TestTypeName { get; }

    public string TestTypeCode { get; }

    public int? SortOrder
    {
        get => _sortOrder;
        set => SetProperty(ref _sortOrder, value);
    }

    public TestProfileItemRowViewModel(TestProfileItem item)
    {
        ProfileItemId = item.ProfileItemId;
        TestTypeId = item.TestTypeId;
        TestTypeName = item.TestType?.TypeNameEn ?? string.Empty;
        TestTypeCode = item.TestType?.TypeCode ?? string.Empty;
        _sortOrder = item.SortOrder;
    }
}

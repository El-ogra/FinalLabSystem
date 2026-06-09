using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class GroupDetailViewModel : ViewModelBase
{
    private string _editableGroupCode = string.Empty;
    private string _editableGroupNameEn = string.Empty;
    private string? _editableGroupNameAr;
    private int _editableSortOrder;
    private bool _editableIsActive = true;
    private int? _editableSelectedCategoryId;
    private bool _isDirty;
    private bool _isNewRecord;
    private int? _editingGroupId;

    public event EventHandler? DirtyStateChanged;

    public ObservableCollection<CategoryRowViewModel> AvailableCategories { get; } = new();

    public string EditableGroupCode
    {
        get => _editableGroupCode;
        set
        {
            if (SetProperty(ref _editableGroupCode, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string EditableGroupNameEn
    {
        get => _editableGroupNameEn;
        set
        {
            if (SetProperty(ref _editableGroupNameEn, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string? EditableGroupNameAr
    {
        get => _editableGroupNameAr;
        set
        {
            if (SetProperty(ref _editableGroupNameAr, value))
                MarkDirty();
        }
    }

    public int EditableSortOrder
    {
        get => _editableSortOrder;
        set
        {
            if (SetProperty(ref _editableSortOrder, value))
                MarkDirty();
        }
    }

    public bool EditableIsActive
    {
        get => _editableIsActive;
        set
        {
            if (SetProperty(ref _editableIsActive, value))
                MarkDirty();
        }
    }

    public int? EditableSelectedCategoryId
    {
        get => _editableSelectedCategoryId;
        set
        {
            if (SetProperty(ref _editableSelectedCategoryId, value))
                MarkDirty();
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (SetProperty(ref _isDirty, value))
                DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsNewRecord
    {
        get => _isNewRecord;
        private set => SetProperty(ref _isNewRecord, value);
    }

    public int? EditingGroupId
    {
        get => _editingGroupId;
        private set => SetProperty(ref _editingGroupId, value);
    }

    public void LoadAvailableCategories(IEnumerable<CategoryRowViewModel> categories)
    {
        AvailableCategories.Clear();
        foreach (var cat in categories)
            AvailableCategories.Add(cat);
    }

    public void Load(GroupRowViewModel row)
    {
        EditableGroupCode = row.GroupCode;
        EditableGroupNameEn = row.GroupNameEn;
        EditableGroupNameAr = row.GroupNameAr;
        EditableSortOrder = row.SortOrder;
        EditableIsActive = row.IsActive;
        EditableSelectedCategoryId = row.CategoryId;
        EditingGroupId = row.GroupId;
        IsNewRecord = false;
        IsDirty = false;
    }

    public void StartNew(int? preselectedCategoryId = null)
    {
        _editableGroupCode = string.Empty;
        _editableGroupNameEn = string.Empty;
        _editableGroupNameAr = null;
        _editableSortOrder = 0;
        _editableIsActive = true;
        _editableSelectedCategoryId = preselectedCategoryId;
        EditingGroupId = null;
        IsNewRecord = true;
        IsDirty = false;
    }

    public void Clear()
    {
        _editableGroupCode = string.Empty;
        _editableGroupNameEn = string.Empty;
        _editableGroupNameAr = null;
        _editableSortOrder = 0;
        _editableIsActive = true;
        _editableSelectedCategoryId = null;
        EditingGroupId = null;
        IsNewRecord = false;
        IsDirty = false;
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(EditableGroupCode))
            return false;

        if (string.IsNullOrWhiteSpace(EditableGroupNameEn))
            return false;

        if (EditableSelectedCategoryId is null)
            return false;

        return true;
    }

    public TestGroup BuildEntity()
    {
        return new TestGroup
        {
            GroupId = EditingGroupId ?? 0,
            CategoryId = EditableSelectedCategoryId ?? 0,
            GroupCode = EditableGroupCode,
            GroupNameEn = EditableGroupNameEn,
            GroupNameAr = EditableGroupNameAr,
            SortOrder = (short)EditableSortOrder,
            IsActive = EditableIsActive
        };
    }

    public void AcceptChanges()
    {
        IsDirty = false;
    }

    private void MarkDirty()
    {
        IsDirty = true;
    }
}

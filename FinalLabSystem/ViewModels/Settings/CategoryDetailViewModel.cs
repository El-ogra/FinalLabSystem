using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CategoryDetailViewModel : ViewModelBase
{
    private string _editableCategoryCode = string.Empty;
    private string _editableCategoryNameEn = string.Empty;
    private string _editableCategoryNameAr = string.Empty;
    private int _editableSortOrder;
    private bool _editableIsActive = true;
    private bool _isDirty;
    private bool _isNewRecord;
    private int? _editingCategoryId;

    public event EventHandler? DirtyStateChanged;

    public string EditableCategoryCode
    {
        get => _editableCategoryCode;
        set
        {
            if (SetProperty(ref _editableCategoryCode, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string EditableCategoryNameEn
    {
        get => _editableCategoryNameEn;
        set
        {
            if (SetProperty(ref _editableCategoryNameEn, value ?? string.Empty))
                MarkDirty();
        }
    }

    public string EditableCategoryNameAr
    {
        get => _editableCategoryNameAr;
        set
        {
            if (SetProperty(ref _editableCategoryNameAr, value ?? string.Empty))
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

    public int? EditingCategoryId
    {
        get => _editingCategoryId;
        private set => SetProperty(ref _editingCategoryId, value);
    }

    public void Load(CategoryRowViewModel row)
    {
        EditableCategoryCode = row.CategoryCode;
        EditableCategoryNameEn = row.CategoryNameEn;
        EditableCategoryNameAr = row.CategoryNameAr;
        EditableSortOrder = row.SortOrder;
        EditableIsActive = row.IsActive;
        EditingCategoryId = row.CategoryId;
        IsNewRecord = false;
        IsDirty = false;
    }

    public void StartNew()
    {
        EditableCategoryCode = string.Empty;
        EditableCategoryNameEn = string.Empty;
        EditableCategoryNameAr = string.Empty;
        EditableSortOrder = 0;
        EditableIsActive = true;
        EditingCategoryId = null;
        IsNewRecord = true;
        IsDirty = false;
    }

    public void Clear()
    {
        EditableCategoryCode = string.Empty;
        EditableCategoryNameEn = string.Empty;
        EditableCategoryNameAr = string.Empty;
        EditableSortOrder = 0;
        EditableIsActive = true;
        EditingCategoryId = null;
        IsNewRecord = false;
        IsDirty = false;
    }

    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(EditableCategoryCode)
            && !string.IsNullOrWhiteSpace(EditableCategoryNameEn);
    }

    public TestCategory BuildEntity()
    {
        return new TestCategory
        {
            CategoryId = EditingCategoryId ?? 0,
            CategoryCode = EditableCategoryCode,
            CategoryNameEn = EditableCategoryNameEn,
            CategoryNameAr = EditableCategoryNameAr,
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

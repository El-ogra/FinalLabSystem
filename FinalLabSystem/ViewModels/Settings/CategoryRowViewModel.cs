using System.Linq;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CategoryRowViewModel : ViewModelBase
{
    public CategoryRowViewModel(TestCategory testCategory)
    {
        TestCategory = testCategory;
    }

    public TestCategory TestCategory { get; }

    public int CategoryId => TestCategory.CategoryId;

    public string CategoryCode => TestCategory.CategoryCode;

    public string CategoryNameEn => TestCategory.CategoryNameEn;

    public string CategoryNameAr => TestCategory.CategoryNameAr ?? string.Empty;

    public int SortOrder => TestCategory.SortOrder;

    public bool IsActive => TestCategory.IsActive;
}

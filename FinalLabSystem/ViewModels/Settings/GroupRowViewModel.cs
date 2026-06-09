using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class GroupRowViewModel : ViewModelBase
{
    public GroupRowViewModel(TestGroup testGroup)
    {
        TestGroup = testGroup;
    }

    public TestGroup TestGroup { get; }

    public int GroupId => TestGroup.GroupId;

    public int CategoryId => TestGroup.CategoryId;

    public string GroupCode => TestGroup.GroupCode;

    public string GroupNameEn => TestGroup.GroupNameEn;

    public string GroupNameAr => TestGroup.GroupNameAr ?? string.Empty;

    public int SortOrder => TestGroup.SortOrder;

    public bool IsActive => TestGroup.IsActive;
}

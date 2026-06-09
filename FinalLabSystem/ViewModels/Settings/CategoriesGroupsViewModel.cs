using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CategoriesGroupsViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly INavigationService _navigationService;

    public CategoriesGroupsViewModel(
        CategoryListViewModel categoryList,
        CategoryDetailViewModel categoryDetail,
        GroupListViewModel groupList,
        GroupDetailViewModel groupDetail,
        ITestCatalogService testCatalogService,
        INavigationService navigationService)
    {
        CategoryList = categoryList;
        CategoryDetail = categoryDetail;
        GroupList = groupList;
        GroupDetail = groupDetail;
        _testCatalogService = testCatalogService;
        _navigationService = navigationService;

        NewCategoryCommand = new RelayCommand(_ => NewCategory());
        SaveCategoryCommand = new AsyncRelayCommand(SaveCategoryAsync, () => CategoryDetail.IsDirty && CategoryDetail.Validate());
        DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync, () => CategoryDetail.EditingCategoryId != null && !CategoryDetail.IsNewRecord);
        NewGroupCommand = new RelayCommand(_ => NewGroup(), _ => CategoryList.SelectedCategory != null);
        SaveGroupCommand = new AsyncRelayCommand(SaveGroupAsync, () => GroupDetail.IsDirty && GroupDetail.Validate());
        DeleteGroupCommand = new AsyncRelayCommand(DeleteGroupAsync, () => GroupDetail.EditingGroupId != null && !GroupDetail.IsNewRecord);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CloseCommand = new RelayCommand(parameter =>
        {
            if (parameter is Window window)
                window.Close();
        });

        categoryList.SelectedCategoryChanged += OnSelectedCategoryChanged;
        categoryDetail.DirtyStateChanged += (_, _) => CommandManager.InvalidateRequerySuggested();
        groupList.SelectedGroupChanged += OnSelectedGroupChanged;
        groupDetail.DirtyStateChanged += (_, _) => CommandManager.InvalidateRequerySuggested();
    }

    public CategoryListViewModel CategoryList { get; }

    public CategoryDetailViewModel CategoryDetail { get; }

    public GroupListViewModel GroupList { get; }

    public GroupDetailViewModel GroupDetail { get; }

    public ICommand NewCategoryCommand { get; }

    public ICommand SaveCategoryCommand { get; }

    public ICommand DeleteCategoryCommand { get; }

    public ICommand NewGroupCommand { get; }

    public ICommand SaveGroupCommand { get; }

    public ICommand DeleteGroupCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand CloseCommand { get; }

    public async Task InitializeAsync()
    {
        await CategoryList.RefreshAsync();
        CategoryDetail.Clear();
        GroupList.Clear();
        GroupDetail.Clear();
    }

    private void NewCategory()
    {
        CategoryDetail.StartNew();
        GroupList.Clear();
        GroupDetail.Clear();
    }

    private async Task SaveCategoryAsync()
    {
        var entity = CategoryDetail.BuildEntity();
        if (CategoryDetail.IsNewRecord)
        {
            var created = await _testCatalogService.CreateCategoryAsync(entity);
            await CategoryList.RefreshAsync();
            foreach (var cat in CategoryList.AllCategories)
            {
                if (cat.CategoryCode == created.CategoryCode)
                {
                    CategoryList.SelectedCategory = cat;
                    break;
                }
            }
        }
        else
        {
            await _testCatalogService.UpdateCategoryAsync(entity);
            await CategoryList.RefreshAsync();
            CategoryDetail.AcceptChanges();
        }

        GroupDetail.LoadAvailableCategories(CategoryList.AllCategories);
    }

    private async Task DeleteCategoryAsync()
    {
        try
        {
            await _testCatalogService.DeleteCategoryAsync(CategoryDetail.EditingCategoryId!.Value);
            await CategoryList.RefreshAsync();
            CategoryDetail.Clear();
            GroupList.Clear();
            GroupDetail.Clear();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void NewGroup()
    {
        GroupDetail.StartNew(CategoryList.SelectedCategory?.CategoryId);
        GroupDetail.LoadAvailableCategories(CategoryList.AllCategories);
    }

    private async Task SaveGroupAsync()
    {
        var entity = GroupDetail.BuildEntity();
        if (GroupDetail.IsNewRecord)
        {
            await _testCatalogService.CreateGroupAsync(entity);
            await GroupList.RefreshCurrentCategoryAsync();
        }
        else
        {
            await _testCatalogService.UpdateGroupAsync(entity);
            await GroupList.RefreshCurrentCategoryAsync();
        }

        GroupDetail.AcceptChanges();
    }

    private async Task DeleteGroupAsync()
    {
        await _testCatalogService.DeleteGroupAsync(GroupDetail.EditingGroupId!.Value);
        await GroupList.RefreshCurrentCategoryAsync();
        GroupDetail.Clear();
    }

    private async Task RefreshAsync()
    {
        await CategoryList.RefreshAsync();
        CategoryDetail.Clear();
        GroupList.Clear();
        GroupDetail.Clear();
    }

    private async void OnSelectedCategoryChanged(object? sender, CategoryRowViewModel? row)
    {
        if (row is null)
        {
            CategoryDetail.Clear();
            GroupList.Clear();
            GroupDetail.Clear();
            return;
        }

        CategoryDetail.Load(row);
        await GroupList.RefreshAsync(row.CategoryId);
        GroupDetail.Clear();
        GroupDetail.LoadAvailableCategories(CategoryList.AllCategories);
    }

    private void OnSelectedGroupChanged(object? sender, GroupRowViewModel? row)
    {
        if (row is null)
        {
            GroupDetail.Clear();
            return;
        }

        GroupDetail.Load(row);
    }
}

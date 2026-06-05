using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestDataManagementViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly INavigationService _navigationService;

    public TestDataManagementViewModel(
        TestListViewModel testList,
        TestDetailViewModel testDetail,
        ITestCatalogService testCatalogService,
        INavigationService navigationService)
    {
        TestList = testList;
        TestDetail = testDetail;
        _testCatalogService = testCatalogService;
        _navigationService = navigationService;

        NewCommand = new RelayCommand(_ => New());
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => TestDetail.IsDirty);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => TestDetail.TesttypeId > 0);
        CloseCommand = new RelayCommand(_ => _navigationService.ReturnToMain());

        TestList.SelectedTestChanged += OnSelectedTestChanged;
        TestDetail.DirtyStateChanged += (_, _) => System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        TestDetail.OpenNormalRangesRequested += OnOpenNormalRangesRequested;
        _ = InitializeAsync();
    }

    public TestListViewModel TestList { get; }

    public TestDetailViewModel TestDetail { get; }

    public ICommand NewCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand CloseCommand { get; }

    private async Task InitializeAsync()
    {
        await TestDetail.InitializeLookupsAsync();
        await TestList.RefreshAsync();
        TestDetail.StartNew(TestList.AllTests.ToList());
    }

    private void New()
    {
        TestList.SelectedTest = null;
        TestDetail.StartNew(TestList.AllTests.ToList());
    }

    private async Task SaveAsync()
    {
        if (!TestDetail.Validate())
            return;

        var entity = TestDetail.BuildEntity();
        if (entity.TesttypeId == 0)
        {
            var id = await _testCatalogService.CreateTestTypeAsync(
                entity,
                TestDetail.PatientPrice,
                TestDetail.LabToLabPrice,
                TestDetail.BuildTubes());
            await TestDetail.LoadAsync(id, TestList.AllTests.ToList());
        }
        else
        {
            await _testCatalogService.UpdateTestTypeAsync(
                entity,
                TestDetail.PatientPrice,
                TestDetail.LabToLabPrice,
                TestDetail.BuildTubes());
            TestDetail.AcceptChanges();
        }

        await TestList.RefreshAsync();
    }

    private async Task DeleteAsync()
    {
        if (TestDetail.TesttypeId <= 0)
            return;

        var result = MessageBox.Show("هل تريد حذف هذا التحليل؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
            return;

        await _testCatalogService.DeleteTestTypeAsync(TestDetail.TesttypeId);
        await TestList.RefreshAsync();
        TestDetail.StartNew(TestList.AllTests.ToList());
    }

    private async void OnSelectedTestChanged(object? sender, TestRowViewModel? row)
    {
        if (row is null)
            return;

        await TestDetail.LoadAsync(row.TesttypeId, TestList.AllTests.ToList());
    }

    private async void OnOpenNormalRangesRequested(object? sender, EventArgs e)
    {
        if (TestDetail.EditableTest.TesttypeId <= 0)
        {
            MessageBox.Show("احفظ التحليل أولاً قبل إدخال القيم الطبيعية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var window = App.ServiceProvider.GetRequiredService<NormalRangesWindow>();
        if (window.DataContext is NormalRangeWindowViewModel vm)
            await vm.InitializeAsync(TestDetail.EditableTest);

        window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        window.ShowDialog();
    }
}

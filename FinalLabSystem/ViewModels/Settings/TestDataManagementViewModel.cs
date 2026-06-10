using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestDataManagementViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<TestDataManagementViewModel> _logger;
    private readonly IDialogService _dialogService;
    private bool _isAdding;
    private bool _isEditing;
    private bool _isBrowsing = true;
    private ICommand? _editCommand;
    private ICommand? _cancelCommand;

    public TestDataManagementViewModel(
        TestListViewModel testList,
        TestDetailViewModel testDetail,
        ITestCatalogService testCatalogService,
        INavigationService navigationService,
        ILogger<TestDataManagementViewModel> logger,
        IDialogService dialogService)
    {
        TestList = testList;
        TestDetail = testDetail;
        _testCatalogService = testCatalogService;
        _navigationService = navigationService;
        _logger = logger;
        _dialogService = dialogService;

        AddCommand = new RelayCommand(_ => Add(), _ => IsBrowsing);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => (IsAdding || IsEditing) && TestDetail.IsDirty);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => IsBrowsing && TestDetail.TesttypeId > 0);
        CloseCommand = new RelayCommand(_ => _navigationService.ReturnToMain());

        TestList.SelectedTestChanged += OnSelectedTestChanged;
        TestDetail.DirtyStateChanged += (_, _) => System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        TestDetail.OpenNormalRangesRequested += OnOpenNormalRangesRequested;
    }

    public TestListViewModel TestList { get; }

    public TestDetailViewModel TestDetail { get; }

    public ICommand AddCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand EditCommand => _editCommand ??= new RelayCommand(_ => Edit(), _ => IsBrowsing && TestList.SelectedTest is not null);

    public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(_ => Cancel());

    public bool IsAdding
    {
        get => _isAdding;
        set => SetProperty(ref _isAdding, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public bool IsBrowsing
    {
        get => _isBrowsing;
        set => SetProperty(ref _isBrowsing, value);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await TestDetail.InitializeLookupsAsync();
            await TestList.RefreshAsync();
            TestDetail.StartNew(TestList.AllTests.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initialization failed in {VM}", GetType().Name);
            _dialogService.ShowError("حدث خطأ أثناء تهيئة النموذج.");
        }
    }

    private void Add()
    {
        TestList.SelectedTest = null;
        TestDetail.StartNew(TestList.AllTests.ToList());
        IsAdding = true;
        IsBrowsing = false;
        IsEditing = false;
    }

    private void Edit()
    {
        if (TestList.SelectedTest is null)
            return;

        TestDetail.SaveBaseline();
        IsEditing = true;
        IsBrowsing = false;
        IsAdding = false;
    }

    private void Cancel()
    {
        TestDetail.CancelChanges();
        IsBrowsing = true;
        IsAdding = false;
        IsEditing = false;
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
        IsBrowsing = true;
        IsAdding = false;
        IsEditing = false;
    }

    private async Task DeleteAsync()
    {
        if (TestDetail.TesttypeId <= 0)
            return;

        if (!_dialogService.ShowConfirmation("هل تريد حذف هذا التحليل؟", "تأكيد الحذف"))
            return;

        await _testCatalogService.DeleteTestTypeAsync(TestDetail.TesttypeId);
        await TestList.RefreshAsync();
        TestDetail.StartNew(TestList.AllTests.ToList());
        IsBrowsing = true;
        IsAdding = false;
        IsEditing = false;
    }

    private async void OnSelectedTestChanged(object? sender, TestRowViewModel? row)
    {
        try
        {
            if (row is null)
                return;

            await TestDetail.LoadAsync(row.TesttypeId, TestList.AllTests.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnSelectedTestChanged");
            _dialogService.ShowError("حدث خطأ أثناء تحميل البيانات.");
        }
    }

    private async void OnOpenNormalRangesRequested(object? sender, EventArgs e)
    {
        try
        {
            if (TestDetail.EditableTest.TesttypeId <= 0)
            {
                _dialogService.ShowMessage("احفظ التحليل أولاً قبل إدخال القيم الطبيعية.", "تنبيه");
                return;
            }

            var window = App.ServiceProvider.GetRequiredService<NormalRangesWindow>();
            if (window.DataContext is NormalRangeWindowViewModel vm)
                await vm.InitializeAsync(TestDetail.EditableTest);

            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnOpenNormalRangesRequested");
            _dialogService.ShowError("حدث خطأ أثناء تحميل البيانات.");
        }
    }
}

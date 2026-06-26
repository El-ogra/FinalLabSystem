using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CompaniesWindowViewModel : ViewModelBase
{
    private readonly ICompanyService _companyService;
    private CompanyRowViewModel? _selectedCompany;
    private string _title = "إدارة الشركات";
    private bool _isEditing;
    private CompanyRowViewModel _editModel = new();

    public CompaniesWindowViewModel(ICompanyService companyService)
    {
        _companyService = companyService;

        Companies = new ObservableCollection<CompanyRowViewModel>();

        AddCommand = new AsyncRelayCommand(ExecuteAddAsync);
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, () => IsEditing);
        CancelCommand = new RelayCommand(_ => CancelEdit());
        RefreshCommand = new AsyncRelayCommand(ExecuteRefreshAsync);
        DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync, () => SelectedCompany is not null);
    }

    public ObservableCollection<CompanyRowViewModel> Companies { get; }

    public CompanyRowViewModel? SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            if (SetProperty(ref _selectedCompany, value) && value is not null)
                LoadEditFromSelection();
        }
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    public CompanyRowViewModel EditModel
    {
        get => _editModel;
        private set => SetProperty(ref _editModel, value);
    }

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task LoadAsync()
    {
        var companies = await _companyService.GetAllAsync();
        Companies.Clear();
        foreach (var c in companies)
        {
            var vm = new CompanyRowViewModel();
            vm.LoadFromModel(c);
            Companies.Add(vm);
        }
    }

    private Task ExecuteAddAsync()
    {
        var newCompany = new CompanyRowViewModel
        {
            CompanyType = "CORPORATE",
            IsActive = true
        };
        EditModel = newCompany;
        IsEditing = true;
        Title = "إضافة شركة جديدة";
        return Task.CompletedTask;
    }

    private async Task ExecuteSaveAsync()
    {
        try
        {
            var model = EditModel.ToModel();

            if (model.CompanyId == 0)
            {
                var created = await _companyService.CreateAsync(model);
                var vm = new CompanyRowViewModel();
                vm.LoadFromModel(created);
                Companies.Add(vm);
            }
            else
            {
                await _companyService.UpdateAsync(model);
                var existing = Companies.FirstOrDefault(c => c.CompanyId == model.CompanyId);
                if (existing is not null)
                    existing.LoadFromModel(model);
            }

            IsEditing = false;
            Title = "إدارة الشركات";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"خطأ في الحفظ: {ex.Message}", "خطأ", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        Title = "إدارة الشركات";
        EditModel = new CompanyRowViewModel();
    }

    private async Task ExecuteRefreshAsync()
    {
        await LoadAsync();
    }

    private async Task ExecuteDeleteAsync()
    {
        if (SelectedCompany is null) return;

        var result = System.Windows.MessageBox.Show(
            $"هل أنت متأكد من تعطيل الشركة \"{SelectedCompany.CompanyName}\"؟",
            "تأكيد التعطيل",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        SelectedCompany.IsActive = false;
        await _companyService.UpdateAsync(SelectedCompany.ToModel());
    }

    private void LoadEditFromSelection()
    {
        if (SelectedCompany is null) return;

        var copy = new CompanyRowViewModel();
        copy.LoadFromModel(SelectedCompany.ToModel());
        EditModel = copy;
        IsEditing = true;
        Title = $"تعديل — {SelectedCompany.CompanyName}";
    }
}

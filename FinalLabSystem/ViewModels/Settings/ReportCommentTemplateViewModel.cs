using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ReportCommentTemplateViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IReportCommentTemplateService _templateService;
    private readonly ITestCatalogService _testCatalogService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IDialogService _dialogService;

    private ObservableCollection<ReportCommentTemplate> _templates = new();
    private ReportCommentTemplate? _selectedTemplate;

    private string _editableTitle = string.Empty;
    private string _editableCommentText = string.Empty;
    private string _editableCommentLang = "AR";
    private string _editableTriggerCondition = "Manual";
    private int? _editableCategoryId;
    private int? _editableTesttypeId;
    private int? _editableComponentId;
    private short _editableSortOrder;
    private bool _isEditing;

    public ReportCommentTemplateViewModel(
        IReportCommentTemplateService templateService,
        ITestCatalogService testCatalogService,
        ICurrentUserSession currentUserSession,
        IDialogService dialogService)
    {
        _templateService = templateService;
        _testCatalogService = testCatalogService;
        _currentUserSession = currentUserSession;
        _dialogService = dialogService;

        NewCommand = new RelayCommand(_ => StartNew());
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => IsEditing && !string.IsNullOrWhiteSpace(EditableTitle));
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedTemplate != null);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        CloseCommand = new RelayCommand(parameter =>
        {
            if (parameter is Window window)
                window.Close();
        });
    }

    public ObservableCollection<ReportCommentTemplate> Templates
    {
        get => _templates;
        set => SetProperty(ref _templates, value);
    }

    public ReportCommentTemplate? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (SetProperty(ref _selectedTemplate, value) && value != null)
                LoadTemplate(value);
        }
    }

    public string EditableTitle
    {
        get => _editableTitle;
        set => SetProperty(ref _editableTitle, value);
    }

    public string EditableCommentText
    {
        get => _editableCommentText;
        set => SetProperty(ref _editableCommentText, value);
    }

    public string EditableCommentLang
    {
        get => _editableCommentLang;
        set => SetProperty(ref _editableCommentLang, value);
    }

    public string EditableTriggerCondition
    {
        get => _editableTriggerCondition;
        set => SetProperty(ref _editableTriggerCondition, value);
    }

    public int? EditableCategoryId
    {
        get => _editableCategoryId;
        set
        {
            if (SetProperty(ref _editableCategoryId, value))
                _ = LoadTestTypesAsync();
        }
    }

    public int? EditableTesttypeId
    {
        get => _editableTesttypeId;
        set
        {
            if (SetProperty(ref _editableTesttypeId, value))
                _ = LoadComponentsAsync();
        }
    }

    public int? EditableComponentId
    {
        get => _editableComponentId;
        set => SetProperty(ref _editableComponentId, value);
    }

    public short EditableSortOrder
    {
        get => _editableSortOrder;
        set => SetProperty(ref _editableSortOrder, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public ObservableCollection<TestCategory> AvailableCategories { get; } = new();
    public ObservableCollection<TestType> AvailableTestTypes { get; } = new();
    public ObservableCollection<TestComponent> AvailableComponents { get; } = new();

    public List<string> AvailableLanguages { get; } = new() { "AR", "EN" };
    public List<string> AvailableTriggers { get; } = Enum.GetNames<ReportCommentTrigger>().ToList();

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }

    public async Task InitializeAsync()
    {
        await LoadTemplatesAsync();
        await LoadCategoriesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        var templates = await _templateService.GetActiveTemplatesAsync();
        Templates = new ObservableCollection<ReportCommentTemplate>(templates);
    }

    private async Task LoadCategoriesAsync()
    {
        AvailableCategories.Clear();
        var hierarchy = await _testCatalogService.GetFullHierarchyAsync();
        foreach (var cat in hierarchy)
            AvailableCategories.Add(cat);
    }

    private async Task LoadTestTypesAsync()
    {
        AvailableTestTypes.Clear();
        AvailableComponents.Clear();
        EditableComponentId = null;

        var allTests = await _testCatalogService.GetAllTestTypesAsync();
        foreach (var test in allTests)
            AvailableTestTypes.Add(test);
    }

    private async Task LoadComponentsAsync()
    {
        AvailableComponents.Clear();
        EditableComponentId = null;

        if (EditableTesttypeId == null) return;

        var testType = await _testCatalogService.GetTestTypeDetailsAsync(EditableTesttypeId.Value);
        if (testType?.TestComponents != null)
        {
            foreach (var comp in testType.TestComponents)
                AvailableComponents.Add(comp);
        }
    }

    private void StartNew()
    {
        SelectedTemplate = null;
        EditableTitle = string.Empty;
        EditableCommentText = string.Empty;
        EditableCommentLang = "AR";
        EditableTriggerCondition = "Manual";
        EditableCategoryId = null;
        EditableTesttypeId = null;
        EditableComponentId = null;
        EditableSortOrder = 0;
        IsEditing = true;
    }

    private void LoadTemplate(ReportCommentTemplate template)
    {
        EditableTitle = template.Title;
        EditableCommentText = template.CommentText;
        EditableCommentLang = template.CommentLang;
        EditableTriggerCondition = template.TriggerCondition ?? "Manual";
        EditableCategoryId = template.CategoryId;
        EditableTesttypeId = template.TesttypeId;
        EditableComponentId = template.ComponentId;
        EditableSortOrder = template.SortOrder;
        IsEditing = true;
    }

    private async Task SaveAsync()
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId;

        if (SelectedTemplate != null)
        {
            SelectedTemplate.Title = EditableTitle;
            SelectedTemplate.CommentText = EditableCommentText;
            SelectedTemplate.CommentLang = EditableCommentLang;
            SelectedTemplate.TriggerCondition = EditableTriggerCondition;
            SelectedTemplate.CategoryId = EditableCategoryId;
            SelectedTemplate.TesttypeId = EditableTesttypeId;
            SelectedTemplate.ComponentId = EditableComponentId;
            SelectedTemplate.SortOrder = EditableSortOrder;
            SelectedTemplate.ModifiedBy = staffId;

            await _templateService.UpdateTemplateAsync(SelectedTemplate);
        }
        else
        {
            var newTemplate = new ReportCommentTemplate
            {
                Title = EditableTitle,
                CommentText = EditableCommentText,
                CommentLang = EditableCommentLang,
                TriggerCondition = EditableTriggerCondition,
                CategoryId = EditableCategoryId,
                TesttypeId = EditableTesttypeId,
                ComponentId = EditableComponentId,
                SortOrder = EditableSortOrder,
                CreatedBy = staffId
            };

            await _templateService.CreateTemplateAsync(newTemplate);
        }

        await LoadTemplatesAsync();
        IsEditing = false;
        _dialogService.ShowMessage("تم الحفظ بنجاح", "حفظ");
    }

    private async Task DeleteAsync()
    {
        if (SelectedTemplate == null) return;

        if (!_dialogService.ShowConfirmation("هل أنت متأكد من حذف هذا القالب؟", "تأكيد الحذف"))
            return;

        await _templateService.DeleteTemplateAsync(SelectedTemplate.TemplateId);
        await LoadTemplatesAsync();
        IsEditing = false;
        _dialogService.ShowMessage("تم الحذف بنجاح", "حذف");
    }

    private async Task RefreshAsync()
    {
        await LoadTemplatesAsync();
        IsEditing = false;
    }
}

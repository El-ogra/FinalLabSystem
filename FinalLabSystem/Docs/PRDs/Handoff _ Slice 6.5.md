===BEGIN FILE CONTENT===

Handoff — Slice 6.5: Print Queue / Batch Printing (PRD Task 6.6)
SECTION 1 — SLICE IDENTITY
Field	Value
Slice Number	6.5
Slice Name	Print Queue / Batch Printing
PRD Task	6.6
Goal	Enable batch printing of multiple documents (receipts, reports, invoices) via an in-memory print queue, with progress reporting, cancellation, and per-item failure handling.
In-Scope Files (8 files total)
New Files (6):

FinalLabSystem/Models/Enums/PrintQueueItemStatus.cs
FinalLabSystem/Models/DTOs/PrintQueueItemDto.cs
FinalLabSystem/Services/Interfaces/IPrintQueueService.cs
FinalLabSystem/Services/Implementations/PrintQueueService.cs
FinalLabSystem/ViewModels/Settings/PrintQueueWindowViewModel.cs
FinalLabSystem/Views/Settings/PrintQueueWindow.xaml + FinalLabSystem/Views/Settings/PrintQueueWindow.xaml.cs
Modified Files (3): 7. FinalLabSystem/ViewModels/Patients/TestResultsViewModel.cs 8. FinalLabSystem/Views/Patients/TestResultsWindow.xaml 9. FinalLabSystem/App.xaml.cs

Explicit Out-of-Scope (DO NOT touch)
FinalLabSystem/Services/Interfaces/IPrintService.cs — complete from Slice 6.1
FinalLabSystem/Services/Implementations/WpfFlowDocumentPrintService.cs
FinalLabSystem/Services/Interfaces/IPrintPreviewDialogService.cs
FinalLabSystem/Services/Implementations/PrintPreviewDialogService.cs
FinalLabSystem/ViewModels/Patients/PrintPreviewViewModel.cs
FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml
All test files not listed in Section 7
All ViewModels not listed in Section 5
SharedStyles.xaml, SharedConverters.xaml
Any file not explicitly listed in Section 4 or Section 5
SECTION 2 — PREREQUISITE STATE
Slice 6.4 must be complete and all its tests passing. The implementing agent MUST verify the following before writing a single line of code:

 G6.4 passes: Run the full test suite; total must be ≥ 615 passing tests (the pre-6.5 baseline).
 IPrintService exists and is correct: FinalLabSystem/Services/Interfaces/IPrintService.cs contains exactly two methods:
Task PrintAsync(string documentType, object data) (accepts object, not FlowDocument)
Task PrintFlowDocumentAsync(FlowDocument document, string description)
 IPrintService is registered as Scoped: App.xaml.cs line 196 contains services.AddScoped<IPrintService, WpfFlowDocumentPrintService>();
 INavigationService.OpenTaskWindow() is available: Verified in INavigationService.cs with at least the generic overload void OpenTaskWindow<TViewModel>(Action<TViewModel>? configure = null).
 CommunityToolkit.Mvvm is available: RelayCommand and AsyncRelayCommand types are usable (via global usings or explicit imports).
 TestResultsViewModel.cs is intact: 949 lines, 12 constructor dependencies (including IPrintService and INavigationService), 38 commands.
 TestResultsWindow.xaml bottom toolbar exists: WrapPanel inside DockPanel at lines 69–109 containing existing bottom bar buttons.
 TestResultsWindow.xaml InputBindings block exists: Lines 818–831 containing F2–F12, Ctrl+R, Ctrl+F, Ctrl+P, Escape bindings.
SECTION 3 — CONFLICT CLEARANCE & KEYBOARD SAFETY
3a. Keyboard Shortcut Clearance Certificate
Ctrl+Q: Checked against every XAML file in the project and every .xaml.cs code-behind file. No Key="Q" with Modifiers="Ctrl" found anywhere. AVAILABLE.

Ctrl+Shift+Q: Checked against every XAML file and code-behind. No Key="Q" with Modifiers="Ctrl+Shift" found anywhere. AVAILABLE.

Evidence:

TestResultsWindow.xaml lines 818–831: only F2–F8, F12, Ctrl+R (line 827), Ctrl+F (line 828), Ctrl+P (line 829), Escape.
PatientRegistrationWindow.xaml lines 163–177: only F1–F12, Escape.
MainWindow.xaml: zero InputBindings.
All other XAML files (BarcodeDialog, ResultEntryWindow, PrintPreviewWindow, DeliveryWindow, PatientSearchWindow, ReceiptDialog, TodayPatientsDialog, AuditTrailWindow, LoginWindow, all Settings windows): zero keyboard shortcuts.
TestResultsWindow.xaml.cs lines 72–88: bare Ctrl handler (see 3b below).
App.xaml.cs lines 87–91: InputManager.Current.PreProcessInput subscription — idle timer only, no key handling.
3b. Bare-Ctrl Handler — Full Explanation
Location: FinalLabSystem/Views/Patients/TestResultsWindow.xaml.cs, lines 72–88.

Behavior: The handler subscribes to KeyDown and checks e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl (bare Control key with no additional modifier). When detected, it toggles keyboard focus between the patient list ListView and the tests DataGrid.

Why it does NOT conflict with Ctrl+Q: WPF processes these as entirely different key events:

Bare Ctrl handler fires when Key == Key.Control (the physical Ctrl key) with no additional key pressed.
Ctrl+Q fires when Key == Key.Q with Modifiers == ModifierKeys.Control.
These are distinct Key enum values (Key.Control vs Key.Q). The bare-Ctrl handler will not intercept Ctrl+Q, and Ctrl+Q will not trigger the bare-Ctrl handler.
Conclusion: No code change needed. No additional research needed by the implementing agent.

SECTION 4 — NEW FILE SPECIFICATIONS
File 1: FinalLabSystem/Models/Enums/PrintQueueItemStatus.cs
namespace FinalLabSystem.Models.Enums;

public enum PrintQueueItemStatus
{
    Pending,
    Printing,
    Done,
    Failed
}
File 2: FinalLabSystem/Models/DTOs/PrintQueueItemDto.cs
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models.DTOs;

public class PrintQueueItemDto
{
    public int VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public PrintQueueItemStatus Status { get; set; } = PrintQueueItemStatus.Pending;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
}
File 3: FinalLabSystem/Services/Interfaces/IPrintQueueService.cs
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IPrintQueueService
{
    void Enqueue(PrintQueueItemDto item);
    void Remove(PrintQueueItemDto item);
    void Clear();
    List<PrintQueueItemDto> GetItems();
    Task PrintAllAsync(IProgress<double>? progress, CancellationToken cancellationToken);
}
File 4: FinalLabSystem/Services/Implementations/PrintQueueService.cs
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.Services.Implementations;

public class PrintQueueService : IPrintQueueService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<PrintQueueItemDto> _items = new();
    private readonly object _lock = new();

    public PrintQueueService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Enqueue(PrintQueueItemDto item)
    {
        lock (_lock)
        {
            item.Status = PrintQueueItemStatus.Pending;
            _items.Add(item);
        }
    }

    public void Remove(PrintQueueItemDto item)
    {
        lock (_lock)
        {
            _items.Remove(item);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _items.Clear();
        }
    }

    public List<PrintQueueItemDto> GetItems()
    {
        lock (_lock)
        {
            return new List<PrintQueueItemDto>(_items);
        }
    }

    public async Task PrintAllAsync(IProgress<double>? progress, CancellationToken cancellationToken)
    {
        PrintQueueItemDto[] snapshot;
        lock (_lock)
        {
            snapshot = _items.ToArray();
        }

        int total = snapshot.Length;
        for (int i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            PrintQueueItemDto item = snapshot[i];
            item.Status = PrintQueueItemStatus.Printing;

            using IServiceScope scope = _scopeFactory.CreateScope();
            IPrintService printService = scope.ServiceProvider.GetRequiredService<IPrintService>();

            try
            {
                await printService.PrintAsync(item.DocumentType, item);
                item.Status = PrintQueueItemStatus.Done;
            }
            catch (Exception ex)
            {
                item.Status = PrintQueueItemStatus.Failed;
                item.Error = ex.Message;
            }

            double percent = total > 0 ? (double)(i + 1) / total * 100.0 : 100.0;
            progress?.Report(percent);
        }
    }
}
Key design points in this file:

Constructor takes IServiceScopeFactory (NOT IPrintService) — avoids stale scoped dependency bug.
Every method that accesses _items uses lock (_lock).
GetItems() returns a defensive copy (new List<> from snapshot).
PrintAllAsync takes a snapshot before iterating — items added during print are not printed.
ThrowIfCancellationRequested() is INSIDE the loop body (line after the for condition).
A new IServiceScope is created per item — each PrintAsync call gets a fresh IPrintService instance.
item itself (the DTO reference) is passed as the object data to PrintAsync. The service modifies Status and Error on this same reference, so the ViewModel sees updated state.
progress?.Report(percent) is called after each item, including after failures.
File 5: FinalLabSystem/ViewModels/Settings/PrintQueueWindowViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class PrintQueueWindowViewModel : ViewModelBase
{
    private readonly IPrintQueueService _printQueueService;
    private readonly IDialogService _dialogService;

    private CancellationTokenSource? _cancellationTokenSource;

    public PrintQueueWindowViewModel(
        IPrintQueueService printQueueService,
        IDialogService dialogService)
    {
        _printQueueService = printQueueService;
        _dialogService = dialogService;

        Items = new ObservableCollection<PrintQueueItemDto>();

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        PrintAllCommand = new AsyncRelayCommand(PrintAllAsync, () => !IsRunning && Items.Count > 0);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, () => SelectedItem != null);
    }

    public ObservableCollection<PrintQueueItemDto> Items { get; }

    private double _progress;
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            SetProperty(ref _isRunning, value);
            PrintAllCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private PrintQueueItemDto? _selectedItem;
    public PrintQueueItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            RemoveSelectedCommand.NotifyCanExecuteChanged();
        }
    }

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand PrintAllCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand RemoveSelectedCommand { get; }

    private async Task LoadAsync()
    {
        Items.Clear();
        foreach (var item in _printQueueService.GetItems())
        {
            Items.Add(item);
        }
        StatusText = $"{Items.Count} items in queue.";
    }

    private async Task PrintAllAsync()
    {
        IsRunning = true;
        Progress = 0;
        StatusText = "Printing...";
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(value =>
            {
                Progress = value;
                StatusText = $"Printing... {value:F0}%";
            });

            await _printQueueService.PrintAllAsync(progress, _cancellationTokenSource.Token);

            int failed = Items.Count(i => i.Status == PrintQueueItemStatus.Failed);
            int done = Items.Count(i => i.Status == PrintQueueItemStatus.Done);
            StatusText = failed > 0
                ? $"Completed. {done} printed, {failed} failed."
                : $"All {done} items printed successfully.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Print cancelled by user.";
        }
        finally
        {
            IsRunning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void RemoveSelected()
    {
        if (SelectedItem == null) return;
        _printQueueService.Remove(SelectedItem);
        Items.Remove(SelectedItem);
        SelectedItem = null;
    }
}
Key design points:

Uses CommunityToolkit.Mvvm pattern: RelayCommand (sync) and AsyncRelayCommand (async), matching the codebase convention.
Progress<double> is instantiated on the UI thread (inside the ViewModel), so SynchronizationContext is captured and callbacks marshal to the UI thread automatically.
PrintAllCommand has CanExecute: !IsRunning && Items.Count > 0.
CancelCommand has CanExecute: IsRunning.
RemoveSelectedCommand has CanExecute: SelectedItem != null.
IsRunning setter calls NotifyCanExecuteChanged() on both PrintAllCommand and CancelCommand.
File 6a: FinalLabSystem/Views/Settings/PrintQueueWindow.xaml
<Window x:Class="FinalLabSystem.Views.Settings.PrintQueueWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        Title="قائمة الطباعة - Print Queue"
        Height="500" Width="700"
        WindowStartupLocation="CenterOwner"
        FlowDirection="RightToLeft"
        Background="#0D2137">

    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Row 0: Header -->
        <TextBlock Grid.Row="0"
                   Text="قائمة الطباعة"
                   Foreground="White"
                   FontSize="18"
                   FontWeight="Bold"
                   Margin="0,0,0,8" />

        <!-- Row 1: DataGrid -->
        <DataGrid Grid.Row="1"
                  ItemsSource="{Binding Items}"
                  SelectedItem="{Binding SelectedItem}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  CanUserAddRows="False"
                  CanUserDeleteRows="False"
                  SelectionMode="Single"
                  Background="#1A2A3D"
                  Foreground="White"
                  GridLinesVisibility="None"
                  HeadersVisibility="Column"
                  BorderBrush="#334455"
                  BorderThickness="1">
            <DataGrid.Columns>
                <DataGridTextColumn Header="المريض" Binding="{Binding PatientName}" Width="*" />
                <DataGridTextColumn Header="النوع" Binding="{Binding DocumentType}" Width="120" />
                <DataGridTextColumn Header="الحالة" Binding="{Binding Status}" Width="100" />
                <DataGridTextColumn Header="الوقت" Binding="{Binding AddedAt, StringFormat=HH:mm:ss}" Width="80" />
                <DataGridTextColumn Header="خطأ" Binding="{Binding Error}" Width="150" />
            </DataGrid.Columns>
        </DataGrid>

        <!-- Row 2: Progress -->
        <ProgressBar Grid.Row="2"
                     Value="{Binding Progress}"
                     Minimum="0" Maximum="100"
                     Height="20"
                     Margin="0,8,0,4"
                     Background="#1A2A3D"
                     Foreground="#4CAF50" />

        <!-- Row 3: Status Text -->
        <TextBlock Grid.Row="3"
                   Text="{Binding StatusText}"
                   Foreground="#B0BEC5"
                   FontSize="12"
                   Margin="0,0,0,8" />

        <!-- Row 4: Buttons -->
        <WrapPanel Grid.Row="4" Orientation="Horizontal">

            <Button Content="تحميل"
                    Command="{Binding LoadCommand}"
                    Padding="16,6"
                    Margin="4,0"
                    Background="#42A5F5"
                    Foreground="White"
                    FontWeight="Bold" />

            <Button Content="طباعة الكل"
                    Command="{Binding PrintAllCommand}"
                    Padding="16,6"
                    Margin="4,0"
                    Background="#4CAF50"
                    Foreground="White"
                    FontWeight="Bold" />

            <Button Content="إلغاء"
                    Command="{Binding CancelCommand}"
                    Padding="16,6"
                    Margin="4,0"
                    Background="#F44336"
                    Foreground="White"
                    FontWeight="Bold" />

            <Button Content="حذف المحدد"
                    Command="{Binding RemoveSelectedCommand}"
                    Padding="16,6"
                    Margin="4,0"
                    Background="#FF9800"
                    Foreground="White"
                    FontWeight="Bold" />

        </WrapPanel>
    </Grid>
</Window>
File 6b: FinalLabSystem/Views/Settings/PrintQueueWindow.xaml.cs
using System.Windows;

namespace FinalLabSystem.Views.Settings;

public partial class PrintQueueWindow : Window
{
    public PrintQueueWindow()
    {
        InitializeComponent();
    }
}
SECTION 5 — FILE MODIFICATION SPECIFICATIONS
Modification 1: FinalLabSystem/ViewModels/Patients/TestResultsViewModel.cs
Change A — Add using directive (if file-scoped namespace, no using block — add at top of file, after existing usings)
Add this using at the top of the file, among the existing using directives (before the namespace declaration at line 22):

using FinalLabSystem.ViewModels.Settings;
Position: After the last existing using directive, before namespace FinalLabSystem.ViewModels.Patients; at line 22.

Change B — Add field (after line 38, alongside existing service fields)
Add this field after the existing _resultEntryDialogService field (line 38):

private readonly IPrintQueueService _printQueueService;
Position: After line 38 (private readonly IResultEntryDialogService _resultEntryDialogService;).

Change C — Modify constructor signature
Current constructor signature (lines 56–68):

public TestResultsViewModel(
    IVisitService visitService,
    IRoutineResultService routineResultService,
    IAuditService auditService,
    IReportingService reportingService,
    IAuthService authService,
    IDialogService dialogService,
    ICurrentUserSession currentUserSession,
    INavigationService navigationService,
    IPrintService printService,
    IReceiptService receiptService,
    IAuditTrailDialogService auditTrailDialogService,
    IResultEntryDialogService resultEntryDialogService)
Add IPrintQueueService printQueueService as the 13th parameter, after resultEntryDialogService:

public TestResultsViewModel(
    IVisitService visitService,
    IRoutineResultService routineResultService,
    IAuditService auditService,
    IReportingService reportingService,
    IAuthService authService,
    IDialogService dialogService,
    ICurrentUserSession currentUserSession,
    INavigationService navigationService,
    IPrintService printService,
    IReceiptService receiptService,
    IAuditTrailDialogService auditTrailDialogService,
    IResultEntryDialogService resultEntryDialogService,
    IPrintQueueService printQueueService)
Change D — Add field assignment in constructor body
After the existing assignment _resultEntryDialogService = resultEntryDialogService; (the last assignment before command initialization), add:

_printQueueService = printQueueService;
Change E — Add two new commands
After the existing command declarations (line 309: NavigateToExternalSamplesCommand), add two new command properties:

public ICommand AddToPrintQueueCommand { get; }
public ICommand OpenPrintQueueCommand { get; }
Change F — Initialize the two new commands in the constructor
After the last command initialization (around line 129, the last new AsyncRelayCommand(...) or new RelayCommand(...) in the constructor), add:

AddToPrintQueueCommand = new AsyncRelayCommand(AddToPrintQueueAsync, () => HasSelectedPatient);
OpenPrintQueueCommand = new RelayCommand(OpenPrintQueue);
Change G — Add the two new command handler methods
After the last existing method in the file (before the closing brace of the class), add:

private async Task AddToPrintQueueAsync()
{
    if (SelectedPatient == null) return;

    var dto = new PrintQueueItemDto
    {
        VisitId = SelectedPatient.VisitId,
        PatientName = SelectedPatient.PatientName,
        DocumentType = "CompositeReport",
        Status = PrintQueueItemStatus.Pending,
        AddedAt = DateTime.UtcNow
    };

    _printQueueService.Enqueue(dto);
    _dialogService.ShowMessage("تمت الإضافة إلى قائمة الطباعة");
}

private void OpenPrintQueue()
{
    _navigationService.OpenTaskWindow<PrintQueueWindowViewModel>();
}
Required using for this method (add at top of file):

using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
Required using for ICommand (add at top of file):

using System.Windows.Input;
Summary of all usings to add (if not already present):

using System.Windows.Input;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.ViewModels.Settings;
Modification 2: FinalLabSystem/Views/Patients/TestResultsWindow.xaml
Change A — Add two new buttons in the bottom toolbar WrapPanel
Location: Inside the WrapPanel at lines 69–109. Add after the existing "العودة للرئيسية" (Return to Main) button block (line 108), before the closing </WrapPanel> tag.

Insert these two buttons:

<!-- Print Queue -->
<Button Style="{StaticResource BottomBarButton}"
        Background="#FF9800"
        Foreground="White"
        Content="إضافة للقائمة"
        Command="{Binding AddToPrintQueueCommand}"
        ToolTip="إضافة إلى قائمة الطباعة (Ctrl+Q)" />

<Button Style="{StaticResource BottomBarButton}"
        Background="#FF9800"
        Foreground="White"
        Content="قائمة الطباعة"
        Command="{Binding OpenPrintQueueCommand}"
        ToolTip="فتح قائمة الطباعة (Ctrl+Shift+Q)" />
Change B — Add Ctrl+Q and Ctrl+Shift+Q InputBindings
Location: Inside the Window.InputBindings block at lines 818–831. Add after the last existing InputBinding (line 830: Escape → ReturnToMainCommand), before the closing </Window.InputBindings> tag.

Insert:

<KeyBinding Key="Q" Modifiers="Ctrl" Command="{Binding AddToPrintQueueCommand}" />
<KeyBinding Key="Q" Modifiers="Ctrl+Shift" Command="{Binding OpenPrintQueueCommand}" />
Modification 3: FinalLabSystem/App.xaml.cs
Change A — Register PrintQueueService as Singleton
Location: After the existing Singleton registrations block (lines 192–204). Specifically, after line 204 (services.AddSingleton<IProcessService, ProcessService>();).

Insert:

services.AddSingleton<IPrintQueueService, PrintQueueService>();
Change B — Register PrintQueueWindowViewModel as Transient
Location: In the Transient ViewModels block (lines 206–307). Add after the last existing ViewModel registration (find the last services.AddTransient<...ViewModel>(); line).

Insert:

services.AddTransient<PrintQueueWindowViewModel>();
Change C — Register PrintQueueWindow as Transient
Location: In the Transient Views block (same area as above). Add after the ViewModel registration.

Insert:

services.AddTransient<PrintQueueWindow>();
Change D — Register navigation mapping
Location: In the navigation registration block (lines 94–114). Add after the last existing navigation.RegisterWindow<...>(); call.

Insert:

navigation.RegisterWindow<PrintQueueWindowViewModel, PrintQueueWindow>();
Required usings (add at top of App.xaml.cs if not already present):
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.ViewModels.Settings;
using FinalLabSystem.Views.Settings;
Verify these usings are not already present before adding.

SECTION 6 — COMMAND DEFINITIONS TABLE
CommandName	ViewModel	Type	Triggers	Behavior	Already Exists?
AddToPrintQueueCommand	TestResultsViewModel	AsyncRelayCommand	Button click, Ctrl+Q keybinding	Creates a PrintQueueItemDto from the selected patient/visit, calls _printQueueService.Enqueue(dto), shows confirmation message via _dialogService.ShowMessage. CanExecute: HasSelectedPatient.	No
OpenPrintQueueCommand	TestResultsViewModel	RelayCommand	Button click, Ctrl+Shift+Q keybinding	Calls _navigationService.OpenTaskWindow<PrintQueueWindowViewModel>() to open the print queue window. Always enabled.	No
LoadCommand	PrintQueueWindowViewModel	AsyncRelayCommand	Button click	Calls _printQueueService.GetItems(), clears and repopulates Items ObservableCollection.	No
PrintAllCommand	PrintQueueWindowViewModel	AsyncRelayCommand	Button click	Sets IsRunning = true, creates Progress<double> on UI thread, calls _printQueueService.PrintAllAsync(progress, cts.Token), updates StatusText with result summary. CanExecute: !IsRunning && Items.Count > 0.	No
CancelCommand	PrintQueueWindowViewModel	RelayCommand	Button click	Calls _cancellationTokenSource.Cancel() to signal cancellation to PrintAllAsync. CanExecute: IsRunning.	No
RemoveSelectedCommand	PrintQueueWindowViewModel	RelayCommand	Button click	Calls _printQueueService.Remove(SelectedItem) and removes from local Items collection. CanExecute: SelectedItem != null.	No
SECTION 7 — TEST SPECIFICATIONS
Test Class 1: FinalLabSystem.Tests/Services/PrintQueueServiceTests.cs
File path: FinalLabSystem.Tests/Services/PrintQueueServiceTests.cs

Mocked interfaces: IServiceScopeFactory, IPrintService (via mocked scope)

Setup pattern: Each test creates a Mock<IServiceScopeFactory> that returns a Mock<IServiceScope>, which in turn returns a Mock<IPrintService>. The PrintQueueService is constructed with the mock factory.

using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class PrintQueueServiceTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IPrintService> _printServiceMock;
    private readonly PrintQueueService _sut;

    public PrintQueueServiceTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _printServiceMock = new Mock<IPrintService>();

        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider.GetRequiredService<IPrintService>())
                  .Returns(_printServiceMock.Object);

        _sut = new PrintQueueService(_scopeFactoryMock.Object);
    }

    private static PrintQueueItemDto CreateItem(int visitId = 1, string docType = "Receipt")
    {
        return new PrintQueueItemDto
        {
            VisitId = visitId,
            PatientName = "Test Patient",
            DocumentType = docType,
            Status = PrintQueueItemStatus.Pending,
            AddedAt = DateTime.UtcNow
        };
    }
Test 1: Enqueue_AddsItem_WithDateTimeUtcNow
    [Fact]
    public void Enqueue_AddsItem_WithDateTimeUtcNow()
    {
        var item = CreateItem();
        var before = DateTime.UtcNow.AddSeconds(-1);

        _sut.Enqueue(item);

        var items = _sut.GetItems();
        Assert.Single(items);
        Assert.True(items[0].AddedAt >= before);
        Assert.Equal(PrintQueueItemStatus.Pending, items[0].Status);
    }
Test 2: Remove_RemovesSpecificItem
    [Fact]
    public void Remove_RemovesSpecificItem()
    {
        var item1 = CreateItem(1, "Receipt");
        var item2 = CreateItem(2, "Report");
        var item3 = CreateItem(3, "Invoice");

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        _sut.Remove(item2);

        var items = _sut.GetItems();
        Assert.Equal(2, items.Count);
        Assert.DoesNotContain(items, i => i.VisitId == 2);
    }
Test 3: Clear_EmptiesQueue
    [Fact]
    public void Clear_EmptiesQueue()
    {
        _sut.Enqueue(CreateItem(1));
        _sut.Enqueue(CreateItem(2));
        _sut.Enqueue(CreateItem(3));

        _sut.Clear();

        Assert.Empty(_sut.GetItems());
    }
Test 4: PrintAllAsync_CallsPrintService_ForEachItem
    [Fact]
    public async Task PrintAllAsync_CallsPrintService_ForEachItem()
    {
        _sut.Enqueue(CreateItem(1));
        _sut.Enqueue(CreateItem(2));
        _sut.Enqueue(CreateItem(3));

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        await _sut.PrintAllAsync(null, CancellationToken.None);

        _printServiceMock.Verify(
            s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Exactly(3));
    }
Test 5: PrintAllAsync_ReportsProgress_AfterEachItem
    [Fact]
    public async Task PrintAllAsync_ReportsProgress_AfterEachItem()
    {
        _sut.Enqueue(CreateItem(1));
        _sut.Enqueue(CreateItem(2));
        _sut.Enqueue(CreateItem(3));

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var progressMock = new Mock<IProgress<double>>();
        var reportedValues = new List<double>();
        progressMock.Setup(p => p.Report(It.IsAny<double>()))
                    .Callback<double>(v => reportedValues.Add(v));

        await _sut.PrintAllAsync(progressMock.Object, CancellationToken.None);

        Assert.Equal(3, reportedValues.Count);
        Assert.Equal(33.3, reportedValues[0], 1);
        Assert.Equal(66.7, reportedValues[1], 1);
        Assert.Equal(100.0, reportedValues[2], 1);
    }
Test 6: PrintAllAsync_OneItemFails_ContinuesOthers_MarksFailed
    [Fact]
    public async Task PrintAllAsync_OneItemFails_ContinuesOthers_MarksFailed()
    {
        var item1 = CreateItem(1);
        var item2 = CreateItem(2);
        var item3 = CreateItem(3);

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        int callCount = 0;
        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 2)
                    throw new InvalidOperationException("Printer jam");
                return Task.CompletedTask;
            });

        await _sut.PrintAllAsync(null, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Done, item1.Status);
        Assert.Equal(PrintQueueItemStatus.Failed, item2.Status);
        Assert.Equal("Printer jam", item2.Error);
        Assert.Equal(PrintQueueItemStatus.Done, item3.Status);
    }
Test 7: PrintAllAsync_RespectsCancellationToken
    [Fact]
    public async Task PrintAllAsync_RespectsCancellationToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel before calling

        _sut.Enqueue(CreateItem(1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.PrintAllAsync(null, cts.Token));

        _printServiceMock.Verify(
            s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }
Test 8: PrintAllAsync_EmptyQueue_CompletesImmediately
    [Fact]
    public async Task PrintAllAsync_EmptyQueue_CompletesImmediately()
    {
        var progressMock = new Mock<IProgress<double>>();

        await _sut.PrintAllAsync(progressMock.Object, CancellationToken.None);

        progressMock.Verify(p => p.Report(It.IsAny<double>()), Times.Never);
        _printServiceMock.Verify(
            s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    }
Test 9: PrintAllAsync_CancelledMidBatch_StopsAfterCurrentItem
    [Fact]
    public async Task PrintAllAsync_CancelledMidBatch_StopsAfterCurrentItem()
    {
        var cts = new CancellationTokenSource();
        var item1 = CreateItem(1);
        var item2 = CreateItem(2);
        var item3 = CreateItem(3);

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        int callCount = 0;
        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 2)
                    cts.Cancel();
                return Task.CompletedTask;
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.PrintAllAsync(null, cts.Token));

        Assert.Equal(PrintQueueItemStatus.Done, item1.Status);
        Assert.Equal(PrintQueueItemStatus.Done, item2.Status);
        Assert.Equal(PrintQueueItemStatus.Pending, item3.Status);
    }
Test 10: PrintAllAsync_AllItemsFail_MarksAllFailed
    [Fact]
    public async Task PrintAllAsync_AllItemsFail_MarksAllFailed()
    {
        var item1 = CreateItem(1);
        var item2 = CreateItem(2);
        var item3 = CreateItem(3);

        _sut.Enqueue(item1);
        _sut.Enqueue(item2);
        _sut.Enqueue(item3);

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("Test failure"));

        var progressMock = new Mock<IProgress<double>>();

        await _sut.PrintAllAsync(progressMock.Object, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Failed, item1.Status);
        Assert.Equal(PrintQueueItemStatus.Failed, item2.Status);
        Assert.Equal(PrintQueueItemStatus.Failed, item3.Status);
        progressMock.Verify(p => p.Report(100.0), Times.Once);
    }
Test 11: Enqueue_SetsStatusToPending
    [Fact]
    public void Enqueue_SetsStatusToPending()
    {
        var item = CreateItem();
        item.Status = PrintQueueItemStatus.Printing; // intentionally wrong

        _sut.Enqueue(item);

        var items = _sut.GetItems();
        Assert.Equal(PrintQueueItemStatus.Pending, items[0].Status);
        // Enqueue resets Status to Pending regardless of the item's
        // prior status, ensuring every queued item starts as Pending.
    }
Test 12: PrintAllAsync_SetsPrintingStatus_DuringProcessing
    [Fact]
    public async Task PrintAllAsync_SetsPrintingStatus_DuringProcessing()
    {
        var item = CreateItem(1);
        _sut.Enqueue(item);

        PrintQueueItemStatus? capturedStatus = null;
        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Callback<string, object>((dt, data) =>
            {
                var dto = (PrintQueueItemDto)data;
                capturedStatus = dto.Status;
            })
            .Returns(Task.CompletedTask);

        await _sut.PrintAllAsync(null, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Printing, capturedStatus);
        Assert.Equal(PrintQueueItemStatus.Done, item.Status);
    }
Test 13: PrintAllAsync_ErrorPropertySet_OnFailure
    [Fact]
    public async Task PrintAllAsync_ErrorPropertySet_OnFailure()
    {
        var item = CreateItem(1);
        _sut.Enqueue(item);

        _printServiceMock
            .Setup(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("Specific error message"));

        await _sut.PrintAllAsync(null, CancellationToken.None);

        Assert.Equal(PrintQueueItemStatus.Failed, item.Status);
        Assert.Equal("Specific error message", item.Error);
    }
Test Class 2: FinalLabSystem.Tests/ViewModels/Settings/PrintQueueWindowViewModelTests.cs
File path: FinalLabSystem.Tests/ViewModels/Settings/PrintQueueWindowViewModelTests.cs

Mocked interfaces: IPrintQueueService, IDialogService

using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class PrintQueueWindowViewModelTests
{
    private readonly Mock<IPrintQueueService> _printQueueServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly PrintQueueWindowViewModel _sut;

    public PrintQueueWindowViewModelTests()
    {
        _printQueueServiceMock = new Mock<IPrintQueueService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _sut = new PrintQueueWindowViewModel(
            _printQueueServiceMock.Object,
            _dialogServiceMock.Object);
    }

    private static PrintQueueItemDto CreateItem(int id, PrintQueueItemStatus status = PrintQueueItemStatus.Pending)
    {
        return new PrintQueueItemDto
        {
            VisitId = id,
            PatientName = $"Patient {id}",
            DocumentType = "Receipt",
            Status = status,
            AddedAt = DateTime.UtcNow
        };
    }
Test 14: LoadCommand_PopulatesItems_FromService
    [Fact]
    public async Task LoadCommand_PopulatesItems_FromService()
    {
        var items = new List<PrintQueueItemDto>
        {
            CreateItem(1), CreateItem(2), CreateItem(3), CreateItem(4), CreateItem(5)
        };
        _printQueueServiceMock.Setup(s => s.GetItems()).Returns(items);

        await _sut.LoadCommand.ExecuteAsync(null);

        Assert.Equal(5, _sut.Items.Count);
    }
Test 15: PrintAllCommand_TogglesIsRunning_DuringExecution
    [Fact]
    public async Task PrintAllCommand_TogglesIsRunning_DuringExecution()
    {
        var tcs = new TaskCompletionSource();
        _printQueueServiceMock
            .Setup(s => s.PrintAllAsync(It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        _sut.Items.Add(CreateItem(1));

        var task = _sut.PrintAllCommand.ExecuteAsync(null);

        Assert.True(_sut.IsRunning);

        tcs.SetResult();
        await task;

        Assert.False(_sut.IsRunning);
    }
Test 16: CancelCommand_TriggersCancellationTokenSource_Cancel
    [Fact]
    public async Task CancelCommand_TriggersCancellationTokenSource_Cancel()
    {
        CancellationToken? capturedToken = null;
        var tcs = new TaskCompletionSource();

        _printQueueServiceMock
            .Setup(s => s.PrintAllAsync(It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()))
            .Callback<IProgress<double>, CancellationToken>((p, ct) =>
            {
                capturedToken = ct;
            })
            .Returns(tcs.Task);

        _sut.Items.Add(CreateItem(1));

        var printTask = _sut.PrintAllCommand.ExecuteAsync(null);
        await Task.Delay(50); // Let the async operation start

        _sut.CancelCommand.Execute(null);

        Assert.NotNull(capturedToken);
        Assert.True(capturedToken.Value.IsCancellationRequested);

        tcs.SetCanceled(); // Complete the task to avoid hanging
        await printTask;
    }
Test Class 3: FinalLabSystem.Tests/Services/PrintQueueServiceRegistrationTests.cs
File path: FinalLabSystem.Tests/Services/PrintQueueServiceRegistrationTests.cs

using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class PrintQueueServiceRegistrationTests
{
    [Fact]
    public void DI_Resolves_IPrintQueueService_AsSingleton()
    {
        var services = new ServiceCollection();

        // Register the same services as App.xaml.cs would
        services.AddSingleton<IPrintQueueService, PrintQueueService>();
        services.AddSingleton<IServiceScopeFactory, MockScopeFactory>();

        var provider = services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<IPrintQueueService>();
        var instance2 = provider.GetRequiredService<IPrintQueueService>();

        Assert.Same(instance1, instance2);
    }
}

// Minimal mock for IServiceScopeFactory to satisfy DI resolution
internal class MockScopeFactory : IServiceScopeFactory
{
    public IServiceScope CreateScope() => throw new NotImplementedException();
}
SECTION 8 — EXECUTION ORDER
Step 1: Create FinalLabSystem/Models/Enums/PrintQueueItemStatus.cs

No dependencies. Create the enum file.
Step 2: Create FinalLabSystem/Models/DTOs/PrintQueueItemDto.cs

Depends on Step 1 (references PrintQueueItemStatus).
Step 3: Create FinalLabSystem/Services/Interfaces/IPrintQueueService.cs

Depends on Step 2 (references PrintQueueItemDto).
Step 4: Create FinalLabSystem/Services/Implementations/PrintQueueService.cs

Depends on Step 3 (implements IPrintQueueService). Depends on existing IPrintService.
Step 5: Create FinalLabSystem/ViewModels/Settings/PrintQueueWindowViewModel.cs

Depends on Step 3 (references IPrintQueueService). Depends on existing IDialogService, ViewModelBase.
Step 6: Create FinalLabSystem/Views/Settings/PrintQueueWindow.xaml + PrintQueueWindow.xaml.cs

Depends on Step 5 (DataContext will be PrintQueueWindowViewModel).
Step 7: Modify FinalLabSystem/App.xaml.cs

Depends on Steps 1–6 (all new types must exist). Register IPrintQueueService as Singleton, PrintQueueWindowViewModel as Transient, PrintQueueWindow as Transient, navigation mapping.
Step 8: Modify FinalLabSystem/ViewModels/Patients/TestResultsViewModel.cs

Depends on Steps 3 and 5 (references IPrintQueueService and PrintQueueWindowViewModel). Add field, constructor parameter, 2 commands, 2 handler methods.
Step 9: Modify FinalLabSystem/Views/Patients/TestResultsWindow.xaml

Depends on Step 8 (commands must exist in the ViewModel). Add 2 buttons and 2 InputBindings.
Step 10: Build the solution. Verify 0 errors, 0 warnings.

Step 11: Create FinalLabSystem.Tests/Services/PrintQueueServiceTests.cs

Depends on Steps 1–4. Contains tests 1–13.
Step 12: Create FinalLabSystem.Tests/ViewModels/Settings/PrintQueueWindowViewModelTests.cs

Depends on Steps 3 and 5. Contains tests 14–16.
Step 13: Create FinalLabSystem.Tests/Services/PrintQueueServiceRegistrationTests.cs

Depends on Steps 3–4. Contains test 17 (DI resolution).
Step 14: Run all tests. Verify 626 total passing (existing + 19 new).

SECTION 9 — VERIFICATION CHECKLIST
#	Check	Pass/Fail
1	Solution builds with 0 errors, 0 warnings	
2	All 19 new tests pass	
3	Total passing tests = 626	
4	PrintQueueService constructor takes IServiceScopeFactory (NOT IPrintService) — verifiable by grep for IServiceScopeFactory in PrintQueueService.cs	
5	Ctrl+Q is bound in TestResultsWindow.xaml InputBindings (Key="Q" Modifiers="Ctrl")	
6	Ctrl+Shift+Q is bound in TestResultsWindow.xaml InputBindings (Key="Q" Modifiers="Ctrl+Shift")	
7	PrintQueueService registered as Singleton in App.xaml.cs (AddSingleton<IPrintQueueService, PrintQueueService>())	
8	No existing test is broken (regression = 0)	
9	PrintQueueItemStatus enum has exactly 4 values: Pending, Printing, Done, Failed	
10	PrintQueueItemDto has exactly 6 properties: VisitId, PatientName, DocumentType, Status, AddedAt, Error	
11	PrintQueueService uses lock (_lock) in all 4 methods (Enqueue, Remove, Clear, GetItems) plus inside PrintAllAsync snapshot	
12	ThrowIfCancellationRequested() is called INSIDE the for loop body in PrintAllAsync (not before the loop)	
13	PrintAllAsync creates IServiceScope per item via _scopeFactory.CreateScope()	
14	TestResultsViewModel constructor has 13 parameters (12 original + 1 new IPrintQueueService)	
15	TestResultsWindow.xaml has 2 new buttons in the bottom WrapPanel with styles matching existing buttons	
SECTION 10 — DO-NOT-TOUCH LIST
The implementing agent MUST NOT modify any of the following files:

FinalLabSystem/Services/Interfaces/IPrintService.cs (Slice 6.1 — complete)
FinalLabSystem/Services/Implementations/WpfFlowDocumentPrintService.cs
FinalLabSystem/Services/Interfaces/IPrintPreviewDialogService.cs
FinalLabSystem/Services/Implementations/PrintPreviewDialogService.cs
FinalLabSystem/ViewModels/Patients/PrintPreviewViewModel.cs
FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml and .xaml.cs
FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs
FinalLabSystem/ViewModels/Patients/PatientSearchViewModel.cs
FinalLabSystem/ViewModels/Patients/DeliveryViewModel.cs
FinalLabSystem/ViewModels/Patients/ResultEntryViewModel.cs
FinalLabSystem/ViewModels/Patients/ExternalLabsWindowViewModel.cs
FinalLabSystem/Views/Patients/PatientRegistrationWindow.xaml
FinalLabSystem/Views/Patients/PatientSearchWindow.xaml
FinalLabSystem/Views/Patients/DeliveryWindow.xaml
FinalLabSystem/Views/Patients/ResultEntryWindow.xaml
FinalLabSystem/Views/Patients/ExternalLabsWindow.xaml
FinalLabSystem/SharedStyles.xaml
FinalLabSystem/SharedConverters.xaml
All test files not listed in Section 7
All other ViewModels, Views, Services, Models, and DTOs not listed in Section 4 or Section 5
SECTION 11 — RISKS AND MITIGATIONS
Risk 1: Stale Scoped Dependency
Description: PrintQueueService is Singleton and needs to call IPrintService which is Scoped. Direct injection would capture a stale Scoped instance after the first scope disposes.

Mitigation: PrintQueueService constructor receives IServiceScopeFactory (NOT IPrintService). Inside PrintAllAsync, a fresh IServiceScope is created per item, and IPrintService is resolved from that scope. This is the standard .NET pattern for this problem. Already mandated in Section 4, File 4.

Verification: grep -r "IServiceScopeFactory" PrintQueueService.cs must return a match. grep -r "IPrintService" PrintQueueService.cs must only appear inside the method body (not in constructor parameters).

Risk 2: ThrowIfCancellationRequested Placement
Description: If cancellationToken.ThrowIfCancellationRequested() is placed before the for loop, it only catches cancellation that happened before printing started. Mid-batch cancellation would not be detected until the next item.

Mitigation: The call MUST be the first statement inside the for loop body. This is already specified in Section 4, File 4. The implementing agent must not move it outside the loop.

Verification: In PrintQueueService.cs, ThrowIfCancellationRequested() must appear between for (int i = 0; and the PrintQueueItemDto item = snapshot[i]; line.

Risk 3: IProgress<double> + WPF Dispatcher
Description: Progress<T> callbacks execute on the SynchronizationContext captured at construction time. If Progress<double> is instantiated on a background thread, callbacks will not marshal to the UI thread, causing cross-thread exceptions when binding to WPF properties.

Mitigation: In PrintQueueWindowViewModel.PrintAllAsync(), the Progress<double> is instantiated directly in the async method body. Since AsyncRelayCommand captures the WPF SynchronizationContext and resumes continuations on the UI thread, the Progress<double> constructor runs on the UI thread, capturing the correct SynchronizationContext. All callbacks (setting Progress, StatusText) will execute on the UI thread. No explicit Dispatcher.Invoke is needed.

Verification: new Progress<double>(...) must appear inside PrintQueueWindowViewModel.PrintAllAsync() — NOT inside PrintQueueService.PrintAllAsync() (the service has no UI thread access).

Risk 4: Bare-Ctrl Handler
Description: TestResultsWindow.xaml.cs lines 72–88 has a KeyDown handler that toggles focus on bare Key.Control.

Mitigation: This handler fires only when e.Key == Key.Control (the physical Ctrl key alone). Ctrl+Q fires when e.Key == Key.Q with Modifiers == ModifierKeys.Control. These are distinct WPF key events. No code change is needed. Already documented in Section 3b.

Verification: No code change required. The implementing agent should NOT modify TestResultsWindow.xaml.cs. ===END FILE CONTENT===


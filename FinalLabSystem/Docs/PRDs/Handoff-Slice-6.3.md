# Handoff-Slice-6.3.md — Backup UI & Restore Workflow

> **Project:** FinalLabSystem
> **Slice:** 6.3
> **Prepared by:** Winston (Software Architect Agent)
> **Date:** 2026-07-01
> **Status:** FINAL — Ready for Implementation
> **Revision:** 2.0 — Post-verification corrections applied

---

## 1. Scope

**Objective:** Build `BackupRestoreWindow` with Admin Password confirmation workflow (BR-061) and activate the currently-placeholder `BackupMenuViewModel`.

**Pre-condition:** Slice 6.2 must be complete (`IBackupService` with AES-256-CBC + PBKDF2 is registered as Scoped).

**Files to create:** 5 new files
**Files to modify:** 6 existing files
**New tests:** 25 tests
**Expected cumulative test count after G6.3:** 605 passing tests

---

## 2. Current System Snapshot

### BackupMenuViewModel — Current State

**File:** `FinalLabSystem/ViewModels/Menu/BackupMenuViewModel.cs`

```csharp
public sealed class BackupMenuViewModel : ViewModelBase
{
    public BackupMenuViewModel(IDialogService dialogService)
    {
        PlaceholderCommand = new RelayCommand(_ =>
            dialogService.ShowMessage("سيتم تفعيل هذه الميزة في المرحلة 6", "قريباً"));
    }
    public ICommand PlaceholderCommand { get; }
}
```

- Takes `IDialogService` only — no `INavigationService`.
- Contains only `PlaceholderCommand` — no navigation.

### MainViewModel — How BackupMenuVM Is Created

**File:** `FinalLabSystem/ViewModels/MainViewModel.cs`, line 31

```csharp
ShowBackupMenuCommand = new RelayCommand(_ => CurrentView = new BackupMenuViewModel(_dialogService));
```

- Passes `_dialogService` instead of `_navigationService`.
- **This line is NOT modified in Slice 6.3.**

### ReportSettingsMenuViewModel — Reference Pattern (NOT followed by BackupMenuVM)

**File:** `FinalLabSystem/ViewModels/Menu/ReportSettingsMenuViewModel.cs`

```csharp
public sealed class ReportSettingsMenuViewModel : ViewModelBase
{
    public ReportSettingsMenuViewModel(INavigationService navigationService)
    {
        ManageTemplatesCommand = new RelayCommand(_ =>
            navigationService.OpenTaskWindow<ReportCommentTemplateViewModel>());
    }
    public ICommand ManageTemplatesCommand { get; }
}
```

### INavigationService — Verified Signatures

**File:** `FinalLabSystem/Infrastructure/Navigation/INavigationService.cs`

```csharp
public interface INavigationService
{
    void ShowLogin();
    void ShowFirstRunSetup();
    void ShowMain();
    void OpenTaskWindow<TViewModel>() where TViewModel : class;
    void OpenTaskWindow<TViewModel>(Action<TViewModel>? configure) where TViewModel : class;
    void RegisterWindow<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : System.Windows.Window;
    void ReturnToMain();
    void Shutdown();
}
```

### NavigationService — Shutdown Implementation

**File:** `FinalLabSystem/Infrastructure/Navigation/NavigationService.cs`, line 94

```csharp
public void Shutdown() => Application.Current.Shutdown();
```

### IDialogService — Current Signatures (will be extended)

**File:** `FinalLabSystem/Services/Interfaces/IDialogService.cs`

```csharp
public interface IDialogService
{
    void ShowMessage(string message, string title = "");
    void ShowError(string message, string title = "خطأ");
    void ShowWarning(string message, string title = "تنبيه");
    bool ShowConfirmation(string message, string title = "تأكيد");
}
```

**NEW method to be added in Slice 6.3:**
```csharp
T? ShowCustomDialog<T>() where T : System.Windows.Window;
```

### DialogService — Current Implementation (will be extended)

**File:** `FinalLabSystem/Services/Implementations/DialogService.cs`

- Implements `IDialogService` with `MessageBox.Show` wrappers.
- Constructor: parameterless.
- **Will be extended** to inject `IServiceProvider` and implement `ShowCustomDialog<T>()`.

### CashDrawerUnlockDialog — Password Dialog Pattern

**File:** `FinalLabSystem/Views/Settings/CashDrawerUnlockDialog.xaml.cs`

```csharp
public partial class CashDrawerUnlockDialog : Window
{
    public CashDrawerUnlockDialog()
    {
        InitializeComponent();
        PasswordBox.Focus();
    }

    public string? EnteredPassword { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ErrorText.Text = "يُرجى إدخال كلمة المرور";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }
        EnteredPassword = PasswordBox.Password;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OK_Click(sender, e);
    }
}
```

**XAML attributes:** `WindowStartupLocation="CenterOwner"`, `FlowDirection="RightToLeft"`, `ResizeMode="NoResize"`, `Height="220" Width="380"`.

### Window DI Registration Pattern

**File:** `FinalLabSystem/App.xaml.cs`

```csharp
services.AddTransient<CashDrawerWindowViewModel>();
services.AddTransient<CashDrawerWindow>();
```

**Navigation registration (NOT used for BackupRestoreWindow — ShowCustomDialog pattern instead):**
```csharp
navigation.RegisterWindow<CashDrawerWindowViewModel, CashDrawerWindow>();
```

### BackupOutputFolder — Exists in LabSetting

**File:** `FinalLabSystem/Models/LabSetting.cs`, line 34

```csharp
public string? BackupOutputFolder { get; set; }
```

### IBackupService — Verified Signatures (will be extended)

**File:** `FinalLabSystem/Services/Interfaces/IBackupService.cs`

```csharp
public interface IBackupService
{
    Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type);
    Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword);
    Task<List<BackupMetadataDto>> ListBackupsAsync(string folder);
    Task<bool> ValidateBackupFileAsync(string backupFilePath, string adminPassword);
}
```

**NEW methods to be added in Slice 6.3:**
```csharp
Task<string> GetBackupOutputFolderAsync();
Task SaveBackupOutputFolderAsync(string folderPath, int staffId);
```

### BackupService — Has FinalLabDbContext Access (can read/write LabSetting)

**File:** `FinalLabSystem/Services/Implementations/BackupService.cs`

- Injects `FinalLabDbContext _context` — can access `_context.LabSettings`.
- `GetBackupOutputFolderAsync()` will read `LabSetting.SettingValue` where `SettingKey = "BackupOutputFolder"`.
- `SaveBackupOutputFolderAsync()` will upsert the setting value.

### BackupMetadataDto — Verified Fields

**File:** `FinalLabSystem/Models/DTOs/BackupMetadataDto.cs`

```csharp
public class BackupMetadataDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public int? CreatedByStaffId { get; set; }
    public bool IsEncrypted { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;
}
```

### BackupType Enum

**File:** `FinalLabSystem/Models/Enums/BackupType.cs`

```csharp
public enum BackupType { Full, Incremental }
```

### ICurrentUserSession — Verified

**File:** `FinalLabSystem/Infrastructure/Session/CurrentUserSession.cs`

```csharp
public interface ICurrentUserSession
{
    Staff? CurrentUser { get; }
    bool IsAuthenticated { get; }
    // ...
}
```

`CurrentUser?.IsAdmin` is the admin check.

### ViewModelBase — Verified

**File:** `FinalLabSystem/Infrastructure/ViewModelBase.cs`

- Implements `INotifyPropertyChanged` and `INotifyDataErrorInfo`.
- Provides `SetProperty<T>()`, `AddError()`, `ClearErrors()`, `ClearAllErrors()`.

### AsyncRelayCommand — Verified

**File:** `FinalLabSystem/Infrastructure/AsyncRelayCommand.cs`

- Has `ErrorOccurred` event, `IsExecuting` property.
- Constructor: `AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute)`.

### RelayCommand — Verified

**File:** `FinalLabSystem/Infrastructure/RelayCommand.cs`

- Constructor: `RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute)`.

---

## 3. Locked Architectural Decisions

| ID | Decision | Rule | Rationale |
|----|----------|------|-----------|
| **FD-01** | Password Handling | Use `string` for passwords. **No `SecureString`.** | Consistent with `CashDrawerUnlockDialog`. Avoids WPF PasswordBox binding complexity. |
| **FD-02** | Shutdown Flow | Use `INavigationService.Shutdown()` after successful Restore. **No `Application.Current.Shutdown()`** in ViewModels. | Maintains layer separation. ViewModel never references Application layer directly. |
| **FD-03** | Backup Menu Navigation | `BackupMenuViewModel` **keeps `IDialogService`** dependency. Does **NOT** use `INavigationService`. Opens `BackupRestoreWindow` via `IDialogService.ShowCustomDialog<BackupRestoreWindow>()`. `MainViewModel` line 31 is **NOT modified**. | Minimizes impact on existing DI chain. MainViewModel passes `IDialogService` — we keep that. |
| **FD-04** | Password Dialog Pattern | `BackupPasswordDialog` follows `CashDrawerUnlockDialog` pattern: code-behind reads `PasswordBox.Password` directly, exposes `EnteredPassword` property, uses `DialogResult`. **No ViewModel**, no SecureString binding, no attached properties. | Consistent with existing project patterns. |
| **FD-05** | DI Lifetime | `BackupRestoreWindowViewModel` = Transient, `BackupRestoreWindow` = Transient, `BackupPasswordDialog` = Transient. **No Singleton.** | Matches all other Windows/ViewModels in the project. |
| **FD-06** | Target Folder Persistence | `TargetFolder` reads from `IBackupService.GetBackupOutputFolderAsync()` on init. Saved via `IBackupService.SaveBackupOutputFolderAsync()` on change. **`ISettingsService` is NOT used.** | `LabSetting.BackupOutputFolder` belongs to the backup domain. `IBackupService` owns all backup-related settings. |
| **FD-07** | Error Handling | Every command must wrap service calls in `try/catch`. Use `_dialogService.ShowError(...)` for all failure paths. | Prevents unhandled exceptions from crashing the application. |
| **FD-08** | Selection Model | `BackupRowViewModel` must expose `bool IsSelected` with `INotifyPropertyChanged` support. | Enables DataGrid selection binding. |
| **FD-09** | Admin Validation Order | Before opening `BackupPasswordDialog`: check `_currentUserSession.CurrentUser?.IsAdmin == true`. If not admin: show error via `_dialogService.ShowError()` and return. **Do not open dialog for non-admin users.** | BR-061 enforcement at VM level, before any service call. |
| **FD-10** | Restore Confirmation Order | Strict sequence: (1) User selects backup → (2) `ShowConfirmation()` → (3) Password dialog → (4) Execute restore → (5) Success dialog → (6) Shutdown. **No deviation.** | Prevents accidental data loss. |
| **FD-11** | ShowCustomDialog Pattern | `IDialogService` gains `T? ShowCustomDialog<T>() where T : Window`. Implementation uses `IServiceProvider.GetRequiredService<T>()` — **NOT `new T()`**. This preserves constructor injection for all windows (e.g., `BackupRestoreWindow` needs `BackupRestoreWindowViewModel` + `INavigationService`). | Avoids breaking dependency chain. `new()` constraint would bypass DI. |
| **FD-12** | ProcessStartInfo for Folder Opening | Use `Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true })` — **NOT** `Process.Start("explorer.exe", path)`. | `UseShellExecute = true` is the correct and recommended pattern for opening Explorer on Windows. |

---

## 4. Files To Create

### 4.1 — `FinalLabSystem/ViewModels/Settings/BackupRowViewModel.cs`

**Purpose:** Wrapper for `BackupMetadataDto` with UI-friendly properties.

**Required contents:**
- Namespace: `FinalLabSystem.ViewModels.Settings`
- Class: `public sealed class BackupRowViewModel : INotifyPropertyChanged`
- Constructor: `BackupRowViewModel(BackupMetadataDto dto)`
- Properties:
  - `string FileName` — from `dto.FileName`
  - `string FilePath` — from `dto.FilePath`
  - `string DisplaySize` — formatted via `FormatBytes(dto.FileSizeBytes)` (static helper: bytes → KB → MB → GB)
  - `string DisplayCreatedAt` — `dto.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")`
  - `bool IsSelected` — with `INotifyPropertyChanged` backing, default `false`
- Static helper: `private static string FormatBytes(long bytes)` — returns human-readable string
- No dependencies on DI services.

### 4.2 — `FinalLabSystem/ViewModels/Settings/BackupRestoreWindowViewModel.cs`

**Purpose:** Main ViewModel for the backup/restore window.

**Required contents:**
- Namespace: `FinalLabSystem.ViewModels.Settings`
- Class: `public sealed class BackupRestoreWindowViewModel : ViewModelBase`
- Constructor dependencies (injected via DI):
  - `IBackupService _backupService`
  - `IDialogService _dialogService`
  - `ICurrentUserSession _currentUserSession`
- **NOTE:** `INavigationService` is **NOT** injected. `ISettingsService` is **NOT** injected.
- Private fields:
  - `ObservableCollection<BackupRowViewModel> _backups`
  - `string _targetFolder`
  - `DateTime? _lastBackupAt`
  - `bool _isBusy`
- Public property:
  - `Action? RequestShutdown { get; set; }` — set by `BackupRestoreWindow` code-behind to call `INavigationService.Shutdown()`
- Constructor logic:
  - Initialize `_targetFolder` from `await _backupService.GetBackupOutputFolderAsync()` with fallback to `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FinalLabBackups")`
  - Initialize `_backups = new ObservableCollection<BackupRowViewModel>()`
  - Call `LoadBackupsAsync()` on construction (fire-and-forget or `Loaded` event)
- Commands:
  - `ICommand LoadBackupsCommand` — `AsyncRelayCommand` calling `LoadBackupsAsync()`
  - `ICommand CreateBackupCommand` — `AsyncRelayCommand` calling `CreateBackupAsync()`, guarded by `() => !IsBusy`
  - `ICommand RestoreCommand` — `AsyncRelayCommand` calling `RestoreAsync()`, guarded by `() => !IsBusy`
  - `ICommand BrowseFolderCommand` — `RelayCommand` calling `BrowseFolderAsync()`
  - `ICommand OpenFolderCommand` — `RelayCommand` calling `OpenFolder()`
- Private async methods:
  - `LoadBackupsAsync()` — calls `_backupService.ListBackupsAsync(_targetFolder)`, populates `_backups`
  - `CreateBackupAsync()` — validates admin → opens `BackupPasswordDialog` → calls `_backupService.CreateBackupAsync(...)` → refreshes list
  - `RestoreAsync()` — validates admin → validates selection → `ShowConfirmation` → opens `BackupPasswordDialog` → calls `_backupService.RestoreBackupAsync(...)` → success message → `RequestShutdown?.Invoke()`
  - `BrowseFolderAsync()` — uses `Microsoft.Win32.OpenFolderDialog` → persists via `_backupService.SaveBackupOutputFolderAsync(...)`
  - `OpenFolder()` — wraps `Process.Start(new ProcessStartInfo("explorer.exe", _targetFolder) { UseShellExecute = true })` in `try/catch`
- All commands use `try/catch` with `_dialogService.ShowError(...)` in failure paths (FD-07).
- Admin check at start of `CreateBackupAsync` and `RestoreAsync` (FD-09).

### 4.3 — `FinalLabSystem/Views/Settings/BackupRestoreWindow.xaml` + `.cs`

**Purpose:** Main backup/restore window with DataGrid and toolbar.

**XAML requirements:**
- `WindowStartupLocation="CenterOwner"`
- `FlowDirection="RightToLeft"`
- Title: "النسخ الاحتياطي والاستعادة"
- Layout: 3-row Grid (Toolbar / DataGrid / Status bar)
- Row 0: Border with StackPanel containing buttons (إنشاء نسخة, استعادة, تصفح المجلد, فتح المجلد), separator, backup count label
- Row 1: `DataGrid` bound to `Backups`, columns: `FileName`, `DisplaySize`, `DisplayCreatedAt`, `IsEncrypted`
- Row 2: Status bar with `TargetFolder` text and `LastBackupAt` text
- All buttons bound to ViewModel commands

**Code-behind requirements:**
```csharp
public partial class BackupRestoreWindow : Window
{
    public BackupRestoreWindow(BackupRestoreWindowViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestShutdown = () => navigationService.Shutdown();
    }
}
```

**Required using additions:**
```csharp
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;
```

### 4.4 — `FinalLabSystem/Views/Settings/BackupPasswordDialog.xaml` + `.cs`

**Purpose:** Password confirmation dialog following `CashDrawerUnlockDialog` pattern exactly.

**XAML requirements:**
- Follow `CashDrawerUnlockDialog.xaml` structure exactly
- `WindowStartupLocation="CenterOwner"`, `FlowDirection="RightToLeft"`, `ResizeMode="NoResize"`
- `Height="280" Width="380"`
- Grid with rows: Title, PasswordBox, ConfirmPasswordBox, ErrorText, Buttons
- Title: "أدخل كلمة المرور"
- `PasswordBox` with `x:Name="PasswordBox"` and `KeyDown` handler (Enter → OK)
- `ConfirmPasswordBox` with `x:Name="ConfirmPasswordBox"`
- `ErrorText` TextBlock (Red, Collapsed by default) with `x:Name="ErrorText"`
- Two buttons: "تأكيد" (blue) and "إلغاء" (red)

**Code-behind requirements:**
```csharp
public partial class BackupPasswordDialog : Window
{
    public BackupPasswordDialog()
    {
        InitializeComponent();
        PasswordBox.Focus();
    }

    public string? EnteredPassword { get; private set; }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            ErrorText.Text = "يُرجى إدخال كلمة المرور";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (PasswordBox.Password.Length < 8)
        {
            ErrorText.Text = "كلمة المرور يجب أن تكون 8 أحرف على الأقل";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            ErrorText.Text = "كلمتا المرور غير متطابقتين";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        EnteredPassword = PasswordBox.Password;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            OK_Click(sender, e);
    }
}
```

---

## 5. Files To Modify

### 5.1 — `FinalLabSystem/ViewModels/Menu/BackupMenuViewModel.cs`

**Current state:** Placeholder with `IDialogService` dependency and `PlaceholderCommand`.

**Required changes:**
1. Replace `PlaceholderCommand` with `OpenBackupCommand`
2. Replace the command body to use `ShowCustomDialog<BackupRestoreWindow>()`
3. Add `using FinalLabSystem.Views.Settings;`
4. Keep `IDialogService` dependency (do NOT change to `INavigationService`)

**Final file should be:**
```csharp
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class BackupMenuViewModel : ViewModelBase
{
    public BackupMenuViewModel(IDialogService dialogService)
    {
        OpenBackupCommand = new RelayCommand(_ =>
            dialogService.ShowCustomDialog<BackupRestoreWindow>());
    }

    public ICommand OpenBackupCommand { get; }
}
```

### 5.2 — `FinalLabSystem/Services/Interfaces/IDialogService.cs`

**Add new method:**
```csharp
/// <summary>
/// Shows a modal dialog resolved from the DI container.
/// </summary>
/// <typeparam name="T">The Window type to show.</typeparam>
/// <returns>The dialog result, or <c>null</c> when cancelled.</returns>
T? ShowCustomDialog<T>() where T : System.Windows.Window;
```

### 5.3 — `FinalLabSystem/Services/Implementations/DialogService.cs`

**Changes:**
1. Add `IServiceProvider` injection in constructor
2. Implement `ShowCustomDialog<T>()` using `IServiceProvider.GetRequiredService<T>()`

**Final file should be:**
```csharp
using System.Windows;
using FinalLabSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.Services.Implementations;

public sealed class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ShowMessage(string message, string title = "")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "خطأ")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "تنبيه")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool ShowConfirmation(string message, string title = "تأكيد")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public T? ShowCustomDialog<T>() where T : Window
    {
        var dialog = _serviceProvider.GetRequiredService<T>();
        dialog.Owner = Application.Current.MainWindow;
        return dialog.ShowDialog() == true ? dialog : null;
    }
}
```

### 5.4 — `FinalLabSystem/Services/Interfaces/IBackupService.cs`

**Add new methods:**
```csharp
Task<string> GetBackupOutputFolderAsync();
Task SaveBackupOutputFolderAsync(string folderPath, int staffId);
```

### 5.5 — `FinalLabSystem/Services/Implementations/BackupService.cs`

**Add implementation for new methods:**
```csharp
public async Task<string> GetBackupOutputFolderAsync()
{
    var setting = await _context.LabSettings
        .FirstOrDefaultAsync(s => s.SettingKey == "BackupOutputFolder");
    return setting?.SettingValue
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FinalLabBackups");
}

public async Task SaveBackupOutputFolderAsync(string folderPath, int staffId)
{
    var setting = await _context.LabSettings
        .FirstOrDefaultAsync(s => s.SettingKey == "BackupOutputFolder");

    if (setting is null)
    {
        setting = new LabSetting { SettingKey = "BackupOutputFolder", SettingValue = folderPath };
        _context.LabSettings.Add(setting);
    }
    else
    {
        setting.SettingValue = folderPath;
    }

    await _context.SaveChangesAsync();
}
```

### 5.6 — `FinalLabSystem/App.xaml.cs`

**Add DI registrations in ConfigureServices method:**
```csharp
// Slice 6.3 — Backup UI
services.AddTransient<BackupRestoreWindowViewModel>();
services.AddTransient<BackupRestoreWindow>();
services.AddTransient<BackupPasswordDialog>();
```

**Add using at top:**
```csharp
using FinalLabSystem.ViewModels.Settings;
using FinalLabSystem.Views.Settings;
```

**NO `RegisterWindow` call** — `BackupRestoreWindow` is opened via `ShowCustomDialog`, not via navigation.

**NOTE:** `BackupPasswordDialogViewModel` is **NOT registered** — the dialog uses code-behind only.

---

## 6. Exact DI Registrations

Add to `FinalLabSystem/App.xaml.cs` in the `ConfigureServices` method (after existing Transient registrations):

```csharp
// Slice 6.3 — Backup UI
services.AddTransient<BackupRestoreWindowViewModel>();
services.AddTransient<BackupRestoreWindow>();
services.AddTransient<BackupPasswordDialog>();
```

**Required using additions at top of App.xaml.cs:**
```csharp
using FinalLabSystem.ViewModels.Settings;
using FinalLabSystem.Views.Settings;
```

**NOT registered:**
- `BackupPasswordDialogViewModel` — does not exist (FD-04)
- `RegisterWindow<BackupRestoreWindowViewModel, BackupRestoreWindow>` — not needed (FD-11)

---

## 7. Navigation Registration

**No `RegisterWindow` call for `BackupRestoreWindow`.**

`BackupRestoreWindow` is opened via `IDialogService.ShowCustomDialog<BackupRestoreWindow>()` (FD-11), which uses `IServiceProvider.GetRequiredService<T>()` and `window.ShowDialog()`. It is **NOT** part of the navigation system (`OpenTaskWindow` / `RegisterWindow`).

The `BackupRestoreWindow` code-behind receives `INavigationService` via constructor injection and passes `Shutdown` callback to the ViewModel via `RequestShutdown` property.

---

## 8. ViewModel Contracts

### BackupRestoreWindowViewModel — Full Contract

```csharp
public sealed class BackupRestoreWindowViewModel : ViewModelBase
{
    // Dependencies (injected) — 3 only
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserSession _currentUserSession;

    // Callback — set by BackupRestoreWindow code-behind
    public Action? RequestShutdown { get; set; }

    // Observable properties
    public ObservableCollection<BackupRowViewModel> Backups { get; }
    public string TargetFolder { get; set; }       // with INPC
    public DateTime? LastBackupAt { get; }          // read-only
    public bool IsBusy { get; set; }                // with INPC

    // Commands
    public ICommand LoadBackupsCommand { get; }     // AsyncRelayCommand
    public ICommand CreateBackupCommand { get; }    // AsyncRelayCommand
    public ICommand RestoreCommand { get; }         // AsyncRelayCommand
    public ICommand BrowseFolderCommand { get; }    // RelayCommand
    public ICommand OpenFolderCommand { get; }      // RelayCommand
}
```

---

## 9. BackupRowViewModel Contract

```csharp
public sealed class BackupRowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public BackupRowViewModel(BackupMetadataDto dto) { ... }

    public string FileName { get; }
    public string FilePath { get; }
    public string DisplaySize { get; }
    public string DisplayCreatedAt { get; }
    public bool IsSelected { get; set; }  // with INPC

    private static string FormatBytes(long bytes) { ... }
}
```

---

## 10. BackupPasswordDialog Contract

**Window (code-behind only — no ViewModel):**
- Constructor: parameterless
- Public property: `string? EnteredPassword { get; private set; }`
- DialogResult: `true` on OK, `false` on Cancel
- Validation in `OK_Click`: non-empty, length >= 8, passwords match
- Uses `x:Name="PasswordBox"`, `x:Name="ConfirmPasswordBox"`, `x:Name="ErrorText"`

---

## 11. BackupRestoreWindow Contract

**Window:**
- Constructor: `BackupRestoreWindow(BackupRestoreWindowViewModel viewModel, INavigationService navigationService)`
- Code-behind: `InitializeComponent()` + `DataContext = viewModel` + `viewModel.RequestShutdown = () => navigationService.Shutdown();`
- No event handlers, no business logic beyond wiring

---

## 12. Backup Flow (Step-by-Step)

```
1. User clicks "النسخ الاحتياطي" in sidebar menu
   → MainViewModel.ShowBackupMenuCommand
   → CurrentView = new BackupMenuViewModel(_dialogService)

2. User clicks "إنشاء نسخة احتياطية" in BackupMenuViewModel
   → dialogService.ShowCustomDialog<BackupRestoreWindow>()
   → BackupRestoreWindow opens with BackupRestoreWindowViewModel

3. User clicks "إنشاء نسخة احتياطية" button in BackupRestoreWindow
   → BackupRestoreWindowViewModel.CreateBackupCommand

4. Admin check (FD-09):
   → if (_currentUserSession.CurrentUser?.IsAdmin != true)
   → _dialogService.ShowError("فقط المسؤولون يمكنهم إنشاء نسخ احتياطية")
   → return

5. IsBusy = true

6. Open BackupPasswordDialog (FD-04):
   → var dialog = new BackupPasswordDialog { Owner = Window.GetWindow(this) }
   → if (dialog.ShowDialog() != true || dialog.EnteredPassword is null)
   → IsBusy = false; return

7. Call service:
   → try { await _backupService.CreateBackupAsync(_targetFolder, dialog.EnteredPassword, BackupType.Full); }
   → catch (DirectoryNotFoundException) { _dialogService.ShowError("المجلد غير موجود"); }
   → catch (IOException ex) { _dialogService.ShowError($"خطأ في الكتابة: {ex.Message}"); }
   → catch (UnauthorizedAccessException) { _dialogService.ShowError("لا توجد صلاحية كتابة"); }
   → catch (Exception ex) { _dialogService.ShowError($"خطأ غير متوقع: {ex.Message}"); }

8. On success:
   → _dialogService.ShowMessage("تم إنشاء النسخة الاحتياطية بنجاح")
   → await LoadBackupsAsync()  // refresh list

9. IsBusy = false
```

---

## 13. Restore Flow (Step-by-Step)

```
1. User selects a backup row in DataGrid
   → BackupRowViewModel.IsSelected = true

2. User clicks "استعادة" button
   → BackupRestoreWindowViewModel.RestoreCommand

3. Admin check (FD-09):
   → if (_currentUserSession.CurrentUser?.IsAdmin != true)
   → _dialogService.ShowError("فقط المسؤولون يمكنهم الاستعادة")
   → return

4. Selection validation:
   → var selected = Backups.FirstOrDefault(b => b.IsSelected)
   → if (selected is null) { _dialogService.ShowWarning("يُرجى تحديد نسخة احتياطية"); return; }

5. Confirmation (FD-10, step 2):
   → if (!_dialogService.ShowConfirmation("هل أنت متأكد من استعادة هذه النسخة؟ سيتم استبدال جميع البيانات الحالية.", "تأكيد الاستعادة"))
   → return

6. IsBusy = true

7. Open BackupPasswordDialog (FD-10, step 3):
   → var dialog = new BackupPasswordDialog { Owner = Window.GetWindow(this) }
   → if (dialog.ShowDialog() != true || dialog.EnteredPassword is null)
   → IsBusy = false; return

8. Call service (FD-10, step 4):
   → try { var success = await _backupService.RestoreBackupAsync(selected.FilePath, dialog.EnteredPassword); }
   → catch (DirectoryNotFoundException) { _dialogService.ShowError("ملف النسخة الاحتياطية غير موجود"); }
   → catch (IOException ex) { _dialogService.ShowError($"خطأ في القراءة: {ex.Message}"); }
   → catch (UnauthorizedAccessException) { _dialogService.ShowError("لا توجد صلاحية قراءة"); }
   → catch (Exception ex) { _dialogService.ShowError($"خطأ غير متوقع: {ex.Message}"); }

9. On success (FD-10, step 5):
   → _dialogService.ShowMessage("تمت الاستعادة بنجاح. يُرجى إعادة تشغيل التطبيق.")

10. Shutdown (FD-10, step 6, FD-02):
    → if (_dialogService.ShowConfirmation("هل تريد إغلاق التطبيق الآن؟"))
    → RequestShutdown?.Invoke()   // callback set by BackupRestoreWindow code-behind

11. IsBusy = false
```

---

## 14. Error Handling Matrix

| Error Condition | Where | Handling |
|----------------|-------|----------|
| User is not admin | `CreateBackupAsync`, `RestoreAsync` (start) | `_dialogService.ShowError("فقط المسؤولون يمكنهم تنفيذ هذا الإجراء")` + return |
| No backup selected | `RestoreAsync` (selection check) | `_dialogService.ShowWarning("يُرجى تحديد نسخة احتياطية")` + return |
| Password dialog cancelled | Both flows | `IsBusy = false; return` (silent — no error message) |
| Password too short (< 8) | `BackupPasswordDialog.OK_Click` | `ErrorText.Text = "كلمة المرور يجب أن تكون 8 أحرف على الأقل"` |
| Passwords mismatch | `BackupPasswordDialog.OK_Click` | `ErrorText.Text = "كلمتا المرور غير متطابقتين"` |
| Password empty | `BackupPasswordDialog.OK_Click` | `ErrorText.Text = "يُرجى إدخال كلمة المرور"` |
| Folder not found | `CreateBackupAsync`, `RestoreAsync` | `_dialogService.ShowError("المجلد غير موجود")` |
| Disk full / IO error | `CreateBackupAsync` | `_dialogService.ShowError($"خطأ في الكتابة: {ex.Message}")` |
| Permission denied | `CreateBackupAsync`, `RestoreAsync` | `_dialogService.ShowError("لا توجد صلاحية")` |
| Wrong password / invalid file | `RestoreAsync` (service returns false) | `_dialogService.ShowError("كلمة المرور غير صحيحة أو الملف تالف")` |
| Folder open fails | `OpenFolder` | `_dialogService.ShowError($"تعذّر فتح المجلد: {ex.Message}")` |
| Any unexpected exception | All async methods | `catch (Exception ex) { _dialogService.ShowError($"خطأ غير متوقع: {ex.Message}"); }` |

---

## 15. State Persistence Rules

| Property | Read From | Write To | When |
|----------|-----------|----------|------|
| `TargetFolder` | `IBackupService.GetBackupOutputFolderAsync()` | `IBackupService.SaveBackupOutputFolderAsync(newPath, staffId)` | On `BrowseFolderCommand` completion, before updating UI |
| `LastBackupAt` | Computed from latest `BackupMetadataDto.CreatedAt` in `Backups` collection | Not persisted (derived) | After each `LoadBackupsAsync()` |

**BackupOutputFolder default fallback:** `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FinalLabBackups")`

**StaffId for persistence:** `_currentUserSession.CurrentUser?.StaffId ?? 0`

---

## 16. Required Tests (25 Tests)

### Category 1: BackupRowViewModel (3 tests)

| # | Test Name | Asserts |
|---|-----------|---------|
| T6.3.1 | `WrapsDto_ExposesAllFields` | `FileName`, `FilePath`, `DisplaySize`, `DisplayCreatedAt` match DTO input |
| T6.3.2 | `DisplaySize_FormatsBytes_HumanReadable` | `FileSizeBytes=1048576` → `DisplaySize == "1 MB"` |
| T6.3.3 | `DisplayCreatedAt_ConvertsUTC_ToLocal` | `DateTime.UtcNow` → converted to local time string |

### Category 2: BackupRestoreWindowViewModel (14 tests)

| # | Test Name | Asserts |
|---|-----------|---------|
| T6.3.4 | `LoadBackupsCommand_PopulatesCollection_FromService` | Mock returns 3 items → `Backups.Count == 3` |
| T6.3.5 | `CreateBackupCommand_NonAdmin_ShowsError_DoesNotCallService` | `IsAdmin = false` → `ShowError` called, `CreateBackupAsync` NOT called |
| T6.3.6 | `CreateBackupCommand_AdminUser_OpensPasswordDialog` | `IsAdmin = true` → dialog opened (mock verifies) |
| T6.3.7 | `CreateBackupCommand_DirectoryNotFoundException_ShowsError` | Service throws `DirectoryNotFoundException` → `ShowError` called |
| T6.3.8 | `CreateBackupCommand_IOException_ShowsError` | Service throws `IOException` → `ShowError` with message |
| T6.3.9 | `CreateBackupCommand_UnauthorizedAccessException_ShowsError` | Service throws `UnauthorizedAccessException` → `ShowError` called |
| T6.3.10 | `CreateBackupCommand_PasswordDialogCancelled_DoesNotCallService` | Dialog returns null → `CreateBackupAsync` NOT called |
| T6.3.11 | `CreateBackupCommand_OnSuccess_RefreshesBackupList` | After create → `ListBackupsAsync` called again |
| T6.3.12 | `RestoreCommand_NoSelection_ShowsWarning` | `IsSelected = false` on all rows → `ShowWarning` called |
| T6.3.13 | `RestoreCommand_NonAdmin_ShowsError` | `IsAdmin = false` → `ShowError` called |
| T6.3.14 | `RestoreCommand_UserCancelsConfirmation_DoesNotCallService` | `ShowConfirmation` returns `false` → `RestoreBackupAsync` NOT called |
| T6.3.15 | `RestoreCommand_PasswordDialogCancelled_DoesNotCallService` | Dialog returns null → `RestoreBackupAsync` NOT called |
| T6.3.16 | `RestoreCommand_OnSuccess_ShowsSuccessMessage` | Service returns `true` → `ShowMessage` called with success text |
| T6.3.17 | `RestoreCommand_OnSuccess_InvokesRequestShutdown` | Service returns `true` → `RequestShutdown` callback invoked |

### Category 3: Additional ViewModel Tests (3 tests)

| # | Test Name | Asserts |
|---|-----------|---------|
| T6.3.18 | `IsBusy_TrueDuringOperation_FalseAfter` | `IsBusy` toggles during async operation |
| T6.3.19 | `BrowseFolderCommand_UpdatesTargetFolder` | After browse → `TargetFolder` updated, persisted via `IBackupService` |
| T6.3.20 | `OpenFolderCommand_FolderNotExists_ShowsError` | Non-existent path → `ShowError` called, no exception thrown |

### Category 4: BackupMenuViewModel (1 test)

| # | Test Name | Asserts |
|---|-----------|---------|
| T6.3.21 | `OpenBackupCommand_CallsShowCustomDialog` | `OpenBackupCommand.Execute()` → `IDialogService.ShowCustomDialog<BackupRestoreWindow>()` called |

### Category 5: Registration / Integration (4 tests)

| # | Test Name | Asserts |
|---|-----------|---------|
| T6.3.22 | `DI_Resolves_BackupRestoreWindowViewModel` | `TestServiceProvider.GetRequiredService<BackupRestoreWindowViewModel>()` resolves |
| T6.3.23 | `DI_Resolves_BackupRestoreWindow` | `TestServiceProvider.GetRequiredService<BackupRestoreWindow>()` resolves |
| T6.3.24 | `DI_Resolves_BackupPasswordDialog` | `TestServiceProvider.GetRequiredService<BackupPasswordDialog>()` resolves |
| T6.3.25 | `BackupRestoreWindow_SetsRequestShutdownCallback` | After construction, `viewModel.RequestShutdown` is not null and invokes `INavigationService.Shutdown()` |

---

## 17. Validation Gate G6.3

**Expected cumulative test count: 605 passing tests.**

Additional verification points:
- `grep -n "PlaceholderCommand" FinalLabSystem/ViewModels/Menu/BackupMenuViewModel.cs` returns **0 results**
- `grep -n "NavigateCommand" FinalLabSystem/ViewModels/Menu/BackupMenuViewModel.cs` returns **0 results**
- `BackupMenuViewModel` constructor takes `IDialogService` (not `INavigationService`)
- `BackupRestoreWindow.xaml.cs` contains `viewModel.RequestShutdown = () => navigationService.Shutdown();`
- `BackupPasswordDialog.xaml.cs` contains only `InitializeComponent()`, constructor, and event handlers (matching `CashDrawerUnlockDialog` pattern)
- `IDialogService` contains `ShowCustomDialog<T>()` method
- `DialogService` implements `ShowCustomDialog<T>()` using `IServiceProvider.GetRequiredService<T>()`
- `IBackupService` contains `GetBackupOutputFolderAsync()` and `SaveBackupOutputFolderAsync()`
- `dotnet build` produces 0 errors, 0 warnings

---

## 18. Explicit Non-Goals

| Item | Reason Excluded |
|------|----------------|
| SecureString usage | FD-01: Inconsistent with project patterns, adds unnecessary complexity |
| Singleton lifetime for any ViewModel/Window | FD-05: All Windows in project are Transient |
| `IMenuViewModelFactory` for MainViewModel | Out of scope — current `new` pattern is acceptable per work_plan.md notes |
| Automatic backup scheduling | Out of scope for Slice 6.3 (future slice) |
| Backup encryption strength testing | Already covered in Slice 6.2 (`AesEncryptionHelperTests`) |
| `Application.Current.Shutdown()` usage in ViewModels | FD-02: Must use `INavigationService.Shutdown()` via `RequestShutdown` callback |
| `BackupPasswordDialogViewModel` | FD-04: Code-behind pattern only, no ViewModel |
| `RegisterWindow` for `BackupRestoreWindow` | FD-11: Uses `ShowCustomDialog` pattern instead of navigation |
| `new T()` in `ShowCustomDialog` | FD-11: Must use `IServiceProvider.GetRequiredService<T>()` to preserve DI |
| `ISettingsService` for TargetFolder | FD-06: `IBackupService` owns backup-related settings |
| Natigh.com integration | Permanently excluded from project scope |

---

## 19. Execution Constraints

1. **No code changes outside the 6 files listed in Sections 4 and 5.** Do not modify any other existing files.

2. **No new NuGet packages.** `Microsoft.Win32.OpenFolderDialog` is available in WPF (.NET 8) without extra references.

3. **All DateTime usage must be `DateTime.UtcNow`** in seed data and assertions (never `DateTime.Now`).

4. **All Moq setups must use `It.IsAny<>()`** for non-critical parameters.

5. **All test files go in `FinalLabSystem.Tests/`** following existing folder structure.

6. **DI registration tests must use `TestServiceProvider`** pattern from existing tests.

7. **No `App.ServiceProvider.GetRequiredService<>()` calls in ViewModels** — this is a hard rule from Slice 6.0.

8. **No `new BackupPasswordDialog()` in ViewModels** — the dialog is created directly (code-behind pattern), not via DI, because it has no constructor dependencies.

9. **`ShowCustomDialog<T>()` uses `IServiceProvider.GetRequiredService<T>()`** — never `new T()`.

10. **Build must pass with 0 errors and 0 warnings** before considering the slice complete.

11. **Existing 544+ tests from Phases 1-5 must continue passing** without modification.

12. **`dotnet test` must show 605/605 passing** at Validation Gate G6.3.

---

*This document is a complete and final product of architectural analysis. No need to re-read or re-inspect the source files mentioned above — begin implementation directly based on what is documented here.*

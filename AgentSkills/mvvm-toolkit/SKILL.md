---
name: mvvm-toolkit
description: 'CommunityToolkit.Mvvm (the MVVM Toolkit) core: source generators ([ObservableProperty], [RelayCommand], [NotifyPropertyChangedFor], [NotifyCanExecuteChangedFor], [NotifyDataErrorInfo]), base classes (ObservableObject / ObservableValidator / ObservableRecipient), commands (RelayCommand / AsyncRelayCommand), and validation. Companion skills: mvvm-toolkit-messenger for pub/sub, mvvm-toolkit-di for Microsoft.Extensions.DependencyInjection wiring. Works across WPF, WinUI 3, MAUI, Uno, and Avalonia.'
---

# CommunityToolkit.Mvvm (core)

> ⚠️ **تحذير حرج — خاص بمشروع FinalLabSystem:**
>
> هذا المشروع يستخدم **`ViewModelBase` مخصصاً** (موجود في `ViewModels/ViewModelBase.cs`) وليس `ObservableObject` من الـ Toolkit.
>
> - ❌ **لا تستبدل `ViewModelBase` بـ `ObservableObject`** — سيكسر كل الـ ViewModels
> - ❌ **لا تضيف `[ObservableProperty]`** على ViewModel يرث من `ViewModelBase`
> - ❌ **لا تستخدم `[RelayCommand]` من الـ Toolkit** إذا كان الـ ViewModel يرث `ViewModelBase`
> - ✅ ورّث جميع الـ ViewModels الجديدة من `ViewModelBase` كما هو
> - ✅ استخدم هذه المهارة **فقط** للاطلاع على مفاهيم الـ Toolkit أو إذا أنشأت ViewModel مستقلاً لا يرث `ViewModelBase`
>
> **قبل أي عمل على ViewModel، افتح `ViewModels/ViewModelBase.cs` وافهم ما تقدمه أولاً.**

---

## متى تستخدم هذه المهارة في FinalLabSystem

| موقف | الإجراء |
|------|---------|
| إنشاء ViewModel جديد | ✅ ورّث `ViewModelBase` — لا تستخدم `ObservableObject` |
| فهم مفهوم `RelayCommand` | ✅ هذه المهارة مرجع للفهم |
| استخدام `[RelayCommand]` كـ attribute | ❌ ممنوع — المشروع يستخدم `RelayCommand` يدوياً |
| استخدام `[ObservableProperty]` | ❌ ممنوع على ViewModels ترث `ViewModelBase` |
| ViewModel مستقل (مثل dialog helper) | ⚠️ استشر أولاً هل يجب أن يرث `ViewModelBase` |

---

Use this skill when authoring or reviewing ViewModels, properties,
commands, or validation in apps that use `CommunityToolkit.Mvvm` 8.x.

> **Companion skills.** Load **`mvvm-toolkit-messenger`** for `IMessenger`
> pub/sub patterns. Load **`mvvm-toolkit-di`** for
> `Microsoft.Extensions.DependencyInjection` integration.

> **Quick recap.** `[ObservableProperty]` on private fields in `partial`
> classes; `[RelayCommand]` on instance methods; inherit from
> `ObservableObject` (or `ObservableValidator` for input forms,
> `ObservableRecipient` when using `IMessenger`).

---

## Package & setup

```xml
<ItemGroup>
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
</ItemGroup>
```

Targets: `netstandard2.0`, `netstandard2.1`, `net6.0`+. Works on .NET, .NET
Framework, Mono. Source generators ship in the same NuGet — no extra
analyzer reference required.

Namespaces:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;   // ObservableObject, [ObservableProperty]
using CommunityToolkit.Mvvm.Input;             // [RelayCommand], RelayCommand, AsyncRelayCommand
```

> **Universal rule.** Every type that uses `[ObservableProperty]` or
> `[RelayCommand]` — and every enclosing type, if nested — must be
> declared `partial`. Without it, the generators emit
> `MVVMTK0008` / `MVVMTK0042`.

---

## Source generators cheat sheet

| Attribute | Applied to | Generates |
|-----------|-----------|-----------|
| `[ObservableProperty]` | private field | Public `INotifyPropertyChanged` property + `OnXxxChanging`/`OnXxxChanged` partial-method hooks |
| `[NotifyPropertyChangedFor(nameof(Other))]` | observable field | Also raises `PropertyChanged` for the listed property |
| `[NotifyCanExecuteChangedFor(nameof(MyCommand))]` | observable field | Calls `MyCommand.NotifyCanExecuteChanged()` on change |
| `[NotifyDataErrorInfo]` | observable field on `ObservableValidator` | Calls `ValidateProperty(value)` from the setter |
| `[NotifyPropertyChangedRecipients]` | observable field on `ObservableRecipient` | `Broadcast(old, new)` after the change |
| `[RelayCommand]` | instance method | Lazy `RelayCommand` / `AsyncRelayCommand` exposed as `IRelayCommand` / `IAsyncRelayCommand` |
| `[RelayCommand(CanExecute = nameof(CanX))]` | instance method | Wires `CanExecute` to a method or property |
| `[RelayCommand(IncludeCancelCommand = true)]` | async method with `CancellationToken` | Also generates `XxxCancelCommand` |
| `[RelayCommand(AllowConcurrentExecutions = true)]` | async method | Allows queued/parallel invocations (default disables while running) |
| `[RelayCommand(FlowExceptionsToTaskScheduler = true)]` | async method | Surfaces exceptions via `ExecutionTask` instead of awaiting and rethrowing |
| `[property: SomeAttr]` | observable field or `[RelayCommand]` method | Forwards `SomeAttr` onto the generated property (e.g., `[JsonIgnore]`) |

**Naming.** Field `name` / `_name` / `m_name` → `Name`. Method `LoadAsync` →
`LoadCommand` (the `Async` suffix is stripped; a leading `On` is also
stripped).

See [`references/source-generators.md`](references/source-generators.md) for
the full attribute reference with generated-code samples.

---

## ViewModel patterns

### Simple observable property

```csharp
public partial class ContactViewModel : ObservableObject
{
    [ObservableProperty]
    private string? name;
}
```

### Hooks: `OnXxxChanging` / `OnXxxChanged`

```csharp
[ObservableProperty]
private string? name;

partial void OnNameChanged(string? value) =>
    Logger.LogInformation("Name changed to {Name}", value);
```

Both single-arg `(value)` and two-arg `(oldValue, newValue)` overloads
are available. Implement only the ones you need; unimplemented hooks are
elided by the compiler (zero runtime cost).

### Dependent properties + dependent commands

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private string? firstName;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private string? lastName;

public string FullName => $"{FirstName} {LastName}".Trim();
```

---

## Commands

```csharp
[RelayCommand]
private void Refresh() => Items.Reset();

[RelayCommand]
private async Task LoadAsync()
{
    foreach (var item in await service.GetItemsAsync())
        Items.Add(item);
}

[RelayCommand(IncludeCancelCommand = true)]
private async Task DownloadAsync(CancellationToken token)
{
    await using var stream = await http.GetStreamAsync(url, token);
    // ...
}

[RelayCommand(CanExecute = nameof(CanSave))]
private Task SaveAsync() => repo.SaveAsync(Name!);

private bool CanSave() => !string.IsNullOrWhiteSpace(Name);
```

---

## Base class selection

| Base class | Use when |
|------------|---------|
| `ObservableObject` | Default. `INotifyPropertyChanged` + `INotifyPropertyChanging` + `SetProperty` overloads |
| `ObservableValidator` | The VM needs `INotifyDataErrorInfo` (forms, settings input) |
| `ObservableRecipient` | The VM sends or receives `IMessenger` messages |

---

## Top pitfalls

1. **Forgetting `partial`.** Class must be `partial`. Compile error `MVVMTK0008`.
2. **PascalCase field name.** `[ObservableProperty] private string Name;` collides with the generated property. Use `name`, `_name`, or `m_name`.
3. **`async void` on `[RelayCommand]`.** Always return `Task`.
4. **Forgetting `[NotifyCanExecuteChangedFor]`.** The Save button stays disabled even though `CanSave()` would now return `true`.

---

## References & companion skills

| Topic | Where |
|-------|-------|
| Source generator attribute reference | [`references/source-generators.md`](references/source-generators.md) |
| RelayCommand recipes | [`references/relaycommand-cookbook.md`](references/relaycommand-cookbook.md) |
| Validation deep dive | [`references/validation.md`](references/validation.md) |
| Full walkthrough | [`references/end-to-end-walkthrough.md`](references/end-to-end-walkthrough.md) |
| `MVVMTK0xxx` diagnostics | [`references/troubleshooting.md`](references/troubleshooting.md) |
| **Messenger pub/sub** | Companion skill: **`mvvm-toolkit-messenger`** |


---
name: wpf-mvvm-conventions
description: Apply the project's strict MVVM conventions when creating or modifying WPF ViewModels, Views, and code-behind in FinalLabSystem. Trigger on any request to add a new screen, refactor a ViewModel, write a RelayCommand, handle INotifyDataErrorInfo validation, or bind XAML. Do NOT use for service-layer work, EF Core schema design, or any non-MVVM backend concern (route to the relevant skill). Stops any code-behind that touches business logic, any direct DbContext call from a ViewModel, or any use of CommunityToolkit's ObservableObject instead of the project's custom ViewModelBase.
---

# WPF MVVM Conventions (FinalLabSystem)

This codebase enforces a strict, opinionated MVVM. Every screen follows the same skeleton. New screens must conform or they will be rejected in review.

## The three non-negotiables

1. **All ViewModels inherit `Infrastructure.ViewModelBase`** (NOT `ObservableObject` from CommunityToolkit). The custom base provides `INotifyDataErrorInfo`; replacing it breaks DataGrid validation and error adorners.
2. **All commands inherit `Infrastructure.RelayCommand` or `Infrastructure.AsyncRelayCommand`**. Do not introduce `CommunityToolkit.Mvvm.Input.RelayCommand` directly, even though the package is referenced.
3. **Code-behind contains zero business logic.** Only `InitializeComponent`, `Loaded`/`Closed` handlers, and `Dispatcher.Invoke` when the VM must run on UI thread. No `if`, no DB calls, no navigation calls.

## ViewModel skeleton

```csharp
using CommunityToolkit.Mvvm.ComponentModel; // ONLY for [ObservableProperty] if you migrate; project uses SetProperty from base
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.<Area>;

public sealed class <Name>ViewModel : ViewModelBase
{
    private string _someField = string.Empty;
    public string SomeField
    {
        get => _someField;
        set => SetProperty(ref _someField, value);
    }

    public AsyncRelayCommand SaveAsyncCommand { get; }

    public <Name>ViewModel(ISomeService service, ILogger<<Name>ViewModel> logger)
    {
        _service = service;
        SaveAsyncCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    private bool CanSave() => !HasErrors && !IsBusy;
    private async Task SaveAsync() { /* see references/error-and-busy.md */ }
}
```

Rules enforced:
- Constructor injection only. No `ServiceLocator`, no `App.ServiceProvider`.
- One ViewModel per screen. Sub-ViewModels live in `<Name>ViewModel.Nested.cs` or under the same folder with name `<Name>RowViewModel`.
- `SetProperty` (from `ViewModelBase`) — not `Set` from CommunityToolkit. The base also fires `OnPropertyChanged` for computed properties when `SetProperty` returns true.

## View skeleton

```xml
<Window x:Class="FinalLabSystem.Views.<Area>.<Name>Window"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:FinalLabSystem.ViewModels.<Area>"
        d:DataContext="{d:DesignInstance Type=vm:<Name>ViewModel}"
        mc:Ignorable="d" xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        FlowDirection="RightToLeft" Language="ar-SA">
    <Window.Resources>
        <!-- converters, styles -->
    </Window.Resources>
    <!-- never set DataContext here; set by NavigationService -->
</Window>
```

Rules:
- `FlowDirection="RightToLeft"` on every Window. The app is Arabic-first.
- `d:DataContext` is mandatory for design-time IntelliSense and blend preview.
- `DataContext` is assigned by `NavigationService.OpenTaskWindow<TViewModel>(configure)` — never set in XAML.
- No `Click="..."` handlers in XAML. Wire commands via `Command="{Binding ...Command}"`.

## Command conventions

| Need | Use |
|---|---|
| Sync action, simple canExecute | `new RelayCommand(action, canExecute)` |
| Async work, may show progress | `new AsyncRelayCommand(action, canExecute)` |
| Async with cancel | `new AsyncRelayCommand(action, canExecute, cancel)` |
| Per-row in DataGrid | Bind `CommandParameter` to the row VM |

`IsBusy` is a property on `ViewModelBase` that auto-flips during `AsyncRelayCommand` execution. Bind progress bars and disable forms to `IsBusy`. Use `BeginBusy("جاري الحفظ...")` / `EndBusy()` for manual control.

## Validation (INotifyDataErrorInfo)

```csharp
private void ValidateNonEmpty(string? value)
{
    ClearErrors(nameof(FullNameAr));
    if (string.IsNullOrWhiteSpace(value))
        AddError(nameof(FullNameAr), "الاسم بالعربية مطلوب");
}
```

Rules:
- Call validators from property setters, not from `SaveAsync`. Save just inspects `HasErrors`.
- Error messages are Arabic strings (see `wpf-localization-ar-en` skill for future migration).
- Use `ClearErrors(propertyName)` before re-adding to avoid duplicates.

## Forbidden patterns

- `new ObservableObject()` directly.
- `ICommand` implementations that are not the project's `RelayCommand`/`AsyncRelayCommand`.
- `Code-behind` setting `DataContext`, calling services, or navigating.
- `Application.Current.MainWindow` from a ViewModel — use `INavigationService`.
- `Task.Run` inside an `AsyncRelayCommand` — the command already marshals to the UI thread on completion.
- Static `App.ServiceProvider` lookups — constructor injection only.

## Where things live

```
FinalLabSystem/
  Infrastructure/
    ViewModelBase.cs        ← base class, do not edit lightly
    RelayCommand.cs
    AsyncRelayCommand.cs
  ViewModels/
    Patients/               ← one folder per area
      <Name>ViewModel.cs
    Menu/                   ← top-level menu screens
    Settings/               ← settings screens
  Views/
    Patients/
      <Name>Window.xaml + .xaml.cs
```

## Quick checklist for new screens

- [ ] VM inherits `ViewModelBase`
- [ ] VM ctor takes services via DI
- [ ] Commands are `RelayCommand`/`AsyncRelayCommand` from project
- [ ] XAML has `FlowDirection="RightToLeft"` and `d:DataContext`
- [ ] No `Click` handlers; no code-behind business logic
- [ ] `DataContext` is set by `NavigationService`, not in XAML
- [ ] Validation runs in setters, errors are Arabic
- [ ] Saved file paths follow the `ViewModels/<Area>/` and `Views/<Area>/` convention
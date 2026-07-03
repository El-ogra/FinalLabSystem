# Error handling and busy state in ViewModels

`ViewModelBase` exposes `IsBusy`, `BeginBusy(reason)`, `EndBusy()`, and `HasErrors`.

## Async command lifecycle

```csharp
public AsyncRelayCommand SaveAsyncCommand { get; }

public MyViewModel(...)
{
    SaveAsyncCommand = new AsyncRelayCommand(SaveAsync, () => !HasErrors && !IsBusy);
}

private async Task SaveAsync()
{
    BeginBusy("جاري الحفظ...");
    try
    {
        await _service.SaveAsync(...);
    }
    catch (DbUpdateException ex)
    {
        LogError(ex);
        AddError("", "تعذّر الحفظ. تحقق من الاتصال بقاعدة البيانات.");
    }
    finally
    {
        EndBusy();
    }
}
```

## Property-level vs object-level errors

- `AddError(nameof(FullNameAr), "...")` — surfaces next to the field.
- `AddError("", "...")` — surfaces at the form level (use empty property name).

Both flow through `INotifyDataErrorInfo`. The XAML `Validation.ErrorTemplate` already highlights bound fields; the empty-name pattern is for banner-style messages.

## Cancelling running work

```csharp
public AsyncRelayCommand LongWorkCommand { get; }

public MyViewModel(...)
{
    LongWorkCommand = new AsyncRelayCommand(
        execute: LongWorkAsync,
        canExecute: () => !IsBusy,
        cancel: () => _cts.Cancel());
}

private async Task LongWorkAsync()
{
    _cts = new CancellationTokenSource();
    BeginBusy("جاري العمل...");
    try
    {
        await _service.LongWorkAsync(_cts.Token);
    }
    catch (OperationCanceledException) { /* expected */ }
    finally
    {
        EndBusy();
        _cts.Dispose();
        _cts = null;
    }
}
```

The `CanExecute` re-evaluation after cancel re-enables the button.

## What "Busy" looks like in the UI

Bind a `Border`/`ProgressBar` `Visibility` to a converter on `IsBusy`. The codebase ships a `BoolToVisibilityConverter` for this — use it, do not write a new one.
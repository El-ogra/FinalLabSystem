---
name: mvvm-toolkit
description: "FinalLabSystem-customized MVVM guidance. This project does NOT use CommunityToolkit.Mvvm patterns ([ObservableProperty] / ObservableObject / [RelayCommand]) — it uses the project's custom Infrastructure.ViewModelBase + Infrastructure.RelayCommand / AsyncRelayCommand. All ViewModel patterns below are rewritten to match the project's real conventions. Keep CommunityToolkit reference at the bottom for concept lookup ONLY."
---

# MVVM Toolkit — FinalLabSystem Customized

> 🛑 **تحذير حرج — يجب قراءته أولاً قبل أي كود:**
>
> مشروع **FinalLabSystem** لا يتبع أنماط `CommunityToolkit.Mvvm` القياسية. رغم أن الحزمة قد تكون مُشار إليها في بعض الملفات، فإن **قواعد المشروع الملزمة** (المُوثّقة في `wpf-mvvm-conventions`) هي:
>
> | العنصر | القاعدة الملزمة في FinalLabSystem |
> |---|---|
> | فئة أساس ViewModel | `Infrastructure.ViewModelBase` (المخصصة) — **لا `ObservableObject`** |
> | الأمر (Command) | `Infrastructure.RelayCommand` / `Infrastructure.AsyncRelayCommand` — **لا `CommunityToolkit.Mvvm.Input.RelayCommand`** |
> | خصائص تُخطر بالتغيير | `SetProperty(ref _field, value)` يدوياً — **لا `[ObservableProperty]`** |
> | التحقق (Validation) | `AddError` / `ClearErrors` من `ViewModelBase` (تنفّذ `INotifyDataErrorInfo`) — **لا `ObservableValidator`** |
> | Source generators / attributes | ممنوعة — لا `[RelayCommand]`، لا `[NotifyPropertyChangedFor]` |
> | Class `partial` من أجل Toolkit | غير مطلوبة — الفئات عادية (`public sealed class ...`) |
>
> **راجع أولاً:** `AgentSkills/wpf-mvvm-conventions/SKILL.md` — هي المصدر المعتمد.
> **افتح أيضاً:** `FinalLabSystem/Infrastructure/ViewModelBase.cs` لمعرفة الأعضاء المحمية (`SetProperty`, `AddError`, `ClearErrors`, `HasErrors`).

---

## متى تستخدم هذه المهارة

| موقف | استخدام هذه المهارة؟ |
|------|----------------------|
| إنشاء ViewModel جديد في FinalLabSystem | ✅ نعم — اتبع الأنماط أدناه فقط |
| مراجعة كود ViewModel قائم | ✅ نعم — تحقق أنه يتّبع أنماط `ViewModelBase` |
| فهم مفاهيم `CommunityToolkit.Mvvm` نظرياً لمقارنتها | ⚠️ نعم — لكن فقط قسم "المرجع النظري" في النهاية |
| نسخ نمط `[ObservableProperty]` إلى المشروع | ❌ لا — سيُرفض في المراجعة |
| اقتراح ترحيل الكود لأنماط Toolkit | ❌ لا — قرار معماري مغلق |

---

## النمط المعتمد #1 — Simple Observable Property (بدلاً من `[ObservableProperty]`)

**❌ ممنوع في FinalLabSystem (نمط Toolkit التقليدي):**

```csharp
public partial class ContactViewModel : ObservableObject
{
    [ObservableProperty]
    private string? name;
}
```

**✅ النمط الصحيح لمشروع FinalLabSystem:**

```csharp
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Contacts;

public sealed class ContactViewModel : ViewModelBase
{
    private string? _name;
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

**قواعد:**
- الحقل يبدأ بـ `_` (underscore + camelCase).
- الخاصية عامة `PascalCase`.
- `SetProperty` من `ViewModelBase` تعيد `bool` تشير إن تغيّرت القيمة فعلاً.
- لا `partial`، لا `[ObservableProperty]`.

---

## النمط المعتمد #2 — Property Change Hooks (بدلاً من `partial void OnXxxChanged`)

**❌ ممنوع:**

```csharp
[ObservableProperty]
private string? name;

partial void OnNameChanged(string? value) => _logger.LogInformation("...");
```

**✅ الصحيح:**

```csharp
private string? _name;
public string? Name
{
    get => _name;
    set
    {
        if (SetProperty(ref _name, value))
        {
            OnNameChanged(value);
        }
    }
}

private void OnNameChanged(string? value)
{
    _logger.LogInformation("Name changed to {Name}", value);
}
```

`SetProperty` تعيد `true` إذا تغيرت القيمة، فتستدعي الـ hook يدوياً. هذا يعطي نفس نتيجة `partial void` من الـ Toolkit، مع بقاء الفئة عادية غير `partial`.

---

## النمط المعتمد #3 — Dependent Properties (بدلاً من `[NotifyPropertyChangedFor]`)

**❌ ممنوع:**

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
private string? firstName;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
private string? lastName;

public string FullName => $"{FirstName} {LastName}".Trim();
```

**✅ الصحيح:**

```csharp
private string? _firstName;
public string? FirstName
{
    get => _firstName;
    set
    {
        if (SetProperty(ref _firstName, value))
        {
            OnPropertyChanged(nameof(FullName));
            SaveCommand.RaiseCanExecuteChanged();
        }
    }
}

private string? _lastName;
public string? LastName
{
    get => _lastName;
    set
    {
        if (SetProperty(ref _lastName, value))
        {
            OnPropertyChanged(nameof(FullName));
            SaveCommand.RaiseCanExecuteChanged();
        }
    }
}

public string FullName => $"{FirstName} {LastName}".Trim();
```

- `OnPropertyChanged(nameof(...))` متاحة `protected` من `ViewModelBase`.
- إعادة تقييم `CanExecute` للأوامر يتم بـ `RaiseCanExecuteChanged()` على مثيل الأمر.

---

## النمط المعتمد #4 — Commands (بدلاً من `[RelayCommand]`)

**❌ ممنوع:**

```csharp
[RelayCommand]
private async Task SaveAsync() { ... }

[RelayCommand(CanExecute = nameof(CanSave))]
private Task LoadAsync() { ... }
```

**✅ الصحيح:**

```csharp
using FinalLabSystem.Infrastructure;

public sealed class OrderViewModel : ViewModelBase
{
    private readonly IOrderService _orderService;

    public AsyncRelayCommand SaveAsyncCommand { get; }
    public AsyncRelayCommand LoadAsyncCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public OrderViewModel(IOrderService orderService)
    {
        _orderService = orderService;

        SaveAsyncCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        LoadAsyncCommand = new AsyncRelayCommand(LoadAsync);
        RefreshCommand   = new RelayCommand(Refresh);
    }

    private bool CanSave() => !HasErrors && !IsBusy;

    private async Task SaveAsync()   { await _orderService.SaveAsync(...); }
    private async Task LoadAsync()   { /* ... */ }
    private void      Refresh()      { /* ... */ }
}
```

**قواعد:**
- كل أمر يُعرَّف كـ `public ... Command { get; }` ويُبنى في المُنشئ (Constructor Injection).
- `CanExecute` تُمرَّر كدالة `Func<bool>` (وليس اسم عبر attribute).
- عند تغير الحالة، نادِ `SomeCommand.RaiseCanExecuteChanged()` يدوياً.

---

## النمط المعتمد #5 — Validation (بدلاً من `ObservableValidator` + `[NotifyDataErrorInfo]`)

**❌ ممنوع:**

```csharp
public partial class SignupVm : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string? email;
}
```

**✅ الصحيح (يعتمد على `INotifyDataErrorInfo` المدمج في `ViewModelBase`):**

```csharp
public sealed class SignupViewModel : ViewModelBase
{
    private string? _email;
    public string? Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                ValidateEmail();
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private void ValidateEmail()
    {
        ClearErrors(nameof(Email));
        if (string.IsNullOrWhiteSpace(_email))
        {
            AddError(nameof(Email), "البريد الإلكتروني مطلوب");
        }
        else if (!_email.Contains('@'))
        {
            AddError(nameof(Email), "بريد إلكتروني غير صالح");
        }
    }

    public AsyncRelayCommand SubmitCommand { get; }
    public SignupViewModel() { SubmitCommand = new AsyncRelayCommand(SubmitAsync, () => !HasErrors); }
    private Task SubmitAsync() { /* ... */ return Task.CompletedTask; }
}
```

`ViewModelBase` يوفّر:
- `AddError(propertyName, message)`
- `ClearErrors(propertyName)`
- `ClearAllErrors()`
- `HasErrors` (property)
- `GetErrors(propertyName)`

هذا يحرك عرض علامات الخطأ في XAML عبر `Validation.ErrorTemplate` تلقائياً.

---

## النمط المعتمد #6 — Cancelable Async Command (بدلاً من `IncludeCancelCommand`)

**✅ الصحيح:**

```csharp
public AsyncRelayCommand DownloadCommand { get; }
private CancellationTokenSource? _downloadCts;

public MyViewModel()
{
    DownloadCommand = new AsyncRelayCommand(DownloadAsync);
}

private async Task DownloadAsync()
{
    _downloadCts?.Cancel();
    _downloadCts = new CancellationTokenSource();
    try
    {
        await _service.DownloadAsync(_downloadCts.Token);
    }
    catch (OperationCanceledException) { /* silent */ }
}

public void CancelDownload() => _downloadCts?.Cancel();
```

بديلاً يمكن استخدام `AsyncRelayCommand` مع دعم Cancellation إذا كانت النسخة المخصصة في `Infrastructure/` تدعم ذلك (راجع الملف مباشرةً قبل الاستخدام).

---

## قائمة مراجعة سريعة قبل الـ Commit

- [ ] ViewModel يرث `Infrastructure.ViewModelBase` (لا `ObservableObject`)
- [ ] لا يوجد `[ObservableProperty]` ولا `[RelayCommand]` في الملف
- [ ] الفئة **ليست** `partial`
- [ ] كل خاصية تستخدم `SetProperty(ref _field, value)`
- [ ] الأوامر من `Infrastructure.RelayCommand` / `AsyncRelayCommand`
- [ ] `CanExecute` دالة `Func<bool>` مُمرَّرة للمُنشئ
- [ ] Validation عبر `AddError` / `ClearErrors`
- [ ] Constructor Injection (لا `ServiceLocator`)
- [ ] لا كود منطق أعمال في code-behind

---

## أخطاء شائعة يجب تجنبها

| الخطأ | الأثر | البديل |
|-------|-------|--------|
| استخدام `ObservableObject` بجانب `ViewModelBase` | كسر `INotifyDataErrorInfo` وعطب Adorners | ورث `ViewModelBase` وحده |
| نسيان `RaiseCanExecuteChanged` بعد تغيير خاصية | زر Save يبقى معطلاً/مفعّلاً بشكل خاطئ | نادِ `RaiseCanExecuteChanged()` صراحةً |
| استدعاء `DbContext` مباشرة من ViewModel | كسر معمارية الطبقات | مرّر عبر خدمة (Service) بـ DI |
| إعلان الفئة `partial` بلا سبب | إشارة أن المطوّر ينوي استخدام Toolkit | احذف `partial` |
| ترك attribute `[ObservableProperty]` "للتنظيم فقط" | Source generator سينشئ خاصية متعارضة | احذف الـ attribute |

---

## المهارات المرتبطة

- `wpf-mvvm-conventions` — **المصدر المعتمد** لقواعد MVVM في المشروع.
- `di-and-navigation-registration` — لتسجيل ViewModel و View في `App.xaml.cs`.
- `lis-xunit-testing-conventions` — لكتابة اختبارات ViewModels.
- `adding-new-lab-module` — سياق كامل لإضافة موديول من الصفر.

---

## 📚 المرجع النظري لـ CommunityToolkit.Mvvm (للفهم فقط — لا للنسخ)

> ⚠️ **الأقسام التالية موروثة من مرجع CommunityToolkit.Mvvm للفهم النظري فقط. لا تنقل هذه الأنماط إلى كود FinalLabSystem — استخدم الأنماط المخصصة أعلاه.**

### Source generators cheat sheet (concept reference)

| Attribute | Toolkit generates | Equivalent in FinalLabSystem |
|-----------|-------------------|-------------------------------|
| `[ObservableProperty]` | Property + change hooks | Manual `SetProperty(ref _f, value)` |
| `[NotifyPropertyChangedFor]` | Also raises PropertyChanged for another | Manual `OnPropertyChanged(nameof(Other))` inside setter |
| `[NotifyCanExecuteChangedFor]` | `Command.NotifyCanExecuteChanged()` | Manual `Command.RaiseCanExecuteChanged()` |
| `[NotifyDataErrorInfo]` | Calls `ValidateProperty` | Manual `AddError` / `ClearErrors` |
| `[RelayCommand]` | Lazy `IRelayCommand` from method | Explicit `new RelayCommand(fn, canFn)` in ctor |
| `[RelayCommand(CanExecute=...)]` | Auto-wires CanExecute | Pass `Func<bool>` to command ctor |

### Base classes (concept reference — do NOT use in FinalLabSystem)

| Toolkit base class | Purpose | FinalLabSystem replacement |
|-------------------|---------|----------------------------|
| `ObservableObject` | INotifyPropertyChanged + SetProperty | `Infrastructure.ViewModelBase` |
| `ObservableValidator` | Adds INotifyDataErrorInfo | Already in `Infrastructure.ViewModelBase` |
| `ObservableRecipient` | IMessenger integration | Not used — the project doesn't use Toolkit's Messenger |

### References (external, informational only)

| Topic | Where |
|-------|-------|
| Source generator attribute reference | [`references/source-generators.md`](references/source-generators.md) |
| RelayCommand recipes | [`references/relaycommand-cookbook.md`](references/relaycommand-cookbook.md) |
| Validation deep dive | [`references/validation.md`](references/validation.md) |
| Full walkthrough | [`references/end-to-end-walkthrough.md`](references/end-to-end-walkthrough.md) |
| `MVVMTK0xxx` diagnostics | [`references/troubleshooting.md`](references/troubleshooting.md) |

> **تذكير أخير:** كل ما في مجلد `references/` هو مرجع خارجي عام. **لا تنسخ منه إلى كود FinalLabSystem** — استخدم الأنماط المخصصة في هذه المهارة وفي `wpf-mvvm-conventions`.

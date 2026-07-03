---
name: di-and-navigation-registration
description: تسجيل أي Service/ViewModel/Window جديد في حاوية DI وفي NavigationService الخاص بـ FinalLabSystem بالترتيب الدقيق. يستند إلى الكود الفعلي في Infrastructure/Navigation/NavigationService.cs و App.xaml.cs ConfigureServices. لا يُستخدم عند إضافة migration أو كيان جديد فقط (ذلك لـ ef-core-migration-safety ولا يضيف شاشة).
---

# DI & Navigation Registration — FinalLabSystem

## القاعدة الأساسية

لا تُكتشف مكونات WPF تلقائيًا. كل `Service`, `ViewModel`, `Window` يجب تسجيلها يدويًا في `App.xaml.cs`. بدون هذا التسجيل تفشل النوافذ بخطأ `No service for type ...`، أو `No window is registered for ViewModel ...` في runtime.

## الهيكل الفعلي

ملف `App.xaml.cs` يحوي دالة `ConfigureServices(IServiceCollection services)`. بداخلها يوجد **مكانان منفصلان**:

1. **تسجيل DI نقي** (مثل `services.AddScoped<...>(...)`) — خدمات و ViewModels و Windows.
2. **تسجيل Navigation** (مثل `navigation.RegisterWindow<...>(...)`) — ربط ViewModel بـ Window.

النظام الفعلي المستخدم: `IServiceCollection` من `Microsoft.Extensions.DependencyInjection`، و `IServiceProvider` يُمرَّر إلى `NavigationService`.

## الخطوة 1 — تسجيل الـ Service

افتح `FinalLabSystem/App.xaml.cs` وابحث عن `ConfigureServices`. أضف في القسم المخصص للخدمات (عادة بعد الخدمات الموجودة المماثلة مع تجميع أبجدي):

```csharp
services.AddScoped<IEquipmentService, EquipmentService>();
services.AddScoped<IAuditService, AuditService>();  // ← إذا الموديول يحوي تدقيق
```

**القواعد الملاحظة من الكود الفعلي:**

| نمط الـ DI | متى يُستخدم |
|---|---|
| `AddScoped<>` | الخدمات التي تعتمد على DbContext (DbContext نفسه scoped). |
| `AddTransient<>` | النوافذ والـ ViewModels — ليس لها حالة مشتركة بين الجلسات. |
| لا `AddSingleton<>` إلا لـ thread-safe stateless helpers (مثل `PasswordHasher`, `AesEncryptionHelper` فهي static فعلًا). |

**ترتيب التسجيل** في `App.xaml.cs`: عادة أبجدي حسب اسم الواجهة. لا تكسر هذا الترتيب — يستخدمه الفريق للمراجعة البصرية. أضف السطر الجديد في مكانه الأبجدي.

مثال واقعي من الكود:
```csharp
services.AddScoped<ISettingsService, SettingsService>();
services.AddScoped<IFeatureToggleService, FeatureToggleService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IPatientService, PatientService>();
services.AddScoped<IReferralService, ReferralService>();
services.AddScoped<IVisitService, VisitService>();
// (في الوسط هنا يضاف السطر الجديد مثل services.AddScoped<IEquipmentService, EquipmentService>();)
services.AddScoped<IFinancialService, FinancialService>();
```

## الخطوة 2 — تسجيل الـ ViewModel

في نفس `ConfigureServices`:

```csharp
services.AddTransient<ViewModels.Settings.EquipmentWindowViewModel>();
```

السطر يذهب بعد خدمات Windows المماثلة، أو في كتلة مستقلة للـ VMs. تأكد من:

- الـ namespace صحيح. الـ ViewModels في `FinalLabSystem/ViewModels/<Area>/`.
- لا تكرر تسجيل VM — كل VM يسجَّل مرة واحدة.

## الخطوة 3 — تسجيل الـ Window

في نفس `ConfigureServices`:

```csharp
services.AddTransient<Views.Settings.EquipmentWindow>();
```

- الـ namespace `FinalLabSystem.Views.<Area>`.
- `Transient` لأن النافذة تحمل VMs scoped داخلها.

## الخطوة 4 — تسجيل في Navigation

ابحث عن `var navigation = ...` ثم عن حلقة أو كتلة `RegisterWindow`. أضف:

```csharp
navigation.RegisterWindow<EquipmentWindowViewModel, EquipmentWindow>();
```

**مثال واقعي** من `App.xaml.cs`:
```csharp
navigation.RegisterWindow<PatientRegistrationViewModel, PatientRegistrationWindow>();
navigation.RegisterWindow<TestResultsViewModel, TestResultsWindow>();
navigation.RegisterWindow<DeliveryViewModel, DeliveryWindow>();
navigation.RegisterWindow<PatientSearchViewModel, PatientSearchWindow>();
navigation.RegisterWindow<TestDataManagementViewModel, TestDataManagementWindow>();
navigation.RegisterWindow<NormalRangeWindowViewModel, NormalRangesWindow>();
```

**القواعد:**

- الترتيب الفعلي المستخدم في الكود: حسب الـ area (Settings → Visits → Reports → ...). اتبع الترتيب المناطق عند الإضافة.
- `TViewModel` يجب أن يكون نفس الـ class المستخدم في الخطوة 2.
- `TWindow` يجب أن يكون نفس الـ class المستخدم في الخطوة 3.

**بدون هذه الخطوة** — استدعاء `navigation.OpenTaskWindow<EquipmentWindowViewModel>()` يرمي:

```
InvalidOperationException: No window is registered for ViewModel 'EquipmentWindowViewModel'.
```

## الخطوة 5 — استدعاء الـ Navigation من القائمة

في `MainWindow.xaml` أو `ViewModels/Menu/`:

```xml
<MenuItem Header="إدارة المعدات" 
          Command="{Binding OpenEquipmentCommand}" />
```

في الـ ViewModel للقائمة:

```csharp
public ICommand OpenEquipmentCommand => _navigationService
    .OpenTaskWindowCommand<EquipmentWindowViewModel>();
```

(أو delegate يحوي `() => _navigationService.OpenTaskWindow<EquipmentWindowViewModel>()`)

## نمط `RegisterWindow` — كيف يعمل فعلياً

من `Infrastructure/Navigation/NavigationService.cs`:

```csharp
private readonly Dictionary<Type, Type> _viewModelToWindowMap = new();

public void RegisterWindow<TViewModel, TWindow>()
    where TViewModel : class
    where TWindow : Window
{
    _viewModelToWindowMap[typeof(TViewModel)] = typeof(TWindow);
}
```

ثم `OpenTaskWindow<TViewModel>()`:
1. يبحث عن type الـ Window في القاموس.
2. يحل Window من `IServiceProvider` (يجب أن يكون مسجل في DI).
3. يضبط `DataContext` (إذا الـ Window يقرأ VM من DI).
4. يخفي MainWindow، يعرض Task Window، يستعيد MainWindow عند الإغلاق.

## Checklist — تسجيل عنصر جديد

```
□ Service interface مُعرَّف (I<X>Service في Services/Interfaces/)
□ Service implementation مُنفَّذ
□ DbContext factory إن لزم (عادة لا)
□ ViewModel موجود في ViewModels/<Area>/
□ Window (XAML + .cs) في Views/<Area>/
□ services.AddScoped<I<X>Service, <X>Service>() في ConfigureServices (ترتيب أبجدي)
□ services.AddTransient<<X>WindowViewModel>()
□ services.AddTransient<Views.<Area>.<X>Window>()
□ navigation.RegisterWindow<<X>WindowViewModel, <X>Window>() في نفس الترتيب الذي يستخدمه الفريق
□ Menu item يربط _navigationService.OpenTaskWindow<<X>WindowViewModel>()
□ Build نظيف، smoke test: فتح النافذة → إغلاقها → رجوع لـ MainWindow
```

## الفشل المنمط

| العَرَض | السبب | الحل |
|---|---|---|
| "No service for type 'I<X>Service'..." | نسيت `AddScoped` | أضف في ConfigureServices |
| "No window is registered for ViewModel" | نسيت `RegisterWindow` | أضف في كتلة navigation |
| "Window is not registered in the DI container" | نسيت `AddTransient` للـ Window | أضف في ConfigureServices |
| الـ Window يفتح فارغ | الـ VM لم يُربط في الـ DataContext | تحقق من `XAML`: `d:DataContext` وكود `InitializeComponent()` |
| MainWindow لا يرجع بعد إغلاق Window | لم تُربط `Closed` event | تحقق أن الـ OpenTaskWindow تضيف `Closed += OnTaskWindowClosed` — افتراضيًا موجود |

## قيد مهم: ترتيب Constructor

النظام يستخدم constructor injection تقليدي (لا primary constructors). خدمة مثل `EquipmentService`:

```csharp
public sealed class EquipmentService : IEquipmentService
{
    private readonly FinalLabDbContext _db;
    private readonly ICurrentUserSession _session;
    private readonly IAuditService _audit;
    private readonly ILogger<EquipmentService> _log;

    public EquipmentService(
        FinalLabDbContext db,
        ICurrentUserSession session,
        IAuditService audit,
        ILogger<EquipmentService> log)
    {
        _db = db;
        _session = session;
        _audit = audit;
        _log = log;
    }
    // ...
}
```

الـ DI container يحلّه تلقائيًا لأنك سجّلت كل واحدة في `ConfigureServices`. لا تحتاج تسجيل إضافي للـ `ILogger<>` — `AddLogging()` مضبوط في `App.xaml.cs`.

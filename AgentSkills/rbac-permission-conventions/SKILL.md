---
name: rbac-permission-conventions
description: نظام الصلاحيات الفعلي المطبَّق في FinalLabSystem: كيانات Permission/StaffPermission، كود الصلاحية PermissionCode، ربط الصلاحية بـ Staff، كيف تحرس قائمة MenuItem، كيف تحرس Action في ViewModel، سيناريوهات الفرق (فني/طبيب/محاسب/استقبال). يستند إلى الكود الفعلي في Models/Permission.cs و StaffPermission.cs و AuthService.
---

# RBAC & Permission Conventions — FinalLabSystem

## النظام الفعلي

النظام مبني على جداول `Permission` و `StaffPermission` فقط (لا توجد جداول `Role` صريحة). كل صلاحية لها **رمز** فريد `PermissionCode` (مثل `PATIENT.MANAGE`، `RESULT.VALIDATE`). كل موظف عنده قائمة صريحة بالصلاحيات المسموحة أو الممنوعة. الموظفون الذين `IsAdmin = true` يتجاوزون كل التحققات — هذا التحكم ثنائي المستوى.

## الكيانات الأساسية

من `Models/Permission.cs`:

```csharp
public partial class Permission
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = null!;     // UNIQUE
    public string PermissionName { get; set; } = null!;
    public string PermissionGroup { get; set; } = null!;    // e.g. "PATIENT", "RESULT"
    public string? Description { get; set; }
}
```

من `Models/StaffPermission.cs`:

```csharp
public partial class StaffPermission
{
    public int StaffPermId { get; set; }
    public int StaffId { get; set; }                       // FK → Staff
    public int PermissionId { get; set; }                   // FK → Permission
    public bool IsGranted { get; set; }                    // true = granted, false = denied
    public int? GrantedBy { get; set; }                    // FK → Staff
    public DateTime GrantedAt { get; set; }
}
```

من `Models/Staff.cs`:

```csharp
public partial class Staff
{
    // ... حقول أخرى
    public bool IsAdmin { get; set; }    // تجاوز كامل لكل الصلاحيات
    public bool IsActive { get; set; }   // تعطيل عام → لا login
}
```

## اصطلاح تسمية `PermissionCode`

النمط الفعلي المعتمد في البيانات الموجودة:

```
<MODULE>.<ACTION>
<MODULE>.<SUBMODULE>.<ACTION>
```

| PermissionCode الفعلي | المعنى |
|---|---|
| `PATIENT.MANAGE` | إنشاء/تعديل/حذف المرضى |
| `PATIENT.SEARCH` | البحث عن مريض في شاشة الاستقبال |
| `VISIT.CREATE` | تسجيل زيارة جديدة |
| `RESULT.ENTER` | إدخال نتيجة |
| `RESULT.VALIDATE` | اعتماد نتيجة (senior فقط) |
| `RESULT.RELEASE` | إصدار نتيجة للطباعة |
| `BILLING.MANAGE` | إدارة الفواتير |
| `PAYMENT.COLLECT` | تحصيل دفعة |
| `BACKUP.CREATE` | إنشاء نسخة احتياطية |
| `BACKUP.RESTORE` | استرجاع نسخة احتياطية (ممنوع للجميع عدا Admin) |
| `AUDIT.VIEW` | عرض سجل التدقيق |

عند إنشاء صلاحية جديدة، اتبع هذا النمط تمامًا — كل شيء uppercase، نقطة. لا تستخدم underscore أو camelCase.

## التحقق من الصلاحية في الكود

`IAuthService` (في `Services/Interfaces/IAuthService.cs`) يحوي:

```csharp
public interface IAuthService
{
    Task<bool> HasPermissionAsync(int staffId, string permissionCode);
    Task<bool> HasAnyPermissionAsync(int staffId, params string[] permissionCodes);
    // ... و طرق login/logout
}
```

كل منها يستعلم `StaffPermission` مع التحقق من `IsAdmin` كتجاوز:

```csharp
public async Task<bool> HasPermissionAsync(int staffId, string permissionCode)
{
    var staff = await _db.Staff.FindAsync(staffId);
    if (staff?.IsAdmin == true) return true;     // admin يتجاوز
    if (!staff.IsActive) return false;            // معطل بالكامل

    // جلب صلاحية الـ permission code
    var perm = await _db.Permissions
        .FirstOrDefaultAsync(p => p.PermissionCode == permissionCode);
    if (perm == null) return false;

    // هل عنده grant صريح؟
    var sp = await _db.StaffPermissions
        .FirstOrDefaultAsync(s => s.StaffId == staffId && s.PermissionId == perm.PermissionId);
    return sp?.IsGranted ?? false;
}
```

## كيف تحرس قائمة (Menu Item)

في `MainWindow.xaml` أو في `ViewModels/Menu/*ViewModel.cs`:

### نمط 1 — `IsEnabled` فقط

```xml
<MenuItem Header="النتائج" 
          Command="{Binding OpenResultsCommand}"
          IsEnabled="{Binding CanOpenResults}" />
```

في الـ ViewModel:

```csharp
public bool CanOpenResults => _session.CurrentUser?.IsAdmin == true
    || Has("RESULT.ENTER");

private bool Has(string code) => _authService
    .HasPermissionAsync(_session.CurrentUser!.StaffId, code).GetAwaiter().GetResult();
```

### نمط 2 — إخفاء كامل

```xml
<MenuItem Header="النسخ الاحتياطي" 
          Command="{Binding OpenBackupCommand}"
          Visibility="{Binding BackupVisibility}" />
```

في الـ ViewModel:

```csharp
public Visibility BackupVisibility => CanCreateBackup 
    ? Visibility.Visible : Visibility.Collapsed;
```

### نمط 3 — menu item يحوي قائمة فرعية بشروط متعددة

```xml
<MenuItem Header="الفوترة">
    <MenuItem Header="إنشاء فاتورة" 
              Command="{Binding CreateInvoiceCommand}"
              IsEnabled="{Binding CanCreateInvoice}" />
    <MenuItem Header="تحصيل دفعة" 
              Command="{Binding CollectPaymentCommand}"
              IsEnabled="{Binding CanCollectPayment}" />
</MenuItem>
```

## كيف تحرس Action في ViewModel

في الـ RelayCommand callback:

```csharp
private async Task SaveAsync()
{
    var staffId = _session.CurrentUser!.StaffId;
    
    if (!await _authService.HasPermissionAsync(staffId, "EQUIPMENT.MANAGE"))
    {
        MessageBox.Show("ليست لديك صلاحية", "تنبيه");
        return;
    }
    
    // ... المنطق
}

public AsyncRelayCommand SaveAsyncCommand { get; }

// في الـ ctor:
SaveAsyncCommand = new AsyncRelayCommand(SaveAsync, 
    () => !HasErrors 
       && _session.CurrentUser is not null
       && _authService.HasPermissionAsync(_session.CurrentUser.StaffId, "EQUIPMENT.MANAGE").GetAwaiter().GetResult());
```

**القاعدة المهمة**: التحقق يجب أن يحدث في **مكانين**:
1. في `CanExecute` (في الـ constructor الـ RelayCommand) — ليظهر الزر disabled.
2. في الـ handler الفعلي — لأن المستخدم قد يفتح الـ window قبل تغيير الصلاحيات.

## أدوار نموذجية في المشروع

| الموظف | الـ IsAdmin | الـ PermissionCodes الأساسية |
|---|---|---|
| Admin (مدير النظام) | `true` | (لا يحتاج — يتجاوز الكل) |
| استقبال Reception | `false` | `PATIENT.SEARCH`, `VISIT.CREATE`, `PAYMENT.COLLECT`, `BILLING.VIEW` |
| فني Technologist | `false` | `RESULT.ENTER`, `SAMPLE.COLLECT`, `TUBE.PRINT`, `RESULT.VIEW` |
| طبيب Pathologist | `false` | `RESULT.REVIEW`, `RESULT.VALIDATE`, `REPORT.PRINT`, `RESULT.RELEASE` |
| محاسب Accountant | `false` | `BILLING.MANAGE`, `PAYMENT.COLLECT`, `PAYMENT.REFUND`, `REPORT.FINANCIAL` |
| مدير مالي | `false` | (BILLING.MANAGE + REPORT.FINANCIAL + REPORT.COMMISSION) |
| مدير المختبر | `false` | (كل ما سبق + EQUIPMENT.MANAGE + STAFF.MANAGE + SETTINGS.MANAGE) |

عند الإعداد، لا تنشئ دور صريح — بدلاً من ذلك، اجمع الصلاحيات لكل موظف يدويًا عبر StaffPermission rows. هذا يعطي مرونة دقيقة في تخصيص كل موظف.

## إضافة صلاحية جديدة (permission code جديد)

عند إنشاء شاشة جديدة يجب أن تكون محروسة:

### 1. أضف الـ row في جدول Permission

```sql
INSERT INTO Permission (permission_code, permission_name, permission_group, description)
VALUES (
    'EQUIPMENT.MANAGE',
    'إدارة المعدات',
    'EQUIPMENT',
    'إنشاء وتعديل وحذف سجلات المعدات المخبرية'
);
```

أو في Service عند أول تشغيل:
```csharp
public async Task SeedDefaultPermissionsAsync()
{
    var defaults = new[]
    {
        ("EQUIPMENT.MANAGE", "إدارة المعدات", "EQUIPMENT"),
        // ...
    };
    foreach (var (code, name, group) in defaults)
    {
        if (!await _db.Permissions.AnyAsync(p => p.PermissionCode == code))
        {
            _db.Permissions.Add(new Permission {
                PermissionCode = code,
                PermissionName = name,
                PermissionGroup = group
            });
        }
    }
    await _db.SaveChangesAsync();
}
```

### 2. احرس الواجهة

في الشاشة الرئيسية menu: استخدم نمط `IsEnabled` أو `Visibility` كما سبق.

### 3. احرس الـ Action

في كل ViewModel يحتوي على `AsyncRelayCommand` يحمي أمرًا متعلقًا، أضف التحقق في `CanExecute` والـ handler.

## سيناريو: موظف انتقل بين الأقسام

نقل موظف من الاستقبال للفني لا يحتاج "حذف كل الصلاحيات وزيادتها"، بل:

```sql
-- احذف صلاحيات القسم القديم
DELETE FROM StaffPermission
WHERE staff_id = @sid
  AND PermissionId IN (SELECT PermissionId FROM Permission WHERE PermissionGroup = 'BILLING');

-- أضف صلاحيات القسم الجديد
INSERT INTO StaffPermission (staff_id, permission_id, is_granted, granted_by, granted_at)
SELECT @sid, PermissionId, 1, @adminId, SYSUTCDATETIME()
FROM Permission
WHERE PermissionGroup = 'SAMPLE';
```

هذا يتركك مرنًا لترك بعض الصلاحيات المشتركة (مثل `PATIENT.SEARCH`) دون حذفها.

## ربط ViewModelBase بسياق المستخدم

`FinalLabSystem.Infrastructure.Session.CurrentUserSession` يحمل المستخدم الحالي:

```csharp
public class CurrentUserSession : ICurrentUserSession
{
    public Staff? CurrentUser { get; set; }
    public void Set(Staff staff) => CurrentUser = staff;
    public void Clear() => CurrentUser = null;
}
```

في ViewModels:

```csharp
public class EquipmentWindowViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUserSession _session;

    private int CurrentStaffId => _session.CurrentUser!.StaffId;
    private bool HasPermission(string code) =>
        _auth.HasPermissionAsync(CurrentStaffId, code).GetAwaiter().GetResult();

    // Commands
}
```

## أخطاء شائعة

| الخطأ | لماذا فشل | الصواب |
|---|---|---|
| التحقق فقط في `CanExecute` | مستخدم يفتح window ثم ترفع الصلاحية، يفعل العملية بدون تحقق | تحقق مزدوج |
| التحقق فقط في الـ handler | الزر يبدو متاح لكنه لا يعمل | تحقق في `CanExecute` |
| استخدام `IsAdmin` للتحقق من عملية حساسة | قد يحدث شخص غير Admin. لكن لـ admin فقط اتفق، فلا مشكلة. | استخدم `_auth.HasPermissionAsync(sid, "ADMIN.ONLY")` |
| نسيان `IsActive` | الموظف المعطل (إجازة/فصل) لا يزال يرى القائمة | تحقق `staff.IsActive` في AuthService |
| استعلام Permission لكل menu render | بطء | خزّن الصلاحيات في `CurrentUserSession` بعد login |

## Checklist لكل شاشة جديدة

```
□ PermissionCode محجوز وموجود في جدول Permission
□ Menu item محروس بـ IsEnabled أو Visibility (حسب الـ UX)
□ كل Command في الـ ViewModel يفحص صلاحية في CanExecute
□ كل Command handler يفحص صلاحية مرة ثانية قبل التنفيذ
□ Service methods (_audit, _db) لا يحتاجون فحص — التكليف على الـ VM
□ إذا كانت شاشة لصلاحيات نادرة (مثل BACKUP.RESTORE)، ضعها في قائمة "الإدارة" المخفية عن الاستقبال العادي
□ إذا كانت الشاشة الجديدة تستبدل موجودة، تأكد من حذف PermissionCode القديمة، أو اتركها لدعم legacy
```

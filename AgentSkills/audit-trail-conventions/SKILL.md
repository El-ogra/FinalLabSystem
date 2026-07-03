---
name: audit-trail-conventions
description: نمط التدقيق التلقائي المطبَّق فعلياً في FinalLabSystem عبر override على SaveChangesAsync وكيان [Auditable] وجدول AuditLog. يشرح بالتفصيل كيف يُسجَّل كل تغيير، وكيف تُوسِّع نمط التدقيق لأي كيان جديد، وكيف تقرأ AuditLog، ومتى لا تلتقط الـ override، وكيف يتعامل الـ audit مع نفسه (re-entrancy guard). عنصر إلزامي لكل كيان يحوي بيانات طبية أو مالية.
---

# Audit Trail Conventions — FinalLabSystem

## القاعدة الجوهرية

التدقيق في هذا المشروع **تلقائي بالكامل**. لا يستدعي المبرمج شيئًا، ولا يُسجَّل في طبقة Service. المنطق في `Data/FinalLabDbContext.cs` — السطور الـ (1-90). كل كيان عليه `[Auditable]` يُسجَّل كل تغيير في حقل من حقوله عبر جدول `AuditLog`.

## الكود الفعلي المعني

`/workspace/lab-system/FinalLabSystem/Data/FinalLabDbContext.cs`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    if (_auditingFlag.Value)
        return await base.SaveChangesAsync(cancellationToken);

    _auditingFlag.Value = true;
    try
    {
        var snapshot = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                && e.Entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), false).Length > 0)
            .Select(e => new
            {
                State = e.State,
                TableName = e.Metadata.GetTableName() ?? e.Metadata.Name,
                RecordId = e.Metadata.FindPrimaryKey()?.Properties
                    .Select(p => Convert.ToInt32(e.Property(p.Name).CurrentValue))
                    .FirstOrDefault() ?? 0,
                Properties = e.Properties.Select(p => new
                {
                    p.Metadata.Name,
                    p.IsModified,
                    OriginalValue = p.OriginalValue,
                    CurrentValue = p.CurrentValue
                }).ToList()
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (snapshot.Count == 0 || _session is null)
            return result;

        var staffId = _session.CurrentUser?.StaffId;
        var now = DateTime.UtcNow;

        foreach (var item in snapshot)
        {
            foreach (var prop in item.Properties)
            {
                if (!prop.IsModified && item.State == EntityState.Modified)
                    continue;

                AuditLogs.Add(new AuditLog
                {
                    TableName = item.TableName,
                    RecordId = item.RecordId,
                    Action = item.State switch
                    {
                        EntityState.Added => "A",    // Added
                        EntityState.Modified => "M", // Modified
                        EntityState.Deleted => "D",  // Deleted
                        _ => "U"
                    },
                    FieldName = prop.Name,
                    OldValue = item.State == EntityState.Added ? null : prop.OriginalValue?.ToString(),
                    NewValue = prop.CurrentValue?.ToString(),
                    ChangedBy = staffId,
                    ChangedAt = now
                });
            }
        }

        await base.SaveChangesAsync(cancellationToken);
        return result;
    }
    finally
    {
        _auditingFlag.Value = false;
    }
}
```

## الكيان `AuditLog`

من `Models/AuditLog.cs`:

| العمود | النوع | المعنى |
|---|---|---|
| `AuditId` | `long` (مفتاح أساسي) | identity |
| `TableName` | `string(100)` | اسم الجدول كما في DB (يأتي من `GetTableName()`) |
| `RecordId` | `int` | معرّف السطر الأصلي |
| `Action` | `char(1)` | `"A"` = Added, `"M"` = Modified, `"D"` = Deleted |
| `FieldName` | `string(100)` | اسم العمود الذي تغيّر (snapshot لكل عمود) |
| `OldValue` | `string?` | `.ToString()` للقيمة قبل — أو `null` في الإضافة |
| `NewValue` | `string?` | `.ToString()` للقيمة بعد |
| `ChangedBy` | `int?` | `staff_id` من الـ session الحالي |
| `ChangedAt` | `DateTime` | UTC time عند الـ Save |
| `SessionInfo` | `string?(200)` | غير مستخدم في override — اتركه `null` إلا يدويًا |
| `Notes` | `string?(500)` | نفس، إلا إذا ملأته Service يدويًا |

## الـ Attribute

`Data/AuditableAttribute.cs` — بسيط، فقط marker:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class AuditableAttribute : Attribute
{
}
```

## Re-entrancy Guard

الـ `_auditingFlag` من نوع `AsyncLocal<bool>` يحمي من التسجيل داخل تسجيل:

```csharp
_auditingFlag.Value = true;
try
{
    // 1. snapshot
    var result = await base.SaveChangesAsync(cancellationToken);  // ← يحفظ الـ snapshot فعلاً
    // 2. add AuditLog rows
    AuditLogs.Add(...);
    // 3. save again, but _auditingFlag = true, so reentrancy skipped
    await base.SaveChangesAsync(cancellationToken);
    return result;
}
finally
{
    _auditingFlag.Value = false;
}
```

هذا يعني أن `SaveChangesAsync` يُستدعى **مرتين** لكتابة تدقيق: مرة لـ entities الأساسية، ومرة لـ AuditLog rows. إذا أي كود في طبقة Service يستدعي `SaveChangesAsync` من داخل override (مثل handler)، فإنه يمر عبر الحارس ولا ينتج audit entry لانهائي.

## متى يُضاف `[Auditable]`

أضف `[Auditable]` على أي كيان يطابق:

- يحوي **بيانات طبية** (Patient, Visit, TestResult, NormalRange, SampleTube…).
- يحوي **بيانات مالية** (Payment, ContractInvoice, VisitCharge…).
- يحوي **بيانات تحكم** (Staff, Permission, StaffPermission, PriceScheme…).
- يحوي **سجلات تتبع** (AuditLog نفسه ليس Auditable — الـ audit logged بواسطة الـ override بلا self-tracking).

**لا تضفه** على:
- كيانات الـ join البحتة (مثل `TestTypeSampleTube` إن وجدت).
- الـ views (`VOutstandingBalance` و `VResultAuditTrail` إلخ) — الـ views للقراءة فقط.
- `AuditLog` نفسه (سيُحدث لا نهائيًا).

## كيفية إضافة كيان جديد مع التدقيق

### 1. زُد الكيان

```csharp
// Models/Equipment.cs
using FinalLabSystem.Data;

namespace FinalLabSystem.Models;

[Auditable]
public partial class Equipment
{
    public int EquipmentId { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    // ... بقية الحقول
}
```

### 2. أضف migration

(راجع skill `ef-core-migration-safety`). الـ migration المُولَّد لا يحتاج شيء خاص — الـ attribute على الـ class فقط.

### 3. سجِّل في DbContext

في `Data/FinalLabDbContext.cs` داخل الكلاس:

```csharp
public virtual DbSet<Equipment> Equipments { get; set; }
```

وفي `OnModelCreating` أضف الكنفيغ (EF يولّده تلقائيًا إذا استخدم `Scaffold`، أو انسخ من كيان موجود مشابه كـ template).

### 4. لا حاجة لتسجيل يدوي للـ Audit

الـ override يمسكها. عند أي `SaveChangesAsync` (حتى غير صريح — cascade من service يضيف `Equipment` ثم `SaveChangesAsync`)، الـ audit ينطلق.

## كيف تقرأ `AuditLog`

الـ View المعرَّف `VResultAuditTrail` يقرأ نتائج المرضى. لأن `AuditLog` يحوي عمود لكل تغيير حقل، يمكنك عمل:

```sql
-- تاريخ التعديلات على نتيجة معينة
SELECT changed_at, action, field_name, old_value, new_value, changed_by
FROM AuditLog
WHERE table_name = 'TestResult' AND record_id = @resultId
ORDER BY changed_at DESC;
```

```sql-- من عدّل حقل حساس في آخر 24 ساعة
SELECT *
FROM AuditLog
WHERE changed_at >= DATEADD(day, -1, SYSUTCDATETIME())
  AND field_name IN ('ResultValueNumeric', 'ValidationStatus')
ORDER BY changed_at DESC;
```

الـ `StaffId` من `ChangedBy`. الـ UI في المشروع يحوي `AuditTrailWindow` (`Views/Settings/AuditTrailWindow.xaml`) المرتبطة بـ `AuditTrailViewModel` — تصفّح هذه الموجودة قبل إضافة شاشة جديدة للـ audit.

## ما لا يلتقطه الـ override

لكي لا تستيقظ لاحقًا وتقول "ليش ما في audit row"، افهم الحدود:

| الحالة | النتيجة |
|---|---|
| `ExecuteSqlRaw("UPDATE Equipment SET NameAr = @v WHERE EquipmentId = @id")` | **لا audit row** — raw SQL يتجاوز الـ ChangeTracker. |
| Stored procedure يعدّل مباشرة | **لا audit row**. |
| `BulkInsert` / `BulkUpdate` بدون تتبع | **لا audit row**. الـ EF Core 7+ يدعم `ExecuteUpdate`/`ExecuteDelete` بدون تتبع — لا تستخدمهما على Auditable دون قراءة يدوية. |
| حذف cascade من DatabaseEngine | **لا audit row** للكيان المحذوف (ليس تمر عبر DbContext). |
| تعديل من service لا يستدعي `SaveChangesAsync` (مثل استدعاء DB مباشرة) | **لا audit row**. |

إذا احتاج تدقيق على raw SQL — استخدم `Database.ExecuteSqlRaw` بعد تسجيل الـ change في `ChangeTracker` يدويًا، أو املأ `AuditLog` يدويًا مع `Notes = "via raw SQL"`.

## تكامل الـ AuditLog مع `AuditService` على مستوى التطبيق

الكود يحوي `IAuditService` و `AuditService` (في `Services/Implementations/AuditService.cs`). هذا **للاستخدام اليدوي** — للأحداث التي ليست CRUD (مثل فتح نافذة، أو تسجيل دخول). لا تستبدل به الـ override — إنما استخدمه للإضافة.

مثال من طبقة Service:

```csharp
public sealed class PatientService : IPatientService
{
    private readonly IAuditService _audit;

    public async Task DeactivateAsync(int patientId, int staffId)
    {
        // ... يحدث Patient.Status = "Inactive"
        await _audit.LogActionAsync(
            tableName: "Patient",
            recordId: patientId,
            action: "DEACTIVATE",
            staffId: staffId);
    }
}
```

`IAuditService.LogActionAsync` يكتب إلى `AuditLog` مباشرة مع `FieldName = "status_action"`، `Notes = "manual action"`.

## تخصيص السلوك (إذا لزم)

النظام الحالي:
- يحفظ كل عمود في صف مستقل. على كيان بحقل واحد فقط تغير، هذا = صف واحد. لكن كيان بحقول كثيرة (مثل Visit مع أكثر من 30 حقل) تعدلها = 30 صف audit. هذا مقصود — أمن > تخزين.

إذا بدك معالجة مختلفة:
- **إضافة helper method على service** يكتب event مركّب بدلاً من per-field rows.
- **إضافة trigger على DB** يحفظ نسخة قديمة في جدول archive منفصل، ثم الـ override يكتب الـ diff فقط.

لكن لا تجرّب تغيير الـ override دون اختبار شامل — `SaveChangesAsync` مرتين، re-entrancy، AsyncLocal — كله حساس.

## Checklist لكل كيان جديد يحوي بيانات طبية/مالية

```
□ [Auditable] على الـ class
□ DbSet<...> في DbContext
□ الكنفيغ (column mappings) في OnModelCreating
□ Migration (skill ef-core-migration-safety)
□ إذا كانت خدمة/VM تلمس الكيان، _audit غير ضروري إلا إذا actions غير-CRUD
□ تحقق على dev: أنشئ سجل، عدّل حقل، تحقق صف في AuditLog لكل تغيير
□ إذا raw SQL يلمس الكيان → أضف logging يدوي أو غيّر الكود ليستخدم EF
```

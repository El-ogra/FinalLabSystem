---
name: stage-gating-workflow-conventions
description: "مهارة متخصصة في stage-gating-workflow-conventions لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# Stage-Gating Workflow Conventions — FinalLabSystem

## ما هو Stage-Gating

Stage-Gating في هذا المشروع يعني: **كل VisitTest/TestResult يمر بسلسلة مراحل معتمدة** (Pending → InProgress → ResultEntered → Validated → Released). الانتقال من مرحلة للتالية يحتاج:
- **شروط أعمال** (مثلاً: لا يُحرر قبل Validation).
- **صلاحيات الموظف** (مثلاً: لا تعتمد إلا Pathologist).
- **حالة البيانات** (مثلاً: كل المكونات مدخلة).

هذا نمط صارم مطبق فعلياً عبر `Infrastructure/ResultStageRules.cs`.

## الكود الفعلي المعني

`/workspace/lab-system/FinalLabSystem/Infrastructure/ResultStageRules.cs`:

```csharp
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Infrastructure;

public static class ResultStageRules
{
    public static bool CanPrint(VisitTest vt)
    {
        if (vt.TestResults == null || vt.TestResults.Count == 0)
            return false;
        return vt.TestResults.All(tr => tr.ValidationStatus >= ResultValidationStatus.Reviewed);
    }

    public static bool CanExport(VisitTest vt)
    {
        return vt.IsPrinted;
    }

    public static bool CanDeliver(VisitTest vt)
    {
        return vt.IsPrinted;
    }

    public static ResultValidationStatus GetMinimumValidationStatus(VisitTest vt)
    {
        if (vt.TestResults == null || vt.TestResults.Count == 0)
            return ResultValidationStatus.Entered;
        return vt.TestResults.Min(tr => tr.ValidationStatus);
    }
}
```

## الأنواع الـ enum الأساسية

### `TestStage` (مرحلة الـ VisitTest):

```csharp
public enum TestStage
{
    Pending,        // 0 - أُنشئ الطلب، لم يُبدأ العمل
    InProgress,     // 1 - جارٍ العمل (بدأ الفني)
    ResultEntered,  // 2 - كل المكونات دُخلت، لم تُراجع
    Validated,      // 3 - راجع واعتمد، جاهز للإصدار
    Released,       // 4 - صدر/طُبع
    SentOut,        // 5 - أُرسل لمختبر خارجي
    Cancelled       // 6 - ملغي
}
```

### `ResultValidationStatus` (مرحلة الـ TestResult):

```csharp
public enum ResultValidationStatus
{
    Entered = 0,    // فني أدخل النتيجة
    Reviewed = 1,   // كبير الفنيين راجع
    Validated = 2,  // طبيب/اعتمد
    Released = 3    // صدر للتقرير
}
```

التمييز: `TestStage` على مستوى الـ VisitTest (إجمالي)؛ `ResultValidationStatus` على مستوى كل TestResult (مكوّن).

## مخطّط التدفق (Flow Chart)

```
[Visit Created]
     ↓
   Pending ←─────────────────┐
     ↓ (يُبدأ العمل)        │
  InProgress                 │
     ↓ (فني يُدخل القيم)    │
  ResultEntered ←──── (إعادة إدخال/تصحيح) ── Reviewer يعدّل → Reviewed
     ↓                        ↑
  All Reviewed? ──No──────────┘
     ↓ Yes
  Validated (Pathologist)
     ↓
  Released (Print/Export)
     ↓
  Delivered (Receive confirmation)

[في أي وقت] → Cancelled (مع السبب)
```

## القواعد الـ `BR` المرافقة

| القاعدة | منطقها |
|---|---|
| `ResultStageRules.CanPrint(vt)` | كل TestResults.All(tr => tr.ValidationStatus >= Reviewed) |
| `ResultStageRules.CanExport(vt)` | فقط بعد `IsPrinted = true` |
| `ResultStageRules.CanDeliver(vt)` | فقط بعد `IsPrinted = true` |
| `BR-007` (في الـ glossary) | Released يحتاج Validation ≥ Validated |

## القاعدة الرئيسية: بوابة في كل UI Action

كل زر في الـ UI يفعل عملية يجب أن يفحص القواعد قبل:

```csharp
public sealed class ResultEntryViewModel : ViewModelBase
{
    private VisitTest? _selectedTest;
    private readonly IResultStageRules _stageRules;  // قد يكون static، راجع الأسفل

    public AsyncRelayCommand SaveResultCommand { get; }
    public AsyncRelayCommand ReviewCommand { get; }
    public AsyncRelayCommand ValidateCommand { get; }
    public AsyncRelayCommand ReleaseCommand { get; }
    public AsyncRelayCommand PrintCommand { get; }

    public ResultEntryViewModel(...)
    {
        SaveResultCommand = new AsyncRelayCommand(SaveResultAsync,
            () => _selectedTest?.Stage == TestStage.Pending
               || _selectedTest?.Stage == TestStage.InProgress
                  && !HasErrors);
        
        ReviewCommand = new AsyncRelayCommand(ReviewAsync,
            () => _selectedTest?.Stage == TestStage.ResultEntered
               && HasPermission("RESULT.REVIEW"));
        
        ValidateCommand = new AsyncRelayCommand(ValidateAsync,
            () => _selectedTest?.Stage == TestStage.ResultEntered  // مشروط
               && AllResultsReviewed(_selectedTest)
               && HasPermission("RESULT.VALIDATE"));
        
        ReleaseCommand = new AsyncRelayCommand(ReleaseAsync,
            () => _selectedTest?.Stage == TestStage.Validated
               && HasPermission("RESULT.RELEASE"));
        
        PrintCommand = new AsyncRelayCommand(PrintAsync,
            () => _selectedTest?.Stage >= TestStage.Validated
               && ResultStageRules.CanPrint(_selectedTest));
    }
}
```

## كيف يُحدَّث الـ Stage عند كل action

داخل الـ Service methods:

```csharp
public async Task SaveResultAsync(int visitTestId, decimal value, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    if (vt.Stage != TestStage.Pending && vt.Stage != TestStage.InProgress)
        throw new InvalidStageException(
            $"Cannot edit in stage {vt.Stage}. Expected Pending or InProgress.");

    var tr = vt.TestResults.First();
    tr.ResultValueNumeric = value;
    tr.EnteredBy = staffId;
    tr.EnteredAt = DateTime.UtcNow;
    tr.ValidationStatus = ResultValidationStatus.Entered;

    if (vt.Stage == TestStage.Pending)
        vt.Stage = TestStage.InProgress;
    
    // هل كل المكونات دُخلت؟
    if (vt.TestResults.All(r => r.ResultValueNumeric.HasValue || !string.IsNullOrEmpty(r.ResultValueText)))
        vt.Stage = TestStage.ResultEntered;

    await _db.SaveChangesAsync();
}
```

```csharp
public async Task ReviewAsync(int visitTestId, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    if (vt.Stage != TestStage.ResultEntered)
        throw new InvalidStageException($"Cannot review from stage {vt.Stage}.");

    foreach (var tr in vt.TestResults)
    {
        if (tr.ValidationStatus < ResultValidationStatus.Entered)
            throw new InvalidDataException($"Component {tr.ComponentId} not entered yet.");
        tr.ValidationStatus = ResultValidationStatus.Reviewed;
    }

    await _db.SaveChangesAsync();
}
```

```csharp
public async Task ValidateAsync(int visitTestId, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    var minStatus = ResultStageRules.GetMinimumValidationStatus(vt);
    if (minStatus < ResultValidationStatus.Reviewed)
        throw new InvalidStageException($"Not all reviewed. Current min: {minStatus}.");

    foreach (var tr in vt.TestResults)
    {
        tr.ValidationStatus = ResultValidationStatus.Validated;
        tr.ValidatedBy = staffId;
        tr.ValidatedAt = DateTime.UtcNow;
    }
    vt.Stage = TestStage.Validated;
    await _db.SaveChangesAsync();
}
```

```csharp
public async Task ReleaseAsync(int visitTestId, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    if (vt.Stage != TestStage.Validated)
        throw new InvalidStageException($"Cannot release from stage {vt.Stage}.");

    foreach (var tr in vt.TestResults)
    {
        tr.ValidationStatus = ResultValidationStatus.Released;
        tr.ReleasedBy = staffId;
        tr.ReleasedAt = DateTime.UtcNow;
    }
    vt.Stage = TestStage.Released;
    await _db.SaveChangesAsync();
}
```

## مُساعد `IsAllowedTransition`

إن أردت إنشاء هيكل أنظف، اجعل الـ rules في helper:

```csharp
public static class StageTransitions
{
    private static readonly Dictionary<(TestStage From, TestStage To), bool> Allowed = new()
    {
        { (TestStage.Pending,       TestStage.InProgress),    true },
        { (TestStage.InProgress,    TestStage.ResultEntered), true },
        { (TestStage.ResultEntered, TestStage.Validated),     true },
        { (TestStage.Validated,     TestStage.Released),      true },
        { (TestStage.ResultEntered, TestStage.InProgress),    true },  // للعودة للتصحيح
        { (_,                       TestStage.Cancelled),     true },  // أي مرحلة → Cancelled
    };

    public static bool IsAllowed(TestStage from, TestStage to) =>
        Allowed.TryGetValue((from, to), out var allowed) && allowed;
}
```

في الـ Service:

```csharp
if (!StageTransitions.IsAllowed(vt.Stage, TestStage.Validated))
    throw new InvalidStageException(
        $"Transition not allowed: {vt.Stage} → {TestStage.Validated}");

vt.Stage = TestStage.Validated;
await _db.SaveChangesAsync();
```

## طبقة الـ UI تتفاعل مع الـ Stage

`XAML`:

```xml
<Button Content="حفظ النتيجة" 
        Command="{Binding SaveResultCommand}" />
<Button Content="مراجعة" 
        Command="{Binding ReviewCommand}" />
<Button Content="اعتماد" 
        Command="{Binding ValidateCommand}" />
<Button Content="إصدار" 
        Command="{Binding ReleaseCommand}" />
```

لكن الأزرار ستظهر **معطلة** حسب الـ CanExecute. أو يمكنك استخدام `Visibility`:

```xml
<Button Content="اعتماد" 
        Command="{Binding ValidateCommand}"
        Visibility="{Binding ValidateVisibility}" />
```

```csharp
public Visibility ValidateVisibility =>
    _selectedTest?.Stage == TestStage.ResultEntered
        && AllResultsReviewed(_selectedTest)
            ? Visibility.Visible
            : Visibility.Collapsed;
```

## المسارات الخاصة

### 1. الاختبارات الخارجية (SentOut)

```csharp
public async Task SendOutAsync(int visitTestId, int externalLabId, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    if (vt.Stage != TestStage.Pending && vt.Stage != TestStage.InProgress)
        throw new InvalidStageException($"Cannot send out from {vt.Stage}.");

    var shipment = new ExternalShipment
    {
        ExternalLabId = externalLabId,
        CreatedBy = staffId,
        CreatedAt = DateTime.UtcNow,
        Status = ExternalShipmentStatus.Pending
    };
    _db.ExternalShipments.Add(shipment);
    
    foreach (var tr in vt.TestResults)
    {
        _db.ExternalShipmentItems.Add(new ExternalShipmentItem
        {
            ExternalShipmentId = shipment.ExternalShipmentId,
            TestResultId = tr.ResultId
        });
    }

    vt.Stage = TestStage.SentOut;
    await _db.SaveChangesAsync();
}

public async Task MarkExternalReturnedAsync(int shipmentId, int staffId)
{
    var items = await _db.ExternalShipmentItems
        .Where(i => i.ExternalShipmentId == shipmentId)
        .Include(i => i.TestResult)
        .ThenInclude(tr => tr.VisitTest)
        .ToListAsync();

    foreach (var item in items)
    {
        if (item.TestResult.ResultValueNumeric.HasValue 
            || !string.IsNullOrEmpty(item.TestResult.ResultValueText))
            item.TestResult.ValidationStatus = ResultValidationStatus.Entered;
    }

    await _db.SaveChangesAsync();
}
```

### 2. الإلغاء (`Cancelled`)

```csharp
public async Task CancelAsync(int visitTestId, string reason, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    if (vt.Stage == TestStage.Released)
        throw new InvalidStageException("Cannot cancel a released test.");

    if (vt.IsPrinted)
        throw new InvalidStageException("Cannot cancel a printed test.");

    vt.Stage = TestStage.Cancelled;
    vt.Notes = (vt.Notes ?? "") + $"\n[CANCELLED by staff {staffId} at {DateTime.UtcNow:o}]: {reason}";
    
    await _db.SaveChangesAsync();
}
```

### 3. التصحيح بعد الاعتماد (Regression)

Pathologist أحيانًا يكتشف خطأ في نتيجة اعتُمدت. الإجراء:

```csharp
public async Task RevalidateAsync(int visitTestId, string reason, int staffId)
{
    var vt = await _db.VisitTests
        .Include(v => v.TestResults)
        .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);

    if (vt.Stage != TestStage.Validated)
        throw new InvalidStageException($"Revalidate only from Validated, not {vt.Stage}.");

    foreach (var tr in vt.TestResults)
    {
        tr.ValidationStatus = ResultValidationStatus.Entered;  // يرجع للبداية
        tr.Notes = (tr.Notes ?? "") + $"\n[REVALIDATED by staff {staffId}]: {reason}";
    }
    vt.Stage = TestStage.ResultEntered;
    await _db.SaveChangesAsync();
}
```

هذا لا يحذف التاريخ - الـ `ValidatedAt` يبقى مسجلاً. الـ الجديد مسجل بـ `EnteredAt`.

## كيفية إضافة مرحلة جديدة

افترض أنك تريد إضافة مرحلة `QualityControlled` بين Validated و Released:

1. **عدّل الـ enum:**
   ```csharp
   public enum TestStage
   {
       Pending, InProgress, ResultEntered, Validated, 
       QualityControlled,  // ← جديد
       Released, SentOut, Cancelled
   }
   ```
2. **أضف Transition:**
   ```csharp
   { (TestStage.Validated, TestStage.QualityControlled), true },
   { (TestStage.QualityControlled, TestStage.Released), true },
   ```
3. **عدّل `CanPrint`** إذا QualityControlled شرط:
   ```csharp
   public static bool CanPrint(VisitTest vt)
   {
       return vt.Stage >= TestStage.QualityControlled;
   }
   ```
4. **عدّل `ResultStageRules`** و `IsAllowedTransition`.

## تذكير مهم

`ResultStageRules` methods accept `VisitTest`. يجب أن تأخذ `VisitTest` مُحمَّلاً بـ `Include(v => v.TestResults)` وإلا `vt.TestResults == null`. هذا سبب استدعاء الـ NullReferenceException في الفشل المُتكرر.

```csharp
var vt = await _db.VisitTests
    .Include(v => v.TestResults)        // ← ضروري
    .FirstOrDefaultAsync(v => v.VisitTestId == visitTestId);
```

## Checklist لكل flow جديد في الـ stage-gating

```
□ enum Stage الجديد/المعدّل مع migrations في db (skill ef-core-migration-safety)
□ helper على Service لكل transition
□ helper يحقق IsAllowed (لا تكرّر الشروط في كل handler)
□ permission code (skill rbac-permission-conventions) لكل action حساس
□ CanExecute في RelayCommand يحقق Stage + Permission + data state
□ button visibility/Enabled state في XAML حسب stage
□ لا transition صامت — كل تغيير stage يُسجَّل في AuditLog
□ exceptions واضحة (InvalidStageException) — لا generic Exception
□ tests تُغطّي: happy path + كل invalid transition
```

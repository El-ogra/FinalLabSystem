# خطة إصلاح الأعطال الخمسة (Handoff Plan)

| البند | القيمة |
|---|---|
| **التاريخ** | 2026-07-16 |
| **المرجع** | `Docs/PRDs/Verification_Critical_Defects.md` |
| **طبيعة المهمة** | تخطيط فقط — لا تنفيذ |

---

## العطل رقم 1: تعارض قيد الخصم يُفشل أي حفظ فيه خصم

### 1. الإصلاح المقترح

**الخيار الأنظف: إسقاط القيد بالكامل.**

**التحقق من دعم الاتجاهين في الكود الحالي:**
يُدعم `FinancialViewModel` الاشتقاق في كلا الاتجاهين بالفعل:

- **اتجاه مبلغ → نسبة** (سطر 76):
  ```csharp
  DiscountPercent = Subtotal <= 0 ? 0 : Math.Round(DiscountAmount / Subtotal * 100, 2);
  ```
- **اتجاه نسبة → مبلغ** (سطر 92):
  ```csharp
  DiscountAmount = Math.Round(Subtotal * DiscountPercent / 100, 2);
  ```

كلا الاتجاهين يعملان بشكل صحيح بفضل حقل `_isUpdating` (سطر 75 و 91) الذي يمنع الحساب الدائري المتضارب عند تحديث أحد الحقلين. لا حاجة لإضافة `DiscountEntryMode` أو أي آلية تتبّع إضافية — الكود الحالي يتعامل مع كلا الاتجاهين بشكل صحيح بالفعل.

**المشكلة الوحيدة:** القيد `CK_Visit_DiscountExclusivity` يمنع أن يكون كلا الحقلين موجباً في آن واحد، لكن الكود يُشتق كلاهما تلقائياً في كل مرة.

**ملف واحد يتغير:** `FinalLabDbContext.cs`

**قبل (سطر 1869-1872):**
```csharp
entity.ToTable("Visit", tb =>
    {
        tb.HasCheckConstraint("CK_Visit_DiscountExclusivity",
            "NOT (discount_amount > 0 AND discount_percent > 0)");
    });
```

**بعد:**
```csharp
entity.ToTable("Visit");
```

(إزالة `HasCheckConstraint` بالكامل)

### 2. البدائل

| البدائل | إيجابيات | سلبيات | التوصية |
|---|---|---|---|
| **A) إسقاط القيد (المُقترح)** | أبسط تعديل، لا يكسر أي كود موجود، يعمل مع التصميم الحالي الذي يدعم كلا الاتجاهين | يفقد قيداً قد يكون مهماً لو تغيّر التصميم مستقبلاً | **الأنسب** — لأن الكود يدعم كلا الاتجاهين بالفعل والقيد مستحيل الامتثال |
| B) إسقاط `DiscountPercent` من الكود وجعله readonly computed column | يحافظ على القيد ويعيد الحساب من قاعدة البيانات فقط | يُلغي دعم اتجاه نسبة → مبلغ في الواجهة، يحتاج migration لإعادة بناء الجدول | يُفقد وظيفة مطلوبة |
| C) تغيير القيد ليتحقق من أن `discount_percent = discount_amount / subtotal * 100` | يحافظ على فكرة التحقق | لا يمكن التعبير بهذه المعادلة في Check Constraint | غير ممكن |

### 3. Migration المطلوب

```csharp
migrationBuilder.DropCheckConstraint(
    name: "CK_Visit_DiscountExclusivity",
    table: "Visit");
```

### 4. أثر الإصلاح على بيانات موجودة

**لا يوجد خطر.** القييد المُسقط كان يمنع الإدخال فقط (يمنع `DiscountAmount > 0 AND DiscountPercent > 0`). البيانات الموجودة مسبقاً بالتنسيق القديم (كلاهما موجب) ستبقى سليمة ولن تتأثر.

### 5. خطوة التحقق بعد التنفيذ

يجب اختبار كلا الاتجاهين صراحةً:

1. **اختبار اتجاه مبلغ → نسبة:**
   - أنشئ زيارة جديدة مع `subtotal = 100`
   - أدخل `discountAmount = 10`
   - تأكد من أن `discountPercent = 10` (مشتق تلقائياً)
   - استدعِ `SaveChangesAsync` — يجب أن ينجح بدون خطأ

2. **اختبار اتجاه نسبة → مبلغ:**
   - أنشئ زيارة جديدة مع `subtotal = 200`
   - أدخل `discountPercent = 15`
   - تأكد من أن `discountAmount = 30` (مشتق تلقائياً)
   - استدعِ `SaveChangesAsync` — يجب أن ينجح بدون خطأ

3. **اختبار في قاعدة البيانات:**
   - تحقق من كل زيارة أن كلا الحقلين موجب في نفس الوقت (الحالة الطبيعية بعد الإصلاح)

### 6. تقدير حجم التغيير

**صغير جداً** — حذف سطر واحد + سطران من `FinalLabDbContext.cs` + migration بسطرين.

---

## العطل رقم 2: تصادم الترتيب اليومي للباركود

### 1. الإصلاح المقترح

تعديل `GetDailyOrdinalAsync` لعدّ الترتيب لكل اليوم (ولكل مريض على حدة).

**ملف واحد يتغير:** `FinalLabSystem/Services/Implementations/BarcodeGenerator.cs`

**قبل (سطرا 101-111):**
```csharp
private async Task<int> GetDailyOrdinalAsync(int patientId, DateTime date)
{
    var dayStart = date.Date;
    var dayEnd = dayStart.AddDays(1);
    return await _context.PatientBarcodes
        .Where(pb => pb.PatientId == patientId       // <-- المشكلة
                  && pb.IssueDate >= dayStart
                  && pb.IssueDate < dayEnd
                  && pb.CodeType == BarcodeCodeType.Case)
        .CountAsync() + 1;
}
```

**بعد:**
```csharp
private async Task<int> GetDailyOrdinalAsync(int patientId, DateTime date)
{
    var dayStart = date.Date;
    var dayEnd = dayStart.AddDays(1);
    return await _context.PatientBarcodes
        .Where(pb => pb.IssueDate >= dayStart
                  && pb.IssueDate < dayEnd
                  && pb.CodeType == BarcodeCodeType.Case)
        .CountAsync() + 1;
}
```

(حذف `pb.PatientId == patientId &&` فقط)

### 2. البدائل

| البدائل | إيجابيات | سلبيات | التوصية |
|---|---|---|---|
| **A) عدّاد يومي عام (المُقترح)** | أبسط تعديل، يضمن فريدية الباركود، لا يغير هيكل الباركود | الترتيب يزيد لكل مريض وليس لكل مريض على حدة | **الأنسب** — لأن الباركود يُستخدم كمعرف عام |
| B) إضافة PatientId للباركود | يجعل الباركود فريداً بالتصميم | يُغيّر هيكل الباركود، يحتاج تعديل لكل مكان يقرأه | مفرط التعقيد |
| C) استخدام Composite Unique Index (PatientId + CodeType + Date + Ordinal) | يسمح بنفس الترتيب لكل مريض | لا يمنع تطابق BarcodeValue الفعلي | غير كافٍ |

### 3. Migration المطلوب

**لا يوجد.** التعديل على كود فقط، لا يتغير هيكل قاعدة البيانات.

### 4. أثر الإصلاح على بيانات موجودة

**لا يوجد خطر.** التغيير على المنطق فقط. الباركودات الموجودة لن تتأثر — الأرقام اللاحقة للزيارات الجديدة فقط ستتغير.

### 5. خطوة التحقق بعد التنفيذ

1. سجّل مريض "أ" اليوم واطلب باركود Case — يجب أن يحصل على ordinal = (عدد الباركودات السابقة + 1)
2. سجّل مريض "ب" اليوم واطلب باركود Case — يجب أن يحصل على ordinal = (عدد الباركودات السابقة + 1) ≠ ordinal المريض "أ"
3. تحقق أن `BarcodeValue` مختلفين بين المريضَيْن
4. افحص قاعدة البيانات: لا يوجد صفان لهما نفس `BarcodeValue`

### 6. تقدير حجم التغيير

**صغير جداً** — حذف سطر واحد من `BarcodeGenerator.cs`.

---

## العطل رقم 3: إعادة إدراج Lab ID تُفشل المريض العائد

### 1. الإصلاح المقترح

التحقق من وجود `PatientBarcode` بـ `CodeType.Lab` قبل الإدراج في `GenerateBarcodesForVisitAsync`.

**ملف واحد يتغير:** `FinalLabSystem/Services/Implementations/SampleTrackingService.cs`

**قبل (سطرا 51 ثم 58-88):**
```csharp
var labId = await _barcodeGenerator.GetOrCreateLabIdAsync(patientId);
// ... ثم:
_context.PatientBarcodes.AddRange(
    new PatientBarcode { /* Case */ },
    new PatientBarcode { /* File */ },
    new PatientBarcode
    {
        PatientId = patientId,
        VisitId = null,
        CodeType = BarcodeCodeType.Lab,
        BarcodeValue = labId,
        IssueDate = DateTime.Now,
        SortOrdinal = 0,
        CreatedBy = staffId
    }
);
```

**بعد:**
```csharp
var labId = await _barcodeGenerator.GetOrCreateLabIdAsync(patientId);
// ... ثم:

var hasLabBarcode = await _context.PatientBarcodes
    .AnyAsync(pb => pb.PatientId == patientId && pb.CodeType == BarcodeCodeType.Lab);

var patientBarcodes = new List<PatientBarcode>
{
    new PatientBarcode
    {
        PatientId = patientId,
        VisitId = visitId,
        CodeType = BarcodeCodeType.Case,
        BarcodeValue = caseCode,
        IssueDate = DateTime.Now,
        SortOrdinal = ordinal,
        CreatedBy = staffId
    },
    new PatientBarcode
    {
        PatientId = patientId,
        VisitId = visitId,
        CodeType = BarcodeCodeType.File,
        BarcodeValue = fileCode,
        IssueDate = DateTime.Now,
        SortOrdinal = ordinal,
        CreatedBy = staffId
    }
};

if (!hasLabBarcode)
{
    patientBarcodes.Add(new PatientBarcode
    {
        PatientId = patientId,
        VisitId = null,
        CodeType = BarcodeCodeType.Lab,
        BarcodeValue = labId,
        IssueDate = DateTime.Now,
        SortOrdinal = 0,
        CreatedBy = staffId
    });
}

_context.PatientBarcodes.AddRange(patientBarcodes);
```

### 2. البدائل

| البدائل | إيجابيات | سلبيات | التوصية |
|---|---|---|---|
| **A) فحص قبل الإدراج (المُقترح)** | يمنع التكرار، لا يغير هيكل البيانات | يحتاج استعلام إضافي | **الأنسب** — الحل الأوضح والأكثر أماناً |
| B) استخدام `AddOrUpdate` / Upsert | يجمع الفحص والإدراج في مكان واحد | أعقد في EF Core بدون مكتبة إضافية | مفرط التعقيد |
| C) تعديل GetOrCreateLabIdAsync ليعيد هل هو جديد أم موجود | يجمع المسؤوليات | يُغيّر واجهة الدالة | غير مبرر |

### 3. Migration المطلوب

**لا يوجد.** التعديل على كود فقط.

### 4. أثر الإصلاح على بيانات موجودة

**لا يوجد خطر.** لا يُنشئ بيانات جديدة — فقط يمنع إدراج صف مكرر.

### 5. خطوة التحقق بعد التنفيذ

1. أنشئ مريض جديد → زيارته الأولى → توليد باركود — يجب أن ينجح وينشئ Lab Barcode
2. أنشئ زيارته الثانية → توليد باركود — يجب أن ينجح **بدون** خطأ (يُعيد استخدام Lab Barcode الموجود)
3. افحص قاعدة البيانات: لا يوجد أكثر من صف واحد بـ `CodeType = Lab` لنفس المريض

### 6. تقدير حجم التغيير

**صغير** — تعديل بسيط في `SampleTrackingService.cs` (إضافة ~10 أسطر).

---

## العطل رقم 4: خطأ في حساب LOrH

### 1. الإصلاح المقترح

استخدام `SnapLowCritical` و `SnapHighCritical` (الحدود الحرجة الفعلية) بدلاً من `low * 2` و `high * 2`.

**ملف واحد يتغير:** `FinalLabSystem/Models/DTOs/TestComponentResultDto.cs`

**قبل (سطرا 80-95):**
```csharp
public string LOrH
{
    get
    {
        if (ResultNumeric == null || SnapLowNormal == null || SnapHighNormal == null)
            return string.Empty;
        var val = (double)ResultNumeric.Value;
        var low = SnapLowNormal.Value;
        var high = SnapHighNormal.Value;
        if (val >= high * 2) return "HH";
        if (val <= low * 2) return "LL";
        if (val > high) return "H";
        if (val < low) return "L";
        return string.Empty;
    }
}
```

**بعد:**
```csharp
public string LOrH
{
    get
    {
        if (ResultNumeric == null || SnapLowNormal == null || SnapHighNormal == null)
            return string.Empty;
        var val = (double)ResultNumeric.Value;
        var low = SnapLowNormal.Value;
        var high = SnapHighNormal.Value;

        if (SnapHighCritical.HasValue && val >= SnapHighCritical.Value)
            return "HH";
        if (SnapLowCritical.HasValue && val <= SnapLowCritical.Value)
            return "LL";
        if (val > high) return "H";
        if (val < low) return "L";
        return string.Empty;
    }
}
```

**الشرح:** نستخدم `SnapHighCritical` و `SnapLowCritical` (الحدود الحرجة الفعلية) بدلاً من `high * 2` و `low * 2`.

**السلوك الاحتياطي (Fallback) عند غياب الحدود الحرجة:**
إذا لم تكن `SnapLowCritical` أو `SnapHighCritical` مملوءة (أي `HasValue == false`) — وهو الوضع الطبيعي لعدد كبير من التحاليل — فإن الخاصية لا تُرجع "HH" أو "LL" أبداً لذلك التحليل. تبقى المقارنة العادية بـ H/L (المدى الطبيعي) فقط تعمل كما هي دون تأثير. لا خطأ، لا استثناء، فقط غياب تصنيف "حرج جداً" لهذا التحليل تحديداً.

**البيانات الفعلية في قاعدة البيانات:**
من أصل **461** صف NormalRange مُزرع في قاعدة البيانات:
- **0** صف (0%) يملك حدوداً حرجة (`LowCritical`/`HighCritical`) مملوءة
- **461** صف (100%)均有 `LowCritical = NULL` و `HighCritical = NULL`

هذا يعني أن السلوك الاحتياطي المذكور أعلاه هو **الحالة المهيمنة** — معظم التحاليل ستعمل بتصنيف H/L العادي فقط، وهو السلوك الصحيح.

### 2. البدائل

| البدائل | إيجابيات | سلبيات | التوصية |
|---|---|---|---|
| **A) استخدام SnapLowCritical/SnapHighCritical مع Fallback آمن (المُقترح)** | يعتمد على الحدود الحرجة المُعرّفة فعلياً، يعمل بشكل صحيح مع 461 تحليل بلا حدود حرجة | لا يُظهر HH/LL إلا للتحاليل التي تملك حدود حرجة | **الأنسب** |
| B) إسقاط HH/LL بالكامل واستبدالها بـ H/L فقط | أبسط تعديل | يفقد ميزة التمييز بين مرتفع ومنخفض جداً | أقل دقة |
| C) تغيير `low * 2` إلى `low` فقط (HH → high، LL → low) | أبسط تعديل هيكلياً | لا يُصلح المشكلة — القيم الطبيعية ستبقى "L" أو "H" بدلاً من فارغ | **غير صحيح** |

### 3. Migration المطلوب

**لا يوجد.** التعديل على كود فقط (logic في DTO).

### 4. أثر الإصلاح على بيانات موجودة

**أثر محدود ومتوقع:**
- **البيانات الرقمية:** لا تأثير — القيم المُدخلة (`ResultValue`, `ResultNumeric`) صحيحة دائماً
- **التصنيف المعروض:** سيتغير فقط للتحاليل التي كان يظهر فيها "HH" أو "LL" بسبب الخطأ
- **التحاليل بلا حدود حرجة (461 تحليل):** لن يظهر لها HH/LL أبداً — فقط H/L كما هو
- لا حاجة لتحديث البيانات المحفوظة — الحساب يحدث في runtime فقط

### 5. خطوة التحقق بعد Executors

1. **تحليل به حدود حرجة (إن وُجد):** أدخل نتيجة أعلى من HighCritical — يجب أن تظهر "HH"
2. **تحليل بلا حدود حرجة (الأغلبية):** أدخل نتيجة Hemoglobin = 5.0 مع مدى طبيعي 3.8–5.2:
   - **قبل الإصلاح:** LOrH = "LL" (خطأ — 5.0 طبيعية)
   - **بعد الإصلاح:** LOrH = "" (صحيح — ضمن النطاق)
3. **تحليل منخفض فعلاً:** أدخل نتيجة Hemoglobin = 2.0 مع مدى طبيعي 3.8–5.2 — يجب أن تظهر "L" (ليس "LL" لأن لا حدود حرجة)
4. **تحليل مرتفع فعلاً:** أدخل نتيجة Hemoglobin = 8.0 مع مدى طبيعي 3.8–5.2 — يجب أن تظهر "H" (ليس "HH")
5. **تحليل بلا حدود حرجة — لا خطأ:** تأكد من عدم ظهور أي استثناء أو خطأ في وقت التشغيل لأي نتيجة مُدخلة

### 6. تقدير حجم التغيير

**صغير جداً** — تعديل سطرين في `TestComponentResultDto.cs`.

---

## العطل رقم 5: Hard Delete للزيارات بلا فحص صلاحية ولا أثر تدقيقي

### 1. الإصلاح المقترح

تحويل الحذف الفعلي إلى إلغاء ناعم (Soft Delete) باستخدام `VisitStatus.Cancelled`، مع فحص صلاحية صريح والاعتماد على آلية التدقيق الموجودة.

**3 ملفات تتغير:**
1. `FinalLabSystem/Services/Interfaces/IVisitService.cs` — تغيير signature
2. `FinalLabSystem/Services/Implementations/VisitService.cs` — `CancelVisitAsync`
3. `FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs` — المُستدعي

**آليات الصلاحية والتدقيق الموجودة فعلاً في المشروع:**

| الآلية | الملف | الوصف |
|---|---|---|
| `IAuthService.HasPermissionAsync(staffId, permissionCode)` | `AuthService.cs` | يتحقق من صلاحية موظف محدد عبر `Permission` + `StaffPermission` |
| `Staff.IsAdmin` | `Staff.cs` | تجاوز المدير لكل الصلاحيات تلقائياً |
| `[Auditable]` على `Visit` | `Visit.cs:10` | يفعّل آلية التدقيق الأوتوماتيكية في `SaveChangesAsync` |

**آلية التدقيق الأوتوماتيكية (`FinalLabDbContext.SaveChangesAsync`):**
عند تغيير `visit.VisitStatus` واستدعاء `SaveChangesAsync()`:
1. `ChangeTracker` يلتقط التغيير لأن `Visit` يملك `[Auditable]`
2. يُنشأ `AuditLog` تلقائياً مع: `TableName = "Visit"`, `FieldName = "VisitStatus"`, `OldValue` = القيمة القديمة, `NewValue` = "Cancelled", `ChangedBy` = `_session.CurrentUser?.StaffId`
3. لا نحتاج `CancelledBy` أو `CancelledAt` إضافياً — التدقيق الأوتوماتيكية تُسجّل كل شيء

**الكود الحالي (قبل) — VisitService.csسطرا 373-404:**
```csharp
public async Task<bool> CancelVisitAsync(int visitId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var visit = await _context.Visits
            .Include(v => v.VisitTests)
            .Include(v => v.Payments)
            .Include(v => v.SampleTubes)
            .Include(v => v.VisitCharges)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit is null)
            return false;

        _context.VisitTests.RemoveRange(visit.VisitTests);
        _context.Payments.RemoveRange(visit.Payments);
        _context.SampleTubes.RemoveRange(visit.SampleTubes);
        _context.VisitCharges.RemoveRange(visit.VisitCharges);
        _context.Visits.Remove(visit);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

**الكود الجديد (بعد):**
```csharp
public async Task<bool> CancelVisitAsync(int visitId, int staffId)
{
    // 1. فحص الصلاحية — يجب أن يملك الموظف صلاحية "VISITS.CANCEL" أو يكون Admin
    var hasPermission = await _authService.HasPermissionAsync(staffId, "VISITS.CANCEL");
    if (!hasPermission)
        throw new UnauthorizedAccessException(
            "ليس لديك صلاحية لإلغاء الزيارة. يُرجى التواصل مع المسؤول.");

    // 2. الإلغاء الناعم
    var visit = await _context.Visits.FindAsync(visitId);
    if (visit is null)
        return false;

    visit.VisitStatus = VisitStatus.Cancelled;
    visit.PaymentStatus = PaymentStatus.Cancelled;
    visit.UpdatedAt = DateTime.UtcNow;

    // 3. SaveChangesAsync يُسجّل التغيير تلقائياً عبر آليات [Auditable]
    await _context.SaveChangesAsync();
    return true;
}
```

**تغيير signature في IVisitService.cs:**
```csharp
// قبل:
Task<bool> CancelVisitAsync(int visitId);

// بعد:
Task<bool> CancelVisitAsync(int visitId, int staffId);
```

**تغيير المُستدعي في PatientRegistrationViewModel.cs (سطر 450):**
```csharp
// قبل:
var deleted = await _visitService.CancelVisitAsync(CurrentVisitId);

// بعد:
var staffId = _currentUserSession.CurrentUser?.StaffId
    ?? throw new InvalidOperationException("لا يمكن إلغاء الزيارة بدون جلسة مستخدم.");
var deleted = await _visitService.CancelVisitAsync(CurrentVisitId, staffId);
```

**ملاحظة:** `_authService` يجب أن يكون مُسجّلاً في DI ومتاحاً في `VisitService`. إذا لم يكن كذلك، يجب إضافته إلى Constructor:
```csharp
public VisitService(FinalLabDbContext context, ILogger<VisitService> logger, IAuthService authService)
{
    _context = context;
    _logger = logger;
    _authService = authService;
}
```

**إنشاء صلاحية "VISITS.CANCEL":**
يجب إدراج صلاحية جديدة في جدول `Permission` (يتم يدوياً أو عبر Seed Script):
```sql
INSERT INTO Permission (PermissionCode, PermissionName, PermissionGroup, Description)
VALUES ('VISITS.CANCEL', 'إلغاء الزيارة', 'VISITS', 'الصلاحية لإلغاء زيارات المرضى');
```

### 2. البدائل

| البدائل | إيجابيات | سلبيات | التوصية |
|---|---|---|---|
| **A) Soft Delete + فحص صلاحية + تدقيق أوتوماتيكي (المُقترح)** | يحافظ على البيانات، يدعم التدقيق عبر `[Auditable]` الموجود، يمنع الحذف بدون صلاحية | لا يحذف البيانات فعلياً (قد يكون مطلوباً أحياناً) | **الأنسب** — الأنظف والأكثر أماناً |
| B) Hard Delete مع فحص صلاحية + تسجيل AuditLog يدوياً | يحافظ على الحذف الفعلي لكن مع أمان | لا يدعم التراجع، البيانات تضيع نهائياً، يحتاج تسجيل يدوي | أقل أماناً من A |
| C) Hard Delete + حفظ نسخة في جدول أرشيف | يجمع الحذف والأرشيف | يحتاج تعديل أكبر | مفرط لهذا العطل |

### 3. Migration المطلوب

**لا يوجد للكود الأساسي.** `VisitStatus.Cancelled` موجود مسبقاً في ENUM.
**يُضاف فقط:** سجل `Permission` جديد بـ `PermissionCode = "VISITS.CANCEL"` (ليس migration —只是 بيانات seed).

### 4. أثر الإصلاح على بيانات موجودة

**⚠️ قد يكون هناك أثر.** إذا كانت هناك زيارات مُلغاة سابقاً (محذوفة بالفعل من قاعدة البيانات)، فلن يمكن استعادتها. هذا هو الفرق بين الحذف الفعلي والناعم — كل الزيارات المُلغاة حالياً **محذوفة نهائياً** ولا يمكن استعادتها.

### 5. خطوة التحقق بعد التنفيذ

1. **اختبار صلاحية — موظف غير مخوَّل:**
   - سجّل دخول بموظف لا يملك صلاحية `VISITS.CANCEL` وليس Admin
   - افتح زيارة → اضغط "إلغاء"
   - **المتوقع:** رسالة خطأ: "ليس لديك صلاحية لإلغاء الزيارة" ولا يحدث أي تغيير

2. **اختبار صلاحية — موظف مخوَّل:**
   - سجّل دخول بموظف يملك صلاحية `VISITS.CANCEL` أو يكون Admin
   - افتح زيارة → اضغط "إلغاء" → تأكيد
   - **المتوقع:** الزيارة تبقى في قاعدة البيانات مع `VisitStatus = Cancelled`

3. **اختبار التدقيق (Audit Trail):**
   - بعد الإلغاء، استعلم عن `AuditLog`:
     ```sql
     SELECT * FROM AuditLog
     WHERE TableName = 'Visit'
       AND FieldName = 'VisitStatus'
       AND NewValue = 'Cancelled'
     ORDER BY ChangedAt DESC;
     ```
   - **المتوقع:** يظهر سجل يُظهر: `OldValue` = القيمة القديمة, `NewValue` = "Cancelled", `ChangedBy` = معرّف الموظف

4. **اختبار عدم ظهور الزيارة المُلغاة:**
   - تحقق من أن الزيارة المُلغاة لا تظهر في قائمة المرضى اليوميين

### 6. تقدير حجم التغيير

**صغير-متوسط** — تعديل `CancelVisitAsync` + تعديل الـ signature + تعديل المُستدعي + إضافة صلاحية واحدة + تعديل Constructor.

---

## ملخص التحديثات بناءً على توضيحات صاحب المشروع

| العطل | التحديث |
|---|---|
| **العطل 1 (الخصم)** | تأكيد أن الكود يدعم كلا الاتجاهين (مبلغ→نسبة ونسبة→مبلغ) بالفعل مع `_isUpdating`. لا حاجة لـ `DiscountEntryMode`. الحل يبقى: إسقاط القيد فقط. أُضيف اختبار صريح لكلا الاتجاهين في خطوة التحقق. |
| **العطل 4 (LOrH)** | تأكيد السلوك الاحتياطي: عند غياب الحدود الحرجة لا يظهر HH/LL أبداً، فقط H/L يعمل. أُضيف رقم فعلي: 0 من 461 تحليل يملك حدوداً حرجة. أُضيف اختبار تحليل بلا حدود حرجة في خطوة التحقق. |
| **العطل 5 (إلغاء الزيارة)** | استخدام آلية الصلاحية الموجودة (`IAuthService.HasPermissionAsync` + `VISITS.CANCEL`). الاعتماد على `[Auditable]` الأوتوماتيكي للتدقيق (لا `CancelledBy` إضافي). تأكيد أن `SaveChangesAsync` يُسجّل `VisitStatus` تلقائياً. إضافة استعلام AuditLog في خطوة التحقق. |

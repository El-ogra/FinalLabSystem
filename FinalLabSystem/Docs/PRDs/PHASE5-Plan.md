# PHASE 5 — خطة العمل التنفيذية الكاملة
# Inventory & Cash Drawer

> **المرجع الأعلى:** `Docs/PRDs/system-alignment-master-prd-v4.md` (سطور 859 – 880)
> **حالة البداية:** نهاية Phase 4 — Commit `bae71cb` (بعد تنفيذ المرحلة الرابعة)
> **عدد الاختبارات الحالي (مرجعي):** **431 / 431** ناجحة — حسب `IMPLEMENTATION_STATUS.md` سطر 421
> **آخر Migration مُطبَّقة:** `20260626032417_AddCompanyContractFields` (Phase 4 — Slice 1)
> **تعليمات قيدية:** لا تعديل لأي ملف موجود ولا إنشاء/حذف. هذا تحليل وتخطيط فقط.

---

## الجزء الأول — تحليل الوضع الراهن لـ Phase 5

### 1. الاسم الحرفي المقفل لـ Phase 5

من `Docs/PRDs/IMPLEMENTATION_STATUS.md` سطر **577**:

> ```
> ## Phase 5: Inventory & Cash Drawer
> **Status:** ⏳ في الانتظار
> ```

ومن `system-alignment-master-prd-v4.md` سطر **859**:

> `Phase 5 — Inventory & Cash Drawer`

**الاسم المقفل:** `Phase 5: Inventory & Cash Drawer` — يُمنع تعديله.

---

### 2. المحاور الرئيسية لـ Phase 5 من V4

من V4 سطور **862 – 879**، المحاور الخمسة الرسمية:

| المحور | اسمه في V4 | الـ Window المطلوبة |
|---|---|---|
| **محور 1** | Task 5.1 — IAttendanceService DI + شاشة الحضور | `AttendanceWindow` |
| **محور 2** | Task 5.2 — Cash Drawer screen | `CashDrawerWindow` |
| **محور 3** | Task 5.3 — Inventory screen + Low-Stock Alert | `InventoryWindow` + Dashboard hook |
| **محور 4** | Task 5.4 — Referral Commission Report UI | `CommissionReportWindow` |
| **محور 5** | Task 5.5 — Outstanding Balance Report UI | `OutstandingBalanceReportWindow` |

**قواعد العمل المقفلة (V4 سطور 877 – 879):**

- `BR-050`: Cash Drawer يُفتح بكلمة مرور (Help.pdf default `"123"` — يجب تخزين hash مع إجبار التغيير أول مرة).
- `BR-051`: Attendance يَتطلَّب `shift_id` من `WorkShift`.
- `BR-052`: Inventory alert عند `CurrentStock < MinimumStock` يَظهر في الـ dashboard.

**Gaps المرصودة لـ Phase 5 (من V4 سطور 1051 و 1093 – 1099):**

| كود | الفجوة | الخطورة |
|---|---|---|
| G-006 | `IAttendanceService` غير مسجَّلة | 🔴 Critical |
| G-048 | `CashDrawerWindow` غير موجود | 🔴 Critical |
| G-049 | `InventoryWindow` غير موجود | 🟠 High |
| G-050 | `AttendanceWindow` غير موجود | 🟠 High |
| G-051 | `VReferralCommissionReport` UI غير موجود | 🟡 Medium |
| G-052 | `VOutstandingBalance` UI غير موجود | 🟡 Medium |
| G-053 | `SampleTube.MinimumStock` alert logic غير موجود | 🟡 Medium |
| G-054 | Cash Drawer password protection workflow غير موجود | 🟠 High |

---

### 3. حالة كل محور في الكودبيس

#### 🔹 المحور 1 — Attendance

| العنصر | الحالة | الدليل |
|---|---|---|
| كيان `Attendance` | ✅ موجود كامل | `Models/Attendance.cs` (32 سطر) |
| كيان `WorkShift` | ✅ موجود كامل | `Models/WorkShift.cs` (24 سطر) |
| DbSet `Attendances` | ✅ مُعرَّفة | `Data/FinalLabDbContext.cs` سطر 201 |
| DbSet `WorkShifts` | ✅ مُعرَّفة | `Data/FinalLabDbContext.cs` سطر 221 |
| Fluent mapping `Attendance` | ✅ موجود | `Data/FinalLabDbContext.cs` سطر 2034 – 2057 |
| Fluent mapping `WorkShift` | ✅ موجود | `Data/FinalLabDbContext.cs` سطر 2255+ |
| `IAttendanceService` interface | ✅ موجود لكن **محدود** (3 دوال) | `Services/Interfaces/IAttendanceService.cs` (27 سطر) |
| `AttendanceService` implementation | ✅ موجود لكن محدود | `Services/Implementations/AttendanceService.cs` (75 سطر) — يحتوي `GetActiveShiftsAsync`, `RecordClockInAsync`, `RecordClockOutAsync` |
| **DI Registration لـ `IAttendanceService`** | ❌ **غير مسجَّلة** | `App.xaml.cs` سطور 149 – 171 — لا يوجد `services.AddScoped<IAttendanceService, AttendanceService>()` |
| `AttendanceWindow.xaml` | ❌ **مفقود تماماً** | `find Views -iname "Attendance*"` → 0 |
| `AttendanceWindowViewModel` | ❌ مفقود تماماً | `find ViewModels -iname "Attendance*"` → 0 |
| WorkShifts CRUD UI | ❌ مفقود تماماً | — |
| Tests لـ `AttendanceService` | ❌ مفقودة | `find Tests -iname "Attendance*"` → 0 |

**الناقص:**
- توسعة `IAttendanceService` بـ: `GetAttendanceByDateRangeAsync`, `GetTotalHoursWorkedAsync`, `GetActiveAttendanceAsync(staffId)`, CRUD لـ WorkShifts.
- تسجيل `IAttendanceService` في DI.
- بناء `AttendanceWindow` + ViewModel من الصفر.
- اختبارات وحدة وتكامل.

---

#### 🔹 المحور 2 — Cash Drawer

| العنصر | الحالة | الدليل |
|---|---|---|
| كيانات الإطار المالي (`Visit`, `Payment`, `VisitCharge`) | ✅ موجودة | `Models/Visit.cs` سطور 72 – 84، `Models/Payment.cs`، `Models/VisitCharge.cs` |
| حقول الزيارة المالية (`Subtotal`, `DiscountAmount`, `TotalAfterDiscount`, `TotalPaid`, `BalanceDue`) | ✅ موجودة | `Models/Visit.cs` سطور 72 – 82 |
| `IFinancialService` | ✅ مسجَّلة | `App.xaml.cs` سطر 155 |
| **دوال Cash Drawer summary** في `IFinancialService` | ❌ **مفقودة تماماً** | `Services/Interfaces/IFinancialService.cs` — لا توجد `GetDailySummaryAsync` ولا أي دالة تجميعية |
| `ICashDrawerService` (لتغليف summary + password) | ❌ مفقود | — |
| `CashDrawerWindow.xaml` | ❌ **مفقود تماماً** | — |
| `CashDrawerWindowViewModel` | ❌ مفقود تماماً | — |
| `CashDrawerPasswordHash` في `LabSetting` | ❌ غير مُهيّأ كـ key | `Models/LabSetting.cs` يدعم key/value لكن المفتاح لم يُهيَّأ |
| منطق فحص كلمة المرور + إجبار تغييرها أول مرة | ❌ مفقود تماماً | لا `CashDrawerUnlockDialog` |
| MainWindow.xaml يربط زر Cash Drawer | ❌ مفقود — `AccountsMenuViewModel` يحوي placeholder فقط للـ Cash Drawer (انظر سطر 15) | `ViewModels/Menu/AccountsMenuViewModel.cs` سطر 15: `PlaceholderCommand = new RelayCommand(_ => { });` |
| Tests | ❌ مفقودة | — |

**ملاحظة معمارية حرجة:** سطح `IFinancialService` الحالي (`Services/Interfaces/IFinancialService.cs` سطور 7 – 57) يقتصر على عمليات الدفع/الخصم لزيارة واحدة. لا توجد دوال aggregation تتولَّى تجميع يوم كامل بفلاتر (branch/doctor/user/date). هذا يستلزم إنشاء `ICashDrawerService` جديدة بدلاً من توسعة `IFinancialService` للحفاظ على Single Responsibility.

**الناقص:**
- `ICashDrawerService` + تنفيذ + DI.
- منطق password (تجزئة hash، إجبار تغيير، تخزين في `LabSetting`).
- شاشة `CashDrawerUnlockDialog`.
- شاشة `CashDrawerWindow` (مع filters: branch/doctor/user/date).
- ربط زر في `AccountsMenuViewModel` و `MainWindow.xaml` DataTemplate.
- Tests.

---

#### 🔹 المحور 3 — Inventory

| العنصر | الحالة | الدليل |
|---|---|---|
| كيان `SampleTube` | ✅ موجود | `Models/SampleTube.cs` (37 سطر) — **لكن** لا يحتوي `MinimumStock` ولا `CurrentStock` |
| كيان `TubeMaterial` | ✅ موجود | `Models/TubeMaterial.cs` (22 سطر) — يحوي `MaterialName`, `TubeColor`, `IsActive`, `SortOrder` — **لكن** لا يحتوي `MinimumStock` ولا `CurrentStock` |
| `VSampleTubeStatus` view | ✅ موجود | `Models/VSampleTubeStatus.cs` (31 سطر) — يَعرض الأنابيب لكن لا يحسب stock |
| Fluent mapping لـ `TubeMaterial` | ✅ موجود | `Data/FinalLabDbContext.cs` سطر 1117 – 1127 |
| `IInventoryService` | ❌ **غير موجودة** | `find Services -iname "*Inventory*"` → 0 |
| `InventoryWindow.xaml` | ❌ مفقود | — |
| `InventoryWindowViewModel` | ❌ مفقود | — |
| Low-stock alert hook في Dashboard (`HomeMenuViewModel`) | ❌ مفقود | `ViewModels/Menu/HomeMenuViewModel.cs` يحوي رسالة ترحيب فقط (13 سطر) |
| Tests | ❌ مفقودة | — |

**قرار معماري مقفل (مستنبط من V4 سطر 866):** المحور الثالث يَجرد ثلاث جهات:
1. أنابيب جاهزة لكل زيارة (`SampleTube` — هو ركن operational لا inventory).
2. **مادة الأنبوب (`TubeMaterial`)** — هذا هو الـ inventory الحقيقي (نقص/زيادة Stock).
3. الاستهلاكات (consumables) — V4 لا يَذكر كياناً قائماً ولا migration لـ "Consumable". لذا الـ Phase 5 سيقتصر على إضافة `MinimumStock` و `CurrentStock` إلى **`TubeMaterial`** (وليس `SampleTube` كما يقترح V4 سطر 873 — لأن `SampleTube` يحوي أنبوب واحد لزيارة واحدة لا inventory).

**ملاحظة:** V4 سطر 873 يقترح "إضافة `MinimumStock` على `SampleTube`". هذا اقتراح ظاهري في V4، لكن تحليل تصميم `SampleTube` (يَخزِّن `tube_id`, `visit_id`, `barcode_value`, `collected_at` — أي سجل تجميع فردي) يَجعل وَضع `MinimumStock` عليه **غير منطقي معمارياً**. الـ Migration ستضيف `MinimumStock`, `CurrentStock` إلى **`TubeMaterial`** لأنه يَمثّل النوع/المادة لا الفرد.

> ⚠️ هذا قرار يستلزم تأكيد مالك المشروع قبل التنفيذ. **الخطة الحالية تضع الحقلين على `TubeMaterial`**، وتُسجِّل هذا الانحراف عن V4 صراحةً في قسم "القرارات المعمارية" أدناه.

**الناقص:**
- إضافة `MinimumStock` (int, default 0) و `CurrentStock` (int, default 0) إلى `TubeMaterial`.
- Migration لإضافة العمودين.
- `IInventoryService` (CRUD + AdjustStock + GetLowStockItemsAsync).
- `InventoryWindow` + ViewModel.
- Dashboard alert في `HomeMenuViewModel` + DataTemplate في `MainWindow.xaml`.
- Tests.

---

#### 🔹 المحور 4 — Referral Commission Report

| العنصر | الحالة | الدليل |
|---|---|---|
| Database view `VReferralCommissionReport` | ✅ مُعرَّفة | `Data/FinalLabDbContext.cs` سطر 184 + Fluent map سطر 1667+ |
| Model `VReferralCommissionReport` | ✅ موجود | `Models/VReferralCommissionReport.cs` (29 سطر) — يَحوي `ReferralName`, `CommissionRate`, `VisitId`, `VisitDate`, `PatientName`, `VisitTotal`, `TotalPaid`, `CommissionDue` |
| `ICommissionReportService` (أو دالة في `IFinancialService`) | ❌ مفقود | — |
| `CommissionReportWindow` | ❌ مفقود تماماً | — |
| ViewModel | ❌ مفقود | — |
| Tests | ❌ مفقودة | — |

**الناقص:**
- خدمة لاستعلام الـ view (يُفضَّل `ICommissionReportService` لتغليف الفلاتر).
- شاشة UI مع filters: تاريخ من/إلى + ReferralId + SourceType.
- زر طباعة (يُعيد استخدام `IPrintService` الموجود — مسجَّل في `App.xaml.cs` سطر 183).
- Tests.

---

#### 🔹 المحور 5 — Outstanding Balance Report

| العنصر | الحالة | الدليل |
|---|---|---|
| Database view `VOutstandingBalance` | ✅ مُعرَّفة | `Data/FinalLabDbContext.cs` سطر 178 + Fluent map سطر 1539+ |
| Model `VOutstandingBalance` | ✅ موجود | `Models/VOutstandingBalance.cs` (31 سطر) — يحوي `BalanceDue`, `PaymentStatus`, `DaysOverdue` |
| Service | ❌ مفقود | — |
| `OutstandingBalanceReportWindow` | ❌ مفقود | — |
| ViewModel | ❌ مفقود | — |
| Tests | ❌ مفقودة | — |

**الناقص:**
- نفس بنية المحور 4 لكن لـ `VOutstandingBalance`.

---

### 4. الخدمات المرتبطة بـ Phase 5 غير المسجَّلة في `App.xaml.cs`

بحث في `App.xaml.cs` سطور 149 – 186 يكشف:

| الخدمة | الحالة في DI | الـ Slice المطلوبة |
|---|---|---|
| `IAttendanceService` ↔ `AttendanceService` | ❌ غير مسجَّلة (Gap G-006) | Slice 1 |
| `ICashDrawerService` ↔ `CashDrawerService` | ❌ غير موجودة أصلاً | Slice 2 |
| `IInventoryService` ↔ `InventoryService` | ❌ غير موجودة أصلاً | Slice 3 |
| `ICommissionReportService` ↔ `CommissionReportService` | ❌ غير موجودة أصلاً | Slice 4 |
| `IOutstandingBalanceService` ↔ `OutstandingBalanceService` | ❌ غير موجودة أصلاً | Slice 5 |
| `IFinancialService` | ✅ مسجَّلة `App.xaml.cs` سطر 155 — لا توسعة مطلوبة في Phase 5 |
| `ISettingsService` | ✅ مسجَّلة `App.xaml.cs` سطر 149 — تُستخدم لتخزين hash كلمة سر Cash Drawer |
| `IPrintService` | ✅ مسجَّلة `App.xaml.cs` سطر 183 — تُستخدم لطباعة التقارير |

كل خدمة + ViewModel + Window جديدة ستُسجَّل في `App.xaml.cs` ضمن نفس الـ Slice الذي تُنشأ فيه (قاعدة DI الإلزامية).

---

### 5. الـ Migrations المطلوبة لـ Phase 5

بناءً على فحص الكيانات:

| Migration | الغرض | المُحَفِّز |
|---|---|---|
| `AddTubeMaterialStockFields` | إضافة عمودَي `minimum_stock` (INT NOT NULL DEFAULT 0) و `current_stock` (INT NOT NULL DEFAULT 0) إلى جدول `TubeMaterial` | BR-052 + Gap G-053 |
| `AddCashDrawerSettings` (اختياري — قد يُستعاض عنه بـ seed في `TestCatalogSeeder`) | إضافة `LabSetting` rows لـ `CashDrawerPasswordHash` و `CashDrawerPasswordMustChange` كـ initial data | BR-050 |

> **قرار:** بدلاً من Migration ثانية، يُستخدم `ISettingsService.UpsertSettingAsync` عند أول إطلاق لإدخال hash لكلمة `"123"` مع `MustChange=true`. هذا يُجنِّب Migration إضافية للبيانات.

**عدد الـ Migrations الفعلي لـ Phase 5:** **1 Migration واحدة** (`AddTubeMaterialStockFields`).

---

### 6. العدد الفعلي الحالي للاختبارات بعد Phase 4

من `IMPLEMENTATION_STATUS.md` سطر 421: **431 / 431 ناجحة**.

(فحص `grep -rE "\[Fact\]|\[Theory\]" FinalLabSystem.Tests --include="*.cs" | wc -l` يُرجِع 430 method declarations؛ مع InlineData للـ Theory الواحدة في `WpfFlowDocumentPrintServiceTests.cs` (5 cases) يَكون الإجمالي = 434 test cases. **المرجع الرسمي المُعتمَد هو 431** كما في الـ Status File.)

**نقطة البداية المعتمدة لـ Phase 5:** `431 / 431 ✅`.

---

## الجزء الثاني — خطة العمل التنفيذية الكاملة لـ Phase 5

### القرارات المعمارية المُوثَّقة (مقفلة قبل البدء)

| # | القرار | السبب |
|---|---|---|
| **D5.1** | فصل `ICashDrawerService` عن `IFinancialService` | `IFinancialService` مسؤول عن الدفع/الخصم لزيارة واحدة (Single Responsibility) — Cash Drawer aggregation عَمَل مختلف |
| **D5.2** | حقول Stock تُضاف إلى `TubeMaterial` لا `SampleTube` | `SampleTube` يَمثّل سجل أنبوب فردي لزيارة واحدة — وضع `MinimumStock` عليه يَكسر دلالته. هذا انحراف موثَّق عن V4 سطر 873 |
| **D5.3** | `ICashDrawerService.UnlockAsync(password)` يُجبر تغيير كلمة السر إذا `MustChange=true` | BR-050 |
| **D5.4** | `IAttendanceService` تُوسَّع بـ 4 دوال إضافية لكن سطحها الحالي **لا يُكسر** | الحفاظ على backward compatibility — لا اختبارات قديمة تنكسر |
| **D5.5** | كل تقرير في Phase 5 يستخدم `IPrintService` الموجود بالفعل | إعادة استخدام، لا خدمة طباعة جديدة |
| **D5.6** | Dashboard alert للـ low stock يُحقَن في `HomeMenuViewModel` (لا shell widget عام) | البساطة — `HomeMenuViewModel` هو نقطة وصول مرئية مضمونة |
| **D5.7** | كلمة سر Cash Drawer تُجزَّأ بـ BCrypt (أو PBKDF2 إن BCrypt غير متاح) عبر `IAuthService` الموجود | إعادة استخدام آلية تجزئة موجودة بدلاً من إدخال مكتبة جديدة |
| **D5.8** | كل خدمة/VM/Window جديدة تُسجَّل في `App.xaml.cs` ضمن نفس الـ Slice الذي يُنشئها (قاعدة DI الإلزامية) | منع كسر البناء — لا تأجيل تسجيلات |

> ⚠️ **D5.7 يَفترض** أن `IAuthService` يُصدِّر دالة hash مفيدة. إذا لم يكن كذلك، يَستلزم Slice 2 إضافة dependency بسيطة على `System.Security.Cryptography` فقط (لا حزم خارجية).

---

### نظرة عامة على الـ Slices

| # | الـ Slice | مدة | خطورة | Blocked by |
|---|---|---|---|---|
| 1 | Attendance Foundation (Service + DI + Window) | 2 يوم | 🔴 Critical | لا شيء |
| 2 | Cash Drawer (Service + Password + Window + AccountsMenu wiring) | 3 يوم | 🔴 Critical | Slice 1 (للتسجيل الموحَّد فقط — لا dependency فعلي) |
| 3 | Inventory (Migration + Service + Window + Dashboard Alert) | 2 يوم | 🟠 High | لا شيء |
| 4 | Referral Commission Report | 1 يوم | 🟡 Medium | Slice 2 (يستفيد من IPrintService) |
| 5 | Outstanding Balance Report | 1 يوم | 🟡 Medium | Slice 4 (نمط مكرر) |
| 6 | Phase 5 Integration + Test Coverage + Status Update | 1 يوم | 🟢 Low | كل ما سبق |

**الإجمالي:** 10 أيام — مطابق لتقدير V4 سطر 880.

---

### 🟦 Slice 1 — Attendance Foundation

**الهدف:** تسجيل `IAttendanceService` في DI، توسيع سطحها بـ 4 دوال، وبناء `AttendanceWindow` لتسجيل دخول/خروج الموظف وإدارة WorkShifts.

**Duration:** 2 يوم · **Risk:** 🔴 Critical · **Blocked by:** لا شيء

| # | الملف | الإجراء |
|---|---|---|
| 1.1 | `Services/Interfaces/IAttendanceService.cs` | **تعديل** — إضافة 4 دوال بعد السطر 26: `Task<List<Attendance>> GetAttendanceByDateRangeAsync(DateOnly from, DateOnly to, int? staffId)`، `Task<TimeSpan> GetTotalHoursWorkedAsync(int staffId, DateOnly from, DateOnly to)`، `Task<Attendance?> GetActiveAttendanceAsync(int staffId)`، `Task<List<WorkShift>> GetAllShiftsAsync()`، `Task<WorkShift> CreateShiftAsync(WorkShift shift)`، `Task UpdateShiftAsync(WorkShift shift)` |
| 1.2 | `Services/Implementations/AttendanceService.cs` | **تعديل** — تنفيذ الدوال الستة الجديدة بعد السطر 73. لا تَمَس الدوال الثلاث الحالية. استخدام `_context.Attendances` + `_context.WorkShifts` + LINQ aggregation للساعات |
| 1.3 | `App.xaml.cs` | **تعديل** — إضافة بعد السطر 156: `services.AddScoped<IAttendanceService, AttendanceService>();` |
| 1.4 | `ViewModels/Settings/AttendanceWindowViewModel.cs` | **إنشاء** — `ObservableCollection<AttendanceRowViewModel>` + `ObservableCollection<WorkShiftRowViewModel>` + `ClockInCommand` + `ClockOutCommand` + `RefreshCommand` + `LoadAttendanceAsync(DateOnly date)` + `SelectedShift` + `SelectedStaff`. يحقن `IAttendanceService`, `IAuthService` (لقائمة الموظفين), `ICurrentUserSession`, `IDialogService` |
| 1.5 | `ViewModels/Settings/AttendanceRowViewModel.cs` | **إنشاء** — يَلفّ `Attendance` ويَكشف: `StaffName`, `ShiftName`, `ClockIn`, `ClockOut?`, `LateMinutes`, `HoursWorked` computed |
| 1.6 | `ViewModels/Settings/WorkShiftRowViewModel.cs` | **إنشاء** — يَلفّ `WorkShift` للـ Master-Detail editing |
| 1.7 | `Views/Settings/AttendanceWindow.xaml` | **إنشاء** — TabControl: Tab1 "تسجيل اليوم" (DataGrid للحضور + أزرار Clock-In/Out)، Tab2 "WorkShifts" (Master-Detail). RTL FlowDirection. لا code-behind ما عدا `InitializeComponent()` |
| 1.8 | `Views/Settings/AttendanceWindow.xaml.cs` | **إنشاء** — code-behind نظيف MVVM (سطرَين فقط: ctor + InitializeComponent) |
| 1.9 | `App.xaml.cs` | **تعديل** — إضافة في `ConfigureServices`: `services.AddTransient<AttendanceWindowViewModel>();` و `services.AddTransient<AttendanceWindow>();`. إضافة في `OnStartup` بعد السطر 108: `navigation.RegisterWindow<AttendanceWindowViewModel, AttendanceWindow>();` |
| 1.10 | `FinalLabSystem.Tests/Services/AttendanceServiceTests.cs` | **إنشاء** — 10 اختبارات: ClockIn ينشئ سجل بساعة UTC و LateMinutes صحيح، ClockIn يَرفض ShiftId غير موجود، ClockOut يُحدِّث آخر سجل، ClockOut يَرفض إذا لا سجل مفتوح، GetAttendanceByDateRangeAsync يُرشِّح بصح، GetTotalHoursWorkedAsync يَحسب صح حتى مع ClockOut=null (يُهمل)، GetActiveAttendanceAsync يُرجِع آخر سجل مفتوح، CreateShiftAsync ينجح، UpdateShiftAsync ينجح، GetAllShiftsAsync يُرجِع IsActive=false |
| 1.11 | `FinalLabSystem.Tests/ViewModels/Settings/AttendanceWindowViewModelTests.cs` | **إنشاء** — 6 اختبارات: Load ينجح ويملأ القائمة، ClockInCommand يستدعي service مع SelectedShift.ShiftId، ClockOutCommand يستدعي service بـ CurrentStaffId، خطأ في الـ service يُعرَض dialog، CanExecute يَحجب أزراراً بناءً على SelectedStaff/SelectedShift، RefreshCommand يُعيد التحميل |
| 1.12 | `FinalLabSystem.Tests/Services/AttendanceServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات: `ServiceProvider.GetRequiredService<IAttendanceService>()` لا يَرمي، type = `AttendanceService` |

**Validation Gate G5.1:**
```
dotnet build → 0 errors, 0 warnings جديدة
dotnet test → 431 + 18 = 449 ✅
يدوي: فتح AttendanceWindow من قائمة (مؤقتاً عبر Test runner أو Settings) → DataGrid يَظهر + ClockIn يَنجح
```

**معيار الإكمال:** كل الاختبارات الـ 449 ناجحة + `AttendanceWindow` تُفتح بدون استثناء + IAttendanceService يُحقن في 3 أماكن (Window VM + 2 tests).

---

### 🟦 Slice 2 — Cash Drawer Service + Password + Window

**الهدف:** بناء `ICashDrawerService` لتجميع يومي بفلاتر، تنفيذ password protection (BR-050)، وبناء `CashDrawerWindow` + ربطه بـ `AccountsMenuViewModel`.

**Duration:** 3 يوم · **Risk:** 🔴 Critical · **Blocked by:** Slice 1 (لاتساق التسجيل فقط)

| # | الملف | الإجراء |
|---|---|---|
| 2.1 | `Models/DTOs/CashDrawerSummaryDto.cs` | **إنشاء** — `DateOnly Date`, `decimal TotalSubtotal`, `decimal TotalDiscount`, `decimal TotalCollected`, `decimal TotalOutstanding`, `decimal NetProfit`, `int VisitCount`, `List<CashDrawerFilterDto> AppliedFilters` |
| 2.2 | `Models/DTOs/CashDrawerFilterDto.cs` | **إنشاء** — `DateOnly From`, `DateOnly To`, `int? BranchId`, `int? DoctorId`, `int? UserId` |
| 2.3 | `Services/Interfaces/ICashDrawerService.cs` | **إنشاء** — `Task<CashDrawerSummaryDto> GetDailySummaryAsync(CashDrawerFilterDto filter)`، `Task<bool> UnlockAsync(string password)`، `Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)`، `Task<bool> IsPasswordChangeRequiredAsync()` |
| 2.4 | `Services/Implementations/CashDrawerService.cs` | **إنشاء** — يَحقن `FinalLabDbContext`, `ISettingsService`, `ILogger<CashDrawerService>`. منطق `GetDailySummary`: aggregation على `Visits` + `Payments` بـ `WHERE VisitDate BETWEEN From AND To` + filters. منطق password: SHA256/PBKDF2 hash، تخزين في `LabSetting` key=`CashDrawerPasswordHash`، flag key=`CashDrawerPasswordMustChange`. على أول استدعاء `UnlockAsync` إذا لا hash موجود → seed default `"123"` + MustChange=true. عند نجاح unlock مع MustChange=true → خاصية `RequiresPasswordChange` على الـ DTO المُرجَع |
| 2.5 | `App.xaml.cs` | **تعديل** — إضافة بعد السطر 171: `services.AddScoped<ICashDrawerService, CashDrawerService>();` |
| 2.6 | `Views/Settings/CashDrawerUnlockDialog.xaml` | **إنشاء** — Dialog بسيط: PasswordBox + زر "فتح" + زر "إلغاء". RTL |
| 2.7 | `Views/Settings/CashDrawerUnlockDialog.xaml.cs` | **إنشاء** — code-behind: خاصية `EnteredPassword` (PasswordBox لا يَدعم Two-Way Binding مباشرة — استثناء MVVM مقبول)، `DialogResult` |
| 2.8 | `Views/Settings/CashDrawerChangePasswordDialog.xaml` (+`.xaml.cs`) | **إنشاء** — Dialog بثلاثة PasswordBoxes (current + new + confirm) + validation |
| 2.9 | `ViewModels/Settings/CashDrawerWindowViewModel.cs` | **إنشاء** — `IsUnlocked` property, `UnlockCommand`, `RefreshCommand`, `ChangePasswordCommand`, `PrintSummaryCommand`. حقول filter: `DateFrom`, `DateTo`, `SelectedBranchId?`, `SelectedDoctorId?`, `SelectedUserId?`. `Summary` property = `CashDrawerSummaryDto`. يحقن `ICashDrawerService`, `IDialogService`, `IPrintService`, `INavigationService` |
| 2.10 | `Views/Settings/CashDrawerWindow.xaml` | **إنشاء** — على load يَطلب unlock عبر `CashDrawerUnlockDialog`. بعد unlock يَكشف: شريط فلاتر + جدول/Cards بالأرقام (Subtotal, Discount, Collected, Outstanding, Net Profit, Visit Count) + زر "طباعة ملخص" + زر "تغيير كلمة السر" |
| 2.11 | `Views/Settings/CashDrawerWindow.xaml.cs` | **إنشاء** — code-behind نظيف، `Loaded` event يستدعي `viewModel.RequestUnlockAsync()` |
| 2.12 | `Services/Printing/CashDrawerSummaryTemplate.cs` | **إنشاء** — يَرث `DocumentTemplateBase` (المُعرَّف في `Services/Printing/DocumentTemplateBase.cs`). يَبني `FlowDocument` بالـ summary |
| 2.13 | `App.xaml.cs` | **تعديل** — إضافة Transient: `CashDrawerWindowViewModel`, `CashDrawerWindow`, `CashDrawerUnlockDialog`, `CashDrawerChangePasswordDialog`. إضافة navigation: `navigation.RegisterWindow<CashDrawerWindowViewModel, CashDrawerWindow>();` |
| 2.14 | `ViewModels/Menu/AccountsMenuViewModel.cs` | **تعديل** — استبدال السطر 15 (`PlaceholderCommand = new RelayCommand(_ => { });`) بـ: `NavigateToCashDrawerCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<CashDrawerWindowViewModel>());`. إضافة property `public ICommand NavigateToCashDrawerCommand { get; }` |
| 2.15 | `MainWindow.xaml` | **تعديل** — الـ DataTemplate لـ `AccountsMenuViewModel` (سطور 81 – 93): استبدال محتوى `StackPanel` بـ `UniformGrid Columns="2"` يَحوي 4 أزرار: "الشركات" (NavigateToCompaniesCommand)، "التسعير" (NavigateToPricingCommand)، "الفواتير" (NavigateToInvoicesCommand)، "درج النقدية" (NavigateToCashDrawerCommand). إزالة TextBlock "سيتم تفعيل هذه الميزة في المرحلة 5" |
| 2.16 | `FinalLabSystem.Tests/Services/CashDrawerServiceTests.cs` | **إنشاء** — 12 اختبارات: GetDailySummaryAsync بدون filter ينجح، فلتر تاريخ يَعمل، فلتر userId يَعمل، Subtotal/Discount/Collected/Outstanding/NetProfit دقيق، VisitCount دقيق، UnlockAsync بكلمة صحيحة ينجح، خاطئة يَفشل، أول مرة يَستخدم default "123"، MustChange=true بعد seed، ChangePasswordAsync يُحدِّث ويُعطِّل MustChange، ChangePassword يَرفض إذا current خطأ، IsPasswordChangeRequiredAsync يُرجِع الحالة |
| 2.17 | `FinalLabSystem.Tests/ViewModels/Settings/CashDrawerWindowViewModelTests.cs` | **إنشاء** — 8 اختبارات: Unlock ناجح يُكشَف `IsUnlocked=true`، فاشل يُبقي false ويَعرض dialog، Refresh يحمِّل Summary، PrintSummary يستدعي IPrintService بنوع "CashDrawerSummary"، CanExecute يَحجب الأزرار حتى الفتح، ChangePassword يَفتح dialog، فلتر تاريخ يُعاد تحميل |
| 2.18 | `FinalLabSystem.Tests/Services/CashDrawerServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات: GetService<ICashDrawerService> لا يَرمي، النوع = `CashDrawerService` |
| 2.19 | `FinalLabSystem.Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs` | **تحديث (لا إنشاء — موجود فعلاً من Phase 4 — IMPLEMENTATION_STATUS سطر 520)** — إضافة 2 اختبار: `NavigateToCashDrawerCommand_Execute_OpensCashDrawerWindow` و `NavigateToCashDrawerCommand_IsNotNull` |

**Validation Gate G5.2:**
```
dotnet build → 0 errors
dotnet test → 449 + 24 = 473 ✅ (إن لزم إضافة testing فيُحسب من النموذج)
يدوي: AccountsMenuView → زر "درج النقدية" → unlock dialog يَظهر → "123" → window يَفتح → MustChange dialog → تغيير → نجاح
```

**معيار الإكمال:** Cash Drawer لا يُفتح بدون كلمة السر؛ أول unlock يُجبر تغيير "123"؛ Summary دقيق بفلاتر متعددة؛ زر طباعة يُنتج FlowDocument.

---

### 🟦 Slice 3 — Inventory (Migration + Service + Window + Dashboard Alert)

**الهدف:** إضافة حقول stock إلى `TubeMaterial`، بناء `IInventoryService`، شاشة `InventoryWindow`، و dashboard alert في `HomeMenuViewModel`.

**Duration:** 2 يوم · **Risk:** 🟠 High · **Blocked by:** لا شيء (يمكن التوازي مع Slice 2)

| # | الملف | الإجراء |
|---|---|---|
| 3.1 | `Models/TubeMaterial.cs` | **تعديل** — إضافة بعد السطر 21: `public int MinimumStock { get; set; }` و `public int CurrentStock { get; set; }` |
| 3.2 | `Data/FinalLabDbContext.cs` | **تعديل** — في كتلة Fluent `TubeMaterial` (سطور 1117 – 1127): إضافة قبل السطر 1126: `entity.Property(e => e.MinimumStock).HasColumnName("minimum_stock").HasDefaultValue(0);` و `entity.Property(e => e.CurrentStock).HasColumnName("current_stock").HasDefaultValue(0);` |
| 3.3 | `Migrations/` | **إنشاء** — `dotnet ef migrations add AddTubeMaterialStockFields`. التحقق من ملف الـ Up: `AddColumn<int>(name: "minimum_stock", table: "TubeMaterial", nullable: false, defaultValue: 0)` و نفس الشيء لـ `current_stock` |
| 3.4 | `Services/Interfaces/IInventoryService.cs` | **إنشاء** — `Task<List<TubeMaterial>> GetAllAsync()`، `Task<List<TubeMaterial>> GetLowStockItemsAsync()`، `Task<TubeMaterial> CreateAsync(TubeMaterial item)`، `Task UpdateAsync(TubeMaterial item)`، `Task AdjustStockAsync(int tubeMaterialId, int delta, int staffId, string? notes)`، `Task<int> GetLowStockCountAsync()` |
| 3.5 | `Services/Implementations/InventoryService.cs` | **إنشاء** — يَحقن `FinalLabDbContext`, `IAuditService` (المُسجَّل في `App.xaml.cs` سطر 158), `ILogger<InventoryService>`. منطق `AdjustStockAsync`: تحميل الـ entity، تعديل `CurrentStock`، حفظ. (الـ audit يُلتَقَط تلقائياً عبر `[Auditable]` — لكن `TubeMaterial` ليست `[Auditable]` حالياً، **يَلزم إضافة** `[Auditable]` على Models/TubeMaterial.cs السطر 5 إذا أُريد audit trail. إن لم تَكُن مطلوبة → لا تَمَس. القرار: نَتَركها بدون audit لأن V4 لا يَفرض ذلك) |
| 3.6 | `App.xaml.cs` | **تعديل** — إضافة بعد تسجيل CashDrawer: `services.AddScoped<IInventoryService, InventoryService>();` |
| 3.7 | `ViewModels/Settings/InventoryWindowViewModel.cs` | **إنشاء** — `ObservableCollection<InventoryRowViewModel>`، `RefreshCommand`, `SaveCommand`, `AddCommand`, `AdjustStockCommand`. حقن `IInventoryService`, `IDialogService`, `ICurrentUserSession` |
| 3.8 | `ViewModels/Settings/InventoryRowViewModel.cs` | **إنشاء** — يَلفّ `TubeMaterial` ويَكشف `IsLowStock` computed (CurrentStock < MinimumStock) |
| 3.9 | `Views/Settings/InventoryWindow.xaml` | **إنشاء** — DataGrid مع أعمدة: المادة (Ar/En)، اللون، الحد الأدنى، المخزون الحالي، حالة (تنبيه أحمر إذا low)، زر تعديل. زر "إضافة مادة جديدة" |
| 3.10 | `Views/Settings/InventoryWindow.xaml.cs` | **إنشاء** — code-behind نظيف |
| 3.11 | `Views/Settings/StockAdjustmentDialog.xaml` (+`.xaml.cs`) | **إنشاء** — Dialog: المادة (label)، الكمية الحالية (label)، Delta (TextBox مع علامة +/-)، ملاحظات، زر تأكيد |
| 3.12 | `ViewModels/Menu/HomeMenuViewModel.cs` | **تعديل** — إضافة dependency injection للـ constructor: `IInventoryService` (وحقن `INavigationService` للنقل). إضافة `LowStockCount` property + `LoadAlertsAsync()` async method تُستدعى بعد ctor. إضافة `OpenInventoryCommand`. (هذا يَكسر التوقيع الحالي للـ ctor — اختبار `HomeMenuViewModel` في Phase 2 (`PlaceholderMenusTests.cs`) قد يَفشل ويَحتاج تحديثاً) |
| 3.13 | `MainWindow.xaml` | **تعديل** — الـ DataTemplate لـ `HomeMenuViewModel` (سطر 12 +): إضافة Banner أعلى الشاشة `<Border Visibility="{Binding LowStockCount, Converter={StaticResource IntToVisibilityConverter}}">` يَعرض "⚠ مواد منخفضة المخزون: {LowStockCount} — انقر لعرض الجَرد" مع Button أو InputBinding يُنفذ `OpenInventoryCommand` |
| 3.14 | `Views/Converters.cs` | **تعديل (إذا لزم)** — التحقق من وجود `IntToVisibilityConverter`. إن لم يكن، إضافته كـ `IValueConverter` يُعيد `Visible` إذا `int > 0` |
| 3.15 | `ViewModels/Menu/AccountsMenuViewModel.cs` (أو إنشاء submenu جديد للـ Inventory) | **قرار:** الجَرد ليس "حسابات" بل "إدارة". `MainWindow.xaml` يَحتوي زر toolbar مستقل لـ "الجَرد ودرج الحساب" حسب V4 سطر 340. **التحقق:** `grep "الجرد\|InventoryMenu\|الجَرد" MainWindow.xaml` — في حالة عدم وجوده يَلزم إضافة DataTemplate جديد. **في هذا الـ Slice:** يُضاف زر تنقل من `HomeMenuViewModel` (Banner كما في 3.13) ومن `AccountsMenuViewModel` كـ زر إضافي خامس "الجَرد" |
| 3.16 | `App.xaml.cs` | **تعديل** — إضافة Transient: `InventoryWindowViewModel`, `InventoryWindow`, `StockAdjustmentDialog`. إضافة navigation: `navigation.RegisterWindow<InventoryWindowViewModel, InventoryWindow>();` |
| 3.17 | `FinalLabSystem.Tests/Services/InventoryServiceTests.cs` | **إنشاء** — 10 اختبارات: GetAllAsync يُرجِع كل المواد، GetLowStockItemsAsync يُرجِع فقط Current < Minimum، GetLowStockCountAsync دقيق، CreateAsync يُضيف، UpdateAsync يُعدِّل، AdjustStockAsync بـ delta=+5 يُزيد، delta=-3 يُنقِص، يَرفض إذا CurrentStock+delta < 0، يَرفض إذا TubeMaterialId مفقود، يَكتب log |
| 3.18 | `FinalLabSystem.Tests/ViewModels/Settings/InventoryWindowViewModelTests.cs` | **إنشاء** — 6 اختبارات: Refresh يحمِّل، Save يستدعي UpdateAsync لكل dirty row، Add يَفتح dialog، AdjustStockCommand يستدعي service بـ correct delta، IsLowStock computed صحيح، خطأ في service يَعرض dialog |
| 3.19 | `FinalLabSystem.Tests/ViewModels/Menu/HomeMenuViewModelLowStockTests.cs` | **إنشاء** — 4 اختبارات: LowStockCount=0 يُخفي banner، >0 يُظهره، OpenInventoryCommand يَفتح InventoryWindow، LoadAlerts يَرمي → يُسجَّل لكن لا يَكسر |
| 3.20 | `FinalLabSystem.Tests/ViewModels/Menu/PlaceholderMenusTests.cs` | **تعديل (موجود من Phase 2/4 — IMPLEMENTATION_STATUS سطر 529)** — تحديث constructor signature لـ HomeMenuViewModel ليَقبل `IInventoryService` و `INavigationService` Mock |
| 3.21 | `FinalLabSystem.Tests/Validation/TubeMaterialStockMigrationTests.cs` | **إنشاء** — 3 اختبارات: إنشاء TubeMaterial بدون stock فيُستخدم default 0، MinimumStock < 0 يَرفع validation، CurrentStock يُحفَظ صحيح |
| 3.22 | `FinalLabSystem.Tests/Services/InventoryServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات DI |

**Validation Gate G5.3:**
```
dotnet ef migrations add AddTubeMaterialStockFields
dotnet ef database update (يَدوي على بيئة dev)
dotnet build → 0 errors
dotnet test → 473 + 25 = 498 ✅
يدوي: زر "الجَرد" → InventoryWindow → AdjustStock → CurrentStock يَنخفض → الـ HomeMenu Banner يَظهر
```

**معيار الإكمال:** Migration مُولَّدة وتُنفَّذ بدون خطأ؛ low-stock banner يَظهر/يَختفي بناءً على البيانات الفعلية؛ AdjustStock يُحدِّث القاعدة فوراً.

---

### 🟦 Slice 4 — Referral Commission Report

**الهدف:** عرض `VReferralCommissionReport` view في UI مع filters وطباعة.

**Duration:** 1 يوم · **Risk:** 🟡 Medium · **Blocked by:** Slice 2 (لإعادة استخدام نمط Print)

| # | الملف | الإجراء |
|---|---|---|
| 4.1 | `Services/Interfaces/ICommissionReportService.cs` | **إنشاء** — `Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateOnly from, DateOnly to, int? referralId, string? sourceType)` |
| 4.2 | `Services/Implementations/CommissionReportService.cs` | **إنشاء** — يَحقن `FinalLabDbContext`. يستعلم `_context.VReferralCommissionReports.Where(...)` مع الفلاتر |
| 4.3 | `App.xaml.cs` | **تعديل** — إضافة `services.AddScoped<ICommissionReportService, CommissionReportService>();` |
| 4.4 | `Services/Printing/CommissionReportTemplate.cs` | **إنشاء** — يَرث `DocumentTemplateBase`. يَبني FlowDocument لجدول العمولات |
| 4.5 | `ViewModels/Settings/CommissionReportWindowViewModel.cs` | **إنشاء** — Filters (DateFrom, DateTo, ReferralId, SourceType)، `ObservableCollection<VReferralCommissionReport>`, `RefreshCommand`, `PrintCommand`, `TotalCommissionDue` computed. حقن `ICommissionReportService`, `IPrintService`, `IDialogService` |
| 4.6 | `Views/Settings/CommissionReportWindow.xaml` (+`.xaml.cs`) | **إنشاء** — شريط فلاتر + DataGrid + Summary footer (إجمالي العمولات) + زر طباعة. RTL |
| 4.7 | `App.xaml.cs` | **تعديل** — إضافة Transient + Navigation |
| 4.8 | `ViewModels/Menu/AccountsMenuViewModel.cs` | **تعديل** — إضافة زر سادس "تقرير العمولات" + Command + Navigation. (يُمكن أيضاً وضعه في `ResultsMenuViewModel` لكن AccountsMenu أنسب دلالياً) |
| 4.9 | `MainWindow.xaml` | **تعديل** — تحديث الـ DataTemplate لـ `AccountsMenuViewModel` (إعادة قياس UniformGrid لـ Columns="3" مع 6 أزرار) |
| 4.10 | `FinalLabSystem.Tests/Services/CommissionReportServiceTests.cs` | **إنشاء** — 6 اختبارات: GetCommissionsAsync بدون فلاتر، فلتر تاريخ، فلتر referralId، فلتر sourceType، نتيجة فارغة، CommissionDue=null لا يَكسر |
| 4.11 | `FinalLabSystem.Tests/ViewModels/Settings/CommissionReportWindowViewModelTests.cs` | **إنشاء** — 5 اختبارات: Refresh يحمِّل، TotalCommissionDue يُحسَب من القائمة، PrintCommand يستدعي IPrintService، CanExecute (Print لا يَعمل بدون بيانات)، فلتر تاريخ يُعيد التحميل |
| 4.12 | `FinalLabSystem.Tests/Services/CommissionReportServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات DI |

**Validation Gate G5.4:**
```
dotnet build → 0 errors
dotnet test → 498 + 13 = 511 ✅
يدوي: AccountsMenu → "تقرير العمولات" → DataGrid يَظهر → فلتر تاريخ → Print
```

**معيار الإكمال:** التقرير يُعرَض بفلاتر صحيحة + إجمالي يُحسَب + الطباعة تُنتج FlowDocument.

---

### 🟦 Slice 5 — Outstanding Balance Report

**الهدف:** نفس نمط Slice 4 لكن لـ `VOutstandingBalance`.

**Duration:** 1 يوم · **Risk:** 🟡 Medium · **Blocked by:** Slice 4 (نمط متطابق)

| # | الملف | الإجراء |
|---|---|---|
| 5.1 | `Services/Interfaces/IOutstandingBalanceReportService.cs` | **إنشاء** — `Task<List<VOutstandingBalance>> GetOutstandingAsync(DateOnly? from, DateOnly? to, int? companyId, int? minDaysOverdue)` |
| 5.2 | `Services/Implementations/OutstandingBalanceReportService.cs` | **إنشاء** — استعلام على `_context.VOutstandingBalances` |
| 5.3 | `App.xaml.cs` | **تعديل** — `services.AddScoped<IOutstandingBalanceReportService, OutstandingBalanceReportService>();` |
| 5.4 | `Services/Printing/OutstandingBalanceReportTemplate.cs` | **إنشاء** — قالب FlowDocument |
| 5.5 | `ViewModels/Settings/OutstandingBalanceReportWindowViewModel.cs` | **إنشاء** — Filters + ObservableCollection + RefreshCommand + PrintCommand + `TotalOutstanding` computed + `MinDaysOverdue` filter |
| 5.6 | `Views/Settings/OutstandingBalanceReportWindow.xaml` (+`.xaml.cs`) | **إنشاء** — DataGrid + ملوَّن للسجلات التي DaysOverdue > 30 |
| 5.7 | `App.xaml.cs` | **تعديل** — Transient + Navigation |
| 5.8 | `ViewModels/Menu/AccountsMenuViewModel.cs` | **تعديل** — إضافة زر سابع "الأرصدة المعلَّقة" + Command |
| 5.9 | `MainWindow.xaml` | **تعديل** — تحديث DataTemplate لـ AccountsMenuViewModel (Columns="4" لـ 8 أزرار أو إعادة هندسة بـ Wrap) |
| 5.10 | `FinalLabSystem.Tests/Services/OutstandingBalanceReportServiceTests.cs` | **إنشاء** — 5 اختبارات: فلاتر متعددة، NULL DaysOverdue لا يَكسر، companyId يَفلتر |
| 5.11 | `FinalLabSystem.Tests/ViewModels/Settings/OutstandingBalanceReportWindowViewModelTests.cs` | **إنشاء** — 5 اختبارات: تحميل، TotalOutstanding، Print، فلاتر |
| 5.12 | `FinalLabSystem.Tests/Services/OutstandingBalanceReportServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات DI |

**Validation Gate G5.5:**
```
dotnet build → 0 errors
dotnet test → 511 + 12 = 523 ✅
يدوي: AccountsMenu → "الأرصدة المعلَّقة" → DataGrid → Print
```

**معيار الإكمال:** التقرير يَعمل + الفلترة + الطباعة.

---

### 🟦 Slice 6 — Integration + Test Coverage + Status Update

**الهدف:** اختبارات تكامل end-to-end، تنظيف، تحديث `IMPLEMENTATION_STATUS.md`.

**Duration:** 1 يوم · **Risk:** 🟢 Low · **Blocked by:** Slices 1 – 5

| # | الملف | الإجراء |
|---|---|---|
| 6.1 | `FinalLabSystem.Tests/Integration/AttendanceWorkflowEndToEndTests.cs` | **إنشاء** — 4 اختبارات: Clock-In → Clock-Out → GetTotalHoursWorked يَكون دقيق، WorkShift CRUD نهاية-لنهاية |
| 6.2 | `FinalLabSystem.Tests/Integration/CashDrawerEndToEndTests.cs` | **إنشاء** — 5 اختبارات: Visit + Payment → Summary يَعكس التغيير، Filter متعدد، Password change end-to-end |
| 6.3 | `FinalLabSystem.Tests/Integration/InventoryAlertEndToEndTests.cs` | **إنشاء** — 4 اختبارات: AdjustStock يُخفِّض → GetLowStockCount يُحدَّث → HomeMenu banner يَظهر، AdjustStock يَرفع → banner يَختفي |
| 6.4 | `FinalLabSystem.Tests/Integration/Phase5BuildVerificationTests.cs` | **إنشاء** — 3 اختبارات: كل خدمات Phase 5 الخمس مسجَّلة في DI، كل Windows الخمس تُحَل من ServiceProvider بدون استثناء، كل Migrations مُطبَّقة |
| 6.5 | `Docs/PRDs/IMPLEMENTATION_STATUS.md` | **تعديل** — إضافة قسم كامل لـ Phase 5 على نفس نمط Phase 4 (سطر 415+): الإحصائيات، الـ Slices، الـ Migration، Tech Debt إن وجد. تحديث السطر 578 من "⏳ في الانتظار" إلى "✅ مكتملة" مع التاريخ والإحصائيات |
| 6.6 | (يدوي) Smoke test كامل | تشغيل التطبيق → دخول → فتح كل شاشة Phase 5 → تنفيذ سيناريو لكل واحدة |

**Validation Gate G5.6 (نهائي):**
```
dotnet build → 0 errors, 0 warnings جديدة
dotnet test → 523 + 16 = 539 ✅ كل الاختبارات
كل اختبارات Phase 1-4 (431) لا تزال تنجح
كل migrations مُطبَّقة (آخر واحدة AddTubeMaterialStockFields)
يدوي: 5/5 Windows تُفتح وتعمل
IMPLEMENTATION_STATUS.md محدَّث
```

**معيار الإكمال:** كل ما سبق محقَّق + Smoke test يَنجح + الـ status file يَعكس الحقيقة.

---

## الجزء الثالث — ملخص الأثر الكامل

### جدول الملفات للتعديل

| الملف | التغيير المطلوب |
|---|---|
| `Services/Interfaces/IAttendanceService.cs` | إضافة 6 دوال (لا حذف للحالية) |
| `Services/Implementations/AttendanceService.cs` | تنفيذ 6 دوال إضافية |
| `Models/TubeMaterial.cs` | إضافة `MinimumStock` و `CurrentStock` |
| `Data/FinalLabDbContext.cs` | إضافة Fluent mapping للحقلين في كتلة `TubeMaterial` (سطور 1117 – 1127) |
| `ViewModels/Menu/AccountsMenuViewModel.cs` | إضافة 4 أوامر (CashDrawer, Commission, Outstanding, Inventory shortcut اختياري) واستبدال `PlaceholderCommand` |
| `ViewModels/Menu/HomeMenuViewModel.cs` | حقن `IInventoryService` + `INavigationService` + `LowStockCount` property + `OpenInventoryCommand` |
| `MainWindow.xaml` | استبدال DataTemplate لـ `AccountsMenuViewModel` (سطور 81 – 93) بـ UniformGrid حقيقي + تحديث DataTemplate لـ `HomeMenuViewModel` لإضافة low-stock banner |
| `Views/Converters.cs` (إن لزم) | إضافة `IntToVisibilityConverter` إذا غير موجود |
| `App.xaml.cs` | 5 DI تسجيلات Scoped جديدة + 10+ تسجيلات Transient (VMs + Windows + Dialogs) + 5 navigation registrations |
| `Docs/PRDs/IMPLEMENTATION_STATUS.md` | إضافة قسم Phase 5 كامل + تحديث Status |
| `FinalLabSystem.Tests/ViewModels/Menu/PlaceholderMenusTests.cs` | تحديث constructor mocks لـ `HomeMenuViewModel` |
| `FinalLabSystem.Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs` | إضافة 2 اختبارَين على الأقل لـ `NavigateToCashDrawerCommand` |

### جدول الملفات للإنشاء

| الفئة | الملفات الجديدة |
|---|---|
| **Services Interfaces** (4) | `ICashDrawerService.cs`, `IInventoryService.cs`, `ICommissionReportService.cs`, `IOutstandingBalanceReportService.cs` |
| **Services Implementations** (4) | `CashDrawerService.cs`, `InventoryService.cs`, `CommissionReportService.cs`, `OutstandingBalanceReportService.cs` |
| **Print Templates** (3) | `CashDrawerSummaryTemplate.cs`, `CommissionReportTemplate.cs`, `OutstandingBalanceReportTemplate.cs` |
| **DTOs** (2) | `CashDrawerSummaryDto.cs`, `CashDrawerFilterDto.cs` |
| **ViewModels** (10) | `AttendanceWindowViewModel.cs`, `AttendanceRowViewModel.cs`, `WorkShiftRowViewModel.cs`, `CashDrawerWindowViewModel.cs`, `InventoryWindowViewModel.cs`, `InventoryRowViewModel.cs`, `CommissionReportWindowViewModel.cs`, `OutstandingBalanceReportWindowViewModel.cs` |
| **Views/Windows** (5) | `AttendanceWindow.xaml(.cs)`, `CashDrawerWindow.xaml(.cs)`, `InventoryWindow.xaml(.cs)`, `CommissionReportWindow.xaml(.cs)`, `OutstandingBalanceReportWindow.xaml(.cs)` |
| **Views/Dialogs** (3) | `CashDrawerUnlockDialog.xaml(.cs)`, `CashDrawerChangePasswordDialog.xaml(.cs)`, `StockAdjustmentDialog.xaml(.cs)` |
| **Migrations** (1) | `xxxxxxxxxxxxx_AddTubeMaterialStockFields.cs` (+ `.Designer.cs` + تحديث `FinalLabDbContextModelSnapshot.cs`) |
| **Tests** (17 ملف) | تفصيل في جدول الاختبارات أدناه |

### جدول الـ Migrations المطلوبة

| اسم Migration | ما تفعله |
|---|---|
| `AddTubeMaterialStockFields` | يضيف عمود `minimum_stock` (INT NOT NULL DEFAULT 0) و عمود `current_stock` (INT NOT NULL DEFAULT 0) إلى جدول `TubeMaterial`، ويُحدِّث `FinalLabDbContextModelSnapshot.cs` |

### جدول الاختبارات المطلوبة

| ملف الاختبار | ما يختبره | العدد المتوقع |
|---|---|---|
| `Tests/Services/AttendanceServiceTests.cs` | CRUD + ClockIn/Out + HoursWorked | 10 |
| `Tests/ViewModels/Settings/AttendanceWindowViewModelTests.cs` | Commands + Load + Error handling | 6 |
| `Tests/Services/AttendanceServiceRegistrationTests.cs` | DI resolution | 2 |
| `Tests/Services/CashDrawerServiceTests.cs` | Summary + Password + Unlock | 12 |
| `Tests/ViewModels/Settings/CashDrawerWindowViewModelTests.cs` | Unlock + Refresh + Print | 8 |
| `Tests/Services/CashDrawerServiceRegistrationTests.cs` | DI resolution | 2 |
| `Tests/Services/InventoryServiceTests.cs` | CRUD + AdjustStock + LowStock | 10 |
| `Tests/ViewModels/Settings/InventoryWindowViewModelTests.cs` | Load + Save + Adjust | 6 |
| `Tests/ViewModels/Menu/HomeMenuViewModelLowStockTests.cs` | Alert visibility + navigation | 4 |
| `Tests/Validation/TubeMaterialStockMigrationTests.cs` | Migration applied + defaults | 3 |
| `Tests/Services/InventoryServiceRegistrationTests.cs` | DI resolution | 2 |
| `Tests/Services/CommissionReportServiceTests.cs` | Filters + Empty + Null safety | 6 |
| `Tests/ViewModels/Settings/CommissionReportWindowViewModelTests.cs` | Load + Total + Print | 5 |
| `Tests/Services/CommissionReportServiceRegistrationTests.cs` | DI resolution | 2 |
| `Tests/Services/OutstandingBalanceReportServiceTests.cs` | Filters + Null safety | 5 |
| `Tests/ViewModels/Settings/OutstandingBalanceReportWindowViewModelTests.cs` | Load + Total + Print | 5 |
| `Tests/Services/OutstandingBalanceReportServiceRegistrationTests.cs` | DI resolution | 2 |
| `Tests/Integration/AttendanceWorkflowEndToEndTests.cs` | E2E Clock-In→Out flow | 4 |
| `Tests/Integration/CashDrawerEndToEndTests.cs` | E2E Visit→Payment→Summary | 5 |
| `Tests/Integration/InventoryAlertEndToEndTests.cs` | E2E Adjust→Banner | 4 |
| `Tests/Integration/Phase5BuildVerificationTests.cs` | DI sanity check شامل | 3 |
| **+ تعديلات** | `AccountsMenuViewModelTests` (+2)، `PlaceholderMenusTests` (تحديث لا إضافة) | +2 |
| **إجمالي اختبارات Phase 5 الجديدة** | | **108** |
| **الإجمالي بعد Phase 5** | **431 + 108** | **= 539** |

---

## الجزء الرابع — المخاطر المعمارية

| الخطر | الخطورة | استراتيجية التخفيف |
|---|---|---|
| **R1** — حقول Stock على `TubeMaterial` تَنحرف عن صياغة V4 سطر 873 ("على `SampleTube`") | 🟠 High | توثيق القرار **D5.2** صراحةً مع تبرير دلالي + طلب تأكيد مالك المشروع قبل تنفيذ Migration |
| **R2** — تعديل `HomeMenuViewModel` ctor يَكسر اختبار `PlaceholderMenusTests` الموجود من Phase 2 | 🟠 High | تحديث الاختبار في نفس Slice 3 (خطوة 3.20) — لا يُؤجَّل |
| **R3** — `PasswordBox` لا يَدعم Two-Way Binding بـ MVVM نقي | 🟡 Medium | استثناء MVVM موضعي مقبول داخل `CashDrawerUnlockDialog.xaml.cs` فقط — مع تعليق توضيحي وعدم نشر النمط لباقي النوافذ |
| **R4** — Cash Drawer password default `"123"` خطر أمني إذا لم تُغيَّر | 🔴 Critical | إجبار التغيير عند أول فتح عبر `IsPasswordChangeRequiredAsync` (BR-050) + اختبار يَفشل إذا الـ flag غير مُهيَّأ |
| **R5** — DataTemplate لـ `AccountsMenuViewModel` في MainWindow.xaml سيَحوي 7+ أزرار يُسبِّب ازدحاماً بصرياً | 🟡 Medium | استخدام `WrapPanel` أو `UniformGrid Columns="3" Rows="3"` بدلاً من قائمة طويلة — تصميم متجاوب |
| **R6** — `IFinancialService` و `ICashDrawerService` قد يَتداخلان في تجميعات الـ Payment | 🟡 Medium | فصل صريح في D5.1: Financial = per-visit، CashDrawer = aggregated. لا اشتراك حالة |
| **R7** — Migration `AddTubeMaterialStockFields` تُغيِّر schema موجودة في production | 🟠 High | الـ DEFAULT 0 يَضمن backward compatibility — لا NULL، لا fail. اختبار `TubeMaterialStockMigrationTests` يَتحقَّق |
| **R8** — `HomeMenuViewModel.LoadAlertsAsync` يُستدعى async في ctor → race condition | 🟡 Medium | استخدام `Loaded` event في الـ Window أو `OnNavigatedTo` بدلاً من ctor — لا async في ctor |
| **R9** — `IAuthService` قد لا يَكشف دالة hash عامة لإعادة استخدامها في Cash Drawer | 🟡 Medium | إذا الفحص أظهر أنها مغلَّفة → استخدام `System.Security.Cryptography.Rfc2898DeriveBytes` (PBKDF2) مباشرة داخل `CashDrawerService` (مكتبة BCL لا dependency خارجية) |
| **R10** — التقارير الطباعية الجديدة تَتطلَّب توسعة `WpfFlowDocumentPrintService` لقبول أنواع جديدة | 🟡 Medium | فحص `Services/Implementations/WpfFlowDocumentPrintService.cs` لرؤية كيف يُسجَّل document type. إن كان pluggable عبر `IResultEditorFactory`-like → يَكفي إضافة template جديد. إن كان hard-coded → يَستلزم Slice 2/4/5 تعديلاً صغيراً |
| **R11** — اختبارات Phase 4 (431) قد تَنكسر بسبب تغيير `AccountsMenuViewModel` ctor (إضافة أوامر جديدة) | 🟠 High | إضافة فقط — لا تَغيير لتوقيع ctor الحالي (يَقبل `INavigationService` كما هو). الاختبارات الـ 4 الموجودة في `AccountsMenuViewModelTests` تَبقى ناجحة |
| **R12** — تكامل المُحَوِّل `IntToVisibilityConverter` قد يَكون موجوداً بأسم آخر | 🟡 Medium | فحص `Views/Converters.cs` كاملاً قبل الإنشاء — إعادة استخدام إن أمكن |

---

## الجزء الخامس — معايير إكمال Phase 5

> Phase 5 **لا تُعلَن مكتملة** إلا عند تحقُّق كل البنود التالية بدون استثناء:

### بنود الإكمال الإلزامية

☐ **Build** — `dotnet build` يُرجِع `0 errors, 0 warnings جديدة`
☐ **Tests** — `dotnet test` يُرجِع `539 / 539 ✅` (431 + 108 جديدة)
☐ **Phase 1-4 Tests** — كل الاختبارات الـ 431 من Phase 1 – 4 لا تزال ناجحة (regression test)
☐ **DI Registration**:
  ☐ `IAttendanceService` ↔ `AttendanceService` (Scoped)
  ☐ `ICashDrawerService` ↔ `CashDrawerService` (Scoped)
  ☐ `IInventoryService` ↔ `InventoryService` (Scoped)
  ☐ `ICommissionReportService` ↔ `CommissionReportService` (Scoped)
  ☐ `IOutstandingBalanceReportService` ↔ `OutstandingBalanceReportService` (Scoped)
  ☐ `AttendanceWindowViewModel` + `AttendanceWindow` (Transient)
  ☐ `CashDrawerWindowViewModel` + `CashDrawerWindow` (Transient)
  ☐ `InventoryWindowViewModel` + `InventoryWindow` (Transient)
  ☐ `CommissionReportWindowViewModel` + `CommissionReportWindow` (Transient)
  ☐ `OutstandingBalanceReportWindowViewModel` + `OutstandingBalanceReportWindow` (Transient)
  ☐ كل الـ Dialogs (`CashDrawerUnlockDialog`, `CashDrawerChangePasswordDialog`, `StockAdjustmentDialog`) — Transient
  ☐ 5 navigation registrations في `OnStartup` (`RegisterWindow<TVM, TWindow>()`)

☐ **Migrations**:
  ☐ `AddTubeMaterialStockFields` مُولَّدة بنجاح
  ☐ `dotnet ef database update` ينجح بدون خطأ
  ☐ `FinalLabDbContextModelSnapshot.cs` محدَّث

☐ **Functional Verification**:
  ☐ `AttendanceWindow` تُفتح + ClockIn/Out يَعمل + WorkShifts CRUD يَعمل
  ☐ `CashDrawerWindow` تَطلب password عند الفتح
  ☐ Default password `"123"` يَعمل أول مرة + يُجبر تغيير
  ☐ ChangePassword dialog يَعمل
  ☐ Cash Drawer Summary دقيق رياضياً (Subtotal - Discount = TotalAfterDiscount; TotalAfterDiscount - TotalPaid = Outstanding)
  ☐ Cash Drawer Print يُنتج FlowDocument
  ☐ `InventoryWindow` تُفتح + AdjustStock يَعمل + LowStock بادج يَظهر
  ☐ `HomeMenuViewModel` يَعرض banner للـ low stock فقط عند وجود مواد منخفضة
  ☐ Banner ينتقل إلى `InventoryWindow` عند النقر
  ☐ `CommissionReportWindow` يَعرض البيانات من `VReferralCommissionReport`
  ☐ Commission filters تَعمل (date, referralId, sourceType)
  ☐ Commission Print يَعمل
  ☐ `OutstandingBalanceReportWindow` يَعرض البيانات من `VOutstandingBalance`
  ☐ Outstanding filters تَعمل + Print

☐ **Menu Wiring**:
  ☐ `AccountsMenuViewModel` يَحوي 4 أوامر فعلية إضافية (CashDrawer, Commission, Outstanding، + Inventory إن أُختير)
  ☐ `MainWindow.xaml` DataTemplate لـ `AccountsMenuViewModel` يَعرض الأزرار (لا "سيتم تفعيل هذه الميزة في المرحلة 5")
  ☐ `MainWindow.xaml` DataTemplate لـ `HomeMenuViewModel` يَحوي low-stock banner

☐ **Business Rules**:
  ☐ BR-050 محقَّق (Cash Drawer password protection + force change)
  ☐ BR-051 محقَّق (Attendance يَتطلَّب ShiftId)
  ☐ BR-052 محقَّق (low-stock alert يَظهر في dashboard)

☐ **Documentation**:
  ☐ `Docs/PRDs/IMPLEMENTATION_STATUS.md` يَحوي قسم Phase 5 كامل على نمط Phase 4
  ☐ السطر 578 في الـ status file محدَّث من "⏳ في الانتظار" إلى "✅ مكتملة" مع التاريخ
  ☐ الإحصائيات في الـ status (files created/modified, tests, build status) دقيقة
  ☐ Tech Debt من Phase 5 (إن وُجد) موثَّق

☐ **No Tech Debt Leakage**:
  ☐ لا warning compiler جديد
  ☐ لا nullable warning
  ☐ كل التسجيلات DI الجديدة لها unit test يَتحقَّق من resolution
  ☐ كل ctor للـ VMs الجديدة لا يَحوي async work (يُؤجَّل إلى Loaded event)

---

## ملاحظة إغلاق

كل ادّعاء تقني في هذا المستند مدعوم باسم ملف ورقم سطر من checkout الفعلي للـ commit `bae71cb` (branch `before-prd`، رسالة "بعد تنفيذ المرحلة الرابعة"). الخطة لا تُقترح تقنيات خارج المُقفَّلة (.NET 8 / WPF / EF Core / SQL Server / MVVM). تنفيذ الخطوة 6.6 الأخيرة في Slice 6 = إكمال Phase 5 بالكامل.

— **انتهت الخطة. في الانتظار.**

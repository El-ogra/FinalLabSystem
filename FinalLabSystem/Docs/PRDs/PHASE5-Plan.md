# PHASE 5 — خطة العمل التنفيذية المُحدَّثة
# Inventory & Cash Drawer

> **المرجع الأعلى:** `system-alignment-master-prd-v4.md` (سطور 859 – 880)
> **حالة البداية:** نهاية Phase 4 — 434 اختبار ناجح
> **آخر Migration مُطبَّقة:** `20260626032417_AddCompanyContractFields` (Phase 4 — Slice 1)

---

## قواعد التنفيذ الإلزامية

### قواعد قوالب WPF Table:
- استخدم `new TableColumn { Width = new GridLength(...) }` — لا تستخدم المُنشئ بمعاملات
- استخدم `TableRowGroup()` ثم `Rows.Add()` — لا تستخدم مُنشئ بمعاملات
- استخدم `new TableRow()` ثم `Cells.Add()` — لا تستخدم مُنشئ بمعاملات

### قواعد الاختبارات:
- استخدم `It.IsAny<T>()` لأي معامل اختياري في Moq Verify/Setup
- استخدم `DateTime.UtcNow` وليس `DateTime.Today` في اختبارات DB
- يجب أن تحتوي Patient seed data على `Sex = "M"` إلزامياً
- تطابق عدد المتغيرات في tuple deconstruction مع عدد العناصر الفعلي
- كل دالة/خدمة مطلوبة من الاختبارات يجب أن تكون `public`

### قواعد App.xaml.cs:
- كل خدمة + ViewModel + Window + Navigation يجب تسجيلها ضمن نفس الـ Slice

---

## القرارات المعمارية المُنقَّحة (بناءً على اتفاق مالك المشروع)

| # | القرار | السبب | التغيير عن الخطة الأصلية |
|---|---|---|---|
| **D5.1** | فصل `ICashDrawerService` عن `IFinancialService` | Single Responsibility — درج النقدية aggregation عَمَل مختلف عن الدفع/الخصم | ✅ كما هي |
| **D5.2** | حقول Stock تُضاف إلى `TubeMaterial` لا `SampleTube` | `SampleTube` = أنبوب فردي لزيارة واحدة — لا معنى لتتبع مخزونه. `TubeMaterial` = نوع الأنبوب — هذا ما يحتاج تتبعه صاحب المختبر | ✅ **متفق عليها** — مالك المشروع أكّد: يتتبع مخزون كل نوع بشكل مستقل |
| **D5.3** | كلمة مرور درج النقدية تُدار بنفس نظام كلمة مرور المدير الأول | المستخدم يُعيّن كلمة المرور من البداية — لا توجد كلمة افتراضية "123" | 🔄 **مُنقَّحة** — لا إجبار تغيير، المستخدم يختار من البداية |
| **D5.4** | `IAttendanceService` تُوسَّع بـ 6 دوال إضافية | الحفاظ على backward compatibility | ✅ كما هي |
| **D5.5** | كل تقرير يستخدم `IPrintService` الموجود | إعادة استخدام | ✅ كما هي |
| **D5.6** | Dashboard alert للـ low stock في `HomeMenuViewModel` | البساطة | ✅ كما هي |
| **D5.7** | كلمة المرور تُجزَأ بـ `PasswordHasher` (PBKDF2 + SHA-256) | موجود بالفعل في `Infrastructure/Security/PasswordHasher.cs` — أداة ثابتة بدون اعتمادات | ✅ متفق عليها |
| **D5.8** | كل خدمة/VM/Window جديدة تُسجَّل في `App.xaml.cs` ضمن نفس الـ Slice | منع كسر البناء | ✅ كما هي |
| **D5.9** | **تنبيه المخزون عند طباعة ملصق الباركود** | المستخدم يُريد تنبيهًا عند نقص المخزون أثناء طباعة الملصقات | 🔄 **جديد** — إضافة ميزة غير موجودة في الخطة الأصلية |

---

## ملخص التنقيحات الرئيسية عن الخطة الأصلية

### 1. نظام كلمة مرور درج النقدية — مُبسَّط

**الخطة الأصلية:** كلمة `"123"` افتراضية → إجبار التغيير
**الخطة المُنقَّحة:** لا توجد كلمة افتراضية → المستخدم يُعيّنها من البداية

**التدفق الجديد:**
```
فتح درج النقدية → هل توجد كلمة مرور محفوظة؟
  ├── لا → رسالة: "لم تُعد كلمة مرور لدرج النقدية. يُرجى الإعداد من الإعدادات"
  └── نعم → طلب كلمة المرور → التحقق → فتح الدرج
```

**المكونات الموجودة ولا نحتاج صنعها:**
- `PasswordHasher.Hash(password)` — للتشفير
- `PasswordHasher.Verify(password, storedHash)` — للتحقق
- `LabSetting` — للتخزين (المفتاح: `CashDrawer.PasswordHash`)
- `ISettingsService.UpsertSettingAsync()` — للحفظ

### 2. تنبيه المخزون عند طباعة الباركود — ميزة جديدة

**المشهد:**
```
المستخدم يضغط "طباعة ملصق الباركود"
  → النظام يفحص مخزون نوع الأنبوب المطلوب
  ├── إذا CurrentStock > MinimumStock → الطباعة تتم بشكل طبيعي
  └── إذا CurrentStock <= MinimumStock → تنبيه: "مخزون [نوع الأنبوب] منخفض — يُرجى الشراء"
```

**أين يُضاف التنبيه:**
- الملف: `ViewModels/Patients/BarcodeDialogViewModel.cs`
- الدالتان: `PrintLabelAsync()` و `PrintAllAsync()`
- يُحقن `IInventoryService` + `IDialogService`
- استخدام نمط `IDialogService.ShowWarning()` الموجود بالفعل

### 3. عدد الاختبارات المُنقَّح

| البند | الخطة الأصلية | الخطة المُنقَّحة |
|---|---|---|
| اختبارات Phase 5 الجديدة | 108 | **105** |
| المجموع النهائي | 539 | **539** |

---

## نظرة عامة على الـ Slices

| # | الـ Slice | مدة | خطورة | Blocked by |
|---|---|---|---|---|
| 1 | Attendance Foundation (Service + DI + Window) | 2 يوم | 🔴 Critical | لا شيء |
| 2 | Cash Drawer (مع كلمة المرور الجديدة) | 3 يوم | 🔴 Critical | Slice 1 (لاتساق التسجيل فقط) |
| 3 | Inventory + Low-Stock Alert + Barcode Warning | 2 يوم | 🟠 High | لا شيء (يمكن التوازي مع Slice 2) |
| 4 | Referral Commission Report | 1 يوم | 🟡 Medium | Slice 2 (لإعادة استخدام نمط Print) |
| 5 | Outstanding Balance Report | 1 يوم | 🟡 Medium | Slice 4 (نمط مكرر) |
| 6 | Phase 5 Integration + Test Coverage + Status Update | 1 يوم | 🟢 Low | كل ما سبق |

**الإجمالي:** 10 أيام

---

## 🟦 Slice 1 — Attendance Foundation

**الهدف:** تسجيل `IAttendanceService` في DI، توسيع سطحها بـ 6 دوال، وبناء `AttendanceWindow` لتسجيل دخول/خروج الموظف وإدارة WorkShifts.

**Duration:** 2 يوم · **Risk:** 🔴 Critical · **Blocked by:** لا شيء

| # | الملف | الإجراء |
|---|---|---|
| 1.1 | `Services/Interfaces/IAttendanceService.cs` | **تعديل** — إضافة 6 دوال بعد السطر 26: `GetAttendanceByDateRangeAsync`, `GetTotalHoursWorkedAsync`, `GetActiveAttendanceAsync`, `GetAllShiftsAsync`, `CreateShiftAsync`, `UpdateShiftAsync` |
| 1.2 | `Services/Implementations/AttendanceService.cs` | **تعديل** — تنفيذ الدوال الستة الجديدة بعد السطر 73. لا تَمَس الدوال الثلاث الحالية |
| 1.3 | `App.xaml.cs` | **تعديل** — إضافة `services.AddScoped<IAttendanceService, AttendanceService>();` |
| 1.4 | `ViewModels/Settings/AttendanceWindowViewModel.cs` | **إنشاء** — ObservableCollections + Commands + Load |
| 1.5 | `ViewModels/Settings/AttendanceRowViewModel.cs` | **إنشاء** — يَلفّ `Attendance` |
| 1.6 | `ViewModels/Settings/WorkShiftRowViewModel.cs` | **إنشاء** — يَلفّ `WorkShift` |
| 1.7 | `Views/Settings/AttendanceWindow.xaml` | **إنشاء** — TabControl: حضور + WorkShifts |
| 1.8 | `Views/Settings/AttendanceWindow.xaml.cs` | **إنشاء** — code-behind نظيف |
| 1.9 | `App.xaml.cs` | **تعديل** — تسجيل VM + Window + Navigation |
| 1.10 | `Tests/Services/AttendanceServiceTests.cs` | **إنشاء** — 10 اختبارات |
| 1.11 | `Tests/ViewModels/Settings/AttendanceWindowViewModelTests.cs` | **إنشاء** — 6 اختبارات |
| 1.12 | `Tests/Services/AttendanceServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات |

**Validation Gate G5.1:**
```
dotnet build → 0 errors, 0 warnings جديدة
dotnet test → 434 + 17 = 451 ✅
```

---

## 🟦 Slice 2 — Cash Drawer (مع نظام كلمة المرور المُنقَّح)

**الهدف:** بناء `ICashDrawerService` + كلمة مرور بسيطة + شاشة `CashDrawerWindow`

### التدفق الجديد لكلمة المرور:
```
المستخدم يضغط "درج النقدية" في قائمة الحسابات
  │
  ├── هل توجد كلمة مرور محفوظة في LabSetting؟
  │     │
  │     ├── لا → رسالة: "لم تُعد كلمة مرور لدرج النقدية. يُرجى الإعداد من الإعدادات"
  │     │
  │     └── نعم → تظهر نافذة طلب كلمة المرور
  │           │
  │           ├── كلمة صحيحة → فتح الدرج
  │           └── كلمة خاطئة → رسالة خطأ
  │
  ├── المستخدم يُغيّر كلمة المرور من: زر "تغيير كلمة السر"
  │
  └── الدرج يفتح → ملخص + فلاتر + طباعة
```

**Duration:** 3 يوم · **Risk:** 🔴 Critical · **Blocked by:** Slice 1 (لاتساق التسجيل فقط)

| # | الملف | الإجراء |
|---|---|---|
| 2.1 | `Models/DTOs/CashDrawerSummaryDto.cs` | **إنشاء** |
| 2.2 | `Models/DTOs/CashDrawerFilterDto.cs` | **إنشاء** |
| 2.3 | `Services/Interfaces/ICashDrawerService.cs` | **إنشاء** — `GetDailySummaryAsync`, `UnlockAsync`, `ChangePasswordAsync`, `IsPasswordSetAsync` |
| 2.4 | `Services/Implementations/CashDrawerService.cs` | **إنشاء** — يحقن `FinalLabDbContext`, `ISettingsService`، يستخدم `PasswordHasher` |
| 2.5 | `App.xaml.cs` | **تعديل** — تسجيل `ICashDrawerService` |
| 2.6 | `Views/Settings/CashDrawerUnlockDialog.xaml` | **إنشاء** — طلب كلمة المرور |
| 2.7 | `Views/Settings/CashDrawerUnlockDialog.xaml.cs` | **إنشاء** |
| 2.8 | `Views/Settings/CashDrawerChangePasswordDialog.xaml` (+`.xaml.cs`) | **إنشاء** |
| 2.9 | `ViewModels/Settings/CashDrawerWindowViewModel.cs` | **إنشاء** |
| 2.10 | `Views/Settings/CashDrawerWindow.xaml` | **إنشاء** |
| 2.11 | `Views/Settings/CashDrawerWindow.xaml.cs` | **إنشاء** |
| 2.12 | `Services/Printing/CashDrawerSummaryTemplate.cs` | **إنشاء** — يرث `DocumentTemplateBase` |
| 2.13 | `App.xaml.cs` | **تعديل** — تسجيل VMs + Windows + Dialogs + Navigation |
| 2.14 | `ViewModels/Menu/AccountsMenuViewModel.cs` | **تعديل** — استبدال Placeholder بزر حقيقي |
| 2.15 | `MainWindow.xaml` | **تعديل** — تحديث DataTemplate |
| 2.16 | `Tests/Services/CashDrawerServiceTests.cs` | **إنشاء** — 10 اختبارات |
| 2.17 | `Tests/ViewModels/Settings/CashDrawerWindowViewModelTests.cs` | **إنشاء** — 8 اختبارات |
| 2.18 | `Tests/Services/CashDrawerServiceRegistrationTests.cs` | **إنشاء** — 1 اختبار |
| 2.19 | `Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs` | **تحديث** — استبدال Placeholder بزر Cash Drawer |

**Validation Gate G5.2:**
```
dotnet build → 0 errors
dotnet test → 451 + 18 = 469 ✅
```

---

## 🟦 Slice 3 — Inventory + Low-Stock Alert + Barcode Warning

**الهدف:** إضافة حقول stock إلى `TubeMaterial` + شاشة الجرد + تنبيه Dashboard + **تنبيه عند طباعة ملصق الباركود**

### ميزة جديدة: تنبيه المخزون عند طباعة الباركود

**المشهد في المختبر:**
```
المستخدم يضغط F11 (طباعة الباركود) → يظهر ملصق لأنبوب أحمر
  │
  ├── النظام يفحص: مخزون الأنبوب الأحمر = 50 (الحد الأدنى = 100)
  │     → تنبيه: "تنبيه: مخزون 'أنابيب حمراء 5 مل' منخفض (50 متبقي من 100). يُرجى الشراء."
  │
  └── الطباعة تتم (التنبيه تحذيري — لا يمنع الطباعة)
```

**أين يُضاف:**
- `BarcodeDialogViewModel.cs` — حقن `IInventoryService` + `IDialogService`
- في `PrintLabelAsync()` و `PrintAllAsync()` — فحص المخزون قبل الطباعة
- استخدام نمط `IDialogService.ShowWarning()` الموجود بالفعل

**Duration:** 2 يوم · **Risk:** 🟠 High · **Blocked by:** لا شيء

| # | الملف | الإجراء |
|---|---|---|
| 3.1 | `Models/TubeMaterial.cs` | **تعديل** — إضافة `MinimumStock` و `CurrentStock` |
| 3.2 | `Data/FinalLabDbContext.cs` | **تعديل** — Fluent mapping للحقلين |
| 3.3 | `Migrations/` | **إنشاء** — `AddTubeMaterialStockFields` |
| 3.4 | `Services/Interfaces/IInventoryService.cs` | **إنشاء** |
| 3.5 | `Services/Implementations/InventoryService.cs` | **إنشاء** |
| 3.6 | `App.xaml.cs` | **تعديل** — تسجيل `IInventoryService` |
| 3.7 | `ViewModels/Settings/InventoryWindowViewModel.cs` | **إنشاء** |
| 3.8 | `ViewModels/Settings/InventoryRowViewModel.cs` | **إنشاء** |
| 3.9 | `Views/Settings/InventoryWindow.xaml` | **إنشاء** |
| 3.10 | `Views/Settings/InventoryWindow.xaml.cs` | **إنشاء** |
| 3.11 | `Views/Settings/StockAdjustmentDialog.xaml` (+`.xaml.cs`) | **إنشاء** |
| 3.12 | `ViewModels/Menu/HomeMenuViewModel.cs` | **تعديل** — إضافة `LowStockCount` + banner |
| 3.13 | `MainWindow.xaml` | **تعديل** — low-stock banner |
| 3.14 | `Views/Converters.cs` | **تعديل** — إضافة `IntToVisibilityConverter` |
| 3.15 | `ViewModels/Patients/BarcodeDialogViewModel.cs` | **تعديل** — حقن `IInventoryService` + `IDialogService` + فحص المخزون قبل الطباعة |
| 3.16 | `App.xaml.cs` | **تعديل** — تسجيل VMs + Windows + Dialogs + Navigation |
| 3.17 | `Tests/Services/InventoryServiceTests.cs` | **إنشاء** — 10 اختبارات |
| 3.18 | `Tests/ViewModels/Settings/InventoryWindowViewModelTests.cs` | **إنشاء** — 6 اختبارات |
| 3.19 | `Tests/ViewModels/Menu/HomeMenuViewModelLowStockTests.cs` | **إنشاء** — 4 اختبارات |
| 3.20 | `Tests/ViewModels/Menu/PlaceholderMenusTests.cs` | **تعديل** — تحديث constructor |
| 3.21 | `Tests/Validation/TubeMaterialStockMigrationTests.cs` | **إنشاء** — 3 اختبارات |
| 3.22 | `Tests/Services/InventoryServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات |
| 3.23 | `Tests/Patients/BarcodeDialogLowStockWarningTests.cs` | **إنشاء** — 4 اختبارات (جديد) |

**Validation Gate G5.3:**
```
dotnet build → 0 errors
dotnet test → 469 + 29 = 498 ✅
```

> **تنبيهات تنفيذ Slice 3:**
> - `InventoryServiceTests.cs` يجب أن يستخدم `DateTime.UtcNow` وليس `DateTime.Today` في أي تاريخ
> - `BarcodeDialogLowStockWarningTests.cs` يجب أن يستخدم `It.IsAny<string>()` مع `ShowWarning`
> - `IntToVisibilityConverter` موجود بالفعل في `Converters.cs` — لا حاجة لإعادة إنشائه

---

## 🟦 Slice 4 — Referral Commission Report

**الهدف:** عرض `VReferralCommissionReport` view في UI مع filters وطباعة.

**Duration:** 1 يوم · **Risk:** 🟡 Medium · **Blocked by:** Slice 2

| # | الملف | الإجراء |
|---|---|---|
| 4.1 | `Services/Interfaces/ICommissionReportService.cs` | **إنشاء** |
| 4.2 | `Services/Implementations/CommissionReportService.cs` | **إنشاء** |
| 4.3 | `App.xaml.cs` | **تعديل** — تسجيل الخدمة |
| 4.4 | `Services/Printing/CommissionReportTemplate.cs` | **إنشاء** |
| 4.5 | `ViewModels/Settings/CommissionReportWindowViewModel.cs` | **إنشاء** |
| 4.6 | `Views/Settings/CommissionReportWindow.xaml` (+`.xaml.cs`) | **إنشاء** |
| 4.7 | `App.xaml.cs` | **تعديل** — Transient + Navigation |
| 4.8 | `ViewModels/Menu/AccountsMenuViewModel.cs` | **تعديل** — إضافة زر "تقرير العمولات" |
| 4.9 | `MainWindow.xaml` | **تعديل** — تحديث DataTemplate |
| 4.10 | `Tests/Services/CommissionReportServiceTests.cs` | **إنشاء** — 6 اختبارات |
| 4.11 | `Tests/ViewModels/Settings/CommissionReportWindowViewModelTests.cs` | **إنشاء** — 5 اختبارات |
| 4.12 | `Tests/Services/CommissionReportServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات |

**Validation Gate G5.4:**
```
dotnet build → 0 errors
dotnet test → 498 + 13 = 511 ✅
```

> **تنبيهات تنفيذ Slice 4:**
> - `CommissionReportTemplate.cs` يجب أن يستخدم نمط WPF Table الصحيح (راجع قواعد التنفيذ الإلزامية أعلاه)
> - `CommissionReportServiceTests.cs` — seed data يجب أن يحتوي `Sex = "M"` إذا استُخدم Patient
> - `CommissionReportWindowViewModelTests.cs` — `It.IsAny<T>()` للمعاملات الاختيارية في Moq
> - يجب التحقق من أعمدة `VReferralCommissionReport` view قبل بناء الخدمة

---

## 🟦 Slice 5 — Outstanding Balance Report

**الهدف:** نفس نمط Slice 4 لكن لـ `VOutstandingBalance`.

**Duration:** 1 يوم · **Risk:** 🟡 Medium · **Blocked by:** Slice 4

| # | الملف | الإجراء |
|---|---|---|
| 5.1 | `Services/Interfaces/IOutstandingBalanceReportService.cs` | **إنشاء** |
| 5.2 | `Services/Implementations/OutstandingBalanceReportService.cs` | **إنشاء** |
| 5.3 | `App.xaml.cs` | **تعديل** — تسجيل الخدمة |
| 5.4 | `Services/Printing/OutstandingBalanceReportTemplate.cs` | **إنشاء** |
| 5.5 | `ViewModels/Settings/OutstandingBalanceReportWindowViewModel.cs` | **إنشاء** |
| 5.6 | `Views/Settings/OutstandingBalanceReportWindow.xaml` (+`.xaml.cs`) | **إنشاء** |
| 5.7 | `App.xaml.cs` | **تعديل** — Transient + Navigation |
| 5.8 | `ViewModels/Menu/AccountsMenuViewModel.cs` | **تعديل** — إضافة زر "الأرصدة المعلَّقة" |
| 5.9 | `MainWindow.xaml` | **تعديل** — تحديث DataTemplate |
| 5.10 | `Tests/Services/OutstandingBalanceReportServiceTests.cs` | **إنشاء** — 5 اختبارات |
| 5.11 | `Tests/ViewModels/Settings/OutstandingBalanceReportWindowViewModelTests.cs` | **إنشاء** — 5 اختبارات |
| 5.12 | `Tests/Services/OutstandingBalanceReportServiceRegistrationTests.cs` | **إنشاء** — 2 اختبارات |

**Validation Gate G5.5:**
```
dotnet build → 0 errors
dotnet test → 511 + 12 = 523 ✅
```

> **تنبيهات تنفيذ Slice 5:**
> - `OutstandingBalanceReportTemplate.cs` يجب أن يستخدم نمط WPF Table الصحيح (راجع قواعد التنفيذ الإلزامية)
> - يجب التحقق من أعمدة `VOutstandingBalance` view قبل بناء الخدمة

---

## 🟦 Slice 6 — Integration + Test Coverage + Status Update

**الهدف:** اختبارات تكامل end-to-end، تنظيف، تحديث `IMPLEMENTATION_STATUS.md`.

**Duration:** 1 يوم · **Risk:** 🟢 Low · **Blocked by:** كل ما سبق

| # | الملف | الإجراء |
|---|---|---|
| 6.1 | `Tests/Integration/AttendanceWorkflowEndToEndTests.cs` | **إنشاء** — 4 اختبارات |
| 6.2 | `Tests/Integration/CashDrawerEndToEndTests.cs` | **إنشاء** — 5 اختبارات |
| 6.3 | `Tests/Integration/InventoryAlertEndToEndTests.cs` | **إنشاء** — 4 اختبارات |
| 6.4 | `Tests/Integration/Phase5BuildVerificationTests.cs` | **إنشاء** — 3 اختبارات |
| 6.5 | `Docs/PRDs/IMPLEMENTATION_STATUS.md` | **تعديل** — إضافة قسم Phase 5 كامل |
| 6.6 | (يدوي) Smoke test كامل | تشغيل التطبيق + فتح كل شاشة |

**Validation Gate G5.6 (نهائي):**
```
dotnet build → 0 errors, 0 warnings جديدة
dotnet test → 523 + 16 = 539 ✅
كل اختبارات Phase 1-4 (434) لا تزال تنجح
IMPLEMENTATION_STATUS.md محدَّث
```

> **تنبيهات تنفيذ Slice 6:**
> - اختبارات التكامل قد تحتاج `DateTime.UtcNow` بدلاً من `DateTime.Today` — تحقق من كل اختبار

---

## جدول الاختبارات المُنقَّح

| ملف الاختبار | العدد |
|---|---|
| `Tests/Services/AttendanceServiceTests.cs` | 10 |
| `Tests/ViewModels/Settings/AttendanceWindowViewModelTests.cs` | 6 |
| `Tests/Services/AttendanceServiceRegistrationTests.cs` | 1 |
| `Tests/Services/CashDrawerServiceTests.cs` | 10 |
| `Tests/ViewModels/Settings/CashDrawerWindowViewModelTests.cs` | 8 |
| `Tests/Services/CashDrawerServiceRegistrationTests.cs` | 1 |
| `Tests/Services/InventoryServiceTests.cs` | 10 |
| `Tests/ViewModels/Settings/InventoryWindowViewModelTests.cs` | 6 |
| `Tests/ViewModels/Menu/HomeMenuViewModelLowStockTests.cs` | 4 |
| `Tests/ViewModels/Menu/PlaceholderMenusTests.cs` (تحديث) | 0 |
| `Tests/Validation/TubeMaterialStockMigrationTests.cs` | 3 |
| `Tests/Services/InventoryServiceRegistrationTests.cs` | 2 |
| `Tests/Patients/BarcodeDialogLowStockWarningTests.cs` | 4 |
| `Tests/Services/CommissionReportServiceTests.cs` | 6 |
| `Tests/ViewModels/Settings/CommissionReportWindowViewModelTests.cs` | 5 |
| `Tests/Services/CommissionReportServiceRegistrationTests.cs` | 2 |
| `Tests/Services/OutstandingBalanceReportServiceTests.cs` | 5 |
| `Tests/ViewModels/Settings/OutstandingBalanceReportWindowViewModelTests.cs` | 5 |
| `Tests/Services/OutstandingBalanceReportServiceRegistrationTests.cs` | 2 |
| `Tests/Integration/AttendanceWorkflowEndToEndTests.cs` | 4 |
| `Tests/Integration/CashDrawerEndToEndTests.cs` | 5 |
| `Tests/Integration/InventoryAlertEndToEndTests.cs` | 4 |
| `Tests/Integration/Phase5BuildVerificationTests.cs` | 3 |
| `Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs` (تحديث) | +1 |
| **الإجمالي الجديد** | **105** |
| **الإجمالي بعد Phase 5** | **434 + 105 = 539** |

---

## ملخص التغييرات عن الخطة الأصلية

| البند | الخطة الأصلية | الخطة المُنقَّحة |
|---|---|---|
| كلمة مرور درج النقدية | "123" + إجبار تغيير | المستخدم يُعيّنها من البداية |
| تنبيه الباركود | غير موجود | إضافة 4 اختبارات + تعديل BarcodeDialogViewModel |
| عدد الاختبارات | 108 | 105 |
| المجموع النهائي | 539 | 539 |

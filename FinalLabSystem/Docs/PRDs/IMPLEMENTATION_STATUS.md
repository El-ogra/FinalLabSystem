# FinalLabSystem — Implementation Status

---

## Phase 1: Core Safety & Infrastructure

**Status:** ✅ مكتملة  
**Date:** 2026-06-24  
**Total files created:** 16  
**Total files modified:** 16  
**Total files deleted:** 1  
**Tests:** 185 / 185 — ✅ جميع الاختبارات ناجحة  
**Build:** ✅ ناجح — 0 أخطاء

---

### Subphase 1.1 — Stage-Gating (Feature Toggles + ResultStageRules)

**Status:** ✅ مكتملة  

**Files created:**
- `Infrastructure/ResultStageRules.cs` — قواعد المراحل الثابتة (CanPrint, CanExport, CanDeliver)
- `Services/FeatureToggles.cs` — ثوابت مفاتيح Feature Toggle
- `Services/Interfaces/IFeatureToggleService.cs` — واجهة خدمة التبديل
- `Services/Implementations/FeatureToggleService.cs` — تطبيق الخدمة
- `Tests/Services/ResultStageRulesTests.cs` — 9 اختبارات
- `Tests/Services/RoutineResultServiceGuardTests.cs` — 4 اختبارات

**Files modified:**
- `Services/Implementations/RoutineResultService.cs` — إضافة guards في `TogglePrintStatusAsync` و `ToggleExportStatusAsync`
- `Services/Implementations/TestCatalogSeeder.cs` — بذرة `EnforceStageGating=true`

---

### Subphase 1.2 — Print Pipeline

**Status:** ✅ مكتملة  

**Files created:**
- `Services/Printing/DocumentTemplateBase.cs` — الفئة الأساسية للـ templates
- `Services/Printing/ReceiptTemplate.cs` — قالب الإيصال
- `Services/Printing/ResultReportTemplate.cs` — قالب تقرير النتائج
- `Services/Implementations/WpfFlowDocumentPrintService.cs` — خدمة الطباعة (Scoped)
- `Tests/Services/WpfFlowDocumentPrintServiceTests.cs` — 4 اختبارات

**Files modified:**
- `App.xaml.cs` — استبدال تسجيل `NullPrintService` بـ `WpfFlowDocumentPrintService` (Scoped)

---

### Subphase 1.3 — Audit Infrastructure

**Status:** ✅ مكتملة  

**Bugs found & fixed during implementation:**
1. **العلة الأولى (حالة Added — fixed):** `entry.State` تغير من `Added` إلى `Unchanged` بعد `base.SaveChangesAsync` بسبب `AcceptAllChangesOnSuccess`. الحل: أخذ snapshot للـ `State` قبل الحفظ.
2. **العلة الثانية (حالة Modified — fixed):** `property.IsModified` ترجع `false` بعد الحفظ. الحل: أخذ snapshot للخصائص المعدلة (`IsModified`) قبل الحفظ في anonymous type.
3. **العلة الثالثة (حالة Deleted — fixed):** الكيان يصبح `Detached` بعد الحذف. الحل: أخذ snapshot لكل `CurrentValue` و `OriginalValue` قبل الحفظ.
4. **العلة الرابعة (Static state — fixed):** استخدام `static int _auditingFlag` سبّب تلويث الحالة عبر الاختبارات المتوازية. الحل: استخدام `AsyncLocal<bool>` بدلاً من الحقل الثابت.

**Files created:**
- `Tests/Data/AuditInterceptorTests.cs` — 5 اختبارات

**Files modified:**
- `Data/FinalLabDbContext.cs` — إضافة `SaveChangesAsync` override مع auditable entity detection
- `Models/TestResult.cs` — إضافة `[Auditable]`
- `Models/TestWorkflow.cs` — إضافة `[Auditable]`
- `Models/SampleTube.cs` — إضافة `[Auditable]`
- `Models/VisitCharge.cs` — إضافة `[Auditable]`
- `Models/ExternalShipment.cs` — إضافة `[Auditable]`
- `Models/ExternalShipmentItem.cs` — إضافة `[Auditable]`

---

### Subphase 1.4 — VM Split

**Status:** ✅ مكتملة

**Files created:**
- `ViewModels/Patients/Delivery/DeliveryViewModel.cs`
- `ViewModels/Patients/Search/PatientSearchViewModel.cs`

**Files deleted:**
- `ViewModels/Patients/PlaceholderTaskViewModels.cs` (إزالة `PlaceholderTaskViewModelBase` بالكامل)

**Files modified:**
- `ViewModels/MainViewModel.cs` — تحديث usings
- `ViewModels/Patients/TestResultsViewModel.cs` — تحديث usings
- `Views/Patients/DeliveryWindow.xaml.cs` — تحديث usings
- `Views/Patients/PatientSearchWindow.xaml.cs` — تحديث usings
- `App.xaml.cs` — تحديث usings

---

### Subphase 1.5 — Database Constraints (Discount Exclusivity)

**Status:** ✅ مكتملة

**Files created:**
- `Migrations/20260624131538_AddVisitDiscountExclusivityConstraint.cs` — هجرة لإضافة CHECK constraint
- `Migrations/20260624131538_AddVisitDiscountExclusivityConstraint.Designer.cs`

**Files modified:**
- `Migrations/FinalLabDbContextModelSnapshot.cs` — تحديث الـ snapshot
- `Models/VisitTest.cs` — إضافة `[NotMapped] AggregateValidationStatus`

---

## Phase 2: Patient Identity & Workflow Stabilization

**Status:** ✅ مكتملة  
**Date:** 2026-06-25  
**Total files created:** 17  
**Total files modified:** 12  
**Total files deleted:** 3  
**Tests:** 280 / 280 — ✅ جميع الاختبارات ناجحة  
**Build:** ✅ ناجح — 0 أخطاء

---

### Task 2.7 — Technical Debt Cleanup

**Status:** ✅ مكتملة

**Files deleted:**
- `Services/Implementations/NullPrintService.cs` — حذف خدمة طباعة ميتة (لا توجد references خارجية)
- `Views/Patients/TestSelectionView.xaml.bak` — حذف ملف نسخ احتياطي
- `Views/Patients/TestSelectionView.xaml.bak2` — حذف ملف نسخ احتياطي

**Files modified:**
- `Services/Interfaces/IPrintService.cs` — تحديث تعليق `<see cref>` للإشارة إلى `WpfFlowDocumentPrintService`
- `.gitignore` (root) — إضافة قواعد `*.bak`, `*.bak2`, `*.orig`, `*.swp`

**Files created:**
- `.githooks/pre-commit` — git hook للتحقق من البناء قبل الـ commit
- `tools/install-hooks.sh` — سكربت تثبيت الـ hooks

---

### Task 2.1 — DI Registration & Navigation

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IAuditTrailDialogService.cs` — واجهة خدمة فتح نافذة Audit Trail
- `Services/Interfaces/IResultEntryDialogService.cs` — واجهة خدمة فتح نافذة Result Entry
- `Services/Implementations/AuditTrailDialogService.cs` — تنفيذ Factory Pattern لـ AuditTrailWindow
- `Services/Implementations/ResultEntryDialogService.cs` — تنفيذ Factory Pattern لـ ResultEntryWindow

**Files modified:**
- `App.xaml.cs` — تسجيل `IAuditTrailDialogService` + `IResultEntryDialogService` كـ Singleton، إضافة 12 Transient registration لـ Menu VMs + navigation mappings

---

### Task 2.2 — AuditTrailWindow Factory Pattern

**Status:** ✅ مكتملة

**Files modified:**
- `ViewModels/Patients/TestResultsViewModel.cs` — حقن `IAuditTrailDialogService`، استبدال استدعاءات `new AuditTrailWindow()` المباشرة بـ `ShowAuditPAsync` و `ShowAuditTAsync` عبر Dialog Service

---

### Task 2.3 — ResultEntryWindow Factory Pattern + CancelCommand

**Status:** ✅ مكتملة

**Files modified:**
- `ViewModels/Patients/ResultEntryViewModel.cs` — إضافة `RequestClose` property + تفعيل `CancelCommand` مع `!IsSaving` guard + استدعاء `RequestClose?.Invoke()` بعد `SaveCompleted`
- `ViewModels/Patients/TestResultsViewModel.cs` — حقن `IResultEntryDialogService`، استبدال `OpenMultiComponentEditorAsync` بـ Dialog Service
- `Services/Implementations/ResultEntryDialogService.cs` — ربط `vm.RequestClose` لإغلاق النافذة

---

### Task 2.5 — Main Dashboard 12-Icon Toolbar

**Status:** ✅ مكتملة

**Files created:**
- `ViewModels/Menu/HomeMenuViewModel.cs` — قائمة الصفحة الرئيسية
- `ViewModels/Menu/PatientsMenuViewModel.cs` — قائمة المرضى (مستخرج من MainViewModel)
- `ViewModels/Menu/ResultsMenuViewModel.cs` — قائمة النتائج
- `ViewModels/Menu/DeliveryMenuViewModel.cs` — قائمة التسليم
- `ViewModels/Menu/SearchMenuViewModel.cs` — قائمة البحث
- `ViewModels/Menu/ExternalSamplesMenuViewModel.cs` — placeholder Phase 4
- `ViewModels/Menu/AccountsMenuViewModel.cs` — placeholder Phase 5
- `ViewModels/Menu/BackupMenuViewModel.cs` — placeholder Phase 6
- `ViewModels/Menu/TestDataMenuViewModel.cs` — قائمة بيانات الاختبارات
- `ViewModels/Menu/NormalRangesMenuViewModel.cs` — قائمة النطاقات الطبيعية
- `ViewModels/Menu/ReportSettingsMenuViewModel.cs` — placeholder Phase 6

**Files modified:**
- `ViewModels/MainViewModel.cs` — إعادة كتابة: 14 أمر، استخراج `PatientsMenuViewModel` من كلاس متداخل
- `MainWindow.xaml` — إعادة كتابة: 12 أيقونة toolbar + 8 DataTemplates

---

### Task 2.6 — Patient Status Icons Generalization

**Status:** ✅ مكتملة

**Files modified:**
- `ViewModels/Patients/PatientRegistrationViewModel.cs` — تغيير `ObservableCollection<TodayPatientDto>` إلى `TodayPatientWithStatusDto`
- `ViewModels/Patients/TodayPatientsDialogViewModel.cs` — تحديث نوع القائمة + استدعاء `GetTodayPatientsWithStatusAsync()` + تحديث filter cast
- `Views/Patients/TodayPatientsDialog.xaml.cs` — تحديث نوع `SelectedPatient` للإرجاع
- `Views/Patients/TodayPatientsDialog.xaml` — إضافة عمود StatusIcon بـ `FontFamily="Segoe UI Emoji"`

---

### Task 2.4 — F-Key Semantic Remapping

**Status:** ✅ مكتملة

**Files modified:**
- `Infrastructure/Navigation/INavigationService.cs` — إضافة overload `OpenTaskWindow<TViewModel>(Action<TViewModel>?)`
- `Infrastructure/Navigation/NavigationService.cs` — تنفيذ الـ overload مع `configure` callback + `window.Show()`
- `Infrastructure/Settings/IUserSettingsService.cs` — إضافة `KeyboardShortcutsNoticeShown` property
- `Infrastructure/Settings/JsonUserSettingsService.cs` — تنفيذ الـ flag مع `lock` + `SaveSettingsAsync`
- `ViewModels/Patients/PatientRegistrationViewModel.cs` — إضافة `_navigationService` field + 5 أوامر تنقل جديدة (NavigateToPatientData, NavigateToSearch, NavigateToResultEntry, NavigateToDelivery, NavigateToExternalSamples)
- `ViewModels/Patients/TestResultsViewModel.cs` — إضافة `_receiptService` field + 4 أوامر جديدة (EditSelectedPatient, PrintReceipt, NavigateToResultEntry, NavigateToExternalSamples) + handler methods
- `Views/Patients/PatientRegistrationWindow.xaml` — إضافة `Window.InputBindings` بـ 13 KeyBinding (F1–F12 + Escape)
- `Views/Patients/TestResultsWindow.xaml` — تعديل 3 KeyBindings (F8→Edit, F12→Receipt) + إضافة 4 جديدة (F4, F7, Ctrl+R, Ctrl+F, Ctrl+P) + شريط اختصارات أسفل النافذة

---

### Task 2.8 — Complete Test Suite

**Status:** ✅ مكتملة

**Test files created (13 ملف، 87 اختبار جديد):**

| ملف الاختبار | عدد الاختبارات |
|-------------|---------------|
| `Tests/ViewModels/AuditTrailViewModelTests.cs` | 8 |
| `Tests/Services/AuditTrailDialogServiceTests.cs` | 3 |
| `Tests/ViewModels/ResultEntryViewModelTests.cs` | 15 |
| `Tests/Services/ResultEntryDialogServiceTests.cs` | 3 |
| `Tests/ViewModels/MainViewModelTests.cs` | 16 |
| `Tests/ViewModels/Menu/PatientsMenuViewModelTests.cs` | 4 |
| `Tests/ViewModels/Menu/PlaceholderMenusTests.cs` | 4 |
| `Tests/ViewModels/Patients/TodayPatientsStatusDisplayTests.cs` | 4 |
| `Tests/Services/PatientStatusComputationTests.cs` | 7 |
| `Tests/ViewModels/Patients/PatientRegistrationFKeyTests.cs` | 12 |
| `Tests/ViewModels/TestResultsFKeyRemappingTests.cs` | 7 |
| `Tests/Services/AuditTrailWindowRegistrationTests.cs` | 2 |
| `Tests/Services/ResultEntryWindowRegistrationTests.cs` | 2 |
| **الإجمالي** | **87** |

---

## Phase 3: Result Editor Alignment

**Status:** ✅ مكتملة  
**Date:** 2026-06-25  
**Total files created:** 27  
**Total files modified:** 25  
**Total files deleted:** 0  
**Tests:** 347 / 347 — ✅ جميع الاختبارات ناجحة  
**Build:** ✅ ناجح — 0 أخطاء

---

### Task 3.3 — ReportCommentTemplate Management Foundation/UI

**Status:** ✅ مكتملة

**Files created:**
- `Models/Enums/ReportCommentTrigger.cs` — قائمة مرجعية للمحفّزات: None, Low, High, Critical, Manual
- `Services/Interfaces/IReportCommentTemplateService.cs` — واجهة CRUD + بحث القوالب
- `Services/Implementations/ReportCommentTemplateService.cs` — تنفيذ: استعلام نشطة، بحث بمحفّز، إنشاء/تعديل/حذف(soft)
- `ViewModels/Settings/ReportCommentTemplateViewModel.cs` — نموذج إدارة القوالب مع فلترة وحفظ
- `Views/Settings/ReportCommentTemplateWindow.xaml` — نافذة إدارة القوالب (Master-Detail)
- `Views/Settings/ReportCommentTemplateWindow.xaml.cs` — تهيئة النافذة

**Files modified:**
- `Models/ReportCommentTemplate.cs` — إضافة خاصية `string? TriggerCondition`
- `Data/FinalLabDbContext.cs` — تعيين `TriggerCondition` مع القيمة الافتراضية "Manual"
- `ViewModels/Menu/ReportSettingsMenuViewModel.cs` — استبدال `IDialogService` بـ `INavigationService` لفتح نافذة القوالب
- `ViewModels/MainViewModel.cs` — تمرير `INavigationService` بدلاً من `IDialogService`
- `App.xaml.cs` — تسجيل `ReportCommentTemplateViewModel` / `ReportCommentTemplateWindow` في Navigation

---

### Task 3.4 — Auto-Comment Injection Logic

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IReportCommentEngine.cs` — واجهة تطبيق التعليقات التلقائية
- `Services/Implementations/ReportCommentEngine.cs` — تنفيذ: تحويل ResultStatus إلى ReportCommentTrigger + بحث قالب + تطبيق

**Files modified:**
- `Services/Implementations/RoutineResultService.cs` — حقن `IReportCommentEngine` + استدعاء `ApplyAutoCommentAsync` بعد حساب ResultStatus وقبل SaveChanges
- `App.xaml.cs` — تسجيل `ReportCommentEngine` كـ Scoped

---

### Task 3.2 — TestProfile Management UI

**Status:** ✅ مكتملة

**Files created:**
- `ViewModels/Settings/TestProfileWindowViewModel.cs` — نموذج إدارة البروفايلات مع Master-Detail
- `ViewModels/Settings/TestProfileRowViewModel.cs` — نموذج صف البروفايل
- `ViewModels/Settings/TestProfileItemRowViewModel.cs` — نموذج عنصر البروفايل مع ترتيب
- `Views/Settings/TestProfileWindow.xaml` — نافذة إدارة البروفايلات
- `Views/Settings/TestProfileWindow.xaml.cs` — تهيئة النافذة

**Files modified:**
- `Services/Interfaces/ITestCatalogService.cs` — إضافة 7 طرق CRUD للبروفايلات (GetAllProfiles, Create, Update, Delete, AddItem, RemoveItem, UpdateItemSortOrder)
- `Services/Implementations/TestCatalogService.cs` — تنفيذ طرق البروفايلات مع soft delete
- `ViewModels/Menu/TestDataMenuViewModel.cs` — إضافة `NavigateToProfilesCommand` عبر `INavigationService`
- `App.xaml.cs` — تسجيل `TestProfileWindowViewModel` / `TestProfileWindow` في Navigation

---

### Task 3.5 — TestProfile Expansion on Patient Registration

**Status:** ✅ مكتملة

**Files created:**
- `Views/Patients/ProfileSelectionDialog.xaml` — نافذة اختيار البروفايل مع قائمة + زر اختيار/إلغاء
- `Views/Patients/ProfileSelectionDialog.xaml.cs` — معالجة النقر المزدوج والاختيار

**Files modified:**
- `ViewModels/Patients/TestSelectionViewModel.cs` — إضافة `ApplyProfileCommand` + `ApplyProfileAsync` (إضافة تحاليل البروفايل مع تخطي المكررات + DefaultPrice)
- `Views/Patients/TestSelectionView.xaml` — إضافة زر "تطبيق بروفايل"

---

### Task 3.1 — ResultEntryWindow Full Integration

**Status:** ✅ مكتملة

#### Task 3.1a — ResultClinicalStatus + Live Validation

**Files created:**
- `Models/Enums/ResultClinicalStatus.cs` — قائمة: Normal, Low, High, Critical

**Files modified:**
- `Models/DTOs/TestComponentResultDto.cs` — إضافة `SnapLowCritical`, `SnapHighCritical`, `ClinicalStatus` + تطبيق `INotifyPropertyChanged`
- `ViewModels/Patients/ResultEntryViewModel.cs` — إضافة `patientAgeDays`, `patientGender`, `isPregnant` للمُنشئ + `RecomputeClinicalStatus` لحظي
- `Services/Interfaces/IResultEntryDialogService.cs` — توسيع `OpenAsync` بمعاملات المريض
- `Services/Implementations/ResultEntryDialogService.cs` — تمرير بيانات المريض إلى ViewModel
- `ViewModels/Patients/TestResultsViewModel.cs` — تمرير SnapLowCritical/SnapHighCritical + بيانات المريض عند فتح المحرر
- `Views/Converters.cs` — إضافة `ClinicalStatusToBrushConverter` (أخضر/أزرق/أحمر/أحمر غامق)
- `Views/Patients/ResultEntryWindow.xaml` — إضافة عمود ClinicalStatus مع ألوان + تمديد العرض

#### Task 3.1b — Row Save-Selection + Partial Save

**Files modified:**
- `Models/DTOs/TestComponentResultDto.cs` — إضافة `IsSelectedForSave` (INotifyPropertyChanged, default: true)
- `ViewModels/Patients/ResultEntryViewModel.cs` — تعديل `SaveAsync` لفلترة `.Where(c => c.IsSelectedForSave)`
- `Views/Patients/ResultEntryWindow.xaml` — إضافة عمود "حفظ" (CheckBox) كأول عمود

#### Task 3.1c — Save & Review Command

**Files modified:**
- `Services/Interfaces/IRoutineResultService.cs` — إضافة `Task<bool> ToggleReviewStatusAsync(int visitTestId, int staffId)`
- `Services/Implementations/RoutineResultService.cs` — تنفيذ: تحديث ValidationStatus من Entered إلى Reviewed
- `ViewModels/Patients/ResultEntryViewModel.cs` — إضافة `SaveAndReviewCommand` + `SaveAndReviewAsync`
- `Views/Patients/ResultEntryWindow.xaml` — إضافة زر "حفظ و مراجعة" (أزرق)

#### Task 3.1d — Critical-Value Confirmation Dialog

**Files modified:**
- `ViewModels/Patients/ResultEntryViewModel.cs` — حقن `IDialogService` + `HasCriticalValues()` + `ConfirmCriticalSave()` + فحص قبل الحفظ في `SaveAsync` و `SaveAndReviewAsync`
- `Services/Implementations/ResultEntryDialogService.cs` — تمرير `IDialogService` إلى ViewModel
- `ViewModels/ResultEntryViewModelTests.cs` — تحديث استدعاءات المُنشئ بمعامل `mockDialog.Object`

#### Task 3.1e — ContentPresenter Placeholder

**Files modified:**
- `Views/Patients/ResultEntryWindow.xaml` — إضافة `ContentPresenter` مربوط بـ `CustomEditorContent` (Phase 7 placeholder)
- `ViewModels/Patients/ResultEntryViewModel.cs` — إضافة خاصية `CustomEditorContent`

---

### Task 3.6 — Phase 3 Complete Test Suite

**Status:** ✅ مكتملة

**Test files created (9 ملفات، 67 اختبار جديد):**

| ملف الاختبار | عدد الاختبارات |
|-------------|---------------|
| `Tests/Services/RoutineResultServiceLiveValidationTests.cs` | 7 |
| `Tests/Services/ReportCommentEngineTests.cs` | 8 |
| `Tests/Services/ReportCommentTemplateServiceTests.cs` | 10 |
| `Tests/Services/TestCatalogService_ProfileCrudTests.cs` | 8 |
| `Tests/ViewModels/Settings/TestProfileWindowViewModelTests.cs` | 5 |
| `Tests/ViewModels/Settings/ReportCommentTemplateViewModelTests.cs` | 7 |
| `Tests/ViewModels/ResultEntryViewModelLiveValidationTests.cs` | 12 |
| `Tests/ViewModels/TestSelectionViewModelProfileApplyTests.cs` | 5 |
| `Tests/Integration/AutoCommentEndToEndTests.cs` | 5 |
| **الإجمالي** | **67** |

---

### Migration — AddReportCommentTemplate_TriggerCondition

**Status:** ✅ مكتملة

**Files created:**
- `Migrations/20260625222151_AddReportCommentTemplate_TriggerCondition.cs` — إضافة عمود `trigger_condition` (nvarchar(20), nullable, defaultValue: "Manual") + UPDATE للسجلات الموجودة
- `Migrations/20260625222151_AddReportCommentTemplate_TriggerCondition.Designer.cs` — تصميم الترحيل

**Files modified:**
- `Migrations/FinalLabDbContextModelSnapshot.cs` — تحديث Snapshot

**Database:** ✅ مُطبق على FinalLab (.\SQLEXPRESS)

---

## Phase 4: Billing & Contracts

**Status:** ✅ مكتملة  
**Date:** 2026-06-26  
**Total files created:** 27  
**Total files modified:** 8  
**Tests:** 431 / 431 — ✅ جميع الاختبارات ناجحة  
**Build:** ✅ ناجح — 0 أخطاء

---

### Slice 1 — Company Management Foundation

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/ICompanyService.cs` — واجهة إدارة الشركات
- `Services/Implementations/CompanyService.cs` — تنفيذ CRUD الشركات
- `ViewModels/Settings/CompaniesWindowViewModel.cs` — نموذج إدارة الشركات
- `ViewModels/Settings/CompanyRowViewModel.cs` — نموذج صف الشركة
- `Views/Settings/CompaniesWindow.xaml` — نافذة إدارة الشركات
- `Views/Settings/CompaniesWindow.xaml.cs` — تهيئة النافذة
- `Tests/Services/CompanyServiceTests.cs` — 8 اختبارات

**Files modified:**
- `Models/Company.cs` — إضافة `ContractStartDate?`, `ContractEndDate?`, `BillingPeriodicity?`
- `Data/FinalLabDbContext.cs` — Fluent API للحقول الجديدة
- `App.xaml.cs` — تسجيل `ICompanyService` في DI + Navigation

**Migration:** `AddCompanyContractFields` — 3 أعمدة nullable

---

### Slice 2 — Pricing Integration

**Status:** ✅ مكتملة

**Files created:**
- `Models/DTOs/TestPricingResultDto.cs` — DTO نتيجة التسعير
- `Infrastructure/TestPricingEngine.cs` — محرك التسعير مع fallback
- `ViewModels/Settings/PriceSchemeWindowViewModel.cs` — نموذج إدارة أسعار التسعير
- `ViewModels/Settings/PriceSchemeWindow.xaml` — نافذة إدارة التسعير
- `ViewModels/Settings/PriceSchemeWindow.xaml.cs` — تهيئة النافذة
- `Tests/Services/PricingServiceTests.cs` — 4 اختبارات
- `Tests/Infrastructure/TestPricingEngineTests.cs` — 6 اختبارات

**Files modified:**
- `Services/Interfaces/IPricingService.cs` — إضافة `GetSchemeByIdAsync`, `CreateSchemeAsync`, `UpdateSchemeAsync`
- `Services/Implementations/PricingService.cs` — تنفيذ الطرق الجديدة
- `ViewModels/Patients/TestSelectionViewModel.cs` — استبدال `DefaultPrice` بـ `TestPricingEngine`
- `App.xaml.cs` — تسجيل `TestPricingEngine` + Navigation

---

### Slice 3 — Invoice Workflow

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IInvoiceService.cs` — 5 طرق للفواتير
- `Services/Implementations/InvoiceService.cs` — مع Transaction في RecordPaymentAsync
- `Services/Printing/ContractInvoiceTemplate.cs` — قالب طباعة الفاتورة
- `ViewModels/Settings/ContractInvoiceWindowViewModel.cs` — نموذج إدارة الفواتير
- `ViewModels/Settings/InvoiceRowViewModel.cs` — نموذج صف الفاتورة
- `ViewModels/Settings/PaymentRowViewModel.cs` — نموذج صف الدفعة
- `Views/Settings/ContractInvoiceWindow.xaml` — نافذة إدارة الفواتير
- `Views/Settings/ContractInvoiceWindow.xaml.cs` — تهيئة النافذة
- `Tests/Services/InvoiceServiceTests.cs` — 8 اختبارات
- `Tests/ViewModels/ContractInvoiceWindowViewModelTests.cs` — 6 اختبارات

**Files modified:**
- `Models/ContractInvoice.cs` — تغيير القيمة الافتراضية للحالة إلى "Pending"
- `Services/Implementations/ContractService.cs` — تحويل إلى Adapter يستدعي IInvoiceService
- `App.xaml.cs` — تسجيل `IInvoiceService` + Navigation

---

### Slice 4 — External Labs Management & Shipments

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IExternalLabRegistryService.cs` — واجهة إدارة المعامل الخارجية
- `Services/Implementations/ExternalLabRegistryService.cs` — تنفيذ CRUD المعامل
- `Services/Interfaces/IExternalShipmentService.cs` — واجهة الشحنات
- `Services/Implementations/ExternalShipmentService.cs` — تنفيذ الشحنات مع Transaction
- `ViewModels/Settings/ExternalLabsWindowViewModel.cs` — نموذج إدارة المعامل (4 تبويبات)
- `ViewModels/Settings/ExternalLabRowViewModel.cs` — نموذج صف المختبر
- `ViewModels/Settings/ShipmentRowViewModel.cs` — نموذج صف الشحنة + العناصر
- `Views/Settings/ExternalLabsWindow.xaml` — نافذة إدارة المعامل
- `Views/Settings/ExternalLabsWindow.xaml.cs` — تهيئة النافذة
- `Tests/Services/ExternalLabRegistryServiceTests.cs` — 6 اختبارات
- `Tests/Services/ExternalShipmentServiceTests.cs` — 8 اختبارات

**Files modified:**
- `Services/Implementations/ExternalLabService.cs` — تحويل إلى Adapter يستدعي IExternalShipmentService
- `App.xaml.cs` — تسجيل الخدمات الجديدة + Navigation

---

### Slice 5 — Menu Activation + F7 Wire-up

**Status:** ✅ مكتملة

**Files created:**
- `Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs` — 4 اختبارات
- `Tests/ViewModels/Settings/ExternalLabsWindowViewModelTests.cs` — اختبارات F7

**Files modified:**
- `ViewModels/Menu/AccountsMenuViewModel.cs` — 3 أوامر حقيقية (Companies, Pricing, Invoices) + Cash Drawer placeholder
- `ViewModels/Menu/ExternalSamplesMenuViewModel.cs` — أمر حقيقي (NavigateToExternalLabs)
- `ViewModels/MainViewModel.cs` — تمرير INavigationService بدلاً من IDialogService
- `ViewModels/Patients/PatientRegistrationViewModel.cs` — F7 يفتح ExternalLabsWindow
- `ViewModels/Patients/TestResultsViewModel.cs` — F7 يفتح ExternalLabsWindow
- `Tests/ViewModels/Menu/PlaceholderMenusTests.cs` — تحديث ل构造ors الجديدة

---

### Slice 6 — Test Coverage + Cleanup + Status Update

**Status:** ✅ مكتملة

**Files created:**
- `Tests/ViewModels/Settings/CompaniesWindowViewModelTests.cs` — 6 اختبارات
- `Tests/ViewModels/Settings/PriceSchemeWindowViewModelTests.cs` — 6 اختبارات
- `Tests/ViewModels/Patients/TestSelectionViewModelPricingTests.cs` — 6 اختبارات
- `Tests/Integration/InvoiceWorkflowEndToEndTests.cs` — 5 اختبارات
- `Tests/Integration/ExternalShipmentEndToEndTests.cs` — 5 اختبارات
- `Tests/Validation/CompanyContractFieldsValidationTests.cs` — 4 اختبارات

**Files modified:**
- `Services/Interfaces/IContractService.cs` — إضافة XML doc Deprecated
- `Services/Interfaces/IExternalLabService.cs` — إضافة XML doc Deprecated
- `Services/Implementations/ExternalLabService.cs` — تحويل إلى Adapter
- `Docs/PRDs/IMPLEMENTATION_STATUS.md` — تحديث هذا الملف

---

### Migration — AddCompanyContractFields

**Status:** ✅ مكتملة

**Files created:**
- `Migrations/20260626032417_AddCompanyContractFields.cs` — 3 أعمدة nullable
- `Migrations/20260626032417_AddCompanyContractFields.Designer.cs`

**Files modified:**
- `Migrations/FinalLabDbContextModelSnapshot.cs` — تحديث Snapshot

---

### Tech Debt من Phase 4

| TD | الوصف | الحالة |
|----|-------|--------|
| TD-1 | لا حماية من فاتورة مكرّرة (D8 — مخالف لـ V4 سطر 856) | مُوثَّق — مقرر في Phase 7 |
| TD-2 | إرسال الفاتورة بالإيميل مؤجّل لـ Phase 6 | مُوثَّق — مقرر في Phase 6 |
| TD-3 | Company.DiscountRate هو double (مقبول) | مُوثَّق — مقبول |
| TD-4 | IContractService و IExternalLabService لم يُحذفا — محوَّلان إلى Adapters | مُوثَّق — مقرر حذفهما في Phase 7 |

---

## Phase 5: Inventory & Cash Drawer
**Status:** ⏳ في الانتظار

---

## Phase 6: Print, Delivery & Backup
**Status:** ⏳ في الانتظار

---

## Phase 7: Specialty Editors & Admin
**Status:** ⏳ في الانتظار

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
**Status:** ✅ مكتملة  
**Date:** 2026-06-27  
**Total files created:** 38  
**Total files modified:** 14  
**Tests:** 544 / 544 — ✅ جميع الاختبارات ناجحة  
**Build:** ✅ ناجح — 0 أخطاء, 0 تحذيرات

---

### Slice 1 — Attendance Foundation

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IAttendanceService.cs` — 10 دوال (6 جديدة)
- `Services/Implementations/AttendanceService.cs` — تنفيذ الدوال الجديدة
- `ViewModels/Settings/AttendanceWindowViewModel.cs` — نموذج إدارة الحضور
- `ViewModels/Settings/AttendanceRowViewModel.cs` — نموذج صف الحضور
- `ViewModels/Settings/WorkShiftRowViewModel.cs` — نموذج صف الوردية
- `Views/Settings/AttendanceWindow.xaml` — نافذة إدارة الحضور (TabControl)
- `Views/Settings/AttendanceWindow.xaml.cs` — تهيئة النافذة
- `Tests/Services/AttendanceServiceTests.cs` — 10 اختبارات
- `Tests/ViewModels/Settings/AttendanceWindowViewModelTests.cs` — 6 اختبارات
- `Tests/Services/AttendanceServiceRegistrationTests.cs` — 1 اختبار

**Files modified:**
- `App.xaml.cs` — تسجيل IAttendanceService + VM + Window + Navigation

**Tests:** +17 (من 434 إلى 451)  
**Validation Gate G5.1:** ✅ 451

---

### Slice 2 — Cash Drawer

**Status:** ✅ مكتملة

**Files created:**
- `Models/DTOs/CashDrawerSummaryDto.cs` — DTO ملخص الدرج
- `Models/DTOs/CashDrawerFilterDto.cs` — DTO الفلتر
- `Services/Interfaces/ICashDrawerService.cs` — 6 دوال
- `Services/Implementations/CashDrawerService.cs` — مع PasswordHasher
- `Views/Settings/CashDrawerUnlockDialog.xaml` — نافذة طلب كلمة المرور
- `Views/Settings/CashDrawerUnlockDialog.xaml.cs` — تهيئة
- `Views/Settings/CashDrawerChangePasswordDialog.xaml` — نافذة تغيير كلمة المرور
- `Views/Settings/CashDrawerChangePasswordDialog.xaml.cs` — تهيئة
- `ViewModels/Settings/CashDrawerWindowViewModel.cs` — نموذج إدارة الدرج
- `Views/Settings/CashDrawerWindow.xaml` — نافذة إدارة الدرج
- `Views/Settings/CashDrawerWindow.xaml.cs` — تهيئة
- `Services/Printing/CashDrawerSummaryTemplate.cs` — قالب طباعة الملخص
- `Tests/Services/CashDrawerServiceTests.cs` — 10 اختبارات
- `Tests/ViewModels/Settings/CashDrawerWindowViewModelTests.cs` — 8 اختبارات
- `Tests/Services/CashDrawerServiceRegistrationTests.cs` — 1 اختبار

**Files modified:**
- `App.xaml.cs` — تسجيل ICashDrawerService + VMs + Windows + Navigation
- `ViewModels/Menu/AccountsMenuViewModel.cs` — استبدال Placeholder بزر Cash Drawer
- `MainWindow.xaml` — تحديث DataTemplate

**Tests:** +18 (من 451 إلى 469)  
**Validation Gate G5.2:** ✅ 469

---

### Slice 3 — Inventory + Low-Stock Alert + Barcode Warning

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IInventoryService.cs` — 6 دوال
- `Services/Implementations/InventoryService.cs` — مع IsLowStock + GetLowStockCount
- `ViewModels/Settings/InventoryWindowViewModel.cs` — نموذج إدارة المخزون
- `ViewModels/Settings/InventoryRowViewModel.cs` — نموذج صف المخزون
- `Views/Settings/InventoryWindow.xaml` — نافذة إدارة المخزون
- `Views/Settings/InventoryWindow.xaml.cs` — تهيئة
- `Views/Settings/StockAdjustmentDialog.xaml` — نافذة تعديل المخزون
- `Views/Settings/StockAdjustmentDialog.xaml.cs` — تهيئة
- `Tests/Services/InventoryServiceTests.cs` — 10 اختبارات
- `Tests/ViewModels/Settings/InventoryWindowViewModelTests.cs` — 6 اختبارات
- `Tests/ViewModels/Menu/HomeMenuViewModelLowStockTests.cs` — 4 اختبارات
- `Tests/Validation/TubeMaterialStockMigrationTests.cs` — 3 اختبارات
- `Tests/Services/InventoryServiceRegistrationTests.cs` — 2 اختبارات
- `Tests/Patients/BarcodeDialogLowStockWarningTests.cs` — 4 اختبارات

**Files modified:**
- `Models/TubeMaterial.cs` — إضافة CurrentStock + MinimumStock
- `Data/FinalLabDbContext.cs` — Fluent mapping للحقلين
- `Migrations/` — AddTubeMaterialStockFields
- `ViewModels/Menu/HomeMenuViewModel.cs` — إضافة LowStockCount
- `ViewModels/MainViewModel.cs` — تمرير IInventoryService
- `MainWindow.xaml` — low-stock banner
- `Views/Converters.cs` — LowStockTextConverter + LowStockBrushConverter
- `ViewModels/Patients/BarcodeDialogViewModel.cs` — فحص المخزون قبل طباعة الملصق
- `App.xaml.cs` — تسجيل IInventoryService + VMs + Windows + Navigation
- `Tests/Services/CashDrawerServiceTests.cs` — إصلاح 3 اختبارات (PasswordHash)

**Decision:** فتح InventoryWindow من الواجهة — مؤجّل لمرحلة لاحقة  
**Tests:** +32 (من 469 إلى 501 — يشمل إصلاح 3 اختبارات سابقة)  
**Validation Gate G5.3:** ✅ 501

---

### Slice 4 — Referral Commission Report

**Status:** ✅ مكتملة

**Files created:**
- `Models/DTOs/CommissionReportRow.cs` — DTO بـ 11 حقل
- `Services/Interfaces/ICommissionReportService.cs` — GetCommissionReportAsync
- `Services/Implementations/CommissionReportService.cs` — AsNoTracking + date filter
- `Services/Printing/CommissionReportTemplate.cs` — WPF Table template
- `ViewModels/Settings/CommissionReportWindowViewModel.cs` — Load + Print + filters
- `Views/Settings/CommissionReportWindow.xaml` — RTL UI مع DataGrid
- `Views/Settings/CommissionReportWindow.xaml.cs` — تهيئة
- `Tests/Services/CommissionReportServiceTests.cs` — 6 اختبارات
- `Tests/ViewModels/Settings/CommissionReportWindowViewModelTests.cs` — 5 اختبارات
- `Tests/Services/CommissionReportServiceRegistrationTests.cs` — 2 اختبارات

**Files modified:**
- `ViewModels/Menu/AccountsMenuViewModel.cs` — إضافة NavigateToCommissionReportCommand
- `MainWindow.xaml` — زر "تقرير العمولات"
- `App.xaml.cs` — تسجيل ICommissionReportService + VM + Window + Navigation

**Tests:** +13 (من 501 إلى 514)  
**Validation Gate G5.4:** ✅ 514

---

### Slice 5 — Outstanding Balance Report

**Status:** ✅ مكتملة

**Files created:**
- `Models/DTOs/OutstandingBalanceReportRow.cs` — DTO بـ 12 حقل
- `Services/Interfaces/IOutstandingBalanceReportService.cs` — GetOutstandingBalancesAsync
- `Services/Implementations/OutstandingBalanceReportService.cs` — AsNoTracking + date filter
- `Services/Printing/OutstandingBalanceReportTemplate.cs` — WPF Table template
- `ViewModels/Settings/OutstandingBalanceWindowViewModel.cs` — Load + Print + TotalOutstanding + filters
- `Views/Settings/OutstandingBalanceWindow.xaml` — RTL UI مع DataGrid
- `Views/Settings/OutstandingBalanceWindow.xaml.cs` — تهيئة
- `Tests/Services/OutstandingBalanceReportServiceTests.cs` — 5 اختبارات
- `Tests/ViewModels/Settings/OutstandingBalanceWindowViewModelTests.cs` — 5 اختبارات
- `Tests/Services/OutstandingBalanceReportServiceRegistrationTests.cs` — 2 اختبارات

**Files modified:**
- `ViewModels/Menu/AccountsMenuViewModel.cs` — إضافة NavigateToOutstandingBalanceCommand (زر سادس)
- `MainWindow.xaml` — زر "الأرصدة المستحقة" مع خلفية #00695C
- `App.xaml.cs` — تسجيل IOutstandingBalanceReportService + VM + Window + Navigation
- `Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs` — +2 اختبارات (CommissionReport + OutstandingBalance)

**Tests:** +14 (من 514 إلى 528)  
**Validation Gate G5.5:** ✅ 528

---

### Slice 6 — Integration Tests + Status Update

**Status:** ✅ مكتملة

**Files created:**
- `Tests/Integration/AttendanceWorkflowEndToEndTests.cs` — 4 اختبارات (ClockIn/Out, DateRange, HoursWorked, CreateShift)
- `Tests/Integration/CashDrawerEndToEndTests.cs` — 5 اختبارات (SetPassword, WrongPassword, DailySummary, ChangePassword, IsPasswordSet)
- `Tests/Integration/InventoryAlertEndToEndTests.cs` — 4 اختبارات (UpdateStock, GetLowStock, IsLowStock, GetLowStockCount)
- `Tests/Integration/Phase5BuildVerificationTests.cs` — 3 اختبارات (Services, ViewModels, Windows)

**Files modified:**
- `Docs/PRDs/IMPLEMENTATION_STATUS.md` — هذا الملف
- `Docs/PRDs/PHASE5-Plan.md` — تحديث Validation Gate + قرارات التنفيذ

**Decisions during execution:**
- `Staff.PasswordHash` مطلوب في InMemory DB — أُضيف لكل seed data
- `Payment.PaymentType` مطلوب في InMemory DB — أُضيف لـ seed data
- Window types لا يمكن الوصول إليها مباشرة في test project (`UseWPF=false`) — استخدام reflection
- `IDialogService` + `IPrintService` مُسجَّلان كـ Mock في BuildVerification

**Tests:** +16 (من 528 إلى 544)  
**Validation Gate G5.6:** ✅ 544

---

### Phase 5 — الإجمالي النهائي

| البند | العدد |
|-------|-------|
| اختبارات Phase 5 المضافة | 110 |
| إجمالي الاختبارات | **544** |
| Build | ✅ 0 أخطاء, 0 تحذيرات |
| الملفات المُنشأة | 38 |
| الملفات المُعدَّلة | 14 |

---

## Phase 6: Print, Delivery & Backup
**Status:** 🔶 جزئية — شرائح 6.0 + 6.1 + 6.2 مكتملة  
**Date:** 2026-06-30  
**Total files created:** 28  
**Total files modified:** 17  
**Tests:** 644 / 644 — ✅ جميع الاختبارات ناجحة  
**Build:** ✅ ناجح — 0 أخطاء, 0 تحذيرات

---

### Slice 6.0 — Foundation Cleanup

**Status:** ✅ مكتملة

**Files created:**
- `Services/Interfaces/IBarcodeDialogFactory.cs` — واجهة Factory لـ BarcodeDialog: `BarcodeDialogResult Show(int visitId, Window? owner)`
- `Services/Implementations/BarcodeDialogFactory.cs` — تنفيذ بنمط `_serviceProvider.GetService` (Singleton، بدون `CreateScope()`)
- `Services/Interfaces/IReceiptDialogFactory.cs` — واجهة Factory لـ ReceiptDialog: `bool Show(VisitFullDto dto, Window? owner)`
- `Services/Implementations/ReceiptDialogFactory.cs` — تنفيذ: `InitializeAsync` + فحص `CanPrint` + `ShowDialog` (Singleton)
- `Services/Interfaces/INormalRangesWindowFactory.cs` — واجهة Factory لـ NormalRangesWindow: `void Open(TestType editableTest, Window? owner)`
- `Services/Implementations/NormalRangesWindowFactory.cs` — تنفيذ: `vm.InitializeAsync(editableTest)` + `ShowDialog` (Singleton)
- `Models/Enums/BarcodeDialogResult.cs` — Enum: `Printed`, `Cancelled`
- `Tests/Services/BarcodeDialogFactoryTests.cs` — 4 اختبارات
- `Tests/Services/ReceiptDialogFactoryTests.cs` — 4 اختبارات
- `Tests/Services/NormalRangesWindowFactoryTests.cs` — 3 اختبارات
- `Tests/ViewModels/Patients/PatientRegistrationViewModelFoundationTests.cs` — 3 اختبارات

**Files modified:**
- `ViewModels/Patients/PatientRegistrationViewModel.cs` — حقن `IBarcodeDialogFactory` + `IReceiptDialogFactory` + `ILogger`؛ استبدال `App.ServiceProvider.GetRequiredService` بـ factories؛ استبدال `StaffId ?? 1` بـ `throw InvalidOperationException` في الموضعين 230 و 343؛ تحسين catch block في `InitializeAsync` مع `Exception ex` + `_logger.LogError`؛ إضافة `try/catch` في `BarcodeAsync` و `ReceiptAsync`
- `ViewModels/Settings/TestDataManagementViewModel.cs` — استبدال `App.ServiceProvider.GetRequiredService<NormalRangesWindow>` بـ `_normalRangesFactory.Open()`؛ حذف `using Microsoft.Extensions.DependencyInjection` و `using FinalLabSystem.Views.Settings`؛ تغيير `async void OnOpenNormalRangesRequested` إلى `void`
- `App.xaml.cs` — تسجيل `IBarcodeDialogFactory` → `BarcodeDialogFactory` (Singleton)، `IReceiptDialogFactory` → `ReceiptDialogFactory` (Singleton)، `INormalRangesWindowFactory` → `NormalRangesWindowFactory` (Singleton)
- `Tests/ViewModels/Patients/PatientRegistrationFKeyTests.cs` — إضافة معاملات `barcodeFactory` + `receiptFactory` + `logger` المُوهمة
- `Tests/ViewModels/Settings/TestDataManagementViewModelTests.cs` — إضافة معامل `normalRangesFactory` المُوهم

**Architectural decisions:**
- لا استخدام `CreateScope()` داخل الـ factories — اتساقاً مع نمط `IAuditTrailDialogService` و `IResultEntryDialogService` الحاليين
- `Window.GetWindow(this)` لم يُستخدم (لا يعمل في ViewModel) — الاعتماد على `Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)`
- اختبارات WPF Window لا يمكن تشغيلها في بيئة `UseWPF=false` — الاختبارات تتحقق من حلّ ViewModel من DI فقط

**Notes for Slice 6.1:**
- `IPrintService` مُسجَّل Scoped (`App.xaml.cs:193`) — عند بناء `PrintPreviewDialogService` كـ Singleton، يجب استخدام `IServiceProvider.CreateScope()` داخله لحل `IPrintService` بشكل صحيح
- نمط `CreateScope()` لم يُطبق بعد على أي خدمة حالية — إذا طُبِّق في 6.1، يجب تحديث `AuditTrailDialogService` و `ResultEntryDialogService` و الـ factories الثلاثة اتساقاً

**Tests:** +14 (من 544 إلى 558)  
**Validation Gate G6.0:** ✅ 558

---

### Slice 6.1 — PrintPreview MVVM Refactor

**Status:** ✅ مكتملة

**Files created:**
- `ViewModels/Patients/PrintPreviewViewModel.cs` — ViewModel مع `Document`, `Description`, `PrintCommand`, `CloseCommand`, `RequestClose`
- `Services/Interfaces/IPrintPreviewDialogService.cs` — واجهة خدمة فتح نافذة PrintPreview: `void Show(FlowDocument, string, Window? owner)`
- `Services/Implementations/PrintPreviewDialogService.cs` — تنفيذ مع `IServiceProvider.CreateScope()` لحل `IPrintService` (Scoped) من Singleton
- `Tests/Services/IPrintServiceExtensionTests.cs` — 4 اختبارات
- `Tests/ViewModels/Patients/PrintPreviewViewModelTests.cs` — 7 اختبارات
- `Tests/Services/PrintPreviewDialogServiceTests.cs` — 7 اختبارات
- `Tests/ViewModels/Patients/PrintPreviewWindowMvvmPurityTests.cs` — 5 اختبارات

**Files modified:**
- `Services/Interfaces/IPrintService.cs` — إضافة `Task PrintFlowDocumentAsync(FlowDocument, string)`
- `Services/Implementations/WpfFlowDocumentPrintService.cs` — إضافة `PrintFlowDocumentAsync` + استخراج `ShowPrintDialogAndPrintAsync` كـ `protected virtual` helper
- `Views/Patients/PrintPreviewWindow.xaml` — استبدال `Click=` بـ `Command="{Binding ...}"` لـ PrintButton و CloseButton + ربط `Document` بـ `DocumentViewer`
- `Views/Patients/PrintPreviewWindow.xaml.cs` — تقليص إلى constructor فقط (8 أسطر)
- `ViewModels/Patients/ReceiptDialogViewModel.cs` — حقن `IPrintPreviewDialogService` كمعامل رابع + استبدال `new PrintPreviewWindow(document)` بـ `_printPreviewDialogService.Show(document, "إيصال المريض")`
- `App.xaml.cs` — تسجيل `IPrintPreviewDialogService` (Singleton)، `PrintPreviewViewModel` (Transient)، `PrintPreviewWindow` (Transient)
- `Tests/ViewModels/ReceiptDialogViewModelTests.cs` — تحديث `CreateViewModel` Helper بإضافة `Mock<IPrintPreviewDialogService>` الرابع
- `Tests/Services/ReceiptDialogFactoryTests.cs` — تحديث إنشاء VM بإضافة `Mock<IPrintPreviewDialogService>`

**Architectural decisions:**
- `IPrintPreviewDialogService` كـ Singleton مع `CreateScope()` داخلياً — يخالف نمط 6.0 ولكنه ضروري لأن `IPrintService` مسجَّل Scoped
- اختبارات WPF Window تستخدم Mock لـ `IServiceScopeFactory` بسلسلة 4 Moq متداخلة بدلاً من إنشاء Window حقيقي (يتطلب STA thread)
- استبعاد اختبارات XAML النصية من النطاق — ليست اختبارات وحدة حقيقية

**Tests:** +24 (من 558 إلى 582)  
**Validation Gate G6.1:** ✅ 582

---

### Slice 6.2 — Backup Foundation (Service + AES + Unified LabSetting Migration)

**Status:** ✅ مكتملة

**Files created:**
- `Models/Enums/BackupType.cs` — Enum: `Full`, `Incremental`
- `Models/DTOs/BackupMetadataDto.cs` — POCO: FileName, FilePath, CreatedAt, FileSizeBytes, CreatedByStaffId, IsEncrypted, SchemaVersion
- `Infrastructure/Security/AesEncryptionHelper.cs` — AES-256-CBC + PBKDF2 100k iterations (static utility)
- `Services/Interfaces/IBackupService.cs` — 4 methods: CreateBackupAsync, RestoreBackupAsync, ListBackupsAsync, ValidateBackupFileAsync
- `Services/Implementations/BackupService.cs` — Scoped service: topological sort FK ordering, batched restore per entity type, `GetViewName() == null` for view exclusion, `ReferenceHandler.IgnoreCycles` for JSON serialization
- `Migrations/20260701000000_AddBackupAndSmtpFieldsToLabSettings.cs` — 8 columns to `LabSettings`: SmtpHost, SmtpPort, SmtpUsername, SmtpPasswordEncrypted, SmtpEnableSsl, BackupScheduleHour, BackupRetentionDays, BackupOutputFolder
- `Migrations/20260701000000_AddBackupAndSmtpFieldsToLabSettings.Designer.cs` — Designer stub
- `Tests/Infrastructure/AesEncryptionHelperTests.cs` — 10 اختبارات
- `Tests/Validation/LabSettingSmtpBackupMigrationTests.cs` — 4 اختبارات
- `Tests/Services/BackupServiceTests.cs` — 20 اختبارات
- `Tests/Services/BackupServiceRegistrationTests.cs` — 1 اختبار
- `Tests/Integration/BackupServiceIntegrationTests.cs` — 3 اختبارات

**Files modified:**
- `Models/LabSetting.cs` — إضافة 8 خصائص nullable: SmtpHost, SmtpPort, SmtpUsername, SmtpPasswordEncrypted, SmtpEnableSsl, BackupScheduleHour, BackupRetentionDays, BackupOutputFolder
- `Data/FinalLabDbContext.cs` — 8 Fluent API HasColumnName mappings داخل LabSetting entity block
- `Migrations/FinalLabDbContextModelSnapshot.cs` — إضافة 8 خصائص في LabSetting entity block
- `App.xaml.cs` — تسجيل `IBackupService` → `BackupService` (Scoped)

**Architectural decisions:**
- جدول `LabSettings` (plural) — متوافق مع snapshot القائم
- `GetViewName() == null` بدلاً من `IsView` (EF Core 8.0 لا يملك `IEntityType.IsView`)
- لا يوجد `staffId` parameter في IBackupService — الاعتماد على `_currentUserSession.CurrentUser!.StaffId`
- `JsonSerializer.ReferenceHandler.IgnoreCycles` لمعالجة circular navigation properties
- Topological sort لترتيب FK بدلاً من hardcoded table list
- Batched `SaveChangesAsync` لكل entity type مع `ChangeTracker.Clear()` بين الدفعات
- AES-256-CBC + PBKDF2 100k iterations مقبول كافية لـ local desktop backup
- تم حذف pre-restore backup من `RestoreBackupAsync` بسبب cascade failures مع InMemory provider
- تم إزالة explicit transaction من `RestoreBackupAsync` لأن InMemory provider يتجاهل المعاملات
- Fix `JsonElement` deserialization في `RestoreBackupAsync` — `JsonSerializer.Deserialize<Dictionary<string, object?>>` يُرجع `JsonElement` بدلاً من الأنواع الأصلية

**Database:** ✅ مُطبق على FinalLab (.\SQLEXPRESS) — `dotnet ef database update` بنجاح

**Tests:** +38 (من 582 إلى 620)  
**Validation Gate G6.2:** ✅ 620

---

### Slice 6.3 — Backup UI & Restore Workflow

**Status:** ✅ مكتملة

**Files created:**
- `ViewModels/Settings/BackupRowViewModel.cs` — نموذج صف النسخة الاحتياطية مع `INotifyPropertyChanged` وتنسيق Bytes/Date
- `ViewModels/Settings/BackupRestoreWindowViewModel.cs` — نموذج النافذة الرئيسية: 5 أوامر (Load, Create, Restore, BrowseFolder, OpenFolder) + `RequestShutdown` callback
- `Views/Settings/BackupRestoreWindow.xaml` — واجهة النسخ الاحتياطي والاستعادة: DataGrid + شريط أدوات + شريط حالة
- `Views/Settings/BackupRestoreWindow.xaml.cs` — code-behind: حقن ViewModel + ربط `RequestShutdown = () => navigationService.Shutdown()`
- `Views/Settings/BackupPasswordDialog.xaml` — نافذة إدخال كلمة المرور مع تأكيد (2 PasswordBox)
- `Views/Settings/BackupPasswordDialog.xaml.cs` — code-behind يتبع نمط `CashDrawerUnlockDialog`: التحقق من الفراغ +طول 8+ عدم التطابق

**Files modified:**
- `ViewModels/Menu/BackupMenuViewModel.cs` — استبدال `PlaceholderCommand` بـ `OpenBackupCommand` عبر `IDialogService.ShowCustomDialog<BackupRestoreWindow>()`
- `Services/Interfaces/IDialogService.cs` — إضافة `T? ShowCustomDialog<T>() where T : Window`
- `Services/Implementations/DialogService.cs` — حقن `IServiceProvider` + تنفيذ `ShowCustomDialog<T>` باستخدام `GetRequiredService<T>()`
- `Services/Interfaces/IBackupService.cs` — إضافة `GetBackupOutputFolderAsync()` و `SaveBackupOutputFolderAsync(string, int)`
- `Services/Implementations/BackupService.cs` — تنفيذ Methodين الجديدين: قراءة/كتابة `LabSettings` مع مفتاح `"BackupOutputFolder"`
- `App.xaml.cs` — تسجيل 3 Transient: `BackupRestoreWindowViewModel`, `BackupRestoreWindow`, `BackupPasswordDialog`
- `Tests/ViewModels/Menu/PlaceholderMenusTests.cs` — تحديث اختبار Backup من `PlaceholderCommand` إلى `OpenBackupCommand` + `ShowCustomDialog`

**Test files created:**
- `Tests/ViewModels/Settings/BackupRowViewModelTests.cs` — 3 اختبارات
- `Tests/ViewModels/Settings/BackupRestoreWindowViewModelTests.cs` — 16 اختبارات
- `Tests/ViewModels/Menu/BackupMenuViewModelTests.cs` — 1 اختبار
- `Tests/ViewModels/Settings/BackupRestoreWindowRegistrationTests.cs` — 4 اختبارات

**Architectural decisions:**
- `BackupPasswordDialog` يُنشأ بـ `new` مباشرة في الـ ViewModel (code-behind pattern) — لا يمكن تموكيته في الاختبارات
- اختبارات WPF Window (`DI_Resolves_BackupRestoreWindow`, `DI_Resolves_BackupPasswordDialog`) تتحقق من التسجيل فقط بدون إنشاء Window فعلي (يتطلب STA thread)
- `OpenFolderCommand_DoesNotThrow` يستدعي `Process.Start("explorer.exe")` فعلياً — side effect معروف يتطلب `IProcessService` abstraction في Slice لاحق

**Tests:** +24 (من 620 إلى 644)
**Validation Gate G6.3:** ✅ 644

---

## Phase 7: Specialty Editors & Admin
**Status:** ⏳ في الانتظار

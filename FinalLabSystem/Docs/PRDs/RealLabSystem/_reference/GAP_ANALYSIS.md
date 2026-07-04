# وثيقة تحليل الفجوات ومتطلبات إغلاقها — FinalLabSystem مقابل Real Lab System

**تاريخ الوثيقة:** 2026-07-04
**الكوميت المرجعي:** `c8a896796369500616c42b49dfb673159253ae4e` — "بعد إضافة وثيقة الشرح الخاصة بالنظام المرجعي"
**المستودع:** `El-ogra/FinalLabSystem`
**التقنيات المقفلة:** .NET 8 / WPF / EF Core / SQL Server / MVVM
**خارج النطاق (لا يُذكر في أي شريحة):** تكامل Natigh.com، نظام الفروع المتعدد، أي إعادة بناء لما هو موجود ويعمل.

---

## 1. ملخص تنفيذي

FinalLabSystem مشروع WPF/.NET 8 ناضج نسبياً، بُنيته سليمة والمعمارية تحترم قراراتك المعمارية (ViewModels تحمل منطق الأعمال، لا Repository Pattern، God Classes مثل `TestCatalogService` بـ 51 دالة محفوظة كما صُمّمت). البنية التحتية للمشروع تحوي:

- **955 ملف كود**، **79 موديل EF**، **58 Migration** (آخرها `AddDeliveryConfirmationFields` بتاريخ 2026-07-02).
- **26 خدمة أعمال بواجهاتها**، **71 ViewModel**، **46 نافذة XAML**.
- **110 ملف اختبار** بينهم اختبارات تكامل End-to-End (Attendance, Backup, CashDrawer, ExternalShipment, Inventory Alert, Invoice, AutoComment).
- المخطط البياني للحالة (Stage Gating عبر `ResultStageRules`) والتتبع (`AuditService` + `VResultAuditTrail`) والتحصيل (`FinancialService`) والحوكمة (`CurrentUserSession` + `StaffPermission`) — كلها موجودة وتعمل.

### نسبة الإنجاز التقديرية مقابل المرجع

| المحور | الإنجاز |
|---|---|
| موديول المرضى والزيارات (S-04 → S-13c) | **~ 82%** |
| موديول المحاسبة والمالية (S-07, S-15) | **~ 75%** |
| إدخال النتائج والتقارير (S-08, S-09, S-12, S-13) | **~ 78%** |
| كتالوج التحاليل والأسعار (S-16 → S-22) | **~ 88%** |
| موديول المزارع والحساسية (S-13a) | **~ 55%** |
| العينات الخارجية (S-14) | **~ 80%** |
| المستخدمون والصلاحيات (S-29, S-30) | **~ 70%** |
| الإحصائيات وأوراق العمل (S-24, S-25, S-26) | **~ 15%** |
| الإعدادات والنسخ الاحتياطي (S-27, S-28) | **~ 70%** |
| **الإجمالي المرجَّح** | **~ 70%** |

### أهم ثلاث فجوات إستراتيجية

1. **S-24 لوحة الإحصائيات مفقودة كلياً** — لا نافذة ولا ViewModel ولا حتى نموذج DTO. المرجع يفرد لها فصلاً كاملاً (5-1 → 5-4).
2. **الباركود ثلاثي المستويات (Case / File / Lab ID)** ينتج حالياً باركوداً واحداً بصيغة `{PatientCode}-{ordinal:D2}` — بعيدة كلياً عن مواصفة 13 خانة `1-4-100623-1-006-8` (BR-BC-001, BR-BC-002).
3. **نافذة المزارع (Culture Entry S-13a) مفقودة** — الخدمة `CultureResultService` وموديلات `MicrobiologyCulture/Organism/OrganismAntibiotic` موجودة، لكن لا نافذة WPF ولا ViewModel لإدخال Highly/Moderate/Low/Resistant وربطها بالتقرير.

---

## 2. جدول الفجوات الكامل

### 2.1 شاشات تهيئة النظام والدخول

| ID | الشاشة/الميزة | الحالة | الملفات الموجودة | ما ينقص |
|---|---|---|---|---|
| S-01 | إعدادات وبيانات السيرفر | 🟡 جزئي | `FirstRunSetupView.xaml`, `FirstRunSetupViewModel.cs` | زر "احضار" لجلب `Environment.MachineName`، تحقق اتصال SQL قبل الحفظ، رسائل تشخيصية إذا سقط الاتصال أثناء التشغيل |
| S-02 | تسجيل الدخول | ✅ مكتمل | `LoginView.xaml`, `LoginViewModel.cs`, `AuthService.LoginAsync` | (Case-sensitive مضمون بـ `PasswordHasher.Verify`) |
| S-02.1 | حذف/تعطيل حساب `admin` الافتراضي بعد التثبيت | ❌ مفقود | — | لا سياسة تُجبر المدير الأول على استبدال `admin` قبل الاستمرار |
| S-03 | القائمة الرئيسية (شريط الأزرار) | ✅ مكتمل | `MainViewModel.cs` + 8 قوائم فرعية | — |

### 2.2 موديول المرضى

| ID | الميزة | الحالة | الملفات | ما ينقص |
|---|---|---|---|---|
| S-04 | قائمة المرضى (4 أزرار F2/F4/F6/F3) | ✅ مكتمل | `PatientsMenuViewModel.cs` | — |
| S-05 | تسجيل/تعديل المريض | ✅ مكتمل | `PatientRegistrationViewModel.cs` (14.9KB) + 6 Sub-VMs (`PatientInfo`, `Financial`, `Referral`, `MedicalHistory`, `TestSelection`, `TodayPatientsDialog`) | جميع الحقول موجودة، الاختصارات F1-F12+Esc موجودة في XAML |
| S-05.1 | خانة الاسم بالأحمر للـ VIP في كل النوافذ | 🟡 جزئي | `Patient.IsVip` موجود؛ استخدام محدود | لا Style/Trigger عام يطبق `Foreground=Red` عبر ResultEntry, Delivery, Search, ExternalShipment |
| S-05.2 | ملاحظات المريض تظهر في Result Entry & Delivery | 🟡 جزئي | `Patient.Notes`, `Visit.UpdateVisitNotesAsync` موجودان | لا Binding في `TestResultsWindow.xaml` أو `DeliveryWindow.xaml` لإظهار الملاحظات كـ Popup/Banner |
| S-05.3 | زر "Lab ID" مستقل لطباعة كود المعمل الدائم | ❌ مفقود | `Patient.PatientCode` موجود | لا زر ولا Command لطباعة ملصق Lab ID فقط |
| S-05.4 | زر "إيصال" (F12) | ✅ مكتمل | `ReceiptCommand` + `ReceiptDialog.xaml` + `ReceiptTemplate.cs` + `ReceiptPrintLog` | — |
| S-05.5 | "مرضى اليوم" منسدلة سريعة | ✅ مكتمل | `TodayPatientsDialog.xaml` + `LoadTodayPatientsCommand` | — |
| S-05.6 | Auto-title بناءً على الجنس (Mr./Mrs./Miss/طفل/طفلة) | 🟡 جزئي | `Patient.Title` + `GetPatientTitlesAsync` | لا منطق auto-suggest في `PatientInfoViewModel` (المستخدم يكتب Title يدوياً) |
| S-05.7 | Age unit تلقائي (أيام/أشهر/سنوات) حسب القيمة | 🟡 جزئي | `Patient.ApproxAge`, `ApproxAgeUnit` + `NormalRange.AgeFromDays` | لا محوّل يجمع (قيمة + وحدة) إلى `AgeInDays` لمطابقة NormalRange تلقائياً — منطق التحويل يحتاج توحيد |
| S-05.8 | "Taken Outside Lab" بأنواعه الفرعية | ✅ مكتمل | `Visit.TakenOutsideLab` + `OutsideUrine/Stool/Blood/Semen/Csf` (5 أعلام) | تعليق مخصص في التقرير يعتمد على النوع (بحاجة تحقق في `ResultReportTemplate`) |
| S-05.9 | نظام الحساب Individual/Lab-to-Lab/Free | ✅ مكتمل | `Patient.PatientType` + `TestPricingEngine` + `PricingService` | — |
| S-05.10 | جهة الإحالة auto-complete + auto-save + auto-discount | ✅ مكتمل | `ReferralService.SearchReferralSourcesAsync`, `AddReferralSourceAsync`, `LinkReferralToSchemeAsync` | — |
| S-05.11 | حالات طبية (صيام، حمل، مضاد حيوي…) | ✅ مكتمل | `Visit.IsFasting/FastingHours/IsPregnant/OnAnticoagulant/HasViralInfection/HasLiverDisease` + 10 أعلام | — |
| S-06 | نافذة الباركود | 🟡 جزئي | `BarcodeDialog.xaml`, `BarcodeDialogViewModel.cs` | **نقص جوهري:** لا يوجد Case Code / File Code / Lab Code منفصلين بالبنية 13-خانة `1-4-100623-1-006-8`. الحالي `{PatientCode}-{ordinal:D2}` فقط |
| S-06.1 | Drag & Drop لدمج/فصل التحاليل بين الملصقات | ❌ مفقود | — | لا `DragDrop` handler في `BarcodeDialog.xaml.cs` |
| S-06.2 | منزلقان أفقي/رأسي لضبط طباعة الباركود + حفظ | ❌ مفقود | — | لا حقول `BarcodeHorizontalOffset/VerticalOffset` في `LabSetting` ولا Sliders في الواجهة |
| S-06.3 | باركود إضافي لحالات خاصة (تبرع دم…) | 🟡 جزئي | `SampleTube.Notes` قابل للاستخدام | لا منطق إنتاج ملصق إضافي عند طلب `CrossMatchTest` |
| S-07 | نافذة الحساب داخل S-05 (Enter مرتين، خصم %، +رسم) | 🟡 جزئي | `FinancialViewModel.cs` + `FinancialService` (7 دوال) | لم يُتحقق من دعم "Enter مرتين" (Two-Enter) في XAML، ولا زر `+` لإضافة رسوم غير مرتبطة بتحليل. `VisitCharge` موجود لكن لا يظهر واجهياً |
| S-08 | Result Entry (قائمة اليوم + رموز حالة 7) | 🟡 جزئي | `TestResultsWindow.xaml` + `TestResultsViewModel.cs` + F2-F8/F12 موجودة | **رموز الحالة السبعة كصور/أيقونات (دائرة حمراء، ورقة، سهمان، طابعة، عربة، جنيه، وسام):** لم يُلاحظ أي `DataTemplateSelector`/`ImageSource` من نوع Status Icon مربوط بحالة الزيارة |
| S-08.1 | فلاتر سريعة (لم تُكتَب/لم تُراجَع/…) | 🟡 جزئي | — | حاجة CollectionView filters داخل `TestResultsViewModel` (تحقق مطلوب) |
| S-08.2 | زرا "P" و "T" (للأدمن فقط: من فعل ماذا) | 🟡 جزئي | `AuditTrailViewModel.cs` + `AuditTrailWindow.xaml` + `IAuditService.GetResultModificationsAsync` | يوجد نافذة عامة، لكن الفصل الحاصل في المرجع بين زر "P" (بيانات المريض: من سجّل/عدّل/حصّل) وزر "T" (بيانات التحليل: من كتب/راجع/طبع/سلّم) غير موجود صراحةً في XAML |
| S-08.3 | ورقة عمل للمريض المحدد + طباعة ظرف + طباعة تاريخ مرضي + طباعة تقرير فارغ | ✅ مكتمل | `WorksheetTemplate.cs`, `EnvelopeTemplate.cs`, `MedicalHistoryTemplate.cs`, `BlankReportTemplate.cs` | ربط الأزرار في `TestResultsWindow.xaml` يحتاج تحقق |
| S-09 | Panel Value Entry | ✅ مكتمل | `ResultEntryViewModel.cs`, `ResultEntryWindow.xaml`, `ReportCommentEngine.ApplyAutoCommentAsync`, `NormalRange` بجميع حقول Low/High/Critical | — |
| S-09.1 | تعليقات محفوظة + زر إدراج تعليق مخزّن + Undo تعليق | 🟡 جزئي | `ReportCommentTemplate` (14 حقل) + `ReportCommentTemplateService` | زر تراجع (Undo) عن آخر تعليق مضاف غير مُنفَّذ صراحةً |
| S-09.2 | زر "Constants" لتعديل ثوابت الحساب (Hgb، ISI، Control Time) | ❌ مفقود | — | لا جدول `TestConstants` ولا واجهة لضبط `HgbFactor` (× 8.25 / 7.50 / 6.25 / 6.75) ولا حقول `ISI`/`ControlTime` في `TestType` |
| S-09.3 | حساب تلقائي `HCT = Hgb × 3.3` إذا لم تُدخَل HCT | ❌ مفقود | — | لا Rule Engine في `RoutineResultService.SaveNumericOrTextResultsAsync` تفحص Component=HCT missing → compute |
| S-10 | تسليم النتائج | 🟡 جزئي | `DeliveryWindow.xaml`, `DeliveryViewModel.cs`, `DeliveryConfirmationService` (Signature + OTP) | آلية "قراءة كود الإيصال ثم كود الملف → مطابقة" غير موجودة. لا حقل ScanInput ثنائي الخطوة. التنبيه بباقي الحساب موجود بشكل عام |
| S-11 | البحث (F3) | ✅ مكتمل | `PatientSearchViewModel.cs`, `PatientSearchWindow.xaml`, `SearchPatientsAsync` مع Paging | حد 100 نتيجة يحتاج تأكيد داخل `PatientService.SearchPatientsAsync` (`pageSize = 50` افتراضياً — قابل للتعديل) |
| S-11.1 | بحث بحرف البداية + بحث بحرف في أي مكان + الجمع بينهما | 🟡 جزئي | — | تحقق مطلوب من دعم النمطين (`StartsWith` + `Contains`) في نفس الاستعلام مع UI منفصل |
| S-11.2 | بحث بكود الحالة / Lab ID / كود الملف كل في حقله | 🟡 جزئي | — | حقل بحث واحد `searchTerm` يُلاحظ في `IPatientService`. المرجع يطلب حقولاً منفصلة |
| S-11.3 | نتائج مجمّعة لعدة مرضى مشتركين في تحاليل | ❌ مفقود | — | لا Command `PrintGroupedResultsCommand` في `PatientSearchViewModel` |
| S-12 | التقرير المجمَّع (Composite) | 🟡 جزئي | `CompositeReportTemplate.cs` (خدمة) | لا نافذة اختيار تحاليل + Reorder Arrows Up/Down |
| S-13 | تقرير فارغ | ✅ مكتمل | `BlankReportTemplate.cs` | ربط زر في `TestResultsWindow` |
| S-13a | تقرير المزرعة (Culture Report) | ❌ مفقود UI | خلفية: `MicrobiologyCulture` (11 حقل)، `MicrobiologyOrganism`، `OrganismAntibiotic`، `AntibioticCatalog`، `CultureResultService.GetSafeAntibioticsAsync(isPregnant, isChild)` | **لا نافذة WPF ولا ViewModel** لإدخال Sample/Organism A/B/C/Culture Condition/Colony Count. لا Grid للحساسية بأربعة مستويات (Highly/Moderate/Low/Resistant). لا Culture Report Template |
| S-13a.1 | فلترة تلقائية للمضادات للحوامل/الأطفال (<12) | ✅ مكتمل خدمياً | `CultureResultService.GetSafeAntibioticsAsync` | يحتاج ربط UI عند إنشاء نافذة الإدخال |
| S-13b | التاريخ المرضي (Patient History) | 🟡 جزئي | `VPatientHistory` view + `MedicalHistoryViewModel` + `MedicalHistoryTemplate` + `GetHistoricalComparisonsAsync` | لا خيار "Print history in separate report" vs "Append at footer" في `ReportLayoutDto`. لا زر "Patient History" في `ResultEntryWindow` يعرض عدد النتائج السابقة |
| S-13b.1 | تاريخ مرضي مخصص يدوي (اختيار زيارات) | ❌ مفقود | — | لا Selector للزيارات السابقة قبل الطباعة |
| S-13c | متابعة الحالات (Case Follow-up) | 🟡 جزئي | فلاتر Result Entry تغطي جزءاً | لا نافذة متخصصة كما في المرجع |

### 2.3 موديول العينات الخارجية والمحاسبة

| ID | الميزة | الحالة | الملفات | ما ينقص |
|---|---|---|---|---|
| S-14 | العينات المرسلة للخارج (F7) | ✅ مكتمل تقريباً | `ExternalShipmentService` + `ExternalSamplesMenuViewModel` + `ExternalShipmentEndToEndTests` + `ExternalLabsWindow.xaml` | زر "دائماً إليه" (تثبيت الجهة كافتراضية لتحليل) — يحتاج تحقق |
| S-14.1 | جعل تحليل "مرسل خارجياً" مؤقتاً على مستوى المريض دون تعديل الكتالوج | ✅ مكتمل | `VisitTest.IsOutsourced`, `ExternalLabId`, `OutsourceCost` | — |
| S-14.2 | زران لعرض كل ما لم يُرسل / لم يُسدَّد في تاريخ النظام كله | 🟡 جزئي | — | حاجة Query `GetAllUnsentEverAsync` و `GetAllUnpaidEverAsync` |
| S-15 | الجرد وحساب الدرج | ✅ مكتمل | `CashDrawerService` (6 دوال) + `CashDrawerWindowViewModel` + `CashDrawerWindow.xaml` + `CashDrawerEndToEndTests` + قفل بكلمة مرور | — |
| S-15.1 | خيار: "ما تم تحصيله من مرضى الفترة فقط" vs "أي مريض سدّد في هذه الفترة" | 🟡 جزئي | `CashDrawerFilterDto` | يحتاج تحقق من وجود العلم الثنائي (تحصيل مرضى الفترة/تحصيل في الفترة) |
| S-15.2 | إحصاء المستخدمين (المبالغ التي حصّلها كل مستخدم) | 🟡 جزئي | `Payment.ReceivedBy` + `CommissionReportService` | حاجة `GetUserCollectionsByPeriodAsync` مستقلة |
| S-15a | صرف وإيداع نقدية | ❌ مفقود | — | لا موديل `CashMovement` / `Expense` / `Deposit`، لا نافذة إدخال، لا انعكاس في `CashDrawerSummaryDto` |

### 2.4 موديول كتالوج التحاليل والأسعار

| ID | الميزة | الحالة | الملفات | ما ينقص |
|---|---|---|---|---|
| S-16 | كتالوج التحاليل | ✅ مكتمل | `TestDataManagementViewModel.cs`, `TestListViewModel.cs`, `TestType` (28 حقل يغطي ReportName/BillName/HistoryName/CollectionNotes/PatientQuestion/OutsideLabName/OutsideCostPrice) + Checkboxes `SpecialType` | تحقق: هل `SpecialType` يفصل بين `SeeReport/PrintWithOther/AddWithGroup/MainTest` بشكل قابل للتحرير في الواجهة؟ (الكلمات موجودة، الحقول محتمل) |
| S-16.1 | Sample Type / Container per test | ✅ مكتمل | `TestTypeSampleTube` + `TubeMaterial` (8 حقل بمخزون) + `CollectionType` | — |
| S-16.2 | External Sample flag + Cost Price في الكتالوج | ✅ مكتمل | `TestType.OutsideLabName`, `OutsideCostPrice` | — |
| S-16.3 | Patient Question | ✅ مكتمل | `TestType.PatientQuestion` | ربط ظهورها في `PatientRegistrationViewModel` عند اختيار التحليل — يحتاج تحقق |
| S-16.4 | Special Chars bar | ✅ مكتمل | `SpecialCharsBar.xaml` | — |
| S-17 | المعدلات الطبيعية | ✅ مكتمل | `NormalRangeWindowViewModel`, `NormalRangeDetailViewModel`, `NormalRange` (29 حقل يشمل Age/Sex/Unit/Low/High/Critical/Comments/Flags) | — |
| S-17.1 | قاعدة "6 مدخلات دنيا" لتحاليل لا تفرّق (BR-NR-002) | ❌ مفقود كتحقق | — | لا Validator يُنبِّه المستخدم إذا كان التحليل بلا 6 مدخلات (ذكور 0-120س، إناث 0-120س، ذكور 1-29ي، إناث 1-29ي، ذكور 1-11ش، إناث 1-11ش) |
| S-18 | قوائم أسعار الجهات (Price Schemes) | ✅ مكتمل | `PriceSchemeWindowViewModel`, `TestTypePrice`, `PricingService` (6 دوال) | زر "طباعة قائمة الأسعار" — يحتاج تحقق |
| S-19 | الكومنت الثابت | ✅ مكتمل | `ReportCommentTemplate` + `ReportCommentTemplateService` + `ReportCommentTemplateWindow` | — |
| S-20 | مجموعات التحاليل (Profiles) | ✅ مكتمل | `TestProfile`, `TestProfileItem`, `TestProfileWindowViewModel`, `IsActive`, `GetActiveProfilesAsync` | — |
| S-20.1 | تسعير المجموعة كحزمة (Package Pricing) | 🟡 جزئي | — | لا حقل `PackagePrice` في `TestProfile` — التسعير يُحسب بمجموع مكوّناته فقط |
| S-21 | الأطباء وجهات الإحالة | ✅ مكتمل | `ReferralSource` (13 حقل) + `ReferralService` (5 دوال) | — |
| S-22 | كتالوج المضادات الحيوية | ✅ مكتمل | `AntibioticCatalog` (6 حقل) + IsSafePregnancy/IsSafeChildren | نافذة إدارة الكتالوج (Add/Edit) — يحتاج تحقق |
| S-22.1 | كتالوج أنواع المزارع | ❌ مفقود | — | لا موديل `CultureTypeCatalog` (يُدار حالياً كنص حر في `MicrobiologyCulture.SpecimenSource`) |

### 2.5 شاشات النتائج المتقدمة والتقارير

| ID | الميزة | الحالة | الملفات | ما ينقص |
|---|---|---|---|---|
| S-23 | Report Designer | 🟡 جزئي | `ReportLayoutService` + `ReportSettingsWindowViewModel` + `LabSetting` (24 حقل تقرير) | القاعدة الحاسمة **BR-REP-001 (per-machine)**: لم أعثر على منطق يُخزّن الإعدادات لكل جهاز باستخدام `Environment.MachineName` كمفتاح. الإعدادات في `LabSetting` جدولية لكن مشتركة لجميع المحطات |
| S-23.1 | خيارات إظهار/إخفاء (Abnormal BG, Status Flag, Bold NR, Critical, History) | 🟡 جزئي | `LabSetting` يحوي بعض هذه الأعلام | تحقق كامل مطلوب من مطابقة 5 خيارات مع `LabSetting` |
| S-23.2 | ضبط ألوان رأس/ذيل التقرير في نافذة فرعية | 🟡 جزئي | `ReportPrimaryColor`, `ReportSecondaryColor` | نافذة ColorPicker فرعية لضبط الرأس/الذيل بشكل مستقل — يحتاج تحقق |
| S-24 | لوحة الإحصائيات | ❌ مفقود | `IReportingService.GetDashboardMetricsAsync` واحدة فقط ترجع object | **لا نافذة كاملة**، لا رسم بياني، لا تقارير 5-1/5-2/5-3/5-4، لا فرز بجنس/شهر، لا إحصاء إنتاجية الموظفين |
| S-25 | ورقة عمل بأسماء المرضى | 🟡 جزئي | `WorksheetTemplate.cs` | لا نافذة اختيار فترة + طباعة قائمة بالمرضى مع حالاتهم |
| S-26 | ورقة عمل بأسماء التحاليل (+ Log) | 🟡 جزئي | `WorksheetTemplate.cs` + `IReportingService.GetPendingWorksheetsAsync` | لا Log لتصنيف التحاليل حسب عدد مرات إجرائها |

### 2.6 الإعدادات والأمان والنسخ الاحتياطي

| ID | الميزة | الحالة | الملفات | ما ينقص |
|---|---|---|---|---|
| S-27 | إعدادات النظام | ✅ مكتمل | `ReportSettingsWindowViewModel`, `LabSetting` (39 حقل) | يشمل هوامش، حجم ورق، رأس/ذيل، ألوان، SMTP، Backup schedule |
| S-27.1 | طابعات مخصصة لكل نوع (تقارير/باركود/إيصال/ظرف/كارنيه) | 🟡 جزئي | — | لا 5 حقول Printer في `LabSetting` بشكل مستقل — تحقق مطلوب |
| S-27.2 | نوع الحساب الافتراضي (Individual/Contract/Commissions/Discounts) | 🟡 جزئي | `Patient.PatientType` (Individual افتراضي) | لا حقل `DefaultAccountType` في `LabSetting` |
| S-27.3 | إعدادات الظرف (Envelope) و الإيصال بتفاصيلها | ✅ مكتمل | `EnvelopeTemplate.cs`, `ReceiptTemplate.cs`, `ReceiptSettings` | — |
| S-28 | صيانة قاعدة البيانات (Backup) | 🟡 جزئي | `BackupService` (JSON-based backup لكل الجداول عبر Reflection) + `BackupRestoreWindow` + `BackupServiceIntegrationTests` | **الفجوة الجوهرية:** المرجع يستخدم `BACKUP DATABASE ... TO DISK` بصيغة `.bak` (SQL Server native)، لكن التنفيذ الحالي **JSON serialization**. اسم الملف الحالي غير مُثبت على `Patient_yyyymmdd.bak`. لا يوجد `T-SQL BACKUP/RESTORE` صريح |
| S-28.1 | كلمة مرور صيانة (افتراضياً 123) + إجبار تغييرها | 🟡 جزئي | `CashDrawer` عنده كلمة مرور مستقلة، Backup يحتاج IsAdmin فقط | لا كلمة مرور مستقلة للنسخ الاحتياطي كما في المرجع |
| S-28.2 | تصفير بيانات النظام (كلي / جزئي) | ❌ مفقود | — | لا خيار Truncate all data / حدد الجداول |
| S-28.3 | استعادة بيانات مريض واحد بكوده من قاعدة النسخ | ❌ مفقود | — | لا `RestoreSinglePatientByCodeAsync` |
| S-28.4 | تسمية الملف `Patient_yyyymmdd.bak` | ❌ مفقود | `BackupService.CreateBackupAsync` يستخدم اسم JSON | راجع BR-BAK-002 |
| S-28.5 | عرض قائمة النسخ السابقة مع الأحجام + عدّاد | ✅ مكتمل | `IBackupService.ListBackupsAsync` + `BackupMetadataDto` | — |
| S-29 | المستخدمون والصلاحيات | 🟡 جزئي | `Staff` (13 حقل، `IsAdmin`, `DiscountLimit`, `LastLoginAt`) + `Permission` + `StaffPermission` + `AuthService.CreateUserAsync/HasPermissionAsync` + `PasswordHasher` | **لا نافذة WPF واحدة** لإدارة المستخدمين (إضافة/تعديل/إعادة تعيين كلمة مرور/توزيع صلاحيات). الخدمات موجودة، لكن الواجهة مفقودة |
| S-29.1 | تحرير Permission Matrix لكل مستخدم | ❌ مفقود | Model + Service جاهزان | لا `UserManagementWindow.xaml` ولا `PermissionEditorViewModel` |
| S-30 | الحضور والانصراف | ✅ مكتمل | `AttendanceService` (10 دوال) + `AttendanceWindow.xaml` + `AttendanceServiceTests` + `WorkShift` + `Attendance.LateMinutes` | — |
| S-30.1 | فترة الراحة (Break) داخل الوردية | ❌ مفقود | `Attendance` بلا `BreakStart/End` | تحقق مطلوب — قد يُهمَل حسب أولويتك |

### 2.7 القواعد التشغيلية العابرة (Cross-cutting)

| BR | القاعدة | الحالة | الملاحظات |
|---|---|---|---|
| BR-SYS-005 | حذف حساب `admin` الافتراضي بعد إنشاء مستخدمين | ❌ مفقود | لا Enforcement |
| BR-BC-001/002/003 | باركود 13 خانة، ثلاثة أنواع (Case/File/Lab)، Lab ID دائم | ❌ مفقود | الحالي `{PatientCode}-{ordinal:D2}` (SampleTrackingService) |
| BR-RES-003 | لا طباعة قبل مراجعة، لا تسليم قبل طباعة | ✅ مكتمل | `ResultStageRules.CanPrint/CanDeliver` |
| BR-CBC-001 | HCT = Hgb × 3.3 تلقائياً | ❌ مفقود | لا Rule Engine |
| BR-CBC-002 | Hgb% بمعامل حسب النوع/العمر | ❌ مفقود | لا `TestConstants` table ولا زر Constants |
| BR-PT-CT-001/002 | ISI + Control Time + Concentration Table لـ PT/PTT | ❌ مفقود | لا حقول |
| BR-NR-002 | تعريف NormalRange بست تركيبات على الأقل | ❌ مفقود | لا Validation |
| BR-REP-001 | إعدادات التقرير Per-Machine | ❌ مفقود | جدول `LabSetting` مشترك |
| BR-BAK-001 | كلمة مرور صيانة افتراضياً `123` تُجبر على التغيير | 🟡 جزئي | `IsAdmin` فقط |
| BR-BAK-002 | اسم النسخة `Patient_yyyymmdd.bak` بجوار DB | ❌ مفقود | JSON serialization |
| BR-CULT-001 | تصنيف الحساسية Highly/Moderate/Low/Resistant | 🟡 جزئي | `OrganismAntibiotic.Sensitivity` نصي مفتوح، بلا Enum مُقيَّد |
| BR-SR-001 | حد 100 نتيجة بحث | 🟡 جزئي | `PatientService.SearchPatientsAsync` يقبل `pageSize` — لا Hard Cap |
| BR-SR-002 | الجمع بين "يبدأ بـ" و "يحتوي" | ❌ مفقود | تحقق مطلوب |
| BR-SEC-001 | زر P و T للأدمن فقط في Result Entry | 🟡 جزئي | AuditTrail موجود عامًا؛ الفصل غير واضح |

---

## 3. الشرائح المقترحة (Slices) بالترتيب

كل شريحة مستقلة، تنتج قيمة قابلة للاختبار، وتحترم القيود المعمارية (منطق الأعمال في ViewModels، لا Repository، لا Clean Architecture Refactor).

### Slice 1 — الباركود ثلاثي المستويات (Foundational — عالي الأثر)
**الأولوية:** 🔴 حرجة
**الجهد المقدَّر:** 3-4 أيام
**الأثر:** هوية النظام مقابل المرجع؛ كل سير عمل الاستقبال يعتمد عليه.

**النطاق:**
- إضافة موديل `PatientBarcode` بحقول: `CodeType (Case/File/Lab)`, `BarcodeValue (13 char)`, `IssueDate`, `SortOrdinal`, `BranchNumber`.
- بناء `BarcodeGenerator` service ينتج البنية `1-{branch}-{ddmmyy}-{weekday}-{ordinal:D3}-{caseCode}` للـ Case، وبادئات `3` و `5` للـ File و Lab.
- Lab ID يُنشأ عند أول زيارة ويُحفظ في `Patient.LabId` (حقل جديد)، ثم يُعاد استخدامه.
- تحديث `BarcodeDialogViewModel` لعرض القوائم الثلاث وطباعة كل نوع بشكل مستقل.
- طباعة ملصق Lab ID فقط عبر زر جديد `PrintLabIdCommand` في `PatientRegistrationViewModel`.

**Migration المطلوبة:** `AddPatientBarcodeAndLabId` (1 جدول جديد + 1 عمود على `Patient` + 1 عمود `BranchNumber` على `LabSetting`).

**الملفات المتأثرة:**
- `Models/PatientBarcode.cs` (جديد)
- `Models/Patient.cs` (+`LabId`)
- `Services/Implementations/SampleTrackingService.cs` (إعادة كتابة `GenerateBarcodesForVisitAsync`)
- `Services/Interfaces/IBarcodeGenerator.cs` (جديد) + `Implementations/BarcodeGenerator.cs`
- `ViewModels/Patients/BarcodeDialogViewModel.cs`
- `Views/Patients/BarcodeDialog.xaml`
- `ViewModels/Patients/PatientRegistrationViewModel.cs`
- `Data/FinalLabDbContext.cs` (DbSet + Fluent API)

**اختبارات مقترحة:** 8 اختبارات
- 4 unit لـ `BarcodeGenerator`: صيغة Case، صيغة File، صيغة Lab، إعادة استخدام Lab ID للزيارات اللاحقة.
- 2 integration لـ `SampleTrackingService`: توليد 3 أكواد لزيارة جديدة، توليد Case وحده لزيارة ثانية لنفس المريض.
- 2 ViewModel: زر "Lab ID" enabled فقط عند وجود Lab ID، الطباعة تستدعي `WpfLabelPrintService`.

**معايير القبول:**
1. مريض جديد يُنتَج له 3 أكواد بالبادئات 1/3/5 وطول 13 خانة كل واحد.
2. الزيارة الثانية لنفس المريض تُنتج Case Code جديد لكن Lab ID القديم يُقرأ من قاعدة البيانات.
3. زر مستقل لطباعة Lab ID موجود ويعمل من `PatientRegistrationWindow`.
4. جميع الاختبارات تجتاز.

---

### Slice 2 — نافذة إدخال المزرعة والحساسية (Culture Entry Window S-13a)
**الأولوية:** 🔴 حرجة
**الجهد المقدَّر:** 4-5 أيام
**الأثر:** الميكروبيولوجي كامل مسدود بلا واجهة.

**النطاق:**
- نافذة `CultureEntryWindow.xaml` تُفتَح من `ResultEntryWindow` عند التحليل من نوع Culture.
- ViewModel `CultureEntryViewModel` يحمل:
  - حقول: Sample Source، Colony Count، Culture Condition، Incubation Hours.
  - قائمة Organism A/B/C (0-3) — كل واحد بـ GramStain و ColonyCount.
  - Grid المضادات: أعمدة (Antibiotic Name, Highly, Moderate, Low, Resistant) بحيث كل صف يسمح باختيار واحد فقط.
  - الفلترة التلقائية: عند فتح النافذة، `GetSafeAntibioticsAsync(visit.IsPregnant, patient.IsChild<12)` يستدعى.
- إضافة Enum `AntibioticSensitivity` (Highly=0, Moderate=1, Low=2, Resistant=3) واستبدال `OrganismAntibiotic.Sensitivity` string بـ enum.
- `CultureReportTemplate.cs` جديدة تنتج تقرير المزرعة بالتصنيف الأربعي.

**Migration المطلوبة:** `AddCultureSensitivityEnum` + `AddCultureTypeCatalog` (اختياري إن أردت جدول أنواع المزارع).

**الملفات المتأثرة:**
- `Models/Enums/AntibioticSensitivity.cs` (جديد)
- `Models/OrganismAntibiotic.cs` (تحويل Sensitivity من string إلى enum)
- `ViewModels/Patients/CultureEntryViewModel.cs` (جديد)
- `Views/Patients/CultureEntryWindow.xaml` (جديد)
- `Services/Printing/CultureReportTemplate.cs` (جديد)
- `Services/Implementations/CultureResultService.cs` (تعزيز)
- `Infrastructure/Navigation/NavigationService.cs` (تسجيل)

**اختبارات مقترحة:** 10 اختبارات
- 3 unit لـ `CultureResultService`: فلترة للحوامل، فلترة للأطفال، فلترة مركّبة.
- 3 ViewModel: منع اختيار مستويين حساسية لنفس مضاد، حفظ Culture مع 3 كائنات ومصفوفة حساسية، Reset عند فتح مريض جديد.
- 2 integration: كتابة نتيجة مزرعة كاملة + قراءتها.
- 2 report: توليد `CultureReportTemplate` PDF بمريضة حامل vs طفل.

**معايير القبول:**
1. عند فتح مريض حامل تختفي المضادات غير الآمنة.
2. حفظ نتيجة كاملة (3 كائنات + مصفوفة حساسية 20 مضاد) يعمل ويحفظ في DB.
3. طباعة التقرير تُظهر تصنيف Highly/Moderate/Low/Resistant.
4. جميع الاختبارات تجتاز.

---

### Slice 3 — نافذة إدارة المستخدمين والصلاحيات (S-29)
**الأولوية:** 🟠 عالية
**الجهد المقدَّر:** 3 أيام
**الأثر:** الحوكمة والأمان — بدون واجهة، الإدارة تعتمد على SQL يدوي.

**النطاق:**
- `UserManagementWindow.xaml` بلوحين: قائمة المستخدمين على اليمين + محرر تفاصيل + Permission Matrix Grid.
- `UserManagementViewModel` يستهلك `AuthService` (CreateUserAsync, HasPermissionAsync) + جديد `IStaffService.UpdateStaffAsync`, `ResetPasswordAsync`, `DeactivateStaffAsync`.
- Permission Matrix: DataGrid بأعمدة (PermissionName, IsGranted) لكل مجموعة صلاحيات.
- Enforcement Rule: عند إنشاء أول مستخدم غير `admin`، يُعرض تنبيه لتعطيل حساب `admin` الافتراضي.

**Migration:** لا شيء جديد (`Permission` و `StaffPermission` موجودان).

**Seed المطلوب:** جدول `Permission` بأكواد جاهزة (PATIENT.DELETE, RESULT.OVERRIDE_STAGE, AUDIT.VIEW_TRAIL, BACKUP.RUN, USER.MANAGE...).

**الملفات المتأثرة:**
- `Services/Interfaces/IStaffService.cs` (جديد) + `Implementations/StaffService.cs`
- `ViewModels/Settings/UserManagementViewModel.cs` (جديد)
- `Views/Settings/UserManagementWindow.xaml` (جديد)
- `ViewModels/Settings/SystemSettingsMenuViewModel.cs` (زر جديد)

**اختبارات مقترحة:** 6
- 2 خدمة: `CreateStaffAsync` مع كلمة مرور مُشفَّرة، `ResetPasswordAsync` تُبطل الجلسات.
- 2 ViewModel: تفعيل/تعطيل صلاحية يستدعي `StaffPermissions.Update`، الحفظ مع Permissions ينشئ Rows.
- 2 integration: مستخدم بلا `USER.MANAGE` لا يستطيع فتح النافذة، `admin` الافتراضي يُنبَّه بحذفه عند إنشاء ثاني مستخدم.

**معايير القبول:**
1. إضافة/تعديل/حذف مستخدم يعمل من الواجهة.
2. Permission Matrix تحفظ التغييرات وتنعكس على `HasPermissionAsync`.
3. تنبيه حذف `admin` يظهر مرة واحدة.

---

### Slice 4 — قواعد بيانات الباركود Per-Machine للتقرير + النسخ الاحتياطي المطابق للمرجع
**الأولوية:** 🟠 عالية
**الجهد المقدَّر:** 4 أيام
**الأثر:** يسد قواعد BR-REP-001 و BR-BAK-002 و BR-BAK-005.

**النطاق A — تقرير Per-Machine:**
- إضافة جدول `MachineReportSettings` بمفتاح مركب `(MachineName, LayoutKey)` يحمل نسخة من `ReportLayoutDto`.
- تعديل `ReportLayoutService.GetCurrentLayoutAsync` ليقرأ أولاً من `MachineReportSettings` بـ `Environment.MachineName`، ثم يقع إلى `LabSetting` كافتراضي.

**النطاق B — Backup مطابق:**
- إضافة `SqlBackupService` (بديل / مكمل لـ `BackupService`) يستخدم T-SQL `BACKUP DATABASE [db] TO DISK = @path`.
- تسمية الملف `Patient_yyyymmdd.bak` ومسار افتراضي بجوار DB.
- كلمة مرور مستقلة `BackupPassword` في `LabSetting` (Hashed) + إجبار تغييرها من الافتراضية `123`.
- `RestoreSinglePatientByCodeAsync` عبر فتح نسخة `.bak` في قاعدة `staging` والنسخ الانتقائي (يتطلب صلاحيات SQL).
- **ملاحظة:** الإبقاء على `BackupService` الحالي كتصدير JSON مساعد للتنقل بين البيئات.

**Migration:** `AddMachineReportSettingsAndBackupPassword` (1 جدول + 2 عمود على `LabSetting`).

**الملفات المتأثرة:**
- `Models/MachineReportSetting.cs` (جديد)
- `Services/Implementations/SqlBackupService.cs` (جديد) + `ISqlBackupService`
- `Services/Implementations/ReportLayoutService.cs` (تعديل)
- `ViewModels/Settings/BackupRestoreWindowViewModel.cs` (إضافة كلمة مرور + خيار SQL vs JSON)
- `Views/Settings/BackupPasswordDialog.xaml` (موجود ويحتاج ربطاً بـ `LabSetting.BackupPasswordHash`)

**اختبارات مقترحة:** 8
- 3 لـ ReportLayout: قراءة Per-Machine، fallback إلى Default، تحديث لا يؤثر على Machines أخرى.
- 3 لـ SqlBackup: توليد اسم `Patient_yyyymmdd.bak`، فشل بدون كلمة مرور، Restore يعيد بيانات مريض واحد.
- 2 UI: إجبار تغيير كلمة مرور `123` عند أول استخدام.

**معايير القبول:**
1. تعديل ألوان التقرير على جهاز A لا يظهر على جهاز B.
2. Backup يُنتِج ملف `.bak` بالاسم الصحيح.
3. Restore Single Patient يعمل من ملف `.bak` احتياطي.

---

### Slice 5 — لوحة الإحصائيات (S-24)
**الأولوية:** 🟠 عالية
**الجهد المقدَّر:** 4 أيام
**الأثر:** يكشف أداء المعمل — مفقود كلياً حالياً.

**النطاق:**
- `StatisticsWindow.xaml` بأربعة تبويبات (Tabs): 5-1 Performance، 5-2 Yearly Patients، 5-3 Monthly Patients، 5-4 Test Frequency.
- استخدام `LiveCharts2` (مكتبة WPF مجانية) أو `OxyPlot.Wpf` — قرارك.
- `StatisticsService` جديدة تجمع بيانات من `Visit`, `VisitTest`, `Payment`, `Staff`.

**Migration:** لا شيء (استعلامات فقط).

**الملفات المتأثرة:**
- `Services/Interfaces/IStatisticsService.cs` (جديد) + `Implementations/StatisticsService.cs`
- `ViewModels/Settings/StatisticsWindowViewModel.cs` (جديد)
- `Views/Settings/StatisticsWindow.xaml` (جديد)
- `Models/DTOs/StatisticsDtos.cs` (جديد — 4 DTOs)

**اختبارات مقترحة:** 6
- 4 لـ StatisticsService: عدد المرضى سنوياً، شهرياً، معدل تحليل معين، إنتاجية موظف.
- 2 ViewModel: تغيير Tab يحدّث السلاسل، تصدير PDF.

**معايير القبول:**
1. عرض رسم بياني شهري للمرضى في سنة مختارة.
2. عرض معدل طلب تحليل مختار عبر الفترة.
3. تقرير إنتاجية موظفين للفترة.

---

### Slice 6 — رموز الحالة السبعة في Result Entry (S-08)
**الأولوية:** 🟡 متوسطة
**الجهد المقدَّر:** 1-2 يوم
**الأثر:** UX تشغيلي — الفنيون يعتمدون على الرموز البصرية.

**النطاق:**
- إضافة `DataTemplateSelector` أو `IValueConverter` يُحوّل `TodayPatientWithStatusDto` إلى Icon:
  1. دائرة حمراء — New بلا نتائج
  2. ورقة — نتائج لم تُكتَب
  3. سهمان — نتائج لم تُراجَع
  4. طابعة — نتائج لم تُطبَع
  5. عربة تسوق — نتائج لم تُسلَّم
  6. LE — باقي حساب
  7. وسام — مكتمل
- 7 أيقونات SVG/PNG في `Resources/StatusIcons/`.
- تحديث `TestResultsWindow.xaml` DataGrid لعرض الأيقونة في العمود الأول.
- تعزيز `TodayPatientWithStatusDto` بحقل `VisitDerivedStatus` enum إن لم يكن موجوداً.

**Migration:** لا شيء.

**الملفات المتأثرة:**
- `Views/Converters.cs` (إضافة `PatientStatusToIconConverter`)
- `Views/Patients/TestResultsWindow.xaml`
- `Models/DTOs/TodayPatientWithStatusDto.cs` (تعزيز)
- `Services/Implementations/VisitService.cs` (منطق تحديد الحالة)

**اختبارات مقترحة:** 7 (واحد لكل حالة)

**معايير القبول:**
1. مريض جديد → دائرة حمراء.
2. مريض دفع كامل ونتائجه مطبوعة ومسلَّمة → وسام.
3. مريض عليه باقي حساب رغم اكتمال النتائج → LE.

---

### Slice 7 — نظافة UX: VIP في كل النوافذ + ملاحظات المريض + Auto Age Unit
**الأولوية:** 🟡 متوسطة
**الجهد المقدَّر:** 1-2 يوم
**الأثر:** تحسين UX حاسم لكن غير معطّل.

**النطاق:**
- `Style` عام في `SharedStyles.xaml` يقلب Foreground=Red لعنصر Name إذا `Patient.IsVip = true` — بربط `DataTrigger`.
- تحديث 4 نوافذ لاستخدام Style: `TestResultsWindow`, `DeliveryWindow`, `PatientSearchWindow`, `ExternalLabsWindow`.
- Popup ملاحظات يظهر تلقائياً في `TestResultsWindow` و `DeliveryWindow` عند اختيار مريض بملاحظات.
- Converter `AgeToUnitConverter`: يقرأ `Patient.DateOfBirth` أو `ApproxAge+ApproxAgeUnit` ويرجع (Days/Months/Years) الأنسب. تحديث `PatientInfoViewModel` لاستخدامه.

**Migration:** لا شيء.

**الملفات المتأثرة:**
- `Views/Shared/SharedStyles.xaml`
- 4 XAML للنوافذ المذكورة
- `Views/Converters.cs`
- `ViewModels/Patients/PatientInfoViewModel.cs`

**اختبارات مقترحة:** 4

**معايير القبول:**
1. مريض VIP يظهر اسمه أحمر في كل نافذة تعرضه.
2. Popup ملاحظات يظهر عند تحديد المريض في Result Entry.
3. مريض عمره 15 يوم يُعرض بوحدة "يوم" تلقائياً.

---

### Slice 8 — الحساب داخل S-05: Enter مرتين + رسم غير مرتبط بتحليل
**الأولوية:** 🟡 متوسطة
**الجهد المقدَّر:** 1 يوم
**الأثر:** توافق سلوكي مع المرجع.

**النطاق:**
- في `FinancialSectionView.xaml`: `TextBox` لخانة "المبلغ المدفوع" يُعالج `KeyDown` بحيث Enter الأول ينفّذ CalculateSubtotal ويترك التركيز في نفس الخانة، Enter الثاني يستدعي SaveCommand.
- زر `+` بجانب حقل الإجمالي يفتح Popup صغير لإضافة `VisitCharge` جديد (اسم، مبلغ). الموديل `VisitCharge` موجود.
- عرض `VisitCharges` في نفس القسم مع إمكانية حذفها قبل الحفظ.

**Migration:** لا شيء (`VisitCharge` مُنشَأ سابقاً).

**الملفات المتأثرة:**
- `Views/Patients/FinancialSectionView.xaml`
- `Views/Patients/FinancialSectionView.xaml.cs`
- `ViewModels/Patients/FinancialViewModel.cs`

**اختبارات مقترحة:** 3

**معايير القبول:**
1. Enter الأول يحسب المطلوب، Enter الثاني يحفظ.
2. رسم إضافي يظهر في الإيصال.

---

### Slice 9 — قواعد النتائج الخاصة: Constants + HCT + PT/PTT
**الأولوية:** 🟡 متوسطة
**الجهد المقدَّر:** 3 أيام
**الأثر:** دقة سريرية لتحاليل CBC و التخثر.

**النطاق:**
- جدول جديد `TestConstant` بمفتاح `(TestTypeId, ConstantKey)` وقيمة `decimal Value` (مثال: `HGB_FACTOR_MALE_ADULT = 6.25`).
- Seed بالقيم الافتراضية للمرجع (4 معاملات Hgb).
- زر `Constants` في `ResultEntryWindow` لبعض التحاليل — يفتح Dialog لتعديل القيم.
- Rule Engine في `RoutineResultService.SaveNumericOrTextResultsAsync`:
  - إذا Component=HCT فارغ و Hgb موجود → `HCT = Hgb * 3.3`.
  - إذا Component=Hgb% فارغ → `Hgb * FactorByAgeSex`.
  - PT: إذا `INR` مطلوب، اقرأ `ISI` و `Control Time` من TestConstants.

**Migration:** `AddTestConstantsTable`.

**الملفات المتأثرة:**
- `Models/TestConstant.cs` (جديد)
- `Data/FinalLabDbContext.cs`
- `Services/Implementations/RoutineResultService.cs` (Rule Engine)
- `ViewModels/Patients/ResultEntryViewModel.cs` (زر Constants)
- `Views/Patients/ConstantsDialog.xaml` (جديد)

**اختبارات مقترحة:** 6
- 4 لـ Rule Engine: HCT auto-fill، Hgb% للأطفال، Hgb% ذكر بالغ، Hgb% أنثى بالغة.
- 2 UI: زر Constants يظهر فقط للتحاليل ذات constants.

**معايير القبول:**
1. إدخال Hgb=12 يحسب HCT=39.6 تلقائياً.
2. Hgb% للأطفال (<12 سنة) = 12 × 7.5 = 90.

---

### Slice 10 — إغلاق الفجوات الصغيرة (Sweep)
**الأولوية:** 🟢 منخفضة
**الجهد المقدَّر:** 3-4 أيام
**الأثر:** تنظيف نهائي.

**النطاق:**
- **Cash Movements (S-15a):** موديل `CashMovement` (Expense/Deposit) + نافذة إدخال + انعكاس في `CashDrawerSummaryDto`.
- **Composite Report Window (S-12):** نافذة اختيار تحاليل + Reorder Up/Down.
- **BR-NR-002 Validator:** عند حفظ تحليل جديد بلا 6 مدخلات NormalRange، عرض تحذير غير حاجب.
- **BR-SR-002 Search:** إضافة حقلين `StartsWith` و `Contains` منفصلين في PatientSearchWindow.
- **BR-SR-001:** hard-cap 100 في `PatientService.SearchPatientsAsync`.
- **BR-BC-004 Drag & Drop:** في BarcodeDialog (اختياري — يعتمد على Slice 1).
- **Grouped Results (S-11.3):** زر جديد في `PatientSearchWindow` لطباعة نتائج عدة مرضى مشتركين.
- **Package Pricing (S-20.1):** حقل `PackagePrice` في `TestProfile` واستخدامه في `PricingService`.

**Migration:** `AddCashMovementsAndPackagePricing` + `AddNormalRangeValidatorFlag` (اختياري).

**اختبارات مقترحة:** 10

**معايير القبول:** كل عنصر يمر باختبار End-to-End مستقل.

---

## 4. جدول الأولويات الملخّص

| الشريحة | الأولوية | الجهد | Migrations | اختبارات |
|---|---|---|---|---|
| S-1 الباركود ثلاثي المستويات | 🔴 حرجة | 3-4 ي | 1 | 8 |
| S-2 نافذة المزرعة والحساسية | 🔴 حرجة | 4-5 ي | 1-2 | 10 |
| S-3 إدارة المستخدمين والصلاحيات | 🟠 عالية | 3 ي | 0 (Seed) | 6 |
| S-4 Per-Machine + Backup مطابق | 🟠 عالية | 4 ي | 1 | 8 |
| S-5 لوحة الإحصائيات | 🟠 عالية | 4 ي | 0 | 6 |
| S-6 رموز الحالة السبعة | 🟡 متوسطة | 1-2 ي | 0 | 7 |
| S-7 VIP + Notes + Age Unit | 🟡 متوسطة | 1-2 ي | 0 | 4 |
| S-8 Enter مرتين + Extra Charge | 🟡 متوسطة | 1 ي | 0 | 3 |
| S-9 Constants + HCT + PT/PTT | 🟡 متوسطة | 3 ي | 1 | 6 |
| S-10 Sweep (10 عناصر صغيرة) | 🟢 منخفضة | 3-4 ي | 1-2 | 10 |
| **الإجمالي** | | **~ 30 يوم** | **6-8** | **68** |

---

## 5. ملاحظات معمارية (احتراماً لقراراتك)

1. **God Classes مُبقاة:** `TestCatalogService` بـ 51 دالة يبقى كما هو. لن نجرّئ إلى `TestTypeService`, `ProfileService`, `NormalRangeService`. أي إضافة تُضاف عليه.
2. **منطق الأعمال في ViewModels:** الشرائح تلتزم بهذا — مثلاً حساب Age Unit في `PatientInfoViewModel` مباشرة لا في خدمة Domain.
3. **لا Repository Pattern:** كل شريحة تستخدم `FinalLabDbContext` مباشرة داخل الخدمات (كما هو الحال في `AttendanceService`, `BackupService`, ...).
4. **EF Core Migrations قابل للتطبيق التصاعدي:** كل شريحة تُنتج Migration واحدة على الأكثر أو اثنتين، ومسمّاة بوصف صريح.

---

## 6. المخاطر والاعتبارات

1. **Slice 4 — SQL Backup:** يتطلب صلاحيات `db_backupoperator` على SQL Server. يجب توثيق ذلك في README.
2. **Slice 2 — تحويل `OrganismAntibiotic.Sensitivity` من string إلى enum:** Migration يحتاج data seed للـ enum أو استبقاء string parallel لفترة.
3. **Slice 1 — تغيير صيغة الباركود:** الزيارات القديمة سترى الصيغة القديمة في التقارير القديمة — قد نحتاج إلى ترحيل باركود يدوي لأول مرة، أو قبول أن كل زيارة جديدة فقط تحصل على الصيغة الجديدة.
4. **Slice 5 — مكتبة الرسوم البيانية:** LiveCharts2 vs OxyPlot — قرار يُتخذ في بداية الشريحة، الأول أحدث والثاني أنضج.

---

*انتهت الوثيقة.*
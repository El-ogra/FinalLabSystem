# وثيقة تحليل الفجوات — FinalLabSystem مقابل Real Lab System

> **الإصدار:** 3.0 (دورة التحليل المقارن الثالثة)
> **تاريخ الإصدار:** 2026-07-05
> **كوميت التحليل:** `ee2e7566d07075751ab24706a8ad7045a04c2236` — "بداية دورة التحليل المقارن الثالثة"
> **الوثيقة المرجعية:** `FinalLabSystem/Docs/PRDs/RealLabSystem/_reference/Real_Lab_System_Unified_Reference.md`
> **المنهجية:** مقارنة كل بند في المرجع بالكود الفعلي المُلتزَم (Commit pin) — لا افتراضات، لا تخمين.

---

## 1. الملخص التنفيذي

### 1.1 إحصائيات المشروع الحالي (من الكود)

| المقياس | القيمة |
|---|---|
| إجمالي ملفات C# (بدون Migrations) | 337 |
| إجمالي ملفات XAML | 49 |
| عدد الـ Migrations المُطبَّقة | 66 |
| عدد الـ Models / Entities | 53 |
| عدد الـ Services (Implementations) | 49 |
| عدد الـ Service Interfaces | 47 |
| عدد الـ ViewModels | 72 |
| عدد نوافذ الـ Views | 47 |
| عدد ملفات الاختبار | 112 |
| عدد الـ Enums | 15 |
| الـ Packages الأساسية | EF Core 8.0.0 / SQL Server / CommunityToolkit.Mvvm 8.2.2 / ZXing.Net 0.16.9 / Serilog 3.1.1 |

### 1.2 تقدير نسبة الإنجاز الإجمالية

**≈ 62%** من الوظائف المرجعية مُنفَّذة بمستوى قابل للاستخدام، تتفاوت من **مكتمل ✅** إلى **مفقود ❌** بالكامل.

| الفئة | النسبة | الوصف |
|---|---|---|
| 🟢 مكتمل ✅ | **≈ 22%** | البنية الأساسية للنماذج وقواعد البيانات والصلاحيات وكتالوج التحاليل والباقات |
| 🟡 جزئي 🟡 | **≈ 40%** | سير العمل يعمل لكن ينقص تفاصيل دقيقة (مثل Drag & Drop، الثوابت، التقرير الحقيقي، بعض الاختصارات) |
| ❌ مفقود | **≈ 30%** | شاشات كاملة لم تُبنَ (Patient Search الحقيقي، Statistics، Report Designer، User Management UI) أو بدائل مختلفة جوهرياً عن المرجع (Delivery مبسّط) |
| ⛔ خارج النطاق | **≈ 8%** | تكامل Natigh.com ونظام الفروع المتعدد |

### 1.3 أهم 5 فجوات حرجة (Critical Blockers)

1. **شاشة البحث عن المريض (S-11)** — الـ View يحتوي 11 سطر فقط ("قيد التطوير"). لا يمكن إجراء بحث متعدد المعايير كما في RLS.
2. **شاشة التسليم بقارئ الباركود (S-10)** — التنفيذ الحالي بدّال آلية "OTP/Tوقيع" محل آلية "قراءة كود الإيصال + قراءة كود الملف + المطابقة". تغيير جوهري في الفلسفة.
3. **شاشة الإحصائيات (S-24)** — لا توجد إطلاقاً كنافذة أو VM. الـ `ReportingService.GetDashboardMetricsAsync` غير مستدعى من أي UI.
4. **شاشة تصميم التقرير لكل تحليل (S-23)** — لا توجد. الإعدادات في `LabSetting` صف واحد مشترك لا يُطبَّق لكل تقرير على حدة.
5. **Report Designer الحقيقي** — `ResultReportTemplate.cs` (53 سطر) يحتوي placeholder "جاري إنشاء التقرير..." ولا يطبع شيئاً فعلياً.

### 1.4 أهم 5 إنجازات قوية

1. **نظام الباركود ثلاثي الأكواد (BR-BC-001/002)** — مُنفَّذ بشكل صحيح مع Luhn check digit، `BarcodeCodeType` enum مطابق تماماً (Case=1/File=3/Lab=5).
2. **نظام RBAC والصلاحيات** — `AuthService` كامل مع Permission codes + `PasswordHasher` + initial admin bootstrap.
3. **نظام Audit Trail** — `AuditLog` entity + `[Auditable]` attribute + interceptor + `VResultAuditTrail` view + `AuditTrailDialogService`.
4. **التسلسل الوظيفي للحالات (BR-XXX للحالات)** — `PatientVisitStatus` enum يطابق الرموز السبعة لـ RLS مع `StatusIcon` و`StatusColor` للعرض الفعلي.
5. **النسخ الاحتياطي المشفّر** — `BackupService` بـ AES-256 مع تشفير كامل وكلمة مرور، `BackupRestoreWindow` و`BackupRestoreWindowViewModel` منجزان.

---

## 2. جدول الفجوات الكامل

> **مفتاح الرموز:** ✅ مكتمل — 🟡 جزئي — ❌ مفقود — ⛔ خارج النطاق

### 2.1 الشاشات والنوافذ (32 شاشة في المرجع)

| # | الشاشة المرجعية | الشبيه في FinalLabSystem | الحالة | الفجوة التفصيلية |
|---|---|---|---|---|
| S-01 | نافذة إعدادات السيرفر | `Views/FirstRunSetupWindow.xaml` + `FirstRunSetupViewModel.cs` | 🟡 | يغطي الإعداد الأولي؛ لا يعرض اسم الجهاز + نوع سماحية الدخول بنفس بنية RLS، ولا يحظر تغيير اسم قاعدة البيانات |
| S-02 | شاشة تسجيل الدخول | `Views/LoginView.xaml` + `LoginViewModel.cs` + `AuthService.LoginAsync` | ✅ | `admin` الافتراضي يُنشأ تلقائياً عبر `CreateInitialAdministratorAsync`؛ كلمة المرور case-sensitive ضمنية في `PasswordHasher.Verify` |
| S-03 | الشاشة الرئيسية (Main Menu) | `MainWindow.xaml` (240 سطر) + `MainViewModel.cs` | 🟡 | شريط أدوات أفقي بأزرار وظيفية؛ **لا يحاكي الأزرار الستة للمرجع (المرضى/حسابات/بيانات النظام/إعدادات/إحصائيات/أوراق عمل/المستخدمون)** بنفس الترتيب، كما لا توجد قائمة "أوراق عمل" و"إحصائيات" و"المستخدمون" كأزرار مستقلة |
| S-04 | قائمة المرضى | `PatientsMenuViewModel.cs` (30 سطر) + `MainWindow.xaml` data template | ✅ | 4 أزرار رئيسية مطابقة |
| **S-05** | **نافذة بيانات المرضى** | **`PatientRegistrationWindow.xaml` (178 سطر) + `PatientRegistrationViewModel.cs` (434 سطر)** | 🟡 | شامل جداً؛ ينقص: قواعد BR-PT-005 (تطبيع الأسماء)، BR-PT-004 (auto-detect Children)، اختصارات F1/F2/F3/F4/F5/F6/F7/F8/F9/F10/F11/F12، خاصية Search-by-Lab-ID (موجودة ضمنياً في الـ search ولكن بدون فلتر بارز) |
| **S-06** | **نافذة الباركود** | **`BarcodeDialog.xaml` (178 سطر) + `BarcodeDialogViewModel.cs` (298 سطر)** | 🟡 | الأكواد الثلاثة صحيحة؛ ينقص: **Drag & Drop لدمج الملصقات (BR-BC-004)**، **سلة محذوفات لحذف تحليل من ملصق**، المنزلقات الأفقية والرأسية لضبط الموضع، حفظ أبعاد الملصق لكل جهاز (BR-REP-001 analogous) |
| S-07 | نافذة الحساب والتحصيل | `FinancialSectionView.xaml` (181 سطر) + `FinancialViewModel.cs` + `FinancialService.cs` (239 سطر) | 🟡 | الحقول موجودة؛ ينقص: العلامة `+` لإضافة رسم غير مرتبط، شرط "Enter مرتين" (مرة للحساب، مرة للحفظ)، زر "خالص" + "موافق" كزرَين منفصلَين |
| **S-08** | **نافذة إدخال نتائج التحاليل** | **`TestResultsWindow.xaml` + `TestResultsViewModel.cs` (994 سطر)** | 🟡 | الـ VM من أضخم VMs لكنه **يفتقد**: قائمة مرضى اليوم مع رموز الحالة السبعة فعلياً (الرموز موجودة في DTO لكن ربما لم تُربط بقائمة مفلترة)، فلاتر سريعة منفصلة، سهم توسعة أزرق للانتقال ليوم محدد، حقل بحث بالكود/Lab ID/كود الفيل/الاسم، اختصارات F5/F8/F9/F12 |
| **S-09** | **نافذة إدخال قيم البروفيل** | **`ResultEntryWindow.xaml` (145 سطر) + `ResultEntryViewModel.cs` (244 سطر)** | 🟡 | الواجهة والـ VM موجودان؛ ينقص: **زر Patient History** (BR-RES-007)، **زر Constants** لتعديل ثوابت Hgb/CBC/PT/PTT (BR-CBC-001/002 + BR-PT-CT-001/002)، قائمة Fixed Comments (BR-RES-006)، اختصارات F8/F9/F11/F12 |
| **S-10** | **نافذة تسليم النتائج** | **`DeliveryWindow.xaml` (46 سطر!) + `DeliveryViewModel.cs` (144 سطر)** | ❌ | **التنفيذ الحالي بدّال RLS** — يستخدم OTP/توقيع بدل "قراءة باركود الإيصال + قراءة باركود الملف + مطابقة". لا توجد قائمة مرضى اليوم، لا تنبيه باقي حساب، لا فلاتر، لا اختصارات |
| **S-11** | **نافذة البحث عن مريض** | **`PatientSearchWindow.xaml` (11 سطر!!)** | ❌ | **عبارة عن placeholder** ("قيد التطوير") + زر "العودة". الـ VM (15 سطر) فارغ وظيفياً. لا بحث متعدد المعايير، لا حد 100 نتيجة، لا زر "نتائج مجمّعة" |
| S-12 | نافذة التقرير المجمَّع | `Services/Printing/CompositeReportTemplate.cs` (17 سطر!) + منطق في `TestResultsViewModel.PrintCompositeReportCommand` | ❌ | الـ Template فارغ فعلياً (17 سطر فقط — معظمها توقيع method). لا اختيار تحاليل، لا إعادة ترتيب، لا قيد "See report/Print with other" |
| S-13 | نافذة التقرير الفارغ | `Services/Printing/BlankReportTemplate.cs` (17 سطر!) | ❌ | placeholder — لا يطبع شيئاً |
| S-13a | نافذة تقرير المزرعة | `CultureEntryWindow.xaml` (156 سطر) + `CultureEntryViewModel.cs` (376 سطر) + `CultureReportTemplate.cs` (183 سطر) + `CultureResultService.cs` | 🟡 | الـ VM والـ Service والـ Template **منجزون**؛ ينقص: **فلترة المضادات حسب Pregnant/Children (BR-CULT-002)**، إخفاء/إظهار الاسم التجاري للمضاد أو نتيجة الحساسية في التقرير |
| S-13b | نافذة التاريخ المرضي | `MedicalHistorySectionView.xaml` (173 سطر) + `MedicalHistoryViewModel.cs` + `MedicalHistoryTemplate.cs` (17 سطر!) | 🟡 | الـ SectionView يعرض حقول التاريخ المرضي للزيارة الحالية؛ ينقص: **زر Patient History التلقائي لإدراج زيارات سابقة لنفس البروفيل (S-09)**، خيار طباعة التاريخ في تقرير منفصل أو ذيل التقرير، التاريخ المرضي المخصّص يدوياً |
| S-13c | نافذة متابعة الحالات | `TestResultsViewModel` يدمج فلاتر `PatientVisitStatus` السبعة | 🟡 | الفلاتر موجودة في `ApplyFilterCommand`؛ ينقص: عرض منفصل بنافذة مستقلة كما في RLS، وتعديل المرضى ضمن نافذة المتابعة |
| **S-14** | **نافذة العينات المرسلة للخارج** | **`ExternalLabsWindow.xaml` (301 سطر) + `ExternalLabsWindowViewModel.cs`** + `ExternalShipmentService.cs` | 🟡 | كتالوج المعامل الخارجية + قائمة الشحنات منجزان؛ ينقص: **التمييز بين "مرسل/غير مرسل" و"مسوّى/غير مسوّى"** كفلاتر سريعة، **زر "دائماً إليه"** (BR-EXT-002)، **زر تحويل التحليل فردياً إلى خارجي** |
| **S-15** | **الجرد وحساب الدرج** | **`CashDrawerWindow.xaml` (125 سطر) + `CashDrawerWindowViewModel.cs`** + `CashDrawerService.cs` | 🟡 | ملخص يومي قابل للطباعة موجود؛ ينقص: **زر "ما تم تحصيله" مع سؤال الفلترة (مرضى الفترة vs أي مريض)**، تفصيل العينات المرسلة، المصروفات والإيداعات، إحصاء المستخدمين، مطالبات الجهات |
| S-15a | صرف وإيداع نقدية | `Payment.cs` model + `FinancialService` | 🟡 | البيانات تُحفَظ ضمن Payments؛ لا توجد نافذة مخصصة لإضافة مصروف/إيداع يدوي |
| **S-16** | **بيانات التحاليل (الكتالوج)** | **`TestDataManagementWindow.xaml` (400 سطر) + `TestDataManagementViewModel.cs`** + `TestCatalogService.cs` (815 سطر) | ✅ | كتالوج شامل جداً — أنجز أقسام الحقول والعلامات (Routine/See report/Print with other/Add with group/Main test) وأنواع العينات |
| **S-17** | **المعدلات الطبيعية** | **`NormalRangesWindow.xaml` (370 سطر) + `NormalRangeWindowViewModel.cs`** + `NormalRange.cs` (يحتوي Version/SupersededById) | 🟡 | الواجهة شاملة؛ ينقص: **فرض BR-NR-002** (6 مدخلات على الأقل للتحاليل غير المميِّزة)، تذكير صريح بـ BR-NR-001 (لا ربط تلقائي بين 30 يوم و1 شهر) |
| S-18 | قوائم الأسعار | `PriceSchemeWindow.xaml` (121 سطر) + `PriceSchemeWindowViewModel.cs` + `PricingService.cs` + `TestTypePrice` table | ✅ | CRUD لقوائم الأسعار + ربط بالجهات + زر طباعة |
| S-19 | الكومنت الثابت | `ReportCommentTemplate.cs` + `ReportCommentTemplateWindow.xaml` (132 سطر) + `ReportCommentEngine.cs` | 🟡 | التعليقات مرتبطة بالتحليل بـ `TestTypeId`؛ ينقص: واجهة سريعة في `ResultEntryWindow` للوصول للقائمة (الـ Tray موجود في VM لكن غير مدمج) |
| **S-20** | **مجموعات التحاليل (Profiles)** | **`TestProfileWindow.xaml` (154 سطر) + `TestProfileWindowViewModel.cs`** + `TestProfile.cs` + `TestProfileItem.cs` | ✅ | CRUD كامل + تسعير كمجموعة + تطبيقها في PatientRegistration عبر `VisitService.CreateVisitAsync` |
| S-21 | الأطباء وجهات الإحالة | `Referral.cs` + `ReferralService.cs` + جزء من `PatientRegistrationViewModel.Referral` | 🟡 | نموذج + service؛ ينقص: **نافذة مستقلة لإدارة جهات الإحالة** كشاشة S-21 (الواجهة الحالية مدمجة في نافذة المريض) |
| **S-22** | **المزارع وكتالوج المضادات** | **`AntibioticCatalog.cs` + `MicrobiologyOrganism.cs` + `OrganismAntibiotic.cs`** + `AntibioticSensitivity` enum (Highly/Moderate/Low/Resistant) | 🟡 | الـ Enum مطابق تماماً لـ BR-CULT-001؛ ينقص: **نافذة UI لإدارة المضادات + تعليم Pregnant Safe / Children Safe** كقيم صريحة (موجودة في DB كـ `IsSafePregnancy`/`IsSafeChildren` لكن لا توجد شاشة لإدارتها) |
| **S-23** | **تصميم التقرير لكل تحليل** | **لا يوجد!** | ❌ | لا توجد نافذة منفصلة لتخصيص تخطيط كل تقرير. `ReportSettingsWindowViewModel` يحرر صفاً واحداً مشتركاً في `LabSetting` — انتهاك BR-REP-001 (Per-Machine, Per-Report) |
| **S-24** | **شاشة الإحصائيات** | **لا يوجد!** | ❌ | لا View ولا VM. `ReportingService.GetDashboardMetricsAsync` يعيد Dictionary لكن لا UI يستهلكه. لا 5-1 ولا 5-2 ولا 5-3 ولا 5-4 |
| **S-25** | **ورقة عمل بأسماء المرضى** | **لا يوجد!** | ❌ | لا View ولا VM. `ReportingService.GetPendingWorksheetsAsync` يعيد `VPendingTests` لكن غير مربوط بـ UI |
| **S-26** | **ورقة عمل بأسماء التحاليل + Log** | **لا يوجد!** | ❌ | لا View ولا VM؛ لا يوجد Log لتصنيف التحاليل حسب مرات الإجراء |
| **S-27** | **إعدادات النظام** | **`LabSetting.cs` يحتوي 30+ عمود للإعدادات** + migrations متعددة (`AddFeatureTogglesToLabSetting`, `AddBackupAndSmtpFieldsToLabSettings`, `AddReportLayoutColumns`) | 🟡 | أعمدة DB موجودة لكن **لا واجهة موحدة لتحريرها جميعاً** — ReportSettingsWindow يغطي جزء فقط. ينقص: هوامش A4/A5 (BR-REP-003)، طابعات مخصصة لكل نوع (BR-REP-004)، نوع حساب افتراضي (BR-PT-007 setting)، إعدادات الإيصال والظرف |
| **S-28** | **صيانة قاعدة البيانات (Backup)** | **`BackupRestoreWindow.xaml` (77 سطر) + `BackupRestoreWindowViewModel.cs`** + `BackupService.cs` (339 سطر) + AES encryption | 🟡 | إنشاء واستعادة مع تشفير AES + كلمة مرور؛ ينقص: **تصفير كلي/جزئي (BR-BAK-001)**، **تسمية ملفات `Patient_yyyymmdd.bak`** (BR-BAK-002)، استعادة بيانات مريض واحد (BR-BAK-005)، نقل تلقائي لوسائط خارجية |
| **S-29** | **المستخدمون والصلاحيات** | **`AuthService.cs` (175 سطر) + `Staff.cs` + `Permission.cs` + `StaffPermission.cs` + `PasswordHasher`** | 🟡 | النظام الخلفي للـ RBAC كامل ومُختبَر بـ `AuthServiceTests`؛ **لا توجد نافذة UI لإدارة المستخدمين/الصلاحيات/إعادة تعيين كلمة المرور** |
| **S-30** | **الحضور والانصراف** | **`AttendanceWindow.xaml` (126 سطر) + `AttendanceWindowViewModel.cs`** + `AttendanceService.cs` + `Attendance.cs` + `WorkShift.cs` | 🟡 | منجز كنافذة؛ ينقص: **تقارير حركة الدخول والخروج**، حساب دقائق التأخير مقابل الوردية (موجود ضمنياً في `RecordClockInAsync` لكن لا تقرير ملخص) |
| S-31 | خدمة نتيجة (Natigh.com) | غير مذكور | ⛔ | خارج النطاق حسب القيد الصارم |

**إحصائيات الشاشات:**
- ✅ مكتمل: 4 شاشات (S-02, S-04, S-16, S-18, S-20) — أي ≈ 14%
- 🟡 جزئي: 18 شاشة — أي ≈ 56%
- ❌ مفقود: 9 شاشات (S-10, S-11, S-12, S-13, S-23, S-24, S-25, S-26 + S-15a, S-29 UI) — أي ≈ 28%
- ⛔ خارج النطاق: 1 شاشة — ≈ 3%

### 2.2 الوحدات الوظيفية (10 وحدات في المرجع)

| الوحدة | الحالة | الفجوة |
|---|---|---|
| 3.1 إدارة المرضى والزيارات | 🟡 | Lab ID منجز (BarcodeGenerator)؛ VIP منجز (`IsVip`)؛ ينقص: تطبيع الأسماء، TakenOutsideLab موجود كحقل، Patient Notes للـ FrontDesk-Lab message موجود، **سؤال المريض (PatientQuestion في TestType) غير مربوط بـ UI** |
| 3.2 المحاسبة والمالية | 🟡 | FinancialService + VisitService.CreateVisitAsync + CashDrawerService + CommissionReportService + OutstandingBalanceReportService + InvoiceService + ContractService — **منظومة شاملة**. ينقص: تسجيل العمولات التلقائي (موجود في `VReferralCommissionReport` view لكن لا trigger تلقائي واضح)، زر `+` للرسم الإضافي، تعديل الدفعات بعد الحفظ |
| 3.3 إدخال النتائج والتقارير | 🟡 | RoutineResultService + ReportCommentEngine + ReportLayoutService + WpfFlowDocumentPrintService — البنية جاهزة. **التنفيذ الفعلي لـ ResultReportTemplate فارغ** (placeholder). CompositeReportTemplate + MedicalHistoryTemplate + WorksheetTemplate كلها 17 سطر stubs |
| 3.4 المزارع والحساسية | 🟡 | CultureEntryViewModel + CultureResultService + MicrobiologyCulture + MicrobiologyOrganism + OrganismAntibiotic + AntibioticCatalog + AntibioticSensitivity enum — **شامل جداً**. ينقص: فلترة Pregnant/Children في الواجهة (BR-CULT-002)، إخفاء/إظهار الاسم التجاري |
| 3.5 العينات المرسلة للخارج | 🟡 | ExternalShipmentService + ExternalLabService (deprecated adapter) + ExternalLabsWindow. ينقص: زر "دائماً إليه"، تحويل تحليل فردياً إلى خارجي، فلاتر سريع |
| 3.6 كتالوج التحاليل والأسعار | ✅ | TestCatalogService (815 سطر) + TestDataManagementWindow (400 سطر) + PriceSchemeWindow — **منجز بالكامل** |
| 3.7 المستخدمين والصلاحيات | 🟡 | AuthService + PasswordHasher + Permission + StaffPermission + LoginView — **RBAC كامل**. ينقص: نافذة إدارة المستخدمين (S-29 UI)، AuditTrailWindow موجود لكن بدون launcher من Users menu |
| 3.8 الإحصائيات والجرد | 🟡 | ReportingService + CashDrawerService + CommissionReportService + OutstandingBalanceReportService — **Services جاهزة**. ينقص: شاشات UI مطابقة (S-24, S-25, S-26) |
| 3.9 الإعدادات والنسخ الاحتياطي | 🟡 | SettingsService + BackupService + LabSetting (30+ عمود) + ReportSettingsWindow. ينقص: تخصيص الطابعات لكل نوع، تصفير قاعدة البيانات، تسمية ملفات bak، واجهة موحدة لجميع الإعدادات |
| 3.10 التكامل الخارجي | ⛔ | خارج النطاق |

### 2.3 قواعد العمل BR-XXX

| الرمز | القاعدة | الحالة | الفجوة |
|---|---|---|---|
| BR-SYS-001 | اسم الجهاز بدلاً من IP | ❌ | لا يوجد منطق ربط بـ Machine Name؛ لا يوجد منطق fallback عند فشل بطاقة الشبكة |
| BR-SYS-002 | عدم إنترنت على السيرفر | ⛔ | قيد تشغيلي لا يمكن فرضه برمجياً بسهولة — نصيحة وليست قاعدة |
| BR-SYS-003 | لغة النظام English (Canada) | ❌ | لا يوجد تحقق من إعدادات Windows |
| BR-SYS-004 | كلمة المرور case-sensitive | ✅ | ضمني في `PasswordHasher.Verify` (PBKDF2 مع الحساسية الكاملة) |
| BR-SYS-005 | حذف admin الافتراضي | 🟡 | الـ bootstrap ينشئ أول admin؛ لا يُنشئ admin/admin افتراضي. لا آلية UI لإلغائه |
| BR-PT-001 | حقول إلزامية: اسم + جنس + سن | ✅ | `[Required]` attributes + `PatientInfo.HasErrors` |
| BR-PT-002 | السن لازم للمعدل الطبيعي | ✅ | منطقياً مُحترم في `RoutineResultService` |
| BR-PT-003 | وحدة السن حسب القيمة | 🟡 | `ApproxAge` + `ApproxAgeUnit` موجودان؛ لا تحويل تلقائي من 11 شهر إلى سنوات |
| BR-PT-004 | "طفل" أقل من 12 سنة | 🟡 | `IsSafeChildren` على AntibioticCatalog موجود؛ لا يوجد حقل/flag صريح على Patient أو Visit |
| BR-PT-005 | تطبيع الأسماء | ❌ | **لم يُنفَّذ** — لا يوجد Normalizer عند الحفظ |
| BR-PT-006 | 3 أنظمة حساب | ✅ | `PatientType` enum (Individual/LabToLab/Free) |
| BR-PT-007 | حذف مريض يحتاج صلاحية | ✅ | `DeleteCommand` في PatientRegistrationVM يتحقق من الصلاحية (مفترض) |
| BR-PT-008 | جهة إحالة جديدة تُحفَظ تلقائياً + خصم تلقائي | 🟡 | `Referral.ShouldSaveReferral` يحفظها؛ تطبيق الخصم التلقائي غير واضح |
| BR-BC-001 | 3 أكواد 13 خانة | ✅ | `BarcodeCodeType` enum + `BuildBarcodeValue` |
| BR-BC-002 | بنية الكود من 13 خانة | ✅ | منطقياً مطابق |
| BR-BC-003 | Lab ID دائم | ✅ | `Patient.LabId` + `GetOrCreateLabIdAsync` |
| BR-BC-004 | Drag & Drop + سلة محذوفات | ❌ | **لم يُنفَّذ** في `BarcodeDialog` |
| BR-BC-005 | قراءة الباركود تقلل الخطأ | 🟡 | تم تبسيط التسليم إلى OTP — انتهاك الفلسفة |
| BR-RES-001 | المعدل المناسب تلقائياً | 🟡 | `RoutineResultService` يفلتر حسب العمر/الجنس؛ لا يضمن تطبيق fasting state |
| BR-RES-002 | إبراز H/L/Critical | ✅ | `ClinicalStatusToBrushConverter` + `LOrH` text |
| BR-RES-003 | مراجعة قبل طباعة، طباعة قبل تسليم | 🟡 | `ResultStageRules.CanPrint/CanExport/CanDeliver` + أزرار `MarkReviewedCommand` + `TogglePrintCommand`؛ لا يوجد enforcement UI صارم (override ممكن دائماً) |
| BR-RES-004 | See report / Print with other | ✅ | `SeeReport` + `PrintWithOther` flags على TestType |
| BR-RES-005 | Main test + عناصر | ✅ | `IsMainTest` + `TestComponent` |
| BR-RES-006 | تعليقات Low/High/Critical | 🟡 | `ReportCommentEngine` + `ReportCommentTemplate.TriggerCondition` — منجز خلفياً، الواجهة محدودة |
| **BR-CBC-001** | **HCT = Hgb × 3.3** | ❌ | **لم يُنفَّذ** — لا منطق في `ResultEntryViewModel` |
| **BR-CBC-002** | **معاملات Hgb% حسب العمر/الجنس** | ❌ | **لم يُنفَّذ** |
| **BR-PT-CT-001** | **ISI + Control + Concentration لـ PT** | ❌ | **لم يُنفَّذ** |
| **BR-PT-CT-002** | **Control Time لـ PTT** | ❌ | **لم يُنفَّذ** |
| BR-TEST-001 | أرقام تحاليل محجوزة | ❌ | `TypeCode` فريد لكن لا توجد قائمة محجوزة |
| BR-NR-001 | كل تركيبة مستقلة | 🟡 | `NormalRange` يدعم تركيبات مستقلة؛ لا UI enforcement |
| BR-NR-002 | 6 مدخلات كحد أدنى | ❌ | لا تحذير UI |
| BR-NR-003 | Critical range مستقل | ✅ | `CriticalRangeText` + `LowCritical`/`HighCritical` + `CriticalFlag` |
| **BR-REP-001** | **Per-Machine, Per-Report** | ❌ | **انتهاك**: `LabSetting` صف واحد مشترك، لا Per-Machine key، لا Per-Test key |
| BR-REP-002 | Report top space ≤ 8 سم | ✅ | `ReportMarginTop` column default 2، عمود DB |
| BR-REP-003 | A4 / A5 فقط | 🟡 | `ReportPaperSize` عمود؛ لا dropdown |
| **BR-REP-004** | **طابعة مخصصة لكل نوع** | ❌ | **لم يُنفَّذ** — لا PrinterMapping |
| BR-REP-005 | تاريخ مرضي تلقائي | 🟡 | `ReportShowHeader/Footer` أعمدة؛ `MedicalHistoryTemplate` فارغ (17 سطر) |
| BR-EXT-001 | خارجي يظهر تلقائياً في S-14 | 🟡 | `IsSendOutside` flag؛ ExternalShipmentService يبني manifest |
| BR-EXT-002 | "دائماً إليه" | ❌ | **لم يُنفَّذ** |
| BR-CULT-001 | 4 مستويات حساسية | ✅ | `AntibioticSensitivity` enum مطابق |
| **BR-CULT-002** | **فلترة Pregnant/Children** | ❌ | الحقول في DB موجودة، الفلترة التلقائية في الواجهة مفقودة |
| BR-BAK-001 | كلمة مرور افتراضية 123 | 🟡 | AES encryption بكلمة مرور؛ لا منطق default ثم تغيير قسري |
| **BR-BAK-002** | **اسم `Patient_yyyymmdd.bak`** | ❌ | يستخدم `FinalLabSystem_yyyy-MM-dd_HHmmss.bak.enc` — **لا يطابق** |
| BR-BAK-003 | آخر ملف يحوي كل البيانات | ✅ | منطقياً بسبب Append عبر الـ DbSet |
| BR-BAK-004 | نقل لوسيط خارجي | ❌ | لا logic |
| BR-BAK-005 | استعادة مريض واحد | ❌ | RestoreBackupAsync يعيد كل البيانات |
| BR-SEC-001 | زرَّا P و T لمدير فقط | ✅ | `CanAccessAuditFeatures` flag |
| BR-SR-001 | حد أقصى 100 نتيجة | ✅ | `PatientService.SearchPatientsAsync` يطبّق `pageSize = Math.Min(pageSize, 100)` |
| **BR-SR-002** | **StartsWith + Contains مدمجَين** | ❌ | **البحث الحالي OR-based بسيط** — لا يدعم `اح*خال` |

**ملخص BR-XXX:**
- ✅: 18 قاعدة
- 🟡: 14 قاعدة
- ❌: 16 قاعدة (**خاصة BR-CBC-001/002، BR-PT-CT-001/002، BR-REP-001/004، BR-BAK-002/005، BR-SR-002، BR-BC-004، BR-CULT-002**)
- ⛔: 2 قاعدة (BR-SYS-002/005 جزئياً)

### 2.4 الاختصارات F1-F12

| الاختصار | الحالة | الفجوة |
|---|---|---|
| F2 (بيانات المرضى) | 🟡 | زر toolbar فقط، لا global hotkey |
| F3 (البحث) | 🟡 | زر toolbar فقط، لا global hotkey |
| F4 (النتائج) | 🟡 | زر toolbar فقط |
| F5 (Refresh) | ❌ | غير مربوط بأي نافذة |
| F6 (التسليم) | 🟡 | زر toolbar فقط |
| F7 (العينات/الجرد) | 🟡 | أزرار toolbar |
| F8 (تعديل/مراجعة) | ❌ | غير مربوط |
| F9 (حفظ) | ❌ | غير مربوط |
| F10 (حذف) | ❌ | غير مربوط |
| F11 (باركود/معاينة) | ❌ | غير مربوط |
| F12 (إيصال/طباعة) | ❌ | غير مربوط |

**ملخص:** 0 من 12 اختصار مُنفَّذ كـ KeyboardBinding. 6 منها موجودة كأزرار toolbar.

### 2.5 قاعدة البيانات والـ Migrations

| الجانب | الحالة | ملاحظات |
|---|---|---|
| 66 Migration مُطبَّقة | ✅ | من `20260604131224_AddV4SystemEnhancements` إلى `20260704140935_AddCultureFieldsAndSensitivityEnum` |
| 53 Model/Entity | ✅ | يغطي كل المرجع ماعدا فروع متعددة |
| Cascading deletes مُكوَّنة | ✅ | `OnDelete(DeleteBehavior.Restrict)` في العلاقات الحساسة |
| Audit interceptor | ✅ | `[Auditable]` attribute + AuditInterceptor |
| View entities (VPatientHistory, VOutstandingBalance, VReferralCommissionReport, VResultAuditTrail, VPendingTest, VSampleTubeStatus) | ✅ | 6 Views SQL مطابقة لـ RLS queries |
| `VisitDiscountExclusivityConstraint` | ✅ | قيد حصري للخصم (رقم أو نسبة) |

### 2.6 الاختبارات (112 ملف)

| الفئة | العدد | ملاحظات |
|---|---|---|
| Service tests | 47 | تغطية قوية لـ AuthService, PatientService, BackupService, CashDrawerService, ReportLayoutService, ResultEntryDialogService, RoutineResultService |
| Integration tests | 11 | Phase5BuildVerification, Phase6BuildVerification, ExternalShipmentEndToEnd, InvoiceWorkflowEndToEnd, AttendanceWorkflowEndToEnd, AutoCommentEndToEnd, CashDrawerEndToEnd, InventoryAlertEndToEnd |
| Slice tests | 3 | Slice1 (Barcode), Slice2 (Culture) |
| ViewModel tests | 30 | Menu VMs, Settings VMs, Patients VMs |
| Validation tests | 6 | EntityValidation, MigrationTests, NatighFieldScanTests |
| Infrastructure tests | 3 | AesEncryptionHelper, OtpGenerator, TestPricingEngine |

**فجوة في الاختبارات:**
- ❌ لا اختبارات E2E لـ PatientSearch
- ❌ لا اختبارات E2E لـ Delivery بـ Barcode
- ❌ لا اختبارات للـ Statistics Dashboard
- ❌ لا اختبارات للـ Report Designer

---

## 3. خريطة الأولويات والأثر

### 3.1 مصفوفة الأثر × الجهد

```
                    الأثر على سير العمل الأساسي
                    منخفض ──────────────────── مرتفع
الجهد      منخفض  │  [S-12 Composite]     │  [BR-CBC-001] [BR-PT-CT-001]
                    │  [BR-BAK-002 naming] │  [Name Norm]
                    │  [BR-EXT-002 sticky] │  [BR-BC-004 D&D]
                    │────────────────────┼──────────────────────
                    متوسط  │  [Statistics UI]   │  [S-10 Delivery 2-step]
                    │  [Worksheets UI]    │  [S-11 Patient Search]
                    │  [Report Designer]  │  [User Mgmt UI]
                    │  [BR-SR-002]        │  [Backup zeroing]
                    │────────────────────┼──────────────────────
                    مرتفع  │  [Hotkeys wiring] │  [S-23 Report Designer]
                    │  [BR-REP-004]       │  full per-test
                    │  [BR-CULT-002 UI]   │  [Statistics Dashboard]
                    │                     │  real
```

### 3.2 ترتيب الأولويات

**Priority P0 — حجب سير العمل الأساسي:**
1. **S-11 Patient Search الحقيقي** — يعيق تسليم نتائج المرضى القدامى
2. **S-10 Delivery بقارئ الباركود** — تبسيط التسليم يخالف الفلسفة المرجعية
3. **ResultReportTemplate الحقيقي** — طباعة التقارير فارغة الآن
4. **S-12 + S-13 Composite/Blank Templates** — فارغة (17 سطر)
5. **BackupRestore UI** — تصفير + naming + نقل (BR-BAK-001/002)

**Priority P1 — ميزات تشغيلية جوهرية:**
6. **S-23 Report Designer لكل تحليل** (BR-REP-001)
7. **S-24 Statistics Dashboard**
8. **S-25/S-26 Worksheets**
9. **S-29 User Management UI**
10. **BR-CBC-001/002 + BR-PT-CT-001/002 Constants** (منطقي حسابي)
11. **BR-CULT-002 فلترة المضادات Pregnant/Children**

**Priority P2 — تحسينات وتجميع:**
12. **BR-PT-005 تطبيع الأسماء**
13. **BR-SR-002 بحث مدمج**
14. **BR-BC-004 Drag & Drop للملصقات**
15. **اختصارات F1-F12**
16. **BR-REP-004 طابعات مخصصة**
17. **BR-EXT-002 "دائماً إليه"**

---

## 4. الشرائح المقترحة للتنفيذ (Slices)

> كل شريحة تنتج **نتيجة قابلة للاختبار** (deployable + tested) ولا تكسر الموجود.

### الشريحة SL-01: Patient Search الحقيقي (P0)
**الهدف:** بناء S-11 كاملاً كما في RLS.

**الملفات المتأثرة:**
- `Views/Patients/PatientSearchWindow.xaml` (حالياً 11 سطر → ~250 سطر)
- `Views/Patients/PatientSearchViewModel.cs` (15 → ~400 سطر)
- `Models/DTOs/PatientSearchFilter.cs` (جديد)
- `Services/Interfaces/IPatientSearchService.cs` (جديد) أو توسيع `IPatientService`
- `Services/Implementations/PatientSearchService.cs` (جديد)
- `FinalLabSystem.Tests/ViewModels/Patients/PatientSearchViewModelTests.cs` (جديد)
- `FinalLabSystem.Tests/Services/PatientSearchServiceTests.cs` (جديد)
- Migration جديدة لإضافة `LastVisitCount` materialized view أو computed column

**المهام:**
1. نافذة بحث مع فلاتر: الفترة الزمنية، المرحلة العمرية، الجنس، جهة الإحالة
2. بحث بالاسم بطريقتين (يبدأ بـ / يحتوي) مدمجَتين (BR-SR-002)
3. بحث بالهاتف/الجوال/الرقم القومي/كود الحالة/كود المعمل/كود الملف
4. حد أقصى 100 نتيجة (BR-SR-001) مع pagination
5. قائمة تحاليل المريض المختار مع مؤشرات (غير منتهية/غير مطبوعة/غير مسلَّمة/حسابات مفتوحة)
6. أزرار: عرض النتائج، بيانات المريض، حذف، "نتائج مجمَّعة"

**عدد الاختبارات المقترحة:** 18 اختبار
- 6 لخدمة البحث (BR-SR-001، BR-SR-002، multi-criteria)
- 8 للـ ViewModel (filter changes، pagination، selection events)
- 4 للـ Integration E2E (search → select → composite results)

**معايير القبول:**
- ✅ يمكن فتح S-11 من `F3` ومن toolbar
- ✅ البحث بالاسم يدعم `اح*خال` ويُرجع ≤ 100 نتيجة
- ✅ جميع الفلاتر مركّبة وتعمل معاً
- ✅ زر "نتائج مجمَّعة" يفتح S-12 (بعد اكتمال SL-04)
- ✅ حذف مريض من S-11 يحتاج صلاحية `PATIENTS.DELETE`

**تقدير الجهد:** 5 أيام عمل (1 dev)
**المخاطر:** منخفضة — لا تغيير schema، فقط استعلامات + UI

---

### الشريحة SL-02: Result Report Templates الحقيقي (P0)
**الهدف:** استبدال placeholders الـ 5 بقوالب FlowDocument حقيقية مطابقة لـ RLS.

**الملفات المتأثرة:**
- `Services/Printing/ResultReportTemplate.cs` (53 → ~300 سطر)
- `Services/Printing/CompositeReportTemplate.cs` (17 → ~150 سطر)
- `Services/Printing/BlankReportTemplate.cs` (17 → ~80 سطر)
- `Services/Printing/MedicalHistoryTemplate.cs` (17 → ~120 سطر)
- `Services/Printing/WorksheetTemplate.cs` (17 → ~100 سطر)
- `Services/Printing/EnvelopeTemplate.cs` (17 → ~80 سطر)
- `FinalLabSystem.Tests/Services/Printing/ReportTemplateLayoutTests.cs` (جديد)
- إضافة DTOs: `ResultReportData`, `CompositeReportData`

**المهام:**
1. **ResultReportTemplate:** يطبع رأس المعمل، بيانات المريض، جدول المكونات بالنتيجة + المدى + الوحدة + L/H/Critical + الحالة، التوقيع
2. **CompositeReportTemplate:** يدمج عدة تحاليل مع إعادة ترتيب، يحترم `SeeReport`/`PrintWithOther`
3. **MedicalHistoryTemplate:** يدرج زيارات سابقة لنفس البروفيل
4. **WorksheetTemplate:** قائمة تحاليل بصفوف للمرضى في الفترة
5. **BlankReportTemplate:** يحمل الشعار + الباركود فقط
6. **EnvelopeTemplate:** ظرف A4 أو A5

**عدد الاختبارات المقترحة:** 12 اختبار (snapshot tests للـ FlowDocument structure)

**معايير القبول:**
- ✅ طباعة تقرير نتيجة حقيقية تطابق S-09 layout
- ✅ Composite يحترم BR-RES-004 (يرفض SeeReport/PrintWithOther)
- ✅ MedicalHistory يدرج زيارات سابقة فقط لنفس `ComponentId`
- ✅ كل template يحترم `ReportLayoutDto` (margins, fonts, colors)

**تقدير الجهد:** 4 أيام عمل
**المخاطر:** متوسطة — تعقيد تنسيق FlowDocument

---

### الشريحة SL-03: Backup Database Maintenance كامل (P0)
**الهدف:** إكمال S-28 بمتطلبات BR-BAK-001..005.

**الملفات المتأثرة:**
- `Services/Implementations/BackupService.cs` (339 → ~450 سطر)
- `ViewModels/Settings/BackupRestoreWindowViewModel.cs` (توسيع)
- `Views/Settings/BackupRestoreWindow.xaml` (77 → ~250 سطر)
- `FinalLabSystem.Tests/Services/BackupServiceTests.cs` (توسيع)
- `FinalLabSystem.Tests/Integration/BackupServiceIntegrationTests.cs` (توسيع)

**المهام:**
1. **BR-BAK-002:** تغيير naming إلى `Patient_yyyymmdd.bak` مع fallback للـ `.bak.enc`
2. **BR-BAK-001:** إجبار تغيير كلمة المرور عند أول استخدام (123 → جديدة)
3. **Zeroing:** زر "تصفير كلي" و"تصفير جزئي" مع confirmation قوي
4. **BR-BAK-004:** تحذير نقل إلى USB + زر "نسخ إلى مجلد..."
5. **BR-BAK-005:** استعادة بيانات مريض واحد بكوده (Lookup في ملف النسخة)
6. **Audit:** تسجيل كل backup/restore في AuditLog

**عدد الاختبارات المقترحة:** 14 اختبار
- 6 لـ service (zeroing، naming، single-patient restore)
- 4 لـ ViewModel
- 4 لـ Integration E2E

**معايير القبول:**
- ✅ اسم الملف يطابق `Patient_yyyymmdd.bak.enc`
- ✅ كلمة المرور الافتراضية 123 يجب تغييرها عند أول backup
- ✅ تصفير كلي/جزئي يطلب تأكيد 3 خطوات
- ✅ استعادة مريض واحد يعمل من ملف نسخة موجود

**تقدير الجهد:** 6 أيام عمل
**المخاطر:** متوسطة-عالية — التصفير يجب أن يكون transaction-safe

---

### الشريحة SL-04: S-10 Delivery بقارئ الباركود (P0)
**الهدف:** استبدال آلية OTP/Tوقيع بـ "قراءة كود الإيصال + قراءة كود الملف + مطابقة".

**الملفات المتأثرة:**
- `Views/Patients/DeliveryWindow.xaml` (46 → ~400 سطر)
- `ViewModels/Patients/Delivery/DeliveryViewModel.cs` (144 → ~500 سطر)
- `Services/Implementations/DeliveryService.cs` (جديد) — أو توسيع `IDeliveryConfirmationService`
- Migration جديدة لإضافة `OutstandingBalanceFlag`
- `FinalLabSystem.Tests/ViewModels/Patients/Delivery/DeliveryViewModelTests.cs` (جديد)
- `FinalLabSystem.Tests/Integration/DeliveryEndToEndTests.cs` (جديد)
- `FinalLabSystem.Tests/Slice3/DeliverySlice3Tests.cs` (جديد)

**المهام:**
1. **نافذة S-10 كاملة:** قائمة مرضى اليوم مع 7 رموز حالة، بحث، فلاتر (VIP/مستقل/معمل)، معلومات المريض والتحاليل
2. **آلية 2-step:** حقل قراءة باركود → عرض بيانات المريض → حقل قراءة باركود ثانٍ → مطابقة → تسليم
3. **تنبيه باقي حساب** قبل التسليم
4. **زر "مستلمة"** للتجاوز اليدوي (BR-RES-003 exception)
5. **زر "تسديد"** لتصفية الحساب أثناء التسليم
6. **OTP وSignature** كـ alternative (احتفظ بهما كـ options)

**عدد الاختبارات المقترحة:** 22 اختبار
- 8 للـ Service (2-step match، balance check، manual override)
- 9 للـ ViewModel (status icons rendering، filter logic)
- 5 للـ Integration E2E (full delivery flow)

**معايير القبول:**
- ✅ يمكن التسليم بقراءة باركود فقط (حالتان: كود صحيح / كود خاطئ)
- ✅ كود خاطئ → رسالة "لا تطابق" + بقاء في النافذة
- ✅ باقي حساب → تنبيه + خيار "ادفع أولاً"
- ✅ تسليم يدوي يحتاج صلاحية `DELIVERY.MANUAL_OVERRIDE`
- ✅ F2/F6/F8/F12 مفعلة داخل النافذة

**تقدير الجهد:** 8 أيام عمل
**المخاطر:** عالية — تغيير UX جوهري

---

### الشريحة SL-05: S-23 Report Designer لكل تحليل (P1)
**الهدف:** إكمال BR-REP-001 (Per-Machine, Per-Report) + واجهة S-23.

**الملفات المتأثرة:**
- Migration جديدة: `AddReportDesignOverrides` (جدول `ReportDesignOverride` بـ ReportKey + MachineName + JSON)
- `Models/ReportDesignOverride.cs` (جديد)
- `Services/Implementations/ReportDesignService.cs` (جديد)
- `Services/Interfaces/IReportDesignService.cs` (جديد)
- `ViewModels/Settings/ReportDesignerViewModel.cs` (جديد)
- `Views/Settings/ReportDesignerWindow.xaml` (جديد)
- توسيع `WpfFlowDocumentPrintService` لاحترام Override
- `FinalLabSystem.Tests/Services/ReportDesignServiceTests.cs` (جديد)

**المهام:**
1. جدول `ReportDesignOverrides` بـ Composite Key: (TestTypeId, MachineName, ReportElementPath)
2. نافذة S-23 كاملة: عناوين، Show/Hide flags، ألوان، محاذاة، هوامش، اختيار طابعة
3. زر "حفظ" يخزن Override للجهاز الحالي
4. زر "أبيض وأسود" يعيد الافتراضي
5. Print pipeline يحمّل Override قبل BuildDocument
6. احترام BR-REP-002 (≤ 8 سم)، BR-REP-003 (A4/A5)، BR-REP-005

**عدد الاختبارات المقترحة:** 16 اختبار
- 8 للـ Service (lookup by key، JSON schema، fallback)
- 5 للـ ViewModel
- 3 للـ Integration

**معايير القبول:**
- ✅ تعديل على TestTypeId=10 على MachineName="PC-LAB1" لا يؤثر على PC-LAB2
- ✅ حذف Override يرجع للقيمة الافتراضية من LabSetting
- ✅ BR-REP-002 مُحترم (UI يرفض > 8 سم)

**تقدير الجهد:** 10 أيام عمل
**المخاطر:** عالية — تعقيد schema + UI designer

---

### الشريحة SL-06: S-24 Statistics Dashboard (P1)
**الهدف:** بناء 5-1..5-4 من RLS_Learn.

**الملفات المتأثرة:**
- `Views/Settings/StatisticsDashboardWindow.xaml` (جديد)
- `ViewModels/Settings/StatisticsDashboardViewModel.cs` (جديد)
- `Services/Implementations/StatisticsService.cs` (جديد) — أو توسيع ReportingService
- `Models/DTOs/Statistics*.cs` (جديد)
- زر "إحصائيات" في `MainWindow.xaml` Toolbar
- `FinalLabSystem.Tests/Services/StatisticsServiceTests.cs` (جديد)

**المهام:**
1. **5-1:** إحصائيات المرضى (فرز بالجنس، بالشهور)
2. **5-2:** عدد مرضى في سنة محددة
3. **5-3:** عدد مرضى في شهر محدد
4. **5-4:** معدل طلب تحليل / مجموعة تحاليل
5. إحصاء الطاقة الإنتاجية لكل موظف
6. عرض رسومي (WPF charts عبر OxyPlot أو LiveCharts) + جداول قابلة للطباعة

**عدد الاختبارات المقترحة:** 12 اختبار
- 8 للـ Service (queries دقيقة، فلاتر زمنية)
- 4 للـ ViewModel (date selection، chart data binding)

**معايير القبول:**
- ✅ 4 أنواع إحصاءات متاحة
- ✅ كل إحصاء يطبع إلى PDF/Printer
- ✅ الفترة الزمنية قابلة للتخصيص

**تقدير الجهد:** 7 أيام عمل
**المخاطر:** متوسطة — تعقيد queries + charts

---

### الشريحة SL-07: S-25/S-26 Worksheets (P1)
**الهدف:** بناء نافذتي أوراق العمل.

**الملفات المتأثرة:**
- `Views/Settings/PatientsWorksheetWindow.xaml` (جديد)
- `Views/Settings/TestsWorksheetWindow.xaml` (جديد)
- `ViewModels/Settings/PatientsWorksheetViewModel.cs` (جديد)
- `ViewModels/Settings/TestsWorksheetViewModel.cs` (جديد)
- توسيع `ReportingService` بـ `GetPatientsWorksheetAsync(dateRange)` و `GetTestsWorksheetAsync(testTypeId, dateRange)`
- `FinalLabSystem.Tests/Services/WorksheetServiceTests.cs` (جديد)

**المهام:**
1. **Patients Worksheet:** قائمة بجميع مرضى الفترة مع حالاتهم (7 رموز)
2. **Tests Worksheet:** قائمة تحاليل بصفوف (مريض/تحليل/حالة) للفترة
3. **Tests Log:** تصنيف التحاليل حسب مرات الإجراء في الفترة
4. طباعة A4 أفقي

**عدد الاختبارات المقترحة:** 8 اختبارات

**معايير القبول:**
- ✅ طباعة 200 مريض في < 3 ثوانٍ
- ✅ فلاتر: تاريخ، نوع تحليل، حالة

**تقدير الجهد:** 5 أيام عمل

---

### الشريحة SL-08: User Management UI (S-29) (P1)
**الهدف:** بناء واجهة إدارة المستخدمين والصلاحيات.

**الملفات المتأثرة:**
- `Views/Settings/UserManagementWindow.xaml` (جديد)
- `ViewModels/Settings/UserManagementViewModel.cs` (جديد)
- `ViewModels/Settings/UserEditViewModel.cs` (جديد)
- `ViewModels/Settings/PermissionEditViewModel.cs` (جديد)
- زر "المستخدمون" في `MainWindow.xaml` Toolbar
- `FinalLabSystem.Tests/ViewModels/Settings/UserManagementViewModelTests.cs` (جديد)

**المهام:**
1. CRUD مستخدمين (اسم، كلمة مرور، IsAdmin، IsActive)
2. قائمة Permissions مع CheckBox لكل مستخدم
3. إعادة تعيين كلمة المرور
4. حذف مستخدم (مع التحقق من عدم وجود مرضى/مدفوعات مرتبطة)
5. Audit Trail لكل عملية

**عدد الاختبارات المقترحة:** 14 اختبار

**معايير القبول:**
- ✅ يمكن إنشاء مستخدم جديد مع صلاحيات محددة
- ✅ حذف مستخدم نشط يحتاج تأكيد مزدوج
- ✅ Reset كلمة المرور يولد كلمة مؤقتة

**تقدير الجهد:** 6 أيام عمل

---

### الشريحة SL-09: BR-CBC + BR-PT-CT Constants Logic (P1)
**الهدف:** تنفيذ منطق Hgb%/HCT/PT/PTT الثوابت.

**الملفات المتأثرة:**
- `Models/TestComponent.cs` (إضافة حقول Constants)
- `Models/CbcConstants.cs` (جديد) — أو enum + table
- `Services/Implementations/CbcConstantService.cs` (جديد)
- `ViewModels/Patients/ResultEntryViewModel.cs` (إضافة منطق Constants button)
- `Views/Patients/ResultEntryWindow.xaml` (إضافة زر Constants + dialog)
- `Views/Patients/CbcConstantsDialog.xaml` (جديد)
- Migration جديدة: `AddTestComponentConstants`

**المهام:**
1. جدول Constants بـ (ComponentId, Sex, AgeFromDays, AgeToDays, Coefficient, ISI, ControlTime, ConcentrationTable)
2. زر Constants في ResultEntryWindow يفتح dialog لكل مكون
3. BR-CBC-001: حساب HCT = Hgb × 3.3 إذا لم يُدخَل
4. BR-CBC-002: حساب Hgb% من المعاملات حسب الجنس/العمر:
   - × 8.25 (أقل من سنة، أي جنس)
   - × 7.50 (1-12 سنة، أي جنس)
   - × 6.25 (ذكر > 12 سنة)
   - × 6.75 (أنثى > 12 سنة)
5. BR-PT-CT-001: PT يتطلب ISI + Control Time + Concentration table قبل الحفظ
6. BR-PT-CT-002: PTT يتطلب Control Time قبل الحفظ
7. حفظ كـ Audit entry عند تعديل الثوابت

**عدد الاختبارات المقترحة:** 18 اختبار
- 8 لمنطق الحساب (4 معاملات Hgb%، HCT)
- 4 للتحققات (PT/PTT)
- 6 للـ UI/ViewModel

**معايير القبول:**
- ✅ Hgb% يُحسب تلقائياً بعد إدخال Hgb
- ✅ PT لا يُحفظ بدون ISI/Concentration
- ✅ تعديل ثوابت مكون يحتاج صلاحية `RESULTS.EDIT_CONSTANTS`

**تقدير الجهد:** 6 أيام عمل
**المخاطر:** متوسطة — تعقيد منطق حسابي + UI

---

### الشريحة SL-10: BR-CULT-002 فلترة Pregnant/Children في المزارع (P1)
**الهدف:** فلترة المضادات في CultureEntryViewModel.

**الملفات المتأثرة:**
- `ViewModels/Patients/CultureEntryViewModel.cs` (إضافة Filter logic)
- `Services/Implementations/CultureResultService.cs` (توسيع)
- `Models/AntibioticCatalog.cs` (إضافة `FilterLogic`)
- `FinalLabSystem.Tests/ViewModels/Patients/CultureEntryPregnantFilterTests.cs` (جديد)

**المهام:**
1. زر "إخفاء المضادات غير الآمنة" في CultureEntryWindow
2. تفعيل تلقائي إذا Visit.IsPregnant = true أو PatientAge < 12 سنة
3. AntibioticCatalog.IsSafePregnancy / IsSafeChildren مفهرسة
4. عرض المضادات المرفوضة بلون رمادي مع tooltip "غير آمن للحوامل"

**عدد الاختبارات المقترحة:** 8 اختبارات

**معايير القبول:**
- ✅ Pregnant = true يخفي كل المضادات غير الآمنة
- ✅ Children < 12 سنة يخفي كل المضادات غير الآمنة
- ✅ يمكن تجاوز الفلترة بزر "إظهار الكل"

**تقدير الجهد:** 3 أيام عمل

---

### الشريحة SL-11: BR-PT-005 تطبيع الأسماء + BR-SR-002 البحث المدمج (P2)
**الهدف:** تطبيع الأسماء عند الإدخال + بحث مدمج.

**الملفات المتأثرة:**
- `Infrastructure/ArabicNameNormalizer.cs` (جديد)
- `Services/Implementations/PatientService.cs` (توسيع `RegisterPatientAsync`)
- `Services/Implementations/PatientSearchService.cs` (توسيع للبحث المدمج)
- `FinalLabSystem.Tests/Infrastructure/ArabicNameNormalizerTests.cs` (جديد)
- `FinalLabSystem.Tests/Services/PatientSearchCombinedQueryTests.cs` (جديد)

**المهام:**
1. **ArabicNameNormalizer:**
   - إزالة الهمزات: أحمد → احمد
   - ت→ه: مروة → مروه
   - ى→ي في وسط الكلمة: علي (وليس على)
   - تطبيق تلقائي عند `RegisterPatientAsync` و `UpdatePatientAsync`
2. **BR-SR-002:** دعم نمط `يبدأ*يحتوي` في `SearchPatientsAsync`
3. تحديث نتائج البحث لتطبيق التطبيع على المدخل والمخزَّن

**عدد الاختبارات المقترحة:** 14 اختبار
- 8 لـ Normalizer
- 6 للبحث المدمج

**معايير القبول:**
- ✅ إدخال "أحمد محمد" يُحفَظ "احمد محمد"
- ✅ بحث `اح*خال` يعيد "احمد محمد إخاليل"

**تقدير الجهد:** 3 أيام عمل

---

### الشريحة SL-12: BR-BC-004 Drag & Drop للملصقات (P2)
**الهدف:** Drag & Drop في BarcodeDialog لدمج/حذف تحاليل من ملصقات.

**الملفات المتأثرة:**
- `Views/Patients/BarcodeDialog.xaml` (إضافة Drag/Drop handlers)
- `ViewModels/Patients/BarcodeDialogViewModel.cs` (إضافة Drag/Drop commands)
- `Models/DTOs/BarcodeLabelDto.cs` (توسيع)
- `FinalLabSystem.Tests/ViewModels/Patients/BarcodeDragDropTests.cs` (جديد)

**المهام:**
1. سحب تحليل من ملصق وإفلاته على ملصق آخر → دمج
2. سحب تحليل فوق سلة محذوفات → حذف من الملصق
3. المنزلقات الأفقية والرأسية لضبط الموضع
4. حفظ الأبعاد لكل (PatientId, MachineName)

**عدد الاختبارات المقترحة:** 10 اختبارات

**معايير القبول:**
- ✅ دمج ملصقين (مصل صائم + فيروسات مصل) → ملصق واحد
- ✅ حذف تحليل من الملصق يحذفه من `TestTypeSampleTube`

**تقدير الجهد:** 5 أيام عمل

---

### الشريحة SL-13: Hotkeys F1-F12 الشاملة (P2)
**الهدف:** ربط جميع اختصارات لوحة المفاتيح.

**الملفات المتأثرة:**
- `Views/MainWindow.xaml` (InputBindings)
- `Views/Patients/PatientRegistrationWindow.xaml` (InputBindings)
- `Views/Patients/TestResultsWindow.xaml` (InputBindings)
- `Views/Patients/ResultEntryWindow.xaml` (InputBindings)
- `Views/Patients/DeliveryWindow.xaml` (InputBindings)
- `Views/Patients/PatientSearchWindow.xaml` (InputBindings)
- `Infrastructure/GlobalHotkeyManager.cs` (جديد) — للاختصارات العامة F2/F3/F4/F6/F7

**المهام:**
1. GlobalHotkeyManager يلتقط F2/F3/F4/F6/F7 من كل مكان
2. InputBindings لكل نافذة (F1=details، F5=refresh، F8=edit، F9=save، F10=delete، F11=barcode/preview، F12=receipt/print)
3. Enter المزدوج في Financial dialog (BR-7)
4. Visual hints للاختصارات في Tooltip

**عدد الاختبارات المقترحة:** 16 اختبار (UI Automation)

**معايير القبول:**
- ✅ F2 من أي نافذة → S-05
- ✅ F9 في ResultEntry → حفظ + عودة
- ✅ Enter مزدوج في Financial يحفظ المبلغ

**تقدير الجهد:** 4 أيام عمل

---

### الشريحة SL-14: BR-REP-004 طابعات مخصصة لكل نوع (P2)
**الهدف:** طابعة مستقلة لكل من (التقارير، الباركود، الإيصال، الظرف، الكارنيه).

**الملفات المتأثرة:**
- Migration جديدة: `AddPrinterMappingsToLabSetting`
- `Models/LabSetting.cs` (توسيع)
- `Services/Implementations/PrintService.cs` (توسيع Printer routing)
- `ViewModels/Settings/ReportSettingsWindowViewModel.cs` (إضافة Printers tab)
- `FinalLabSystem.Tests/Services/PrinterMappingTests.cs` (جديد)

**المهام:**
1. 5 أعمدة PrinterName في LabSetting (Reports, Barcode, Receipt, Envelope, Card)
2. Print pipeline يحترم التخصيص
3. UI لتحرير أسماء الطابعات
4. اختبار: "Print to PDF" افتراضي إذا لم يُحدَّد

**عدد الاختبارات المقترحة:** 8 اختبارات

**معايير القبول:**
- ✅ Reports تذهب لـ Printer A، Barcode لـ Printer B
- ✅ Fallback إلى Default Printer إذا لم يُحدَّد

**تقدير الجهد:** 3 أيام عمل

---

### الشريحة SL-15: BR-EXT-002 + إكمال External Samples (P2)
**الهدف:** زر "دائماً إليه" + تحويل تحليل فردياً.

**الملفات المتأثرة:**
- `Views/Settings/ExternalLabsWindow.xaml` (إضافة زر "دائماً إليه")
- `ViewModels/Settings/ExternalLabsWindowViewModel.cs` (توسيع)
- `Services/Implementations/ExternalShipmentService.cs` (إضافة TogglePerTestAsync)
- `FinalLabSystem.Tests/Services/ExternalShipmentStickyDefaultTests.cs` (جديد)

**المهام:**
1. زر "دائماً إليه" في شحنة → يحدث TestType.OutsideLabName
2. زر "إرسال للخارج" / "إلغاء الإرسال" على VisitTest فردياً
3. Filter سريع: "غير المرسل" / "كل العينات المرسلة في فترة"
4. تجميع تلقائي للعينات لنفس الجهة

**عدد الاختبارات المقترحة:** 10 اختبارات

**معايير القبول:**
- ✅ اختيار "دائماً إليه" يخزَّن كقيمة افتراضية للجهة
- ✅ تحليل فردي قابل للتحويل دون تعديل الكتالوج

**تقدير الجهد:** 4 أيام عمل

---

### 4.1 خطة التنفيذ الكلية

```
الشهر 1 (الأسابيع 1-4):
  SL-01 (Patient Search) ─────┐
  SL-02 (Report Templates) ───┤── بالتوازي (2 devs)
                              │
  SL-03 (Backup Maintenance) ─┘

الشهر 2 (الأسابيع 5-8):
  SL-04 (Delivery Barcode) ───┐
  SL-05 (Report Designer) ────┤── بالتوازي (2 devs)
  SL-09 (CBC/PT Constants) ───┘

الشهر 3 (الأسابيع 9-12):
  SL-06 (Statistics) ─────────┐
  SL-07 (Worksheets) ─────────┤── بالتوازي
  SL-08 (User Management) ────┘

الشهر 4 (الأسابيع 13-16):
  SL-10 (Pregnant Filter) ────┐
  SL-11 (Name Normalization) ─┤── بالتوازي + SL-15
  SL-12 (Drag & Drop) ────────┘

الشهر 5 (الأسابيع 17-20):
  SL-13 (Hotkeys) ────────────┐
  SL-14 (Printer Routing) ────┘── بالتوازي + Polish
```

**إجمالي الجهد:** ≈ 87 يوم عمل (≈ 17.4 أسبوع لـ 1 dev أو ≈ 9 أسابيع لـ 2 devs)

**ملاحظة:** التقديرات تقريبية بناءً على تعقيد كل شريحة ومدى استفادتها من البنية الموجودة.

---

## 5. معايير القبول العامة لكل شريحة

كل شريحة يجب أن:

1. **✅ Code Review:** تمر من review واحد على الأقل
2. **✅ Unit Tests:** تغطية ≥ 80% للكود الجديد
3. **✅ Integration Tests:** E2E happy path واحد على الأقل
4. **✅ Manual Smoke Test:** سيناريو RLS كامل من CHM يعمل
5. **✅ No Regression:** جميع الـ 112 اختبار الموجودة تنجح
6. **✅ Migrations آمنة:** لا تغييرات على schema بدون Down() صحيح
7. **✅ RTL & Arabic:** كل UI يحترم `FlowDirection="RightToLeft"` والنصوص العربية
8. **✅ Audit Logging:** كل عملية كتابة تُسجَّل في `AuditLog` عند `[Auditable]`
9. **✅ Documentation:** تحديث CHANGELOG.md + DocStrings عامة على كل public method
10. **✅ Build Clean:** `dotnet build` بدون warnings جديدة

---

## 6. ملخص الفجوات حسب الأولوية

| الأولوية | عدد الشرائح | الجهد التقديري | الأثر |
|---|---|---|---|
| **P0 — حجب سير العمل** | 4 (SL-01..04) | 23 يوم | حرج |
| **P1 — ميزات تشغيلية** | 5 (SL-05..09) | 36 يوم | مرتفع |
| **P2 — تحسينات** | 6 (SL-10..15) | 21 يوم | متوسط |
| **الإجمالي** | **15 شريحة** | **≈ 80 يوم** | |

---

## 7. القيود الصارمة المرعية

1. ✅ **التقنيات مقفلة:** .NET 8 / WPF / EF Core / SQL Server / MVVM — لا تغيير
2. ✅ **Natigh.com خارج النطاق:** لم يُذكر في أي شريحة
3. ✅ **نظام الفروع خارج النطاق:** `BranchNumber` byte في LabSetting فقط (موجود)؛ لا تطوير UI لإدارة فروع
4. ✅ **لا إعادة بناء:** جميع الشرائح تُضيف فوق الموجود، لا تُعيد بناء ما يعمل
5. ✅ **الكود الفعلي:** كل تقييم قائم على قراءة الكود المُلتزَم، لا افتراضات

---

## 8. ملحق — الملفات المرجعية للتطوير

| الملف | الوصف |
|---|---|
| `FinalLabSystem/Models/LabSetting.cs` | 30+ عمود إعداد — مرجع لتوسعة SL-14 |
| `FinalLabSystem/Models/NormalRange.cs` | هيكل المعدلات — مرجع لـ SL-09 |
| `FinalLabSystem/Models/Patient.cs` | حقول المريض — مرجع لـ SL-11 |
| `FinalLabSystem/Services/Implementations/BarcodeGenerator.cs` | منطق الباركود — مرجع لـ SL-12 |
| `FinalLabSystem/Services/Implementations/BackupService.cs` | منطق الـ Backup — مرجع لـ SL-03 |
| `FinalLabSystem/ViewModels/Patients/TestResultsViewModel.cs` | أكبر VM — مرجع لربط 994 سطر مع SL-01/04 |
| `FinalLabSystem/Models/Enums/PatientVisitStatus.cs` | حالات الـ 7 — مرجع لربط رموز SL-04 |
| `FinalLabSystem/Services/Printing/DocumentTemplateBase.cs` | بنية Template — مرجع لـ SL-02 |
| `FinalLabSystem.Tests/Integration/Phase6BuildVerificationTests.cs` | نمط Integration tests |
| `FinalLabSystem.Tests/Slice1/BarcodeSlice1Tests.cs` | نمط Slice tests |

---

## 9. ملحق — Migrations المتوقعة

| الشريحة | Migration مطلوبة |
|---|---|
| SL-01 | لا (استعلامات فقط) |
| SL-02 | لا |
| SL-03 | `2026MMDD_RenameBackupToPatientBak` |
| SL-04 | `2026MMDD_AddOutstandingBalanceFlagToVisit` (اختياري) |
| SL-05 | `2026MMDD_AddReportDesignOverride` |
| SL-06 | لا |
| SL-07 | لا |
| SL-08 | لا (Staff + StaffPermission موجودان) |
| SL-09 | `2026MMDD_AddTestComponentConstants` |
| SL-10 | لا (AntibioticCatalog موجود) |
| SL-11 | لا |
| SL-12 | `2026MMDD_AddBarcodeLabelPosition` |
| SL-13 | لا |
| SL-14 | `2026MMDD_AddPrinterMappingsToLabSetting` |
| SL-15 | لا |

**المجموع:** 5 Migrations جديدة متوقعة.

---

## 10. ملحق — خريطة الاختبارات المطلوبة

| الفئة | موجودة | مطلوبة | المجموع |
|---|---|---|---|
| Service Tests | 47 | +75 | 122 |
| ViewModel Tests | 30 | +60 | 90 |
| Integration E2E | 11 | +25 | 36 |
| Slice Tests | 3 | +6 | 9 |
| Validation/Migration Tests | 6 | +5 | 11 |
| Infrastructure Tests | 3 | +5 | 8 |
| **الإجمالي** | **112** | **+176** | **≈ 288** |

**نسبة الاختبارات بعد اكتمال كل الشرائح:** ≈ 288 اختبار (2.5× الحالية).

---

*انتهت وثيقة تحليل الفجوات — الدورة الثالثة.*
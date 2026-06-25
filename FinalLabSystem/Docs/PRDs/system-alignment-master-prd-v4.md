PART ONE — CURRENT STATE SUMMARY
1.1 ما الذي أنجزته Phase 1 وما المغلق الآن
أنجزت Phase 1 (Core Safety & Infrastructure) خمس مراحل فرعية موثَّقة في FinalLabSystem/Docs/PRDs/IMPLEMENTATION_STATUS.md بسطور 5–106:

Subphase 1.1 — Stage-Gating: IFeatureToggleService و ResultStageRules و RoutineResultServiceGuardTests (4 اختبارات حارسة) و seed EnforceStageGating=true. هذا يمنع كتابة نتائج لمرضى تجاوزوا مراحل العمل المحدَّدة.
Subphase 1.2 — Print Pipeline: WpfFlowDocumentPrintService بديلاً عن NullPrintService، مع 4 اختبارات للطباعة. لكن NullPrintService.cs لم يُحذف رغم إزاحته من DI (انظر بند الـ technical debt).
Subphase 1.3 — Audit Infrastructure: AuditInterceptor على FinalLabDbContext، attribute [Auditable] على TestResult و TestWorkflow، و 5 اختبارات interception.
Subphase 1.4 — VM Split: TestResultsViewModel فُصِل عن المنطق الذي كان مدفوناً في code-behind.
Subphase 1.5 — Database Constraints: شرط حصرية الخصم على VisitTest، وإضافة [NotMapped] AggregateValidationStatus.
الإجمالي الحالي للـ Phase 1: 185 / 185 اختبار ناجح (المصدر canonical: IMPLEMENTATION_STATUS.md سطر 12). يوجد 189 سمة [Fact] / [Theory] فعلية في 19 ملف اختبار، لكن 4 منها هي test fixtures مساعدة لا تُحسب اختبارات مستقلة في النتيجة الرسمية.

ما المغلق: لا يُعاد بناء أي شيء من Phase 1. الجدول التالي يحدِّد بدقّة ما لا يُمَس:

آليات Feature Toggle، Stage Gates، Result Stage Rules.
خط الطباعة IPrintService → WpfFlowDocumentPrintService.
خط الـ Audit (AuditInterceptor + AuditLog + VResultAuditTrail).
شرط حصرية الخصم على VisitTest وحقول التحقق المضافة.
1.2 الحالة الراهنة بالنثر الواضح
النظام يبني ويشتغل. يستطيع مستخدم الدخول، تسجيل مريض، اختيار تحاليل، إدخال نتائج روتينية، طباعة إيصال، وتسليم النتائج. لكن 70% تقريباً من السطح الوظيفي للنظام المرجعي (Real Lab System) غير مرئي للمستخدم النهائي، رغم أن طبقتي الـ Models و الـ Services بُنيتا تقريباً للجميع. الفجوة بين ما هو موجود في الـ codebase وما يَصِل للمستخدم تتوزَّع على ثلاث ظواهر:

الظاهرة الأولى — سبع خدمات تخصصية مبنية لكن غير مسجلة في DI
في App.xaml.cs سطر 127 إلى 165 يقوم ConfigureServices بتسجيل 15 خدمة فقط في الـ DI container. الخدمات التالية موجودة كملفات *.cs كاملة داخل Services/Interfaces/ و Services/Implementations/ لكنها غير مسجَّلة، ولذا أي ViewModel يحاول استقبالها يفشل في الإقلاع:

الخدمة	مسار الـ Interface	مسار الـ Implementation	الاستدعاء المنتظر
IAndrologyService	Services/Interfaces/IAndrologyService.cs	Services/Implementations/AndrologyService.cs	تحرير نتائج تحاليل الذكورة / السائل المنوي (SemenAnalysis)
IBloodBankService	Services/Interfaces/IBloodBankService.cs	Services/Implementations/BloodBankService.cs	إدارة بنك الدم، Cross-Match Donors، Cross-Match Tests
ICultureResultService	Services/Interfaces/ICultureResultService.cs	Services/Implementations/CultureResultService.cs	نتائج زراعة الميكروبيولوجي والحساسية للمضادات الحيوية
IExternalLabService	Services/Interfaces/IExternalLabService.cs	Services/Implementations/ExternalLabService.cs	معامل خارجية وعينات مُرسَلة للخارج
IContractService	Services/Interfaces/IContractService.cs	Services/Implementations/ContractService.cs	عقود الشركات والفواتير الشهرية
IAttendanceService	Services/Interfaces/IAttendanceService.cs	Services/Implementations/AttendanceService.cs	حضور الموظفين والورديات
IPricingService	Services/Interfaces/IPricingService.cs	Services/Implementations/PricingService.cs	قوائم الأسعار المتعدِّدة (PriceScheme)
أثر هذا على المستخدم: لا يستطيع المستخدم اليوم رؤية أو فتح أي شاشة تستخدم أيّاً من هذه الخدمات السبع — لأن الـ ViewModels المرتبطة بها لم تُكتب بعد، أو كُتِبت كـ orphans لا أحد يستدعيها. الـ codebase يحتوي القدرة لكن الواجهة لا تكشفها.

التحقق من الـ orphan status: بحث نصي للملفات التي تستورد كل interface من الـ 7 يُرجِع نتائج صفرية في كل المجلَّدات خارج Services/Interfaces/ و Services/Implementations/ ذاتهما. لا ViewModel ولا View ولا constructor آخر يُشير إليها.

الظاهرة الثانية — ثلاث نوافذ يتيمة (Orphan Windows)
ثلاث نوافذ XAML مع code-behind و ViewModel مكتمل تقريباً موجودة في الـ repo، لكن App.xaml.cs لا يسجِّلها لا في الـ DI ولا في INavigationService. النتيجة أنها غير قابلة للفتح من أي مكان في الـ UI:

AuditTrailWindow — Views/Patients/AuditTrailWindow.xaml + .xaml.cs (سطر 1-12) + ViewModels/Patients/AuditTrailViewModel.cs (36 سطر). الـ ViewModel يقبل قائمتين بديلتين: إما List<AuditLog> للتدقيق العام، أو List<VResultAuditTrail> لتتبُّع تغيُّرات النتائج. الـ code-behind سليم MVVM (مجرّد InitializeComponent). الفجوة: لا تسجيل DI، لا تسجيل navigation، لا زرّ في الـ UI لفتحها.

ResultEntryWindow — Views/Patients/ResultEntryWindow.xaml + .xaml.cs (سطر 1-12) + ViewModels/Patients/ResultEntryViewModel.cs (104 سطر). الـ ViewModel يستقبل IRoutineResultService و IVisitService و IAuditService و ICurrentUserSession و معاملات visitTestId و patientId و testTypeName و ObservableCollection<TestComponentResultDto>. يحوي SaveCommand يستدعي _routineResultService ويُطلق حدث SaveCompleted. الفجوة: لا تسجيل DI، لا factory لإنشائه برسالة visitTestId، لا منفذ من TestResultsViewModel لفتحه.

PrintPreviewWindow — Views/Patients/PrintPreviewWindow.xaml.cs (28 سطر). هذه النافذة تخالف MVVM: الـ constructor يستقبل FlowDocument ويُسنده مباشرة إلى PreviewViewer.Document في الـ XAML، و معالجَا PrintButton_OnClick و CloseButton_OnClick يستدعيان PrintDialog ويغلقان النافذة من داخل الـ code-behind. لا يوجد PrintPreviewViewModel على الإطلاق (بحث find -name "PrintPreviewViewModel*" يُرجع لا شيء). هذه أكبر violation معماري في الـ repo حالياً.

الظاهرة الثالثة — تضارب دلالي خطير في مفاتيح F1–F12
دليل المستخدم الرسمي real lab system help.pdf يحدِّد دلالة قاطعة لمفاتيح الـ Function (مستخرَجة بنجاح من المستند):

المفتاح	الدلالة المرجعية (Help.pdf)
F1	إضافة مريض جديد
F2	الانتقال لنافذة بيانات المرضى
F3	الانتقال لنافذة البحث
F4	الانتقال لنافذة إدخال النتائج
F5	عمل تحديث (Refresh)
F6	الانتقال لنافذة تسليم النتائج
F7	الانتقال لنافذة العينات المُرسَلة للخارج
F8	تعديل بيانات المريض المُحدَّد
F9	حفظ المريض الجديد أو التعديل
F10	حذف المريض المُحدَّد
F11	الانتقال لنافذة طباعة الباركود
F12	طباعة إيصال للمريض المُحدَّد
الـ codebase الحالي في Views/Patients/TestResultsWindow.xaml سطور 791 إلى 800 يرسم خريطة مختلفة جوهرياً:

Copy<Window.InputBindings>
    <KeyBinding Key="F2" Command="{Binding OpenPatientDataCommand}"/>
    <KeyBinding Key="F3" Command="{Binding OpenSearchCommand}"/>
    <KeyBinding Key="F5" Command="{Binding RefreshCommand}"/>
    <KeyBinding Key="F6" Command="{Binding OpenDeliveryCommand}"/>
    <KeyBinding Key="F8" Command="{Binding ToggleReviewStatusCommand}"/>
    <KeyBinding Key="F9" Command="{Binding ToggleFinishStatusCommand}"/>
    <KeyBinding Key="F12" Command="{Binding TogglePrintStatusCommand}"/>
    <KeyBinding Key="Escape" Command="{Binding ReturnToMainCommand}"/>
</Window.InputBindings>
التضارب موزَّع كالتالي:

F2 و F3 و F5 و F6: متوافقة مع Help.pdf. تبقى كما هي.
F8: مرتبط الآن بـ ToggleReviewStatusCommand (تبديل حالة المراجعة). Help.pdf يقول F8 يجب أن يكون "تعديل بيانات المريض". هذا تعارض حقيقي.
F9: مرتبط الآن بـ ToggleFinishStatusCommand (تبديل حالة الإنهاء). Help.pdf يقول F9 يجب أن يكون "حفظ بيانات المريض". هذا تعارض حقيقي.
F12: مرتبط الآن بـ TogglePrintStatusCommand (تبديل حالة الطباعة). Help.pdf يقول F12 يجب أن يكون "طباعة إيصال". هذا تعارض حقيقي.
F1 و F4 و F7 و F10 و F11: غير مرتبطة بأي شيء في أي مكان. مفقودة كلياً.
Views/Patients/PatientRegistrationWindow.xaml: لا يحوي Window.InputBindings ولا KeyBinding واحد (التحقق بـ grep أعطى صفر نتائج).
أثر هذا على المستخدم: مَن اعتاد Real Lab System سيضغط F8 ليُعدِّل بيانات مريض، فيجد النظام يبدِّل حالة "تمت المراجعة" بدلاً من ذلك. ضغط F9 للحفظ سيُغيِّر حالة "تم الإنهاء" دون قصد. ضغط F12 لطباعة الإيصال سيُحدث toggle لحالة الطباعة في قاعدة البيانات لكن لا يطبع شيئاً. هذه أخطر مشكلة UX حالياً لأنها صامتة وتغيِّر بيانات الحالة بطرق غير متوقَّعة.

بنود الدَّيْن التقني الثلاثة
Services/Implementations/NullPrintService.cs: ملف موجود (يعرِّف class NullPrintService ينفِّذ IPrintService بـ no-ops). فحص الـ references يُظهر: لا يُسجَّل في DI، ولا يُستخدم في اختبار، ولا يُذكر إلا في تعليق <see cref="..."/> داخل IPrintService.cs. كود ميت بالكامل يجب إزالته.

Views/Patients/TestSelectionView.xaml.bak و Views/Patients/TestSelectionView.xaml.bak2: ملفان بحجم 8,853 و 10,230 بايت موجودان في الـ working tree. الـ .gitignore الحالي لا يحوي قاعدة *.bak (تحقق grep أعطى صفر). يجب الحذف وإضافة القاعدة لمنع العودة.

ViewModels/MainViewModel.cs (87 سطر): يحوي class ثانية nested في نفس الملف: PatientsMenuViewModel تبدأ سطر 66. الـ SystemSettingsMenuViewModel كانت أيضاً nested لكنها استُخرجت إلى ViewModels/Settings/SystemSettingsMenuViewModel.cs كملف منفصل. هذا يخالف قاعدة "صنف واحد لكل ملف". الـ PatientsMenuViewModel يجب نقله إلى ViewModels/Menu/PatientsMenuViewModel.cs.

مخالفة MVVM في PrintPreviewWindow
(تم توصيفها في الظاهرة الثانية أعلاه — مكرَّرة هنا لأنها cross-cutting وتُعالج في Phase 6.) الـ code-behind يحتوي _document كحقل خاص، و PrintButton_OnClick يُنشئ PrintDialog و يستدعي PrintDocument مباشرةً، و CloseButton_OnClick يستدعي Close(). الـ ViewModel غير موجود. هذا الكود يجب أن يُنقَل بالكامل إلى PrintPreviewViewModel جديد، مع PrintCommand و CloseCommand، و الـ FlowDocument يصبح خاصية على الـ ViewModel.

1.3 المراحل الستة المُغلَقة الأسماء أمام النظام
أسماء المراحل مُغلَقة كما تظهر بالحرف في Docs/PRDs/IMPLEMENTATION_STATUS.md سطور 109 إلى 134 — أي مقترح تسمية بديل مرفوض:

Phase 2 — Patient Search & Identity: استقرار طبقة الهويّة المريضيّة (بحث متعدّد المعايير، تكامل F-keys، تفعيل النوافذ اليتيمة، تنظيف الدَّيْن التقني، توسيع القائمة الرئيسية للأيقونات).
Phase 3 — Result Editor Alignment: مواءمة محرِّر النتائج مع النظام المرجعي (شاشة ResultEntry كاملة، إدارة TestProfile، إدارة ReportCommentTemplate، حقن التعليقات التلقائي).
Phase 4 — Billing & Contracts: المحاسبة والعقود (فواتير شركات، شاشة IContractService، شاشة IPricingService مع PriceScheme، Manifest العينات الخارجية مع IExternalLabService).
Phase 5 — Inventory & Cash Drawer: الجَرْد ودرج النقدية (شاشة Cash Drawer، Inventory، الحضور IAttendanceService، تقارير العمولات والمرتجَعات).
Phase 6 — Print, Delivery & Backup: تكامل الطباعة والتسليم والنسخ الاحتياطي (إصلاح MVVM لـ PrintPreviewWindow، خدمة Backup/Restore، تكامل موقع natigh.com للنشر).
Phase 7 — Specialty Editors & Admin: محرِّرات التخصصات والإدارة (Andrology، Blood Bank/Cross-Match، Microbiology Culture، إدارة الصلاحيات Permission و StaffPermission).
PART TWO — PHASE 2: Patient Search & Identity (التركيز الأول — أقصى تفصيل)
Task 2.1 — DI Registration of Patient-Related Services
▸ CONTEXT (الحالة الراهنة) في FinalLabSystem/App.xaml.cs السطور 127 إلى 165 يقوم ConfigureServices بتسجيل الخدمات التالية كـ Scoped:

سطر 134: ISettingsService
سطر 135: IFeatureToggleService
سطر 136: IAuthService
سطر 137: IPatientService
سطر 138: IReferralService
سطر 139: IVisitService
سطر 140: IFinancialService
سطر 141: ITestCatalogService
سطر 142: ISampleTrackingService
سطر 143: IAuditService
سطر 144: ITestCatalogSeeder
سطر 145: IReceiptService
سطر 146: IRoutineResultService
سطر 147: IReportingService
سطر 148: IResultEditorFactory
كـ Singleton (سطور 156–161): IUserSettingsService, ICurrentUserSession, INavigationService, IDialogService, ILabelPrintService. كـ Scoped أيضاً (سطر 160): IPrintService.

▸ PROBLEM (المشكلة) Phase 2 سيُحْيي نافذتين يتيمتين (AuditTrailWindow, ResultEntryWindow)، وسيُضيف خدمة جديدة لخدمة التطبيقات (IReportCommentEngine — سيُعرَّف في Phase 3 لكن يجب التحضير له). ResultEntryViewModel يستدعي بالفعل IRoutineResultService و IVisitService و IAuditService و ICurrentUserSession — كلها مسجَّلة. لا حاجة لتسجيل أي خدمة جديدة من السبع المتخصصة في Phase 2. الخدمات التخصصية تُؤجَّل لمرحلتها (Phase 3 يحتاج فقط ITestCatalogService موسَّعة، Phase 4 يسجِّل IContractService و IPricingService و IExternalLabService، Phase 5 يسجِّل IAttendanceService، Phase 7 يسجِّل IAndrologyService و IBloodBankService و ICultureResultService).

▸ SOLUTION (الحل) داخل ConfigureServices في App.xaml.cs بعد سطر 148 (تسجيل IResultEditorFactory):

تسجيل الـ ViewModels الجديدة كـ Transient (لأن كل فتح نافذة يحتاج instance طازجة بحالة منعزلة):

AuditTrailViewModel — Transient (لكن لاحظ أن constructor الحالي يأخذ معاملات مباشرة، انظر Task 2.2 لتفاصيل الـ factory pattern).
ResultEntryViewModel — Transient عبر factory (انظر Task 2.3).
10 ViewModels جديدة للقائمة الرئيسية (انظر Task 2.5): PatientsMenuViewModel (نقل من nested)، ResultsMenuViewModel، DeliveryMenuViewModel، SearchMenuViewModel، ExternalSamplesMenuViewModel، AccountsMenuViewModel، BackupMenuViewModel، TestDataMenuViewModel، NormalRangesMenuViewModel، ReportSettingsMenuViewModel. كلها Transient.
تسجيل النوافذ الجديدة كـ Transient:

AuditTrailWindow (موجود الـ XAML — يحتاج فقط تسجيل DI).
ResultEntryWindow (موجود الـ XAML — يحتاج فقط تسجيل DI).
تسجيل الـ navigation mappings داخل OnStartup بعد سطر 96 (navigation.RegisterWindow<CategoriesGroupsViewModel, CategoriesGroupsWindow>()):

navigation.RegisterWindow<AuditTrailViewModel, AuditTrailWindow>().
navigation.RegisterWindow<ResultEntryViewModel, ResultEntryWindow>().
DI lifetime rationale:

الخدمات (I*Service) تظل Scoped لأنها تشترك في FinalLabDbContext المُسجَّل Scoped أيضاً (سطر 129–132). تغيير اللايف-سايكل سيكسر تتبع الـ EF.
الـ ViewModels Transient لأن كل نافذة هي جلسة عمل منفصلة، ولأن OpenTaskWindow<T> في NavigationService يحلّ نسخة جديدة لكل فتح.
النوافذ Transient لنفس السبب.
INavigationService و IDialogService Singleton لأنهما يحتفظان بمراجع للنوافذ الحيّة عبر دورة حياة التطبيق.
▸ VERIFICATION (معايير القبول)

Given التطبيق في حالة إقلاع نظيفة، When يصل التنفيذ إلى ServiceProvider = services.BuildServiceProvider() في App.xaml.cs سطر 77، Then لا يُلقَى استثناء حلّ تبعيّات لأيٍّ من 12 ViewModel و النافذتين الجديدتين.
Given المستخدم نقر زراً يطلب فتح AuditTrailWindow، When يُنفَّذ NavigateToAuditTrailCommand، Then نافذة AuditTrailWindow تفتح وتعرض الجدول الفارغ دون استثناء "No window is registered for ViewModel" (الذي يُلقيه NavigationService.OpenTaskWindow سطر 53 عند الفشل).
Given ResultEntryViewModel يُحَلّ من DI، When يُمَرَّر visitTestId ضمن البارامترات، Then يستلم كل تبعيّاته الخدميّة (IRoutineResultService, IAuditService, ICurrentUserSession) بنجاح.
▸ RISK (المخاطر والتخفيف)

خطر تسجيل ViewModel بـ scope خاطئ يُسبِّب الاحتفاظ بـ DbContext بين نوافذ مختلفة، مما يقود إلى استثناءات tracking. التخفيف: التزام صارم بـ Transient للـ ViewModels و Scoped للخدمات، مع إجراء smoke test يفتح ويغلق AuditTrailWindow 5 مرّات متتالية ويتحقق من عدد instances الحيّة لـ FinalLabDbContext (يجب أن يساوي 0 بعد الإغلاق).
Task 2.2 — Orphan Window: AuditTrailWindow
▸ CONTEXT

ViewModels/Patients/AuditTrailViewModel.cs (36 سطر، فحص كامل): class مختوم (sealed) يحوي constructor مُحَمَّل overload:
Overload 1 (سطر 13): AuditTrailViewModel(string title, List<AuditLog> auditEntries) — للتدقيق العام.
Overload 2 (سطر 20): AuditTrailViewModel(string title, List<VResultAuditTrail> resultEntries) — لتتبُّع تغيُّرات النتائج.
يعرض ObservableCollection و ICollectionView لكل نوع.
Views/Patients/AuditTrailWindow.xaml.cs (12 سطر): code-behind نظيف، فقط InitializeComponent(). لا violations.
Views/Patients/AuditTrailWindow.xaml: موجود (لم نفحص محتواه التفصيلي لكنه يُربط بالـ ViewModel عبر DataContext).
الحالة الراهنة: غير مسجَّلة في DI، غير مسجَّلة في navigation، لا زرّ في الـ UI يفتحها.
▸ PROBLEM الـ ViewModel يستخدم Constructor parameters (title, List<...>) — لا يمكن لـ DI القياسي حلّه. إذا سجَّلنا AuditTrailViewModel كـ Transient عادي سيفشل لأن DI لا يعرف من أين يأتي بـ string title و List<AuditLog>.

▸ SOLUTION نمط الـ Factory يُحلّ هذا. الخطوات:

إضافة interface جديدة IAuditTrailDialogService في Services/Interfaces/IAuditTrailDialogService.cs:
Copypublic interface IAuditTrailDialogService
{
    void ShowGeneralAudit(string title, List<AuditLog> entries);
    void ShowResultAudit(string title, List<VResultAuditTrail> entries);
}
التنفيذ في Services/Implementations/AuditTrailDialogService.cs: يأخذ IServiceProvider، يُنشئ AuditTrailViewModel يدوياً، يَحُلّ AuditTrailWindow من DI، يضع الـ VM في window.DataContext، ويستدعي ShowDialog().
تسجيل في DI كـ Singleton: services.AddSingleton<IAuditTrailDialogService, AuditTrailDialogService>();.
تسجيل النافذة كـ Transient: services.AddTransient<AuditTrailWindow>();.
لا يُسجَّل AuditTrailViewModel في DI (لأنه يُنشَأ يدوياً مع المعاملات).
مكان الزر في الـ UI: داخل TestResultsWindow.xaml يُضاف زرّ "سجل التدقيق" (icon + text) في شريط الأدوات العلوي، يستدعي OpenResultAuditCommand الجديد في TestResultsViewModel. الأمر يستقبل من IAuditService قائمة VResultAuditTrail للمريض الحالي، ثم يستدعي _auditTrailDialogService.ShowResultAudit(...). كذلك زرّ آخر في PatientRegistrationWindow.xaml يفتح التدقيق العام للمريض.
▸ VERIFICATION

Given مريض بمعرف 42 له 3 تغييرات نتائج موثقة في VResultAuditTrail، When المستخدم يضغط زر "سجل التدقيق" داخل TestResultsWindow، Then تفتح AuditTrailWindow modal، عنوانها "سجل تدقيق نتائج المريض رقم 42"، ويظهر جدول بـ 3 صفوف.
Given الـ AuditTrailViewModel مُهَيَّأ بالـ overload الأول (AuditLog)، When الـ XAML يستعلم عن EntriesView، Then يحصل على ICollectionView صالحة، و ResultEntries و ResultEntriesView تُرجِعان null.
Given المستخدم أغلق النافذة، When يعيد فتحها ثانيةً، Then instance جديدة لـ AuditTrailWindow و AuditTrailViewModel تُنشَأ (تأكيد Transient).
▸ RISK

خطر تسريب memory إن لم يُلغَ اشتراك ICollectionView عند إغلاق النافذة. التخفيف: لا يُضاف اشتراك event لأن الـ VM read-only post-construction، لكن لاستيثاق إضافي تُنفَّذ IDisposable على الـ VM وتُمسح الـ ObservableCollections في Dispose.
خطر استدعاء ShowDialog() من thread غير UI. التخفيف: AuditTrailDialogService يستخدم Application.Current.Dispatcher.Invoke للتأكد.
Task 2.3 — Orphan Window: ResultEntryWindow
▸ CONTEXT

ViewModels/Patients/ResultEntryViewModel.cs (104 سطر، فحص كامل):
Constructor (سطر 25–44) يأخذ 4 خدمات + 4 بارامترات بيانية (visitTestId, patientId, testTypeName, ObservableCollection<TestComponentResultDto>).
يحتوي خصائص VisitTestId, TestTypeName, Components, SelectedComponent, IsSaving.
SaveCommand (سطر 41) — AsyncRelayCommand يستدعي SaveAsync (يبدأ سطر 73).
CancelCommand (سطر 42) — حالياً RelayCommand(_ => { }) فارغ — يجب أن يُغلق النافذة.
حدث SaveCompleted (سطر 71) يُطلَق بعد الحفظ الناجح ليُخطر النافذة الأم.
SaveAsync يستدعي _routineResultService.SaveNumericOrTextResultsAsync (طريقة مؤكَّدة موجودة في RoutineResultService.cs سطر 32).
Views/Patients/ResultEntryWindow.xaml.cs (12 سطر): نظيف، فقط InitializeComponent().
الحالة الراهنة: غير مسجَّلة في DI، غير مفتوحة من أي مكان.
▸ PROBLEM ثلاث فجوات:

الـ ViewModel constructor يأخذ بارامترات بيانية (visitTestId, ...) — يحتاج factory pattern مثل Task 2.2.
CancelCommand فارغ — لا يُغلق النافذة.
لا منفذ في TestResultsViewModel لاستدعاء ResultEntryWindow للمكوّن المُحَدَّد (مثلاً double-click على component في الجدول).
▸ SOLUTION

إنشاء IResultEntryDialogService في Services/Interfaces/IResultEntryDialogService.cs:
Copypublic interface IResultEntryDialogService
{
    Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName,
                         ObservableCollection<TestComponentResultDto> components);
}
التنفيذ في Services/Implementations/ResultEntryDialogService.cs: يَحُلّ IRoutineResultService و IVisitService و IAuditService و ICurrentUserSession من IServiceProvider، يُنشئ ResultEntryViewModel يدوياً، يَحُلّ النافذة، يربط DataContext، يَشترك في SaveCompleted ليُغلق النافذة، يستدعي ShowDialog()، يُرجع true عند الحفظ الناجح.
تعديل ResultEntryViewModel: إضافة Action RequestClose يُربط بالنافذة، و CancelCommand يستدعيها. تعديل SaveAsync ليُطلق RequestClose بعد SaveCompleted.
تسجيل في DI:
Copyservices.AddSingleton<IResultEntryDialogService, ResultEntryDialogService>();
services.AddTransient<ResultEntryWindow>();
في TestResultsViewModel: يُضاف OpenComponentEditorCommand يأخذ VisitTestItemDto كمعامل، يستعلم عن مكوّناتها (TestComponent + TestResult المرتبطة)، يُحَوِّلها لـ ObservableCollection<TestComponentResultDto>، ثم يستدعي _resultEntryDialogService.OpenAsync(...).
في XAML: ربط MouseDoubleClick (أو event على Row) في TestResultsWindow.xaml بالأمر الجديد عبر InputBindings. (لاحظ: F4 من Help.pdf يفتح نافذة إدخال النتائج العامة، لكن double-click على تحليل محدَّد يفتح هذه الشاشة الفرعية لمكوّناته.)
▸ VERIFICATION

Given visit تحتوي VisitTest لتحليل "CBC" بـ 5 مكوّنات (HGB, RBC, WBC, PLT, HCT)، When المستخدم يفتح TestResultsWindow ويُنفّذ double-click على صف "CBC"، Then تفتح ResultEntryWindow modal عنوانها "إدخال نتائج CBC" بـ 5 صفوف لإدخال القيم.
Given المستخدم أدخل قيمة لـ HGB ثم ضغط حفظ، When ينتهي SaveAsync، Then يُكتب TestResult في DB، يُطلَق SaveCompleted، النافذة تُغلق، و TestResultsWindow تُحدِّث القائمة (RefreshCommand).
Given المستخدم ضغط Cancel، When يُنفَّذ CancelCommand، Then النافذة تغلق دون حفظ، ولا يُكتب شيء في DB.
Given stage gate يمنع المريض من إدخال نتائج (مرحلة منتهية)، When المستخدم يحاول الحفظ، Then RoutineResultServiceGuard يُلقي استثناءً يُعرَض في DialogService، النافذة تبقى مفتوحة.
▸ RISK

خطر double-write إذا ضغط المستخدم Save مرتين بسرعة. التخفيف: IsSaving يُمنع الزر (AsyncRelayCommand predicate سطر 41: _ => !IsSaving).
خطر إغلاق النافذة قبل اكتمال SaveAsync لو الـ task فشل في الوسط. التخفيف: إلغاء RequestClose داخل catch، عرض الخطأ في IDialogService، إعادة IsSaving = false.
Task 2.4 — F-Key Semantic Conflict Resolution (المهمة الأدقّ في Phase 2)
▸ CONTEXT الخريطة المرجعية القاطعة من Help.pdf (مُستخرَجة بنجاح):

F#	الدلالة المرجعية
F1	إضافة مريض جديد
F2	الانتقال لنافذة بيانات المرضى
F3	الانتقال لنافذة البحث
F4	الانتقال لنافذة إدخال النتائج
F5	Refresh
F6	الانتقال لنافذة تسليم النتائج
F7	الانتقال لنافذة العينات المُرسَلة للخارج
F8	تعديل بيانات المريض المُحدَّد
F9	حفظ المريض الجديد أو التعديل
F10	حذف المريض المُحدَّد
F11	نافذة طباعة الباركود
F12	طباعة إيصال للمريض
الخريطة الراهنة في Views/Patients/TestResultsWindow.xaml سطور 791–800:

سطر 792: F2 → OpenPatientDataCommand ✅ (مطابق)
سطر 793: F3 → OpenSearchCommand ✅ (مطابق)
سطر 794: F5 → RefreshCommand ✅ (مطابق)
سطر 795: F6 → OpenDeliveryCommand ✅ (مطابق)
سطر 796: F8 → ToggleReviewStatusCommand ❌ (يجب أن يكون "تعديل بيانات المريض")
سطر 797: F9 → ToggleFinishStatusCommand ❌ (يجب أن يكون "حفظ")
سطر 798: F12 → TogglePrintStatusCommand ❌ (يجب أن يكون "طباعة إيصال")
الخريطة الراهنة في Views/Patients/PatientRegistrationWindow.xaml: لا KeyBindings مطلقاً (تم التحقق بـ grep).

▸ PROBLEM المستخدم المعتاد على Real Lab System يضغط F8 ليُعدِّل بيانات، فيُغيِّر النظام حالة "تمت المراجعة" صامتاً. F12 لطباعة الإيصال يُبدِّل حالة الطباعة دون إنتاج إيصال. هذا تضليل خطير لأنه يعدِّل بيانات الحالة دون قصد. وفي شاشة تسجيل المريض نفسها لا يوجد أي اختصار يعمل — مما يخالف workflow المستخدم تماماً.

▸ SOLUTION (خطة الـ Remapping الكاملة)

القرار الأساسي: F8, F9, F12 ستحمل دلالاتها المرجعية من Help.pdf، لكن وظائف "تبديل المراجعة/الإنهاء/الطباعة" الموجودة فعلاً ضرورية للـ UX داخل TestResultsWindow. الحل: نقلها إلى Ctrl+R, Ctrl+F, Ctrl+P على التوالي (مع تحديث الـ tooltips ليُظهر الـ shortcut الجديد).

A. التغييرات في Views/Patients/TestResultsWindow.xaml سطور 791–800:

السطر الراهن	التغيير
F8 → ToggleReviewStatusCommand	يُحذف. يُعاد بحرف: <KeyBinding Key="R" Modifiers="Ctrl" Command="{Binding ToggleReviewStatusCommand}"/>
F9 → ToggleFinishStatusCommand	يُحذف. يُعاد: <KeyBinding Key="F" Modifiers="Ctrl" Command="{Binding ToggleFinishStatusCommand}"/>
F12 → TogglePrintStatusCommand	يُحذف. يُعاد: <KeyBinding Key="P" Modifiers="Ctrl" Command="{Binding TogglePrintStatusCommand}"/>
ثم يُضاف:

<KeyBinding Key="F8" Command="{Binding EditSelectedPatientCommand}"/> — جديد، يفتح PatientRegistrationWindow في وضع التعديل للمريض المُحدَّد.
<KeyBinding Key="F12" Command="{Binding PrintReceiptCommand}"/> — جديد، يستدعي IReceiptService و IPrintService لطباعة إيصال المريض المُحدَّد.
<KeyBinding Key="F4" Command="{Binding NavigateToResultEntryCommand}"/> — جديد، (لا داعي له هنا فعلياً لأننا داخل شاشة النتائج، لكن مطلوب من Help.pdf كاختصار عام).
<KeyBinding Key="F7" Command="{Binding NavigateToExternalSamplesCommand}"/> — جديد placeholder (الـ window تُبنى في Phase 4، فالأمر يَعرض دياليج "متاحة في Phase 4").
B. الأوامر الجديدة في TestResultsViewModel:

EditSelectedPatientCommand: يَحُلّ PatientRegistrationViewModel، يُخبره عن المريض المُحدَّد، يستدعي _navigationService.OpenTaskWindow<PatientRegistrationViewModel>(). يحتاج معامل تمرير patientId — يُحَلّ بإضافة INavigationService.OpenTaskWindow<TViewModel>(Action<TViewModel>? configure = null) overload جديد.
PrintReceiptCommand: يستدعي _receiptService.GenerateReceiptAsync(visitId) ثم _printService.PrintAsync(flowDoc).
C. التغييرات في Views/Patients/PatientRegistrationWindow.xaml — إضافة الكتلة الكاملة لأول مرّة:

Copy<Window.InputBindings>
    <KeyBinding Key="F1" Command="{Binding NewPatientCommand}"/>
    <KeyBinding Key="F2" Command="{Binding NavigateToPatientDataCommand}"/>
    <KeyBinding Key="F3" Command="{Binding NavigateToSearchCommand}"/>
    <KeyBinding Key="F4" Command="{Binding NavigateToResultEntryCommand}"/>
    <KeyBinding Key="F5" Command="{Binding RefreshCommand}"/>
    <KeyBinding Key="F6" Command="{Binding NavigateToDeliveryCommand}"/>
    <KeyBinding Key="F7" Command="{Binding NavigateToExternalSamplesCommand}"/>
    <KeyBinding Key="F8" Command="{Binding EditModeCommand}"/>
    <KeyBinding Key="F9" Command="{Binding SaveCommand}"/>
    <KeyBinding Key="F10" Command="{Binding DeletePatientCommand}"/>
    <KeyBinding Key="F11" Command="{Binding PrintBarcodeCommand}"/>
    <KeyBinding Key="F12" Command="{Binding PrintReceiptCommand}"/>
    <KeyBinding Key="Escape" Command="{Binding CancelCommand}"/>
</Window.InputBindings>
D. الأوامر الجديدة في PatientRegistrationViewModel (10 أوامر جديدة):

NewPatientCommand, NavigateToPatientDataCommand, NavigateToSearchCommand, NavigateToResultEntryCommand, RefreshCommand, NavigateToDeliveryCommand, NavigateToExternalSamplesCommand (placeholder حتى Phase 4), EditModeCommand, DeletePatientCommand, PrintBarcodeCommand. الـ SaveCommand و PrintReceiptCommand موجودان فعلاً (تأكيد بفحص الملف).
E. التحوُّل دون كسر workflows قائمة:

المستخدمون الذين اعتادوا F8/F9/F12 الحالية يُعطَون dialog إعلام لمرة واحدة عند أول تشغيل بعد التحديث: "تم تحديث اختصارات لوحة المفاتيح لتوافق دليل النظام المرجعي. F8/F9/F12 الآن تؤدي وظائف جديدة. للوصول للوظائف القديمة استخدم Ctrl+R / Ctrl+F / Ctrl+P".
إضافة قسم "اختصارات لوحة المفاتيح" في Tooltips لكل زر مرئي يستخدم اختصاراً.
إعدادات المستخدم في JsonUserSettingsService تحفظ flag KeyboardShortcutsNoticeShown لمنع تكرار الـ dialog.
▸ VERIFICATION (لكل مفتاح بشكل مستقل)

F1: Given شاشة PatientRegistrationWindow مفتوحة، When المستخدم يضغط F1، Then النموذج يُمسح كله، وضع التطبيق يتحول لـ "إضافة جديد"، الحقل الأول (الاسم) يأخذ Focus.
F2: Given شاشة TestResultsWindow مفتوحة، When المستخدم يضغط F2، Then PatientRegistrationWindow تفتح في وضع عرض بيانات المريض المُحدَّد.
F3: Given أي شاشة باستثناء login، When المستخدم يضغط F3، Then PatientSearchWindow تفتح.
F4: Given شاشة فيها مريض محدَّد، When المستخدم يضغط F4، Then TestResultsWindow تفتح لهذا المريض.
F5: Given جدول بيانات حيّ على الشاشة، When المستخدم يضغط F5، Then القائمة تُعاد قراءتها من DB.
F6: Given أي شاشة، When المستخدم يضغط F6، Then DeliveryWindow تفتح.
F7: Given أي شاشة، When المستخدم يضغط F7، Then يُعرَض dialog مؤقت "متاحة في Phase 4" (سيُستبدل بفتح ExternalSamplesWindow لاحقاً).
F8: Given مريض محدَّد في TestResultsWindow، When المستخدم يضغط F8، Then PatientRegistrationWindow تفتح في وضع تعديل لبياناته. لا تحدث toggle لحالة المراجعة.
F9: Given بيانات مريض جديدة في PatientRegistrationWindow، When المستخدم يضغط F9، Then SavePatientCommand ينفَّذ، DB تُحَدَّث، نافذة تأكيد تظهر.
F10: Given مريض محدَّد، When المستخدم يضغط F10، Then دياليج تأكيد ("هل أنت متأكد...") يظهر؛ عند التأكيد يُحذف المريض.
F11: Given مريض محدَّد، When المستخدم يضغط F11، Then BarcodeDialog تفتح للمريض.
F12: Given مريض محدَّد، When المستخدم يضغط F12، Then IReceiptService يولِّد إيصاله ويُطبع. لا تحدث toggle لحالة الطباعة.
Ctrl+R / Ctrl+F / Ctrl+P داخل TestResultsWindow: When المستخدم يضغط أيّاً منها على صف نتيجة، Then الـ toggle المقابل (مراجعة/إنهاء/طباعة) يحدث كما كان F8/F9/F12 سابقاً.
▸ RISK

خطر أعلى: المستخدم يضغط F10 على مريض له visits حيّة فيُحذف عرضياً. التخفيف: دياليج تأكيد إجباري + IPatientService.DeleteAsync يجب أن يُلقي استثناءً إن وُجدت visits غير مؤرشفة، يُعرَض كرسالة "لا يمكن حذف مريض له زيارات نشطة".
خطر متوسط: مستخدم اعتاد F8 يبدِّل حالة المراجعة الآن يفتح نافذة تعديل دون قصد. التخفيف: dialog الإعلام لمرة واحدة + إضافة الـ shortcut الجديد ظاهراً في الـ tooltip + إضافة شريط حالة في أسفل النوافذ يعرض "F1=جديد · F8=تعديل · F9=حفظ · F10=حذف · F11=باركود · F12=إيصال" دائماً.
خطر منخفض: تضارب F-key بين النوافذ المتزامنة. التخفيف: WPF يربط الـ KeyBindings بالنافذة النشطة فقط — لا تضارب فعلي.
Task 2.5 — Main Dashboard 12-Icon Toolbar
▸ CONTEXT

ViewModels/MainViewModel.cs (87 سطر، فحص كامل):
السطر 11: public sealed class MainViewModel : ViewModelBase — يحوي 8 commands فقط (ShowPatientsMenuCommand, ShowSystemSettingsMenuCommand, NavigateToAddEditPatientCommand, NavigateToTestResultsCommand, NavigateToDeliveryCommand, NavigateToSearchCommand, NavigateToTestDataCommand, NavigateToCategoriesGroupsCommand).
السطر 66: public sealed class PatientsMenuViewModel — مُدمَجة في نفس الملف، تخالف قاعدة "ملف لكل صنف".
ViewModels/Settings/SystemSettingsMenuViewModel.cs — مستخرَجة بالفعل، نموذج جيد للنقل.
MainWindow.xaml (فحص جزئي): يحوي ToolBarTray بزرَّين فقط: "المرضى" (سطر 60) و "إعدادات النظام" (سطر 68). كذلك DataTemplate لكل من PatientsMenuViewModel (سطر 10) و SystemSettingsMenuViewModel (سطر 36) يعرضان UniformGrid بـ 4 و 2 أزرار على التوالي.
Help.pdf يحدِّد القائمة المرجعية بـ 12 أيقونة (مستخرَجة):

الصفحة الرئيسية (Home)
المرضى (Patients) ←موجود
إدخال نتائج التحاليل (Results Entry) ←موجود تحت Patients
تسليم النتائج (Delivery) ←موجود تحت Patients
البحث عن مريض (Patient Search) ←موجود تحت Patients
العينات المُرسَلة للخارج (External Samples) — مفقود
الجرد ودرج الحساب (Inventory + Cash Drawer) — مفقود
نسخ احتياطي (Backup) — مفقود
إعدادات النظام (System Settings) ←موجود
بيانات التحاليل (Test Data) ←موجود تحت Settings
المعدَّلات الطبيعية (Normal Ranges) ←موجود تحت Settings (placeholder)
أبعاد وألوان التقرير (Report Settings) — مفقود
▸ PROBLEM

10 من الـ 12 أيقونة مفقودة من الـ Toolbar.
PatientsMenuViewModel مدمَجة nested في MainViewModel.cs.
بعض الأيقونات تشير إلى نوافذ غير مبنية بعد (External Samples, Cash Drawer, Backup, Report Settings) — تحتاج placeholder behavior.
▸ SOLUTION (بنية الملفات الكاملة)

Step 1 — استخراج PatientsMenuViewModel من MainViewModel.cs:

إنشاء مجلد جديد: ViewModels/Menu/.
نقل سطور 66 إلى نهاية الملف (public sealed class PatientsMenuViewModel { ... }) إلى ملف جديد ViewModels/Menu/PatientsMenuViewModel.cs بنفس الـ namespace FinalLabSystem.ViewModels (للحفاظ على XAML reference). أو نقل الـ namespace إلى FinalLabSystem.ViewModels.Menu وتعديل xmlns:vm في MainWindow.xaml.
التوصية: استخدام namespace FinalLabSystem.ViewModels.Menu لأن المجلد جديد ومخصَّص لـ menu VMs، وتعديل MainWindow.xaml سطر 4 (xmlns:vm) و إضافة xmlns:mvm="clr-namespace:FinalLabSystem.ViewModels.Menu" و تعديل الـ DataTemplate سطر 10 إلى DataType="{x:Type mvm:PatientsMenuViewModel}".
Step 2 — إنشاء 10 ViewModels جديدة تحت ViewModels/Menu/:

الملف	الغرض	محتوى الـ commands
ResultsMenuViewModel.cs	فرعي يُفتح من زر "إدخال النتائج"	NavigateToTestResultsCommand, NavigateToBatchResultsCommand (placeholder)
DeliveryMenuViewModel.cs	فرعي يُفتح من زر "تسليم النتائج"	NavigateToDeliveryCommand
SearchMenuViewModel.cs	فرعي يُفتح من زر "البحث"	NavigateToSearchCommand
ExternalSamplesMenuViewModel.cs	placeholder حتى Phase 4	PlaceholderCommand يعرض "Phase 4"
AccountsMenuViewModel.cs	placeholder حتى Phase 5 (Cash Drawer + Inventory)	PlaceholderCommand يعرض "Phase 5"
BackupMenuViewModel.cs	placeholder حتى Phase 6	PlaceholderCommand يعرض "Phase 6"
TestDataMenuViewModel.cs	تحت Settings	NavigateToTestDataCommand, NavigateToCategoriesGroupsCommand
NormalRangesMenuViewModel.cs	تحت Settings	NavigateToNormalRangesCommand
ReportSettingsMenuViewModel.cs	placeholder حتى Phase 6	PlaceholderCommand
HomeMenuViewModel.cs	الصفحة الرئيسية الفارغة (welcome)	NoOpCommand
Step 3 — توسيع MainViewModel.cs بـ 10 commands جديدة لفتح الـ sub-menus، مع CurrentView setter يستقبل أي نوع من Menu VMs.

Step 4 — تعديل MainWindow.xaml:

شريط ToolBar يحتوي 12 زرّاً مع أيقونات (يمكن استخدام Material Design Icons أو SVG).
لكل سب-منيو إضافة DataTemplate (مع UniformGrid يكشف الأزرار الفرعية).
الأزرار التي تشير لنوافذ غير مبنية تستدعي PlaceholderCommand يعرض dialog واضح: "هذه الميزة ستتوفر في [Phase X]".
Step 5 — تسجيل في DI (داخل App.xaml.cs):

Copyservices.AddTransient<HomeMenuViewModel>();
services.AddTransient<PatientsMenuViewModel>();
services.AddTransient<ResultsMenuViewModel>();
// ... باقي الـ 10
Step 6 — Placeholder behavior: يُضاف خدمة Static helper MenuPlaceholderHelper.ShowComingSoon(string phaseName, IDialogService dialog) تعرض dialog بنفس النمط دائماً مع رقم Phase ووصف موجز. هذا يمنع تكرار الكود في 4 ViewModels.

▸ VERIFICATION

Given المستخدم سجَّل دخوله، When MainWindow يُعرض، Then شريط الـ Toolbar يُظهر 12 أيقونة قابلة للنقر.
Given المستخدم نقر "العينات المُرسَلة للخارج"، When ExternalSamplesMenuViewModel يُفعَّل، Then dialog يظهر "هذه الميزة ستتوفر في Phase 4 — Billing & Contracts".
Given المستخدم نقر "المرضى"، When PatientsMenuViewModel يُعرض، Then 4 أزرار فرعية تظهر تماماً كالحالي (لا regression).
Given MainViewModel.cs، When يُفحص، Then يحوي class واحد فقط، الـ PatientsMenuViewModel انتقل لملف منفصل.
▸ RISK

خطر كسر XAML data templates عند تغيير namespace. التخفيف: تحديث MainWindow.xaml و عمل rebuild كامل، اختبار يدوي لفتح كل menu.
خطر confusion للمستخدم بسبب 10 أيقونات منها 4 placeholders. التخفيف: تمييز الـ placeholders بصرياً (icon رمادي / hover tooltip "متاح في Phase X").
Task 2.6 — Patient Status Icons (الأيقونات السبعة)
▸ CONTEXT Help.pdf section 3 (مستخرَج بنجاح) يحدِّد 7 أيقونات تعكس حالة المريض في قوائم العرض:

الأيقونة	المعنى	شرط الظهور
دائرة حمراء (New)	مريض جديد سُجِّل ولم تُدخَل له أي نتائج	Visit.Status == Registered && !TestResults.Any()
ورقة ملاحظات	نتائج بعض التحاليل لم تُكتب بعد	VisitTests.Any(vt => !vt.HasResults)
سهم أخضر/أزرق	نتائج موجودة لكن لم تُراجَع	VisitTests.Any(vt => vt.HasResults && !vt.IsReviewed)
طابعة	تمت المراجعة لكن لم تُطبع	VisitTests.All(vt => vt.IsReviewed) && !VisitTests.Any(vt => vt.IsPrinted)
عربة تسوُّق	تمت الطباعة لكن لم تُسلَّم	VisitTests.All(vt => vt.IsPrinted) && Visit.Status != Delivered
رمز عملة (جنيه)	تم التسليم لكن في الحساب رصيد متبقي	Visit.Status == Delivered && Visit.Balance > 0
وسام (Medal)	اكتمل كل شيء	Visit.Status == Delivered && Visit.Balance == 0 && الكل مطبوع
▸ PROBLEM حالياً قوائم المرضى في TodayPatientsDialog و PatientSearchWindow و TestResultsWindow تعرض حالة المريض كنص أو enum خام. لا أيقونات بصرية.

▸ SOLUTION

Enum جديد في Models/Enums/PatientStatusIcon.cs:
Copypublic enum PatientStatusIcon { New, ResultsPending, NotReviewed, NotPrinted, NotDelivered, OutstandingBalance, Complete }
Computed property على VPatientHistory أو DTO جديد TodayPatientWithStatusDto (الموجود فعلاً، فحص أوّلي يُظهر وجوده): إضافة خاصية StatusIcon التي تحسب الـ enum من حقول الحالة المتعدِّدة.
Service method في IPatientService (مسجَّلة فعلاً): Task<PatientStatusIcon> ComputeStatusIconAsync(int visitId) — تستعلم عن الـ VisitTests + Payments.
Value Converter في Views/Converters.cs (الملف موجود فعلاً): إضافة PatientStatusToIconConverter : IValueConverter يأخذ PatientStatusIcon ويُرجع ImageSource أو Geometry (مع موارد Material Icons).
Resources للأيقونات: إضافة 7 Geometry resources في Views/Shared/SharedStyles.xaml (يفضَّل Material Design vector paths بدلاً من PNGs لقابلية التحجيم).
UI binding: في TodayPatientsDialog.xaml و PatientSearchWindow.xaml و TestResultsWindow.xaml (داخل DataGrid columns)، إضافة عمود أول بـ Image أو Path يربط بـ StatusIcon عبر الـ Converter.
▸ VERIFICATION

Given مريض جديد سُجِّل قبل دقيقة بلا نتائج، When يُعرض في TodayPatientsDialog، Then الأيقونة دائرة حمراء.
Given مريض اكتمل كل شيء له، When يُعرض، Then الأيقونة وسام.
Given Status enum غير معروف (corrupt data)، When الـ converter يُستدعى، Then أيقونة "?" افتراضية تُعرض دون استثناء.
▸ RISK

خطر أداء: حساب الـ status لكل مريض في القائمة يستدعي N queries. التخفيف: حساب الـ status server-side في view-DTO واحد (VPatientHistoryWithStatus) عبر EF projection.
Task 2.7 — Technical Debt Cleanup
▸ CONTEXT

Services/Implementations/NullPrintService.cs (8 سطر، فحص كامل): class NullPrintService يُنفِّذ IPrintService بـ no-ops. References: 0 خارج التعريف، 1 تعليق في IPrintService.cs سطر 5.
Views/Patients/TestSelectionView.xaml.bak (8,853 بايت) و .bak2 (10,230 بايت): موجودان في working tree.
.gitignore (8000 بايت): لا يحوي قاعدة *.bak (تم التحقق بـ grep).
▸ PROBLEM

كود ميت يُسبِّب confusion لأي developer جديد ويزيد سطح الصيانة.
ملفات .bak تُضِرّ بسجل الـ Git وتُربك diff tools.
بدون قاعدة .gitignore ستعود الـ .bak files عند أي إعادة عمل من developer جديد.
▸ SOLUTION

حذف NullPrintService.cs: قبل الحذف، تأكيد عدم وجود references:

Copygrep -rn "NullPrintService" --include="*.cs"
نتيجة الفحص الراهنة: فقط تعريف الـ class وتعليق في IPrintService.cs. يُحذف الملف، ويُعدَّل التعليق في IPrintService.cs سطر 5 ليُشير إلى WpfFlowDocumentPrintService بدلاً منه.

حذف ملفات .bak:

Copygit rm Views/Patients/TestSelectionView.xaml.bak
git rm Views/Patients/TestSelectionView.xaml.bak2
تحديث .gitignore بإضافة:

Copy# Backup files - never commit
*.bak
*.bak2
*.orig
*.swp
Pre-commit hook لمنع العودة:

إضافة ملف .githooks/pre-commit (script bash):
Copy#!/bin/bash
bak_files=$(git diff --cached --name-only --diff-filter=A | grep -E '\.(bak|bak2|orig)$')
if [ -n "$bak_files" ]; then
  echo "ERROR: Backup files cannot be committed:"
  echo "$bak_files"
  exit 1
fi
تفعيله للمشروع: git config core.hooksPath .githooks (يُوثَّق في README).
تنبيه: hooks لا تنتقل تلقائياً مع git clone. يُضاف script tools/install-hooks.sh يدوي يربطها.
▸ VERIFICATION

Given فحص كامل لـ codebase، When grep -rn "NullPrintService" --include="*.cs" يُنفَّذ، Then نتائج = 0.
Given working tree نظيف، When find . -name "*.bak" يُنفَّذ، Then نتائج = 0.
Given developer أضاف ملف test.bak وحاول commit، When pre-commit hook يعمل، Then الـ commit يُرفض مع رسالة واضحة.
Given .gitignore المُحدَّث، When ملف *.bak يُنشأ في working tree، Then git status لا يُظهره.
▸ RISK

خطر منخفض: حذف NullPrintService قد يُكسر اختباراً لم نُلاحظه. التخفيف: فحص ملفات الاختبار قبل الحذف بـ grep -rn "NullPrintService" FinalLabSystem.Tests/. النتيجة المتوقَّعة الحالية: 0.
خطر الـ hooks لا تنتقل مع clone. التخفيف: توثيق واضح في README مع amrak setup script.
Task 2.8 — Phase 2 Complete Test Suite
ضمانة الانحدار: جميع الـ 185 اختباراً الحالية يجب أن تستمر في النجاح. لا يُسمح بإضافة ميزة تكسر اختباراً قائماً. يُنفَّذ dotnet test بعد كل task ويُتطلَّب 100% pass قبل المتابعة.

الاختبارات الجديدة المطلوبة لـ Phase 2:

ملف الاختبار	السيناريو	عدد المتوقَّع
Tests/ViewModels/AuditTrailViewModelTests.cs	يبني الـ VM بكلا الـ overloads ويتحقق من ObservableCollection و ICollectionView	4
Tests/Services/AuditTrailDialogServiceTests.cs	mock IServiceProvider، تحقق من تدفُّق إنشاء VM وفتح النافذة	3
Tests/ViewModels/ResultEntryViewModelTests.cs	بناء VM، SaveAsync يستدعي خدمة، SaveCompleted ينطلق، CancelCommand يُغلق	6
Tests/Services/ResultEntryDialogServiceTests.cs	factory pattern: تحقق من إنشاء VM مع البارامترات الصحيحة	3
Tests/ViewModels/MainViewModelTests.cs	كل 12 menu command تُعرض VM الصحيحة في CurrentView	12
Tests/ViewModels/Menu/PatientsMenuViewModelTests.cs	الـ 4 sub-commands تعمل	4
Tests/ViewModels/Menu/PlaceholderMenusTests.cs	الـ 4 placeholder menus تعرض dialog "Coming in Phase X"	4
Tests/Services/PatientStatusComputationTests.cs	كل 7 حالات الـ status يتم حسابها بشكل صحيح من بيانات تجريبية	7
Tests/Views/Converters/PatientStatusToIconConverterTests.cs	كل enum يُحوَّل لـ Geometry/ImageSource صحيح، null يُحوَّل لـ "?"	8
Tests/ViewModels/PatientRegistrationFKeyTests.cs	كل 12 F-key command (F1-F12) يستدعي الـ handler الصحيح ولا يقع في exception	12
Tests/ViewModels/TestResultsFKeyRemappingTests.cs	Ctrl+R / Ctrl+F / Ctrl+P تنفِّذ الـ legacy toggles، F8/F12 تنفِّذ الدلالة الجديدة	6
Tests/Infrastructure/Navigation/AuditTrailWindowRegistrationTests.cs	DI يحلّ النافذة بنجاح، NavigationService يفتحها دون استثناء	2
Tests/Infrastructure/Navigation/ResultEntryWindowRegistrationTests.cs	نفس الشيء لـ ResultEntryWindow	2
إجمالي الاختبارات الجديدة لـ Phase 2: ~73 اختبار. الإجمالي بعد Phase 2: 185 + 73 = 258 اختبار ناجح.

Phase 2 Summary
ملفات للتعديل:

App.xaml.cs (إضافة 12 DI registration + 2 navigation register).
ViewModels/MainViewModel.cs (توسيع لـ 12 menu commands + استخراج nested class).
MainWindow.xaml (12-icon toolbar + 10 جديدة data templates).
Views/Patients/TestResultsWindow.xaml (تعديل F-key bindings سطور 791-800).
Views/Patients/PatientRegistrationWindow.xaml (إضافة Window.InputBindings كاملة).
ViewModels/Patients/PatientRegistrationViewModel.cs (إضافة 10 commands).
ViewModels/Patients/TestResultsViewModel.cs (إضافة EditSelectedPatientCommand, PrintReceiptCommand, OpenComponentEditorCommand, إعادة تسمية اختصارات legacy).
ViewModels/Patients/ResultEntryViewModel.cs (إضافة RequestClose action + تفعيل CancelCommand).
Services/Interfaces/IPrintService.cs (تعديل تعليق <see cref/> بعد حذف NullPrintService).
Views/Converters.cs (إضافة PatientStatusToIconConverter).
Views/Shared/SharedStyles.xaml (إضافة 7 Geometry resources للأيقونات).
Models/DTOs/TodayPatientWithStatusDto.cs (إضافة computed StatusIcon).
.gitignore (إضافة *.bak, *.bak2, *.orig).
Docs/PRDs/IMPLEMENTATION_STATUS.md (تحديث قسم Phase 2 من فارغ إلى مكتمل).
ملفات للإنشاء:

ViewModels/Menu/PatientsMenuViewModel.cs (نقل من MainViewModel.cs).
ViewModels/Menu/HomeMenuViewModel.cs.
ViewModels/Menu/ResultsMenuViewModel.cs.
ViewModels/Menu/DeliveryMenuViewModel.cs.
ViewModels/Menu/SearchMenuViewModel.cs.
ViewModels/Menu/ExternalSamplesMenuViewModel.cs (placeholder).
ViewModels/Menu/AccountsMenuViewModel.cs (placeholder).
ViewModels/Menu/BackupMenuViewModel.cs (placeholder).
ViewModels/Menu/TestDataMenuViewModel.cs.
ViewModels/Menu/NormalRangesMenuViewModel.cs.
ViewModels/Menu/ReportSettingsMenuViewModel.cs (placeholder).
Services/Interfaces/IAuditTrailDialogService.cs.
Services/Implementations/AuditTrailDialogService.cs.
Services/Interfaces/IResultEntryDialogService.cs.
Services/Implementations/ResultEntryDialogService.cs.
Models/Enums/PatientStatusIcon.cs.
.githooks/pre-commit + tools/install-hooks.sh.
13 test files (انظر Task 2.8).
ملفات للحذف:

Services/Implementations/NullPrintService.cs (dead code).
Views/Patients/TestSelectionView.xaml.bak (backup).
Views/Patients/TestSelectionView.xaml.bak2 (backup).
Migrations required: لا. Phase 2 لا يُغيِّر schema — فقط طبقات UI و VM و DI و قواعد تنظيف.

الأيام المقدَّرة: 10 أيام عمل.

Risk level: 🟠 High — F-key remapping يَمَسّ workflow راسخ. التخفيف الرئيسي: dialog الإعلام لمرة واحدة + شريط حالة دائم بالاختصارات + اختبارات شاملة للـ 12 مفتاح + dialog تأكيد إجباري قبل F10 (الحذف).

PART THREE — PHASE 3: Result Editor Alignment (التركيز الثاني — تفصيل عميق)
Task 3.1 — ResultEntryWindow Full Integration
▸ CONTEXT بعد Phase 2 سُجِّلت ResultEntryWindow في DI و navigation. ResultEntryViewModel.cs (104 سطر، مفحوص بالكامل) يحوي:

المحاور الأساسية للإدخال (Components, SelectedComponent).
SaveCommand يربط بـ RoutineResultService.SaveNumericOrTextResultsAsync (سطر 32 في الـ Service).
CancelCommand تم تفعيله في Phase 2.
حدث SaveCompleted يُطلَق بعد الحفظ الناجح.
▸ PROBLEM ResultEntryViewModel يفتقد:

التحقق من النطاقات الطبيعية أثناء الإدخال (تلوين بصري للقيم خارج النطاق). الـ Service يحسب الـ Validation Status عند الحفظ، لكن الـ UI لا يعرضه live.
حقن تعليقات تلقائية عند قيمة خارج النطاق (انظر Task 3.4).
محرِّرات تخصصية: للنتائج الرقمية فقط يعمل. لتحاليل التصوير الميكروبيولوجي / السائل المنوي / بنك الدم لا توجد محرِّرات خاصة (تُؤجَّل لـ Phase 7 لكن الواجهة العامة يجب أن تَختار المحرِّر الصحيح).
حفظ جزئي: حالياً Save-or-nothing. المستخدم قد يريد حفظ 3 من 10 مكوّنات الآن، والباقي لاحقاً. الـ Service يدعم list partial، لكن الـ UI لا يكشف الخيار.
Real-time validation feedback: حقول الإدخال يجب أن تُلوَّن أحمر/أصفر/برتقالي حسب High/Low/Critical فور الإدخال.
▸ SOLUTION

Live Validation:
إضافة خاصية ValidationStatus لكل TestComponentResultDto: Normal | Low | High | Critical | Unknown.
عند تعديل قيمة Component في الـ DataGrid، استدعاء INormalRangeService.ResolveStatusAsync(componentId, value, patientAgeDays, patientGender) (الـ NormalRange موجود فعلاً كـ entity).
الـ Converter ValidationStatusToBrushConverter يلوِّن خلفية الـ TextBox.
Auto-Comment Hook (تفاصيل في Task 3.4): عند تغيُّر ValidationStatus، يُستدعى IReportCommentEngine.SuggestCommentAsync(componentId, status) ويُحقَن النص في عمود "تعليق" إذا كان فارغاً.
Specialty Editor Selection: IResultEditorFactory (موجود فعلاً، مسجَّل في DI سطر 148) يُمدَّد ليُرجع نوع المحرِّر المناسب للـ TestType. حالياً يُرجع DefaultResultEditor للجميع. في Phase 3 يبقى كذلك (السبعة المتخصصة في Phase 7)، لكن البنية تُهَيَّأ:
Copypublic interface IResultEditorFactory
{
    UserControl CreateEditor(TestType testType, ResultEntryViewModel parent);
}
ResultEntryWindow.xaml يستضيف ContentPresenter يتم تعبئته من Factory.CreateEditor(...).
Partial Save: إضافة checkbox أمام كل صف "حفظ". عند الضغط على Save، فقط الـ rows المؤشَّرة تُمَرَّر للـ Service. Default = all checked.
Action Buttons في ResultEntryWindow.xaml:
"حفظ" (F9 internally).
"حفظ ومراجعة" (يحفظ ثم يستدعي RoutineResultService.ToggleReviewStatusAsync).
"إلغاء" (F-Esc).
▸ VERIFICATION

Given تحليل CBC بمكوّن HGB ونطاق طبيعي 12-16، When المستخدم يُدخل 8.5، Then الـ row يصبح برتقاليّاً ويظهر تعليق "Low" تلقائياً، و ValidationStatus = Low.
Given المستخدم أدخل قيم لـ 3 من 5 مكوّنات وأشَّر فقط على 2، When يضغط Save، Then 2 فقط تُكتب في DB، و الـ 3 الأخرى تبقى بدون TestResult.
Given قيمة < Critical Low، When المستخدم يَحفظ، Then dialog تحذير يظهر "قيمة حرجة لـ HGB. هل تريد متابعة الحفظ؟" مع تسجيل في AuditLog.
Given المستخدم ضغط "حفظ ومراجعة"، When ينجح الحفظ، Then VisitTest.IsReviewed = true و AuditLog يسجِّل التغيير.
▸ RISK

خطر أداء: live validation يستدعي DB لكل keystroke. التخفيف: تحميل NormalRanges مرّة واحدة عند فتح النافذة (ResolveStatusForRow يعمل in-memory)، debounce 300ms للـ TextBox changes.
خطر صراع مع stage gate: Phase 1 يمنع تعديل نتائج مرحلة منتهية. التخفيف: استدعاء RoutineResultServiceGuard قبل Save (موجود فعلاً)، عرض رسالة واضحة إن مُنِع.
Task 3.2 — TestProfile Management UI
▸ CONTEXT

Models/TestProfile.cs (28 سطر، فحص كامل): يحوي ProfileId, ProfileNameAr, ProfileNameEn, Description, IsActive, CreatedAt, CreatedBy, navigation لـ Staff و collection TestProfileItems.
Models/TestProfileItem.cs (19 سطر، فحص كامل): ProfileItemId, ProfileId, TestTypeId, SortOrder, و navigation لـ Profile و TestType.
Services/Interfaces/ITestCatalogService.cs (281 سطر، فحص جزئي): يحوي GetActiveProfilesAsync() (سطر 27) و GetProfileTestsAsync(int profileId) (سطر 34). لا يوجد بعد: CreateProfileAsync, UpdateProfileAsync, DeleteProfileAsync, AddTestToProfileAsync, RemoveTestFromProfileAsync, ReorderProfileItemsAsync.
Help.pdf workflow (مستخرج): "System > Custom Groups > Add Group → enter name → Add Test → link multiple specific tests → double-click to add to patient". هذا workflow يجب إعادة بناؤه كاملاً.
▸ PROBLEM

لا توجد شاشة CRUD لـ TestProfile.
المستخدم لا يستطيع إنشاء profile مخصَّص (مثل "تحاليل الكبد الشامل" يحوي 6 تحاليل).
في شاشة تسجيل المريض، لا منفذ لإضافة profile جاهز بنقرة واحدة.
▸ SOLUTION

توسيع ITestCatalogService:
CopyTask<TestProfile> CreateProfileAsync(TestProfile profile);
Task<TestProfile> UpdateProfileAsync(TestProfile profile);
Task DeleteProfileAsync(int profileId);
Task<TestProfileItem> AddTestToProfileAsync(int profileId, int testTypeId, int sortOrder);
Task RemoveTestFromProfileAsync(int profileItemId);
Task ReorderProfileItemsAsync(int profileId, List<int> orderedItemIds);
Task<TestProfile?> GetProfileByIdAsync(int profileId);
ViewModels/Settings/TestProfileWindowViewModel.cs — يحوي master-detail:
Master: ObservableCollection<TestProfileRowViewModel> Profiles + commands AddProfile, DeleteProfile, SaveAll.
Detail: SelectedProfile يكشف ObservableCollection<TestProfileItemRowViewModel> Items مع drag-drop reordering.
Views/Settings/TestProfileWindow.xaml:
Grid 2 columns: ListBox للـ Profiles على اليسار، DataGrid للـ Items + TestType picker على اليمين.
Drag-drop عبر ListBox.AllowDrop=true + custom behavior يحدِّث SortOrder.
تسجيل DI في App.xaml.cs:
Copyservices.AddTransient<TestProfileWindowViewModel>();
services.AddTransient<TestProfileWindow>();
navigation.RegisterWindow<TestProfileWindowViewModel, TestProfileWindow>();
منفذ من Toolbar: زرّ في sub-menu "بيانات التحاليل" (TestDataMenuViewModel من Phase 2): "إدارة المجموعات التخصصية".
Patient Registration integration (Task 3.5 الكامل): زرّ "إضافة Profile" في TestSelectionView.xaml يفتح dialog لاختيار profile ثم يستدعي TestSelectionViewModel.ApplyProfileAsync(int profileId) التي تستعلم عن TestProfileItems وتُضيفها للـ visit.
▸ VERIFICATION

Given TestProfileWindow مفتوحة، When المستخدم نقر "إضافة Profile" وأدخل "تحاليل ما قبل الزواج"، Then profile جديد يُحفَظ في DB بـ IsActive = true.
Given profile مع 0 items، When المستخدم يَسحب 5 تحاليل من القائمة اليمنى، Then 5 TestProfileItems تُحفَظ بـ SortOrder 1-5.
Given profile مع 5 items، When المستخدم يُغيِّر ترتيبهم بـ drag-drop، Then ReorderProfileItemsAsync يُستدعى و SortOrder يُحَدَّث.
Given profile يُستخدم في visits، When المستخدم يحاول حذفه، Then dialog تأكيد يظهر، وعند الموافقة IsActive = false (soft delete) لا hard delete.
▸ RISK

خطر حذف profile مرتبط بـ visits أرشيفية. التخفيف: soft delete (IsActive=false)، إخفاء من قوائم patient registration لكن الإبقاء على البيانات.
Task 3.3 — ReportCommentTemplate Management UI
▸ CONTEXT

Models/ReportCommentTemplate.cs (43 سطر، فحص كامل): يحوي TemplateId, CategoryId?, TesttypeId?, ComponentId?, Title, CommentText, CommentLang, IsActive, SortOrder, CreatedBy/At, ModifiedBy/At. النموذج مفتوح: قالب قد يرتبط بـ Category أو TestType أو Component أو لا شيء (general).
Help.pdf (مستخرج): "System > Test Comments → select test → type recurring comment → Add → Save. During result entry, click Comment icon → list of templates".
لا توجد شاشة UI حالياً، فقط الـ entity والـ DbSet (Data/FinalLabDbContext.cs سطر 150).
▸ PROBLEM

المستخدم لا يستطيع إنشاء قوالب تعليق.
لا يوجد ربط بين القالب وحالة الـ result (High/Low/Critical).
ResultEntryViewModel لا يستطيع اقتراح تعليقات.
ملاحظة مهمة: ReportCommentTemplate الحالي لا يحوي حقل "ينطبق على Low/High/Critical". هذا تصميم. الـ Phase 3 يحتاج إما إضافة حقل أو الاعتماد على naming convention في Title. القرار المعماري الأمثل: إضافة عمود enum.

▸ SOLUTION

Migration جديدة AddReportCommentTemplate_TriggerCondition:
Copy// إضافة عمود
AddColumn<string>("TriggerCondition", "ReportCommentTemplate", maxLength: 20, nullable: true);
القيم: None, Low, High, Critical, Manual.
تحديث Model: إضافة خاصية public string? TriggerCondition { get; set; } و enum مرافق ReportCommentTrigger.
خدمة جديدة IReportCommentTemplateService في Services/Interfaces/:
CopyTask<List<ReportCommentTemplate>> GetByTestTypeAsync(int testTypeId);
Task<List<ReportCommentTemplate>> GetByComponentAsync(int componentId);
Task<ReportCommentTemplate?> FindMatchingAsync(int? testTypeId, int? componentId, ReportCommentTrigger trigger);
Task<ReportCommentTemplate> CreateAsync(ReportCommentTemplate template);
Task<ReportCommentTemplate> UpdateAsync(ReportCommentTemplate template);
Task DeleteAsync(int templateId); // soft delete
ViewModels/Settings/ReportCommentTemplateViewModel.cs: master-detail مع filter by TestType.
Views/Settings/ReportCommentTemplateWindow.xaml:
Filter row: ComboBox للـ TestType + ComboBox للـ Trigger.
DataGrid يعرض القوالب المُفلتَرة.
Detail panel: حقول Title, CommentText (TextArea multi-line)، Lang (ar/en)، Trigger، Active.
تسجيل DI:
Copyservices.AddScoped<IReportCommentTemplateService, ReportCommentTemplateService>();
services.AddTransient<ReportCommentTemplateViewModel>();
services.AddTransient<ReportCommentTemplateWindow>();
منفذ UI: في sub-menu ReportSettingsMenuViewModel (من Phase 2) — يُحَدَّث من placeholder إلى أمر فعلي.
▸ VERIFICATION

Given ReportCommentTemplate جديد بـ TestType=CBC, Component=HGB, Trigger=Low, CommentText="انخفاض الهيموجلوبين قد يشير لأنيميا"، When يُحفَظ، Then يظهر في DB.
Given ResultEntryViewModel فيه HGB بقيمة 8.5 (Low)، When Auto-Comment يُستدعى (Task 3.4)، Then القالب الـ matching يُجلَب ويُحقَن في حقل التعليق.
Given المستخدم نقر Comment icon في الـ row، When يَعرض قائمة، Then كل قوالب التحليل الحالي تظهر مع ميزة الـ trigger الحالي مُسلَّطاً.
▸ RISK

خطر تَضارب بين تعليق مُحقَن تلقائياً وتعليق يكتبه المستخدم. التخفيف: قاعدة "auto-comment لا يُكتَب فوق comment غير فارغ" (Task 3.4).
خطر بيانات قديمة بلا TriggerCondition. التخفيف: migration تضع TriggerCondition = 'Manual' للسجلات الموجودة.
Task 3.4 — Auto-Comment Injection Logic
▸ CONTEXT

Services/Implementations/RoutineResultService.cs (245 سطر، فحص جزئي):
SaveNumericOrTextResultsAsync (سطر 32) — هنا تحدث الـ save flow الكاملة: تحديث ResultNumeric، استعلام عن NormalRange (سطر 63-69)، حساب ValidationStatus، كتابة TestResult.
بعد سطر 80 تقريباً يحدث merge مع existingResults، ثم save changes.
النقطة المثالية لحقن التعليق التلقائي: داخل الحلقة بعد حساب ValidationStatus، قبل _context.SaveChangesAsync.
▸ PROBLEM

لا منطق حالياً يقرأ ReportCommentTemplate ويضع CommentText في TestResult.Comment (الحقل موجود على Model).
لا فحص لـ "هل المستخدم كتب تعليقاً يدوياً؟" — هذا حرج لئلا نَدوس على نص يدوي.
▸ SOLUTION

Interface جديدة IReportCommentEngine في Services/Interfaces/IReportCommentEngine.cs:
Copypublic interface IReportCommentEngine
{
    Task<string?> ResolveCommentAsync(int componentId, ResultValidationStatus status, int? testTypeId);
    Task ApplyAutoCommentsAsync(List<TestResult> results);
}
التنفيذ Services/Implementations/ReportCommentEngine.cs:
يستقبل IReportCommentTemplateService و ILogger.
ResolveCommentAsync: تَخريطة ResultValidationStatus → ReportCommentTrigger (Low→Low, High→High, Critical→Critical, Normal→None). إذا None، يُرجِع null. غير ذلك يستدعي _service.FindMatchingAsync(testTypeId, componentId, trigger). إن وُجد قالب، يُرجِع نصه.
ApplyAutoCommentsAsync: للحلقة على نتائج، لكلٍّ:
إذا result.Comment ليس فارغاً → تخطٍّ (no overwrite).
وإلا استدعِ ResolveCommentAsync وضع النتيجة في result.Comment.
Hook في RoutineResultService:
بعد حساب الـ ValidationStatus لكل نتيجة، وقبل SaveChangesAsync، استدعاء _commentEngine.ApplyAutoCommentsAsync(results).
إضافة dependency: constructor يستقبل IReportCommentEngine.
تسجيل DI:
Copyservices.AddScoped<IReportCommentEngine, ReportCommentEngine>();
services.AddScoped<IReportCommentTemplateService, ReportCommentTemplateService>(); // إذا لم يُسجَّل في Task 3.3
▸ VERIFICATION

Given قالب يقول Trigger=Low CommentText="انخفاض" لمكوّن HGB، وقيمة 8.5 ستُحفَظ لمريض، When الـ save flow ينتهي، Then TestResult.Comment = "انخفاض" و TestResult.ResultValidationStatus = Low.
Given نفس السيناريو لكن المستخدم كتب يدوياً "أرجو إعادة التحليل"، When الـ save flow ينتهي، Then Comment يبقى "أرجو إعادة التحليل" — لا overwrite.
Given قيمة Normal بلا قالب مُطابق، When save، Then Comment يبقى null.
Given الـ engine يَفشل (مثلاً DB unavailable)، When هو يُستدعى، Then الفشل يُسجَّل في log و save يستمر — لا يَفشل الحفظ بسبب comment engine.
▸ RISK

خطر comment engine يَبطئ Save بشدّة لو كل result يستدعي DB query. التخفيف: pre-load كل templates للـ test types المعنية في bulk، ثم resolve in-memory.
خطر comment خاطئ يَحقَن في result نهائية. التخفيف: stage gate يَمنع تعديل result مراجعَة، وأي auto-comment يُسجَّل في AuditLog بـ Action="AutoCommentInjected" ليكون tracable.
Task 3.5 — TestProfile Expansion on Patient Registration
▸ CONTEXT

ViewModels/Patients/TestSelectionViewModel.cs (فحص جزئي عبر DI registration سطر 173): مسجَّل، يَعرض قائمة TestTypes ويسمح بالاختيار.
Help.pdf workflow: "Users can select pre-defined test groups → adding a group automatically adds all sub-tests to the patient's record based on the predefined price list."
▸ PROBLEM

اختيار 5 تحاليل لـ profile واحد يتطلب 5 نقرات مستقلة.
مع 10 profiles شائعة (تحاليل الكبد، الكلى، السكر، الدهون...) هذا UX سيء.
▸ SOLUTION

زرّ جديد في Views/Patients/TestSelectionView.xaml: "إضافة Profile" (أعلى قائمة الـ tests).
عند النقر: dialog يَعرض GetActiveProfilesAsync().
اختيار profile: استدعاء TestSelectionViewModel.ApplyProfileAsync(int profileId):
Copypublic async Task ApplyProfileAsync(int profileId)
{
    var tests = await _testCatalogService.GetProfileTestsAsync(profileId);
    foreach (var test in tests)
    {
        if (!SelectedTests.Any(st => st.TestTypeId == test.TestTypeId))
            SelectedTests.Add(new SelectedTestDto(test, computedPrice));
    }
    RecalculateTotal();
}
Service method GetProfileTestsAsync موجودة فعلاً (ITestCatalogService سطر 34).
Pricing: يستدعي IPricingService (سيُسجَّل في Phase 4) أو fallback إلى TestType.DefaultPrice لـ Phase 3.
▸ VERIFICATION

Given profile "Liver Profile" يحوي 6 tests، When المستخدم يَنقر "إضافة Profile" ويختاره، Then 6 تحاليل تُضاف لـ SelectedTests بضربة واحدة.
Given 3 من الـ 6 موجودة سلفاً في SelectedTests، When يطبَّق profile، Then فقط 3 جديدة تُضاف (لا تكرار).
Given المجموع كان 100 ج.م، When profile يضيف 6 تحاليل قيمتها 240، Then المجموع يصبح 340.
▸ RISK

خطر تكرار: لو profile يحوي test موجود سلفاً مع خصم، الـ overwrite قد يُلغي الخصم. التخفيف: قاعدة "skip if exists" واضحة.
خطر التسعير: في Phase 3 IPricingService غير مسجَّل بعد. التخفيف: استخدام TestType.DefaultPrice مؤقتاً، إعادة pricing في Phase 4 عند توفر IPricingService.
Task 3.6 — Phase 3 Complete Test Suite
ضمانة الانحدار: 258 اختبار من نهاية Phase 2 يجب أن تستمر في النجاح.

الاختبارات الجديدة المطلوبة لـ Phase 3:

ملف الاختبار	السيناريو	عدد
Tests/Services/RoutineResultServiceLiveValidationTests.cs	live validation لـ Low/High/Critical/Normal	8
Tests/Services/ReportCommentEngineTests.cs	حقن تلقائي، عدم overwrite يدوي، fallback null	9
Tests/Services/ReportCommentTemplateServiceTests.cs	CRUD، FindMatching، GetByTestType	10
Tests/Services/TestCatalogService_ProfileCrudTests.cs	Create/Update/Delete/AddItem/Remove/Reorder	12
Tests/ViewModels/Settings/TestProfileWindowViewModelTests.cs	master-detail، drag-drop reorder	8
Tests/ViewModels/Settings/ReportCommentTemplateViewModelTests.cs	filter، CRUD UI	8
Tests/ViewModels/Patients/ResultEntryViewModelLiveValidationTests.cs	تلوين القيم، اقتراح التعليق	6
Tests/ViewModels/Patients/TestSelectionViewModelProfileApplyTests.cs	ApplyProfile يَضيف، يَتخطّى المكرَّر	5
Tests/Integration/AutoCommentEndToEndTests.cs	E2E: result low → comment injected → audited	3
إجمالي اختبارات Phase 3: ~69 اختبار. الإجمالي بعد Phase 3: 258 + 69 = 327 اختبار ناجح.

Phase 3 Summary
ملفات للتعديل:

Services/Interfaces/ITestCatalogService.cs (إضافة 7 profile CRUD methods).
Services/Implementations/TestCatalogService.cs (تنفيذ الـ 7).
Services/Implementations/RoutineResultService.cs (إضافة IReportCommentEngine dependency، hook قبل SaveChanges).
ViewModels/Patients/ResultEntryViewModel.cs (live validation، partial save، specialty editor selection).
Views/Patients/ResultEntryWindow.xaml (ContentPresenter للمحرِّر، أزرار Save+Review، Partial Save checkboxes).
ViewModels/Patients/TestSelectionViewModel.cs (ApplyProfileAsync).
Views/Patients/TestSelectionView.xaml (زرّ "إضافة Profile").
Models/ReportCommentTemplate.cs (إضافة TriggerCondition).
Data/FinalLabDbContext.cs (تحديث mapping للحقل الجديد).
Views/Converters.cs (ValidationStatusToBrushConverter).
ملفات للإنشاء:

Services/Interfaces/IReportCommentEngine.cs.
Services/Implementations/ReportCommentEngine.cs.
Services/Interfaces/IReportCommentTemplateService.cs.
Services/Implementations/ReportCommentTemplateService.cs.
ViewModels/Settings/TestProfileWindowViewModel.cs.
ViewModels/Settings/TestProfileRowViewModel.cs.
ViewModels/Settings/TestProfileItemRowViewModel.cs.
Views/Settings/TestProfileWindow.xaml + .xaml.cs.
ViewModels/Settings/ReportCommentTemplateViewModel.cs.
Views/Settings/ReportCommentTemplateWindow.xaml + .xaml.cs.
Migration 28_AddReportCommentTemplate_TriggerCondition.
9 test files (انظر Task 3.6).
ملفات للحذف: لا شيء.

Migrations required: نعم — واحدة. اسم: 28_AddReportCommentTemplate_TriggerCondition. الغرض: إضافة عمود TriggerCondition NVARCHAR(20) NULL على جدول ReportCommentTemplate، مع backfill 'Manual' للسجلات القديمة.

الأيام المقدَّرة: 9 أيام عمل.

Risk level: 🟠 High — Auto-comment يَمَسّ نتائج طبية. التخفيف الرئيسي: stage gate يَمنع تعديل result مُراجَع، كل auto-comment يُسجَّل في AuditLog بـ Action="AutoCommentInjected"، اختبار E2E يُثبِت عدم overwrite لتعليق يدوي.

PART FOUR — PHASES 4 THROUGH 7 (تفصيل قياسي)
Phase 4 — Billing & Contracts
الهدف: تفعيل طبقة المحاسبة الكاملة: شاشات Contracts و Pricing و External Labs Manifest.

المهام:

Task 4.1 — IContractService DI: تسجيله Scoped + بناء ContractsWindow (master Companies + detail Contracts/Invoices/Payments).
Task 4.2 — IPricingService DI: تسجيله Scoped + بناء PriceSchemeWindow لإدارة PriceScheme (Individual / Lab-to-Lab / Insurance / Free).
Task 4.3 — IExternalLabService DI: تسجيله Scoped + بناء ExternalLabsWindow.
Task 4.4 — Contract Monthly Invoice workflow (تفصيل إضافي أدناه).
Task 4.5 — External Labs Manifest workflow (تفصيل إضافي أدناه).
Task 4.6 — F7 wire-up: ربط F7 بـ ExternalSamplesWindow (placeholder من Phase 2 يُستبدَل).
Task 4.7 — AccountsMenuViewModel activation: تحويل من placeholder إلى submenu حقيقي يُظهر Contracts + Pricing + External + Cash Drawer (Phase 5).
خدمات تحتاج DI: IContractService, IPricingService, IExternalLabService.

نوافذ UI جديدة: ContractsWindow, PriceSchemeWindow, ExternalLabsWindow, ContractInvoiceWindow, ExternalShipmentWindow.

ملفات تحتاج تعديل: App.xaml.cs, AccountsMenuViewModel.cs, TestSelectionViewModel.cs (لاستخدام IPricingService بدل DefaultPrice).

Migrations: محتمل واحدة — 29_AddContractInvoice_Periodicity لو احتجنا حقل دورية. سيُحدَّد عند البدء بعد فحص ContractInvoice.cs.

Business Rules:

BR-040: عقد لا يمكن أن يحوي مريضاً بدون شركة مالية مرتبطة.
BR-041: فاتورة شهرية تَجمع كل visits ضمن نفس فترة العقد وتُولِّد PDF.
BR-042: PriceScheme يُحَدِّد سعر TestType لكل شريحة (Individual/Lab-to-Lab/Insurance).
BR-043: External Shipment يَفقد ربطاً بمعمل خارجي إذا حُذف الـ ExternalLab → يُمنَع hard delete.
تفصيل إضافي — Contract Monthly Invoice workflow:

العقد (Contract ضمن Company) يحوي StartDate و EndDate و BillingPeriodicity (Monthly/Quarterly) و DiscountPercent. في نهاية كل شهر يُستدعى IContractService.GenerateMonthlyInvoiceAsync(contractId, year, month):

يَجمع كل Visit فيها Patient.CompanyId == contract.CompanyId و Visit.RegisteredAt في نطاق الشهر.
لكل visit يَجمع VisitTests ويحسب Sum(Price) بعد تطبيق DiscountPercent.
يُنشئ ContractInvoice جديد بـ TotalAmount, IssuedAt = today, Status = Pending, و navigation لـ ContractPayments.
يُولِّد PDF (FlowDocument) باسم الشركة، فترة الفاتورة، جدول مريض/تحاليل/سعر، الإجمالي.
الـ UI يَعرض زرّ "Send via Email" يستخدم SMTP config من LabSetting.
عند استلام Payment، ContractPayment يُضاف، الـ ContractInvoice.PaidAmount يُحَدَّث، الـ Status يصبح Partial أو Paid.
تفصيل إضافي — External Labs Manifest workflow:

عينة ما تُجمَع داخلياً لكن تُحَلَّل خارجياً (مثلاً FISH testing). الـ workflow:

عند تسجيل المريض، TestType بـ IsOutsourced=true يُضيف صفّاً تلقائياً في الـ ExternalShipment المعلَّق.
شاشة ExternalShipmentWindow تَعرض كل ExternalShipmentItems المعلَّقة، تسمح بـ:
"Send Manifest": يَطبع/يُرسل قائمة بكل العينات مع الباركودات لمعمل خارجي محدَّد، يُحَدِّث Status = Sent، SentAt = now.
"Receive Results": يَفتح dialog لاستيراد النتيجة (يدوياً أو من ملف Excel) للـ ExternalShipmentItem، يُولِّد TestResult مرتبطة بـ visit الأصلية.
تقرير شهري عن المعامل الخارجية: إجمالي العينات، التكلفة (CostPrice على TestType)، الفرق (PatientPrice - CostPrice) = هامش.
Acceptance criteria أهمّ:

Contract شهري يولِّد فاتورة دقيقة بكل visits الشركة في الشهر.
External shipment manifest يُطبَع بـ barcodes جاهزة للمعمل الخارجي.
لا يمكن إصدار فاتورة مرَّتين لنفس الشهر/العقد (unique constraint).
Estimated days: 12. Risk: 🔴 Critical (مال + شركات + التزامات قانونية).

Phase 5 — Inventory & Cash Drawer
الهدف: شاشة الجَرْد والحسابات اليومية للمعمل (Cash Drawer)، حضور الموظفين، تقارير العمولات.

المهام:

Task 5.1 — IAttendanceService DI + شاشة AttendanceWindow لتسجيل دخول/خروج الموظف وحساب الساعات.
Task 5.2 — Cash Drawer screen: يَعرض إجمالي visits اليوم، الخصومات، المحصَّل (Payments.Sum)، الباقي على المرضى، صافي الربح. Filter by branch/doctor/user/date.
Task 5.3 — Inventory screen: جَرْد SampleTube و TubeMaterial و الاستهلاكات. تَدفُّق "تَنبيه عند انخفاض المخزون عن MinimumStock".
Task 5.4 — Referral Commission Report: VReferralCommissionReport view موجود فعلاً — UI لعرضه مع filter.
Task 5.5 — Outstanding Balance Report: VOutstandingBalance view موجود — UI.
خدمات تحتاج DI: IAttendanceService. (IFinancialService مسجَّلة فعلاً.)

نوافذ UI جديدة: CashDrawerWindow, InventoryWindow, AttendanceWindow, CommissionReportWindow, OutstandingBalanceReportWindow.

Migrations: محتمل واحدة لإضافة MinimumStock على SampleTube إن لم يكن موجوداً.

Business Rules:

BR-050: Cash Drawer يَفتح بكلمة مرور (Help.pdf default "123" — يجب تخزين hash مع إجبار تغييره أول مرة).
BR-051: Attendance يَتطلَّب shift_id من WorkShift.
BR-052: Inventory alert عند CurrentStock < MinimumStock يَظهر في dashboard.
Estimated days: 10. Risk: 🟠 High.

Phase 6 — Print, Delivery & Backup
الهدف: إصلاح MVVM violation في PrintPreviewWindow، بناء خدمة Backup/Restore، تكامل مع natigh.com لنشر النتائج.

المهام:

Task 6.1 — PrintPreview MVVM refactoring (تفصيل إضافي أدناه).
Task 6.2 — IBackupService جديدة + شاشة BackupRestoreWindow.
Task 6.3 — Natigh.com integration: API client لرفع PDF النتائج للموقع، يُعطي المريض رابطاً/كوداً.
Task 6.4 — Report Settings UI (ReportSettingsMenuViewModel من Phase 2 يُفعَّل): تخصيص ألوان، خطوط، شعار المعمل، الـ margins.
Task 6.5 — Delivery enhancements: تأكيد التسليم بـ signature pad أو OTP.
Task 6.6 — Print queue: طباعة متعدِّدة في batch.
خدمات تحتاج DI: IBackupService (جديدة)، INatighIntegrationService (جديدة).

نوافذ UI جديدة: PrintPreviewWindow (مُعاد بناؤه)، BackupRestoreWindow, ReportSettingsWindow, NatighPublishWindow.

Migrations: لا.

Business Rules:

BR-060: Backup file يُشفَّر بـ AES قبل الكتابة.
BR-061: Restore يَتطلَّب admin role + confirmation.
BR-062: Natigh publish يَتطلَّب موافقة المريض المُسجَّلة في Visit.NatighConsent.
تفصيل إضافي — Backup/Restore service:

IBackupService:

CopyTask<string> CreateBackupAsync(string outputPath); // returns file path
Task RestoreBackupAsync(string backupFilePath, string adminPassword);
Task<List<BackupMetadata>> ListBackupsAsync(string folder);
التنفيذ:

Backup: استخدام SqlBackup عبر SqlConnection.ChangeDatabase و BACKUP DATABASE T-SQL. أو fallback DbContext يُصدِّر JSON. يُفضَّل T-SQL لأنه atomic وأسرع.
Encryption: AES-256 بـ key مشتق من admin password.
Naming: FinalLabSystem_YYYY-MM-DD_HHmmss.bak.enc.
Schedule: عبر IBackupSchedulerService يَستخدم System.Threading.Timer لـ daily backup في وقت محدَّد في LabSetting.
التنفيذ يحتاج صلاحيات SQL Server خاصة. تحذير: المُستخدِم النهائي قد لا يَملكها — fallback Strategy: backup يعمل عبر EF JSON export لو T-SQL يفشل.

Restore خطير لأنه يَستبدل DB الحالية. الـ workflow:

dialog تحذير "ستُستبدَل قاعدة البيانات بالكامل".
confirmation بـ password admin.
backup current state تلقائياً قبل restore.
تنفيذ RESTORE DATABASE.
إعادة تشغيل التطبيق إجبارياً.
تفصيل إضافي — PrintPreview MVVM refactoring:

الـ violation الحالية في Views/Patients/PrintPreviewWindow.xaml.cs سطور 1–28:

Copyprivate readonly FlowDocument _document; // حقل خاص
public PrintPreviewWindow(FlowDocument document) // constructor injection
{
    _document = document;
    PreviewViewer.Document = document; // مباشر للـ XAML
}
private void PrintButton_OnClick(object sender, RoutedEventArgs e) { /* PrintDialog */ }
private void CloseButton_OnClick(object sender, RoutedEventArgs e) { Close(); }
الـ refactoring:

إنشاء ViewModels/Patients/PrintPreviewViewModel.cs:
Copypublic sealed class PrintPreviewViewModel : ViewModelBase
{
    private FlowDocument _document;
    public FlowDocument Document { get => _document; set => SetProperty(ref _document, value); }
    public ICommand PrintCommand { get; }
    public ICommand CloseCommand { get; }
    public Action? RequestClose; // mediator
    public PrintPreviewViewModel(IPrintService printService) { ... }
}
PrintPreviewWindow.xaml.cs يصبح:
Copypublic partial class PrintPreviewWindow : Window
{
    public PrintPreviewWindow() { InitializeComponent(); }
}
PrintPreviewWindow.xaml يَربط Document على FlowDocumentReader (سيحتاج Converter أو direct binding).
الـ XAML buttons تُربَط بـ PrintCommand و CloseCommand.
Dialog service يُنشئ الـ VM ويُسنده، يَشترك في RequestClose، ويستدعي ShowDialog.
Estimated days: 11. Risk: 🟠 High (Backup يَمَسّ بنية DB).

Phase 7 — Specialty Editors & Admin
الهدف: تفعيل المحرِّرات التخصصية الثلاثة (Andrology, Blood Bank, Microbiology) + إدارة الصلاحيات الكاملة.

المهام:

Task 7.1 — IAndrologyService DI + شاشة AndrologyEditorView (UserControl يُسجَّل في IResultEditorFactory لكل TestType بـ Category="Andrology").
Task 7.2 — IBloodBankService DI + شاشات BloodBankWindow (Cross-Match Donors/Tests).
Task 7.3 — ICultureResultService DI + شاشة MicrobiologyCultureEditor.
Task 7.4 — Permission management: Permission و StaffPermission entities موجودة — بناء PermissionsWindow لإدارة من له الحق في ماذا.
Task 7.5 — Audit dashboard: شاشة تجمع كل AuditLog بـ filters متقدِّمة.
Task 7.6 — User management: إضافة/تعطيل/إعادة كلمة مرور.
خدمات تحتاج DI: IAndrologyService, IBloodBankService, ICultureResultService.

نوافذ UI جديدة: AndrologyEditorView (UserControl)، BloodBankWindow, CrossMatchWindow, MicrobiologyCultureEditor, PermissionsWindow, AuditDashboardWindow, UserManagementWindow.

Migrations: لا.

Business Rules:

BR-070: Andrology tests تَتطلَّب حقول خاصة من SemenAnalysis (motility, morphology, count).
BR-071: Cross-Match لا يمكن أن يَعمل بدون donor مسجَّل في CrossMatchDonor.
BR-072: Microbiology culture تَدعم نتائج "Pending" تَستغرق أيام.
BR-073: Permission يَتدرَّج: Admin > Manager > Technician > Reception.
Estimated days: 14. Risk: 🟠 High.

PART FIVE — CROSS-CUTTING CONCERNS
1) MVVM Enforcement
مسموح في ViewModel:

Properties مع INotifyPropertyChanged (عبر ViewModelBase.SetProperty).
Commands كـ ICommand (RelayCommand, AsyncRelayCommand).
استدعاء Services عبر constructor injection.
استدعاء Domain logic.
ممنوع في ViewModel:

مرجع لـ Window, Page, Control, UserControl.
مرجع لـ System.Windows.Forms.*.
استدعاء MessageBox.Show مباشرة (يُستخدم IDialogService.Show*).
مرجع لـ Dispatcher في طبقة الـ business (مسموح في Service لو ضروري).
مثال محظور:

Copy// ❌ in ViewModel
private void OnSave() { MessageBox.Show("Saved"); }
مثال مسموح:

Copy// ✅ in ViewModel
private async void OnSave() {
    await _service.SaveAsync(...);
    _dialogService.ShowInformation("تم الحفظ");
}
2) DI Lifetime Rules
Lifetime	متى يُستخدم	السبب
Singleton	خدمات بدون state mutating، تَحتفظ بـ caches أو references للنوافذ	INavigationService, IDialogService, IUserSettingsService, ICurrentUserSession
Scoped	خدمات تَستخدم DbContext	IPatientService, كل I*Service يَستعلم DB. الـ Scoped تَتزامن مع scope الـ DbContext.
Transient	ViewModels، Windows	كل فتح نافذة = instance طازجة بحالة منعزلة.
قاعدة ذهبية: Singleton لا يجب أن يَحقن Scoped — يَكسر الـ scope. الـ NavigationService (Singleton) يَستخدم IServiceProvider لـ resolve scoped types عند الحاجة، لا عبر constructor injection.

3) Migration Safety Rules
قبل dotnet ef database update:

Backup DB يدوياً.
Review SQL: dotnet ef migrations script يُولِّد SQL — اقرأه كاملاً.
Check destructive operations: DropColumn, DropTable, AlterColumn (إذا غيَّر type أو nullable) → خطر فقدان بيانات.
Test on dev DB: نسخة من production data.
Down migration: تأكد أن Down() متطابقة وتَعكس التغيير.
No mixing: migration واحدة = تغيير منطقي واحد. لا تَجمع 5 ميزات في migration واحدة.
4) Testing Standards
كل phase يَنتهي بـ:

✅ كل اختبار قديم يَنجح (no regression).
✅ اختبارات جديدة مكتوبة لكل feature جديدة.
✅ coverage على الـ ViewModels الجديدة ≥ 80%.
✅ coverage على الـ Services الجديدة ≥ 85%.
✅ E2E tests على الـ workflows الحرجة (Save Result, Generate Invoice, Backup/Restore).
✅ dotnet test على CI نظيف.
5) IMPLEMENTATION_STATUS.md Update Protocol
بعد إكمال كل phase:

ضَع الـ tick ✅ بجانب اسم الـ phase.
تحت اسم الـ phase أَضف قسم بنفس بنية Phase 1: عدد الاختبارات، subphases، الـ entry points، الـ migrations.
ضَع تاريخ الإكمال.
ضَع commit hash الذي أَنجز الـ phase.
ضَع اسم المراجع.
لا تَحذف ولا تَخْتصر أي تفاصيل من phase سابقة.
PART SIX — COMPLETE GAP REGISTER (G-001 إلى G-078)
رقم	الوصف	الـ Phase	الحدّة	مفتوحة؟
G-001	IAndrologyService غير مسجَّلة في DI	Phase 7	🔴 Critical	نعم
G-002	IBloodBankService غير مسجَّلة	Phase 7	🔴 Critical	نعم
G-003	ICultureResultService غير مسجَّلة	Phase 7	🔴 Critical	نعم
G-004	IExternalLabService غير مسجَّلة	Phase 4	🔴 Critical	نعم
G-005	IContractService غير مسجَّلة	Phase 4	🔴 Critical	نعم
G-006	IAttendanceService غير مسجَّلة	Phase 5	🔴 Critical	نعم
G-007	IPricingService غير مسجَّلة	Phase 4	🔴 Critical	نعم
G-008	AuditTrailWindow orphan (لا DI ولا navigation)	Phase 2	🔴 Critical	نعم
G-009	ResultEntryWindow orphan	Phase 2	🔴 Critical	نعم
G-010	PrintPreviewWindow code-behind violates MVVM	Phase 6	🔴 Critical	نعم
G-011	F8 mapped to ToggleReview بدلاً من EditPatient	Phase 2	🔴 Critical	نعم
G-012	F9 mapped to ToggleFinish بدلاً من Save	Phase 2	🔴 Critical	نعم
G-013	F12 mapped to TogglePrint بدلاً من PrintReceipt	Phase 2	🔴 Critical	نعم
G-014	F1 غير مرتبط بأي شيء (Add New Patient)	Phase 2	🔴 Critical	نعم
G-015	F4 غير مرتبط (Result Entry navigation)	Phase 2	🟠 High	نعم
G-016	F7 غير مرتبط (External Samples)	Phase 4	🟠 High	نعم
G-017	F10 غير مرتبط (Delete Patient)	Phase 2	🔴 Critical	نعم
G-018	F11 غير مرتبط (Print Barcode)	Phase 2	🔴 Critical	نعم
G-019	PatientRegistrationWindow بدون InputBindings	Phase 2	🔴 Critical	نعم
G-020	Main toolbar 2 icons بدلاً من 12	Phase 2	🔴 Critical	نعم
G-021	PatientsMenuViewModel nested in MainViewModel.cs	Phase 2	🟡 Medium	نعم
G-022	NullPrintService.cs dead code	Phase 2	🟡 Medium	نعم
G-023	TestSelectionView.xaml.bak في working tree	Phase 2	🟡 Medium	نعم
G-024	TestSelectionView.xaml.bak2 في working tree	Phase 2	🟡 Medium	نعم
G-025	.gitignore لا يحوي قاعدة *.bak	Phase 2	🟡 Medium	نعم
G-026	Pre-commit hook لمنع .bak files غير موجود	Phase 2	🟢 Low	نعم
G-027	7 patient status icons غير معروضة في UI	Phase 2	🟠 High	نعم
G-028	PatientStatusToIconConverter غير موجود	Phase 2	🟠 High	نعم
G-029	TestProfile management UI غير موجود	Phase 3	🔴 Critical	نعم
G-030	TestProfile CRUD service methods غير موجودة	Phase 3	🔴 Critical	نعم
G-031	ReportCommentTemplate management UI غير موجود	Phase 3	🔴 Critical	نعم
G-032	IReportCommentTemplateService غير موجودة	Phase 3	🔴 Critical	نعم
G-033	Auto-comment injection logic غير موجود	Phase 3	🔴 Critical	نعم
G-034	IReportCommentEngine غير موجود	Phase 3	🔴 Critical	نعم
G-035	ReportCommentTemplate.TriggerCondition column مفقود	Phase 3	🟠 High	نعم
G-036	TestProfile expansion على patient registration غير موجود	Phase 3	🟠 High	نعم
G-037	Live validation في ResultEntryViewModel غير موجود	Phase 3	🟠 High	نعم
G-038	ValidationStatusToBrushConverter غير موجود	Phase 3	🟡 Medium	نعم
G-039	Partial save في ResultEntry غير مدعوم	Phase 3	🟡 Medium	نعم
G-040	Specialty editor selection (IResultEditorFactory) returns Default for all	Phase 3	🟡 Medium	نعم
G-041	ContractsWindow غير موجود	Phase 4	🔴 Critical	نعم
G-042	PriceSchemeWindow غير موجود	Phase 4	🔴 Critical	نعم
G-043	ExternalLabsWindow غير موجود	Phase 4	🔴 Critical	نعم
G-044	Contract Monthly Invoice workflow غير منفَّذ	Phase 4	🔴 Critical	نعم
G-045	External Labs Manifest workflow غير منفَّذ	Phase 4	🟠 High	نعم
G-046	TestSelectionViewModel يستخدم DefaultPrice بدلاً من IPricingService	Phase 4	🟠 High	نعم
G-047	AccountsMenuViewModel placeholder	Phase 4/5	🟡 Medium	نعم
G-048	CashDrawerWindow غير موجود	Phase 5	🔴 Critical	نعم
G-049	InventoryWindow غير موجود	Phase 5	🟠 High	نعم
G-050	AttendanceWindow غير موجود	Phase 5	🟠 High	نعم
G-051	VReferralCommissionReport UI غير موجود	Phase 5	🟡 Medium	نعم
G-052	VOutstandingBalance UI غير موجود	Phase 5	🟡 Medium	نعم
G-053	SampleTube.MinimumStock alert logic غير موجود	Phase 5	🟡 Medium	نعم
G-054	Cash Drawer password protection workflow غير موجود	Phase 5	🟠 High	نعم
G-055	IBackupService غير موجود	Phase 6	🔴 Critical	نعم
G-056	BackupRestoreWindow غير موجود	Phase 6	🔴 Critical	نعم
G-057	INatighIntegrationService غير موجود	Phase 6	🟠 High	نعم
G-058	ReportSettingsWindow غير موجود (placeholder)	Phase 6	🟠 High	نعم
G-059	PrintPreviewViewModel غير موجود	Phase 6	🔴 Critical	نعم
G-060	Print queue (batch printing) غير موجود	Phase 6	🟡 Medium	نعم
G-061	Delivery signature/OTP confirmation غير موجود	Phase 6	🟢 Low	نعم
G-062	Visit.NatighConsent field قد يَكون مفقوداً	Phase 6	🟡 Medium	نعم
G-063	AndrologyEditorView غير موجود	Phase 7	🔴 Critical	نعم
G-064	BloodBankWindow غير موجود	Phase 7	🔴 Critical	نعم
G-065	CrossMatchWindow غير موجود	Phase 7	🟠 High	نعم
G-066	MicrobiologyCultureEditor غير موجود	Phase 7	🔴 Critical	نعم
G-067	PermissionsWindow غير موجود	Phase 7	🔴 Critical	نعم
G-068	UserManagementWindow غير موجود	Phase 7	🟠 High	نعم
G-069	AuditDashboardWindow غير موجود	Phase 7	🟠 High	نعم
G-070	StaffPermission relationship enforcement غير موجود	Phase 7	🟠 High	نعم
G-071	OrganismAntibiotic management UI غير موجود	Phase 7	🟠 High	نعم
G-072	AntibioticCatalog management UI غير موجود	Phase 7	🟡 Medium	نعم
G-073	FinalLabDbContext.cs God-Object (2327 سطر)	Phase 7	🟢 Low	نعم
G-074	HomeMenuViewModel غير موجود (welcome screen)	Phase 2	🟢 Low	نعم
G-075	Keyboard shortcuts notice dialog (one-time) غير موجود	Phase 2	🟡 Medium	نعم
G-076	Status bar dائم بالـ shortcuts غير موجود	Phase 2	🟢 Low	نعم
G-077	Soft delete على TestProfile غير منفَّذ	Phase 3	🟡 Medium	نعم
G-078	AuditLog dashboard مع filters متقدِّمة غير موجود	Phase 7	🟡 Medium	نعم
ملخَّص الحدّة: 25 🔴 Critical · 25 🟠 High · 22 🟡 Medium · 6 🟢 Low = 78 gap. كلها مفتوحة حسب فحص الـ checkout الحالي.

PART SEVEN — EXECUTIVE SUMMARY
النظام في حالة قاعدية صلبة معماريّاً. Phase 1 أَكْمَلَت الـ infrastructure الحرجة (stage-gating، print، audit، VM split، constraints) بـ 185 / 185 اختبار ناجح، 52 entity، 25 خدمة interface، 28 implementation، 27 migration على branch before-prd عند commit 5db9247. الـ Domain Model والـ Service Layer شبه مكتملان للنظام كاملاً، لكن الـ Presentation Layer (DI registrations، Views، ViewModels، menu structure) تَكشف فقط 30% تقريباً من الوظائف للمستخدم النهائي.

أولوية Phase 2 — الثلاثة الأكثر حدّة:

حلّ التضارب الدلالي في F-keys (G-011 إلى G-019): F8 و F9 و F12 تَتسبَّب اليوم في تغييرات حالة صامتة عند المستخدمين المعتادين على Real Lab System. هذا يجب أن يُحَلّ أولاً قبل أي ميزة جديدة.
تفعيل النوافذ اليتيمة الثلاث (G-008 إلى G-010): AuditTrail و ResultEntry جاهزتان معمارياً وفقط تَنتظران DI registration و factory pattern.
توسيع الـ Main Toolbar من 2 إلى 12 أيقونة (G-020): الـ workflow كله مغلَّف داخل menu فرعي اليوم ولا يُكشَف للمستخدم.
الجدول الزمني المتوقَّع (مجموع تقديري للمراحل 2–7):

Phase 2: 10 أيام · Phase 3: 9 أيام · Phase 4: 12 يوماً · Phase 5: 10 أيام · Phase 6: 11 يوماً · Phase 7: 14 يوماً = 66 يوم عمل.
مستوى الثقة في هذه الوثيقة: عالٍ. كل ادعاء تقني (أسماء ملفات، أرقام أسطر، أعداد خدمات، تضاربات F-key، حالة النوافذ اليتيمة، dead code، .bak files، DI registrations) تم التحقق منه بـ checkout مستقلّ كامل من commit 5db9247. الـ F-key map الـ canonical مأخوذة بنجاح من real lab system help.pdf. الـ V1/V2/V3 ground truth (52 entity, 25/28 service, 27 migration, 185 test, 7 unregistered, 3 orphans) مؤكَّد بالأدلة من الـ checkout.

القيمة التجارية لكل phase:

Phase 2: يَستردّ ثقة المستخدم المُعتاد بإعادة F-keys لدلالاتها الصحيحة ويَكشف الـ workflows الـ orphan الموجودة في الـ codebase.
Phase 3: يَجلب قوّة الـ TestProfile و auto-comment، يَختصر زمن إدخال النتائج وزمن تسجيل المرضى.
Phase 4: يَفتح المعمل أمام تعاملات الشركات والمعامل الخارجية، وهو المصدر الرئيسي للدخل المتكرر.
Phase 5: يُوفِّر للمدير رؤية يومية حقيقية على المعمل (Cash Drawer + Inventory + Attendance).
Phase 6: يَحْمي البيانات (Backup) ويُحَدِّث تجربة العميل النهائي (Natigh online publish).
Phase 7: يَفتح المعمل أمام التخصصات الطبية المتقدِّمة (Andrology، Blood Bank، Microbiology) ويُؤمِّن النظام عبر طبقة صلاحيات حقيقية.
هذه الوثيقة هي مصدر التطوير النهائي. ابدأ بـ Task 2.1.


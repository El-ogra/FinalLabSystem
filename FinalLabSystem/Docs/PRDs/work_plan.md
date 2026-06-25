القسم 1 — ملخص Phase 2
الهدف بجملة واحدة: استقرار طبقة الهويّة المريضية وكشف الـ workflows اليتيمة عبر تسجيل DI صحيح، تطبيق Factory Pattern لفتح النوافذ، مواءمة F-keys مع دليل النظام المرجعي، توسيع القائمة الرئيسية إلى 12 أيقونة، تَعمِيم أيقونات الحالة، وتنظيف الدَّيْن التقني.
عدد المهام: 7 مهام تنفيذية (2.1 → 2.7) + مهمة اختبارات شاملة (2.8).
الأيام المقدَّرة: 10 أيام عمل (مطابق لـ V4).
مستوى الخطر: 🟠 High — التضارب الدلالي في F-keys يَمَسّ workflow راسخ، وإعادة بناء Patient Status Icons يلامس عرض البيانات في 3 قوائم.
القسم 2 — ترتيب التنفيذ المبرَّر
أتبنى الترتيب المقترح في الـ prompt مع تعديل واحد طفيف بناءً على تحفُّظَيّ الأول والثاني:

#	المهمة	يعتمد على	السبب
1	Task 2.7 — Technical Debt Cleanup	—	حذف NullPrintService.cs + ملفّي .bak + قاعدة .gitignore يُنظف البيئة قبل أي تعديل. كما يُلغي إمكانية الخلط بين الـ IPrintService الحي والميت.
2	Task 2.1 — DI Registration & Navigation	2.7	كل ما يَلي يَستهلك DI صحيحاً. هنا تُسجَّل IAuditTrailDialogService و IResultEntryDialogService + 10 menu VMs + نافذتان جديدتان + navigation mappings.
3	Task 2.2 — AuditTrailWindow Factory Pattern	2.1	(مُعَدَّل بسبب تحفُّظ #1) النافذة تُفتح فعلاً اليوم من TestResultsViewModel بـ new مباشر — المهمة الحقيقية هي نقل الاستدعاء إلى IAuditTrailDialogService لإصلاح كسر MVVM، لا "فتح أول مرة".
4	Task 2.3 — ResultEntryWindow Factory Pattern	2.1, 2.2	(مُعَدَّل بسبب تحفُّظ #1) نفس المنطق: نقل من OpenMultiComponentEditorAsync إلى IResultEntryDialogService + تفعيل CancelCommand + إضافة RequestClose. يَتبع 2.2 لأن نفس النمط.
5	Task 2.5 — Main Dashboard 12-Icon Toolbar	2.1	يَعتمد على تسجيل DI للـ 10 menu VMs الجديدة + استخراج PatientsMenuViewModel من MainViewModel.cs.
6	Task 2.6 — Patient Status Icons Generalization	2.5	(مُعَدَّل بسبب تحفُّظ #2) المهمة فعلياً تَعمِيم عرض الـ TodayPatientWithStatusDto الجاهز على PatientSearchWindow و TodayPatientsDialog (التي تَستخدم TodayPatientDto المختصر). لا إنشاء enum/converter من الصفر.
7	Task 2.4 — F-Key Semantic Remapping	2.1, 2.2, 2.3	(مُعَدَّل بسبب تحفُّظ #3) الأعلى خطراً على UX. يَأتي أخيراً بعد ما تستقر النوافذ والقائمة. 6 أوامر من 10 موجودة فعلاً في PatientRegistrationViewModel — يَكفي إنشاء 4-5 navigation commands جديدة + إضافة Window.InputBindings في XAML.
8	Task 2.8 — Test Suite	متوازي	تُكتب الاختبارات الجديدة بالتوازي مع كل مهمة، وتُشغَّل dotnet test بعد كل مهمة لضمان عدم regression.
التعديل المقترَح على ترتيب الـ prompt الأصلي: لا تَغيير في الترتيب الكلي، لكن نطاق Task 2.2 و 2.3 و 2.4 و 2.6 تَقلَّص بناءً على ما اكتُشف فعلياً (راجع تفاصيل كل مهمة أدناه).

القسم 3 — خطوات تفصيلية لكل مهمة
🧹 Task 2.7 — Technical Debt Cleanup
الحالة الراهنة الفعلية:

FinalLabSystem/Services/Implementations/NullPrintService.cs — 15 سطراً، صفر references خارج تعريف الـ class + تعليق <see cref/> في IPrintService.cs سطر 5.
FinalLabSystem/Views/Patients/TestSelectionView.xaml.bak — 8,853 بايت.
FinalLabSystem/Views/Patients/TestSelectionView.xaml.bak2 — 10,230 بايت.
.gitignore (السطر 271): يحوي فقط *.rptproj.bak لا قاعدة عامة.
الخطوات بالتسلسل:

تنفيذ grep -rn "NullPrintService" --include="*.cs" للتأكد النهائي من صفر references خارج التعريف.
حذف Services/Implementations/NullPrintService.cs.
تَعديل Services/Interfaces/IPrintService.cs سطر 5 — استبدال تعليق الـ <see cref/> بإشارة إلى WpfFlowDocumentPrintService.
git rm Views/Patients/TestSelectionView.xaml.bak و git rm Views/Patients/TestSelectionView.xaml.bak2.
تَحديث .gitignore بإضافة كتلة:
Copy# Backup files - never commit
*.bak
*.bak2
*.orig
*.swp
إنشاء .githooks/pre-commit (bash script يَرفض الـ commits التي تَحوي .bak).
إنشاء tools/install-hooks.sh ووضع توثيقه في README.
الملفات التي ستُعدَّل:

Services/Interfaces/IPrintService.cs (سطر 5: تعليق فقط).
.gitignore (إضافة 4 أسطر).
الملفات الجديدة:

.githooks/pre-commit (script bash).
tools/install-hooks.sh (script bash).
الملفات التي ستُحذف:

Services/Implementations/NullPrintService.cs (مؤكَّد صفر references بـ grep).
Views/Patients/TestSelectionView.xaml.bak.
Views/Patients/TestSelectionView.xaml.bak2.
معيار الإتمام:

grep -rn "NullPrintService" --include="*.cs" يُرجع صفر سطور.
find . -name "*.bak*" يُرجع صفر ملفات.
dotnet build ناجح بصفر warnings.
dotnet test يُحافظ على 185/185.
🔌 Task 2.1 — DI Registration & Navigation
الحالة الراهنة الفعلية:

App.xaml.cs 221 سطراً: ConfigureServices يَنتهي سطر 213. آخر Scoped service سطر 153 (IResultEditorFactory). آخر navigation register سطر 98 (CategoriesGroupsViewModel).
الخطوات بالتسلسل:

إضافة بين سطر 153 و 155 تسجيل خدمتَي Factory الجديدتَين كـ Singleton (تَحتاج IServiceProvider لـ resolve scoped types عند الحاجة):
Copyservices.AddSingleton<IAuditTrailDialogService, AuditTrailDialogService>();
services.AddSingleton<IResultEntryDialogService, ResultEntryDialogService>();
إضافة بعد سطر 198 تسجيل النافذتَين كـ Transient:
Copyservices.AddTransient<AuditTrailWindow>();
services.AddTransient<ResultEntryWindow>();
إضافة قسم جديد لـ Menu ViewModels (Transient — 11 ViewModel):
Copyservices.AddTransient<HomeMenuViewModel>();
services.AddTransient<PatientsMenuViewModel>();     // بعد استخراجه (Task 2.5)
services.AddTransient<ResultsMenuViewModel>();
services.AddTransient<DeliveryMenuViewModel>();
services.AddTransient<SearchMenuViewModel>();
services.AddTransient<ExternalSamplesMenuViewModel>();
services.AddTransient<AccountsMenuViewModel>();
services.AddTransient<BackupMenuViewModel>();
services.AddTransient<TestDataMenuViewModel>();
services.AddTransient<NormalRangesMenuViewModel>();
services.AddTransient<ReportSettingsMenuViewModel>();
إضافة بعد سطر 98 navigation mappings:
Copynavigation.RegisterWindow<AuditTrailViewModel, AuditTrailWindow>();
navigation.RegisterWindow<ResultEntryViewModel, ResultEntryWindow>();
ملاحظة: AuditTrailViewModel و ResultEntryViewModel لن يُسجَّلا في DI لأن الـ Factory يُنشئهما يدوياً.
الملفات التي ستُعدَّل:

App.xaml.cs (إضافة ~16 سطر تسجيل).
الملفات الجديدة: تُنشأ كأجزاء من Task 2.2/2.3/2.5 — هنا فقط تُسجَّل.

الملفات التي ستُحذف: لا شيء.

معيار الإتمام:

dotnet build ناجح.
اختبار services.BuildServiceProvider() لا يُلقي استثناءات (smoke test جديد).
التطبيق يُقلع ويَصِل إلى Login screen دون فشل.
ملاحظة على DI lifetime: تَوافق تام مع PART FIVE من V4 — Singleton للـ navigation/dialog services، Scoped للـ services التي تَستهلك DbContext، Transient للـ ViewModels و Windows.

🪟 Task 2.2 — AuditTrailWindow: تَطبيق Factory Pattern
الحالة الراهنة الفعلية (تَصحيح لـ V4):

Views/Patients/AuditTrailWindow.xaml.cs (12 سطر): code-behind نظيف InitializeComponent.
ViewModels/Patients/AuditTrailViewModel.cs (36 سطر): sealed class، constructor مُحَمَّل بـ overload لـ List<AuditLog> و overload لـ List<VResultAuditTrail>.
النافذة تُفتح فعلاً اليوم من ViewModels/Patients/TestResultsViewModel.cs:
سطر 785-799: ShowAuditPAsync يستدعي new AuditTrailWindow { DataContext = vm, Owner = ... }.ShowDialog().
سطر 801-814: ShowAuditTAsync نفس النمط لتحاليل التحاليل.
الأزرار في TestResultsWindow.xaml سطر 515, 523 مربوطة بـ ShowAuditPCommand و ShowAuditTCommand.
الخطوات بالتسلسل:

إنشاء Services/Interfaces/IAuditTrailDialogService.cs:
Copypublic interface IAuditTrailDialogService
{
    void ShowGeneralAudit(string title, List<AuditLog> entries);
    void ShowResultAudit(string title, List<VResultAuditTrail> entries);
}
إنشاء Services/Implementations/AuditTrailDialogService.cs — يأخذ IServiceProvider ويَستخدم Application.Current.Dispatcher لضمان UI thread.
تَعديل TestResultsViewModel.cs:
حقن IAuditTrailDialogService _auditTrailDialogService في constructor.
استبدال محتوى ShowAuditPAsync و ShowAuditTAsync (سطور 785-814) باستدعاء _auditTrailDialogService.ShowGeneralAudit(...) أو ShowResultAudit(...).
إزالة using FinalLabSystem.Views.Patients إن لم يُستخدم في الـ ViewModel.
(لا تَعديل ضروري في XAML — الـ commands المربوطة تستمر تعمل).
الملفات التي ستُعدَّل:

ViewModels/Patients/TestResultsViewModel.cs — حقن جديد + استبدال جسم 2 methods.
الملفات الجديدة:

Services/Interfaces/IAuditTrailDialogService.cs.
Services/Implementations/AuditTrailDialogService.cs.
الملفات التي ستُحذف: لا شيء.

معيار الإتمام:

grep "new AuditTrailWindow" --include="*.cs" يُرجع 0 سطور خارج AuditTrailDialogService.cs.
زر "سجل تدقيق الزيارة" يَفتح نافذة modal بالمحتوى الصحيح.
5 فتحات/إغلاقات متتالية لا تُسرِّب instances.
🪟 Task 2.3 — ResultEntryWindow: تَطبيق Factory Pattern + تَفعيل CancelCommand
الحالة الراهنة الفعلية (تَصحيح لـ V4):

Views/Patients/ResultEntryWindow.xaml.cs (12 سطر): نظيف.
ViewModels/Patients/ResultEntryViewModel.cs (104 سطر):
Constructor يأخذ 4 خدمات + 4 بارامترات بيانية (سطر 25-44).
SaveCommand يَستدعي SaveAsync (سطر 73).
CancelCommand = new RelayCommand(_ => { }) فارغ (سطر 44).
SaveCompleted event (سطر 71).
النافذة تُفتح فعلاً من TestResultsViewModel.OpenMultiComponentEditorAsync سطر 626-651 بـ new ResultEntryWindow { DataContext = vm }.ShowDialog().
MouseDoubleClick موجود فعلاً في TestResultsWindow.xaml سطر 467 → HandleRowActivateCommand → EnterResultAsync → OpenMultiComponentEditorAsync للـ tests متعددة المكوّنات.
الخطوات بالتسلسل:

إنشاء Services/Interfaces/IResultEntryDialogService.cs:
Copypublic interface IResultEntryDialogService
{
    Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName,
                         ObservableCollection<TestComponentResultDto> components);
}
إنشاء Services/Implementations/ResultEntryDialogService.cs — يَحُلّ الـ 4 خدمات من IServiceProvider، يُنشئ VM، يَربط SaveCompleted و RequestClose بـ window.Close().
تَعديل ResultEntryViewModel.cs:
إضافة public Action? RequestClose { get; set; } بعد سطر 71.
تَعديل CancelCommand سطر 44 إلى:
CopyCancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
بعد SaveCompleted?.Invoke(...) في سطر 97 إضافة RequestClose?.Invoke();.
معالجة الـ exception في SaveAsync بحيث لا يُغلق النافذة عند الفشل (لا تَستدعِ RequestClose داخل catch).
تَعديل TestResultsViewModel.cs:
حقن IResultEntryDialogService _resultEntryDialogService في constructor.
استبدال جسم OpenMultiComponentEditorAsync سطور 626-651 باستدعاء await _resultEntryDialogService.OpenAsync(...).
إن أرجَع true (saved)، استدعاء SelectPatientAsync(SelectedPatient) لـ refresh.
الملفات التي ستُعدَّل:

ViewModels/Patients/ResultEntryViewModel.cs — إضافة RequestClose + تَفعيل CancelCommand + استدعاء RequestClose بعد SaveCompleted.
ViewModels/Patients/TestResultsViewModel.cs — حقن جديد + استبدال جسم method.
الملفات الجديدة:

Services/Interfaces/IResultEntryDialogService.cs.
Services/Implementations/ResultEntryDialogService.cs.
الملفات التي ستُحذف: لا شيء.

معيار الإتمام:

grep "new ResultEntryWindow" --include="*.cs" يُرجع صفر سطور خارج ResultEntryDialogService.cs.
Double-click على صف CBC بـ 5 مكوّنات يَفتح النافذة، الحفظ يُحدِّث القائمة، الـ Cancel يُغلق دون كتابة.
اختبار double-write بضغطتَين سريعتَين: IsSaving يَمنع التَكرار.
🏠 Task 2.5 — Main Dashboard 12-Icon Toolbar
الحالة الراهنة الفعلية:

MainWindow.xaml (81 سطر): ToolBarTray يحوي Button واحد لـ "المرضى" (سطر 60) و Button واحد لـ "إعدادات النظام" (سطر 68). DataTemplate لـ PatientsMenuViewModel (سطر 10) و SystemSettingsMenuViewModel (سطر 37).
ViewModels/MainViewModel.cs (87 سطر): 8 commands. nested class PatientsMenuViewModel من سطر 66 إلى 87.
SystemSettingsMenuViewModel.cs موجود مستقلاً في ViewModels/Settings/ (نموذج جيد للنقل).
الخطوات بالتسلسل:

إنشاء مجلد ViewModels/Menu/.
نقل PatientsMenuViewModel من MainViewModel.cs سطور 66-87 إلى ViewModels/Menu/PatientsMenuViewModel.cs. اختيار: الإبقاء على namespace FinalLabSystem.ViewModels لتجنُّب كسر XAML reference الحالي (xmlns:vm في MainWindow.xaml سطر 4-5). هذا أقل تَدخُّلاً من نقل namespace.
إنشاء 10 ViewModels جديدة تحت ViewModels/Menu/:
HomeMenuViewModel.cs (welcome، NoOpCommand).
ResultsMenuViewModel.cs (NavigateToTestResultsCommand).
DeliveryMenuViewModel.cs (NavigateToDeliveryCommand).
SearchMenuViewModel.cs (NavigateToSearchCommand).
ExternalSamplesMenuViewModel.cs (PlaceholderCommand → "Phase 4").
AccountsMenuViewModel.cs (PlaceholderCommand → "Phase 5").
BackupMenuViewModel.cs (PlaceholderCommand → "Phase 6").
TestDataMenuViewModel.cs (NavigateToTestDataCommand + NavigateToCategoriesGroupsCommand).
NormalRangesMenuViewModel.cs (NavigateToNormalRangesCommand).
ReportSettingsMenuViewModel.cs (PlaceholderCommand → "Phase 6").
إنشاء helper static Infrastructure/MenuPlaceholderHelper.cs لتجنُّب تكرار الـ dialog code.
تَوسيع MainViewModel.cs: إضافة 10 commands جديدة + ربط CurrentView بأي من 12 Menu VM.
تَعديل MainWindow.xaml:
استبدال ToolBar بـ 12 Button (أيقونة + نص) منظَّمة في WrapPanel أو UniformGrid Rows="1" Columns="12".
إضافة 10 DataTemplate جديدة لـ Menu VMs.
الأزرار التي تَستدعي Placeholder تُمَيَّز بصرياً (Foreground رمادي + Tooltip).
تَسجيل DI: مُنفَّذ في Task 2.1.
الملفات التي ستُعدَّل:

ViewModels/MainViewModel.cs (إزالة nested + إضافة 10 commands).
MainWindow.xaml (12 أزرار + 10 DataTemplates).
الملفات الجديدة:

11 ملف في ViewModels/Menu/ (PatientsMenuViewModel.cs + 10 جديدة).
Infrastructure/MenuPlaceholderHelper.cs.
الملفات التي ستُحذف: لا شيء (الـ nested class يُنقَل لا يُحذف).

معيار الإتمام:

MainViewModel.cs يحوي class واحد فقط.
شريط الـ Toolbar يَعرض 12 أيقونة قابلة للنقر.
النقر على "العينات المُرسَلة للخارج" يَعرض dialog "متاحة في Phase 4".
النقر على "المرضى" يَعرض الـ 4 sub-buttons الحالية بلا regression.
🎨 Task 2.6 — Patient Status Icons Generalization
الحالة الراهنة الفعلية (تَصحيح جوهري لـ V4):

Enum PatientVisitStatus موجود بـ 7 قيم تَطابق دلالياً الـ 7 من Help.pdf.
TodayPatientWithStatusDto يحوي ComputedStatus + StatusIcon (string emoji) + StatusColor (hex).
VisitService.GetTodayPatientsWithStatusAsync (سطر 458) يَحسب الكل server-side عبر EF projection (تَجنُّب N+1).
GetStatusIcon (سطر 561) و GetStatusColor (سطر 574) كـ private methods.
الأيقونات معروضة بالفعل في TestResultsWindow.xaml سطر 171-172.
الأيقونات غير معروضة في PatientSearchWindow.xaml و TodayPatientsDialog.xaml.
PatientRegistrationViewModel و TodayPatientsDialogViewModel يَستخدمان TodayPatientDto المختصر (بدون status).
الخطوات بالتسلسل (نطاق مُقَلَّص):

تَحويل PatientRegistrationViewModel.TodayPatients من ObservableCollection<TodayPatientDto> إلى ObservableCollection<TodayPatientWithStatusDto>. تَعديل LoadTodayPatientsAsync لاستخدام GetTodayPatientsWithStatusAsync بدل GetTodayPatientListAsync.
تَحويل TodayPatientsDialogViewModel بنفس الطريقة.
تَحويل PatientSearchViewModel (لـ patients results) — إن أمكن إضافة status بـ method جديدة في IPatientService، أو إعادة استخدام GetTodayPatientsWithStatusAsync مع فلتر ID.
تَعديل 3 XAML files (PatientSearchWindow.xaml, TodayPatientsDialog.xaml, PatientRegistrationWindow.xaml لقسم اليوم) لإضافة عمود/StackPanel فيه TextBlock Text="{Binding StatusIcon}" Foreground="{Binding StatusColor}" — بنفس نمط TestResultsWindow.xaml سطر 171-178.
اختياري: استخراج الـ private methods GetStatusIcon/GetStatusColor من VisitService إلى Infrastructure/PatientStatusPresentation.cs (static helpers) للسماح بإعادة استخدامها في PatientService إن لزم.
ملاحظات على تَعارُض V4:

لا حاجة لإنشاء PatientStatusIcon enum جديد — PatientVisitStatus موجود ويفي بالغرض.
لا حاجة لـ PatientStatusToIconConverter — الـ StatusIcon و StatusColor يُرسَلان كـ strings جاهزة من Service.
لا حاجة لـ Models/Enums/PatientStatusIcon.cs.
إن أصرّت الجهة المنفِّذة على enum مستقل وفقاً لـ V4، فهذا تكرار للمنطق الموجود ولا يُضيف قيمة.
الملفات التي ستُعدَّل:

ViewModels/Patients/PatientRegistrationViewModel.cs (تَغيير نوع TodayPatients collection).
ViewModels/Patients/TodayPatientsDialogViewModel.cs.
ViewModels/Patients/Search/PatientSearchViewModel.cs.
Views/Patients/PatientSearchWindow.xaml (إضافة عمود/أيقونة).
Views/Patients/TodayPatientsDialog.xaml.
Views/Patients/PatientRegistrationWindow.xaml (قسم today patients).
(اختياري) Services/Implementations/VisitService.cs (نقل helpers).
الملفات الجديدة:

(اختياري) Infrastructure/PatientStatusPresentation.cs.
الملفات التي ستُحذف: لا شيء.

معيار الإتمام:

3 قوائم تَعرض أيقونة + لون.
لا regression: TestResultsWindow تَستمر تَعرض الأيقونات.
اختبار: مريض جديد بلا نتائج → 🛒 رمادي. مريض مكتمل → ✅ أخضر.
⌨️ Task 2.4 — F-Key Semantic Remapping
الحالة الراهنة الفعلية (تَصحيح لـ V4):

TestResultsWindow.xaml سطور 791-800: 7 KeyBindings (4 صحيحة + 3 خاطئة + Escape).
PatientRegistrationWindow.xaml: لا KeyBindings.
PatientRegistrationViewModel يحوي 6 أوامر مطابقة (AddNewCommand, SaveCommand, EditCommand, DeleteCommand, BarcodeCommand, ReceiptCommand) — يَكفي ربطها بـ KeyBindings + إنشاء 5 navigation commands.
TestResultsViewModel يحوي ToggleReviewStatusCommand, ToggleFinishStatusCommand, TogglePrintStatusCommand (سطور 109-111).
الخطوات بالتسلسل:

أ. في TestResultsWindow.xaml سطور 791-800:

تَعديل ثلاث KeyBindings خاطئة:
Copy<KeyBinding Key="R" Modifiers="Ctrl" Command="{Binding ToggleReviewStatusCommand}"/>
<KeyBinding Key="F" Modifiers="Ctrl" Command="{Binding ToggleFinishStatusCommand}"/>
<KeyBinding Key="P" Modifiers="Ctrl" Command="{Binding TogglePrintStatusCommand}"/>
إضافة جديدة:
Copy<KeyBinding Key="F8" Command="{Binding EditSelectedPatientCommand}"/>
<KeyBinding Key="F12" Command="{Binding PrintReceiptCommand}"/>
<KeyBinding Key="F4" Command="{Binding NavigateToResultEntryCommand}"/>
<KeyBinding Key="F7" Command="{Binding NavigateToExternalSamplesCommand}"/>
ب. إضافة في TestResultsViewModel.cs (4 أوامر جديدة):

EditSelectedPatientCommand — يَفتح PatientRegistrationWindow لتَعديل بيانات SelectedPatient. يَحتاج توسعة INavigationService.OpenTaskWindow<T>(Action<T>? configure) لتَمرير patientId.
PrintReceiptCommand — يَستدعي IReceiptService.GenerateReceiptAsync(visitId) ثم IPrintService.PrintAsync(doc).
NavigateToResultEntryCommand — يَفتح TestResultsWindow نفسها (تأكيد لـ Help.pdf F4).
NavigateToExternalSamplesCommand — placeholder dialog "Phase 4".
ج. إضافة في Views/Patients/PatientRegistrationWindow.xaml كتلة Window.InputBindings كاملة:

Copy<Window.InputBindings>
    <KeyBinding Key="F1"  Command="{Binding AddNewCommand}"/>          <!-- موجود -->
    <KeyBinding Key="F2"  Command="{Binding NavigateToPatientDataCommand}"/>   <!-- جديد -->
    <KeyBinding Key="F3"  Command="{Binding NavigateToSearchCommand}"/>        <!-- جديد -->
    <KeyBinding Key="F4"  Command="{Binding NavigateToResultEntryCommand}"/>   <!-- جديد -->
    <KeyBinding Key="F5"  Command="{Binding LoadTodayPatientsCommand}"/>       <!-- إعادة استخدام -->
    <KeyBinding Key="F6"  Command="{Binding NavigateToDeliveryCommand}"/>      <!-- جديد -->
    <KeyBinding Key="F7"  Command="{Binding NavigateToExternalSamplesCommand}"/> <!-- جديد placeholder -->
    <KeyBinding Key="F8"  Command="{Binding EditCommand}"/>            <!-- موجود -->
    <KeyBinding Key="F9"  Command="{Binding SaveCommand}"/>            <!-- موجود -->
    <KeyBinding Key="F10" Command="{Binding DeleteCommand}"/>          <!-- موجود -->
    <KeyBinding Key="F11" Command="{Binding BarcodeCommand}"/>         <!-- موجود -->
    <KeyBinding Key="F12" Command="{Binding ReceiptCommand}"/>         <!-- موجود -->
    <KeyBinding Key="Escape" Command="{Binding ReturnToMainCommand}"/> <!-- موجود -->
</Window.InputBindings>
د. إضافة في PatientRegistrationViewModel.cs (5 أوامر تَنقُّل جديدة فقط، لا 10 كما تقول V4):

NavigateToPatientDataCommand
NavigateToSearchCommand
NavigateToResultEntryCommand
NavigateToDeliveryCommand
NavigateToExternalSamplesCommand (placeholder)
هـ. الإعلام لمرّة واحدة:

إضافة flag KeyboardShortcutsNoticeShown في JsonUserSettingsService.
عند أول إقلاع بعد الترقية، dialog: "تم تَحديث اختصارات لوحة المفاتيح. F8/F9/F12 الآن تَفتح/تَحفظ/تَطبع. للوظائف القديمة استخدم Ctrl+R / Ctrl+F / Ctrl+P."
شريط حالة دائم أسفل TestResultsWindow يَعرض الاختصارات الـ 12.
الملفات التي ستُعدَّل:

Views/Patients/TestResultsWindow.xaml (تَعديل 3 سطور KeyBinding + إضافة 4 KeyBindings + شريط حالة).
Views/Patients/PatientRegistrationWindow.xaml (إضافة Window.InputBindings 13 سطر).
ViewModels/Patients/TestResultsViewModel.cs (إضافة 4 commands + handlers).
ViewModels/Patients/PatientRegistrationViewModel.cs (إضافة 5 commands).
Infrastructure/Settings/JsonUserSettingsService.cs (إضافة flag).
Infrastructure/Navigation/INavigationService.cs (إضافة overload).
الملفات الجديدة: لا شيء (إن أمكن إعادة استخدام الـ commands الموجودة في PatientRegistrationViewModel).

الملفات التي ستُحذف: لا شيء.

معيار الإتمام:

F8 على مريض في TestResultsWindow يَفتح PatientRegistrationWindow في وضع التَعديل.
F12 يُولِّد ويَطبع إيصال.
Ctrl+R / Ctrl+F / Ctrl+P تَنفِّذ الـ legacy toggles.
F1-F12 في PatientRegistrationWindow يَنفِّذ الـ 12 وظيفة الصحيحة من Help.pdf.
F10 يَعرض dialog تأكيد ولا يَحذف بلا تأكيد.
اختبارات F-keys (12 test case) جميعها تَنجح.
القسم 4 — متطلبات Migrations
Phase 2 لا تَحتاج أي migration. كل التَغييرات على طبقة UI / VM / DI / تنظيف. لا تَغيير في الـ schema، لا أعمدة جديدة، لا جداول جديدة، لا constraints.

TodayPatientWithStatusDto ليس entity (DTO فقط، لا mapping).
PatientVisitStatus enum مستخدَم بالفعل كـ int.
7 أيقونات الحالة موجودة كـ string emoji في DTO، لا تَحتاج تخزين.
تَأكيد صريح: dotnet ef migrations list بعد Phase 2 يَبقى 27 migration.

القسم 5 — متطلبات الاختبارات
ضمانة الانحدار: الـ 185 اختباراً الأصلية يجب أن تَستمر في النجاح. تَشغيل dotnet test بعد كل مهمة من الـ 7.

الاختبارات الجديدة المطلوبة:

ملف الاختبار	ما يُختبَر	عدد الـ test cases
Tests/ViewModels/AuditTrailViewModelTests.cs	بناء VM بكلا الـ overloads + ObservableCollection + ICollectionView	4
Tests/Services/AuditTrailDialogServiceTests.cs	mock IServiceProvider، تَدفُّق إنشاء VM، فتح النافذة	3
Tests/ViewModels/ResultEntryViewModelTests.cs	بناء VM، SaveAsync يَستدعي service، SaveCompleted، CancelCommand، RequestClose	6
Tests/Services/ResultEntryDialogServiceTests.cs	factory pattern: تَأكيد إنشاء VM مع البارامترات الصحيحة + binding للـ RequestClose	3
Tests/ViewModels/MainViewModelTests.cs	كل 12 menu command تَعرض VM الصحيحة في CurrentView	12
Tests/ViewModels/Menu/PatientsMenuViewModelTests.cs	الـ 4 sub-commands تَعمل بعد النقل	4
Tests/ViewModels/Menu/PlaceholderMenusTests.cs	الـ 4 placeholder menus تَعرض dialog "Coming in Phase X"	4
Tests/ViewModels/Patients/TodayPatientsStatusDisplayTests.cs	PatientRegistrationVM.TodayPatients يَستخدم النوع الجديد، StatusIcon غير فارغ	4
Tests/Services/PatientStatusComputationTests.cs	كل 7 حالات يُحسَب لها icon + color الصحيحان (re-use GetStatusIcon)	7
Tests/ViewModels/PatientRegistrationFKeyTests.cs	كل 12 F-key command (F1-F12) يَستدعي الـ handler الصحيح	12
Tests/ViewModels/TestResultsFKeyRemappingTests.cs	Ctrl+R / Ctrl+F / Ctrl+P تَنفِّذ الـ legacy toggles، F8/F12 الجديدة، F4/F7 الجديدة	6
Tests/Infrastructure/Navigation/AuditTrailWindowRegistrationTests.cs	DI يَحُلّ النافذة، NavigationService يَفتحها	2
Tests/Infrastructure/Navigation/ResultEntryWindowRegistrationTests.cs	نفس الشيء لـ ResultEntryWindow	2
إجمالي اختبارات Phase 2 الجديدة: 69 اختبار (قُلِّص من 73 في V4 بسبب إلغاء اختبار PatientStatusToIconConverter غير المُحتاج).

الإجمالي بعد Phase 2: 185 + 69 = 254 اختبار ناجح.

القسم 6 — نقاط الخطر وكيفية التعامل معها
الخطر	حقيقي؟	الخطوة الوقائية
تسجيل ViewModel بـ scope خاطئ يُسبِّب احتفاظاً بـ DbContext بين نوافذ	نعم — مَخْطُور	التزام صارم: ViewModels = Transient، Services = Scoped، Dialog Services = Singleton (تَستخدم IServiceProvider). Smoke test يَفتح/يَغلق AuditTrailWindow 5 مرات ويتحقق من عدد instances FinalLabDbContext الحيّة.
تسريب memory في ICollectionView عند إغلاق AuditTrailWindow	نعم — منخفض	لا يُضاف event subscription. الـ VM read-only. اختبار: 5 فتحات/إغلاقات متتالية، فحص GC.
استدعاء ShowDialog من thread غير UI	نعم — منخفض	AuditTrailDialogService.ShowGeneralAudit يَستخدم Application.Current.Dispatcher.Invoke.
Double-write في ResultEntryWindow عند ضغط Save مرتين	نعم — متوسط	IsSaving flag + AsyncRelayCommand predicate (موجود فعلاً سطر 43). اختبار جديد يَستدعي SaveCommand مرتين متتاليتين بدون انتظار.
إغلاق النافذة قبل اكتمال SaveAsync	نعم — متوسط	لا تَستدعِ RequestClose داخل catch block. عرض الـ exception في IDialogService. IsSaving = false.
المستخدم يَضغط F10 ويَحذف مريضاً له visits	نعم — مَخْطُور 🔴	dialog تأكيد إجباري + IPatientService.DeleteAsync يُلقي استثناءً إن وُجدت visits غير مؤرشفة. اختبار: محاولة حذف مريض بـ visits → استثناء.
المستخدم اعتاد F8 الآن يَفتح تَعديل بدلاً من toggle review	نعم — UX عالي 🟠	dialog إعلام لمرة واحدة + شريط حالة دائم بالـ shortcuts + tooltips تَعرض الاختصار الجديد.
تَضارب F-keys بين النوافذ المتزامنة	لا — منتفٍ	WPF يَربط Window.InputBindings بالنافذة النشطة فقط.
كسر XAML data templates عند نقل PatientsMenuViewModel	نعم — منخفض	الإبقاء على namespace FinalLabSystem.ViewModels (لا تَغيير) → MainWindow.xaml لا يَحتاج تَعديل في xmlns:vm. اختبار: rebuild كامل + smoke test.
تَكرار منطق Status Icon (V4 تَطلب converter لكن المنطق موجود في Service)	نعم — تَصميمي	الالتزام بتحفُّظ #2: استخدام الـ StatusIcon و StatusColor من DTO مباشرة في XAML. لا إنشاء enum مُكَرَّر. لا converter جديد.
حذف NullPrintService.cs يُكسر اختباراً	لا — منتفٍ	grep -rn "NullPrintService" FinalLabSystem.Tests/ يُرجع 0 (تَحقَّقت بالفحص).
Git hooks لا تَنتقل مع clone	نعم — منخفض	توثيق tools/install-hooks.sh في README.
القسم 7 — شروط إعلان Phase 2 مكتملة
☐ NullPrintService.cs محذوف؛ grep -rn NullPrintService --include=*.cs يُرجع 0.
☐ TestSelectionView.xaml.bak و .bak2 محذوفان؛ find . -name "*.bak*" يُرجع 0.
☐ .gitignore يحوي قاعدة *.bak, *.bak2, *.orig, *.swp.
☐ .githooks/pre-commit يَرفض ملفات .bak (اختبار يدوي).
☐ IAuditTrailDialogService + IResultEntryDialogService مسجَّلتان كـ Singleton.
☐ AuditTrailWindow و ResultEntryWindow مسجَّلتان كـ Transient + navigation mappings.
☐ 10 Menu ViewModels مسجَّلة كـ Transient.
☐ grep "new AuditTrailWindow\|new ResultEntryWindow" --include=*.cs يُرجع 0 خارج Dialog Services.
☐ PatientsMenuViewModel في ملف منفصل تحت ViewModels/Menu/.
☐ MainViewModel.cs يحوي class واحد فقط.
☐ 12 أيقونة تَظهر في الـ Toolbar (5 وظيفية + 5 sub-menu + 4 placeholders).
☐ النقر على placeholder يَعرض dialog "Phase X".
☐ PatientRegistrationWindow لها Window.InputBindings بـ 13 KeyBinding.
☐ F1 → New، F8 → Edit، F9 → Save، F10 → Delete، F11 → Barcode، F12 → Receipt (لا ToggleReview).
☐ Ctrl+R / Ctrl+F / Ctrl+P تَنفِّذ legacy toggles في TestResultsWindow.
☐ Dialog إعلام keyboard shortcuts يَظهر مرة واحدة فقط.
☐ أيقونات الحالة (7 حالات) تُعرض في TestResultsWindow + PatientSearchWindow + TodayPatientsDialog.
☐ كل الاختبارات الجديدة الـ 69 ناجحة.
☐ الـ 185 اختباراً الأصلية لا تزال ناجحة (إجمالي 254/254).
☐ dotnet build بصفر warnings.
☐ dotnet ef migrations list يُرجع 27 migration (لا migration جديد).
☐ التطبيق يُقلع بنجاح، يَصِل إلى Login، Login ينقل إلى MainWindow بـ 12 أيقونة.
☐ Smoke test: فتح/إغلاق AuditTrailWindow و ResultEntryWindow 5 مرات → لا تَسريب memory.
☐ Docs/PRDs/IMPLEMENTATION_STATUS.md مُحَدَّث بقسم Phase 2 مكتمل (تاريخ + commit hash + قائمة الملفات + 254/254).

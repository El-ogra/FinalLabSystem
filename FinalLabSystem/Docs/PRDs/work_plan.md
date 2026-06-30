# خطة العمل التنفيذية — المرحلة السادسة
## FinalLabSystem · Phase 6: Print, Delivery & Backup

---

## خريطة التبعيات بين الشرائح

```
[شريحة 6.0 — Foundation Cleanup]
        ↓
[شريحة 6.1 — PrintPreview MVVM Refactor (تبدأ بتوسيع IPrintService)]
        ↓
[شريحة 6.2 — Backup Foundation (Service + AES + LabSetting SMTP/Backup Migration الموحَّدة)]
        ↓
[شريحة 6.3 — Backup UI & Restore Workflow]
        ↓
[شريحة 6.4 — Report Settings UI]
        ↓
[شريحة 6.5 — Print Queue / Batch]
        ↓
[شريحة 6.6 — Delivery Confirmation (Signature + OTP)]
        ↓
[شريحة 6.7 — Integration Tests + IMPLEMENTATION_STATUS Update] (التي كانت 6.8 في الخطة المصدر)
```

**مبرّر الترتيب:**
- `6.0` أولاً: يَمنع تسرُّب نمط `App.ServiceProvider` و الـ Empty Catch إلى الشرائح التالية.
- `6.1` ثانياً: قلب PRD Task 6.1، يَستفيد من توسيع `IPrintService` (V-28) كشرط مسبق لإنشاء `PrintPreviewViewModel` يستدعي `PrintFlowDocumentAsync` مباشرة.
- `6.2` → `6.3`: خدمة قبل واجهة + توحيد Migration حقول SMTP + Backup Schedule في خطوة واحدة بدل تفريقها.
- `6.4` مستقلة عن `6.2/6.3` لكنها تَستفيد من DI نظيف بعد `6.0`.
- `6.5` يَعتمد على `6.1` (يَستهلك `IPrintPreviewDialogService` و `IPrintService` الموسَّع).
- `6.6` يَتطلَّب Migration جديدة على `Visit` للـ Delivery.
- `6.7` (التي كانت `6.8`) في النهاية: E2E + تحديث Status.

**حالة الأساس المحقَّقة من الكودبيس:**
- `FinalLabSystem/Models/LabSetting.cs` يحوي فقط `EnforceStageGating` + `EnableServerPrinting` ← لا حقول SMTP/Backup (تأكيد V-27).
- `FinalLabSystem/Services/Interfaces/IPrintService.cs` يحوي فقط `Task PrintAsync(string documentType, object data)` ← لا method لطباعة `FlowDocument` جاهز (تأكيد V-28).
- `FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml.cs` يحوي `_document` كحقل + handlers `PrintButton_OnClick` و `CloseButton_OnClick` ← ينتهك MVVM (تأكيد V-02).
- `FinalLabSystem/App.xaml.cs:30` + ثلاث مواضع لاستدعاء `App.ServiceProvider.GetRequiredService<>`:
  - `ViewModels/Patients/PatientRegistrationViewModel.cs:345` (BarcodeDialog)
  - `ViewModels/Patients/PatientRegistrationViewModel.cs:357` (ReceiptDialog)
  - `ViewModels/Settings/TestDataManagementViewModel.cs:203` (NormalRangesWindow)
- `ViewModels/Patients/PatientRegistrationViewModel.cs:91-96`: `catch {}` فارغ بلا متغير `Exception ex` ولا `_logger.LogError`.
- `ViewModels/Patients/PatientRegistrationViewModel.cs:230, 343`: `var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;` (fallback خاطئ منطقياً).
- `ViewModels/Menu/BackupMenuViewModel.cs`: `PlaceholderCommand` فقط (يَعرض رسالة "قريباً").
- `ViewModels/Menu/ReportSettingsMenuViewModel.cs`: يَفتح `ReportCommentTemplateViewModel` فقط، لا نافذة لإعدادات التقرير.

---

## شريحة 6.0 — Foundation Cleanup

**الهدف:** إصلاح الانتهاكات المؤكَّدة العاجلة فقط — Service Locator (3 مواضع) و Empty Catch بدون logging و `StaffId ?? 1` — دون توسيع النطاق إلى Repository أو God Class refactoring.

**Pre-condition:** 544 اختبار ناجح على `before-prd`، `dotnet build` بدون warnings.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem/Services/Interfaces/IBarcodeDialogFactory.cs` | Interface بنمط الـ factory الموجود: `BarcodeDialogResult Show(Window? owner = null)` ترجع enum بسيط `BarcodeDialogResult { Printed, Cancelled }`. يعتمد على `IServiceProvider` داخلياً في الـ impl فقط (نمط `IAuditTrailDialogService`). |
| 2 | إنشاء | `FinalLabSystem/Services/Implementations/BarcodeDialogFactory.cs` | تنفيذ الـ factory: يَستقبل `IServiceProvider`، يَستدعي `CreateScope()`، يَحلّ `BarcodeDialogViewModel` و `BarcodeDialog`، يَضبط `Owner`، يَستدعي `ShowDialog()`. لا يحتوي business logic. |
| 3 | إنشاء | `FinalLabSystem/Services/Interfaces/IReceiptDialogFactory.cs` | Interface: `bool Show(VisitFullDto dto, Window? owner = null)` يَرجع `true` لو طُبعت. |
| 4 | إنشاء | `FinalLabSystem/Services/Implementations/ReceiptDialogFactory.cs` | تنفيذ: يحقن `IServiceProvider`، يَستدعي `GetRequiredService<ReceiptDialogViewModel>()` و `GetRequiredService<ReceiptDialog>()`, يَستدعي `vm.InitializeAsync(dto)`، يَعرض `ShowDialog()`، يَرجع `vm.PrintCommand.WasExecuted`. |
| 5 | إنشاء | `FinalLabSystem/Services/Interfaces/INormalRangesWindowFactory.cs` | Interface: `void Open(Window? owner = null)`. |
| 6 | إنشاء | `FinalLabSystem/Services/Implementations/NormalRangesWindowFactory.cs` | تنفيذ: يحقن `IServiceProvider`، يَستدعي `GetRequiredService<NormalRangesWindow>()`, يَضبط `Owner`، يَستدعي `ShowDialog()`. |
| 7 | تعديل | `FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs` | (أ) حذف `using FinalLabSystem;` و `using Microsoft.Extensions.DependencyInjection;` إن وُجدا. (ب) إضافة `private readonly IBarcodeDialogFactory _barcodeFactory; private readonly IReceiptDialogFactory _receiptFactory;` وحقنهما عبر constructor. (ج) استبدال السطر 345 بـ `var result = _barcodeFactory.Show(Window.GetWindow(this));`. (د) استبدال السطر 357 بـ `var printed = _receiptFactory.Show(dto, Window.GetWindow(this));`. (هـ) استبدال السطرين 230 و 343 من `var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;` إلى: `var staffId = _currentUserSession.CurrentUser?.StaffId ?? throw new InvalidOperationException("لا يمكن حفظ الزيارة بدون جلسة مستخدم نشطة.");`. (و) تعديل `catch` في `InitializeAsync` (السطور 91–96) إلى: `catch (Exception ex) { _logger.LogError(ex, "Failed to initialize PatientRegistrationViewModel"); _dialogService.ShowError("حدث خطأ أثناء تهيئة النموذج."); }`. |
| 8 | تعديل | `FinalLabSystem/ViewModels/Settings/TestDataManagementViewModel.cs` | استبدال السطر 203 (`App.ServiceProvider.GetRequiredService<NormalRangesWindow>()`) بالاستدعاء عبر factory محقونة: `_normalRangesFactory.Open(Window.GetWindow(this));`. إضافة `private readonly INormalRangesWindowFactory _normalRangesFactory;` إلى constructor. |
| 9 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل DI: `IBarcodeDialogFactory` → `BarcodeDialogFactory` (Singleton)، `IReceiptDialogFactory` → `ReceiptDialogFactory` (Singleton)، `INormalRangesWindowFactory` → `NormalRangesWindowFactory` (Singleton). المبرر: كل واحدة تَلجأ لـ `IServiceProvider.CreateScope()` داخلياً عند الحاجة لكائن Transient — Singleton خارجي + scope داخلي. |

### تسجيلات DI

| Lifetime | الخدمة | المبرر |
|----------|--------|--------|
| Singleton | `IBarcodeDialogFactory` | لا state خاص، فقط يَلجأ لـ Service Provider. يَتَسق مع `IAuditTrailDialogService` و `IResultEntryDialogService` (كلها Singleton). |
| Singleton | `IReceiptDialogFactory` | نفس المنطق. |
| Singleton | `INormalRangesWindowFactory` | نفس المنطق. |

### جدول الاختبارات الكامل (14 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Services/BarcodeDialogFactoryTests.cs` | `Show_ResolvesViewModel_FromServiceProvider` | أن الـ factory يحلّ `BarcodeDialogViewModel` عبر `It.IsAny()` في Mock<IServiceProvider>. | Mock<IServiceProvider> |
| نفسه | `Show_OpensDialog_AsModal_AndReturnsPrinted` | أن `ShowDialog()` يُستدعى وتُرجَع `BarcodeDialogResult.Printed` | Mock |
| نفسه | `Show_OpensDialog_AsModal_AndReturnsCancelled` | الحالة المقابلة | Mock |
| نفسه | `DI_Registration_BarcodeDialogFactory_Should_Be_Singleton` | Lifetime صحيح عبر `TestServiceProvider` | TestServiceProvider |
| `Tests/Services/ReceiptDialogFactoryTests.cs` | `Show_ResolvesViewModel_AndInitializesWithDto` | أن `vm.InitializeAsync(dto)` يُستدعى مع `VisitFullDto` | DTO: `VisitId=1, PatientSex="M", RegisteredAt=DateTime.UtcNow` |
| نفسه | `Show_DoesNotOpen_WhenCanPrintFalse` | لو `vm.CanPrint == false` لا `ShowDialog()` | Mock VM |
| نفسه | `Show_ReturnsTrue_WhenPrintCommandExecuted` | القيمة المرجعة | Mock |
| نفسه | `DI_Registration_ReceiptDialogFactory_Should_Be_Singleton` | Lifetime | TestServiceProvider |
| `Tests/Services/NormalRangesWindowFactoryTests.cs` | `Open_ResolvesWindow_FromServiceProvider` | أن `IServiceProvider` يُستدعى | Mock<IServiceProvider> |
| نفسه | `Open_SetsOwner_FromParameter` | الـ `Owner` مُمرَّر صحيحاً | Mock Window |
| نفسه | `DI_Registration_NormalRangesWindowFactory_Should_Be_Singleton` | Lifetime | TestServiceProvider |
| `Tests/ViewModels/Patients/PatientRegistrationViewModelFoundationTests.cs` | `PatientRegistrationVM_Should_Throw_When_StaffId_Is_Null` | `_currentUserSession.CurrentUser = null` ⇒ `InvalidOperationException` | CurrentUser = null |
| نفسه | `PatientRegistrationVM_Should_Use_ActualStaffId_WhenAvailable` | السلوك السوي | StaffId = 5, Sex = "M" |
| نفسه | `PatientRegistrationVM_InitializeAsync_Should_LogError_OnException` | أن `_logger.LogError(ex, ...)` يُستدعى | Mock service يَرمي `It.IsAny<Exception>()` |

### Validation Gate G6.0

**العدد التراكمي المتوقع: 558 اختبار ناجح**

نقاط تحقق إضافية:
- `dotnet build` بدون warnings.
- `grep -rn "App.ServiceProvider.GetRequiredService" FinalLabSystem/ViewModels/` يُرجع صفر نتائج (الإصلاح كامل).
- `grep -n "StaffId ??" FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs` يُرجع صفر نتائج.

---

## شريحة 6.1 — PrintPreview MVVM Refactor (PRD Task 6.1)

**الهدف:** إصلاح آخر انتهاك MVVM في المستودع بإنشاء `PrintPreviewViewModel` كامل، مع توسيع `IPrintService` بميثود طباعة `FlowDocument` جاهز كشرط مسبق لتشغيل الـ VM.

**Pre-condition:** نجاح G6.0.

### الخطوة الأولى الإلزامية (V-28): توسيع IPrintService

هذه الخطوة تَسبق إنشاء `PrintPreviewViewModel` لأنها تَكشف الـ functionality الموجودة ضمنياً في `WpfFlowDocumentPrintService` لكنها غير مكشوفة على الـ interface.

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | تعديل | `FinalLabSystem/Services/Interfaces/IPrintService.cs` | إضافة method جديد إلى الـ interface: `Task PrintFlowDocumentAsync(FlowDocument document, string description);`. يَبقى `PrintAsync(string documentType, object data)` بدون تعديل (لا يَكسر العقد القائم). إضافة `using System.Windows.Documents;`. |
| 2 | تعديل | `FinalLabSystem/Services/Implementations/WpfFlowDocumentPrintService.cs` | تطبيق الـ method الجديد: `public async Task PrintFlowDocumentAsync(FlowDocument document, string description) { if (document == null) throw new ArgumentNullException(nameof(document)); if (string.IsNullOrEmpty(description)) throw new ArgumentNullException(nameof(description)); var enablePrinting = await _featureToggleService.IsEnabledAsync(FeatureToggles.EnableServerPrinting, false); if (!enablePrinting) { _logger.LogInformation("Printing disabled by EnableServerPrinting toggle."); return; } await Dispatcher.CurrentDispatcher.InvokeAsync(() => { var dlg = new PrintDialog(); if (dlg.ShowDialog() == true) { var docSource = (IDocumentPaginatorSource)document; dlg.PrintDocument(docSource.DocumentPaginator, description); _logger.LogInformation("Printed FlowDocument: {Description}", description); } }); }`. استخراج المنطق المشترك مع `PrintDocumentAsync` إلى helper method خاص `protected virtual Task ShowPrintDialogAndPrintAsync(FlowDocument document, string description)` لتقليل التكرار. |

### الملفات المتبقية

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 3 | إنشاء | `FinalLabSystem/ViewModels/Patients/PrintPreviewViewModel.cs` | يرث `ViewModelBase`. الخصائص: `FlowDocument Document { get; set; }` مع backing field افتراضي `new FlowDocument()` و INPC، `string Description { get; set; }` افتراضي `"Lab document"`. الأوامر: `ICommand PrintCommand` (`AsyncRelayCommand(async () => await _printService.PrintFlowDocumentAsync(Document, Description), () => Document is not null)`)، `ICommand CloseCommand` (`RelayCommand(_ => RequestClose?.Invoke())`). خاصية عامة: `Action? RequestClose { get; set; }`. Constructor: `PrintPreviewViewModel(IPrintService printService)`. |
| 4 | إنشاء | `FinalLabSystem/Services/Interfaces/IPrintPreviewDialogService.cs` | Interface بنمط factory: `void Show(FlowDocument document, string description, Window? owner = null);`. يحلّ الـ VM والـ Window من DI. |
| 5 | إنشاء | `FinalLabSystem/Services/Implementations/PrintPreviewDialogService.cs` | تنفيذ يحقن `IServiceProvider`. التدفق: `var scope = _serviceProvider.CreateScope(); var vm = scope.ServiceProvider.GetRequiredService<PrintPreviewViewModel>(); var window = scope.ServiceProvider.GetRequiredService<PrintPreviewWindow>(); vm.Document = document; vm.Description = description; window.DataContext = vm; window.Owner = owner ?? Application.Current.MainWindow; vm.RequestClose = () => window.Close(); window.ShowDialog();`. |
| 6 | تعديل | `FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml.cs` | تَقليص كامل إلى: `public PrintPreviewWindow() { InitializeComponent(); }` ثم حذف الحقول الخاصة `_document` و handlers `PrintButton_OnClick` و `CloseButton_OnClick`. |
| 7 | تعديل | `FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml` | ربط `DocumentViewer` بـ `{Binding Document}`، ربط زر الطباعة بـ `{Binding PrintCommand}`، ربط زر الإغلاق بـ `{Binding CloseCommand}`. حذف `x:Name` غير المستخدم إن وُجد. |
| 8 | تعديل | `FinalLabSystem/ViewModels/Patients/ReceiptDialogViewModel.cs` | حقن `private readonly IPrintPreviewDialogService _printPreviewDialogService;` في الـ constructor. استبدال كتلة `PrintAsync` الحالية التي تَستدعي `new PrintPreviewWindow(document)` بـ: `var document = SelectedFormat == "Thermal" ? BuildThermalDocument(...) : BuildA4Document(...); _printPreviewDialogService.Show(document, "إيصال المريض", Window.GetWindow(this));`. |
| 9 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل DI: `IPrintPreviewDialogService` → `PrintPreviewDialogService` (Singleton)، `PrintPreviewViewModel` (Transient)، `PrintPreviewWindow` (Transient). |

### تسجيلات DI

| Lifetime | العنصر | المبرر |
|----------|--------|--------|
| Singleton | `IPrintPreviewDialogService` | الـ factory لا يحتفظ بـ state؛ يَلجأ لـ DI scope داخلياً. يَتَسق مع `IAuditTrailDialogService`. |
| Transient | `PrintPreviewViewModel` | كل نافذة preview تَحتاج instance مستقل مع `FlowDocument` خاص. |
| Transient | `PrintPreviewWindow` | كل preview نافذة منفصلة، تَتَسق مع بقية Windows في المشروع. |

### جدول الاختبارات الكامل (14 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Services/IPrintServiceExtensionTests.cs` (V-28) | `IPrintService_Should_Expose_PrintFlowDocumentAsync_Method` | أن الـ interface يحوي الـ method (compile-time + reflection check) | — |
| نفسه | `WpfFlowDocumentPrintService_PrintFlowDocumentAsync_Should_Use_PrintDialog_AndPrintDocument` | أن `PrintDialog().ShowDialog() == true` ⇒ `PrintDocument` يُستدعى مع `It.IsAny<FlowDocument>` و description | Mock `IFeatureToggleService` يَرجع true |
| نفسه | `WpfFlowDocumentPrintService_PrintFlowDocumentAsync_Should_Log_Description_On_Success` | أن `_logger.LogInformation` يُستدعى | Mock |
| نفسه | `WpfFlowDocumentPrintService_PrintFlowDocumentAsync_Should_Throw_When_Document_Null` | `ArgumentNullException` | `document = null` |
| `Tests/ViewModels/Patients/PrintPreviewViewModelTests.cs` | `Constructor_Sets_Default_Document_To_Empty_FlowDocument` | أن `Document` ليس null بعد البناء | — |
| نفسه | `Document_Setter_RaisesPropertyChanged` | INPC على `Document` | `new FlowDocument()` |
| نفسه | `Description_Setter_RaisesPropertyChanged` | INPC على `Description` | "test description" |
| نفسه | `PrintCommand_CallsPrintService_WithDocument_AndDescription` | أن `_printService.PrintFlowDocumentAsync(document, "...")` يُستدعى | Mock IPrintService, `It.IsAny<FlowDocument>()` |
| نفسه | `CloseCommand_Invokes_RequestClose` | `RequestClose?.Invoke()` يَستدعى الـ Action | `bool closed = false; vm.RequestClose = () => closed = true;` |
| نفسه | `PrintCommand_CanExecute_False_When_Document_Null` | `Command.CanExecute(null) == false` | — |
| `Tests/Services/PrintPreviewDialogServiceTests.cs` | `Show_ResolvesViewModel_AndWindow_FromServiceProvider` | `IServiceProvider.CreateScope().GetRequiredService<>()` يُستدعى | Mock<IServiceProvider> |
| نفسه | `Show_BindsDocument_AndDescription_On_ViewModel` | أن `vm.Document = document` و `vm.Description = description` | `new FlowDocument()` + "إيصال" |
| نفسه | `Show_Wires_RequestClose_ToWindowClose` | `vm.RequestClose = () => window.Close()` | Mock Window |
| نفسه | `DI_Registration_PrintPreviewDialogService_Should_Be_Singleton` | Lifetime | TestServiceProvider |

### Validation Gate G6.1

**العدد التراكمي المتوقع: 572 اختبار ناجح**

نقاط تحقق إضافية:
- `dotnet build` بدون warnings.
- `wc -l FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml.cs` يُرجع ≤ 12 سطر.
- `grep -n "_document\|PrintButton_OnClick\|CloseButton_OnClick" FinalLabSystem/Views/Patients/PrintPreviewWindow.xaml.cs` يُرجع صفر نتائج.
- `IPrintService` يَحوي `PrintFlowDocumentAsync` (compile-time guarantee).

---

## شريحة 6.2 — Backup Foundation (Service + AES + Unified LabSetting Migration)

**الهدف:** بناء `IBackupService` كاملاً (AES-256-CBC + PBKDF2 100k iterations) مع Migration موحَّدة تضيف حقول SMTP و Backup Schedule إلى `LabSetting` (V-27 من agent_4 — تنفيذاً لتوصية الدمج الطبيعي).

**Pre-condition:** نجاح G6.1.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem/Models/Enums/BackupType.cs` | enum: `Full, Incremental`. تَستهلكها واجهة الخدمة. |
| 2 | إنشاء | `FinalLabSystem/Models/DTOs/BackupMetadataDto.cs` | POCO: `string FileName`, `string FilePath`, `DateTime CreatedAt`, `long FileSizeBytes`, `int? CreatedByStaffId`, `bool IsEncrypted`, `string SchemaVersion`. |
| 3 | إنشاء | `FinalLabSystem/Infrastructure/Security/AesEncryptionHelper.cs` | utility class static. methods: `byte[] Encrypt(byte[] plaintext, string password)`، `byte[] Decrypt(byte[] ciphertext, string password)`, `byte[] DeriveKey(string password, byte[] salt, int iterations = 100_000)`. يستخدم `Aes.Create()` مع `CipherMode.CBC` + `PaddingMode.PKCS7`. يَستخرج salt و IV من رأس الملف بصيغة `[16-byte salt][16-byte IV][ciphertext]`. |
| 4 | إنشاء | `FinalLabSystem/Services/Interfaces/IBackupService.cs` | 4 methods: `Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type, int staffId)`، `Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword, int staffId)`، `Task<List<BackupMetadataDto>> ListBackupsAsync(string folder)`، `Task<bool> ValidateBackupFileAsync(string backupFilePath, string adminPassword)`. |
| 5 | إنشاء | `FinalLabSystem/Migrations/20260701000000_AddLabSettingSmtpAndBackupConfig.cs` | Migration موحَّدة تضيف الحقول الـ 8 إلى جدول `LabSetting` (بدون أي حقل يخص Natigh):<br>① `SmtpHost` (`nvarchar(200)`, nullable)<br>② `SmtpPort` (`int`, nullable, `defaultValue: 587`)<br>③ `SmtpUsername` (`nvarchar(200)`, nullable)<br>④ `SmtpPasswordEncrypted` (`nvarchar(500)`, nullable)<br>⑤ `SmtpEnableSsl` (`bit`, nullable, `defaultValue: 1`)<br>⑥ `BackupScheduleHour` (`int`, nullable, `defaultValue: 2`)<br>⑦ `BackupRetentionDays` (`int`, nullable, `defaultValue: 30`)<br>⑧ `BackupOutputFolder` (`nvarchar(500)`, nullable)<br>(المجموع: 8 أعمدة جديدة، SMTP + Backup Schedule، لا Natigh). |
| 6 | إنشاء | `FinalLabSystem/Migrations/20260701000000_AddLabSettingSmtpAndBackupConfig.Designer.cs` | Designer المعتاد. |
| 7 | تعديل | `FinalLabSystem/Migrations/FinalLabDbContextModelSnapshot.cs` | snapshot. |
| 8 | تعديل | `FinalLabSystem/Models/LabSetting.cs` | إضافة 8 خصائص navigation مطابقة للأعمدة. تبقى الخصائص القائمة (`EnforceStageGating`, `EnableServerPrinting`) بدون تعديل. |
| 9 | تعديل | `FinalLabSystem/Data/FinalLabDbContext.cs` | Fluent API mapping للـ 8 أعمدة (تحديد نوع، default، nullability). |
| 10 | إنشاء | `FinalLabSystem/Services/Implementations/BackupService.cs` | تنفيذ `IBackupService`. يَحقن `FinalLabDbContext, ICurrentUserSession, IAuditService, ILogger<BackupService>`. التدفق الرئيسي في `CreateBackupAsync`: (1) التحقق `IsAdmin` من `_currentUserSession.CurrentUser?.IsAdmin` — رمي `UnauthorizedAccessException` إن لم يكن Admin. (2) قراءة كل `DbSet` (~44 جدول) عبر `ModelExtensions.GetEntityTypes()` أو يدوياً في `Dictionary<string, object>`. (3) `JsonSerializer.Serialize` مع `ReferenceHandler.IgnoreCycles`. (4) `AesEncryptionHelper.Encrypt(jsonBytes, adminPassword)`. (5) كتابة إلى `Path.Combine(targetFolder, $"FinalLabSystem_{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.bak.enc")`. (6) `IAuditService.LogAsync("BackupCreated", staffId, DateTime.UtcNow)`. (7) إرجاع المسار. `RestoreBackupAsync`: (1) نفس التحقق من Admin. (2) Auto-backup قبل الاستعادة بِـ suffix `_pre_restore`. (3) قراءة + Decrypt + Deserialize. (4) `Database.BeginTransactionAsync()`. (5) مسح الجداول بترتيب يحترم FK، ثم إعادة الإدخال. (6) `SaveChangesAsync()` ثم `CommitAsync()`. (7) `LogAsync("BackupRestored", staffId, DateTime.UtcNow)`. (8) في حالة أي exception ⇒ `RollbackAsync()` ثم رمي. |
| 11 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل `IBackupService` → `BackupService` (Scoped). المبرر: يستخدم `FinalLabDbContext` المُسجَّل Scoped. |

### تسجيلات DI

| Lifetime | الخدمة | المبرر |
|----------|--------|--------|
| Scoped | `IBackupService` | يَستخدم `FinalLabDbContext` المُسجَّل Scoped. يَتَسق مع `IPatientService`, `IReceiptService`, `IInvoiceService` (كلها Scoped). |
| Scoped | `AesEncryptionHelper` | لا يحتاج تسجيل (static utility class). |

### محتوى Migration بالتفصيل

Migration class name: `AddLabSettingSmtpAndBackupConfig`

**Up method:**
```csharp
migrationBuilder.AddColumn<string>(name: "SmtpHost", table: "LabSetting", type: "nvarchar(200)", nullable: true);
migrationBuilder.AddColumn<int>(name: "SmtpPort", table: "LabSetting", type: "int", nullable: true, defaultValue: 587);
migrationBuilder.AddColumn<string>(name: "SmtpUsername", table: "LabSetting", type: "nvarchar(200)", nullable: true);
migrationBuilder.AddColumn<string>(name: "SmtpPasswordEncrypted", table: "LabSetting", type: "nvarchar(500)", nullable: true);
migrationBuilder.AddColumn<bool>(name: "SmtpEnableSsl", table: "LabSetting", type: "bit", nullable: true, defaultValue: true);
migrationBuilder.AddColumn<int>(name: "BackupScheduleHour", table: "LabSetting", type: "int", nullable: true, defaultValue: 2);
migrationBuilder.AddColumn<int>(name: "BackupRetentionDays", table: "LabSetting", type: "int", nullable: true, defaultValue: 30);
migrationBuilder.AddColumn<string>(name: "BackupOutputFolder", table: "LabSetting", type: "nvarchar(500)", nullable: true);
```

**Down method:** ثمانية `DropColumn` بترتيب عكسي.

### جدول الاختبارات الكامل (18 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Infrastructure/AesEncryptionHelperTests.cs` | `Encrypt_Decrypt_Roundtrip_RestoresOriginal` | roundtrip يعيد النص الأصلي | bytes: "test data unicode أبجد" |
| نفسه | `Encrypt_Twice_SamePassword_ProducesDifferentCiphertext` | salt + IV عشوائيان | استدعاءان متتاليان |
| نفسه | `Decrypt_WithWrongPassword_ThrowsCryptographicException` | فشل آمن | "right" vs "wrong" |
| نفسه | `Encrypt_EmptyBytes_DoesNotThrow` | حالة حدية | `new byte[0]` |
| نفسه | `DeriveKey_SamePasswordAndSalt_ProducesSameKey` | PBKDF2 deterministic | ثابتات |
| `Tests/Validation/LabSettingSmtpBackupMigrationTests.cs` (V-27) | `Migration_AddsAllEightColumns_NullableOrDefaulted` | schema check شامل | EF InMemory بعد تطبيق الـ migration |
| نفسه | `Migration_SmtpPort_DefaultsTo587` | default value | InMemory |
| نفسه | `Migration_BackupScheduleHour_DefaultsTo2` | default value | InMemory |
| نفسه | `Migration_BackupRetentionDays_DefaultsTo30` | default value | InMemory |
| `Tests/Services/BackupServiceTests.cs` | `CreateBackupAsync_NonAdmin_ThrowsUnauthorized` | BR-061 enforcement | `Staff{IsAdmin=false, Sex="M"}` |
| نفسه | `CreateBackupAsync_AdminUser_WritesFile_ToTargetFolder` | الحالة السعيدة | `Staff{IsAdmin=true, Sex="M"}`, RegisteredAt=DateTime.UtcNow |
| نفسه | `CreateBackupAsync_AdminUser_LogsAuditEvent_WithUtcNow` | `IAuditService.LogAsync` يُستدعى | Mock IAuditService receives `It.IsAny<string>()` |
| نفسه | `CreateBackupAsync_FileName_FollowsTimestampPattern` | regex على اسم الملف | Admin session |
| نفسه | `CreateBackupAsync_EncryptedFile_DoesNotContain_PlaintextMarker` | الملف لا يَحتوي JSON خام | 2 visits + 5 patients |
| نفسه | `RestoreBackupAsync_CreatesPreRestoreBackup_First` | يُرى ملف `_pre_restore` قبل الـ restore | Admin + backup file جاهز |
| نفسه | `RestoreBackupAsync_WrongPassword_ReturnsFalse` | فشل آمن | كلمة مرور خاطئة |
| نفسه | `ListBackupsAsync_EmptyFolder_ReturnsEmpty` | حالة حدية | folder فارغ |
| نفسه | `ValidateBackupFileAsync_ValidFile_ReturnsTrue` | sanity | backup file صحيح |

### Validation Gate G6.2

**العدد التراكمي المتوقع: 590 اختبار ناجح**

نقاط تحقق إضافية:
- `dotnet ef migrations script` يَطبَّق بدون أخطاء.
- `grep -rn "Natigh" FinalLabSystem/Migrations/20260701000000_` يُرجع صفر نتائج (التأكيد على استبعاد Natigh).
- `grep -n "SmtpHost\|SmtpPort\|SmtpUsername\|SmtpPasswordEncrypted\|SmtpEnableSsl\|BackupScheduleHour\|BackupRetentionDays\|BackupOutputFolder" FinalLabSystem/Models/LabSetting.cs` يُرجع 8 نتائج.
- AES key لا يُسجَّل في log (`_logger.LogInformation` لا يَستقبل كلمة المرور أو الـ key).

---

## شريحة 6.3 — Backup UI & Restore Workflow

**الهدف:** بناء `BackupRestoreWindow` كاملة مع workflow الـ Admin Password confirmation (BR-061) وتفعيل `BackupMenuViewModel` الذي هو حالياً placeholder.

**Pre-condition:** نجاح G6.2.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem/ViewModels/Settings/BackupRowViewModel.cs` | wrapper لـ `BackupMetadataDto`. خصائص: `FileName`, `FilePath`, `DisplaySize` (يُنسَّق human-readable: `FormatBytes(FileSizeBytes)`)، `DisplayCreatedAt` (UTC → Local). تَستخدم `INotifyPropertyChanged` فقط على `IsSelected` لو احتاج لاحقاً. |
| 2 | إنشاء | `FinalLabSystem/ViewModels/Settings/BackupRestoreWindowViewModel.cs` | VM رئيسي. يَحقن `IBackupService, IDialogService, ICurrentUserSession`. الخصائص: `ObservableCollection<BackupRowViewModel> Backups`، `string TargetFolder` (افتراضي `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FinalLabBackups")`)، `DateTime? LastBackupAt` (read-only)، `bool IsBusy`. الأوامر: `LoadBackupsCommand`، `CreateBackupCommand` (يَفتح `BackupPasswordDialog` ثم يَستدعي `_backupService.CreateBackupAsync`)، `RestoreCommand` (selection-bound ⇒ تحذير ⇒ password dialog ⇒ `_backupService.RestoreBackupAsync` ⇒ prompt لـ shutdown)، `BrowseFolderCommand` (يَستدعي `IDialogService.OpenFolderDialog`)، `OpenFolderCommand` (يَستدعي `Process.Start("explorer.exe", TargetFolder)`). |
| 3 | إنشاء | `FinalLabSystem/ViewModels/Settings/BackupPasswordDialogViewModel.cs` | VM للـ dialog. خصائص: `SecureString Password` (عبر `PasswordBox` binding helper الموجود)، `string ConfirmPassword`. الأمر: `ConfirmCommand` يَتحقق `Password.Length >= 8` و `Password == ConfirmPassword` ثم `RequestClose(true)`. خاصية: `Action<bool?>? RequestClose`. |
| 4 | إنشاء | `FinalLabSystem/Views/Settings/BackupRestoreWindow.xaml` + `.cs` | نافذة DataGrid + أزرار. الـ code-behind يحوي فقط `InitializeComponent()` و `public BackupRestoreWindow()`. |
| 5 | إنشاء | `FinalLabSystem/Views/Settings/BackupPasswordDialog.xaml` + `.cs` | نافذة مع `PasswordBox` و `ConfirmPasswordBox` و زرَّي "تأكيد" و "إلغاء". الـ code-behind يحوي فقط `InitializeComponent()` و `public BackupPasswordDialog()`. |
| 6 | تعديل | `FinalLabSystem/ViewModels/Menu/BackupMenuViewModel.cs` | استبدال كامل: من `PlaceholderCommand` إلى `NavigateCommand` يستدعي `INavigationService.OpenTaskWindow<BackupRestoreWindowViewModel>()` (نمط `ReportSettingsMenuViewModel` الحالي). |
| 7 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل DI: `BackupRestoreWindowViewModel` (Transient)، `BackupRestoreWindow` (Transient)، `BackupPasswordDialogViewModel` (Transient)، `BackupPasswordDialog` (Transient). إضافة `navigation.RegisterWindow<BackupRestoreWindowViewModel, BackupRestoreWindow>();`. |

### تسجيلات DI

| Lifetime | العنصر | المبرر |
|----------|--------|--------|
| Transient | `BackupRestoreWindowViewModel` | كل نافذة بـ state مستقل (الـ Backups collection). |
| Transient | `BackupRestoreWindow` | نمط Windows المعتمد. |
| Transient | `BackupPasswordDialogViewModel` | dialog بسيط، transient. |
| Transient | `BackupPasswordDialog` | نمط dialogs. |

### جدول الاختبارات الكامل (13 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/ViewModels/Settings/BackupRowViewModelTests.cs` | `WrapsDto_ExposesAllFields` | sanity | DTO{CreatedAt=DateTime.UtcNow, FileSizeBytes=1024*1024} |
| نفسه | `DisplaySize_FormatsBytes_HumanReadable` | "1 MB" | FileSizeBytes=1024*1024 |
| نفسه | `DisplayCreatedAt_ConvertsUTC_ToLocal` | التحويل | DateTime.UtcNow |
| `Tests/ViewModels/Settings/BackupRestoreWindowViewModelTests.cs` | `LoadBackupsCommand_PopulatesCollection_FromService` | 3 backups من Mock | Mock IBackupService |
| نفسه | `CreateBackupCommand_OpensPasswordDialog_BeforeServiceCall` | الـ guard قبل `_backupService` | Mock IDialogService |
| نفسه | `CreateBackupCommand_NonAdmin_ShowsError_DoesNotCallService` | BR-061 على مستوى VM | Staff{IsAdmin=false, Sex="M"} |
| نفسه | `CreateBackupCommand_OnSuccess_RefreshesBackupList` | بعد الإنشاء يَستدعي LoadBackups | Mock |
| نفسه | `RestoreCommand_RequiresConfirmation_BeforeExecution` | تأكيد ⇒ استدعاء | Mock returns false ⇒ لا service call |
| نفسه | `RestoreCommand_OnSuccess_TriggersShutdownPrompt` | UX flow | Mock service returns true |
| نفسه | `IsBusy_TrueDuringOperation_FalseAfter` | UI state | — |
| نفسه | `BrowseFolderCommand_OpensFolderDialog_AndUpdatesTargetFolder` | IDialogService.OpenFolderDialog | Mock |
| `Tests/ViewModels/Menu/BackupMenuViewModelTests.cs` | `NavigateCommand_CallsNavigationService_OpenTaskWindow` | استبدال placeholder | Mock INavigationService |
| `Tests/Services/BackupRestoreWindowRegistrationTests.cs` | `DI_Resolves_BackupRestoreWindowViewModel_And_Window` | resolution + navigation mapping | TestServiceProvider |

### Validation Gate G6.3

**العدد التراكمي المتوقع: 603 اختبار ناجح**

نقاط تحقق إضافية:
- `grep -n "PlaceholderCommand" FinalLabSystem/ViewModels/Menu/BackupMenuViewModel.cs` يُرجع صفر نتائج.
- `BackupRestoreWindow.xaml.cs` يحوي فقط `InitializeComponent()` و constructor.

---

## شريحة 6.4 — Report Settings UI (PRD Task 6.4)

**الهدف:** تخصيص شعار/ألوان/خطوط/margins للتقارير عبر `ReportSettingsWindow` تستهلكها `DocumentTemplateBase` و `WpfFlowDocumentPrintService`.

**Pre-condition:** نجاح G6.3.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem/Infrastructure/Constants/ReportSettingKeys.cs` | static class بـ 12 ثابتاً للمفاتيح: `LabNameAr`, `LabNameEn`, `LogoPath`, `PrimaryColor`, `SecondaryColor`, `FontFamily`, `FontSize`, `MarginCm`, `ShowHeader`, `ShowFooter`, `ShowStamp`, `HeaderText`. كل ثابت من نوع `const string`. |
| 2 | إنشاء | `FinalLabSystem/Models/DTOs/ReportLayoutDto.cs` | POCO بكل الـ 12 حقلاً مع `string` للألوان (hex) و `double FontSize` و `double MarginCm` و `bool ShowHeader/ShowFooter/ShowStamp`. |
| 3 | إنشاء | `FinalLabSystem/Services/Interfaces/IReportLayoutService.cs` | 4 methods: `Task<ReportLayoutDto> GetCurrentLayoutAsync()`, `Task SaveLayoutAsync(ReportLayoutDto layout, int staffId)`, `Task ResetToDefaultsAsync()`, `ReportLayoutDto GetDefaults()`. |
| 4 | إنشاء | `FinalLabSystem/Services/Implementations/ReportLayoutService.cs` | يَحقن `FinalLabDbContext, ICurrentUserSession`. الـ implementation: يَلفّ `LabSetting` كـ key-value store. `GetCurrentLayoutAsync` يَقرأ كل مفتاح من `LabSetting` ويُنشئ `ReportLayoutDto`، fallback إلى `GetDefaults()` للمفاتيح الناقصة. `SaveLayoutAsync` يَكتب كل حقل كمفتاح منفصل ثم `LogAsync("ReportSettingsUpdated", staffId, DateTime.UtcNow)`. |
| 5 | إنشاء | `FinalLabSystem/ViewModels/Settings/ReportSettingsWindowViewModel.cs` | VM مع `ReportLayoutDto CurrentLayout`، `bool IsBusy`. الأوامر: `LoadCommand` (AsyncRelayCommand)، `SaveCommand` (AsyncRelayCommand يَستدعي `_reportLayoutService.SaveLayoutAsync(CurrentLayout, staffId)`)، `ResetToDefaultsCommand`، `BrowseLogoCommand` (يَستدعي `IDialogService.OpenFileDialog` filter PNG/JPG)، `PreviewCommand` (يَنشئ `FlowDocument` بسيط مع الإعدادات الحالية). |
| 6 | إنشاء | `FinalLabSystem/Views/Settings/ReportSettingsWindow.xaml` + `.cs` | نافذة form + preview pane (DataTemplate لـ FlowDocument). |
| 7 | تعديل | `FinalLabSystem/Services/Printing/DocumentTemplateBase.cs` | إضافة optional method `protected virtual void ApplyLayout(ReportLayoutDto? layout)` يَكون no-op افتراضياً (backward compatible — كل الـ templates الحالية تستمر في العمل). |
| 8 | تعديل | `FinalLabSystem/Services/Printing/ReceiptTemplate.cs` | override `ApplyLayout`: لو `layout != null` ⇒ تطبيق `LogoPath`، `PrimaryColor`، `FontFamily/FontSize` على عناصر `FlowDocument`. |
| 9 | تعديل | `FinalLabSystem/Services/Printing/ResultReportTemplate.cs` | نفس override. |
| 10 | تعديل | `FinalLabSystem/Services/Printing/WpfFlowDocumentPrintService.cs` | إضافة overload `Task PrintAsync(string documentType, object data, ReportLayoutDto? layout)` يَستدعي `template.ApplyLayout(layout)` قبل `BuildDocument`. |
| 11 | تعديل | `FinalLabSystem/ViewModels/Menu/ReportSettingsMenuViewModel.cs` | إضافة `NavigateToReportSettingsCommand` (يَستخدم `INavigationService.OpenTaskWindow<ReportSettingsWindowViewModel>`). الإبقاء على `ManageTemplatesCommand` القائم. |
| 12 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل: `IReportLayoutService` → `ReportLayoutService` (Scoped)، `ReportSettingsWindowViewModel` (Transient)، `ReportSettingsWindow` (Transient). إضافة `navigation.RegisterWindow<ReportSettingsWindowViewModel, ReportSettingsWindow>();`. |

### تسجيلات DI

| Lifetime | العنصر | المبرر |
|----------|--------|--------|
| Scoped | `IReportLayoutService` | يَستخدم `FinalLabDbContext`. |
| Transient | `ReportSettingsWindowViewModel` | state مستقل. |
| Transient | `ReportSettingsWindow` | نمط Windows. |

### جدول الاختبارات الكامل (12 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Services/ReportLayoutServiceTests.cs` | `GetCurrentLayoutAsync_NoSettings_ReturnsDefaults` | resilience | InMemory LabSetting فارغة |
| نفسه | `SaveLayoutAsync_PersistsAllTwelveKeys` | تكامل | DTO مع 12 حقلاً |
| نفسه | `GetCurrentLayoutAsync_PartialSettings_MergesWithDefaults` | mixed | 3 مفاتيح فقط |
| نفسه | `ResetToDefaultsAsync_RemovesCustomKeys` | sanity | settings مخصَّصة |
| نفسه | `GetDefaults_ReturnsValidDto` | sanity check | — |
| `Tests/ViewModels/Settings/ReportSettingsWindowViewModelTests.cs` | `LoadCommand_PopulatesAllFields` | الحالة السعيدة | mock layout |
| نفسه | `SaveCommand_CallsService_WithCurrentSettings` | `_reportLayoutService.SaveLayoutAsync(It.IsAny<>(), It.IsAny<int>())` | mock |
| نفسه | `ResetCommand_RestoresDefaults` | UX | mock |
| نفسه | `BrowseLogoCommand_OpensOpenFileDialog_AndUpdatesLogoPath` | UX | Mock IDialogService |
| `Tests/Services/Printing/DocumentTemplateBaseLayoutTests.cs` | `ReceiptTemplate_ApplyLayout_OverridesDefaults` | sanity | DTO custom |
| نفسه | `NullLayout_UsesBuiltInDefaults` | backward compat | layout = null |
| `Tests/ViewModels/Menu/ReportSettingsMenuViewModelTests.cs` | `NavigateToReportSettingsCommand_CallsNavigationService_OpenTaskWindow` | navigation + regression على ManageTemplatesCommand | Mock INavigationService |

### Validation Gate G6.4

**العدد التراكمي المتوقع: 615 اختبار ناجح**

نقاط تحقق إضافية:
- `DocumentTemplateBase.ApplyLayout` لا يُغيِّر سلوك أي template قائمة لو لم يُمرَّر layout.
- `ReportSettingsMenuViewModel` يحوي `NavigateToReportSettingsCommand` و `ManageTemplatesCommand` كلاهما يَعمل.

---

## شريحة 6.5 — Print Queue / Batch Printing (PRD Task 6.6)

**الهدف:** طباعة متعدِّدة في batch باستخدام `IPrintService` الموسَّع من 6.1 و `IPrintPreviewDialogService`.

**Pre-condition:** نجاح G6.4.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem/Models/Enums/PrintQueueItemStatus.cs` | enum: `Pending, Printing, Done, Failed`. |
| 2 | إنشاء | `FinalLabSystem/Models/DTOs/PrintQueueItemDto.cs` | POCO: `int VisitId`, `string PatientName`, `string DocumentType` (Receipt/Report/Invoice)، `PrintQueueItemStatus Status`، `DateTime AddedAt`, `string? Error`. |
| 3 | إنشاء | `FinalLabSystem/Services/Interfaces/IPrintQueueService.cs` | methods: `void Enqueue(PrintQueueItemDto item)`, `void Remove(PrintQueueItemDto item)`, `void Clear()`, `List<PrintQueueItemDto> GetItems()`, `Task PrintAllAsync(IProgress<double>? progress, CancellationToken cancellationToken)`. |
| 4 | إنشاء | `FinalLabSystem/Services/Implementations/PrintQueueService.cs` | Singleton (queue per-session، لا `DbContext`). يَحقن `IPrintService`. التدفق: يَتعقَّب items في `List<PrintQueueItemDto> _items` thread-safe عبر `lock`. `PrintAllAsync` يَمُر على كل item، يَستدعي `_printService.PrintAsync(item.DocumentType, visitData)` (الذي يَبني template ويُطبِع مباشرة)، يُحدِّث Status، يُبلِّغ Progress، يَحتَرم CancellationToken. عند فشل item واحد ⇒ Status=Failed ويُكمل الباقي. |
| 5 | إنشاء | `FinalLabSystem/ViewModels/Settings/PrintQueueWindowViewModel.cs` | VM مع `ObservableCollection<PrintQueueItemDto> Items`، `double Progress`، `bool IsRunning`. الأوامر: `LoadCommand`، `PrintAllCommand`، `CancelCommand` (cancellation token source داخلي)، `RemoveSelectedCommand`. |
| 6 | إنشاء | `FinalLabSystem/Views/Settings/PrintQueueWindow.xaml` + `.cs` | DataGrid + ProgressBar + أزرار. |
| 7 | تعديل | `FinalLabSystem/ViewModels/Patients/TestResultsViewModel.cs` | إضافة `private readonly IPrintQueueService _printQueueService;` (حقن في constructor). إضافة `AddToPrintQueueCommand` (يَنشئ DTO من الـ visit الحالي ويُضيف) و `OpenPrintQueueCommand` (يَستدعي `INavigationService.OpenTaskWindow<PrintQueueWindowViewModel>`). |
| 8 | تعديل | `FinalLabSystem/Views/Patients/TestResultsWindow.xaml` | إضافة زرَّي "إضافة إلى قائمة الطباعة" و "فتح قائمة الطباعة". ربط shortcuts اختيارية Ctrl+Q / Ctrl+Shift+Q (متَّسق مع Phase 2). |
| 9 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل: `IPrintQueueService` → `PrintQueueService` (Singleton)، `PrintQueueWindowViewModel` (Transient)، `PrintQueueWindow` (Transient)، `navigation.RegisterWindow<PrintQueueWindowViewModel, PrintQueueWindow>();`. |

### تسجيلات DI

| Lifetime | العنصر | المبرر |
|----------|--------|--------|
| Singleton | `IPrintQueueService` | يحتفظ بـ queue state في الذاكرة (per-session). لا يستخدم `DbContext` مباشرة. يَتَسق مع `IPrintQueueService` كنمط cache/in-memory state. |
| Transient | `PrintQueueWindowViewModel` | instance مستقل لكل نافذة. |
| Transient | `PrintQueueWindow` | نمط Windows. |

### جدول الاختبارات الكامل (11 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Services/PrintQueueServiceTests.cs` | `Enqueue_AddsItem_WithDateTimeUtcNow` | `AddedAt = DateTime.UtcNow` | DTO{VisitId=1} |
| نفسه | `Remove_RemovesSpecificItem` | sanity | 3 items |
| نفسه | `Clear_EmptiesQueue` | sanity | 3 items |
| نفسه | `PrintAllAsync_CallsPrintService_ForEachItem` | `Mock<IPrintService>.Verify(s => s.PrintAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(3))` | 3 items |
| نفسه | `PrintAllAsync_ReportsProgress_AfterEachItem` | `IProgress<double>.Report` يُستدعى 3x | Mock IProgress |
| نفسه | `PrintAllAsync_OneItemFails_ContinuesOthers_MarksFailed` | resilience | 3 items, الثاني يَرمي `It.IsAny<Exception>()` |
| نفسه | `PrintAllAsync_RespectsCancellationToken` | cancellation | `CancellationTokenSource` |
| `Tests/ViewModels/Settings/PrintQueueWindowViewModelTests.cs` | `LoadCommand_PopulatesItems_FromService` | sanity | 5 items |
| نفسه | `PrintAllCommand_TogglesIsRunning_DuringExecution` | UI state | — |
| نفسه | `CancelCommand_TriggersCancellationTokenSource_Cancel` | cancellation | Mock CTS |
| `Tests/Services/PrintQueueServiceRegistrationTests.cs` | `DI_Resolves_IPrintQueueService_AsSingleton` | lifetime | TestServiceProvider |

### Validation Gate G6.5

**العدد التراكمي المتوقع: 626 اختبار ناجح**

نقاط تحقق إضافية:
- `IPrintQueueService` المُسجَّل Singleton لا يَخلط state بين اختبارات (اختبار isolation عبر `PrintQueueService.Clear()` في setup).

---

## شريحة 6.6 — Delivery Confirmation (Signature + OTP) (PRD Task 6.5)

**الهدف:** تأكيد التسليم عبر signature pad أو OTP مع Migration جديدة على `Visit`.

**Pre-condition:** نجاح G6.5.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem/Models/Enums/DeliveryConfirmationMethod.cs` | enum: `Signature, OTP`. |
| 2 | إنشاء | `FinalLabSystem/Models/DeliveryConfirmation.cs` | entity كامل: `int DeliveryConfirmationId`, `int VisitId` (FK), `DeliveryConfirmationMethod Method`, `DateTime ConfirmedAt`, `byte[]? SignatureImage` (varbinary(MAX) في DB)، `string? OtpCodeHash`, `string ReceivedByName`, `int StaffId` (FK). attribute `[Auditable]` على الـ class. |
| 3 | إنشاء | `FinalLabSystem/Migrations/20260702000000_AddDeliveryConfirmationFields.cs` + `.Designer.cs` | إضافة 3 أعمدة إلى جدول `Visit`:<br>① `DeliveryConfirmedAt` (`datetime2`, nullable)<br>② `DeliverySignature` (`varbinary(max)`, nullable)<br>③ `DeliveryOtpCode` (`nvarchar(50)`, nullable)<br>+ إنشاء جدول جديد `DeliveryConfirmation` بكل أعمدته والـ FKs. |
| 4 | تعديل | `FinalLabSystem/Migrations/FinalLabDbContextModelSnapshot.cs` | snapshot. |
| 5 | تعديل | `FinalLabSystem/Models/Visit.cs` | إضافة 3 خصائص: `DateTime? DeliveryConfirmedAt`, `byte[]? DeliverySignature`, `string? DeliveryOtpCode`. navigation `ICollection<DeliveryConfirmation> DeliveryConfirmations`. **لا يُضاف أي حقل يخص Natigh** (مُستبعَد نهائياً). |
| 6 | تعديل | `FinalLabSystem/Data/FinalLabDbContext.cs` | إضافة `DbSet<DeliveryConfirmation> DeliveryConfirmations` + Fluent mapping + relationship. |
| 7 | إنشاء | `FinalLabSystem/Infrastructure/Security/OtpGenerator.cs` | static class. methods: `string Generate(int digits = 6)` (RandomNumberGenerator.GetInt32 + ToString("D{6}"))، `string Hash(string otp)` (PBKDF2 مع salt ثابت في الـ env أو يَتغيَّر)، `bool Verify(string otp, string hash)`. |
| 8 | إنشاء | `FinalLabSystem/Services/Interfaces/IDeliveryConfirmationService.cs` | methods: `Task SaveSignatureAsync(int visitId, byte[] signatureImage, string receivedByName, int staffId)`، `Task<string> GenerateOtpAsync(int visitId, int staffId)`، `Task<bool> VerifyOtpAsync(int visitId, string otp, int staffId)`، `Task<bool> IsDeliveredAsync(int visitId)`. |
| 9 | إنشاء | `FinalLabSystem/Services/Implementations/DeliveryConfirmationService.cs` | يحقن `FinalLabDbContext, IAuditService, ILogger`. `SaveSignatureAsync`: ينشئ `DeliveryConfirmation{Method=Signature, ConfirmedAt=DateTime.UtcNow, SignatureImage=signatureImage, ...}` + يُحدِّث `Visit.DeliveryConfirmedAt = DateTime.UtcNow` + `AuditService.LogAsync("DeliverySignatureConfirmed", staffId, DateTime.UtcNow)`. `GenerateOtpAsync`: يَنشئ OTP عشوائي، يحفظ hash في `Visit.DeliveryOtpCode`، يَرجع plain للـ caller. `VerifyOtpAsync`: يَتحقق hash، لو صحيح ⇒ يَنشئ `DeliveryConfirmation{Method=OTP, ConfirmedAt=DateTime.UtcNow, OtpCodeHash=hash, ...}` + يُحدِّث `Visit.DeliveryConfirmedAt = DateTime.UtcNow` + Audit. |
| 10 | إنشاء | `FinalLabSystem/ViewModels/Patients/Delivery/SignatureConfirmationDialogViewModel.cs` | VM. الخصائص: `byte[]? CapturedSignature`، `string ReceivedByName`. الأمر: `ClearCommand`، `ConfirmCommand` (يَستدعي `_deliveryConfirmationService.SaveSignatureAsync(...)` ثم `RequestClose(true)`). |
| 11 | إنشاء | `FinalLabSystem/Views/Patients/Delivery/SignatureConfirmationDialog.xaml` + `.cs` | نافذة مع `InkCanvas` للتوقيع (StrokeCollection → SaveAs XPS → byte[]). |
| 12 | إنشاء | `FinalLabSystem/ViewModels/Patients/Delivery/OtpVerificationDialogViewModel.cs` | VM. الخصائص: `string EnteredOtp`. الأمر: `VerifyCommand` (يَستدعي `_deliveryConfirmationService.VerifyOtpAsync(...)`)، `ResendOtpCommand`. |
| 13 | إنشاء | `FinalLabSystem/Views/Patients/Delivery/OtpVerificationDialog.xaml` + `.cs` | نافذة مع TextBox للأرقام (InputScope=Number) + زرَّي تحقق و إعادة إرسال. |
| 14 | تعديل | `FinalLabSystem/ViewModels/Patients/Delivery/DeliveryViewModel.cs` | إضافة `private readonly IDeliveryConfirmationService _deliveryConfirmationService;` (حقن في constructor). إضافة `ConfirmWithSignatureCommand` (يَفتح `SignatureConfirmationDialog` عبر `IDialogService.ShowDialog`) و `ConfirmWithOtpCommand` (يَستدعي `GenerateOtpAsync` ثم يَفتح `OtpVerificationDialog`). |
| 15 | تعديل | `FinalLabSystem/Views/Patients/DeliveryWindow.xaml` | إضافة زرَّين: "تأكيد بالتوقيع" و "تأكيد بـ OTP". |
| 16 | تعديل | `FinalLabSystem/App.xaml.cs` | تسجيل: `IDeliveryConfirmationService` → `DeliveryConfirmationService` (Scoped)، `SignatureConfirmationDialogViewModel` (Transient)، `SignatureConfirmationDialog` (Transient)، `OtpVerificationDialogViewModel` (Transient)، `OtpVerificationDialog` (Transient). |

### تسجيلات DI

| Lifetime | العنصر | المبرر |
|----------|--------|--------|
| Scoped | `IDeliveryConfirmationService` | يَستخدم `FinalLabDbContext`. |
| Transient | `SignatureConfirmationDialogViewModel` + `SignatureConfirmationDialog` | نمط dialogs. |
| Transient | `OtpVerificationDialogViewModel` + `OtpVerificationDialog` | نمط dialogs. |

### جدول الاختبارات الكامل (11 اختبار)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Validation/DeliveryConfirmationMigrationTests.cs` | `Migration_CreatesDeliveryConfirmationTable_With_AllColumns` | schema | EF InMemory |
| نفسه | `Migration_AddsDeliveryConfirmedAt_DeliverySignature_DeliveryOtpCode_ToVisit` | schema check | InMemory |
| نفسه | `Migration_No_NatighRelated_Columns_Added_To_Visit` | تأكيد استبعاد Natigh | grep |
| `Tests/Infrastructure/OtpGeneratorTests.cs` | `Generate_Returns6Digits_ByDefault` | format regex | — |
| نفسه | `Hash_DifferentFromPlaintext` | حماية | "123456" |
| نفسه | `Verify_CorrectOtp_ReturnsTrue` | round-trip | "123456" |
| `Tests/Services/DeliveryConfirmationServiceTests.cs` | `SaveSignatureAsync_PersistsRecord_WithConfirmedAtUtcNow` | `ConfirmedAt = DateTime.UtcNow` | Visit + Staff{IsAdmin=true, Sex="M"} |
| نفسه | `SaveSignatureAsync_LogsAuditEvent_WithUtcNow` | `AuditService.LogAsync(It.IsAny<string>(), It.IsAny<int>(), DateTime.UtcNow)` | Mock IAuditService |
| نفسه | `GenerateOtpAsync_PersistsHash_NotPlaintext` | حماية | Visit |
| نفسه | `GenerateOtpAsync_ReturnsPlainOtp_ToCaller` | الـ caller يَحتاج plain للعرض | Visit |
| نفسه | `VerifyOtpAsync_CorrectOtp_MarksVisitAsDelivered_WithUtcNow` | `Visit.DeliveryConfirmedAt = DateTime.UtcNow` | Visit + OTP generated |

### Validation Gate G6.6

**العدد التراكمي المتوقع: 637 اختبار ناجح**

نقاط تحقق إضافية:
- `grep -n "Natigh" FinalLabSystem/Migrations/20260702000000_` يُرجع صفر نتائج (تأكيد استبعاد).
- `Visit.cs` يحوي `DeliveryConfirmedAt`, `DeliverySignature`, `DeliveryOtpCode` فقط (لا `NatighConsent` ولا أي حقل Natigh).
- `IDeliveryConfirmationService` لا يَستدعي أي HTTP Client (النظام Offline بالكامل).

---

## شريحة 6.7 — Integration Tests + IMPLEMENTATION_STATUS Update

**الهدف:** اختبارات E2E شاملة لـ Phase 6 + تحديث IMPLEMENTATION_STATUS.md بقسم Phase 6 كامل.

**Pre-condition:** نجاح G6.6.

### الملفات

| # | الإجراء | الملف | المحتوى المطلوب بالتفصيل |
|---|---------|-------|--------------------------|
| 1 | إنشاء | `FinalLabSystem.Tests/Integration/Phase6BackupRestoreE2ETests.cs` | E2E: seed 5 patients (Sex="M" أو "F") + 3 visits + 10 test results، RegisteredAt=DateTime.UtcNow. backup، wipe، restore، verify. |
| 2 | إنشاء | `FinalLabSystem.Tests/Integration/Phase6DeliveryConfirmationE2ETests.cs` | E2E: visit ready → generate OTP → verify → assert `Visit.DeliveryConfirmedAt` و `DeliveryConfirmation` record. |
| 3 | إنشاء | `FinalLabSystem.Tests/Integration/Phase6PrintPipelineE2ETests.cs` | E2E: ReceiptDialog → PrintPreviewVM → IPrintService.PrintFlowDocumentAsync. تأكيد عدم وجود `new PrintPreviewWindow` في الـ VM. |
| 4 | إنشاء | `FinalLabSystem.Tests/Integration/Phase6BuildVerificationTests.cs` | DI sanity: كل خدمات/VMs/Windows من Phase 6 تَحلّ من `TestServiceProvider`. |
| 5 | إنشاء | `FinalLabSystem.Tests/Integration/Phase6MigrationVerificationTests.cs` | EF InMemory rebuild: كل الـ migrations الجديدة (LabSetting SMTP/Backup، Delivery Confirmation) تَطبَّق بنجاح. |
| 6 | تعديل | `FinalLabSystem/Docs/PRDs/IMPLEMENTATION_STATUS.md` | إضافة قسم Phase 6 كامل بنفس بنية Phases 1–5. يتضمن: Status = ✅ مكتملة، قائمة الـ slices، عدد الملفات الجديدة والمعدَّلة، عدد الاختبارات (637)، قائمة Migrations الجديدة (`AddLabSettingSmtpAndBackupConfig`, `AddDeliveryConfirmationFields`)، Build status، BR Compliance Matrix. **لا يَذكر Natigh ضمن المميزات المُنجَزة** — بدلاً من ذلك يَذكر بوضوح: "ميزة Natigh.com Integration: مُستبعَدة نهائياً بقرار المشروع من نطاق Phase 6 — النظام Offline بالكامل ولا يحتاج تكامل شبكي." |

### جدول الاختبارات الكامل (5 اختبارات)

| ملف الاختبار | الاختبار | يختبر بالضبط | Seed Data |
|--------------|----------|---------------|-----------|
| `Tests/Integration/Phase6BackupRestoreE2ETests.cs` | `FullBackupRestoreCycle_PreservesData` | E2E: 5 patients (Sex="M"/"F"), 3 visits, 10 results | DateTime.UtcNow |
| `Tests/Integration/Phase6DeliveryConfirmationE2ETests.cs` | `OtpFlow_FromGenerationToConfirmation_End_To_End` | E2E | Visit + Staff{IsAdmin=true, Sex="M"} |
| `Tests/Integration/Phase6PrintPipelineE2ETests.cs` | `ReceiptDialog_UsesNewPrintPreviewMVVM_No_DirectWindowConstruction` | E2E: `grep "new PrintPreviewWindow"` يُرجع صفر في `ViewModels/` | Visit |
| `Tests/Integration/Phase6BuildVerificationTests.cs` | `AllPhase6Services_Resolve_FromDI` | DI sanity | — |
| `Tests/Integration/Phase6MigrationVerificationTests.cs` | `AllPhase6Migrations_Apply_Cleanly_To_InMemoryDatabase` | EF InMemory rebuild | — |

### Validation Gate G6.7 (النهائي)

**العدد التراكمي المتوقع: 642 اختبار ناجح**

نقاط تحقق إضافية:
- `dotnet test` بنجاح 642/642.
- `dotnet build` بدون warnings.
- `grep -rn "Natigh" FinalLabSystem/ FinalLabSystem.Tests/ FinalLabSystem/Docs/PRDs/IMPLEMENTATION_STATUS.md` يُرجع صفر نتائج **باستثناء** السطر الصريح في IMPLEMENTATION_STATUS.md الذي يَذكر الاستبعاد النهائي بقرار المشروع. أي ظهور آخر يُعتبر regression.
- `IMPLEMENTATION_STATUS.md` محدَّث بـ Phase 6 ✅ كامل.

---

## جدول الملخص النهائي

| # | الشريحة | ملفات جديدة | ملفات معدَّلة | Migrations | اختبارات جديدة | إجمالي تراكمي |
|---|---------|:-----------:|:-------------:|:----------:|:--------------:|:--------------:|
| 6.0 | Foundation Cleanup | 6 | 3 | 0 | +14 | 558 |
| 6.1 | PrintPreview MVVM Refactor (+ توسيع IPrintService V-28) | 5 | 5 | 0 | +14 | 572 |
| 6.2 | Backup Foundation + AES + Unified LabSetting Migration (V-27) | 9 | 4 | 1 | +18 | 590 |
| 6.3 | Backup UI & Restore Workflow | 7 | 2 | 0 | +13 | 603 |
| 6.4 | Report Settings UI | 8 | 4 | 0 | +12 | 615 |
| 6.5 | Print Queue / Batch | 6 | 3 | 0 | +11 | 626 |
| 6.6 | Delivery Confirmation (Signature + OTP) | 13 | 4 | 1 | +11 | 637 |
| 6.7 | Integration Tests + IMPLEMENTATION_STATUS Update | 5 | 1 | 0 | +5 | 642 |
| **الإجمالي** | **8 شرائح** | **59 ملف جديد** | **26 ملف معدَّل** | **2 migrations** | **+98 اختبار** | **544 → 642 (+18.0%)** |

---

## ملاحظات تنفيذية حاسمة

### 1. اصطلاحات DI Lifetime (متَّسقة مع Phases 1–5)

| Lifetime | الاستخدام | أمثلة Phase 6 |
|----------|-----------|---------------|
| **Singleton** | Dialog Services / Factories / Navigation / Cache in-memory / Stateless utilities | `IBarcodeDialogFactory`, `IReceiptDialogFactory`, `INormalRangesWindowFactory`, `IPrintPreviewDialogService`, `IPrintQueueService` |
| **Scoped** | Services التي تستخدم `FinalLabDbContext` | `IBackupService`, `IReportLayoutService`, `IDeliveryConfirmationService` |
| **Transient** | ViewModels / Windows / Dialogs | كل الـ ViewModels و Windows المُنشأة في Phase 6 |

**القاعدة الذهبية:** Singleton يَلجأ لـ `IServiceProvider.CreateScope()` داخلياً عند الحاجة لكائن Transient — لا يَحقن Transient مباشرة في Singleton.

### 2. المعايير الفنية الإلزامية في كل اختبار جديد

- **`DateTime.UtcNow`** في كل seed data و assertion على timestamps (لا `DateTime.Now` ولا `DateTime.Today`).
- **`It.IsAny<>()`** في كل Moq setup لكل المعاملات غير الحرجة، و معاملات محدَّدة (`It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc)`) عند اختبار timestamp.
- **`Sex = "M"`** في كل Patient/Staff seed افتراضي (مع تنويع `"F"` في اختبارات التغطية فقط) — `Patient.Sex` هو `string` بطول 1 char.
- **`[Auditable]`** على أي entity جديد يحتاج audit trail (`DeliveryConfirmation`).
- **`xUnit + Moq + EF Core InMemory`** — لا `SqlConnection` حقيقي في الاختبارات.
- **DI registration tests** لكل service جديد (T6.X.Y يَختبر أن الـ lifetime صحيح عبر `TestServiceProvider`).
- **MVVM purity check** عند كل View جديد: الـ code-behind يحوي فقط `InitializeComponent()` و constructor، لا event handlers، لا business logic.
- **No regression:** Phase 1–5 (544 اختبار) يَبقى ناجحاً بدون تعديل في أي من اختباراتها.

### 3. ما لم يُدرَج عمداً

| البند | السبب |
|------|------|
| **Natigh.com Integration** (بكل تفاصيلها: `INatighIntegrationService`, `HttpClient` calls, `Visit.NatighConsent`/`NatighConsentAt`, `NatighPublishWindow`, Feature Toggle, Mock + Http implementations) | **مُستبعَد نهائياً بقرار صاحب المشروع** — النظام يعمل Offline بالكامل ولا يحتاج أي تكامل شبكي. هذا البند **ليس مؤجَّلاً لمرحلة لاحقة** بل **مُستبعَد من النطاق نهائياً**. لا Migration ولا Service ولا ViewModel ولا Window ولا حقل ولا اختبار يخصه يَجب أن يوجد في الـ output. |
| Repository Pattern | مرفوض صراحةً بقواعد المهمة. |
| تفكيك `VisitService` (615 سطر) | مرفوض صراحةً. |
| تفكيك `FinalLabDbContext` (2340 سطر) | مرفوض صراحةً. |
| نقل `TestPricingEngine` / `ResultStageRules` من Infrastructure/ | تجميلي، لا يَطلبه PRD ولا يَمَس وظيفة Phase 6. |
| `IMenuViewModelFactory` لـ MainViewModel | الـ menu VMs الموجودة تَستقبل خدمات مُحقَنة فعلاً في MainViewModel (`INavigationService`, `IDialogService`, `IInventoryService`). `new MenuVM(_navigationService)` ليس Service Locator — يَنقل dependency معروف. |
| استبدال `?? 1` fallback في Staff ID بالكامل عبر كل المشروع | تعديل 6.0 يَقتصر على `PatientRegistrationViewModel.cs` فقط. توسيعه يَمَس 8+ ملفات و يَكسر اختبارات Phase 1–5 التي تَفترض session فارغة. |
| AutoMapper / Mapster | ~50 سطر manual mapping موجودة لا تَستحق إضافة dependency. |
| Polly / retry libraries | `IBackupService` و `IDeliveryConfirmationService` يعملان بالكامل Offline — لا retry policy network مطلوب. |
| Sync-over-async في `FinalLabDbContext.SaveChanges()` | غير مُستدعى من كود إنتاجي (grep يُؤكد) — تأثيره صفر حالياً وإصلاحه قد يُكسر contract الـ DbContext. |
| SMTP sending service | Migration تُضيف config فقط (SmtpHost/Port/Username/PasswordEncrypted/EnableSsl). خدمة الإرسال الفعلية خارج نطاق Phase 6 ولا تُطلب في PRD. |
| توحيد `AsyncRelayCommand.ErrorOccurred` | يَمَسّ 30+ ViewModel، خارج النطاق. |
| Test Pricing Engine / Result Stage Rules نقل من Infrastructure | عمل تجميلي. |

### 4. BR Compliance Matrix (التي بقيت ضمن النطاق)

| BR | الشريحة المنفِّذة | آلية التنفيذ |
|----|--------------------|---------------|
| BR-060 (Backup file مشفَّر AES) | 6.2 | `AesEncryptionHelper` (AES-256-CBC + PBKDF2 100k iterations + random salt + random IV) |
| BR-061 (Restore يَتطلَّب admin + confirmation) | 6.2 + 6.3 | `_currentUserSession.CurrentUser?.IsAdmin == true` في الـ Service + `BackupPasswordDialog` confirmation في الـ VM |

> **ملاحظة:** BR-062 (Natigh publish consent) **مُستبعَد بالكامل** مع الميزة نفسها.

### 5. معايير `dotnet build` / `dotnet test` بعد كل شريحة

كل شريحة تَختتم بـ:
```
dotnet build              # 0 errors, 0 warnings
dotnet test               # {Validation Gate} / {Validation Gate} passed
grep -rn "App.ServiceProvider.GetRequiredService" FinalLabSystem/ViewModels/   # 0 hits
grep -rn "Natigh" FinalLabSystem/ FinalLabSystem.Tests/                       # 0 hits (مُطبَّق بعد شريحة 6.7 فقط)
git status                # no uncommitted .bak files
```

### 6. ترتيب التنفيذ المقترح بالأيام

| اليوم | الشريحة | ساعات تقديرية |
|-------|---------|---------------|
| 1 | 6.0 — Foundation Cleanup | 6 ساعات |
| 2 | 6.1 — PrintPreview MVVM (+ V-28 IPrintService extension) | 8 ساعات |
| 3 | 6.2 — Backup Foundation (+ V-27 LabSetting SMTP/Backup Migration) | 8 ساعات |
| 4 | 6.3 — Backup UI & Restore Workflow | 8 ساعات |
| 5 | 6.4 — Report Settings UI | 6 ساعات |
| 6 | 6.5 — Print Queue / Batch | 5 ساعات |
| 7 | 6.6 — Delivery Confirmation (Signature + OTP) | 8 ساعات |
| 8 | 6.7 — Integration Tests + IMPLEMENTATION_STATUS Update | 4 ساعات |

**المجموع التقديري: ~53 ساعة عمل (8 أيام)**

---

## معايير إكمال Phase 6 بالكامل

القائمة التالية يجب أن تَكون جميعها ✅ قبل اعتبار Phase 6 مُكتملة:

| # | المعيار | نعم/لا |
|---|---------|:------:|
| 1 | ☐ جميع الخدمات الجديدة (`IBarcodeDialogFactory`, `IReceiptDialogFactory`, `INormalRangesWindowFactory`, `IPrintPreviewDialogService`, `IBackupService`, `IReportLayoutService`, `IPrintQueueService`, `IDeliveryConfirmationService`, `OtpGenerator`) مسجَّلة في DI في `App.xaml.cs` | ☐ |
| 2 | ☐ جميع الـ ViewModels والـ Windows الجديدة مُسجَّلة Transient في DI | ☐ |
| 3 | ☐ الـ Migrations الـ 2 (`AddLabSettingSmtpAndBackupConfig`, `AddDeliveryConfirmationFields`) طُبِّقت بنجاح على قاعدة البيانات الفعلية وعلى EF InMemory في الاختبارات | ☐ |
| 4 | ☐ Service Locator في 3 المواضع (`PatientRegistrationViewModel.cs:345`, `PatientRegistrationViewModel.cs:357`, `TestDataManagementViewModel.cs:203`) مُصلَح بالكامل عبر الـ 3 factories الجديدة — `grep "App.ServiceProvider.GetRequiredService" FinalLabSystem/ViewModels/` يُرجع صفر | ☐ |
| 5 | ☐ Empty catch في `PatientRegistrationViewModel.InitializeAsync` يَستقبل `Exception ex` ويَستدعي `_logger.LogError` | ☐ |
| 6 | ☐ `LabSetting.cs` يحوي 8 حقول جديدة (`SmtpHost`, `SmtpPort`, `SmtpUsername`, `SmtpPasswordEncrypted`, `SmtpEnableSsl`, `BackupScheduleHour`, `BackupRetentionDays`, `BackupOutputFolder`) — تنفيذ V-27 **بدون أي حقل Natigh** | ☐ |
| 7 | ☐ `IPrintService` يَكشف `Task PrintFlowDocumentAsync(FlowDocument document, string description)` — تنفيذ V-28 — و `WpfFlowDocumentPrintService` يَنشُذه | ☐ |
| 8 | ☐ `PrintPreviewViewModel` موجود بـ `Document`, `Description`, `PrintCommand`, `CloseCommand`, `RequestClose`، و `PrintPreviewWindow.xaml.cs` يحوي فقط `InitializeComponent()` (≤ 12 سطر) | ☐ |
| 9 | ☐ `BackupRestoreWindow`, `ReportSettingsWindow`, `PrintQueueWindow`, `SignatureConfirmationDialog`, `OtpVerificationDialog` تَفتح وتَعمل | ☐ |
| 10 | ☐ `BackupMenuViewModel` لم يَعد placeholder (يَستدعي `INavigationService.OpenTaskWindow<BackupRestoreWindowViewModel>`) | ☐ |
| 11 | ☐ `ReportSettingsMenuViewModel` يحوي `NavigateToReportSettingsCommand` بالإَضافة إلى `ManageTemplatesCommand` القائم | ☐ |
| 12 | ☐ **لا وجود لأي ملف أو خدمة أو ViewModel أو Window أو Migration أو حقل أو Feature Toggle أو اختبار يَحوي اسم "Natigh" في أي مكان بالمشروع** (الاستثناء الوحيد: السطر الصريح في IMPLEMENTATION_STATUS.md الذي يَذكر الاستبعاد النهائي) | ☐ |
| 13 | ☐ `Visit.cs` يحوي `DeliveryConfirmedAt`, `DeliverySignature`, `DeliveryOtpCode` فقط من التعديلات الجديدة — لا `NatighConsent` ولا `NatighConsentAt` ولا أي حقل Natigh | ☐ |
| 14 | ☐ كل الـ 642 اختبار الجديد ينجح | ☐ |
| 15 | ☐ اختبارات Phase 1–5 (544 اختبار) لا تَزال ناجحة بدون تعديل في أي منها | ☐ |
| 16 | ☐ `dotnet build` بدون errors و بدون warnings | ☐ |
| 17 | ☐ `IMPLEMENTATION_STATUS.md` مُحدَّث بقسم Phase 6 كامل بنفس بنية Phases 1–5 (Status ✅، عدد الاختبارات 642، Migrations الـ 2، قائمة المميزات، BR Compliance Matrix، Build status)، ويَذكر صراحةً أن ميزة Natigh.com Integration مُستبعَدة نهائياً بقرار المشروع | ☐ |
| 18 | ☐ `git status` نظيف، لا ملفات `.bak` أو `bin/obj/` غير مُتتبَّعة | ☐ |

**عند ✅ جميع المعايير الـ 18، Phase 6 مُكتملة رسمياً وقابلة للاعتماد.**
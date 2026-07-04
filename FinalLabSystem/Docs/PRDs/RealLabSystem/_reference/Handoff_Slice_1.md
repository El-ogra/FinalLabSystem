# ملف التسليم — الشريحة الأولى: الباركود ثلاثي المستويات + Lab ID الدائم

**المعرّف:** Slice 1 — S-BC-01
**الأولوية:** حرجة (Foundational)
**الجهد المقدَّر:** 5-6 أيام عمل فعلي
**تاريخ الإعداد:** 2026-07-04

---

## تعليمات إلزامية للوكيل المنفّذ

> **اعتمد حصريًا على محتوى هذا الملف. لا تُعِد تحليل المشروع أو التخطيط من جديد. ابدأ التنفيذ مباشرة من المرحلة صفر.**
>
> هذا الملف يحتوي كل ما تحتاجه: القرارات المحسومة، البنية التفصيلية، الملفات المتأثرة، Migration، الاختبارات، ومعايير القبول. أي انحراف عن محتواه يُعتبر خطأً.

---

## 0. القرارات المحسومة نهائيًا (حقائق ثابتة — لا تُعاد）

### القرار 1: صيغة الباركود النهائية

**الصيغة المعتمدة (13 خانة إجمالًا):**
{النوع:1}-{الفرع:1}-{التاريخ:yymmdd:6}-{يوم الأسبوع:1}-{الترتيب:D3}-{رقم التحقق:1}


**أمثلة:**
- كود الحالة (Case): `1-1-230704-1-001-3`
- كود الملف (File): `3-1-230704-1-001-8`
- كود المعمل (Lab ID): `5-1-230704-1-001-2`

**تفاصيل كل خانة:**

| الخانة | الطول | الوصف |
|---|---|---|
| النوع | 1 | `1` = Case, `3` = File, `5` = Lab |
| الفرع | 1 | رقم الفرع — ثابت `1` لأن نظام الفروع المتعدد خارج نطاق هذا المشروع |
| التاريخ | 6 | صيغة `yymmdd` — مثال: `230704` = 4 يوليو 2023 |
| يوم الأسبوع | 1 | الأحد=1، الاثنين=2، الثلاثاء=3، الأربعاء=4، الخميس=5، الجمعة=6، السبت=7 |
| الترتيب | 3 | ترتيب دخول المريض في يوم الشغل — `D3` (001, 002, ...) |
| رقم التحقق | 1 | خوارزمية Luhn mod 10 |

**ملاحظات إلزامية على الصيغة:**
- صيغة التاريخ هي `yymmdd` وليست `ddmmyy`.
- يوم الأسبوع يبدأ الأحد عند `1` (وليس `0`). التصحيح مبني على صورة ملصق فعلي من النظام المرجعي بتاريخ 25 ديسمبر 2016 (يوم أحد) يظهر فيه الرقم `1` لليوم.
- رقم الفرع ثابت `1` لأن نظام الفروع خارج نطاق المشروع.
- رقم التحقق (آخر خانة) لا يطابق خوارزمية النظام المرجعي الأصلي — نستخدم Luhn mod 10 لأن نظامنا مستقل.

### القرار 2: استقلال SampleTube.BarcodeValue

**معتمد:** الأنابيب تحتفظ بصيغتها الحالية `{PatientCode}-{ordinal:D2}` للاستخدام الداخلي (ملصقات الأنابيب). الأكواد الثلاثة (Case/File/Lab) مستقلة تمامًا على مستوى المريض/الزيارة في جدول `PatientBarcode` الجديد.

### القرار 3: عرض الأكواد الثلاثة عبر TabControl

**معتمد:** `BarcodeDialog` الحالي يُحدَّث بإضافة `TabControl` بثلاثة أقسام:
1. ملصقات الأنابيب (القائمة الحالية)
2. كود الحالة + كود الملف (ملصقان منفصلان)
3. كود المعمل — Lab ID (ملصق واحد)

---

## 1. وصف الفجوة

في الكوميت الحالي:
- `Services/Implementations/SampleTrackingService.cs` سطر 59 يُنتج **كودًا واحدًا فقط** بصيغة `$"{patientCode}-{ordinal:D2}"` (مثال: `P0001-01`, `P0001-02`).
- `Models/Patient.cs` لا يحوي أي حقل `LabId`.
- `Models/LabSetting.cs` لا يحوي `BranchNumber`.
- لا يوجد `Models/PatientBarcode.cs` ولا `IBarcodeGenerator` ولا `BarcodeGenerator`.
- `BarcodeDialogViewModel.cs` يعرض فقط ملصقات الأنابيب (`SampleTube`) — لا Case Code ولا File Code ولا Lab ID.

الفجوة كاملة مقابل المرجع الذي يفرض:
1. ثلاثة أنواع أكواد لكل مريض — Case (يبدأ بـ `1`)، File (يبدأ بـ `3`)، Lab ID (يبدأ بـ `5`).
2. كل كود بطول 13 خانة رقمية وفق الصيغة المعتمدة في القرار 1.
3. Lab ID دائم يُنشأ عند أول زيارة ولا يتغيّر عبر الزيارات اللاحقة.
4. زر مستقل لطباعة Lab ID من `PatientRegistrationWindow`.

---

## 2. المراحل والخطوات التفصيلية

### المرحلة 0: التأسيس (نصف يوم)

#### الخطوة 0.1: إنشاء `Models/Enums/BarcodeCodeType.cs` (جديد)

```csharp
namespace FinalLabSystem.Models.Enums;

public enum BarcodeCodeType : byte
{
    Case = 1,
    File = 3,
    Lab = 5
}
الخطوة 0.2: إنشاء Models/PatientBarcode.cs (جديد)
namespace FinalLabSystem.Models;

public class PatientBarcode
{
    public int PatientBarcodeId { get; set; }
    public int PatientId { get; set; }
    public int? VisitId { get; set; }
    public BarcodeCodeType CodeType { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public int SortOrdinal { get; set; }
    public byte BranchNumber { get; set; } = 1;
    public int? CreatedBy { get; set; }

    // Navigations
    public Patient Patient { get; set; } = null!;
    public Visit? Visit { get; set; }
    public Staff? Staff { get; set; }
}
ملاحظة: VisitId يكون null لـ Lab ID لأن كود المعمل لا يرتبط بزيارة محددة.

الخطوة 0.3: تعديل Models/Patient.cs
إضافة الحقل التالي:

[StringLength(13)]
public string? LabId { get; set; }
الخطوة 0.4: تعديل Models/LabSetting.cs
إضافة الحقل التالي:

public byte BranchNumber { get; set; } = 1;
المرحلة 1: خدمة توليد الباركود (يوم واحد)
الخطوة 1.1: إنشاء Services/Interfaces/IBarcodeGenerator.cs (جديد)
namespace FinalLabSystem.Services.Interfaces;

public interface IBarcodeGenerator
{
    Task<string> GenerateCaseCodeAsync(int visitId);
    Task<string> GenerateFileCodeAsync(int visitId);
    Task<string> GetOrCreateLabIdAsync(int patientId);
}
الخطوة 1.2: إنشاء Services/Implementations/BarcodeGenerator.cs (جديد)
البنية الكاملة للخدمة:

namespace FinalLabSystem.Services.Implementations;

public class BarcodeGenerator : IBarcodeGenerator
{
    private readonly FinalLabDbContext _context;

    public BarcodeGenerator(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateCaseCodeAsync(int visitId)
    {
        return await GenerateCodeAsync(visitId, BarcodeCodeType.Case);
    }

    public async Task<string> GenerateFileCodeAsync(int visitId)
    {
        return await GenerateCodeAsync(visitId, BarcodeCodeType.File);
    }

    public async Task<string> GetOrCreateLabIdAsync(int patientId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null)
            throw new ArgumentException($"Patient {patientId} not found");

        if (!string.IsNullOrEmpty(patient.LabId))
            return patient.LabId;

        var visit = await _context.Visits
            .Where(v => v.PatientId == patientId)
            .OrderBy(v => v.VisitDate)
            .FirstOrDefaultAsync();

        var visitDate = visit?.VisitDate ?? DateTime.Today;
        var labId = await GenerateLabIdAsync(patientId, visitDate);

        patient.LabId = labId;
        await _context.SaveChangesAsync();

        return labId;
    }

    private async Task<string> GenerateCodeAsync(int visitId, BarcodeCodeType codeType)
    {
        var visit = await _context.Visits
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit == null)
            throw new ArgumentException($"Visit {visitId} not found");

        var patientId = visit.PatientId;
        var visitDate = visit.VisitDate;
        var branchNumber = await GetBranchNumberAsync();
        var ordinal = await GetDailyOrdinalAsync(patientId, visitDate);

        return BuildBarcodeValue((byte)codeType, branchNumber, visitDate, ordinal);
    }

    private async Task<string> GenerateLabIdAsync(int patientId, DateTime visitDate)
    {
        var branchNumber = await GetBranchNumberAsync();
        return BuildBarcodeValue((byte)BarcodeCodeType.Lab, branchNumber, visitDate, 0);
    }

    private string BuildBarcodeValue(byte typeDigit, byte branchDigit, DateTime date, int ordinal)
    {
        var datePart = date.ToString("yyMMdd");
        var weekday = (int)date.DayOfWeek + 1;
        var ordinalPart = ordinal.ToString("D3");
        var partial = $"{typeDigit}{branchDigit}{datePart}{weekday}{ordinalPart}";
        var checkDigit = CalculateLuhnCheckDigit(partial);
        return $"{partial}{checkDigit}";
    }

    private async Task<byte> GetBranchNumberAsync()
    {
        var setting = await _context.LabSettings.FirstOrDefaultAsync();
        return setting?.BranchNumber ?? 1;
    }

    private async Task<int> GetDailyOrdinalAsync(int patientId, DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _context.PatientBarcodes
            .Where(pb => pb.PatientId == patientId
                      && pb.IssueDate >= dayStart
                      && pb.IssueDate < dayEnd
                      && pb.CodeType == BarcodeCodeType.Case)
            .CountAsync() + 1;
    }

    private static int CalculateLuhnCheckDigit(string code)
    {
        int sum = 0;
        bool alternate = true;
        for (int i = code.Length - 1; i >= 0; i--)
        {
            int digit = code[i] - '0';
            if (alternate)
                digit *= 2;
            if (digit > 9)
                digit -= 9;
            sum += digit;
            alternate = !alternate;
        }
        int remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }
}
شرح خوارزمية Check Digit (Luhn mod 10):

نقرأ الكود الجزئي (بدون 마지막 خانة) من اليمين لليسار.
الخانة الأولى من اليمين (التي ستكون ordinal الأعلى) تُضرب في 2.
نتناوب بين الضرب في 2 وعدم الضرب.
إذا كان الناتج > 9، نطرح 9.
نجمع كل القيم.
رقم التحقق = (10 - (المجموع % 10)) % 10.
مثال حسابي لكود الحالة 1-1-230704-1-001-?:

الخانة	القيمة
النوع	1
الفرع	1
التاريخ	230704
يوم الأسبوع	1 (الأحد)
الترتيب	001
الجزء الكلي	112307041001
رقم التحقق	يُحسب بـ Luhn mod 10
المرحلة 2: Migration (نصف يوم)
الخطوة 2.1: إنشاء Migration AddPatientBarcodeAndLabId
محتويات SQL:

-- 1. إنشاء جدول PatientBarcode
CREATE TABLE [PatientBarcode] (
    [PatientBarcodeId] INT IDENTITY(1,1) NOT NULL,
    [PatientId] INT NOT NULL,
    [VisitId] INT NULL,
    [CodeType] TINYINT NOT NULL,
    [BarcodeValue] NVARCHAR(13) NOT NULL,
    [IssueDate] DATETIME2 NOT NULL,
    [SortOrdinal] INT NOT NULL,
    [BranchNumber] TINYINT NOT NULL DEFAULT 1,
    [CreatedBy] INT NULL,
    CONSTRAINT [PK_PatientBarcode] PRIMARY KEY ([PatientBarcodeId]),
    CONSTRAINT [FK_PatientBarcode_Patient] FOREIGN KEY ([PatientId])
        REFERENCES [Patient]([patient_id]),
    CONSTRAINT [FK_PatientBarcode_Visit] FOREIGN KEY ([VisitId])
        REFERENCES [Visit]([visit_id]),
    CONSTRAINT [FK_PatientBarcode_Staff] FOREIGN KEY ([CreatedBy])
        REFERENCES [Staff]([staff_id])
);

-- 2. فهرس فريد على BarcodeValue
CREATE UNIQUE INDEX [IX_PatientBarcode_BarcodeValue]
    ON [PatientBarcode]([BarcodeValue]);

-- 3. فهرس مركب على PatientId + CodeType
CREATE INDEX [IX_PatientBarcode_PatientId_CodeType]
    ON [PatientBarcode]([PatientId], [CodeType]);

-- 4. إضافة Patient.LabId
ALTER TABLE [Patient] ADD [LabId] NVARCHAR(13) NULL;

-- 5. فهرس فريد مُرشّح على LabId
CREATE UNIQUE INDEX [IX_Patient_LabId]
    ON [Patient]([LabId]) WHERE [LabId] IS NOT NULL;

-- 6. إضافة LabSetting.BranchNumber
ALTER TABLE [LabSetting] ADD [BranchNumber] TINYINT NOT NULL DEFAULT 1;
ملاحظات على Migration:

لا توجد بيانات في PatientBarcode (جدول جديد) — لا مشكلة.
Patient.LabId يبدأ فارغاً — Filtered Index يستثني NULLs تلقائياً.
LabSetting.BranchNumber يأخذ القيمة الافتراضية 1.
Fluent API المطلوب إضافته في FinalLabDbContext.cs:

// PatientBarcode
modelBuilder.Entity<PatientBarcode>(entity =>
{
    entity.ToTable("PatientBarcode");
    entity.HasKey(e => e.PatientBarcodeId);

    entity.HasIndex(e => e.BarcodeValue)
        .IsUnique();

    entity.HasIndex(e => new { e.PatientId, e.CodeType })
        .HasDatabaseName("IX_PatientBarcode_PatientId_CodeType");

    entity.Property(e => e.CodeType)
        .HasConversion<byte>();

    entity.HasOne(e => e.Patient)
        .WithMany()
        .HasForeignKey(e => e.PatientId);

    entity.HasOne(e => e.Visit)
        .WithMany()
        .HasForeignKey(e => e.VisitId)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(e => e.Staff)
        .WithMany()
        .HasForeignKey(e => e.CreatedBy)
        .OnDelete(DeleteBehavior.SetNull);
});

// Patient.LabId - filtered unique index
modelBuilder.Entity<Patient>()
    .HasIndex(p => p.LabId)
    .IsUnique()
    .HasFilter("[LabId] IS NOT NULL");
DbSet جديد:

public DbSet<PatientBarcode> PatientBarcodes { get; set; }
المرحلة 3: تعديل الخدمات الحالية (يوم واحد)
الخطوة 3.1: تعديل Services/Implementations/SampleTrackingService.cs
التغيير الجوهري: إضافة تبعية IBarcodeGenerator واستدعاءه في GenerateBarcodesForVisitAsync.

ما يجب تغييره:

إضافة حقل _barcodeGenerator في Constructor.
بعد تحميل الزيارة والمريض، استدعاء IBarcodeGenerator لتوليد الأكواد الثلاثة.
حفظ الأكواد في جدول PatientBarcode.
إبقاء منطق SampleTube الحالي كما هو (يُبقى BarcodeValue القديم {PatientCode}-{ordinal:D2} للأنابيب).
الهيكل العام للتعديل:

// في Constructor: إضافة IBarcodeGenerator
private readonly IBarcodeGenerator _barcodeGenerator;

public SampleTrackingService(FinalLabDbContext context,
    ILogger<SampleTrackingService> logger,
    IBarcodeGenerator barcodeGenerator)
{
    _context = context;
    _logger = logger;
    _barcodeGenerator = barcodeGenerator;
}

// في GenerateBarcodesForVisitAsync: بعد تحميل الزيارة
// 1. توليد الأكواد الثلاثة
var caseCode = await _barcodeGenerator.GenerateCaseCodeAsync(visitId);
var fileCode = await _barcodeGenerator.GenerateFileCodeAsync(visitId);
var labId = await _barcodeGenerator.GetOrCreateLabIdAsync(visit.PatientId);

// 2. حفظها في PatientBarcode
var branchNumber = (await _context.LabSettings.FirstOrDefaultAsync())?.BranchNumber ?? 1;
var ordinal = /* حساب ترتيب اليوم */;
var today = DateTime.Today;

_context.PatientBarcodes.AddRange(
    new PatientBarcode { PatientId = visit.PatientId, VisitId = visitId,
        CodeType = BarcodeCodeType.Case, BarcodeValue = caseCode,
        IssueDate = DateTime.Now, SortOrdinal = ordinal,
        BranchNumber = branchNumber, CreatedBy = staffId },
    new PatientBarcode { PatientId = visit.PatientId, VisitId = visitId,
        CodeType = BarcodeCodeType.File, BarcodeValue = fileCode,
        IssueDate = DateTime.Now, SortOrdinal = ordinal,
        BranchNumber = branchNumber, CreatedBy = staffId },
    new PatientBarcode { PatientId = visit.PatientId, VisitId = null,
        CodeType = BarcodeCodeType.Lab, BarcodeValue = labId,
        IssueDate = DateTime.Now, SortOrdinal = 0,
        BranchNumber = branchNumber, CreatedBy = staffId }
);
await _context.SaveChangesAsync();

// 3. إنشاء SampleTubes كالمعتاد (بدون تغيير في الصيغة)
القرار المعماري: SampleTube.BarcodeValue يبقى {PatientCode}-{ordinal:D2} — الأكواد الثلاثة مستقلة تمامًا.

المرحلة 4: تعديل واجهات المستخدم (يوم واحد)
الخطوة 4.1: تحديث ViewModels/Patients/BarcodeDialogViewModel.cs
إضافة الخصائص والأوامر التالية:

// خصائص جديدة
public ObservableCollection<BarcodeLabel> CaseLabels { get; } = new();
public ObservableCollection<BarcodeLabel> FileLabels { get; } = new();
public ObservableCollection<BarcodeLabel> LabIdLabels { get; } = new();

public ICommand PrintCaseLabelsCommand { get; }
public ICommand PrintFileLabelsCommand { get; }
public ICommand PrintLabIdCommand { get; }
public ICommand PrintAllBarcodesCommand { get; }
إضافة دالة تحميل جديدة:

public async Task LoadBarcodesAsync(int visitId)
{
    // تحميل ملصقات الأنابيب (القائمة الحالية)
    await LoadTubesAsync(visitId);

    // تحميل الأكواد الثلاثة من PatientBarcode
    var barcodes = await _context.PatientBarcodes
        .Where(pb => pb.VisitId == visitId
                  || (pb.CodeType == BarcodeCodeType.Lab
                      && pb.PatientId == /* PatientId من الزيارة */))
        .ToListAsync();

    CaseLabels.Clear();
    FileLabels.Clear();
    LabIdLabels.Clear();

    foreach (var barcode in barcodes)
    {
        var label = ProjectBarcodeLabel(barcode);
        switch (barcode.CodeType)
        {
            case BarcodeCodeType.Case:
                CaseLabels.Add(label);
                break;
            case BarcodeCodeType.File:
                FileLabels.Add(label);
                break;
            case BarcodeCodeType.Lab:
                LabIdLabels.Add(label);
                break;
        }
    }
}
ملاحظة: يجب تمرير PatientId إلى LoadBarcodesAsync لتحميل Lab ID حتى لو لم يكن VisitId مطابقاً (لأن Lab ID لا يرتبط بزيارة).

الخطوة 4.2: تحديث Views/Patients/BarcodeDialog.xaml
إضافة TabControl بثلاثة أقسام:

<TabControl>
    <!-- القسم 1: ملصقات الأنابيب (القائمة الحالية) -->
    <TabItem Header="ملصقات الأنابيب">
        <ItemsControl ItemsSource="{Binding Labels}">
            <!-- نفس التخطيط الحالي -->
        </ItemsControl>
    </TabItem>

    <!-- القسم 2: كود الحالة + كود الملف -->
    <TabItem Header="كود الحالة + كود الملف">
        <StackPanel>
            <TextBlock Text="كود الحالة (Case)" FontWeight="Bold" Margin="0,0,0,5"/>
            <ItemsControl ItemsSource="{Binding CaseLabels}">
                <!-- نفس تخطيط الملصق الحالي -->
            </ItemsControl>
            <TextBlock Text="كود الملف (File)" FontWeight="Bold" Margin="0,15,0,5"/>
            <ItemsControl ItemsSource="{Binding FileLabels}">
                <!-- نفس تخطيط الملصق الحالي -->
            </ItemsControl>
            <Button Content="طباعة كود الحالة والملف"
                    Command="{Binding PrintCaseLabelsCommand}"
                    Style="{StaticResource MainActionButtonStyle}"
                    Margin="0,10,0,0"/>
        </StackPanel>
    </TabItem>

    <!-- القسم 3: كود المعمل -->
    <TabItem Header="كود المعمل (Lab ID)">
        <StackPanel>
            <TextBlock Text="كود المعمل الدائم (Lab ID)" FontWeight="Bold" Margin="0,0,0,5"/>
            <ItemsControl ItemsSource="{Binding LabIdLabels}">
                <!-- نفس تخطيط الملصق الحالي -->
            </ItemsControl>
            <Button Content="طباعة كود المعمل"
                    Command="{Binding PrintLabIdCommand}"
                    Style="{StaticResource MainActionButtonStyle}"
                    Margin="0,10,0,0"/>
        </StackPanel>
    </TabItem>
</TabControl>
الخطوة 4.3: تحديث ViewModels/Patients/PatientRegistrationViewModel.cs
إضافة:

private readonly IBarcodeGenerator _barcodeGenerator;
public ICommand PrintLabIdCommand { get; }
public bool CanPrintLabId => CurrentPatientId > 0;

// في Constructor:
PrintLabIdCommand = new AsyncRelayCommand(PrintLabIdAsync, () => CanPrintLabId);

private async Task PrintLabIdAsync()
{
    var labId = await _barcodeGenerator.GetOrCreateLabIdAsync(CurrentPatientId);
    // إنشاء ملصق Lab ID وطبعه
    var labLabel = new BarcodeLabel { BarcodePayload = labId, /* باقي الحقول */ };
    await _labelPrintService.PrintLabelsAsync(new[] { labLabel });
}
الخطوة 4.4: تحديث Views/Patients/PatientRegistrationWindow.xaml
إضافة زر Lab ID بجوار زر الباركود الحالي:

<Button Grid.Row="1" Grid.Column="2"
        Content="طباعة Lab ID"
        Command="{Binding PrintLabIdCommand}"
        IsEnabled="{Binding CanPrintLabId}"
        Style="{StaticResource MainActionButtonStyle}"/>
المرحلة 5: تسجيل DI (نصف ساعة)
الخطوة 5.1: تعديل App.xaml.cs
إضافة سطر واحد:

services.AddScoped<IBarcodeGenerator, BarcodeGenerator>();
المرحلة 6: الاختبارات (يوم واحد)
6.1 اختبارات Unit — BarcodeGenerator (5 اختبارات)
#	اسم الاختبار	ما يتحقق منه
1	GenerateCaseCodeAsync_Returns13Chars_StartsWith1	الطول = 13، البادئة = 1، التاريخ = yymmdd
2	GenerateFileCodeAsync_Returns13Chars_StartsWith3	الطول = 13، البادئة = 3، التاريخ = yymmdd
3	GetOrCreateLabIdAsync_NewPatient_CreatesAndPersists	أول استدعاء يُنشئ Lab ID ويحفظه في Patient.LabId
4	GetOrCreateLabIdAsync_ExistingPatient_ReturnsSame	الاستدعاء الثاني يُرجع نفس القيمة بدون إنشاء جديد
5	CalculateCheckDigit_ProducesValidLuhn	التحقق من أن رقم التحقق يحقق شروط Luhn mod 10
6.2 اختبارات Integration — SampleTrackingService (2 اختبارات)
#	اسم الاختبار	ما يتحقق منه
6	GenerateBarcodesForVisit_FirstVisit_CreatesAll3Codes	زيارة أولى تُنشئ Case + File + Lab ID في PatientBarcode
7	GenerateBarcodesForVisit_SecondVisit_ReusesLabId	Lab ID القديم يُقرأ من Patient.LabId، Case + File جديدَان بـ ordinal متزايد
6.3 اختبارات ViewModel (2 اختبارات)
#	اسم الاختبار	ما يتحقق منه
8	BarcodeDialog_LabIdSection_ShowsOnlyOneLabel	لا يظهر أكثر من ملصق Lab ID واحد في القسم الثالث
9	PatientRegistration_PrintLabIdCommand_EnabledOnlyAfterSave	الزر معطَّل قبل حفظ المريض (CurrentPatientId = 0)، مفعَّل بعده
6.4 اختبار Migration (1 اختبار)
#	اسم الاختبار	ما يتحقق منه
10	Migration_AddPatientBarcodeAndLabId_AppliesWithoutDataLoss	Migration تعمل على قاعدة موجودة، PatientBarcode تُنشأ، Patient.LabId تُضاف، LabSetting.BranchNumber يُضاف
3. الملفات المتأثرة (قائمة نهائية)
الملف	الحالة	الفعل
Models/Enums/BarcodeCodeType.cs	جديد	إنشاء
Models/PatientBarcode.cs	جديد	إنشاء
Models/Patient.cs	تعديل	إضافة LabId
Models/LabSetting.cs	تعديل	إضافة BranchNumber
Services/Interfaces/IBarcodeGenerator.cs	جديد	إنشاء
Services/Implementations/BarcodeGenerator.cs	جديد	إنشاء
Services/Implementations/SampleTrackingService.cs	تعديل جوهري	إضافة IBarcodeGenerator + حفظ في PatientBarcode
ViewModels/Patients/BarcodeDialogViewModel.cs	تعديل جوهري	إضافة 3 أوامر + 3 مجموعات ملصقات
Views/Patients/BarcodeDialog.xaml	تعديل	إضافة TabControl بثلاثة أقسام
ViewModels/Patients/PatientRegistrationViewModel.cs	تعديل	إضافة PrintLabIdCommand
Views/Patients/PatientRegistrationWindow.xaml	تعديل	إضافة زر Lab ID
Data/FinalLabDbContext.cs	تعديل	DbSet + Fluent API
App.xaml.cs	تعديل	تسجيل IBarcodeGenerator → BarcodeGenerator
4. معايير القبول النهائية
#	المعيار
1	مريض جديد يُنتج له عند أول زيارة: Case Code (بادئة 1، 13 خانة، yymmdd) + File Code (بادئة 3، 13 خانة، yymmdd) + Lab ID (بادئة 5، 13 خانة، yymmdd) — الثلاثة محفوظة في PatientBarcode
2	زيارة ثانية لنفس المريض (في نفس اليوم) تُنتج Case + File جديدَين بـ ordinal متزايد، بينما يُقرأ Lab ID القديم من Patient.LabId
3	زر مستقل لطباعة Lab ID موجود ويعمل من PatientRegistrationWindow
4	رقم التحقق (آخر خانة) يُحسب بخوارزمية Luhn mod 10 ومتسق داخلياً
5	BarcodeDialog يعرض three أقسام عبر TabControl: ملصقات الأنابيب / كود الحالة والملف / كود المعمل
6	جميع الاختبارات العشرة تجتاز
7	Migration AddPatientBarcodeAndLabId تُطبَّق على قاعدة موجودة دون فقدان بيانات
5. مخاطر واعتبارات
المخاطرة	التأثير	الحل
الزيارات القديمة لا تملك Lab ID	لا يظهر Lab ID للمرضى الحاليين	لا نُهاجر الأكواد القديمة — Lab ID يُنشأ عند أول زيارة تالية
SampleTube.BarcodeValue يبقى بصيغته القديمة	لا تأثير — الأكواد الثلاثة مستقلة	لا نغير صيغة الأنابيب
PatientBarcode جدول جديد = تكلفة Migration عالية	لا — الجدول فارغ، لا ترحيل بيانات	Migration بسيطة: CREATE TABLE + ALTER TABLE
Filtered Index على Patient.LabId	قد يسبب مشاكل في بعض إصدارات SQL Server	الإصدارات الحديثة (2008+) تدعم Filtered Index بشكل كامل
Check Digit لا يُتحقق من صحة البيانات المدخلة يدوياً	يمكن إدخال كود غير صالح	BarcodeGenerator يحسب Check Digit تلقائياً — لا إدخال يدوي
انتهى ملف التسليم — الشريحة الأولى. ```
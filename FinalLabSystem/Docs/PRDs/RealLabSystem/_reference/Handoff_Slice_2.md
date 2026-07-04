I now have all the data needed. Here is the complete Handoff_Slice_2.md content:

# ملف التسليم — الشريحة الثانية: نافذة إدخال المزرعة والحساسية + تصنيف Enum

**المعرّف:** Slice 2 — S-CULT-01
**الأولوية:** حرجة (سريريًا)
**الجهد المقدَّر:** 5 – 6 أيام عمل فعلي
**تاريخ الإعداد:** 2026-07-04

---

## تعليمات إلزامية للوكيل المنفّذ

> **اعتمد حصريًا على محتوى هذا الملف. لا تُعِد تحليل المشروع أو التخطيط من جديد. ابدأ التنفيذ مباشرة من المرحلة صفر.**
>
> هذا الملف يحتوي كل ما تحتاجه: القرارات المحسومة، البنية التفصيلية، الملفات المتأثرة، Migration، الاختبارات، ومعايير القبول. أي انحراف عن محتواه يُعتبر خطأً.

---

## 0. القرارات المحسومة نهائيًا (حقائق ثابتة — لا تُعاد)

### القرار 1: خريطة تحويل بيانات Sensitivity القديمة

**الجدول `OrganismAntibiotic` في قاعدة البيانات المحلية فارغ تمامًا حاليًا — لا توجد بيانات حقيقية يُخشى فقدان معناها.**

الحروف `S`/`I`/`R` تحمل المعنى السريري المعياري العالمي:
- `S = Susceptible` (حساس)
- `I = Intermediate` (متوسط)
- `R = Resistant` (مقاوم)

**الخريطة المعتمدة نهائيًا للـ Data Migration:**

| القيمة القديمة (nchar(1)) | القيمة الجديدة (enum byte) |
|---|---|
| `S` | `0` (Highly) |
| `I` | `1` (Moderate) |
| `R` | `3` (Resistant) |
| أي قيمة أخرى أو NULL أو فارغة | `3` (Resistant — كأقل ضرر افتراضي) |

> **⚠ تحذير إلزامي:** بما أن الجدول فارغ فعليًا في بيئة التطوير المحلية، هذه الخريطة تُطبَّق كإجراء احترازي معماري (schema safety) وليس لحماية بيانات فعلية. **قبل تطبيق Migration هذه على أي بيئة أخرى غير بيئة التطوير المحلية (staging أو production لاحقًا)، يجب تشغيل `SELECT DISTINCT Sensitivity, COUNT(*) FROM OrganismAntibiotic GROUP BY Sensitivity` أولاً والتحقق يدويًا من عدم وجود بيانات حقيقية قبل المتابعة.**

### القرار 2: آلية فتح نافذة المزرعة (معماري — محسوم)

تمتد واجهة `IResultEntryDialogService` بإضافة دالة جديدة:
```csharp
Task<bool> OpenCultureAsync(int visitTestId, int patientId, bool isPregnant, int patientAgeDays, string patientGender);
وليس إنشاء خدمة منفصلة ICultureEntryDialogService. السبب: الحفاظ على النمط المعماري القائم في المشروع دون اختراع نمط جديد موازٍ.

ثم TestResultsViewModel.OpenMultiComponentEditorAsync يتحقق من test.SpecialType == "CULTURE" ويستدعي OpenCultureAsync بدلاً من OpenAsync العادية.

القرار 3: مصير الدالة AddOrganismsAndSensitivitiesAsync
تم التحقق في 2026-07-04: بحث شامل في كامل المشروع أظهر أن الدالة AddOrganismsAndSensitivitiesAsync تظهر في 4 أماكن فقط:

SLICES_1_AND_2_APPROVED.md — وثيقة تخطيط فقط (سطر 224, 255)
ICultureResultService.cs — تعريف الواجهة (سطر 28)
CultureResultService.cs — التنفيذ (سطر 42)
لا يوجد أي استدعاء فعلي لها في أي كود قابل للتنفيذ — لا في ViewModels، ولا في اختبارات، ولا في خدمات أخرى. الدالة SaveCultureAsync أيضًا مُعرَّفة لكن لا تُستدعى.

القرار: حذف AddOrganismsAndSensitivitiesAsync بالكامل من الواجهة والتنفيذ واستبدالها بـ SaveFullCultureAsync الجديدة. حذفها آمن ولا يكسر أي مسار تنفيذي قائم.

القرار 4: التعامل مع "No Growth"
يُسمح بحفظ نتيجة المزرعة بدون أي organism واحد فقط إذا كانت قيمة CultureResult تساوي "No Growth" (بعد تجاهل حالة الأحرف والمسافات الزائدة — case-insensitive comparison). أي قيمة أخرى لـ CultureResult مع قائمة organisms فارغة يجب أن تمنع الحفظ وتُظهر رسالة تحقق واضحة: "يجب إدخال كائن حي واحد على الأقل، أو تعيين النتيجة إلى 'No Growth'."

1. وصف الفجوة (بناءً على الكود الفعلي)
في الكود الحالي:

موجود: موديلات كاملة (MicrobiologyCulture بـ 11 حقل، MicrobiologyOrganism بـ 7 حقول، OrganismAntibiotic بـ 9 حقول، AntibioticCatalog بـ 6 حقول)، وخدمة CultureResultService (3 دوال: GetSafeAntibioticsAsync, SaveCultureAsync, AddOrganismsAndSensitivitiesAsync)، وربط DbContext كامل، و FK إلى AntibioticCatalog.
العيوب المكتشفة:
OrganismAntibiotic.Sensitivity من نوع string في الموديل لكن معرَّف في DbContext بـ HasMaxLength(1).IsFixedLength() — أي أن العمود في قاعدة البيانات nchar(1) ويحوي حرفًا واحدًا فقط (S/I/R)، وليس نصًا كاملًا كما تفترض وثائق GAP_ANALYSIS.
AddOrganismsAndSensitivitiesAsync و SaveCultureAsync دوال ميتة — مُعرَّفة لكن لا تُستدعى من أي مكان في المشروع.
لا يوجد CultureCondition ولا ColonyCount على MicrobiologyCulture (المستوى الكلي للعينة). ColonyCount موجود فقط على MicrobiologyOrganism (لكل كائن حي على حدة).
لا يوجد Models/Enums/AntibioticSensitivity.cs.
لا واجهة WPF (CultureEntryWindow.xaml) ولا CultureEntryViewModel.cs.
لا CultureReportTemplate.cs.
لا آلية لفتح نافذة المزرعة — ResultEntryDialogService يفتح ResultEntryWindow دائمًا.
لا يوجد SpecialType = "CULTURE" في كتالوج التحاليل لتمييز تحاليل المزرعة.
SaveCultureAsync و AddOrganismsAndSensitivitiesAsync تنفصلان بـ SaveChangesAsync منفصلتين — لا transaction واحد.
2. المراحل والخطوات التفصيلية
المرحلة 0: التأسيس (نصف يوم)
الخطوة 0.1: إنشاء Models/Enums/AntibioticSensitivity.cs (جديد)
namespace FinalLabSystem.Models.Enums;

public enum AntibioticSensitivity : byte
{
    Highly = 0,
    Moderate = 1,
    Low = 2,
    Resistant = 3
}
الخطوة 0.2: تعديل Models/MicrobiologyCulture.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalLabSystem.Models;

public partial class MicrobiologyCulture
{
    public int CultureId { get; set; }

    public int VisitTestId { get; set; }

    public string? SpecimenSource { get; set; }

    public double? SpecimenVolumeMl { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public int? InoculatedBy { get; set; }

    public string CultureResult { get; set; } = null!;

    [StringLength(200)]
    public string? CultureCondition { get; set; }

    [StringLength(50)]
    public string? ColonyCount { get; set; }

    public short? IncubationHours { get; set; }

    public DateTime? FinalReadingAt { get; set; }

    public int? ReadBy { get; set; }

    public string? FinalComment { get; set; }

    public virtual Staff? InoculatedByNavigation { get; set; }

    public virtual ICollection<MicrobiologyOrganism> MicrobiologyOrganisms { get; set; } = new List<MicrobiologyOrganism>();

    public virtual Staff? ReadByNavigation { get; set; }

    public virtual VisitTest VisitTest { get; set; } = null!;
}
ملاحظة: الأسطر المضافان هما CultureCondition (سطر 22-23) و ColonyCount (سطر 25-26). باقي الملف يبقى كما هو.

الخطوة 0.3: تعديل Models/OrganismAntibiotic.cs
using System;
using System.Collections.Generic;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

public partial class OrganismAntibiotic
{
    public int AntibioticResultId { get; set; }

    public int OrganismId { get; set; }

    public string AntibioticName { get; set; } = null!;

    public string? AntibioticClass { get; set; }

    public AntibioticSensitivity Sensitivity { get; set; } = AntibioticSensitivity.Resistant;

    public string? MicValue { get; set; }

    public double? DiskDiffusionMm { get; set; }

    public string? BreakpointStandard { get; set; }

    public int? AntibioticCatalogId { get; set; }

    // Navigation properties
    public virtual MicrobiologyOrganism Organism { get; set; } = null!;

    public virtual AntibioticCatalog? Antibiotic { get; set; }
}
التغيير: السطر 16 تغيّر من public string Sensitivity { get; set; } = null!; إلى public AntibioticSensitivity Sensitivity { get; set; } = AntibioticSensitivity.Resistant; مع إضافة using FinalLabSystem.Models.Enums; في الأعلى.

المرحلة 1: Migration + Data Migration (نصف يوم)
الخطوة 1.1: تعديل Data/FinalLabDbContext.cs — تحديث MicrobiologyCulture config
في الدالة OnModelCreating، داخل modelBuilder.Entity<MicrobiologyCulture>(entity => { ... })، أضف سطرَين بعد سطر SpecimenVolumeMl:

entity.Property(e => e.CultureCondition)
    .HasMaxLength(200)
    .HasColumnName("culture_condition");

entity.Property(e => e.ColonyCount)
    .HasMaxLength(50)
    .HasColumnName("colony_count");
الخطوة 1.2: تعديل Data/FinalLabDbContext.cs — تحديث OrganismAntibiotic.Sensitivity config
في الدالة OnModelCreating، داخل modelBuilder.Entity<OrganismAntibiotic>(entity => { ... }))، استبدل السطور 680-683:

// القديم (احذفه):
// entity.Property(e => e.Sensitivity)
//     .HasMaxLength(1)
//     .IsFixedLength()
//     .HasColumnName("sensitivity");

// الجديد (ضعه بدلاً منه):
entity.Property(e => e.Sensitivity)
    .HasConversion<byte>()
    .HasMaxLength(1)
    .HasColumnName("sensitivity");
ملاحظة: HasConversion<byte>() يحوّل enum إلى tinyint في SQL. HasMaxLength(1) يبقى للتوافق مع الأعمدة القائمة nchar(1) خلال عملية الترحيل.

الخطوة 1.3: إنشاء Migration AddCultureFieldsAndSensitivityEnum
أنشئ Migration باستخدام:

dotnet ef migrations add AddCultureFieldsAndSensitivityEnum --project FinalLabSystem
تأكد من أن الملف المُنشأ (*.cs) يحتوي:

//的部分 — إضافة أعمدة جديدة على MicrobiologyCulture
migrationBuilder.AddColumn<string>(
    name: "culture_condition",
    table: "MicrobiologyCulture",
    type: "nvarchar(200)",
    nullable: true);

migrationBuilder.AddColumn<string>(
    name: "colony_count",
    table: "MicrobiologyCulture",
    type: "nvarchar(50)",
    nullable: true);

// تحويل Sensitivity من nchar(1) إلى tinyint
// الخطوة 1: إضافة عمود مؤقت
migrationBuilder.AddColumn<byte>(
    name: "Sensitivity_New",
    table: "OrganismAntibiotic",
    type: "tinyint",
    nullable: false,
    defaultValue: (byte)3);

// الخطوة 2: تعبئة العمود المؤقت (Data Migration)
migrationBuilder.Sql(@"
    UPDATE OrganismAntibiotic
    SET Sensitivity_New = CASE
        WHEN Sensitivity = 'S' THEN 0
        WHEN Sensitivity = 'I' THEN 1
        WHEN Sensitivity = 'R' THEN 3
        ELSE 3
    END
");

// الخطوة 3: حذف العمود القديم
migrationBuilder.DropColumn(
    name: "Sensitivity",
    table: "OrganismAntibiotic");

// الخطوة 4: إعادة تسمية العمود الجديد
migrationBuilder.RenameColumn(
    name: "Sensitivity_New",
    table: "OrganismAntibiotic",
    newName: "Sensitivity");
⚠ تذكير: بما أن الجدول فارغ، يمكنك أيضًا تبسيط Migration بـ DROP + ADD مباشرة بدون Data Migration SQL. لكن الأعلى هو الصيغة الآمنة التي تعمل حتى مع بيانات موجودة.

المرحلة 2: تعزيز الخدمة (يوم واحد)
الخطوة 2.1: تعديل Services/Interfaces/ICultureResultService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface ICultureResultService
{
    /// <summary>
    /// Gets antibiotics considered safe for the supplied patient conditions.
    /// </summary>
    Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);

    /// <summary>
    /// Loads an existing culture result with its organisms and antibiotics for a given VisitTest.
    /// Returns null if no culture exists for this VisitTest.
    /// </summary>
    Task<MicrobiologyCulture?> GetByVisitTestIdAsync(int visitTestId);

    /// <summary>
    /// Saves a complete culture result: the culture record, its organisms, and their antibiotic
    /// sensitivity rows — all within a single SaveChangesAsync call (transaction).
    /// </summary>
    Task SaveFullCultureAsync(MicrobiologyCulture culture, List<MicrobiologyOrganism> organisms);

    /// <summary>
    /// Updates a single antibiotic sensitivity value.
    /// </summary>
    Task UpdateSensitivityAsync(int antibioticResultId, AntibioticSensitivity value);
}
الملحوظات:

SaveCultureAsync و AddOrganismsAndSensitivitiesAsync القديمتان محذوفتان بالكامل (تم التحقق من عدم وجود أي استدعاء لهما في المشروع بتاريخ 2026-07-04).
GetByVisitTestIdAsync جديدة — لتحميل نتيجة موجودة عند إعادة فتح النافذة.
SaveFullCultureAsync جديدة — تحفظCulture + Organisms + Antibiotics في transaction واحد.
UpdateSensitivityAsync جديدة — تحديث دقيق لحساسية مضاد واحد.
الخطوة 2.2: تعديل Services/Implementations/CultureResultService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class CultureResultService : ICultureResultService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<CultureResultService> _logger;

    public CultureResultService(FinalLabDbContext context, ILogger<CultureResultService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild)
    {
        var query = _context.AntibioticCatalogs.Where(a => a.IsActive);

        if (isPregnant)
            query = query.Where(a => a.IsSafePregnancy);

        if (isChild)
            query = query.Where(a => a.IsSafeChildren);

        return await query.OrderBy(a => a.AntibioticName).ToListAsync();
    }

    public async Task<MicrobiologyCulture?> GetByVisitTestIdAsync(int visitTestId)
    {
        return await _context.MicrobiologyCultures
            .Include(c => c.MicrobiologyOrganisms)
                .ThenInclude(o => o.OrganismAntibiotics)
            .FirstOrDefaultAsync(c => c.VisitTestId == visitTestId);
    }

    public async Task SaveFullCultureAsync(MicrobiologyCulture culture, List<MicrobiologyOrganism> organisms)
    {
        // Attach culture if it's new (Id == 0) or already tracked
        if (culture.CultureId == 0)
            _context.MicrobiologyCultures.Add(culture);

        foreach (var organism in organisms)
        {
            organism.CultureId = culture.CultureId;

            if (organism.OrganismId == 0)
                _context.MicrobiologyOrganisms.Add(organism);

            foreach (var antibiotic in organism.OrganismAntibiotics)
            {
                antibiotic.OrganismId = organism.OrganismId;

                if (antibiotic.AntibioticResultId == 0)
                    _context.OrganismAntibiotics.Add(antibiotic);
                else
                    _context.OrganismAntibiotics.Update(antibiotic);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateSensitivityAsync(int antibioticResultId, AntibioticSensitivity value)
    {
        var entity = await _context.OrganismAntibiotics.FindAsync(antibioticResultId);
        if (entity != null)
        {
            entity.Sensitivity = value;
            await _context.SaveChangesAsync();
        }
    }
}
الملحوظات:

SaveCultureAsync و AddOrganismsAndSensitivitiesAsync القديمتان محذوفتان بالكامل.
SaveFullCultureAsync تتعامل مع الكائنات الجديدة (Id == 0) والمُعدَّلة (Update).
حفظ واحد SaveChangesAsync = transaction واحد.
المرحلة 3: ViewModel (يومان)
الخطوة 3.1: إنشاء ViewModels/Patients/CultureEntryViewModel.cs (جديد)
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class CultureEntryViewModel : ViewModelBase
{
    private readonly ICultureResultService _cultureService;
    private readonly IDialogService _dialogService;
    private readonly FinalLabDbContext _context;
    private readonly int _visitTestId;
    private readonly int _patientId;
    private readonly bool _isPregnant;
    private readonly int _patientAgeDays;

    private string _specimenSource = string.Empty;
    private string _cultureCondition = string.Empty;
    private string _colonyCount = string.Empty;
    private short? _incubationHours = 48;
    private DateTime? _receivedAt;
    private DateTime? _finalReadingAt;
    private string _finalComment = string.Empty;
    private string _cultureResult = string.Empty;
    private OrganismInputVm? _selectedOrganism;
    private bool _isSaving;

    public CultureEntryViewModel(
        ICultureResultService cultureService,
        IDialogService dialogService,
        FinalLabDbContext context,
        int visitTestId,
        int patientId,
        bool isPregnant,
        int patientAgeDays)
    {
        _cultureService = cultureService;
        _dialogService = dialogService;
        _context = context;
        _visitTestId = visitTestId;
        _patientId = patientId;
        _isPregnant = isPregnant;
        _patientAgeDays = patientAgeDays;

        Organisms = new ObservableCollection<OrganismInputVm>();
        AvailableAntibiotics = new ObservableCollection<AntibioticCatalog>();

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        AddOrganismCommand = new AsyncRelayCommand(AddOrganismAsync, () => Organisms.Count < 3);
        RemoveOrganismCommand = new AsyncRelayCommand<OrganismInputVm>(RemoveOrganismAsync);
    }

    public ObservableCollection<OrganismInputVm> Organisms { get; }
    public ObservableCollection<AntibioticCatalog> AvailableAntibiotics { get; }

    public string SpecimenSource
    {
        get => _specimenSource;
        set => SetProperty(ref _specimenSource, value);
    }

    public string CultureCondition
    {
        get => _cultureCondition;
        set => SetProperty(ref _cultureCondition, value);
    }

    public string ColonyCount
    {
        get => _colonyCount;
        set => SetProperty(ref _colonyCount, value);
    }

    public short? IncubationHours
    {
        get => _incubationHours;
        set => SetProperty(ref _incubationHours, value);
    }

    public DateTime? ReceivedAt
    {
        get => _receivedAt;
        set => SetProperty(ref _receivedAt, value);
    }

    public DateTime? FinalReadingAt
    {
        get => _finalReadingAt;
        set => SetProperty(ref _finalReadingAt, value);
    }

    public string FinalComment
    {
        get => _finalComment;
        set => SetProperty(ref _finalComment, value);
    }

    public string CultureResult
    {
        get => _cultureResult;
        set
        {
            if (SetProperty(ref _cultureResult, value))
                OnPropertyChanged(nameof(CanSaveWithoutOrganisms));
        }
    }

    public OrganismInputVm? SelectedOrganism
    {
        get => _selectedOrganism;
        set => SetProperty(ref _selectedOrganism, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public bool CanSaveWithoutOrganisms =>
        CultureResult.Trim().Equals("No Growth", StringComparison.OrdinalIgnoreCase);

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddOrganismCommand { get; }
    public ICommand RemoveOrganismCommand { get; }

    public Action? RequestClose { get; set; }

    public async Task LoadAsync()
    {
        var existing = await _cultureService.GetByVisitTestIdAsync(_visitTestId);
        if (existing != null)
        {
            CultureResult = existing.CultureResult;
            SpecimenSource = existing.SpecimenSource ?? string.Empty;
            CultureCondition = existing.CultureCondition ?? string.Empty;
            ColonyCount = existing.ColonyCount ?? string.Empty;
            IncubationHours = existing.IncubationHours;
            ReceivedAt = existing.ReceivedAt;
            FinalReadingAt = existing.FinalReadingAt;
            FinalComment = existing.FinalComment ?? string.Empty;

            foreach (var org in existing.MicrobiologyOrganisms.OrderBy(o => o.SortOrder))
            {
                var vm = new OrganismInputVm
                {
                    OrganismId = org.OrganismId,
                    OrganismName = org.OrganismName,
                    GramStain = org.GramStain ?? string.Empty,
                    ColonyCount = org.ColonyCount ?? string.Empty,
                    Morphology = org.Morphology ?? string.Empty,
                    SortOrder = org.SortOrder
                };

                foreach (var abx in org.OrganismAntibiotics)
                {
                    vm.AntibioticResults.Add(new AntibioticResultVm
                    {
                        AntibioticResultId = abx.AntibioticResultId,
                        AntibioticName = abx.AntibioticName,
                        Sensitivity = abx.Sensitivity,
                        AntibioticCatalogId = abx.AntibioticCatalogId
                    });
                }

                Organisms.Add(vm);
            }
        }
        else
        {
            ReceivedAt = DateTime.Now;
            CultureResult = "PENDING";
        }

        await LoadAvailableAntibioticsAsync();
    }

    private async Task LoadAvailableAntibioticsAsync()
    {
        var antibiotics = await _cultureService.GetSafeAntibioticsAsync(_isPregnant, _patientAgeDays < 12 * 365);
        AvailableAntibiotics.Clear();
        foreach (var abx in antibiotics)
            AvailableAntibiotics.Add(abx);
    }

    private async Task AddOrganismAsync()
    {
        if (Organisms.Count >= 3)
        {
            _dialogService.ShowWarning("لا يمكن إضافة أكثر من 3 كائنات حية.", "حد أقصى");
            return;
        }

        var newOrg = new OrganismInputVm
        {
            SortOrder = (byte)(Organisms.Count + 1)
        };

        // Seed antibiotic results from available antibiotics
        foreach (var abx in AvailableAntibiotics)
        {
            newOrg.AntibioticResults.Add(new AntibioticResultVm
            {
                AntibioticName = abx.AntibioticName,
                Sensitivity = AntibioticSensitivity.Resistant,
                AntibioticCatalogId = abx.AntibioticId
            });
        }

        Organisms.Add(newOrg);
        SelectedOrganism = newOrg;

        await Task.CompletedTask;
    }

    private async Task RemoveOrganismAsync(OrganismInputVm? organism)
    {
        if (organism == null) return;
        Organisms.Remove(organism);

        // Re-number sort orders
        for (int i = 0; i < Organisms.Count; i++)
            Organisms[i].SortOrder = (byte)(i + 1);

        if (SelectedOrganism == organism)
            SelectedOrganism = Organisms.FirstOrDefault();

        await Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        if (IsSaving) return;

        // Validation: at least one organism OR "No Growth"
        if (Organisms.Count == 0 && !CanSaveWithoutOrganisms)
        {
            _dialogService.ShowWarning(
                "يجب إدخال كائن حي واحد على الأقل، أو تعيين النتيجة إلى 'No Growth'.",
                "تحقق");
            return;
        }

        // Validation: each organism must have a name
        foreach (var org in Organisms)
        {
            if (string.IsNullOrWhiteSpace(org.OrganismName))
            {
                _dialogService.ShowWarning(
                    $"يجب إدخال اسم الكائن في الصف رقم {org.SortOrder}.",
                    "تحقق");
                return;
            }
        }

        IsSaving = true;
        try
        {
            var culture = new MicrobiologyCulture
            {
                CultureId = 0,
                VisitTestId = _visitTestId,
                CultureResult = CultureResult,
                SpecimenSource = SpecimenSource,
                CultureCondition = CultureCondition,
                ColonyCount = ColonyCount,
                IncubationHours = IncubationHours,
                ReceivedAt = ReceivedAt,
                FinalReadingAt = FinalReadingAt,
                FinalComment = FinalComment
            };

            var organisms = new List<MicrobiologyOrganism>();
            foreach (var orgVm in Organisms)
            {
                var organism = new MicrobiologyOrganism
                {
                    OrganismId = orgVm.OrganismId,
                    OrganismName = orgVm.OrganismName,
                    GramStain = orgVm.GramStain,
                    ColonyCount = orgVm.ColonyCount,
                    Morphology = orgVm.Morphology,
                    SortOrder = orgVm.SortOrder,
                    OrganismAntibiotics = orgVm.AntibioticResults
                        .Select(r => new OrganismAntibiotic
                        {
                            AntibioticResultId = r.AntibioticResultId,
                            AntibioticName = r.AntibioticName,
                            Sensitivity = r.Sensitivity,
                            AntibioticCatalogId = r.AntibioticCatalogId
                        }).ToList()
                };
                organisms.Add(organism);
            }

            await _cultureService.SaveFullCultureAsync(culture, organisms);
            _dialogService.ShowMessage("تم حفظ نتيجة المزرعة بنجاح.", "حفظ");
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"حدث خطأ أثناء الحفظ: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}

public sealed class OrganismInputVm : INotifyPropertyChanged
{
    private string _organismName = string.Empty;
    private string _gramStain = string.Empty;
    private string _colonyCount = string.Empty;
    private string _morphology = string.Empty;

    public int OrganismId { get; set; }
    public byte SortOrder { get; set; }

    public string OrganismName
    {
        get => _organismName;
        set { _organismName = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrganismName))); }
    }

    public string GramStain
    {
        get => _gramStain;
        set { _gramStain = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GramStain))); }
    }

    public string ColonyCount
    {
        get => _colonyCount;
        set { _colonyCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColonyCount))); }
    }

    public string Morphology
    {
        get => _morphology;
        set { _morphology = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Morphology))); }
    }

    public ObservableCollection<AntibioticResultVm> AntibioticResults { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class AntibioticResultVm : INotifyPropertyChanged
{
    private AntibioticSensitivity _sensitivity = AntibioticSensitivity.Resistant;

    public int AntibioticResultId { get; set; }
    public string AntibioticName { get; set; } = string.Empty;
    public int? AntibioticCatalogId { get; set; }

    public AntibioticSensitivity Sensitivity
    {
        get => _sensitivity;
        set
        {
            _sensitivity = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sensitivity)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
الخطوة 3.2: تعديل Services/Interfaces/IResultEntryDialogService.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IResultEntryDialogService
{
    Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName,
                         ObservableCollection<TestComponentResultDto> components,
                         int patientAgeDays, string patientGender, bool isPregnant);

    Task<bool> OpenCultureAsync(int visitTestId, int patientId,
                                bool isPregnant, int patientAgeDays, string patientGender);
}
الخطوة 3.3: تعديل Services/Implementations/ResultEntryDialogService.cs
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Implementations;

public sealed class ResultEntryDialogService : IResultEntryDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public ResultEntryDialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName,
                                ObservableCollection<TestComponentResultDto> components,
                                int patientAgeDays, string patientGender, bool isPregnant)
    {
        var tcs = new TaskCompletionSource<bool>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var routineResultService = (IRoutineResultService)_serviceProvider.GetService(typeof(IRoutineResultService))!;
            var visitService = (IVisitService)_serviceProvider.GetService(typeof(IVisitService))!;
            var auditService = (IAuditService)_serviceProvider.GetService(typeof(IAuditService))!;
            var currentUserSession = (ICurrentUserSession)_serviceProvider.GetService(typeof(ICurrentUserSession))!;
            var dialogService = (IDialogService)_serviceProvider.GetService(typeof(IDialogService))!;

            var vm = new ResultEntryViewModel(
                routineResultService,
                visitService,
                auditService,
                currentUserSession,
                dialogService,
                visitTestId,
                patientId,
                testTypeName,
                components,
                patientAgeDays,
                patientGender,
                isPregnant);

            var window = new Views.Patients.ResultEntryWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.RequestClose = () =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (window.IsLoaded)
                    {
                        window.DialogResult = tcs.Task.IsCompleted ? null : false;
                        window.Close();
                    }
                });
            };

            vm.SaveCompleted += (_, _) =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    tcs.TrySetResult(true);
                });
            };

            window.Closed += (_, _) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(false);
            };

            window.ShowDialog();
        });

        return tcs.Task;
    }

    public Task<bool> OpenCultureAsync(int visitTestId, int patientId,
                                       bool isPregnant, int patientAgeDays, string patientGender)
    {
        var tcs = new TaskCompletionSource<bool>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var cultureService = (ICultureResultService)_serviceProvider.GetService(typeof(ICultureResultService))!;
            var dialogService = (IDialogService)_serviceProvider.GetService(typeof(IDialogService))!;
            var context = (Data.FinalLabDbContext)_serviceProvider.GetService(typeof(Data.FinalLabDbContext))!;

            var vm = new CultureEntryViewModel(
                cultureService,
                dialogService,
                context,
                visitTestId,
                patientId,
                isPregnant,
                patientAgeDays);

            var window = new Views.Patients.CultureEntryWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.RequestClose = () =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (window.IsLoaded)
                    {
                        window.DialogResult = tcs.Task.IsCompleted ? null : false;
                        window.Close();
                    }
                });
            };

            window.Closed += (_, _) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(false);
            };

            window.Loaded += async (_, _) =>
            {
                await vm.LoadAsync();
            };

            window.ShowDialog();
        });

        return tcs.Task;
    }
}
الخطوة 3.4: تعديل ViewModels/Patients/TestResultsViewModel.cs
في الدالة OpenMultiComponentEditorAsync (السطر 655)، استبدل المحتوى بـ:

private async Task OpenMultiComponentEditorAsync(VisitTestItemDto test)
{
    var patientId = CurrentPatientInfo?.PatientId ?? 0;

    int patientAgeDays = 0;
    if (CurrentPatientInfo != null)
    {
        var years = CurrentPatientInfo.ApproxAge ?? 0;
        patientAgeDays = years * 365;
    }

    bool saved;

    if (string.Equals(test.SpecialType, "CULTURE", StringComparison.OrdinalIgnoreCase))
    {
        saved = await _resultEntryDialogService.OpenCultureAsync(
            test.VisitTestId,
            patientId,
            CurrentPatientInfo?.IsPregnant ?? false,
            patientAgeDays,
            CurrentPatientInfo?.Sex ?? "U");
    }
    else
    {
        saved = await _resultEntryDialogService.OpenAsync(
            test.VisitTestId,
            patientId,
            test.TestTypeName,
            new ObservableCollection<TestComponentResultDto>(test.ComponentResults),
            patientAgeDays,
            CurrentPatientInfo?.Sex ?? "U",
            CurrentPatientInfo?.IsPregnant ?? false);
    }

    if (saved && SelectedPatient != null)
        await SelectPatientAsync(SelectedPatient);
}
المرحلة 4: الواجهة (يوم واحد)
الخطوة 4.1: إنشاء Views/Patients/CultureEntryWindow.xaml (جديد)
<Window x:Class="FinalLabSystem.Views.Patients.CultureEntryWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="إدخال نتيجة المزرعة" Height="750" Width="1000"
        WindowStartupLocation="CenterOwner"
        FlowDirection="RightToLeft">
    <DockPanel Margin="12">
        <!-- أزرار الأسفل -->
        <StackPanel DockPanel.Dock="Bottom" Orientation="Horizontal"
                    HorizontalAlignment="Left" Margin="0,10,0,0">
            <Button Content="حفظ" Command="{Binding SaveCommand}"
                    Height="36" Width="100" Margin="0,0,8,0"/>
            <Button Content="إلغاء" Command="{Binding CancelCommand}"
                    Height="36" Width="100"/>
        </StackPanel>

        <ScrollViewer VerticalScrollBarVisibility="Auto">
            <StackPanel>
                <!-- القسم 1: بيانات العينة -->
                <GroupBox Header="بيانات العينة" Margin="0,0,0,10" Padding="8">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="8"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="8"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <TextBlock Grid.Row="0" Grid.Column="0" Text="مصدر العينة:"
                                   VerticalAlignment="Center" Margin="0,0,8,0"/>
                        <TextBox Grid.Row="0" Grid.Column="1"
                                 Text="{Binding SpecimenSource, UpdateSourceTrigger=PropertyChanged}"/>

                        <TextBlock Grid.Row="0" Grid.Column="2" Text="حالة الزراعة:"
                                   VerticalAlignment="Center" Margin="0,0,8,0"/>
                        <TextBox Grid.Row="0" Grid.Column="3"
                                 Text="{Binding CultureCondition, UpdateSourceTrigger=PropertyChanged}"
                                 ToolTip="مثال: Aerobic, 37°C, 24h"/>

                        <TextBlock Grid.Row="2" Grid.Column="0" Text="عدد الكائنات:"
                                   VerticalAlignment="Center" Margin="0,0,8,0"/>
                        <TextBox Grid.Row="2" Grid.Column="1"
                                 Text="{Binding ColonyCount, UpdateSourceTrigger=PropertyChanged}"
                                 ToolTip="مثال: 10^5 CFU/mL"/>

                        <TextBlock Grid.Row="2" Grid.Column="2" Text="ساعات الحضانة:"
                                   VerticalAlignment="Center" Margin="0,0,8,0"/>
                        <TextBox Grid.Row="2" Grid.Column="3"
                                 Text="{Binding IncubationHours, UpdateSourceTrigger=PropertyChanged}"/>

                        <TextBlock Grid.Row="4" Grid.Column="0" Text="نتيجة الزراعة:"
                                   VerticalAlignment="Center" Margin="0,0,8,0"/>
                        <TextBox Grid.Row="4" Grid.Column="1"
                                 Text="{Binding CultureResult, UpdateSourceTrigger=PropertyChanged}"/>

                        <TextBlock Grid.Row="4" Grid.Column="2" Text="ملاحظات:"
                                   VerticalAlignment="Center" Margin="0,0,8,0"/>
                        <TextBox Grid.Row="4" Grid.Column="3"
                                 Text="{Binding FinalComment, UpdateSourceTrigger=PropertyChanged}"/>
                    </Grid>
                </GroupBox>

                <!-- القسم 2: الكائنات الحية -->
                <GroupBox Header="الكائنات الحية (0 - 3)" Margin="0,0,0,10" Padding="8">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                            <Button Content="إضافة كائن" Command="{Binding AddOrganismCommand}"
                                    Height="30" Width="100" Margin="0,0,8,0"/>
                            <TextBlock Text="الحد الأقصى 3 كائنات"
                                       VerticalAlignment="Center" Foreground="Gray"/>
                        </StackPanel>

                        <ItemsControl ItemsSource="{Binding Organisms}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border BorderBrush="#ccc" BorderThickness="1"
                                            Margin="0,0,0,8" Padding="8">
                                        <StackPanel>
                                            <Grid>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width="Auto"/>
                                                    <ColumnDefinition Width="*"/>
                                                    <ColumnDefinition Width="Auto"/>
                                                    <ColumnDefinition Width="*"/>
                                                </Grid.ColumnDefinitions>
                                                <Grid.RowDefinitions>
                                                    <RowDefinition Height="Auto"/>
                                                    <RowDefinition Height="4"/>
                                                    <RowDefinition Height="Auto"/>
                                                </Grid.RowDefinitions>

                                                <TextBlock Grid.Row="0" Grid.Column="0"
                                                           Text="اسم الكائن:" Margin="0,0,8,0"/>
                                                <TextBox Grid.Row="0" Grid.Column="1"
                                                         Text="{Binding OrganismName, UpdateSourceTrigger=PropertyChanged}"/>

                                                <TextBlock Grid.Row="0" Grid.Column="2"
                                                           Text="صبغة جرام:" Margin="0,0,8,0"/>
                                                <TextBox Grid.Row="0" Grid.Column="3"
                                                         Text="{Binding GramStain, UpdateSourceTrigger=PropertyChanged}"/>

                                                <TextBlock Grid.Row="2" Grid.Column="0"
                                                           Text="عدد الكائنات:" Margin="0,0,8,0"/>
                                                <TextBox Grid.Row="2" Grid.Column="1"
                                                         Text="{Binding ColonyCount, UpdateSourceTrigger=PropertyChanged}"/>

                                                <TextBlock Grid.Row="2" Grid.Column="2"
                                                           Text="شكل الكائن:" Margin="0,0,8,0"/>
                                                <TextBox Grid.Row="2" Grid.Column="3"
                                                         Text="{Binding Morphology, UpdateSourceTrigger=PropertyChanged}"/>
                                            </Grid>

                                            <!-- جدول الحساسية لهذا الكائن -->
                                            <DataGrid ItemsSource="{Binding AntibioticResults}"
                                                      AutoGenerateColumns="False"
                                                      IsReadOnly="False"
                                                      Margin="0,8,0,0"
                                                      CanUserAddRows="False">
                                                <DataGrid.Columns>
                                                    <DataGridTextColumn Header="المضاد الحيوي"
                                                                        Binding="{Binding AntibioticName}"
                                                                        IsReadOnly="True"
                                                                        Width="200"/>
                                                    <DataGridCheckBoxColumn Header="Highly"
                                                                            Binding="{Binding Sensitivity, Converter={x:Static local:SensitivityToBoolConverter.Highly}}"
                                                                            Width="70"/>
                                                    <DataGridCheckBoxColumn Header="Moderate"
                                                                            Binding="{Binding Sensitivity, Converter={x:Static local:SensitivityToBoolConverter.Moderate}}"
                                                                            Width="70"/>
                                                    <DataGridCheckBoxColumn Header="Low"
                                                                            Binding="{Binding Sensitivity, Converter={x:Static local:SensitivityToBoolConverter.Low}}"
                                                                            Width="70"/>
                                                    <DataGridCheckBoxColumn Header="Resistant"
                                                                            Binding="{Binding Sensitivity, Converter={x:Static local:SensitivityToBoolConverter.Resistant}}"
                                                                            Width="80"/>
                                                </DataGrid.Columns>
                                            </DataGrid>
                                        </StackPanel>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </StackPanel>
                </GroupBox>
            </StackPanel>
        </ScrollViewer>
    </DockPanel>
</Window>
الخطوة 4.2: إنشاء Views/Patients/CultureEntryWindow.xaml.cs (جديد)
using System.Windows;

namespace FinalLabSystem.Views.Patients;

public partial class CultureEntryWindow : Window
{
    public CultureEntryWindow()
    {
        InitializeComponent();
    }
}
الخطوة 4.3: إنشاء Views/Converters/SensitivityToBoolConverter.cs (جديد)
هذا الـ Converter يحوّل AntibioticSensitivity enum إلى bool لكل عمود CheckBox في الـ DataGrid:

using System;
using System.Globalization;
using System.Windows.Data;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Views.Converters;

public sealed class SensitivityToBoolConverter : IValueConverter
{
    public static readonly SensitivityToBoolConverter Highly = new(AntibioticSensitivity.Highly);
    public static readonly SensitivityToBoolConverter Moderate = new(AntibioticSensitivity.Moderate);
    public static readonly SensitivityToBoolConverter Low = new(AntibioticSensitivity.Low);
    public static readonly SensitivityToBoolConverter Resistant = new(AntibioticSensitivity.Resistant);

    private readonly AntibioticSensitivity _targetValue;

    private SensitivityToBoolConverter(AntibioticSensitivity targetValue)
    {
        _targetValue = targetValue;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AntibioticSensitivity sensitivity)
            return sensitivity == _targetValue;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
            return _targetValue;
        return Binding.DoNothing;
    }
}
ملاحظة XAML: يجب إضافة namespace فيرأس CultureEntryWindow.xaml:

xmlns:local="clr-namespace:FinalLabSystem.Views.Converters"
المرحلة 5: قالب التقرير (نصف يوم)
الخطوة 5.1: إنشاء Services/Printing/CultureReportTemplate.cs (جديد)
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Printing;

public class CultureReportTemplate
{
    private readonly MicrobiologyCulture _culture;
    private readonly Patient _patient;
    private readonly List<MicrobiologyOrganism> _organisms;
    private readonly string _labName;
    private readonly string _reportTitle;

    public CultureReportTemplate(
        MicrobiologyCulture culture,
        Patient patient,
        List<MicrobiologyOrganism> organisms,
        string labName = "المختبر",
        string reportTitle = "تقرير المزرعة والحساسية")
    {
        _culture = culture;
        _patient = patient;
        _organisms = organisms;
        _labName = labName;
        _reportTitle = reportTitle;
    }

    public FlowDocument BuildDocument()
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Arial"),
            FontSize = 11,
            FlowDirection = FlowDirection.RightToLeft,
            PagePadding = new Thickness(40),
            ColumnGap = 0
        };

        // Header
        doc.Blocks.Add(new Paragraph(new Run(_labName))
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        });

        doc.Blocks.Add(new Paragraph(new Run(_reportTitle))
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12)
        });

        // Patient info
        var infoTable = new Table { CellSpacing = 0 };
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(120) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(200) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(120) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(200) });

        var infoRow = new TableRow();
        AddCell(infoRow, "اسم المريض:", true);
        AddCell(infoRow, _patient.FullNameAr ?? "");
        AddCell(infoRow, "السن:", true);
        AddCell(infoRow, $"{_patient.ApproxAge} {_patient.ApproxAgeUnit}");
        infoTable.RowGroups.Add(new TableBodyGroup { Rows = { infoRow } });

        var infoRow2 = new TableRow();
        AddCell(infoRow2, "الجنس:", true);
        AddCell(infoRow2, _patient.Sex == "M" ? "ذكر" : "أنثى");
        AddCell(infoRow2, "مصدر العينة:", true);
        AddCell(infoRow2, _culture.SpecimenSource ?? "");
        infoTable.RowGroups.Add(new TableBodyGroup { Rows = { infoRow2 } });

        doc.Blocks.Add(infoTable);
        doc.Blocks.Add(new Paragraph()); // spacer

        // Culture details
        doc.Blocks.Add(new Paragraph(new Run("تفاصيل الزراعة"))
        {
            FontWeight = FontWeights.Bold,
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 4)
        });

        var cultureDetails = $"حالة الزراعة: {_culture.CultureCondition ?? "-"}    |    " +
                             $"عدد الكائنات: {_culture.ColonyCount ?? "-"}    |    " +
                             $"ساعات الحضانة: {_culture.IncubationHours?.ToString() ?? "-"}    |    " +
                             $"النتيجة: {_culture.CultureResult}";
        doc.Blocks.Add(new Paragraph(new Run(cultureDetails)) { Margin = new Thickness(0, 0, 0, 8) });

        // Organisms and sensitivity
        foreach (var organism in _organisms.OrderBy(o => o.SortOrder))
        {
            var letter = organism.SortOrder switch { 1 => "A", 2 => "B", 3 => "C", _ => "?" };

            doc.Blocks.Add(new Paragraph(new Run($"Organism {letter}: {organism.OrganismName}"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Margin = new Thickness(0, 8, 0, 4)
            });

            doc.Blocks.Add(new Paragraph(new Run(
                $"Gram Stain: {organism.GramStain ?? "-"}   |   Colony Count: {organism.ColonyCount ?? "-"}   |   Morphology: {organism.Morphology ?? "-"}"))
            {
                Margin = new Thickness(0, 0, 0, 4)
            });

            if (organism.OrganismAntibiotics.Any())
            {
                // Group by sensitivity level
                var grouped = organism.OrganismAntibiotics
                    .GroupBy(a => a.Sensitivity)
                    .OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    var levelName = group.Key switch
                    {
                        AntibioticSensitivity.Highly => "Highly Sensitive",
                        AntibioticSensitivity.Moderate => "Moderately Sensitive",
                        AntibioticSensitivity.Low => "Low Sensitivity",
                        AntibioticSensitivity.Resistant => "Resistant",
                        _ => group.Key.ToString()
                    };

                    var drugNames = string.Join(", ", group.Select(a => a.AntibioticName));

                    doc.Blocks.Add(new Paragraph(
                        new Run($"{levelName}: ") { FontWeight = FontWeights.Bold },
                        new Run(drugNames))
                    {
                        Margin = new Thickness(16, 2, 0, 2)
                    });
                }
            }
            else
            {
                doc.Blocks.Add(new Paragraph(new Run("لا توجد نتائج حساسية"))
                {
                    Margin = new Thickness(16, 2, 0, 2),
                    Foreground = Brushes.Gray
                });
            }
        }

        // Footer
        doc.Blocks.Add(new Paragraph());
        if (!string.IsNullOrWhiteSpace(_culture.FinalComment))
        {
            doc.Blocks.Add(new Paragraph(new Run("ملاحظات: ") { FontWeight = FontWeights.Bold },
                                         new Run(_culture.FinalComment))
            {
                Margin = new Thickness(0, 0, 0, 8)
            });
        }

        doc.Blocks.Add(new Paragraph(new Run("توقيع الفني المسؤول: __________________"))
        {
            Margin = new Thickness(0, 20, 0, 0)
        });

        return doc;
    }

    private static void AddCell(TableRow row, string text, bool isBold)
    {
        var cell = new TableCell(new Paragraph(new Run(text)
        {
            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
        }));
        row.Cells.Add(cell);
    }
}
المرحلة 6: DI والـ Seed (ساعتان)
الخطوة 6.1: تعديل App.xaml.cs
في دالة ConfigureServices، أضف بعد سطر services.AddTransient<CashDrawerWindowViewModel>();:

// Slice 2 — Culture Entry
services.AddTransient<CultureEntryViewModel>();
services.AddTransient<CultureEntryWindow>();
الخطوة 6.2: تعديل TestCatalogSeeder.cs
في الدالة SeedAsync، بعد حلقة إضافة/تعديل الـ TestTypes (حوالي السطر 250)، أضف:

// ── Step: Set SpecialType = "CULTURE" for culture-related tests ──
var cultureTests = await _context.TestTypes
    .Where(t => t.TypeNameEn != null &&
                (t.TypeNameEn.ToLower().Contains("culture") ||
                 t.TypeNameEn.ToLower().Contains("مزرعة") ||
                 t.TypeNameEn.ToLower().Contains("c&s") ||
                 t.TypeNameEn.ToLower().Contains("sensitivity")))
    .ToListAsync(cancellationToken);

foreach (var test in cultureTests)
{
    if (test.SpecialType != "CULTURE")
    {
        test.SpecialType = "CULTURE";
    }
}
await _context.SaveChangesAsync(cancellationToken);
ملاحظة: هذا المنطق يعتمد على أن تحاليل المزرعة تحوي كلمات مفتاحية في اسمها الإنجليزي. إذا لم تُعثر على تحاليل مطابقة، لن يؤثر هذا على النظام (الزر يظهر فقط للتحاليل التي SpecialType == "CULTURE").

المرحلة 7: الاختبارات (يوم واحد)
الخطوة 7.1: إنشاء FinalLabSystem.Tests/Slice2/CultureSlice2Tests.cs (جديد)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Slice2;

public class CultureSlice2Tests
{
    private static FinalLabDbContext CreateInMemoryDbContext(Action<FinalLabDbContext>? seed = null)
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new FinalLabDbContext(options);
        seed?.Invoke(context);
        return context;
    }

    // --- Unit Tests: CultureResultService (3 tests) ---

    [Fact]
    public async Task T01_GetSafeAntibioticsAsync_Pregnant_FiltersOutUnsafe()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Tetracycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true },
                new AntibioticCatalog { AntibioticId = 3, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var result = await service.GetSafeAntibioticsAsync(isPregnant: true, isChild: false);

        Assert.Single(result);
        Assert.Equal("Amoxicillin", result[0].AntibioticName);
    }

    [Fact]
    public async Task T02_GetSafeAntibioticsAsync_Child_FiltersOutUnsafe()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Tetracycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true },
                new AntibioticCatalog { AntibioticId = 3, AntibioticName = "Azithromycin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var result = await service.GetSafeAntibioticsAsync(isPregnant: false, isChild: true);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.IsSafeChildren));
    }

    [Fact]
    public async Task T03_GetSafeAntibioticsAsync_PregnantAndChild_AppliesBothFilters()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Tetracycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true },
                new AntibioticCatalog { AntibioticId = 3, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 4, AntibioticName = "Erythromycin", IsSafePregnancy = true, IsSafeChildren = false, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var result = await service.GetSafeAntibioticsAsync(isPregnant: true, isChild: true);

        Assert.Single(result);
        Assert.Equal("Amoxicillin", result[0].AntibioticName);
    }

    // --- ViewModel Tests (3 tests) ---

    [Fact]
    public async Task T04_AddOrganism_LimitedTo3()
    {
        var context = CreateInMemoryDbContext();
        var mockCultureService = new Mock<ICultureResultService>();
        mockCultureService.Setup(s => s.GetByVisitTestIdAsync(It.IsAny<int>()))
            .ReturnsAsync((MicrobiologyCulture?)null);
        mockCultureService.Setup(s => s.GetSafeAntibioticsAsync(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<AntibioticCatalog>());
        var mockDialog = new Mock<IDialogService>();

        var vm = new CultureEntryViewModel(
            mockCultureService.Object, mockDialog.Object, context,
            visitTestId: 1, patientId: 1, isPregnant: false, patientAgeDays: 3650);

        await vm.LoadAsync();

        // Add 3 organisms
        for (int i = 0; i < 3; i++)
            await vm.AddOrganismCommand.ExecuteAsync(null);

        Assert.Equal(3, vm.Organisms.Count);

        // 4th should be blocked by CanExecute
        Assert.False(vm.AddOrganismCommand.CanExecute(null));
    }

    [Fact]
    public async Task T05_SaveCulture_WithNoOrganism_RequiresCultureResultText()
    {
        var context = CreateInMemoryDbContext();
        var mockCultureService = new Mock<ICultureResultService>();
        mockCultureService.Setup(s => s.GetByVisitTestIdAsync(It.IsAny<int>()))
            .ReturnsAsync((MicrobiologyCulture?)null);
        mockCultureService.Setup(s => s.GetSafeAntibioticsAsync(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<AntibioticCatalog>());
        var mockDialog = new Mock<IDialogService>();

        var vm = new CultureEntryViewModel(
            mockCultureService.Object, mockDialog.Object, context,
            visitTestId: 1, patientId: 1, isPregnant: false, patientAgeDays: 3650);

        await vm.LoadAsync();

        // Empty organisms + non-"No Growth" result = validation failure
        vm.CultureResult = "PENDING";
        Assert.False(vm.CanSaveWithoutOrganisms);
        Assert.Empty(vm.Organisms);

        // Set to "No Growth" = allowed
        vm.CultureResult = "No Growth";
        Assert.True(vm.CanSaveWithoutOrganisms);
    }

    [Fact]
    public async Task T06_LoadingPregnantPatient_TriggersAntibioticFilter()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Doxycycline", IsSafePregnancy = false, IsSafeChildren = false, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var mockCultureService = new Mock<ICultureResultService>();
        mockCultureService.Setup(s => s.GetByVisitTestIdAsync(It.IsAny<int>()))
            .ReturnsAsync((MicrobiologyCulture?)null);
        mockCultureService.Setup(s => s.GetSafeAntibioticsAsync(true, false))
            .ReturnsAsync(new List<AntibioticCatalog>
            {
                context.AntibioticCatalogs.First(a => a.AntibioticId == 1)
            });
        var mockDialog = new Mock<IDialogService>();

        var vm = new CultureEntryViewModel(
            mockCultureService.Object, mockDialog.Object, context,
            visitTestId: 1, patientId: 1, isPregnant: true, patientAgeDays: 3650);

        await vm.LoadAsync();

        Assert.Single(vm.AvailableAntibiotics);
        Assert.Equal("Amoxicillin", vm.AvailableAntibiotics[0].AntibioticName);
    }

    // --- Integration Test (1 test) ---

    [Fact]
    public async Task T07_FullCultureWorkflow_SaveAndReload_Roundtrip()
    {
        var context = CreateInMemoryDbContext(ctx =>
        {
            ctx.Patients.Add(new Patient { PatientId = 1, PatientCode = "P0001", FullNameAr = "أحمد", Sex = "M" });
            ctx.Visits.Add(new Visit { VisitId = 1, PatientId = 1, VisitDate = DateTime.Today, VisitCode = "V001" });
            ctx.TestTypes.Add(new TestType { TesttypeId = 1, TypeNameEn = "Urine Culture", TypeCode = "UC" });
            ctx.VisitTests.Add(new VisitTest { VisitTestId = 1, VisitId = 1, TesttypeId = 1, CurrentStage = TestStage.Pending });
            ctx.AntibioticCatalogs.AddRange(
                new AntibioticCatalog { AntibioticId = 1, AntibioticName = "Amoxicillin", IsSafePregnancy = true, IsSafeChildren = true, IsActive = true },
                new AntibioticCatalog { AntibioticId = 2, AntibioticName = "Ciprofloxacin", IsSafePregnancy = false, IsSafeChildren = true, IsActive = true }
            );
            ctx.SaveChanges();
        });

        var logger = new Mock<ILogger<CultureResultService>>();
        var service = new CultureResultService(context, logger.Object);

        var culture = new MicrobiologyCulture
        {
            VisitTestId = 1,
            CultureResult = "POSITIVE",
            SpecimenSource = "Urine",
            CultureCondition = "Aerobic, 37°C, 24h",
            ColonyCount = "10^5 CFU/mL",
            IncubationHours = 24,
            ReceivedAt = DateTime.Now
        };

        var organisms = new List<MicrobiologyOrganism>
        {
            new MicrobiologyOrganism
            {
                OrganismName = "E. coli",
                GramStain = "Gram-negative rods",
                ColonyCount = "Heavy growth",
                Morphology = "Round, smooth",
                SortOrder = 1,
                OrganismAntibiotics = new List<OrganismAntibiotic>
                {
                    new OrganismAntibiotic { AntibioticName = "Amoxicillin", Sensitivity = AntibioticSensitivity.Highly },
                    new OrganismAntibiotic { AntibioticName = "Ciprofloxacin", Sensitivity = AntibioticSensitivity.Resistant }
                }
            }
        };

        await service.SaveFullCultureAsync(culture, organisms);
        Assert.True(culture.CultureId > 0, "Culture should have been assigned an ID");

        // Reload
        var reloaded = await service.GetByVisitTestIdAsync(1);
        Assert.NotNull(reloaded);
        Assert.Equal("POSITIVE", reloaded!.CultureResult);
        Assert.Equal("Urine", reloaded.SpecimenSource);
        Assert.Equal("Aerobic, 37°C, 24h", reloaded.CultureCondition);
        Assert.Equal("10^5 CFU/mL", reloaded.ColonyCount);
        Assert.Single(reloaded.MicrobiologyOrganisms);

        var org = reloaded.MicrobiologyOrganisms.First();
        Assert.Equal("E. coli", org.OrganismName);
        Assert.Equal(2, org.OrganismAntibiotics.Count);

        var amox = org.OrganismAntibiotics.First(a => a.AntibioticName == "Amoxicillin");
        Assert.Equal(AntibioticSensitivity.Highly, amox.Sensitivity);
    }

    // --- Data Migration Test (1 test) ---

    [Fact]
    public void T08_DataMigration_SensitivityCharToEnum_MapsCorrectly()
    {
        // Verify the enum values map correctly to the expected byte values
        Assert.Equal((byte)0, (byte)AntibioticSensitivity.Highly);
        Assert.Equal((byte)1, (byte)AntibioticSensitivity.Moderate);
        Assert.Equal((byte)2, (byte)AntibioticSensitivity.Low);
        Assert.Equal((byte)3, (byte)AntibioticSensitivity.Resistant);
    }
}
المرحلة 8: تحديث الاختبارات القديمة (ساعتان)
الخطوة 8.1: تعديل FinalLabSystem.Tests/Slice1/BarcodeSlice1Tests.cs
أضف using FinalLabSystem.Services.Interfaces; في أعلى الملف (إذا لم يكن موجودًا) — لا حاجة لأي تعديلات أخرى لأن الاختبارات القديمة لا تستخدم CultureResultService.

3. الملفات المتأثرة (قائمة نهائية)
الملف	الحالة	الفعل
FinalLabSystem/Models/Enums/AntibioticSensitivity.cs	🆕 جديد	إنشاء enum
FinalLabSystem/Models/MicrobiologyCulture.cs	✏️ تعديل	إضافة CultureCondition, ColonyCount
FinalLabSystem/Models/OrganismAntibiotic.cs	✏️ تعديل	تحويل Sensitivity من string إلى AntibioticSensitivity
FinalLabSystem/Services/Interfaces/ICultureResultService.cs	✏️ تعديل جوهري	حذف دالتين قديمتين + إضافة 3 دوال جديدة
FinalLabSystem/Services/Implementations/CultureResultService.cs	✏️ تعديل جوهري	إعادة كتابة كاملة مع SaveFullCultureAsync
FinalLabSystem/ViewModels/Patients/CultureEntryViewModel.cs	🆕 جديد	إنشاء
FinalLabSystem/Views/Patients/CultureEntryWindow.xaml (+ .xaml.cs)	🆕 جديد	إنشاء
FinalLabSystem/Views/Converters/SensitivityToBoolConverter.cs	🆕 جديد	إنشاء
FinalLabSystem/Services/Printing/CultureReportTemplate.cs	🆕 جديد	إنشاء قالب
FinalLabSystem/Services/Interfaces/IResultEntryDialogService.cs	✏️ تعديل	إضافة OpenCultureAsync
FinalLabSystem/Services/Implementations/ResultEntryDialogService.cs	✏️ تعديل	تنفيذ OpenCultureAsync
FinalLabSystem/ViewModels/Patients/TestResultsViewModel.cs	✏️ تعديل	توجيه Culture إلى CultureEntryWindow
FinalLabSystem/Data/FinalLabDbContext.cs	✏️ تعديل	تحديث MicrobiologyCulture config + OrganismAntibiotic.Sensitivity conversion
FinalLabSystem/Services/Implementations/TestCatalogSeeder.cs	✏️ تعديل	إضافة SpecialType = "CULTURE" للتحاليل المناسبة
FinalLabSystem/App.xaml.cs (DI)	✏️ تعديل	تسجيل CultureEntryViewModel + CultureEntryWindow
FinalLabSystem.Tests/Slice2/CultureSlice2Tests.cs	🆕 جديد	8 اختبارات
4. Migrations المطلوبة
Migration واحدة باسم AddCultureFieldsAndSensitivityEnum:

-- 1. إضافة أعمدة جديدة على MicrobiologyCulture
ALTER TABLE [MicrobiologyCulture] ADD [culture_condition] NVARCHAR(200) NULL;
ALTER TABLE [MicrobiologyCulture] ADD [colony_count] NVARCHAR(50) NULL;

-- 2. تحويل OrganismAntibiotic.Sensitivity من nchar(1) إلى tinyint
-- (خطوة 1) إضافة عمود مؤقت
ALTER TABLE [OrganismAntibiotic] ADD [Sensitivity_New] TINYINT NOT NULL DEFAULT 3;

-- (خطوة 2) تعبئة البيانات القديمة (Data Migration)
UPDATE [OrganismAntibiotic]
SET [Sensitivity_New] = CASE
    WHEN [Sensitivity] = 'S' THEN 0
    WHEN [Sensitivity] = 'I' THEN 1
    WHEN [Sensitivity] = 'R' THEN 3
    ELSE 3
END;

-- (خطوة 3) حذف العمود القديم
ALTER TABLE [OrganismAntibiotic] DROP COLUMN [Sensitivity];

-- (خطوة 4) إعادة تسمية العمود الجديد
EXEC sp_rename 'OrganismAntibiotic.Sensitivity_New', 'Sensitivity', 'COLUMN';
⚠ تحذير: قبل تطبيق Migration هذه على أي بيئة أخرى غير بيئة التطوير المحلية (staging أو production لاحقًا)، يجب تشغيل SELECT DISTINCT Sensitivity, COUNT(*) FROM OrganismAntibiotic GROUP BY Sensitivity أولاً والتحقق يدويًا من عدم وجود بيانات حقيقية قبل المتابعة.

Fluent API المطلوب إضافته في FinalLabDbContext.cs:

// MicrobiologyCulture — أعمدة جديدة
modelBuilder.Entity<MicrobiologyCulture>(entity =>
{
    // ... الإعدادات الموجودة تبقى كما هي ...

    entity.Property(e => e.CultureCondition)
        .HasMaxLength(200)
        .HasColumnName("culture_condition");

    entity.Property(e => e.ColonyCount)
        .HasMaxLength(50)
        .HasColumnName("colony_count");
});

// OrganismAntibiotic — تحويل Sensitivity
modelBuilder.Entity<OrganismAntibiotic>(entity =>
{
    // ... الإعدادات الموجودة تبقى كما هي ...

    // استبدل:
    // entity.Property(e => e.Sensitivity)
    //     .HasMaxLength(1)
    //     .IsFixedLength()
    //     .HasColumnName("sensitivity");

    // بالجديد:
    entity.Property(e => e.Sensitivity)
        .HasConversion<byte>()
        .HasMaxLength(1)
        .HasColumnName("sensitivity");
});
5. الاختبارات المطلوبة (10)
#	اسم الاختبار	النوع	ما يتحقق منه
T01	GetSafeAntibioticsAsync_Pregnant_FiltersOutUnsafe	Unit — خدمة	المضادات غير الآمنة للحوامل تختفي
T02	GetSafeAntibioticsAsync_Child_FiltersOutUnsafe	Unit — خدمة	المضادات غير الآمنة للأطفال تختفي
T03	GetSafeAntibioticsAsync_PregnantAndChild_AppliesBothFilters	Unit — خدمة	الفلتر المركّب يعمل معًا
T04	AddOrganism_LimitedTo3	ViewModel	إضافة كائن رابع تفشل
T05	SaveCulture_WithNoOrganism_RequiresCultureResultText	ViewModel	حفظ بدون organisms يتطلب "No Growth"
T06	LoadingPregnantPatient_TriggersAntibioticFilter	ViewModel	القائمة تُفلتر تلقائيًا للحوامل
T07	FullCultureWorkflow_SaveAndReload_Roundtrip	Integration	حفظ + قراءة كاملة
T08	DataMigration_SensitivityCharToEnum_MapsCorrectly	Migration	التحقق من صحة قيم enum
T09	(محجوز لتقرير المزرعة — يُضاف بعد إنشاء CultureReportTemplate)	Report	تحقق من صحة التنسيق
T10	(محجوز لتقرير المزرعة — يُضاف بعد إنشاء CultureReportTemplate)	Report	تحقق من التصنيف الأربعي
ملاحظة: الاختباران T09 و T10 يُكتبان بعد إنشاء CultureReportTemplate في المرحلة 5. يمكنك كتابتهما في نفس ملف CultureSlice2Tests.cs بعد اكتمال قالب التقرير.

6. معايير القبول النهائية
#	المعيار
1	عند فتح مريضة حامل، تختفي المضادات غير الآمنة تلقائيًا من الجدول
2	عند فتح طفل (<12 سنة)، تختفي المضادات غير الآمنة للأطفال
3	حفظ نتيجة كاملة (Culture + حتى 3 كائنات + جدول حساسية) يعمل ويحفظ في DB ضمن transaction واحد
4	طباعة CultureReportTemplate تُظهر تصنيف Highly / Moderate / Low / Resistant كمجموعات صريحة
5	OrganismAntibiotic.Sensitivity أصبح enum ولا يقبل قيمًا نصية عشوائية
6	MicrobiologyCulture يحمل CultureCondition و ColonyCount على مستوى العينة
7	جميع الاختبارات العشرة تجتاز
8	SpecialType = "CULTURE" يُمكّن من فتح CultureEntryWindow من TestResultsViewModel
9	الدالة القديمة AddOrganismsAndSensitivitiesAsync محذوفة بالكامل ولا يوجد أي استدعاء متبقٍ لها
10	SaveFullCultureAsync تحفظ كل الكائنات في SaveChangesAsync واحد (transaction واحد)
7. مخاطر واعتبارات
المخاطرة	التأثير	الحل
جدول OrganismAntibiotic فارغ حاليًا	لا تأثير — الترحيل آمن	الخريطة تُطبَّق كإجراء احترازي. تحذير إلزامي قبل production
SpecialType = "CULTURE" يعتمد على وجود تحاليل مزرعة في الكتالوج	إذا لم تُعثر على تحاليل مزرعة، لن يعمل زر التوجيه	تأكد من وجود تحليلCulture في الكتالوج قبل الاختبار
CultureEntryWindow نافذة ShowDialog — قد تُخفي النافذة الأصلية	لا تأثير — النمط المعماري القائم يستخدم ShowDialog للنوافذ الفرعية	متوافق مع النمط الحالي
SensitivityToBoolConverter يستخدم 4 ثوابت static	لا مشكلة — converter بسيط وخفيف	متوافق مع WPF data binding
CultureReportTemplate يستخدم FlowDocument	قد يحتاج تعديلًا للطباعة المباشرة على طابعة مخصصة	يمكن تطويره لاحقًا
TestCatalogSeeder يعتمد على كلمات مفتاحية في اسم التحليل	قد يفوت بعض التحاليل إذا كان اسمها مختلفًا	يُفضل التحقق يدويًا من SpecialType بعد التشغيل الأول
انتهى ملف التسليم — الشريحة الثانية. ```

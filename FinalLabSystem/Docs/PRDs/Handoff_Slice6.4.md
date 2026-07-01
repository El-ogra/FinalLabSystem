# Handoff — الشريحة 6.4: Report Settings UI

---

## القسم 1 — السياق والقيود

### الهدف

تخصيص شعار/ألوان/خطوط/hargins/T margins والتقارير المطبوعة عبر `ReportSettingsWindow` جديدة، مع إنشاء `IReportLayoutService` لقراءة/حفظ الإعدادات من `LabSetting`، وتعديل `DocumentTemplateBase` بإضافة `ApplyLayout` لتطبيق الإعدادات على الـ `FlowDocument` قبل الطباعة.

### Pre-condition

شريحة 6.3 (Backup UI & Restore Workflow) يجب أن تكون مكتملة بنجاح — Validation Gate G6.3 (644 اختبار ناجح).

### تنبيه إلزامي

> **هذا الملف هو المرجع الوحيد للتنفيذ. لا تُجرِ أي تحليل إضافي. نفّذ كل ما هو مذكور هنا حرفياً.**

### القيود الصارمة

1. لا تعديل على أي ملف خارج نطاق الشريحة 6.4 (المشار إليها بالتفصيل أدناه).
2. لا تغيير في تقنيات المشروع: .NET 8 / WPF / EF Core / SQL Server / MVVM / xUnit + Moq.
3. لا استخدام Repository Pattern.
4. لا تفكيك `FinalLabDbContext` أو `VisitService`.
5. لا ذكر لـ Natigh في أي ملف.
6. `DateTime.UtcNow` في كل seed data و assertions.
7. `Sex = "M"` في كل Patient/Staff seed افتراضي.
8. `It.IsAny<>()` في كل Moq setup للمعاملات غير الحرجة.
9. الـ code-behind لكل Window يحتوي فقط على `InitializeComponent()` + constructor.

---

## القسم 2 — جدول الملفات الكامل (تسلسل التنفيذ)

### الملف 1 — إنشاء `FinalLabSystem/Infrastructure/Constants/ReportSettingKeys.cs`

**الإجراء:** إنشاء
**التغيير عن الخطة الأصلية:** تم توسيع القائمة من 12 ثابتاً إلى **22 ثابتاً** لإضافة أبعاد الشعار، هامش لكل جانب، اتجاه الصفحة، حجم الورقة، ونص التذييل.

```csharp
namespace FinalLabSystem.Infrastructure.Constants;

public static class ReportSettingKeys
{
    // Branding
    public const string LabNameAr = "Report.LabNameAr";
    public const string LabNameEn = "Report.LabNameEn";
    public const string LogoPath = "Report.LogoPath";

    // Logo sizing
    public const string LogoWidth = "Report.LogoWidth";
    public const string LogoHeight = "Report.LogoHeight";

    // Colors
    public const string PrimaryColor = "Report.PrimaryColor";
    public const string SecondaryColor = "Report.SecondaryColor";

    // Typography
    public const string FontFamily = "Report.FontFamily";
    public const string FontSize = "Report.FontSize";
    public const string HeaderFontSize = "Report.HeaderFontSize";
    public const string FooterFontSize = "Report.FooterFontSize";

    // Page margins (per-side)
    public const string MarginTop = "Report.MarginTop";
    public const string MarginBottom = "Report.MarginBottom";
    public const string MarginLeft = "Report.MarginLeft";
    public const string MarginRight = "Report.MarginRight";

    // Section visibility
    public const string ShowHeader = "Report.ShowHeader";
    public const string ShowFooter = "Report.ShowFooter";
    public const string ShowStamp = "Report.ShowStamp";

    // Section text
    public const string HeaderText = "Report.HeaderText";
    public const string FooterText = "Report.FooterText";

    // Page setup
    public const string PageOrientation = "Report.PageOrientation";
    public const string PaperSize = "Report.PaperSize";
}
التحذير: كل ثابت يبدأ ببادئة "Report." لمنع التعارض مع مفاتيح LabSetting الأخرى.

الملف 2 — إنشاء FinalLabSystem/Models/DTOs/ReportLayoutDto.cs
الإجراء: إنشاء التغيير عن الخطة الأصلية: تم توسيع الـ DTO من 12 حقل إلى 22 حقلاً ليتناسب مع القائمة الموسَّعة.

namespace FinalLabSystem.Models.DTOs;

public class ReportLayoutDto
{
    // Branding
    public string? LabNameAr { get; set; }
    public string? LabNameEn { get; set; }
    public string? LogoPath { get; set; }

    // Logo sizing (cm)
    public decimal? LogoWidth { get; set; }
    public decimal? LogoHeight { get; set; }

    // Colors (hex: "#RRGGBB")
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }

    // Typography
    public string? FontFamily { get; set; }
    public double FontSize { get; set; } = 12;
    public double HeaderFontSize { get; set; } = 16;
    public double FooterFontSize { get; set; } = 10;

    // Page margins (cm)
    public decimal MarginTop { get; set; } = 2;
    public decimal MarginBottom { get; set; } = 2;
    public decimal MarginLeft { get; set; } = 2;
    public decimal MarginRight { get; set; } = 2;

    // Section visibility
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public bool ShowStamp { get; set; } = false;

    // Section text
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }

    // Page setup
    public string PageOrientation { get; set; } = "Portrait";
    public string PaperSize { get; set; } = "A4";
}
الملف 3 — تعديل FinalLabSystem/Models/LabSetting.cs
الإجراء: تعديل التغيير عن الخطة الأصلية: تم التحول من key-value store إلى typed columns (~20 حقل جديد) بدلاً من 12 صف key-value. هذا يتبع precedent SMTP/Backup من الشريحة 2.2.

الأعمدة الجديدة تُضاف بعد BackupOutputFolder (سطر 34):

// === Report Layout Settings ===
public string? ReportLabNameAr { get; set; }
public string? ReportLabNameEn { get; set; }
public string? ReportLogoPath { get; set; }
public decimal? ReportLogoWidth { get; set; }
public decimal? ReportLogoHeight { get; set; }
public string? ReportPrimaryColor { get; set; }
public string? ReportSecondaryColor { get; set; }
public string? ReportFontFamily { get; set; }
public double ReportFontSize { get; set; } = 12;
public double ReportHeaderFontSize { get; set; } = 16;
public double ReportFooterFontSize { get; set; } = 10;
public decimal ReportMarginTop { get; set; } = 2;
public decimal ReportMarginBottom { get; set; } = 2;
public decimal ReportMarginLeft { get; set; } = 2;
public decimal ReportMarginRight { get; set; } = 2;
public bool ReportShowHeader { get; set; } = true;
public bool ReportShowFooter { get; set; } = true;
public bool ReportShowStamp { get; set; } = false;
public string? ReportHeaderText { get; set; }
public string? ReportFooterText { get; set; }
public string? ReportPageOrientation { get; set; }
public string? ReportPaperSize { get; set; }
التحذير: الـ 22 خاصية جديدة تبدأ ببادئة Report لمنع التعارض مع الأعمدة الموجودة (SmtpHost, BackupOutputFolder, إلخ).

الملف 4 — تعديل FinalLabSystem/Data/FinalLabDbContext.cs
الPROCEDURE: تعديل المحتوى: إضافة Fluent API mapping في OnModelCreating لكل حقل جديد.

// Report Layout Settings
modelBuilder.Entity<LabSetting>(entity =>
{
    entity.Property(e => e.ReportLabNameAr).HasColumnName("ReportLabNameAr").HasMaxLength(200);
    entity.Property(e => e.ReportLabNameEn).HasColumnName("ReportLabNameEn").HasMaxLength(200);
    entity.Property(e => e.ReportLogoPath).HasColumnName("ReportLogoPath").HasMaxLength(500);
    entity.Property(e => e.ReportLogoWidth).HasColumnName("ReportLogoWidth");
    entity.Property(e => e.ReportLogoHeight).HasColumnName("ReportLogoHeight");
    entity.Property(e => e.ReportPrimaryColor).HasColumnName("ReportPrimaryColor").HasMaxLength(7);
    entity.Property(e => e.ReportSecondaryColor).HasColumnName("ReportSecondaryColor").HasMaxLength(7);
    entity.Property(e => e.ReportFontFamily).HasColumnName("ReportFontFamily").HasMaxLength(100);
    entity.Property(e => e.ReportFontSize).HasColumnName("ReportFontSize").HasDefaultValue(12.0);
    entity.Property(e => e.ReportHeaderFontSize).HasColumnName("ReportHeaderFontSize").HasDefaultValue(16.0);
    entity.Property(e => e.ReportFooterFontSize).HasColumnName("ReportFooterFontSize").HasDefaultValue(10.0);
    entity.Property(e => e.ReportMarginTop).HasColumnName("ReportMarginTop").HasDefaultValue(2m);
    entity.Property(e => e.ReportMarginBottom).HasColumnName("ReportMarginBottom").HasDefaultValue(2m);
    entity.Property(e => e.ReportMarginLeft).HasColumnName("ReportMarginLeft").HasDefaultValue(2m);
    entity.Property(e => e.ReportMarginRight).HasColumnName("ReportMarginRight").HasDefaultValue(2m);
    entity.Property(e => e.ReportShowHeader).HasColumnName("ReportShowHeader").HasDefaultValue(true);
    entity.Property(e => e.ReportShowFooter).HasColumnName("ReportShowFooter").HasDefaultValue(true);
    entity.Property(e => e.ReportShowStamp).HasColumnName("ReportShowStamp").HasDefaultValue(false);
    entity.Property(e => e.ReportHeaderText).HasColumnName("ReportHeaderText").HasMaxLength(500);
    entity.Property(e => e.ReportFooterText).HasColumnName("ReportFooterText").HasMaxLength(500);
    entity.Property(e => e.ReportPageOrientation).HasColumnName("ReportPageOrientation").HasMaxLength(20);
    entity.Property(e => e.ReportPaperSize).HasColumnName("ReportPaperSize").HasMaxLength(20);
});
التحذير: يجب إضافة هذه الكتلة داخل OnModelCreating بعد الكتل الموجودة. تحقق من عدم تكرار modelBuilder.Entity<LabSetting>().

الملف 5 — إنشاء FinalLabSystem/Migrations/20260701010000_AddReportLayoutColumns.cs
الإجراء: إنشاء التغيير عن الخطة الأصلية: Migration جديدة لإضافة typed columns بدل key-value.

Migration class name: AddReportLayoutColumns

public partial class AddReportLayoutColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Branding
        migrationBuilder.AddColumn<string>(
            name: "ReportLabNameAr",
            table: "LabSettings",
            type: "nvarchar(200)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportLabNameEn",
            table: "LabSettings",
            type: "nvarchar(200)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportLogoPath",
            table: "LabSettings",
            type: "nvarchar(500)",
            nullable: true);

        // Logo sizing
        migrationBuilder.AddColumn<decimal>(
            name: "ReportLogoWidth",
            table: "LabSettings",
            type: "decimal(5,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "ReportLogoHeight",
            table: "LabSettings",
            type: "decimal(5,2)",
            nullable: true);

        // Colors
        migrationBuilder.AddColumn<string>(
            name: "ReportPrimaryColor",
            table: "LabSettings",
            type: "nvarchar(7)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportSecondaryColor",
            table: "LabSettings",
            type: "nvarchar(7)",
            nullable: true);

        // Typography
        migrationBuilder.AddColumn<string>(
            name: "ReportFontFamily",
            table: "LabSettings",
            type: "nvarchar(100)",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "ReportFontSize",
            table: "LabSettings",
            type: "float",
            nullable: false,
            defaultValue: 12.0);

        migrationBuilder.AddColumn<double>(
            name: "ReportHeaderFontSize",
            table: "LabSettings",
            type: "float",
            nullable: false,
            defaultValue: 16.0);

        migrationBuilder.AddColumn<double>(
            name: "ReportFooterFontSize",
            table: "LabSettings",
            type: "float",
            nullable: false,
            defaultValue: 10.0);

        // Page margins
        migrationBuilder.AddColumn<decimal>(
            name: "ReportMarginTop",
            table: "LabSettings",
            type: "decimal(5,2)",
            nullable: false,
            defaultValue: 2m);

        migrationBuilder.AddColumn<decimal>(
            name: "ReportMarginBottom",
            table: "LabSettings",
            type: "decimal(5,2)",
            nullable: false,
            defaultValue: 2m);

        migrationBuilder.AddColumn<decimal>(
            name: "ReportMarginLeft",
            table: "LabSettings",
            type: "decimal(5,2)",
            nullable: false,
            defaultValue: 2m);

        migrationBuilder.AddColumn<decimal>(
            name: "ReportMarginRight",
            table: "LabSettings",
            type: "decimal(5,2)",
            nullable: false,
            defaultValue: 2m);

        // Section visibility
        migrationBuilder.AddColumn<bool>(
            name: "ReportShowHeader",
            table: "LabSettings",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<bool>(
            name: "ReportShowFooter",
            table: "LabSettings",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<bool>(
            name: "ReportShowStamp",
            table: "LabSettings",
            type: "bit",
            nullable: false,
            defaultValue: false);

        // Section text
        migrationBuilder.AddColumn<string>(
            name: "ReportHeaderText",
            table: "LabSettings",
            type: "nvarchar(500)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportFooterText",
            table: "LabSettings",
            type: "nvarchar(500)",
            nullable: true);

        // Page setup
        migrationBuilder.AddColumn<string>(
            name: "ReportPageOrientation",
            table: "LabSettings",
            type: "nvarchar(20)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReportPaperSize",
            table: "LabSettings",
            type: "nvarchar(20)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ReportLabNameAr", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportLabNameEn", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportLogoPath", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportLogoWidth", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportLogoHeight", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportPrimaryColor", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportSecondaryColor", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportFontFamily", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportFontSize", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportHeaderFontSize", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportFooterFontSize", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportMarginTop", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportMarginBottom", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportMarginLeft", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportMarginRight", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportShowHeader", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportShowFooter", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportShowStamp", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportHeaderText", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportFooterText", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportPageOrientation", table: "LabSettings");
        migrationBuilder.DropColumn(name: "ReportPaperSize", table: "LabSettings");
    }
}
التحذير: تحقق من schema قبل dotnet ef database update. شغّل dotnet ef migrations script للتأكد من عدم وجود أخطاء.

الملف 6 — تعديل FinalLabSystem/Services/Printing/DocumentTemplateBase.cs
الإجراء: تعديل التغيير عن الخطة الأصلية: التوقيع تُصَحَّح من ApplyLayout(ReportLayoutDto? layout) إلى ApplyLayout(FlowDocument document, ReportLayoutDto? layout) — هذا تصحيح عيب تصميمي حرج (انظر القسم 3 من التقرير التحليلي).

المحتوى الكامل للملف بعد التعديل:

using System.Windows.Documents;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public abstract class DocumentTemplateBase
{
    public abstract FlowDocument BuildDocument(object data);

    protected virtual Paragraph CreateHeader(string title)
    {
        return new Paragraph(new Run(title))
        {
            FontSize = 16,
            FontWeight = System.Windows.FontWeights.Bold,
            TextAlignment = System.Windows.TextAlignment.Center
        };
    }

    protected virtual Paragraph CreateFooter(string text)
    {
        return new Paragraph(new Run(text))
        {
            FontSize = 10,
            TextAlignment = System.Windows.TextAlignment.Center,
            Margin = new System.Windows.Thickness(0, 20, 0, 0)
        };
    }

    /// <summary>
    /// يُستدعى بعد BuildDocument وليس قبله.
    /// الـ default: no-op (لا يُغيّر المستند).
    /// الـ overrides تُعدّل الـ FlowDocument المُمرَّر مباشرة.
    /// </summary>
    protected virtual void ApplyLayout(FlowDocument document, ReportLayoutDto? layout)
    {
        // Default no-op: لا يُغيّر سلوك أي template قائمة.
    }
}
⚠️ تحذير حرج: ApplyLayout يُستدعى بعد BuildDocument وليس قبله. الـ FlowDocument يكون جاهزاً ويُمرَّر كمعامل أول. أي override يجب أن يُعدّل الـ document مباشرة.

الملف 7 — تعديل FinalLabSystem/Services/Printing/ReceiptTemplate.cs
الإجراء: تعديل المنطق: override ApplyLayout لتطبيق الإعدادات على الـ FlowDocument.

using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class ReceiptTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("إيصال استلام"));
        doc.Blocks.Add(new Paragraph(new Run("جاري إنشاء الإيصال...")));
        return doc;
    }

    protected override void ApplyLayout(FlowDocument document, ReportLayoutDto? layout)
    {
        if (layout is null || document is null) return;

        // Typography
        if (!string.IsNullOrWhiteSpace(layout.FontFamily))
            document.FontFamily = new FontFamily(layout.FontFamily);

        if (layout.FontSize > 0)
            document.FontSize = layout.FontSize;

        // Margins (cm → WPF points: 1 cm ≈ 28.35 points)
        document.PagePadding = new System.Windows.Thickness(
            layout.MarginLeft * 28.35,
            layout.MarginTop * 28.35,
            layout.MarginRight * 28.35,
            layout.MarginBottom * 28.35);

        // Colors on header paragraphs
        var primaryBrush = !string.IsNullOrWhiteSpace(layout.PrimaryColor)
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(layout.PrimaryColor))
            : null;

        if (primaryBrush is not null)
        {
            foreach (var block in document.Blocks)
            {
                if (block is Paragraph para)
                    para.Foreground = primaryBrush;
            }
        }
    }
}
ملاحظة: تحويل cm إلى WPF points يتم عبر × 28.35.

الملف 8 — تعديل FinalLabSystem/Services/Printing/ResultReportTemplate.cs
الإجراء: تعديل المنطق: نفس نمط ReceiptTemplate مع تطبيق ألوان ثانوية أيضاً.

using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class ResultReportTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        var doc = new FlowDocument
        {
            FlowDirection = System.Windows.FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };
        doc.Blocks.Add(CreateHeader("تقرير نتائج"));
        doc.Blocks.Add(new Paragraph(new Run("جاري إنشاء التقرير...")));
        return doc;
    }

    protected override void ApplyLayout(FlowDocument document, ReportLayoutDto? layout)
    {
        if (layout is null || document is null) return;

        // Typography
        if (!string.IsNullOrWhiteSpace(layout.FontFamily))
            document.FontFamily = new FontFamily(layout.FontFamily);

        if (layout.FontSize > 0)
            document.FontSize = layout.FontSize;

        // Page margins
        document.PagePadding = new System.Windows.Thickness(
            layout.MarginLeft * 28.35,
            layout.MarginTop * 28.35,
            layout.MarginRight * 28.35,
            layout.MarginBottom * 28.35);

        // Primary color on all paragraphs
        var primaryBrush = !string.IsNullOrWhiteSpace(layout.PrimaryColor)
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(layout.PrimaryColor))
            : null;

        if (primaryBrush is not null)
        {
            foreach (var block in document.Blocks)
            {
                if (block is Paragraph para)
                    para.Foreground = primaryBrush;
            }
        }
    }
}
الملف 9 — تعديل FinalLabSystem/Services/Implementations/WpfFlowDocumentPrintService.cs
الإجراء: تعديل المنطق: إضافة overload جديد + تعديل الـ switch ليمرر ReportLayoutDto? اختيارياً.

التدفق الجديد للـ overload:

// الخطوة 1: بناء المستند أولاً
var doc = template.BuildDocument(data);
// الخطوة 2: تطبيق التخطيط ثانياً (post-processing)
template.ApplyLayout(doc, layout);
// الخطوة 3: الطباعة أخيراً
await PrintDocumentAsync(documentType, doc);
المحتوى الكامل للملف بعد التعديل:

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Services.Printing;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class WpfFlowDocumentPrintService : IPrintService
{
    private readonly ILogger<WpfFlowDocumentPrintService> _logger;
    private readonly IFeatureToggleService _featureToggleService;

    public WpfFlowDocumentPrintService(
        ILogger<WpfFlowDocumentPrintService> logger,
        IFeatureToggleService featureToggleService)
    {
        _logger = logger;
        _featureToggleService = featureToggleService;
    }

    // === الـ overload الأصلي (بدون layout) — لا يتغير ===
    public async Task PrintAsync(string documentType, object data)
    {
        if (string.IsNullOrEmpty(documentType))
            throw new ArgumentNullException(nameof(documentType));
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (!await IsPrintingEnabledAsync())
        {
            _logger.LogInformation("Printing disabled by EnableServerPrinting toggle. Document type: {DocType}", documentType);
            return;
        }

        DocumentTemplateBase template = ResolveTemplate(documentType);
        var document = template.BuildDocument(data);
        await PrintDocumentAsync(documentType, document);
    }

    // === الـ overload الجديد (مع layout) ===
    public async Task PrintAsync(string documentType, object data, ReportLayoutDto? layout)
    {
        if (string.IsNullOrEmpty(documentType))
            throw new ArgumentNullException(nameof(documentType));
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (!await IsPrintingEnabledAsync())
        {
            _logger.LogInformation("Printing disabled by EnableServerPrinting toggle. Document type: {DocType}", documentType);
            return;
        }

        DocumentTemplateBase template = ResolveTemplate(documentType);

        // ⚠️ التدفق الصحيح: BuildDocument أولاً، ثم ApplyLayout
        var document = template.BuildDocument(data);
        template.ApplyLayout(document, layout);

        await PrintDocumentAsync(documentType, document);
    }

    public async Task PrintFlowDocumentAsync(FlowDocument document, string description)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrEmpty(description))
            throw new ArgumentNullException(nameof(description));

        if (!await IsPrintingEnabledAsync())
        {
            _logger.LogInformation("Printing disabled by EnableServerPrinting toggle.");
            return;
        }

        await ShowPrintDialogAndPrintAsync(document, description);
    }

    private DocumentTemplateBase ResolveTemplate(string documentType)
    {
        return documentType switch
        {
            "ResultReport" => new ResultReportTemplate(),
            "Receipt" => new ReceiptTemplate(),
            "CompositeReport" => new CompositeReportTemplate(),
            "Worksheet" => new WorksheetTemplate(),
            "Envelope" => new EnvelopeTemplate(),
            "MedicalHistory" => new MedicalHistoryTemplate(),
            "BlankReport" => new BlankReportTemplate(),
            "CashDrawerSummary" => new CashDrawerSummaryTemplate(),
            "CommissionReport" => new CommissionReportTemplate(),
            "OutstandingBalance" => new OutstandingBalanceReportTemplate(),
            _ => throw new NotSupportedException($"Document type '{documentType}' is not supported.")
        };
    }

    protected virtual async Task PrintDocumentAsync(string documentType, FlowDocument document)
    {
        await ShowPrintDialogAndPrintAsync(document, documentType);
    }

    protected virtual async Task ShowPrintDialogAndPrintAsync(FlowDocument document, string description)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                var docSource = (IDocumentPaginatorSource)document;
                dlg.PrintDocument(docSource.DocumentPaginator, description);
                _logger.LogInformation("Printed document: {Description}", description);
            }
        });
    }

    private async Task<bool> IsPrintingEnabledAsync()
    {
        return await _featureToggleService.IsEnabledAsync(FeatureToggles.EnableServerPrinting, false);
    }
}
ملاحظة: تم استخراج ResolveTemplate إلى method منفصل لتقليل التكرار بين الـ overloads. الـ switch expression الأصلي يتحول إلى method call.

الملف 10 — تعديل FinalLabSystem/Services/Interfaces/ISettingsService.cs
الإجراء: لا تعديل — الواجهة الحالية كافية. IReportLayoutService يتعامل مع FinalLabDbContext مباشرة (نمط FeatureToggleService) ولا يمر بـ ISettingsService.

الملف 10 (الأصلي) — إنشاء FinalLabSystem/Services/Interfaces/IReportLayoutService.cs
الإجراء: إنشاء

using FinalLabSystem.Models.DTOs;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IReportLayoutService
{
    Task<ReportLayoutDto> GetCurrentLayoutAsync();
    Task SaveLayoutAsync(ReportLayoutDto layout, int staffId);
    Task ResetToDefaultsAsync();
    ReportLayoutDto GetDefaults();
}
الملف 11 — إنشاء FinalLabSystem/Services/Implementations/ReportLayoutService.cs
الإجراء: إنشاء المنطق: يقرأ/يكتب الأعمدة المُنوّقة مباشرة من LabSetting عبر FinalLabDbContext (لا يمر بـ ISettingsService).

using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class ReportLayoutService : IReportLayoutService
{
    private readonly FinalLabDbContext _context;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReportLayoutService> _logger;

    public ReportLayoutService(
        FinalLabDbContext context,
        ICurrentUserSession currentUserSession,
        IAuditService auditService,
        ILogger<ReportLayoutService> logger)
    {
        _context = context;
        _currentUserSession = currentUserSession;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ReportLayoutDto> GetCurrentLayoutAsync()
    {
        var setting = await _context.LabSettings.FirstOrDefaultAsync();
        if (setting is null)
            return GetDefaults();

        return new ReportLayoutDto
        {
            LabNameAr = setting.ReportLabNameAr,
            LabNameEn = setting.ReportLabNameEn,
            LogoPath = setting.ReportLogoPath,
            LogoWidth = setting.ReportLogoWidth,
            LogoHeight = setting.ReportLogoHeight,
            PrimaryColor = setting.ReportPrimaryColor,
            SecondaryColor = setting.ReportSecondaryColor,
            FontFamily = setting.ReportFontFamily,
            FontSize = setting.ReportFontSize,
            HeaderFontSize = setting.ReportHeaderFontSize,
            FooterFontSize = setting.ReportFooterFontSize,
            MarginTop = setting.ReportMarginTop,
            MarginBottom = setting.ReportMarginBottom,
            MarginLeft = setting.ReportMarginLeft,
            MarginRight = setting.ReportMarginRight,
            ShowHeader = setting.ReportShowHeader,
            ShowFooter = setting.ReportShowFooter,
            ShowStamp = setting.ReportShowStamp,
            HeaderText = setting.ReportHeaderText,
            FooterText = setting.ReportFooterText,
            PageOrientation = setting.ReportPageOrientation,
            PaperSize = setting.ReportPaperSize
        };
    }

    public async Task SaveLayoutAsync(ReportLayoutDto layout, int staffId)
    {
        var setting = await _context.LabSettings.FirstOrDefaultAsync();
        if (setting is null)
        {
            setting = new LabSetting();
            _context.LabSettings.Add(setting);
        }

        setting.ReportLabNameAr = layout.LabNameAr;
        setting.ReportLabNameEn = layout.LabNameEn;
        setting.ReportLogoPath = layout.LogoPath;
        setting.ReportLogoWidth = layout.LogoWidth;
        setting.ReportLogoHeight = layout.LogoHeight;
        setting.ReportPrimaryColor = layout.PrimaryColor;
        setting.ReportSecondaryColor = layout.SecondaryColor;
        setting.ReportFontFamily = layout.FontFamily;
        setting.ReportFontSize = layout.FontSize;
        setting.ReportHeaderFontSize = layout.HeaderFontSize;
        setting.ReportFooterFontSize = layout.FooterFontSize;
        setting.ReportMarginTop = layout.MarginTop;
        setting.ReportMarginBottom = layout.MarginBottom;
        setting.ReportMarginLeft = layout.MarginLeft;
        setting.ReportMarginRight = layout.MarginRight;
        setting.ReportShowHeader = layout.ShowHeader;
        setting.ReportShowFooter = layout.ShowFooter;
        setting.ReportShowStamp = layout.ShowStamp;
        setting.ReportHeaderText = layout.HeaderText;
        setting.ReportFooterText = layout.FooterText;
        setting.ReportPageOrientation = layout.PageOrientation;
        setting.ReportPaperSize = layout.PaperSize;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ReportSettingsUpdated", staffId, DateTime.UtcNow);
        _logger.LogInformation("Report layout settings saved by staff {StaffId}", staffId);
    }

    public async Task ResetToDefaultsAsync()
    {
        var defaults = GetDefaults();
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        await SaveLayoutAsync(defaults, staffId);
    }

    public ReportLayoutDto GetDefaults()
    {
        return new ReportLayoutDto
        {
            LabNameAr = null,
            LabNameEn = null,
            LogoPath = null,
            LogoWidth = null,
            LogoHeight = null,
            PrimaryColor = "#000000",
            SecondaryColor = "#FFFFFF",
            FontFamily = "Segoe UI",
            FontSize = 12,
            HeaderFontSize = 16,
            FooterFontSize = 10,
            MarginTop = 2,
            MarginBottom = 2,
            MarginLeft = 2,
            MarginRight = 2,
            ShowHeader = true,
            ShowFooter = true,
            ShowStamp = false,
            HeaderText = null,
            FooterText = null,
            PageOrientation = "Portrait",
            PaperSize = "A4"
        };
    }
}
الملف 12 — إنشاء FinalLabSystem/ViewModels/Settings/ReportSettingsWindowViewModel.cs
الإجراء: إنشاء

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class ReportSettingsWindowViewModel : ViewModelBase
{
    private readonly IReportLayoutService _reportLayoutService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserSession _currentUserSession;

    private ReportLayoutDto _currentLayout = new();
    private bool _isBusy;

    public ReportSettingsWindowViewModel(
        IReportLayoutService reportLayoutService,
        IDialogService dialogService,
        ICurrentUserSession currentUserSession)
    {
        _reportLayoutService = reportLayoutService;
        _dialogService = dialogService;
        _currentUserSession = currentUserSession;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
        ResetToDefaultsCommand = new AsyncRelayCommand(ResetToDefaultsAsync, () => !IsBusy);
        BrowseLogoCommand = new RelayCommand(BrowseLogo);
        PreviewCommand = new AsyncRelayCommand(PreviewAsync);
    }

    public ReportLayoutDto CurrentLayout
    {
        get => _currentLayout;
        set { _currentLayout = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand BrowseLogoCommand { get; }
    public ICommand PreviewCommand { get; }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            CurrentLayout = await _reportLayoutService.GetCurrentLayoutAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            var staffId = _currentUserSession.CurrentUser?.StaffId
                ?? throw new InvalidOperationException("لا يمكن الحفظ بدون جلسة مستخدم نشطة.");
            await _reportLayoutService.SaveLayoutAsync(CurrentLayout, staffId);
            _dialogService.ShowInfo("تم حفظ الإعدادات بنجاح.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResetToDefaultsAsync()
    {
        if (!_dialogService.Confirm("هل أنت متأكد من إعادة جميع الإعدادات إلى القيم الافتراضية؟"))
            return;

        IsBusy = true;
        try
        {
            await _reportLayoutService.ResetToDefaultsAsync();
            CurrentLayout = _reportLayoutService.GetDefaults();
            _dialogService.ShowInfo("تمت إعادة الإعدادات إلى القيم الافتراضية.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BrowseLogo()
    {
        var path = _dialogService.OpenFileDialog("PNG Files|*.png|JPG Files|*.jpg|All Files|*.*");
        if (!string.IsNullOrEmpty(path))
        {
            CurrentLayout.LogoPath = path;
            OnPropertyChanged(nameof(CurrentLayout));
        }
    }

    private async Task PreviewAsync()
    {
        // بناء مستند معاينة بسيط بالإعدادات الحالية
        await Task.CompletedTask;
        _dialogService.ShowInfo("معاينة التقرير: الإعدادات الحالية ستُطبَّع على التقارير.");
    }
}
الملف 13 — إنشاء FinalLabSystem/Views/Settings/ReportSettingsWindow.xaml + .cs
الإجراء: إنشاء الـ code-behind يحتوي فقط على:

namespace FinalLabSystem.Views.Settings;

public partial class ReportSettingsWindow : System.Windows.Window
{
    public ReportSettingsWindow()
    {
        InitializeComponent();
    }
}
الـ XAML: نافذة form مع حقول إدخال لكل الـ 22 خاصية + زر Save + زر Reset + زر Browse Logo + زر Preview.

الملف 14 — تعديل FinalLabSystem/ViewModels/Menu/ReportSettingsMenuViewModel.cs
الإجراء: تعديل المنطق: الحفاظ على ManageTemplatesCommand مع إضافة NavigateToReportSettingsCommand.

using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class ReportSettingsMenuViewModel : ViewModelBase
{
    public ReportSettingsMenuViewModel(INavigationService navigationService)
    {
        ManageTemplatesCommand = new RelayCommand(_ =>
            navigationService.OpenTaskWindow<ReportCommentTemplateViewModel>());

        NavigateToReportSettingsCommand = new RelayCommand(_ =>
            navigationService.OpenTaskWindow<ReportSettingsWindowViewModel>());
    }

    public ICommand ManageTemplatesCommand { get; }

    public ICommand NavigateToReportSettingsCommand { get; }
}
⚠️ تحذير: يجب أن يبقى ManageTemplatesCommand كما هو بالضبط (لا يُحذف).

الملف 15 — تعديل FinalLabSystem/MainWindow.xaml
الإجراء: تعديل المنطق: استبدال الـ DataTemplate الـ placeholder لـ ReportSettingsMenuViewModel بأزرار فعلية.

الـ DataTemplate الحالي (يُحذف):

<!-- Report Settings placeholder — يُستبدل -->
<DataTemplate DataType="{x:Type menu:ReportSettingsMenuViewModel}">
    <Grid Background="#F5F7FA">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <TextBlock Text="إعدادات التقارير" FontSize="24" FontWeight="Bold"
                       Foreground="#666" Margin="0,0,0,12"/>
            <TextBlock Text="سيتم تفعيل هذه الميزة في المرحلة 6"
                       FontSize="16" Foreground="#999" Margin="0,0,0,20"/>
            <Button Content="العودة للرئيسية"
                    Command="{Binding DataContext.ShowHomeMenuCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                    Width="150" Height="36" FontSize="14"/>
        </StackPanel>
    </Grid>
</DataTemplate>
الـ DataTemplate الجديد (يُحلّ محله):

<DataTemplate DataType="{x:Type menu:ReportSettingsMenuViewModel}">
    <Grid Background="#F5F7FA" Margin="20">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Width="300">
            <TextBlock Text="إعدادات التقارير" FontSize="24" FontWeight="Bold"
                       Foreground="#333" Margin="0,0,0,20" TextAlignment="Center"/>

            <Button Content="إدارة قالب التعليقات"
                    Command="{Binding ManageTemplatesCommand}"
                    Width="250" Height="40" FontSize="14" Margin="0,0,0,10"
                    HorizontalAlignment="Center"/>

            <Button Content="إعدادات التنسيق والتخطيط"
                    Command="{Binding NavigateToReportSettingsCommand}"
                    Width="250" Height="40" FontSize="14" Margin="0,0,0,20"
                    HorizontalAlignment="Center"/>

            <Button Content="العودة للرئيسية"
                    Command="{Binding DataContext.ShowHomeMenuCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                    Width="150" Height="36" FontSize="14"
                    HorizontalAlignment="Center"/>
        </StackPanel>
    </Grid>
</DataTemplate>
الملف 16 — تعديل FinalLabSystem/App.xaml.cs
الإجراء: تعديل

تسجيلات DI الثلاثة:

// Scoped: يستخدم FinalLabDbContext
services.AddScoped<IReportLayoutService, ReportLayoutService>();

// Transient: VM بـ state مستقل
services.AddTransient<ReportSettingsWindowViewModel>();

// Transient: نمط Windows
services.AddTransient<ReportSettingsWindow>();
تسجيل Navigation:

navigation.RegisterWindow<ReportSettingsWindowViewModel, ReportSettingsWindow>();
القسم 3 — تسجيلات DI الكاملة
العنصر	Lifetime	المبرر
IReportLayoutService → ReportLayoutService	Scoped	يستخدم FinalLabDbContext المُسجَّل Scoped — يتبع نمط IBackupService و IDeliveryConfirmationService.
ReportSettingsWindowViewModel	Transient	كل نافذة بـ state مستقل (CurrentLayout collection). يتبع نمط كل VMs في المشروع.
ReportSettingsWindow	Transient	نمط Windows — يتبع BackupRestoreWindow, PrintQueueWindow.
القسم 4 — Migration المطلوبة
الاسم المقترح
20260701010000_AddReportLayoutColumns

محتوى Up() الكامل
انظر الملف 5 أعلاه — 22 AddColumn بالضبط.

محتوى Down() العكسي
22 DropColumn بترتيب عكسي (من ReportPaperSize إلى ReportLabNameAr).

تحذير
تحقق من schema قبل dotnet ef database update. شغّل dotnet ef migrations script للتأكد من عدم وجود أخطاء. تأكد من أن LabSetting لا يحتوي أعمدة مكررة.

القسم 5 — جدول الاختبارات الكامل (25 اختباراً)
#	ملف الاختبار	اسم الاختبار	ما يختبره بالضبط	Seed Data
1	Tests/Services/ReportLayoutServiceTests.cs	GetCurrentLayoutAsync_NoSettings_ReturnsDefaults	لو لا يوجد LabSetting row يُرجع defaults	InMemory فارغ
2	نفسه	SaveLayoutAsync_PersistsAllFields	حفظ 22 حقل والقراءة تُطابق	DTO بكل الـ 22 حقلاً
3	نفسه	GetCurrentLayoutAsync_PartialSettings_MergesWithDefaults	3 حقول فقط مملوءة → الباقي defaults	LabSetting بـ 3 حقول
4	نفسه	ResetToDefaultsAsync_ResetsAllReportFields	كل الحقول تعود للقيم الافتراضية	settings مخصَّصة
5	نفسه	GetDefaults_ReturnsValidDto	sanity check على الـ defaults	—
6	نفسه	SaveLayoutAsync_LogsAuditEvent	IAuditService.LogAsync("ReportSettingsUpdated", ...)	Mock IAuditService
7	Tests/ViewModels/Settings/ReportSettingsWindowViewModelTests.cs	LoadCommand_PopulatesAllFields	CurrentLayout مملوء من الخدمة	mock layout
8	نفسه	SaveCommand_CallsService_WithCurrentLayout	_reportLayoutService.SaveLayoutAsync(It.IsAny<>(), It.IsAny<int>())	mock
9	نفسه	ResetCommand_RestoresDefaults	CurrentLayout يعود للـ defaults بعد reset	mock
10	نفسه	BrowseLogoCommand_OpensFileDialog_AndUpdatesLogoPath	IDialogService.OpenFileDialog يُستدعى والـ LogoPath يتحدث	Mock IDialogService
11	نفسه	SaveCommand_NonStaffId_ShowsError	CurrentUser?.StaffId = null ⇒ InvalidOperationException	StaffId = null
12	نفسه	IsBusy_TrueDuringLoad_FalseAfter	UI state toggle	—
13	Tests/Services/Printing/DocumentTemplateBaseLayoutTests.cs	ReceiptTemplate_ApplyLayout_OverridesDefaults	FontFamily و FontSize و margins تتغير	DTO{FontFamily="Arial", FontSize=14}
14	نفسه	ResultReportTemplate_ApplyLayout_OverridesDefaults	نفس الاختبار على ResultReport	DTO{PrimaryColor="#FF0000"}
15	نفسه	NullLayout_UsesBuiltInDefaults	ApplyLayout(doc, null) لا يُغيّر المستند	layout = null
16	نفسه	CompositeReportTemplate_ApplyLayout_NoOp_DoesNotThrow	no-op لا يُ throwing	DTO غير فارغ
17	نفسه	WorksheetTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
18	نفسه	EnvelopeTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
19	نفسه	MedicalHistoryTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
20	نفسه	BlankReportTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
21	نفسه	CashDrawerSummaryTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
22	نفسه	CommissionReportTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
23	نفسه	OutstandingBalanceTemplate_ApplyLayout_NoOp_DoesNotThrow	نفسه	DTO غير فارغ
24	Tests/ViewModels/Menu/ReportSettingsMenuViewModelTests.cs	NavigateToReportSettingsCommand_CallsNavigationService_OpenTaskWindow	أمر جديد يعمل	Mock INavigationService
25	نفسه	ManageTemplatesCommand_StillWorks_AfterAddingNavigateCommand	لا يوجد regression على الأمر القائم	Mock INavigationService
ملاحظة على Seed Data:

DateTime.UtcNow في كل timestamps
Sex = "M" في كل Patient/Staff seed
It.IsAny<>() في كل Moq setup
xUnit + Moq + EF Core InMemory — لا SqlConnection حقيقي
القسم 6 — Validation Gate G6.4
العدد التراكمي المتوقع: 669 اختبار ناجح (644 + 25)
أمر التشغيل
dotnet test --filter "FullyQualifiedName~FinalLabSystem.Tests" --no-build
شروط الاكتمال
#	الشرط	نعم/لا
1	☐ ApplyLayout يقبل FlowDocument كمعامل أول في DocumentTemplateBase	☐
2	☐ ApplyLayout يُستدعى بعد BuildDocument في WpfFlowDocumentPrintService (وليس قبله)	☐
3	☐ LabSetting يحوي 22 حقل جديد بادئتهم Report (Typed columns، لا key-value)	☐
4	☐ Migration AddReportLayoutColumns طُبِّقت بنجاح (dotnet ef database update)	☐
5	☐ DataTemplate لـ ReportSettingsMenuViewModel في MainWindow.xaml يحتوي أزرار فعلية (وليس placeholder)	☐
6	☐ ManageTemplatesCommand لا يزال يعمل بعد إضافة NavigateToReportSettingsCommand (لا regression)	☐
7	☐ ReportSettingsWindow.xaml.cs يحتوي فقط على InitializeComponent() + constructor (MVVM purity)	☐
8	☐ IReportLayoutService مُسجَّل Scoped في DI	☐
9	☐ ReportSettingsWindowViewModel و ReportSettingsWindow مُسجَّلان Transient في DI	☐
10	☐ navigation.RegisterWindow<ReportSettingsWindowViewModel, ReportSettingsWindow>() موجود	☐
11	☐ الـ 25 اختبار ناجحة	☐
12	☐ الـ 644 اختبار الأصلية (من شريحة 6.3) لا تزال ناجحة بدون تعديل	☐
13	☐ dotnet build بدون errors وبدون warnings	☐
14	☐ grep -rn "Natigh" FinalLabSystem/ يُرجع صفر نتائج	☐
15	☐ ReportSettingKeys.cs يحوي 22 ثابتاً تبدأ ببادئة "Report."	☐
القسم 7 — تقدير الوقت والمخاطر
الوقت المتوقع
النشاط	الساعات
Migration + LabSetting + DbContext	1.5 ساعة
ReportSettingKeys + ReportLayoutDto	0.5 ساعة
IReportLayoutService + ReportLayoutService	1.5 ساعة
DocumentTemplateBase + ReceiptTemplate + ResultReportTemplate	1 ساعة
WpfFlowDocumentPrintService overload	0.5 ساعة
ReportSettingsWindowViewModel + Window	1.5 ساعة
ReportSettingsMenuViewModel + MainWindow DataTemplate	0.5 ساعة
App.xaml.cs DI registration	0.25 ساعة
كتابة الـ 25 اختبار	2 ساعة
dotnet test + إصلاح أي أخطاء	1 ساعة
المجموع	~10.25 ساعة
المخاطر بترتيب الخطورة
#	الخطر	الخطورة	استراتيجية التخفيف
1	ApplyLayout timing — تنفيذ خاطئ (قبل BuildDocument بدل بعده)	عالي	التزم بالتدفق الموثّق: BuildDocument ← ApplyLayout ← PrintDocumentAsync. لا تُبدّل الترتيب.
2	Migration conflict — تعارض مع migration سابقة أو schema حالي	عالي	شغّل dotnet ef migrations script قبل database update. تحقق من عدم تكرارأسماء الأعمدة.
3	Color parsing — ColorConverter.ConvertFromString يرمي على hex غير صالح	متوسط	أضف try/catch في ApplyLayout وتجاهل الألوان غير الصالحة. أو أضف validation في ReportLayoutDto.
4	cm → points conversion — × 28.35 قد يختلف قليلاً حسب DPI	منخفض	القيمة 28.35 هي المعيار. لا تُعدّلها.
5	Null LabSetting row — FirstOrDefaultAsync() يُرجع null في قاعدة جديدة	متوسط	GetCurrentLayoutAsync يتعامل مع null بإرجاع defaults — تأكد من ذلك.
6	8 templates placeholders — ApplyLayout no-op قد يُفاجئ مطوراً لاحقاً	منخفض	وثّق في XML comment أن الـ default هو no-op وأن كل template مسؤول عن override.
انتهى Handoff — الشريحة 6.4


---

هذا هو المحتوى الكامل للملف. يمكنك نسخه ووضعه في `Docs/PRDs/Handoff_Slice6.4.md`.

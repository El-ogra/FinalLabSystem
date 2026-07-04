---
name: print-and-barcode-pipeline-conventions
description: "مهارة متخصصة في print-and-barcode-pipeline-conventions لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# Print and Barcode Pipeline Conventions — FinalLabSystem

## نظرة عامة على النظام

النظام يقسم الطباعة إلى خدمتين منفصلتين في `Services/Implementations/`:

1. **`WpfLabelPrintService`** — ملصقات صغيرة (Tube Labels، Barcode Stickers) — تستخدم `PrintDialog` + `Visual` rendering، الباركود مدمج.
2. **`WpfFlowDocumentPrintService`** — تقارير كاملة (Patient Report، Microbiology Report) — تستخدم `FlowDocument` + `DocumentPaginator` → `XpsDocumentWriter`.

الباركود في كلا النظامين عبر مكتبة **ZXing.Net** (`ZXing` + `ZXing.Common` + `ZXing.Rendering`) — مغلَّفة في `Infrastructure/Barcoding/`.

## خدمة الملصقات (WpfLabelPrintService)

في `Services/Implementations/WpfLabelPrintService.cs`:

```csharp
public sealed class WpfLabelPrintService : ILabelPrintService
{
    private readonly IBarcodeDialogFactory _barcodeDialog;

    public async Task PrintTubeLabelAsync(SampleTube tube)
    {
        // 1. اعرض preview dialog إذا المختبر يطلب ذلك
        if (await ShouldPreviewAsync())
        {
            var dialog = _barcodeDialog.Create(tube);
            if (dialog.ShowDialog() != true) return;
        }

        // 2. افتح طابعة افتراضية
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        // 3. حضّر Visual (UserControl من XAML)
        var label = new TubeLabelControl
        {
            DataContext = new TubeLabelViewModel(tube)
        };
        label.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
        label.Arrange(new Rect(0, 0, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));

        // 4. اطبع
        printDialog.PrintVisual(label, $"Tube-{tube.BarcodeValue}");
    }
}
```

`TubeLabelControl` هو `UserControl` بـ XAML يحوي الباركود عبر الـ converter.

## الباركود — البنية التحتية

### الإعدادات (`BarcodeFormatOptions.cs`):

```csharp
public static class BarcodeFormatOptions
{
    public const BarcodeFormat DefaultFormat = BarcodeFormat.CODE_128;
    public const int DefaultWidth = 200;
    public const int DefaultHeight = 60;
    public const int DefaultMargin = 2;
}
```

الـ `CODE_128` هو المعيار لكل من:
- `SampleTube.BarcodeValue` (ملصقات العيّنات)
- `Visit.VisitCode` (ملصق الزيارة الكلية، يظهر في التقرير)
- `TestResult.ResultId` (في تقارير الطباعة كـ QR بسيط اختياري)

### الـ Converter (`BarcodeImageConverter.cs`):

```csharp
[ValueConversion(typeof(string), typeof(BitmapSource))]
public sealed class BarcodeImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return null!;

        var writer = new BarcodeWriter<BitMatrix>
        {
            Format = BarcodeFormatOptions.DefaultFormat,
            Options = new EncodingOptions
            {
                Width = BarcodeFormatOptions.DefaultWidth,
                Height = BarcodeFormatOptions.DefaultHeight,
                Margin = BarcodeFormatOptions.DefaultMargin,
                PureBarcode = true
            },
            Renderer = new BitMatrixRenderer()
        };

        return RenderBitMatrix(writer.Write(text));
    }
    // ...
}
```

هذا converter يربط `string` (BarcodeValue) بـ `ImageSource` يعرض الباركود. استخدامه في XAML:

```xml
<Image Source="{Binding BarcodeValue, Converter={StaticResource BarcodeConverter}}" 
       Width="200" Height="60" />
```

### تسجيل الـ Converter:

في `App.xaml`:

```xml
<Application.Resources>
    <local:BarcodeImageConverter x:Key="BarcodeConverter" />
</Application.Resources>
```

(الـ `local:` يشير إلى namespace الـ converter في الـ assembly).

## توليد Barcode Value

الـ value لا يُولَّد عشوائيًا. الـ pattern:

```csharp
public static class TubeBarcodeGenerator
{
    public static string GenerateForVisit(DateTime collectedAt, int visitId)
    {
        // LAB-{visitCode}-{yyyyMMdd}
        return $"LAB-{visitId:D6}-{collectedAt:yyyyMMdd}";
    }
}
```

هذا الـ pattern:
- يبدأ بـ `LAB-` (ثابت، للتمييز البصري).
- 6 أرقام للمعرّف (يدعم حتى 999,999 زيارة).
- تاريخ الجمع لتمييز بصرية.

نفس الـ value ينخزن في:
1. `SampleTube.BarcodeValue` (UNIQUE INDEX).
2. باركود مطبوع على الملصق.
3. باركود مطبوع على التقرير بجوار اسم المريض.

## ملصق العينة (Tube Label) — التصميم المعتمد

`TubeLabelControl.xaml` — UserControl مرن:

```xml
<UserControl x:Class="FinalLabSystem.Views.Print.TubeLabelControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:conv="clr-namespace:FinalLabSystem.Infrastructure.Barcoding">
    <Border BorderBrush="Black" BorderThickness="1" Padding="6" Width="280">
        <StackPanel>
            <TextBlock Text="{Binding PatientNameAr}" FontWeight="Bold" FontSize="14" />
            <TextBlock Text="{Binding PatientCode}" FontSize="10" />
            <TextBlock Text="{Binding TestNameAr}" FontSize="11" />
            <Image Source="{Binding BarcodeValue, Converter={StaticResource BarcodeConverter}}"
                   Width="200" Height="50" Margin="0,6,0,2" />
            <TextBlock Text="{Binding BarcodeValue}" 
                       FontFamily="Consolas" FontSize="11" 
                       HorizontalAlignment="Center" />
            <TextBlock Text="{Binding CollectedAt, StringFormat='yyyy-MM-dd HH:mm'}" 
                       FontSize="9" />
        </StackPanel>
    </Border>
</UserControl>
```

`TubeLabelViewModel`:

```csharp
public sealed class TubeLabelViewModel
{
    public string PatientNameAr { get; init; }
    public string PatientCode { get; init; }
    public string TestNameAr { get; init; }
    public string BarcodeValue { get; init; }
    public DateTime CollectedAt { get; init; }
}
```

## خدمة التقارير (WpfFlowDocumentPrintService)

في `Services/Implementations/WpfFlowDocumentPrintService.cs`:

```csharp
public sealed class WpfFlowDocumentPrintService : IPrintService
{
    public FlowDocument BuildPatientReport(VisitFullDto visit, IList<TestComponentResultDto> results)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI, Arial"),
            FontSize = 11,
            PageWidth = 793,    // A4 width at 96 DPI (210mm)
            PageHeight = 1122,  // A4 height (297mm)
            ColumnWidth = double.PositiveInfinity,
            PagePadding = new Thickness(40),
            Background = Brushes.White
        };

        doc.Blocks.Add(BuildHeader(visit));
        doc.Blocks.Add(BuildPatientInfo(visit));
        doc.Blocks.Add(new BlockUIContainer(BuildBarcodeImage(visit.VisitBarcode)));
        doc.Blocks.Add(BuildResultsTable(results));
        doc.Blocks.Add(BuildFooter(visit));

        return doc;
    }

    public async Task PrintAsync(FlowDocument doc, string description)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
        printDialog.PrintDocument(paginator, description);
    }

    public async Task ExportPdfAsync(FlowDocument doc, string filePath)
    {
        // XPS intermediate ثم تحويل لـ PDF عبر PdfWriter / XpsToPdf
        // أو عبر: doc → XpsDocument → FixedDocumentSequence → File
        var xpsPath = Path.ChangeExtension(filePath, ".xps");
        using var xps = new XpsDocument(xpsPath, FileAccess.Write);
        var writer = XpsDocument.CreateXpsDocumentWriter(xps);
        writer.Write(((IDocumentPaginatorSource)doc).DocumentPaginator);
        // ثم تحويل XPS → PDF إذا دُعم، أو نسخ كـ .xps
    }
}
```

## بناء البلوكات الفرعية

### Header (Header Section):

```csharp
private Block BuildHeader(VisitFullDto v)
{
    return new Table
    {
        Columns = { new TableColumn { Width = new GridLength(1, GridUnitType.Star) },
                    new TableColumn { Width = GridLength.Auto } },
        CellSpacing = 0
    };
    // ... مع Grid يحتوي اسم المعمل (من LabSetting) + تاريخ التقرير
}
```

### Barcode Container داخل الـ FlowDocument:

```csharp
private UIElement BuildBarcodeImage(string value)
{
    var image = new Image
    {
        Source = (BitmapSource)_barcodeConverter.Convert(value, typeof(BitmapSource), null, CultureInfo.CurrentCulture),
        Width = 200,
        Height = 60,
        Margin = new Thickness(0, 8, 0, 8)
    };
    return image;
}
```

### Results Table:

```csharp
private Table BuildResultsTable(IList<TestComponentResultDto> results)
{
    var table = new Table { CellSpacing = 0 };
    table.Columns.Add(new TableColumn { Width = new GridLength(60) });      // ref range
    table.Columns.Add(new TableColumn { Width = new GridLength(80) });      // result
    table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // component
    table.Columns.Add(new TableColumn { Width = new GridLength(80) });      // unit
    
    var headerGroup = new TableRowGroup();
    var headerRow = new TableRow { Background = Brushes.LightGray };
    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("المادة"))) { FontWeight = FontWeights.Bold });
    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("النتيجة"))) { FontWeight = FontWeights.Bold });
    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("النطاق"))) { FontWeight = FontWeights.Bold });
    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("الوحدة"))) { FontWeight = FontWeights.Bold });
    headerGroup.Rows.Add(headerRow);
    table.RowGroups.Add(headerGroup);
    
    var dataGroup = new TableRowGroup();
    foreach (var r in results)
    {
        var row = new TableRow();
        row.Cells.Add(MakeCell(r.ComponentNameAr));
        row.Cells.Add(MakeCell(r.ResultValueNumeric?.ToString("0.###") ?? r.ResultValueText));
        // إذا abnormal -> red
        if (r.Flag != null)
            row.Cells.Last().Background = Brushes.MistyRose;
        row.Cells.Add(MakeCell(r.NormalRangeText));
        row.Cells.Add(MakeCell(r.Unit));
        dataGroup.Rows.Add(row);
    }
    table.RowGroups.Add(dataGroup);
    return table;
}
```

## تسجيل Backing Services

في `App.xaml.cs`:

```csharp
services.AddSingleton<IBarcodeDialogFactory, BarcodeDialogFactory>();
services.AddScoped<ILabelPrintService, WpfLabelPrintService>();
services.AddScoped<IPrintService, WpfFlowDocumentPrintService>();
services.AddSingleton<IReportLayoutService, ReportLayoutService>(); // لإعدادات التقرير من LabSetting
```

`ReportLayoutService` يقرأ `LabSetting` (الجدول الموجود في الـ DbContext) لاستخراج:
- `ReportLabNameAr/En`
- `ReportLogoPath`
- `ReportPrimaryColor`
- `ReportPaperSize` (A4/A5/Letter)
- `ReportHeaderFontSize` و `ReportFooterFontSize`

## إضافة ملصق جديد

1. أنشئ `Views/Print/<New>LabelControl.xaml` + `.xaml.cs`.
2. أنشئ `ViewModels/Print/<New>LabelViewModel.cs`.
3. أضف method في `ILabelPrintService`:
   ```csharp
   Task Print<New>LabelAsync(<New>Dto dto);
   ```
4. نفّذ في `WpfLabelPrintService`.
5. سجِّل في DI + Navigation إذا كانت تظهر في menu.

## إضافة تقرير جديد

1. أنشئ method في `IPrintService`:
   ```csharp
   FlowDocument Build<New>Report(<New>Dto dto);
   ```
2. نفّذ في `WpfFlowDocumentPrintService` (بعيدًا عن الشغل inline — احفظ helper methods).
3. إذا لزم مصدر بيانات جديد، اعمل method في `IReportingService` لجلب الـ DTOs (لا تنفذ query داخل print service).
4. استدعاء من ViewModel:
   ```csharp
   var doc = _printService.Build<New>Report(dto);
   await _printService.PrintAsync(doc, $"<New>-{dto.Id}");
   ```

## استخدام `PrintQueueService` (اختياري للطباعة المؤجلة)

النظام يحوي `IPrintQueueService` و `PrintQueueService` + جدول `ReceiptPrintLog`. إذا كان الطابعة أو الشبكة غير متاحة (مختبر يعمل offline)، الإجراء يلحق إلى طابور:

```csharp
public async Task QueueForLaterAsync(FlowDocument doc, string description, int staffId)
{
    // خزّن نسخة serialized من الـ FlowDocument كـ XAML string
    var xaml = XamlWriter.Save(doc);
    await _printQueueService.EnqueueAsync(
        documentXaml: xaml,
        description: description,
        staffId: staffId);
}
```

عند عودة الطابعة:
```csharp
public async Task FlushQueueAsync()
{
    var items = await _printQueueService.GetPendingAsync();
    foreach (var item in items)
    {
        var doc = (FlowDocument)XamlReader.Parse(item.DocumentXaml);
        await _printService.PrintAsync(doc, item.Description);
        await _printQueueService.MarkCompletedAsync(item.QueueId);
    }
}
```

## ملاحظات الطباعة العربية

- اضبط `FlowDirection="RightToLeft"` على عناصر النصوص العربية في التقارير — يحاذي للأيمن.
- الـ font الافتراضي للنصوص العربية `"Segoe UI"`. للنصوص اللاتينية (مثل unit names) اترك الـ default أو اضبط `FontFamily="Arial"`.
- الأرقام في الجداول لازم تكون LTR (وضع `XmlLanguage="en-US"` على الـ Run يحلّ المشكلة).

## Checklist لأي تقرير/ملصق جديد

```
□ XAML يحترم FlowDirection للسياق العربي
□ باركود عبر Converter، لا inline rendering
□ BarcodeValue مُولّد عبر دالة Generate مع pattern ثابت
□ Backing service (Interface + Implementation) — لا استدعاء طباعة من ViewModel مباشرة
□ DI registration في App.xaml.cs
□ Report يحترم ReportLayoutService (اسم المعمل، اللون، الـ font size)
□ إذا كان long-form تقرير، استخدم FlowDocument مش UserControl
□ إذا كانت ملصق صغير، استخدم UserControl + PrintVisual
□ PDF export يمر عبر XpsDocument و ليس Image rendering مباشر
```

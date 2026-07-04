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
        AddCell(infoRow, _patient.FullNameAr ?? "", false);
        AddCell(infoRow, "السن:", true);
        AddCell(infoRow, $"{_patient.ApproxAge} {_patient.ApproxAgeUnit}", false);
        var bodyGroup = new TableRowGroup();
        bodyGroup.Rows.Add(infoRow);
        infoTable.RowGroups.Add(bodyGroup);

        var infoRow2 = new TableRow();
        AddCell(infoRow2, "الجنس:", true);
        AddCell(infoRow2, _patient.Sex == "M" ? "ذكر" : "أنثى", false);
        AddCell(infoRow2, "مصدر العينة:", true);
        AddCell(infoRow2, _culture.SpecimenSource ?? "", false);
        var bodyGroup2 = new TableRowGroup();
        bodyGroup2.Rows.Add(infoRow2);
        infoTable.RowGroups.Add(bodyGroup2);

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

                    var para = new Paragraph { Margin = new Thickness(16, 2, 0, 2) };
                    para.Inlines.Add(new Run($"{levelName}: ") { FontWeight = FontWeights.Bold });
                    para.Inlines.Add(new Run(drugNames));
                    doc.Blocks.Add(para);
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
            var footerPara = new Paragraph { Margin = new Thickness(0, 0, 0, 8) };
            footerPara.Inlines.Add(new Run("ملاحظات: ") { FontWeight = FontWeights.Bold });
            footerPara.Inlines.Add(new Run(_culture.FinalComment));
            doc.Blocks.Add(footerPara);
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

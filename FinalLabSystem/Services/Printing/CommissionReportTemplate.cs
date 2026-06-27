using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class CommissionReportTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        if (data is not List<CommissionReportRow> rows)
            throw new ArgumentException("Data must be List<CommissionReportRow>", nameof(data));

        var doc = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };

        doc.Blocks.Add(CreateHeader("تقرير عمولات الإحالات"));

        var table = new Table
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 12, 0, 0)
        };

        table.Columns.Add(new TableColumn { Width = new GridLength(50) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(90) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });

        var rowGroup = new TableRowGroup();

        var headerRow = new TableRow();
        headerRow.Cells.Add(CreateCell("#", true));
        headerRow.Cells.Add(CreateCell("المحيل", true));
        headerRow.Cells.Add(CreateCell("النوع", true));
        headerRow.Cells.Add(CreateCell("المريض", true));
        headerRow.Cells.Add(CreateCell("الكود", true));
        headerRow.Cells.Add(CreateCell("التاريخ", true));
        headerRow.Cells.Add(CreateCell("الإجمالي", true));
        headerRow.Cells.Add(CreateCell("العمولة", true));
        rowGroup.Rows.Add(headerRow);

        double totalCommission = 0;
        foreach (var row in rows)
        {
            var commission = row.CommissionDue ?? 0;
            totalCommission += commission;
            rowGroup.Rows.Add(CreateRow(
                (rows.IndexOf(row) + 1).ToString(),
                row.ReferralName ?? "-",
                row.SourceType,
                row.PatientName,
                row.VisitCode,
                row.VisitDate.ToString("yyyy/MM/dd"),
                row.VisitTotal.ToString("N2"),
                commission.ToString("N2")));
        }

        table.RowGroups.Add(rowGroup);
        doc.Blocks.Add(table);

        doc.Blocks.Add(new Paragraph(new Run($"إجمالي العمولات: {totalCommission:N2}"))
        {
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 12, 0, 8)
        });

        doc.Blocks.Add(CreateFooter("تم الطباعة في: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm")));

        return doc;
    }

    private static TableCell CreateCell(string text, bool isBold = false)
    {
        var cell = new TableCell(new Paragraph(new Run(text)))
        {
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(4)
        };
        if (isBold)
            cell.FontWeight = FontWeights.Bold;
        return cell;
    }

    private static TableRow CreateRow(params string[] cells)
    {
        var row = new TableRow();
        foreach (var c in cells)
            row.Cells.Add(CreateCell(c));
        return row;
    }
}

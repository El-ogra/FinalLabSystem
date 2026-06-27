using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class OutstandingBalanceReportTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        if (data is not List<OutstandingBalanceReportRow> rows)
            throw new ArgumentException("Data must be List<OutstandingBalanceReportRow>", nameof(data));

        var doc = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };

        doc.Blocks.Add(CreateHeader("تقرير الأرصدة المستحقة"));

        var table = new Table
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 12, 0, 0)
        };

        table.Columns.Add(new TableColumn { Width = new GridLength(50) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(90) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });

        var rowGroup = new TableRowGroup();

        var headerRow = new TableRow();
        headerRow.Cells.Add(CreateCell("#", true));
        headerRow.Cells.Add(CreateCell("الكود", true));
        headerRow.Cells.Add(CreateCell("التاريخ", true));
        headerRow.Cells.Add(CreateCell("كود المريض", true));
        headerRow.Cells.Add(CreateCell("المريض", true));
        headerRow.Cells.Add(CreateCell("الهاتف", true));
        headerRow.Cells.Add(CreateCell("الشركة", true));
        headerRow.Cells.Add(CreateCell("الإجمالي", true));
        headerRow.Cells.Add(CreateCell("المدفوع", true));
        headerRow.Cells.Add(CreateCell("المستحق", true));
        rowGroup.Rows.Add(headerRow);

        double totalBalance = 0;
        foreach (var row in rows)
        {
            totalBalance += row.BalanceDue;
            rowGroup.Rows.Add(CreateRow(
                (rows.IndexOf(row) + 1).ToString(),
                row.VisitCode,
                row.VisitDate.ToString("yyyy/MM/dd"),
                row.PatientCode,
                row.PatientName,
                row.Phone ?? "-",
                row.CompanyName ?? "-",
                row.TotalAfterDiscount.ToString("N2"),
                row.TotalPaid.ToString("N2"),
                row.BalanceDue.ToString("N2")));
        }

        table.RowGroups.Add(rowGroup);
        doc.Blocks.Add(table);

        doc.Blocks.Add(new Paragraph(new Run($"إجمالي الأرصدة المستحقة: {totalBalance:N2}"))
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

using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Printing;

public class CashDrawerSummaryTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        if (data is not CashDrawerSummaryDto summary)
            throw new ArgumentException("Data must be CashDrawerSummaryDto", nameof(data));

        var doc = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };

        doc.Blocks.Add(CreateHeader("ملخص درج النقدية"));

        doc.Blocks.Add(new Paragraph(new Run($"التاريخ: {summary.Date:yyyy/MM/dd}"))
        {
            FontSize = 12,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 8, 0, 16)
        });

        var summaryTable = new Table
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 16)
        };

        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(200) });
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(150) });

        var summaryGroup = new TableRowGroup();
        var headerRow = new TableRow();
        headerRow.Cells.Add(CreateCell("البيان", true));
        headerRow.Cells.Add(CreateCell("المبلغ", true));
        summaryGroup.Rows.Add(headerRow);
        summaryGroup.Rows.Add(CreateRow("المدفوعات النقدية", summary.TotalCashReceived.ToString("C")));
        summaryGroup.Rows.Add(CreateRow("مدفوعات التأمين", summary.TotalInsuranceReceived.ToString("C")));
        summaryGroup.Rows.Add(CreateRow("مدفوعات العقود", summary.TotalContractReceived.ToString("C")));
        summaryGroup.Rows.Add(CreateRow("الإجمالي", summary.GrandTotal.ToString("C")));
        summaryTable.RowGroups.Add(summaryGroup);

        doc.Blocks.Add(summaryTable);

        doc.Blocks.Add(new Paragraph(new Run($"عدد عمليات الدفع: {summary.PaymentCount}"))
        {
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 8)
        });

        if (summary.Payments.Any())
        {
            doc.Blocks.Add(new Paragraph(new Run("تفاصيل العمليات:"))
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 8, 0, 8)
            });

            var detailTable = new Table
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            detailTable.Columns.Add(new TableColumn { Width = new GridLength(60) });
            detailTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            detailTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            detailTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            detailTable.Columns.Add(new TableColumn { Width = new GridLength(80) });

            var detailGroup = new TableRowGroup();
            var detailHeader = new TableRow();
            detailHeader.Cells.Add(CreateCell("#", true));
            detailHeader.Cells.Add(CreateCell("المريض", true));
            detailHeader.Cells.Add(CreateCell("الكود", true));
            detailHeader.Cells.Add(CreateCell("المبلغ", true));
            detailHeader.Cells.Add(CreateCell("النوع", true));
            detailGroup.Rows.Add(detailHeader);

            foreach (var p in summary.Payments)
            {
                detailGroup.Rows.Add(CreateRow(
                    p.PaymentId.ToString(),
                    p.PatientName,
                    p.VisitCode,
                    p.Amount.ToString("C"),
                    p.PaymentMethod));
            }

            detailTable.RowGroups.Add(detailGroup);
            doc.Blocks.Add(detailTable);
        }

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

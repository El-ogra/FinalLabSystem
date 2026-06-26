using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Printing;

public class ContractInvoiceTemplate : DocumentTemplateBase
{
    public override FlowDocument BuildDocument(object data)
    {
        if (data is not ContractInvoice invoice)
        {
            var doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph(new Run("خطأ: البيانات المدخلة ليست فاتورة صالحة")));
            return doc;
        }

        var document = new FlowDocument
        {
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI")
        };

        document.Blocks.Add(CreateHeader("فاتورة عقد شركات"));

        document.Blocks.Add(new Paragraph(new Run($"اسم الشركة: {invoice.Company?.CompanyName ?? "غير محدد"}"))
        {
            FontSize = 13,
            Margin = new Thickness(0, 10, 0, 5)
        });

        document.Blocks.Add(new Paragraph(new Run($"الفترة: {invoice.PeriodStart:yyyy/MM/dd} — {invoice.PeriodEnd:yyyy/MM/dd}"))
        {
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 5)
        });

        document.Blocks.Add(new Paragraph(new Run($"تاريخ الفاتورة: {invoice.InvoiceDate:yyyy/MM/dd}"))
        {
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 15)
        });

        var detailsTable = new Table
        {
            Margin = new Thickness(0, 0, 0, 15)
        };

        var col1 = new TableColumn();
        var col2 = new TableColumn();
        detailsTable.Columns.Add(col1);
        detailsTable.Columns.Add(col2);

        var detailsGroup = new TableRowGroup();

        var headerRow = new TableRow();
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("إجمالي الفاتورة:")) { FontWeight = FontWeights.Bold }));
        headerRow.Cells.Add(new TableCell(new Paragraph(new Run($"{invoice.TotalAmount:N2}"))));
        detailsGroup.Rows.Add(headerRow);

        var paidRow = new TableRow();
        paidRow.Cells.Add(new TableCell(new Paragraph(new Run("المبلغ المدفوع:")) { FontWeight = FontWeights.Bold }));
        paidRow.Cells.Add(new TableCell(new Paragraph(new Run($"{invoice.PaidAmount:N2}"))));
        detailsGroup.Rows.Add(paidRow);

        var balanceRow = new TableRow();
        balanceRow.Cells.Add(new TableCell(new Paragraph(new Run("الرصيد المتبقي:")) { FontWeight = FontWeights.Bold }));
        balanceRow.Cells.Add(new TableCell(new Paragraph(new Run($"{invoice.TotalAmount - invoice.PaidAmount:N2}"))));
        detailsGroup.Rows.Add(balanceRow);

        var statusRow = new TableRow();
        statusRow.Cells.Add(new TableCell(new Paragraph(new Run("حالة الفاتورة:")) { FontWeight = FontWeights.Bold }));
        statusRow.Cells.Add(new TableCell(new Paragraph(new Run(invoice.Status))));
        detailsGroup.Rows.Add(statusRow);

        detailsTable.RowGroups.Add(detailsGroup);
        document.Blocks.Add(detailsTable);

        if (invoice.ContractPayments.Any())
        {
            document.Blocks.Add(new Paragraph(new Run("الدفعات:"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            });

            var paymentsTable = new Table();
            var pcol1 = new TableColumn();
            var pcol2 = new TableColumn();
            var pcol3 = new TableColumn();
            var pcol4 = new TableColumn();
            paymentsTable.Columns.Add(pcol1);
            paymentsTable.Columns.Add(pcol2);
            paymentsTable.Columns.Add(pcol3);
            paymentsTable.Columns.Add(pcol4);

            var paymentsGroup = new TableRowGroup();

            var payHeaderRow = new TableRow();
            payHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run("التاريخ")) { FontWeight = FontWeights.Bold }));
            payHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run("المبلغ")) { FontWeight = FontWeights.Bold }));
            payHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run("الطريقة")) { FontWeight = FontWeights.Bold }));
            payHeaderRow.Cells.Add(new TableCell(new Paragraph(new Run("المرجع")) { FontWeight = FontWeights.Bold }));
            paymentsGroup.Rows.Add(payHeaderRow);

            foreach (var payment in invoice.ContractPayments)
            {
                var payRow = new TableRow();
                payRow.Cells.Add(new TableCell(new Paragraph(new Run(payment.PaymentDate.ToString("yyyy/MM/dd")))));
                payRow.Cells.Add(new TableCell(new Paragraph(new Run($"{payment.Amount:N2}"))));
                payRow.Cells.Add(new TableCell(new Paragraph(new Run(payment.PaymentMethod))));
                payRow.Cells.Add(new TableCell(new Paragraph(new Run(payment.ReferenceNumber ?? ""))));
                paymentsGroup.Rows.Add(payRow);
            }

            paymentsTable.RowGroups.Add(paymentsGroup);
            document.Blocks.Add(paymentsTable);
        }

        document.Blocks.Add(CreateFooter("نظام إدارة المختبر — FinalLabSystem"));

        return document;
    }
}

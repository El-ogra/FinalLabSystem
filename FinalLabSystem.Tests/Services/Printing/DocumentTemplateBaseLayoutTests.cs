using System.Windows.Documents;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Printing;
using Xunit;

namespace FinalLabSystem.Tests.Services.Printing;

public class DocumentTemplateBaseLayoutTests
{
    private static ReportLayoutDto CreateCustomLayout()
    {
        return new ReportLayoutDto
        {
            FontFamily = "Arial",
            FontSize = 14,
            PrimaryColor = "#FF0000",
            SecondaryColor = "#0000FF",
            MarginTop = 3,
            MarginBottom = 1.5m,
            MarginLeft = 2.5m,
            MarginRight = 2.5m,
            ShowHeader = true,
            ShowFooter = true
        };
    }

    [Fact]
    public void ReceiptTemplate_ApplyLayout_OverridesDefaults()
    {
        var template = new ReceiptTemplate();
        var doc = template.BuildDocument(null!);
        var layout = CreateCustomLayout();

        template.ApplyLayout(doc, layout);

        Assert.Equal("Arial", doc.FontFamily?.Source);
        Assert.Equal(14, doc.FontSize);
    }

    [Fact]
    public void ResultReportTemplate_ApplyLayout_OverridesDefaults()
    {
        var template = new ResultReportTemplate();
        var doc = template.BuildDocument(null!);
        var layout = new ReportLayoutDto
        {
            FontFamily = "Times New Roman",
            FontSize = 16,
            PrimaryColor = "#FF0000"
        };

        template.ApplyLayout(doc, layout);

        Assert.Equal("Times New Roman", doc.FontFamily?.Source);
        Assert.Equal(16, doc.FontSize);
    }

    [Fact]
    public void NullLayout_UsesBuiltInDefaults()
    {
        var template = new ReceiptTemplate();
        var doc = template.BuildDocument(null!);

        template.ApplyLayout(doc, null);

        Assert.NotNull(doc);
        Assert.Equal("Segoe UI", doc.FontFamily?.Source);
    }

    [Fact]
    public void CompositeReportTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new CompositeReportTemplate();
        var doc = template.BuildDocument(null!);
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void WorksheetTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new WorksheetTemplate();
        var doc = template.BuildDocument(null!);
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void EnvelopeTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new EnvelopeTemplate();
        var doc = template.BuildDocument(null!);
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void MedicalHistoryTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new MedicalHistoryTemplate();
        var doc = template.BuildDocument(null!);
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void BlankReportTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new BlankReportTemplate();
        var doc = template.BuildDocument(null!);
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void CashDrawerSummaryTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new CashDrawerSummaryTemplate();
        var doc = template.BuildDocument(new FinalLabSystem.Models.DTOs.CashDrawerSummaryDto
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            PaymentCount = 0
        });
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void CommissionReportTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new CommissionReportTemplate();
        var doc = template.BuildDocument(new System.Collections.Generic.List<FinalLabSystem.Models.DTOs.CommissionReportRow>());
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }

    [Fact]
    public void OutstandingBalanceTemplate_ApplyLayout_NoOp_DoesNotThrow()
    {
        var template = new OutstandingBalanceReportTemplate();
        var doc = template.BuildDocument(new System.Collections.Generic.List<FinalLabSystem.Models.DTOs.OutstandingBalanceReportRow>());
        var layout = CreateCustomLayout();

        var ex = Record.Exception(() => template.ApplyLayout(doc, layout));
        Assert.Null(ex);
    }
}

using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class InvoiceWorkflowEndToEndTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static InvoiceService CreateService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<InvoiceService>>();
        return new InvoiceService(ctx, logger.Object);
    }

    [Fact]
    public async Task FullWorkflow_CompanyVisitInvoicePayment()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(FullWorkflow_CompanyVisitInvoicePayment)));

        var company = new Company
        {
            CompanyId = 1,
            CompanyName = "Test Corp",
            CompanyType = "CORPORATE",
            ContactPerson = "John",
            Phone = "123",
            DiscountRate = 10
        };
        ctx.Companies.Add(company);

        var patient = new Patient
        {
            PatientCode = "P001",
            FullNameAr = "مريض",
            FullNameEn = "Patient",
            Sex = "M"
        };
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            VisitCode = "V001",
            PatientId = patient.PatientId,
            VisitDate = new DateTime(2026, 1, 15),
            CompanyId = 1,
            TotalAfterDiscount = 1000
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        var invoice = await service.GenerateInvoiceAsync(1, 2026, 1, 0);
        Assert.Equal("Pending", invoice.Status);
        Assert.Equal(900, invoice.TotalAmount);

        await service.RecordPaymentAsync(invoice.ContractInvoiceId, 400, "CASH", "REF-001", 0);
        var afterPartial = await ctx.ContractInvoices.FindAsync(invoice.ContractInvoiceId);
        Assert.Equal("Partial", afterPartial!.Status);
        Assert.Equal(400, afterPartial.PaidAmount);

        await service.RecordPaymentAsync(invoice.ContractInvoiceId, 500, "BANK", "REF-002", 0);
        var afterFull = await ctx.ContractInvoices.FindAsync(invoice.ContractInvoiceId);
        Assert.Equal("Paid", afterFull!.Status);
        Assert.Equal(900, afterFull.PaidAmount);

        var payments = await ctx.ContractPayments
            .Where(p => p.ContractInvoiceId == invoice.ContractInvoiceId)
            .ToListAsync();
        Assert.Equal(2, payments.Count);
    }

    [Fact]
    public async Task Invoice_ShouldFilterByCompany()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Invoice_ShouldFilterByCompany)));
        ctx.Companies.AddRange(
            new Company { CompanyId = 1, CompanyName = "A", CompanyType = "CORPORATE", ContactPerson = "J", Phone = "1", DiscountRate = 0 },
            new Company { CompanyId = 2, CompanyName = "B", CompanyType = "CORPORATE", ContactPerson = "K", Phone = "2", DiscountRate = 0 }
        );
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.GenerateInvoiceAsync(1, 2026, 1, 0);
        await service.GenerateInvoiceAsync(2, 2026, 1, 0);

        var invoices1 = await service.GetInvoicesAsync(1);
        var invoices2 = await service.GetInvoicesAsync(2);

        Assert.Single(invoices1);
        Assert.Single(invoices2);
        Assert.NotEqual(invoices1[0].ContractInvoiceId, invoices2[0].ContractInvoiceId);
    }

    [Fact]
    public async Task Payment_WithTransaction_ShouldBeAtomic()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Payment_WithTransaction_ShouldBeAtomic)));
        var invoice = new ContractInvoice
        {
            CompanyId = 1,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.MinValue,
            PeriodEnd = DateOnly.MaxValue,
            TotalAmount = 100,
            PaidAmount = 0,
            Status = "Pending"
        };
        ctx.ContractInvoices.Add(invoice);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.RecordPaymentAsync(invoice.ContractInvoiceId, 50, "CASH", null, 0);

        var updated = await ctx.ContractInvoices.FindAsync(invoice.ContractInvoiceId);
        Assert.Equal(50, updated!.PaidAmount);
        Assert.Equal("Partial", updated.Status);

        var payment = await ctx.ContractPayments.FirstAsync(p => p.ContractInvoiceId == invoice.ContractInvoiceId);
        Assert.Equal(50, payment.Amount);
    }

    [Fact]
    public async Task MultipleInvoices_SameCompany_ShouldBeIndependent()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(MultipleInvoices_SameCompany_ShouldBeIndependent)));
        ctx.Companies.Add(new Company { CompanyId = 1, CompanyName = "C", CompanyType = "CORPORATE", ContactPerson = "J", Phone = "1", DiscountRate = 0 });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var inv1 = await service.GenerateInvoiceAsync(1, 2026, 1, 0);
        var inv2 = await service.GenerateInvoiceAsync(1, 2026, 2, 0);

        Assert.NotEqual(inv1.ContractInvoiceId, inv2.ContractInvoiceId);
        Assert.Equal("Pending", inv1.Status);
        Assert.Equal("Pending", inv2.Status);
    }

    [Fact]
    public async Task UpdateStatus_ShouldPersist()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateStatus_ShouldPersist)));
        var invoice = new ContractInvoice
        {
            CompanyId = 1,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.MinValue,
            PeriodEnd = DateOnly.MaxValue,
            TotalAmount = 100,
            PaidAmount = 0,
            Status = "Pending"
        };
        ctx.ContractInvoices.Add(invoice);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.UpdateStatusAsync(invoice.ContractInvoiceId, "Paid");

        var updated = await ctx.ContractInvoices.FindAsync(invoice.ContractInvoiceId);
        Assert.Equal("Paid", updated!.Status);
    }
}

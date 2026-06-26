using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class InvoiceServiceTests
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
    public async Task GenerateInvoiceAsync_ShouldCreateInvoice_WhenCompanyHasVisits()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GenerateInvoiceAsync_ShouldCreateInvoice_WhenCompanyHasVisits)));
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
        ctx.Visits.Add(new Visit
        {
            VisitCode = "V001",
            CompanyId = 1,
            VisitDate = new DateTime(2026, 1, 15),
            TotalAfterDiscount = 1000
        });
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var invoice = await service.GenerateInvoiceAsync(1, 2026, 1, 0);

        Assert.Equal(900, invoice.TotalAmount); // 1000 - 10% = 900
        Assert.Equal("Pending", invoice.Status);
        Assert.Equal(1, invoice.CompanyId);
    }

    [Fact]
    public async Task GenerateInvoiceAsync_ShouldCreateInvoice_WithZeroTotalWhenNoVisits()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GenerateInvoiceAsync_ShouldCreateInvoice_WithZeroTotalWhenNoVisits)));
        var company = new Company
        {
            CompanyId = 1,
            CompanyName = "Empty Corp",
            CompanyType = "CORPORATE",
            ContactPerson = "Jane",
            Phone = "456",
            DiscountRate = 0
        };
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var invoice = await service.GenerateInvoiceAsync(1, 2026, 1, 0);

        Assert.Equal(0, invoice.TotalAmount);
        Assert.Equal("Pending", invoice.Status);
    }

    [Fact]
    public async Task RecordPaymentAsync_ShouldSetPaid_WhenFullyPaid()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordPaymentAsync_ShouldSetPaid_WhenFullyPaid)));
        var invoice = new ContractInvoice
        {
            CompanyId = 1,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodEnd = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalAmount = 1000,
            PaidAmount = 0,
            Status = "Pending"
        };
        ctx.ContractInvoices.Add(invoice);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.RecordPaymentAsync(invoice.ContractInvoiceId, 1000, "CASH", "REF001", 0);

        var updated = await ctx.ContractInvoices.FindAsync(invoice.ContractInvoiceId);
        Assert.Equal(1000, updated!.PaidAmount);
        Assert.Equal("Paid", updated.Status);
    }

    [Fact]
    public async Task RecordPaymentAsync_ShouldSetPartial_WhenPartiallyPaid()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordPaymentAsync_ShouldSetPartial_WhenPartiallyPaid)));
        var invoice = new ContractInvoice
        {
            CompanyId = 1,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodEnd = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalAmount = 1000,
            PaidAmount = 0,
            Status = "Pending"
        };
        ctx.ContractInvoices.Add(invoice);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.RecordPaymentAsync(invoice.ContractInvoiceId, 500, "CASH", null, 0);

        var updated = await ctx.ContractInvoices.FindAsync(invoice.ContractInvoiceId);
        Assert.Equal(500, updated!.PaidAmount);
        Assert.Equal("Partial", updated.Status);
    }

    [Fact]
    public async Task RecordPaymentAsync_ShouldThrow_WhenInvoiceNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordPaymentAsync_ShouldThrow_WhenInvoiceNotFound)));
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RecordPaymentAsync(999, 100, "CASH", null, 0));
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldReturnInvoicesForCompany()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetInvoicesAsync_ShouldReturnInvoicesForCompany)));
        ctx.ContractInvoices.AddRange(
            new ContractInvoice { CompanyId = 1, InvoiceDate = DateTime.UtcNow, PeriodStart = DateOnly.MinValue, PeriodEnd = DateOnly.MaxValue, TotalAmount = 100, PaidAmount = 0, Status = "Pending" },
            new ContractInvoice { CompanyId = 2, InvoiceDate = DateTime.UtcNow, PeriodStart = DateOnly.MinValue, PeriodEnd = DateOnly.MaxValue, TotalAmount = 200, PaidAmount = 0, Status = "Pending" }
        );
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var invoices = await service.GetInvoicesAsync(1);

        Assert.Single(invoices);
        Assert.Equal(1, invoices[0].CompanyId);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatus()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateStatusAsync_ShouldUpdateStatus)));
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

    [Fact]
    public async Task RecordPaymentAsync_ShouldCreatePaymentRecord()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RecordPaymentAsync_ShouldCreatePaymentRecord)));
        var invoice = new ContractInvoice
        {
            CompanyId = 1,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = DateOnly.MinValue,
            PeriodEnd = DateOnly.MaxValue,
            TotalAmount = 500,
            PaidAmount = 0,
            Status = "Pending"
        };
        ctx.ContractInvoices.Add(invoice);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        await service.RecordPaymentAsync(invoice.ContractInvoiceId, 200, "BANK", "REF-123", 0);

        var payments = await ctx.ContractPayments.Where(p => p.ContractInvoiceId == invoice.ContractInvoiceId).ToListAsync();
        Assert.Single(payments);
        Assert.Equal(200, payments[0].Amount);
        Assert.Equal("BANK", payments[0].PaymentMethod);
        Assert.Equal("REF-123", payments[0].ReferenceNumber);
    }
}

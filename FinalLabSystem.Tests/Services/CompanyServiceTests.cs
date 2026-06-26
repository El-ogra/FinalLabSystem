using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class CompanyServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static CompanyService CreateService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<CompanyService>>();
        return new CompanyService(ctx, logger.Object);
    }

    private static Company CreateSampleCompany(string name = "Test Corp")
        => new()
        {
            CompanyName = name,
            CompanyType = "CORPORATE",
            ContactPerson = "John Doe",
            Phone = "0123456789",
            IsActive = true,
            DiscountRate = 10.0,
            CreditLimit = 5000.0,
            BillingPeriodicity = "Monthly"
        };

    [Fact]
    public async Task CreateAsync_AddsCompanyToDatabase()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateAsync_AddsCompanyToDatabase)));
        var service = CreateService(ctx);
        var company = CreateSampleCompany("Acme Inc");

        var result = await service.CreateAsync(company);

        Assert.True(result.CompanyId > 0);
        Assert.Equal("Acme Inc", result.CompanyName);
        Assert.Single(ctx.Companies);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCompany_WhenExists()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetByIdAsync_ReturnsCompany_WhenExists)));
        var service = CreateService(ctx);
        var company = CreateSampleCompany("Acme Corp");
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        var result = await service.GetByIdAsync(company.CompanyId);

        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result!.CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetByIdAsync_ReturnsNull_WhenNotExists)));
        var service = CreateService(ctx);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActiveCompanies()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetAllAsync_ReturnsAllActiveCompanies)));
        var service = CreateService(ctx);

        ctx.Companies.Add(CreateSampleCompany("Company A"));
        ctx.Companies.Add(CreateSampleCompany("Company B"));
        ctx.Companies.Add(CreateSampleCompany("Company C"));
        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingCompany()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateAsync_UpdatesExistingCompany)));
        var service = CreateService(ctx);
        var company = CreateSampleCompany("Original Name");
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        company.CompanyName = "Updated Name";
        company.DiscountRate = 25.0;
        company.ContractStartDate = new DateOnly(2026, 1, 1);
        company.ContractEndDate = new DateOnly(2026, 12, 31);
        company.BillingPeriodicity = "Quarterly";
        await service.UpdateAsync(company);

        var updated = await ctx.Companies.FindAsync(company.CompanyId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.CompanyName);
        Assert.Equal(25.0, updated.DiscountRate);
        Assert.Equal(new DateOnly(2026, 1, 1), updated.ContractStartDate);
        Assert.Equal(new DateOnly(2026, 12, 31), updated.ContractEndDate);
        Assert.Equal("Quarterly", updated.BillingPeriodicity);
    }

    [Fact]
    public async Task UpdateAsync_DoesNothing_WhenCompanyNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateAsync_DoesNothing_WhenCompanyNotFound)));
        var service = CreateService(ctx);
        var company = CreateSampleCompany("Ghost Company");
        company.CompanyId = 999;

        await service.UpdateAsync(company);

        Assert.Empty(ctx.Companies);
    }

    [Fact]
    public async Task CreateAsync_SetsContractFields_Correctly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateAsync_SetsContractFields_Correctly)));
        var service = CreateService(ctx);
        var company = CreateSampleCompany("Contract Corp");
        company.ContractStartDate = new DateOnly(2026, 3, 15);
        company.ContractEndDate = new DateOnly(2027, 3, 14);
        company.BillingPeriodicity = "Monthly";

        var result = await service.CreateAsync(company);

        Assert.Equal(new DateOnly(2026, 3, 15), result.ContractStartDate);
        Assert.Equal(new DateOnly(2027, 3, 14), result.ContractEndDate);
        Assert.Equal("Monthly", result.BillingPeriodicity);
    }

    [Fact]
    public async Task UpdateAsync_AllowsSettingContractFieldsToNull()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateAsync_AllowsSettingContractFieldsToNull)));
        var service = CreateService(ctx);
        var company = CreateSampleCompany("Nullable Corp");
        company.ContractStartDate = new DateOnly(2026, 1, 1);
        company.ContractEndDate = new DateOnly(2026, 12, 31);
        company.BillingPeriodicity = "Quarterly";
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        company.ContractStartDate = null;
        company.ContractEndDate = null;
        company.BillingPeriodicity = null;
        await service.UpdateAsync(company);

        var updated = await ctx.Companies.FindAsync(company.CompanyId);
        Assert.NotNull(updated);
        Assert.Null(updated!.ContractStartDate);
        Assert.Null(updated.ContractEndDate);
        Assert.Null(updated.BillingPeriodicity);
    }
}

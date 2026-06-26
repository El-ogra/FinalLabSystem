using FinalLabSystem.Data;
using FinalLabSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinalLabSystem.Tests.Validation;

public class CompanyContractFieldsValidationTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public async Task Company_ContractStartDate_ShouldBeNullByDefault()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Company_ContractStartDate_ShouldBeNullByDefault)));
        var company = new Company
        {
            CompanyName = "Test",
            CompanyType = "CORPORATE",
            ContactPerson = "J",
            Phone = "1"
        };
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Companies.FindAsync(company.CompanyId);
        Assert.Null(loaded!.ContractStartDate);
    }

    [Fact]
    public async Task Company_ContractEndDate_ShouldBeNullByDefault()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Company_ContractEndDate_ShouldBeNullByDefault)));
        var company = new Company
        {
            CompanyName = "Test",
            CompanyType = "CORPORATE",
            ContactPerson = "J",
            Phone = "1"
        };
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Companies.FindAsync(company.CompanyId);
        Assert.Null(loaded!.ContractEndDate);
    }

    [Fact]
    public async Task Company_BillingPeriodicity_ShouldBeNullByDefault()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Company_BillingPeriodicity_ShouldBeNullByDefault)));
        var company = new Company
        {
            CompanyName = "Test",
            CompanyType = "CORPORATE",
            ContactPerson = "J",
            Phone = "1"
        };
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Companies.FindAsync(company.CompanyId);
        Assert.Null(loaded!.BillingPeriodicity);
    }

    [Fact]
    public async Task Company_ContractFields_ShouldPersistWhenSet()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(Company_ContractFields_ShouldPersistWhenSet)));
        var company = new Company
        {
            CompanyName = "Test",
            CompanyType = "CORPORATE",
            ContactPerson = "J",
            Phone = "1",
            ContractStartDate = new DateOnly(2026, 1, 1),
            ContractEndDate = new DateOnly(2026, 12, 31),
            BillingPeriodicity = "Monthly"
        };
        ctx.Companies.Add(company);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Companies.FindAsync(company.CompanyId);
        Assert.Equal(new DateOnly(2026, 1, 1), loaded!.ContractStartDate);
        Assert.Equal(new DateOnly(2026, 12, 31), loaded.ContractEndDate);
        Assert.Equal("Monthly", loaded.BillingPeriodicity);
    }
}

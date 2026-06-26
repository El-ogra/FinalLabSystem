using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(FinalLabDbContext context, ILogger<CompanyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Company>> GetAllAsync()
    {
        return await _context.Companies
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Companies.FindAsync(id);
    }

    public async Task<Company> CreateAsync(Company company)
    {
        company.CreatedAt = System.DateTime.UtcNow;
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        var existing = await _context.Companies.FindAsync(company.CompanyId);
        if (existing is null)
            return;

        existing.CompanyName = company.CompanyName;
        existing.CompanyType = company.CompanyType;
        existing.ContactPerson = company.ContactPerson;
        existing.Phone = company.Phone;
        existing.Phone2 = company.Phone2;
        existing.Email = company.Email;
        existing.Address = company.Address;
        existing.SchemeId = company.SchemeId;
        existing.DiscountRate = company.DiscountRate;
        existing.CreditLimit = company.CreditLimit;
        existing.PaymentTerms = company.PaymentTerms;
        existing.ContractStartDate = company.ContractStartDate;
        existing.ContractEndDate = company.ContractEndDate;
        existing.BillingPeriodicity = company.BillingPeriodicity;
        existing.Notes = company.Notes;
        existing.IsActive = company.IsActive;

        await _context.SaveChangesAsync();
    }
}

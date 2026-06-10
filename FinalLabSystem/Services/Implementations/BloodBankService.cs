using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class BloodBankService : IBloodBankService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<BloodBankService> _logger;

    public BloodBankService(FinalLabDbContext context, ILogger<BloodBankService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveCrossMatchResultAsync(CrossMatchTest test, List<CrossMatchDonor> donors, int staffId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            test.TestedBy = staffId;
            test.TestedAt = DateTime.UtcNow;

            _context.CrossMatchTests.Add(test);
            await _context.SaveChangesAsync();

            foreach (var donor in donors)
            {
                donor.CrossmatchId = test.CrossmatchId;
                _context.CrossMatchDonors.Add(donor);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CrossMatchTest?> GetCrossMatchDetailsAsync(int visitTestId)
    {
        return await _context.CrossMatchTests
            .Include(ct => ct.CrossMatchDonors)
            .Include(ct => ct.VisitTest)
            .Include(ct => ct.TestedByNavigation)
            .FirstOrDefaultAsync(ct => ct.VisitTestId == visitTestId);
    }
}

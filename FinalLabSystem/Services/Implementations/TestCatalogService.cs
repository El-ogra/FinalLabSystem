using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class TestCatalogService : ITestCatalogService
{
    private readonly FinalLabDbContext _context;

    public TestCatalogService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestCategory>> GetFullHierarchyAsync()
    {
        return await _context.TestCategories
            .Include(c => c.TestGroups)
                .ThenInclude(g => g.TestTypes)
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<TestType?> GetTestTypeDetailsAsync(int testTypeId)
    {
        return await _context.TestTypes
            .Include(tt => tt.TestComponents)
                .ThenInclude(tc => tc.NormalRanges)
            .FirstOrDefaultAsync(tt => tt.TesttypeId == testTypeId);
    }

    public async Task<List<TestProfile>> GetActiveProfilesAsync()
    {
        return await _context.TestProfiles
            .Include(p => p.TestProfileItems)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<List<TestType>> GetProfileTestsAsync(int profileId)
    {
        var profile = await _context.TestProfiles
            .Include(p => p.TestProfileItems)
                .ThenInclude(tpi => tpi.TestType)
            .FirstOrDefaultAsync(p => p.ProfileId == profileId);

        return profile?.TestProfileItems
            .Select(tpi => tpi.TestType)
            .ToList() ?? new List<TestType>();
    }
}

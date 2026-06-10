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

public class ReferralService : IReferralService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<ReferralService> _logger;

    public ReferralService(FinalLabDbContext context, ILogger<ReferralService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReferralSource> AddReferralSourceAsync(ReferralSource source)
    {
        _context.ReferralSources.Add(source);
        await _context.SaveChangesAsync();
        return source;
    }

    public async Task LinkReferralToSchemeAsync(int referralId, int schemeId)
    {
        var referral = await _context.ReferralSources.FindAsync(referralId);
        if (referral == null)
            throw new InvalidOperationException($"ReferralSource with Id {referralId} not found.");

        var scheme = await _context.PriceSchemes.FindAsync(schemeId);
        if (scheme == null)
            throw new InvalidOperationException($"PriceScheme with Id {schemeId} not found.");

        referral.SchemeId = schemeId;
        await _context.SaveChangesAsync();
    }

    public async Task<List<ReferralSource>> SearchReferralSourcesAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllReferralSourcesAsync();

        return await _context.ReferralSources
            .Where(r => r.IsActive
                && (r.SourceName.Contains(term)
                    || (r.Title != null && r.Title.Contains(term))
                    || (r.Phone != null && r.Phone.Contains(term))))
            .OrderBy(r => r.SourceName)
            .Take(25)
            .ToListAsync();
    }

    public async Task<List<ReferralSource>> GetAllReferralSourcesAsync()
    {
        return await _context.ReferralSources
            .Where(r => r.IsActive)
            .OrderBy(r => r.SourceName)
            .ToListAsync();
    }

    public async Task<List<string>> GetReferralTitlesAsync()
    {
        return await _context.ReferralSources
            .Where(r => r.Title != null && r.Title != "")
            .Select(r => r.Title!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }
}

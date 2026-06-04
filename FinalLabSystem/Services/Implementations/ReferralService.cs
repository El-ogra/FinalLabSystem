using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class ReferralService : IReferralService
{
    private readonly FinalLabDbContext _context;

    public ReferralService(FinalLabDbContext context)
    {
        _context = context;
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
}

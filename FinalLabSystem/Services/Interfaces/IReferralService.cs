using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IReferralService
{
    Task<ReferralSource> AddReferralSourceAsync(ReferralSource source);
    Task LinkReferralToSchemeAsync(int referralId, int schemeId);
    Task<List<ReferralSource>> SearchReferralSourcesAsync(string term);
    Task<List<ReferralSource>> GetAllReferralSourcesAsync();
    Task<List<string>> GetReferralTitlesAsync();
}

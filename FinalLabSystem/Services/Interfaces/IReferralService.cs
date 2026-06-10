using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IReferralService
{
    /// <summary>
    /// Adds a referral source.
    /// </summary>
    /// <param name="source">The referral source to add.</param>
    /// <returns>The created referral source.</returns>
    Task<ReferralSource> AddReferralSourceAsync(ReferralSource source);

    /// <summary>
    /// Links a referral source to a pricing scheme.
    /// </summary>
    /// <param name="referralId">The referral source identifier.</param>
    /// <param name="schemeId">The pricing scheme identifier.</param>
    Task LinkReferralToSchemeAsync(int referralId, int schemeId);

    /// <summary>
    /// Searches referral sources by text.
    /// </summary>
    /// <param name="term">The search text.</param>
    /// <returns>The matching referral sources.</returns>
    Task<List<ReferralSource>> SearchReferralSourcesAsync(string term);

    /// <summary>
    /// Gets all referral sources.
    /// </summary>
    /// <returns>The referral sources.</returns>
    Task<List<ReferralSource>> GetAllReferralSourcesAsync();

    /// <summary>
    /// Gets available referral title values.
    /// </summary>
    /// <returns>The referral titles.</returns>
    Task<List<string>> GetReferralTitlesAsync();
}

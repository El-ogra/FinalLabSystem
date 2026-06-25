using FinalLabSystem.Data;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Implementations;

public class FeatureToggleService : IFeatureToggleService
{
    private readonly FinalLabDbContext _context;

    public FeatureToggleService(FinalLabDbContext context) 
    { 
        _context = context; 
    }
    
    public async Task<bool> IsEnabledAsync(string featureKey, bool defaultValue = false)
    {
        try
        {
            var setting = await _context.LabSettings.FirstOrDefaultAsync();
            return featureKey switch
            {
                FeatureToggles.EnforceStageGating => setting?.EnforceStageGating ?? true,
                FeatureToggles.EnableServerPrinting => setting?.EnableServerPrinting ?? false,
                _ => defaultValue
            };
        }
        catch
        {
            return defaultValue;
        }
    }
}

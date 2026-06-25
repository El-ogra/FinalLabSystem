namespace FinalLabSystem.Services.Interfaces;

public interface IFeatureToggleService
{
    Task<bool> IsEnabledAsync(string featureName, bool defaultValue = false);
}

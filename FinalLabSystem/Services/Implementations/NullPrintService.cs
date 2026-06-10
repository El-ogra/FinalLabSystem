using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

/// <summary>
/// No-operation print service used until a real printer or PDF exporter is integrated.
/// </summary>
public sealed class NullPrintService : IPrintService
{
    public Task PrintAsync(string documentType, object data)
    {
        // TODO: replace with real print/PDF implementation.
        return Task.CompletedTask;
    }
}

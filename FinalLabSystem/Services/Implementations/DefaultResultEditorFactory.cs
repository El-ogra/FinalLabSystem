using System.Collections.Generic;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public class DefaultResultEditorFactory : IResultEditorFactory
{
    private readonly HashSet<string> _registeredTypes = new();

    public bool HasCustomEditor(string? specialType)
    {
        return !string.IsNullOrEmpty(specialType) && _registeredTypes.Contains(specialType);
    }
}

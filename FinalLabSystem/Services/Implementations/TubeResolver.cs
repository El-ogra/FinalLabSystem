using System.Collections.Generic;
using System.Linq;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Implementations;

internal static class TubeResolver
{
    public static string ResolvePrimaryTubeIdentity(TestType test)
    {
        var primary = test.TestTypeSampleTubes
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .FirstOrDefault();

        if (primary is not null && !string.IsNullOrWhiteSpace(primary.SampleType))
            return primary.SampleType;

        if (test.CollectionType is not null && !string.IsNullOrWhiteSpace(test.CollectionType.TypeNameEn))
            return test.CollectionType.TypeNameEn;

        return "Unknown";
    }

    public static IReadOnlyList<string> ResolveAllTubes(TestType test)
    {
        return test.TestTypeSampleTubes
            .Where(t => t.IsActive && t.SampleType is not null)
            .OrderBy(t => t.SortOrder)
            .Select(t => t.SampleType!)
            .ToList();
    }
}

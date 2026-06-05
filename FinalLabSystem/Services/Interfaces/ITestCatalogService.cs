using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface ITestCatalogService
{
    Task<List<TestCategory>> GetFullHierarchyAsync();
    Task<TestType?> GetTestTypeDetailsAsync(int testTypeId);
    Task<List<TestProfile>> GetActiveProfilesAsync();
    Task<List<TestType>> GetProfileTestsAsync(int profileId);

    Task<List<TestType>> GetAllTestTypesAsync();
    Task<List<TestGroup>> GetActiveGroupsAsync();

    Task<int> CreateTestTypeAsync(
        TestType entity,
        double patientPrice,
        double labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes);

    Task UpdateTestTypeAsync(
        TestType entity,
        double patientPrice,
        double labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes);

    Task DeleteTestTypeAsync(int testTypeId);

    Task<int> AddComponentAsync(int testTypeId, TestComponent component);
    Task UpdateComponentAsync(TestComponent component);
    Task DeleteComponentAsync(int componentId);

    Task<List<NormalRange>> GetRangesForComponentAsync(int componentId);
    Task<int> AddRangeAsync(NormalRange range);
    Task UpdateRangeAsync(NormalRange range);
    Task DeleteRangeAsync(int rangeId);
}

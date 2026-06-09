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

    Task<TestCategory?> GetCategoryByIdAsync(int categoryId);
    Task<TestCategory> CreateCategoryAsync(TestCategory category);
    Task<TestCategory> UpdateCategoryAsync(TestCategory category);
    Task DeleteCategoryAsync(int categoryId);

    Task<TestGroup?> GetGroupByIdAsync(int groupId);
    Task<TestGroup> CreateGroupAsync(TestGroup group);
    Task<TestGroup> UpdateGroupAsync(TestGroup group);
    Task DeleteGroupAsync(int groupId);
    Task<List<TestGroup>> GetGroupsByCategoryIdAsync(int categoryId);

    Task<List<CollectionType>> GetAllCollectionTypesAsync();
    Task<CollectionType?> GetCollectionTypeByIdAsync(int collectionTypeId);
    Task<CollectionType> CreateCollectionTypeAsync(CollectionType collectionType);
    Task<CollectionType> UpdateCollectionTypeAsync(CollectionType collectionType);
    Task<bool> DeleteCollectionTypeAsync(int collectionTypeId);
    Task<bool> CollectionTypeHasTestTypesAsync(int collectionTypeId);
}

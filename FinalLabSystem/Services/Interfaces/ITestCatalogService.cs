using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Services.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface ITestCatalogService
{
    Task<List<TestCategory>> GetFullHierarchyAsync();
    Task<TestType?> GetTestTypeDetailsAsync(int testTypeId);
    Task<List<TestProfile>> GetActiveProfilesAsync();
    Task<List<TestType>> GetProfileTestsAsync(int profileId);

    Task<PagedResult<TestType>> GetTestTypesPagedAsync(int page = 1, int pageSize = 50);

    Task<List<TestType>> GetAllTestTypesAsync();
    Task<List<TestGroup>> GetActiveGroupsAsync();

    Task<int> CreateTestTypeAsync(
        TestType entity,
        decimal patientPrice,
        decimal labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes);

    Task UpdateTestTypeAsync(
        TestType entity,
        decimal patientPrice,
        decimal labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes);

    Task DeleteTestTypeAsync(int testTypeId);

    Task<int> AddComponentAsync(int testTypeId, TestComponent component);
    Task UpdateComponentAsync(TestComponent component);
    Task DeleteComponentAsync(int componentId);

    Task<List<NormalRange>> GetRangesForComponentAsync(int componentId);
    Task<int> AddRangeAsync(NormalRange range);
    Task UpdateRangeAsync(NormalRange range);
    Task DeleteRangeAsync(int rangeId);
    Task<NormalRange> SaveRangeAsync(NormalRange range);
    Task<List<NormalRange>> GetRangesForTestTypeAsync(int testTypeId);

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

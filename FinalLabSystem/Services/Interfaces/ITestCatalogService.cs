using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Services.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface ITestCatalogService
{
    /// <summary>
    /// Gets the full active test catalog hierarchy.
    /// </summary>
    /// <returns>The list of test categories with child groups and tests.</returns>
    Task<List<TestCategory>> GetFullHierarchyAsync();

    /// <summary>
    /// Gets detailed data for a test type.
    /// </summary>
    /// <param name="testTypeId">The test type identifier.</param>
    /// <returns>The test type details, or <c>null</c> when no test exists.</returns>
    Task<TestType?> GetTestTypeDetailsAsync(int testTypeId);

    /// <summary>
    /// Gets active test profiles.
    /// </summary>
    /// <returns>The active test profiles.</returns>
    Task<List<TestProfile>> GetActiveProfilesAsync();

    /// <summary>
    /// Gets the tests included in a profile.
    /// </summary>
    /// <param name="profileId">The profile identifier.</param>
    /// <returns>The test types in the profile.</returns>
    Task<List<TestType>> GetProfileTestsAsync(int profileId);

    /// <summary>
    /// Gets test types using paging.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of test types to return.</param>
    /// <returns>A paged result containing test types.</returns>
    Task<PagedResult<TestType>> GetTestTypesPagedAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets all test types.
    /// </summary>
    /// <returns>The list of test types.</returns>
    Task<List<TestType>> GetAllTestTypesAsync();

    /// <summary>
    /// Gets active test groups.
    /// </summary>
    /// <returns>The active test groups.</returns>
    Task<List<TestGroup>> GetActiveGroupsAsync();

    /// <summary>
    /// Creates a test type with prices and sample tubes.
    /// </summary>
    /// <param name="entity">The test type to create.</param>
    /// <param name="patientPrice">The patient price.</param>
    /// <param name="labToLabPrice">The lab-to-lab price.</param>
    /// <param name="tubes">The sample tube mappings.</param>
    /// <returns>The created test type identifier.</returns>
    Task<int> CreateTestTypeAsync(
        TestType entity,
        decimal patientPrice,
        decimal labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes);

    /// <summary>
    /// Updates a test type with prices and sample tubes.
    /// </summary>
    /// <param name="entity">The test type containing updated values.</param>
    /// <param name="patientPrice">The updated patient price.</param>
    /// <param name="labToLabPrice">The updated lab-to-lab price.</param>
    /// <param name="tubes">The updated sample tube mappings.</param>
    Task UpdateTestTypeAsync(
        TestType entity,
        decimal patientPrice,
        decimal labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes);

    /// <summary>
    /// Deletes a test type.
    /// </summary>
    /// <param name="testTypeId">The test type identifier.</param>
    Task DeleteTestTypeAsync(int testTypeId);

    /// <summary>
    /// Adds a component to a test type.
    /// </summary>
    /// <param name="testTypeId">The parent test type identifier.</param>
    /// <param name="component">The component to add.</param>
    /// <returns>The created component identifier.</returns>
    Task<int> AddComponentAsync(int testTypeId, TestComponent component);

    /// <summary>
    /// Updates a test component.
    /// </summary>
    /// <param name="component">The component containing updated values.</param>
    Task UpdateComponentAsync(TestComponent component);

    /// <summary>
    /// Deletes a test component.
    /// </summary>
    /// <param name="componentId">The component identifier.</param>
    Task DeleteComponentAsync(int componentId);

    /// <summary>
    /// Saves all components for a test type (adds, updates, and deletes as needed).
    /// </summary>
    /// <param name="testTypeId">The test type identifier.</param>
    /// <param name="components">The components to persist.</param>
    Task SaveTestComponentsAsync(int testTypeId, IReadOnlyList<TestComponent> components);

    /// <summary>
    /// Gets normal ranges for a component.
    /// </summary>
    /// <param name="componentId">The component identifier.</param>
    /// <returns>The normal ranges for the component.</returns>
    Task<List<NormalRange>> GetRangesForComponentAsync(int componentId);

    /// <summary>
    /// Adds a normal range.
    /// </summary>
    /// <param name="range">The normal range to add.</param>
    /// <returns>The created range identifier.</returns>
    Task<int> AddRangeAsync(NormalRange range);

    /// <summary>
    /// Updates a normal range.
    /// </summary>
    /// <param name="range">The normal range containing updated values.</param>
    Task UpdateRangeAsync(NormalRange range);

    /// <summary>
    /// Deletes a normal range.
    /// </summary>
    /// <param name="rangeId">The normal range identifier.</param>
    Task DeleteRangeAsync(int rangeId);

    /// <summary>
    /// Creates or updates a normal range.
    /// </summary>
    /// <param name="range">The normal range to save.</param>
    /// <returns>The saved normal range.</returns>
    Task<NormalRange> SaveRangeAsync(NormalRange range);

    /// <summary>
    /// Gets normal ranges for all components under a test type.
    /// </summary>
    /// <param name="testTypeId">The test type identifier.</param>
    /// <returns>The normal ranges for the test type.</returns>
    Task<List<NormalRange>> GetRangesForTestTypeAsync(int testTypeId);

    /// <summary>
    /// Gets a test category by identifier.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The matching category, or <c>null</c> when no category exists.</returns>
    Task<TestCategory?> GetCategoryByIdAsync(int categoryId);

    /// <summary>
    /// Creates a test category.
    /// </summary>
    /// <param name="category">The category to create.</param>
    /// <returns>The created category.</returns>
    Task<TestCategory> CreateCategoryAsync(TestCategory category);

    /// <summary>
    /// Updates a test category.
    /// </summary>
    /// <param name="category">The category containing updated values.</param>
    /// <returns>The updated category.</returns>
    Task<TestCategory> UpdateCategoryAsync(TestCategory category);

    /// <summary>
    /// Deletes a test category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    Task DeleteCategoryAsync(int categoryId);

    /// <summary>
    /// Gets a test group by identifier.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    /// <returns>The matching group, or <c>null</c> when no group exists.</returns>
    Task<TestGroup?> GetGroupByIdAsync(int groupId);

    /// <summary>
    /// Creates a test group.
    /// </summary>
    /// <param name="group">The group to create.</param>
    /// <returns>The created group.</returns>
    Task<TestGroup> CreateGroupAsync(TestGroup group);

    /// <summary>
    /// Updates a test group.
    /// </summary>
    /// <param name="group">The group containing updated values.</param>
    /// <returns>The updated group.</returns>
    Task<TestGroup> UpdateGroupAsync(TestGroup group);

    /// <summary>
    /// Deletes a test group.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    Task DeleteGroupAsync(int groupId);

    /// <summary>
    /// Gets groups for a category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The groups in the category.</returns>
    Task<List<TestGroup>> GetGroupsByCategoryIdAsync(int categoryId);

    /// <summary>
    /// Gets all collection types.
    /// </summary>
    /// <returns>The available collection types.</returns>
    Task<List<CollectionType>> GetAllCollectionTypesAsync();

    /// <summary>
    /// Gets a collection type by identifier.
    /// </summary>
    /// <param name="collectionTypeId">The collection type identifier.</param>
    /// <returns>The matching collection type, or <c>null</c> when no collection type exists.</returns>
    Task<CollectionType?> GetCollectionTypeByIdAsync(int collectionTypeId);

    /// <summary>
    /// Creates a collection type.
    /// </summary>
    /// <param name="collectionType">The collection type to create.</param>
    /// <returns>The created collection type.</returns>
    Task<CollectionType> CreateCollectionTypeAsync(CollectionType collectionType);

    /// <summary>
    /// Updates a collection type.
    /// </summary>
    /// <param name="collectionType">The collection type containing updated values.</param>
    /// <returns>The updated collection type.</returns>
    Task<CollectionType> UpdateCollectionTypeAsync(CollectionType collectionType);

    /// <summary>
    /// Deletes a collection type.
    /// </summary>
    /// <param name="collectionTypeId">The collection type identifier.</param>
    /// <returns><c>true</c> when the collection type was deleted; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteCollectionTypeAsync(int collectionTypeId);

    /// <summary>
    /// Determines whether a collection type is used by any test type.
    /// </summary>
    /// <param name="collectionTypeId">The collection type identifier.</param>
    /// <returns><c>true</c> when test types reference the collection type; otherwise, <c>false</c>.</returns>
    Task<bool> CollectionTypeHasTestTypesAsync(int collectionTypeId);

    /// <summary>
    /// Gets all units ordered by sort order then name.
    /// </summary>
    Task<List<Unit>> GetAllUnitsAsync();

    /// <summary>
    /// Gets a unit by identifier.
    /// </summary>
    Task<Unit?> GetUnitByIdAsync(int unitId);

    /// <summary>
    /// Creates a new unit.
    /// </summary>
    Task<Unit> CreateUnitAsync(Unit unit);

    /// <summary>
    /// Updates an existing unit.
    /// </summary>
    Task<Unit> UpdateUnitAsync(Unit unit);

    /// <summary>
    /// Deletes a unit if it is not in use.
    /// </summary>
    Task<bool> DeleteUnitAsync(int unitId);

    /// <summary>
    /// Gets the fixed list of reference classifications.
    /// </summary>
    Task<List<ReferenceClassification>> GetReferenceClassificationsAsync();

    /// <summary>
    /// Gets all tube materials ordered by sort order then name.
    /// </summary>
    Task<List<TubeMaterial>> GetAllTubeMaterialsAsync();

    /// <summary>
    /// Gets a tube material by identifier.
    /// </summary>
    Task<TubeMaterial?> GetTubeMaterialByIdAsync(int tubeMaterialId);

    /// <summary>
    /// Creates a new tube material.
    /// </summary>
    Task<TubeMaterial> CreateTubeMaterialAsync(TubeMaterial tubeMaterial);

    /// <summary>
    /// Updates an existing tube material.
    /// </summary>
    Task<TubeMaterial> UpdateTubeMaterialAsync(TubeMaterial tubeMaterial);

    /// <summary>
    /// Deletes a tube material if it is not in use by test type sample tubes.
    /// </summary>
    Task<bool> DeleteTubeMaterialAsync(int tubeMaterialId);

    /// <summary>
    /// Gets all profiles including items.
    /// </summary>
    Task<List<TestProfile>> GetAllProfilesAsync();

    /// <summary>
    /// Creates a new test profile.
    /// </summary>
    Task<TestProfile> CreateProfileAsync(TestProfile profile);

    /// <summary>
    /// Updates an existing test profile.
    /// </summary>
    Task UpdateProfileAsync(TestProfile profile);

    /// <summary>
    /// Soft-deletes a test profile by setting IsActive = false.
    /// </summary>
    Task DeleteProfileAsync(int profileId);

    /// <summary>
    /// Adds a test to a profile.
    /// </summary>
    Task AddProfileItemAsync(int profileId, int testTypeId, int? sortOrder);

    /// <summary>
    /// Removes a test from a profile.
    /// </summary>
    Task RemoveProfileItemAsync(int profileItemId);

    /// <summary>
    /// Updates the sort order of a profile item.
    /// </summary>
    Task UpdateProfileItemSortOrderAsync(int profileItemId, int sortOrder);
}

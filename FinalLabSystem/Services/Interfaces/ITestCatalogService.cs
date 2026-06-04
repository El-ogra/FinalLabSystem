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
}

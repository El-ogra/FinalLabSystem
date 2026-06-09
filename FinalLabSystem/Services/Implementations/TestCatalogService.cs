using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class TestCatalogService : ITestCatalogService
{
    private const string PatientSchemeName = "Patient Price";
    private const string LabToLabSchemeName = "Lab-to-Lab Price";

    private readonly FinalLabDbContext _context;

    public TestCatalogService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestCategory>> GetFullHierarchyAsync()
    {
        return await _context.TestCategories
            .Include(c => c.TestGroups)
                .ThenInclude(g => g.TestTypes)
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<TestType?> GetTestTypeDetailsAsync(int testTypeId)
    {
        return await _context.TestTypes
            .Include(tt => tt.Group)
            .Include(tt => tt.TestComponents)
                .ThenInclude(tc => tc.NormalRanges)
            .Include(tt => tt.TestTypePrices)
                .ThenInclude(p => p.Scheme)
            .Include(tt => tt.TestTypeSampleTubes)
            .FirstOrDefaultAsync(tt => tt.TesttypeId == testTypeId);
    }

    public async Task<List<TestProfile>> GetActiveProfilesAsync()
    {
        return await _context.TestProfiles
            .Include(p => p.TestProfileItems)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<List<TestType>> GetProfileTestsAsync(int profileId)
    {
        var profile = await _context.TestProfiles
            .Include(p => p.TestProfileItems)
                .ThenInclude(tpi => tpi.TestType)
            .FirstOrDefaultAsync(p => p.ProfileId == profileId);

        return profile?.TestProfileItems
            .Select(tpi => tpi.TestType)
            .ToList() ?? new List<TestType>();
    }

    public async Task<List<TestType>> GetAllTestTypesAsync()
    {
        return await _context.TestTypes
            .AsNoTracking()
            .Include(tt => tt.Group)
            .Include(tt => tt.TestComponents)
            .Include(tt => tt.TestTypePrices)
                .ThenInclude(p => p.Scheme)
            .Include(tt => tt.TestTypeSampleTubes)
            .OrderBy(tt => tt.SortOrder)
            .ThenBy(tt => tt.TypeNameEn)
            .ToListAsync();
    }

    public async Task<List<TestGroup>> GetActiveGroupsAsync()
    {
        return await _context.TestGroups
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.GroupNameEn)
            .ToListAsync();
    }

    public async Task<int> CreateTestTypeAsync(
        TestType entity,
        double patientPrice,
        double labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        _context.TestTypes.Add(entity);
        await _context.SaveChangesAsync();

        var (patientSchemeId, labToLabSchemeId) = await ResolveSchemeIdsAsync();

        _context.TestTypePrices.Add(new TestTypePrice
        {
            TesttypeId = entity.TesttypeId,
            SchemeId = patientSchemeId,
            Price = patientPrice,
        });
        _context.TestTypePrices.Add(new TestTypePrice
        {
            TesttypeId = entity.TesttypeId,
            SchemeId = labToLabSchemeId,
            Price = labToLabPrice,
        });

        if (tubes is not null)
        {
            foreach (var tube in tubes)
            {
                tube.TestTypeId = entity.TesttypeId;
                _context.TestTypeSampleTubes.Add(tube);
            }
        }

        await _context.SaveChangesAsync();
        return entity.TesttypeId;
    }

    public async Task UpdateTestTypeAsync(
        TestType entity,
        double patientPrice,
        double labToLabPrice,
        IReadOnlyList<TestTypeSampleTube> tubes)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var existing = await _context.TestTypes
            .Include(tt => tt.TestTypePrices)
            .Include(tt => tt.TestTypeSampleTubes)
            .FirstOrDefaultAsync(tt => tt.TesttypeId == entity.TesttypeId)
            ?? throw new InvalidOperationException($"TestType {entity.TesttypeId} not found.");

        existing.GroupId = entity.GroupId;
        existing.TypeCode = entity.TypeCode;
        existing.TypeNameEn = entity.TypeNameEn;
        existing.TypeNameAr = entity.TypeNameAr;
        existing.TypeAbbrev = entity.TypeAbbrev;
        existing.DefaultPrice = entity.DefaultPrice;
        existing.SampleType = entity.SampleType;
        existing.DefaultTubeType = entity.DefaultTubeType;
        existing.DefaultTubeColor = entity.DefaultTubeColor;
        existing.TurnaroundHours = entity.TurnaroundHours;
        existing.IsOutsourceable = entity.IsOutsourceable;
        existing.SpecialType = entity.SpecialType;
        existing.SortOrder = entity.SortOrder;
        existing.IsActive = entity.IsActive;
        existing.Notes = entity.Notes;
        existing.ReportNameLine1 = entity.ReportNameLine1;
        existing.ReportNameLine2 = entity.ReportNameLine2;
        existing.BillNameLine1 = entity.BillNameLine1;
        existing.BillNameLine2 = entity.BillNameLine2;
        existing.HistoryName = entity.HistoryName;
        existing.CollectionNotes = entity.CollectionNotes;
        existing.CollectionTypeId = entity.CollectionTypeId;
        existing.IsRoutineTest = entity.IsRoutineTest;
        existing.SeeReport = entity.SeeReport;
        existing.PrintWithOther = entity.PrintWithOther;
        existing.AddWithGroup = entity.AddWithGroup;
        existing.IsMainTest = entity.IsMainTest;
        existing.IsSendOutside = entity.IsSendOutside;
        existing.OutsideLabName = entity.OutsideLabName;
        existing.OutsideCostPrice = entity.OutsideCostPrice;
        existing.PatientQuestion = entity.PatientQuestion;

        var (patientSchemeId, labToLabSchemeId) = await ResolveSchemeIdsAsync();

        var patientRow = existing.TestTypePrices.FirstOrDefault(p => p.SchemeId == patientSchemeId);
        if (patientRow is null)
        {
            _context.TestTypePrices.Add(new TestTypePrice
            {
                TesttypeId = existing.TesttypeId,
                SchemeId = patientSchemeId,
                Price = patientPrice,
            });
        }
        else
        {
            patientRow.Price = patientPrice;
        }

        var labRow = existing.TestTypePrices.FirstOrDefault(p => p.SchemeId == labToLabSchemeId);
        if (labRow is null)
        {
            _context.TestTypePrices.Add(new TestTypePrice
            {
                TesttypeId = existing.TesttypeId,
                SchemeId = labToLabSchemeId,
                Price = labToLabPrice,
            });
        }
        else
        {
            labRow.Price = labToLabPrice;
        }

        _context.TestTypeSampleTubes.RemoveRange(existing.TestTypeSampleTubes);
        if (tubes is not null)
        {
            foreach (var tube in tubes)
            {
                tube.TestTypeTubeId = 0;
                tube.TestTypeId = existing.TesttypeId;
                _context.TestTypeSampleTubes.Add(tube);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTestTypeAsync(int testTypeId)
    {
        var entity = await _context.TestTypes.FirstOrDefaultAsync(tt => tt.TesttypeId == testTypeId);
        if (entity is null) return;

        var hasVisits = await _context.VisitTests.AnyAsync(vt => vt.TesttypeId == testTypeId);
        if (hasVisits)
        {
            entity.IsActive = false;
        }
        else
        {
            _context.TestTypes.Remove(entity);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> AddComponentAsync(int testTypeId, TestComponent component)
    {
        if (component is null) throw new ArgumentNullException(nameof(component));

        component.TesttypeId = testTypeId;
        _context.TestComponents.Add(component);
        await _context.SaveChangesAsync();
        return component.ComponentId;
    }

    public async Task UpdateComponentAsync(TestComponent component)
    {
        if (component is null) throw new ArgumentNullException(nameof(component));

        var existing = await _context.TestComponents.FirstOrDefaultAsync(tc => tc.ComponentId == component.ComponentId)
            ?? throw new InvalidOperationException($"TestComponent {component.ComponentId} not found.");

        existing.TesttypeId = component.TesttypeId;
        existing.ComponentCode = component.ComponentCode;
        existing.ComponentNameEn = component.ComponentNameEn;
        existing.ComponentNameAr = component.ComponentNameAr;
        existing.Unit = component.Unit;
        existing.ResultType = component.ResultType;
        existing.DecimalPlaces = component.DecimalPlaces;
        existing.SortOrder = component.SortOrder;
        existing.IsActive = component.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteComponentAsync(int componentId)
    {
        var component = await _context.TestComponents
            .Include(tc => tc.NormalRanges)
            .FirstOrDefaultAsync(tc => tc.ComponentId == componentId);
        if (component is null) return;

        _context.NormalRanges.RemoveRange(component.NormalRanges);
        _context.TestComponents.Remove(component);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NormalRange>> GetRangesForComponentAsync(int componentId)
    {
        return await _context.NormalRanges
            .AsNoTracking()
            .Where(r => r.ComponentId == componentId)
            .OrderBy(r => r.Sex)
            .ThenBy(r => r.AgeFromDays)
            .ToListAsync();
    }

    public async Task<int> AddRangeAsync(NormalRange range)
    {
        if (range is null) throw new ArgumentNullException(nameof(range));

        _context.NormalRanges.Add(range);
        await _context.SaveChangesAsync();
        return range.RangeId;
    }

    public async Task UpdateRangeAsync(NormalRange range)
    {
        if (range is null) throw new ArgumentNullException(nameof(range));

        var existing = await _context.NormalRanges.FirstOrDefaultAsync(r => r.RangeId == range.RangeId)
            ?? throw new InvalidOperationException($"NormalRange {range.RangeId} not found.");

        existing.ComponentId = range.ComponentId;
        existing.Sex = range.Sex;
        existing.AgeFromDays = range.AgeFromDays;
        existing.AgeToDays = range.AgeToDays;
        existing.AgeDescription = range.AgeDescription;
        existing.AppliesToPregnant = range.AppliesToPregnant;
        existing.AgeUnit = range.AgeUnit;
        existing.LowFlag = range.LowFlag;
        existing.HighFlag = range.HighFlag;
        existing.LowComment = range.LowComment;
        existing.HighComment = range.HighComment;
        existing.CriticalRangeText = range.CriticalRangeText;
        existing.CriticalFlag = range.CriticalFlag;
        existing.CriticalComment = range.CriticalComment;
        existing.FastingState = range.FastingState;
        existing.LowNormal = range.LowNormal;
        existing.HighNormal = range.HighNormal;
        existing.LowCritical = range.LowCritical;
        existing.HighCritical = range.HighCritical;
        existing.NormalRangeText = range.NormalRangeText;
        existing.RangeNote = range.RangeNote;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(int rangeId)
    {
        var range = await _context.NormalRanges.FirstOrDefaultAsync(r => r.RangeId == rangeId);
        if (range is null) return;

        _context.NormalRanges.Remove(range);
        await _context.SaveChangesAsync();
    }

    public async Task<TestCategory?> GetCategoryByIdAsync(int categoryId)
    {
        return await _context.TestCategories.FindAsync(categoryId);
    }

    public async Task<TestCategory> CreateCategoryAsync(TestCategory category)
    {
        _context.TestCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<TestCategory> UpdateCategoryAsync(TestCategory category)
    {
        _context.TestCategories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.TestCategories.FindAsync(categoryId);
        if (category is null) return;

        var hasGroups = await _context.TestGroups.AnyAsync(g => g.CategoryId == categoryId);
        if (hasGroups)
        {
            throw new InvalidOperationException("لا يمكن حذف هذه الفئة لأنها تحتوي على مجموعات");
        }

        _context.TestCategories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<TestGroup?> GetGroupByIdAsync(int groupId)
    {
        return await _context.TestGroups.FindAsync(groupId);
    }

    public async Task<TestGroup> CreateGroupAsync(TestGroup group)
    {
        _context.TestGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<TestGroup> UpdateGroupAsync(TestGroup group)
    {
        _context.TestGroups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task DeleteGroupAsync(int groupId)
    {
        var group = await _context.TestGroups.FindAsync(groupId);
        if (group is null) return;

        _context.TestGroups.Remove(group);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TestGroup>> GetGroupsByCategoryIdAsync(int categoryId)
    {
        return await _context.TestGroups
            .Where(g => g.CategoryId == categoryId)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
    }

    public async Task<List<CollectionType>> GetAllCollectionTypesAsync()
    {
        return await _context.CollectionTypes
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.TypeNameEn)
            .ToListAsync();
    }

    public async Task<CollectionType?> GetCollectionTypeByIdAsync(int collectionTypeId)
    {
        return await _context.CollectionTypes.FindAsync(collectionTypeId);
    }

    public async Task<CollectionType> CreateCollectionTypeAsync(CollectionType collectionType)
    {
        _context.CollectionTypes.Add(collectionType);
        await _context.SaveChangesAsync();
        return collectionType;
    }

    public async Task<CollectionType> UpdateCollectionTypeAsync(CollectionType collectionType)
    {
        _context.CollectionTypes.Update(collectionType);
        await _context.SaveChangesAsync();
        return collectionType;
    }

    public async Task<bool> DeleteCollectionTypeAsync(int collectionTypeId)
    {
        var entity = await _context.CollectionTypes.FindAsync(collectionTypeId);
        if (entity is null) return false;

        var hasTestTypes = await _context.TestTypes.AnyAsync(tt => tt.CollectionTypeId == collectionTypeId);
        if (hasTestTypes)
        {
            throw new InvalidOperationException("لا يمكن حذف هذا النوع لأنه مرتبط بتحاليل");
        }

        _context.CollectionTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CollectionTypeHasTestTypesAsync(int collectionTypeId)
    {
        return await _context.TestTypes.AnyAsync(tt => tt.CollectionTypeId == collectionTypeId);
    }

    private async Task<(int PatientSchemeId, int LabToLabSchemeId)> ResolveSchemeIdsAsync()
    {
        var schemes = await _context.PriceSchemes
            .Where(s => s.SchemeName == PatientSchemeName || s.SchemeName == LabToLabSchemeName)
            .Select(s => new { s.SchemeId, s.SchemeName })
            .ToListAsync();

        var patient = schemes.FirstOrDefault(s => s.SchemeName == PatientSchemeName)
            ?? throw new InvalidOperationException($"PriceScheme '{PatientSchemeName}' not found. Run migrations.");
        var lab = schemes.FirstOrDefault(s => s.SchemeName == LabToLabSchemeName)
            ?? throw new InvalidOperationException($"PriceScheme '{LabToLabSchemeName}' not found. Run migrations.");

        return (patient.SchemeId, lab.SchemeId);
    }
}

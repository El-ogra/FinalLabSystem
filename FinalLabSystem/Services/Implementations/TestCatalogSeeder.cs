using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class TestCatalogSeeder : ITestCatalogSeeder
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<TestCatalogSeeder> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TestCatalogSeeder(
        FinalLabDbContext context,
        ILogger<TestCatalogSeeder> logger,
        IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // ── Step 1: Resolve CSV file path ──
        var csvPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Docs", "SeedData",
            "NormalRanges_CommonTests_schema_compatible_filtered.csv");

        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("Seed CSV not found at {Path}. Skipping test catalog seed.", csvPath);
            return;
        }

        // ── Step 2: Read CSV with CsvHelper ──
        List<CsvSeedRow> allRows;
        using (var reader = new StreamReader(csvPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
        }))
        {
            allRows = csv.GetRecords<CsvSeedRow>().ToList();
        }

        var rows = allRows
            .Where(r => string.Equals(r.SeedDecision, "KEEP", StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.LogInformation("Seed CSV loaded: {Total} rows read, {Keep} rows with KEEP decision.",
            allRows.Count, rows.Count);

        if (rows.Count == 0)
        {
            _logger.LogInformation("No KEEP rows in seed CSV. Nothing to seed.");
            return;
        }

        // ── Step 3: Open a single database transaction ──
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // ── Step 4: Ensure default CollectionType exists ──
            var collectionType = await _context.CollectionTypes
                .FirstOrDefaultAsync(ct => ct.TypeNameEn == "Standard Venous Blood", cancellationToken);

            if (collectionType is null)
            {
                collectionType = new CollectionType
                {
                    TypeNameEn = "Standard Venous Blood",
                    TypeNameAr = null,
                    SortOrder = 1,
                    IsActive = true
                };
                _context.CollectionTypes.Add(collectionType);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Created default CollectionType 'Standard Venous Blood' (Id={Id}).",
                    collectionType.CollectionTypeId);
            }

            var collectionTypeId = collectionType.CollectionTypeId;

            // ── Step 5: Upsert TestCategory (8 rows) ──
            var distinctCategories = rows
                .GroupBy(r => r.CategoryCode)
                .Select(g => g.First())
                .ToList();

            var categoryMap = new Dictionary<string, int>();
            var catInserted = 0;
            var catUpdated = 0;

            foreach (var row in distinctCategories)
            {
                var existing = await _context.TestCategories
                    .FirstOrDefaultAsync(c => c.CategoryCode == row.CategoryCode, cancellationToken);

                if (existing is null)
                {
                    var entity = new TestCategory
                    {
                        CategoryCode = row.CategoryCode,
                        CategoryNameEn = row.CategoryNameEn,
                        CategoryNameAr = null,
                        SortOrder = (short)row.CategorySortOrder,
                        IsActive = row.CategoryIsActive
                    };
                    _context.TestCategories.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    categoryMap[row.CategoryCode] = entity.CategoryId;
                    catInserted++;
                }
                else
                {
                    existing.CategoryNameEn = row.CategoryNameEn;
                    existing.CategoryNameAr = null;
                    existing.SortOrder = (short)row.CategorySortOrder;
                    existing.IsActive = row.CategoryIsActive;
                    await _context.SaveChangesAsync(cancellationToken);
                    categoryMap[row.CategoryCode] = existing.CategoryId;
                    catUpdated++;
                }
            }

            // ── Step 6: Upsert TestGroup (21 rows) ──
            var distinctGroups = rows
                .GroupBy(r => r.GroupCode)
                .Select(g => g.First())
                .ToList();

            var groupMap = new Dictionary<string, int>();
            var grpInserted = 0;
            var grpUpdated = 0;

            foreach (var row in distinctGroups)
            {
                var categoryId = categoryMap[row.CategoryCode];

                var existing = await _context.TestGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == row.GroupCode, cancellationToken);

                if (existing is null)
                {
                    var entity = new TestGroup
                    {
                        GroupCode = row.GroupCode,
                        GroupNameEn = row.GroupNameEn,
                        GroupNameAr = null,
                        CategoryId = categoryId,
                        SortOrder = (short)row.GroupSortOrder,
                        IsActive = row.GroupIsActive
                    };
                    _context.TestGroups.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    groupMap[row.GroupCode] = entity.GroupId;
                    grpInserted++;
                }
                else
                {
                    existing.CategoryId = categoryId;
                    existing.GroupNameEn = row.GroupNameEn;
                    existing.GroupNameAr = null;
                    existing.SortOrder = (short)row.GroupSortOrder;
                    existing.IsActive = row.GroupIsActive;
                    await _context.SaveChangesAsync(cancellationToken);
                    groupMap[row.GroupCode] = existing.GroupId;
                    grpUpdated++;
                }
            }

            // ── Step 7: Upsert TestType (57 rows) ──
            var distinctTypes = rows
                .GroupBy(r => r.TypeCode)
                .Select(g => g.First())
                .ToList();

            var typeMap = new Dictionary<string, int>();
            var typeInserted = 0;
            var typeUpdated = 0;

            foreach (var row in distinctTypes)
            {
                var groupId = groupMap[row.GroupCode];

                var existing = await _context.TestTypes
                    .FirstOrDefaultAsync(t => t.TypeCode == row.TypeCode, cancellationToken);

                if (existing is null)
                {
                    var entity = new TestType
                    {
                        TypeCode = row.TypeCode,
                        TypeNameEn = row.TypeNameEn,
                        TypeNameAr = null,
                        TypeAbbrev = row.TypeAbbrev,
                        DefaultPrice = 50m,
                        SampleType = row.TypeSampleType,
                        DefaultTubeType = row.TypeDefaultTubeType,
                        DefaultTubeColor = row.TypeDefaultTubeColor,
                        TurnaroundHours = row.TypeTurnaroundHours ?? (short)24,
                        SpecialType = row.TypeSpecialType,
                        SortOrder = (short)row.TypeSortOrder,
                        IsActive = row.TypeIsActive,
                        Notes = row.TypeNotes,
                        ReportNameLine1 = row.TypeReportNameLine1,
                        ReportNameLine2 = row.TypeReportNameLine2,
                        BillNameLine1 = row.TypeBillNameLine1,
                        BillNameLine2 = row.TypeBillNameLine2,
                        HistoryName = row.TypeHistoryName,
                        CollectionNotes = row.TypeCollectionNotes,
                        CollectionTypeId = collectionTypeId,
                        OutsideLabName = row.TypeOutsideLabName,
                        OutsideCostPrice = row.TypeOutsideCostPrice,
                        PatientQuestion = row.TypePatientQuestion,
                        ReferenceType = row.TypeReferenceType,
                        BarcodeName = row.TypeBarcodeName,
                        GroupId = groupId,
                        Behavior = TestTypeBehavior.None
                    };

                    entity.IsRoutineTest = row.TypeIsRoutineTest;
                    entity.SeeReport = row.TypeSeeReport;
                    entity.PrintWithOther = row.TypePrintWithOther;
                    entity.AddWithGroup = row.TypeAddWithGroup;
                    entity.IsMainTest = row.TypeIsMainTest;
                    entity.IsSendOutside = row.TypeIsSendOutside;

                    _context.TestTypes.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    typeMap[row.TypeCode] = entity.TesttypeId;
                    typeInserted++;
                }
                else
                {
                    existing.GroupId = groupId;
                    existing.TypeNameEn = row.TypeNameEn;
                    existing.TypeNameAr = null;
                    existing.TypeAbbrev = row.TypeAbbrev;
                    existing.DefaultPrice = 50m;
                    existing.SampleType = row.TypeSampleType;
                    existing.DefaultTubeType = row.TypeDefaultTubeType;
                    existing.DefaultTubeColor = row.TypeDefaultTubeColor;
                    existing.TurnaroundHours = row.TypeTurnaroundHours ?? (short)24;
                    existing.SpecialType = row.TypeSpecialType;
                    existing.SortOrder = (short)row.TypeSortOrder;
                    existing.IsActive = row.TypeIsActive;
                    existing.Notes = row.TypeNotes;
                    existing.ReportNameLine1 = row.TypeReportNameLine1;
                    existing.ReportNameLine2 = row.TypeReportNameLine2;
                    existing.BillNameLine1 = row.TypeBillNameLine1;
                    existing.BillNameLine2 = row.TypeBillNameLine2;
                    existing.HistoryName = row.TypeHistoryName;
                    existing.CollectionNotes = row.TypeCollectionNotes;
                    existing.CollectionTypeId = collectionTypeId;
                    existing.OutsideLabName = row.TypeOutsideLabName;
                    existing.OutsideCostPrice = row.TypeOutsideCostPrice;
                    existing.PatientQuestion = row.TypePatientQuestion;
                    existing.ReferenceType = row.TypeReferenceType;
                    existing.BarcodeName = row.TypeBarcodeName;

                    existing.IsRoutineTest = row.TypeIsRoutineTest;
                    existing.SeeReport = row.TypeSeeReport;
                    existing.PrintWithOther = row.TypePrintWithOther;
                    existing.AddWithGroup = row.TypeAddWithGroup;
                    existing.IsMainTest = row.TypeIsMainTest;
                    existing.IsSendOutside = row.TypeIsSendOutside;

                    await _context.SaveChangesAsync(cancellationToken);
                    typeMap[row.TypeCode] = existing.TesttypeId;
                    typeUpdated++;
                }
            }

            // ── Step 8: Upsert TestComponent (74 rows) ──
            var distinctComponents = rows
                .GroupBy(r => new { r.TypeCode, r.ComponentCode })
                .Select(g => g.First())
                .ToList();

            var componentMap = new Dictionary<(int TesttypeId, string ComponentCode), int>();
            var compInserted = 0;
            var compUpdated = 0;

            foreach (var row in distinctComponents)
            {
                var testtypeId = typeMap[row.TypeCode];

                var existing = await _context.TestComponents
                    .FirstOrDefaultAsync(
                        c => c.TesttypeId == testtypeId && c.ComponentCode == row.ComponentCode,
                        cancellationToken);

                if (existing is null)
                {
                    var entity = new TestComponent
                    {
                        TesttypeId = testtypeId,
                        ComponentCode = row.ComponentCode,
                        ComponentNameEn = row.ComponentNameEn,
                        ComponentNameAr = null,
                        Unit = row.ComponentUnit,
                        ResultType = row.ComponentResultType,
                        DecimalPlaces = (byte)row.ComponentDecimalPlaces,
                        SortOrder = (short)row.ComponentSortOrder,
                        IsActive = row.ComponentIsActive
                    };
                    _context.TestComponents.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    componentMap[(testtypeId, row.ComponentCode)] = entity.ComponentId;
                    compInserted++;
                }
                else
                {
                    existing.ComponentNameEn = row.ComponentNameEn;
                    existing.ComponentNameAr = null;
                    existing.Unit = row.ComponentUnit;
                    existing.ResultType = row.ComponentResultType;
                    existing.DecimalPlaces = (byte)row.ComponentDecimalPlaces;
                    existing.SortOrder = (short)row.ComponentSortOrder;
                    existing.IsActive = row.ComponentIsActive;
                    await _context.SaveChangesAsync(cancellationToken);
                    componentMap[(testtypeId, row.ComponentCode)] = existing.ComponentId;
                    compUpdated++;
                }
            }

            // ── Step 9: Insert-if-missing NormalRange (461 rows) ──
            var existingRangeKeys = await _context.NormalRanges
                .Select(r => new { r.ComponentId, r.Sex, r.AgeFromDays, r.AgeToDays, r.FastingState })
                .ToListAsync(cancellationToken);

            var existingRangeSet = new HashSet<(int ComponentId, string Sex, int AgeFromDays, int AgeToDays, string FastingState)>();
            foreach (var r in existingRangeKeys)
            {
                existingRangeSet.Add((r.ComponentId, r.Sex, r.AgeFromDays, r.AgeToDays, r.FastingState));
            }

            var rangeInserted = 0;
            var rangeSkipped = 0;

            foreach (var row in rows)
            {
                var componentId = componentMap[(typeMap[row.TypeCode], row.ComponentCode)];

                var key = (
                    ComponentId: componentId,
                    Sex: row.RangeSex,
                    AgeFromDays: row.RangeAgeFromDays ?? 0,
                    AgeToDays: row.RangeAgeToDays ?? 36500,
                    FastingState: row.RangeFastingState
                );

                if (existingRangeSet.Contains(key))
                {
                    rangeSkipped++;
                    continue;
                }

                var entity = new NormalRange
                {
                    ComponentId = componentId,
                    Sex = row.RangeSex,
                    AgeFromDays = row.RangeAgeFromDays ?? 0,
                    AgeToDays = row.RangeAgeToDays ?? 36500,
                    AgeFromValue = row.RangeAgeFromValue,
                    AgeToValue = row.RangeAgeToValue,
                    AgeDescription = row.RangeAgeDescription,
                    ForPregnantOnly = row.RangeForPregnantOnly,
                    AgeUnit = row.RangeAgeUnit,
                    LowFlag = row.RangeLowFlag,
                    HighFlag = row.RangeHighFlag,
                    LowComment = row.RangeLowComment,
                    HighComment = row.RangeHighComment,
                    CriticalRangeText = row.RangeCriticalRangeText,
                    CriticalFlag = row.RangeCriticalFlag,
                    CriticalComment = row.RangeCriticalComment,
                    FastingState = row.RangeFastingState,
                    LowNormal = row.RangeLowNormal,
                    HighNormal = row.RangeHighNormal,
                    LowCritical = row.RangeLowCritical,
                    HighCritical = row.RangeHighCritical,
                    NormalRangeText = row.RangeNormalRangeText,
                    RangeNote = row.RangeNote,
                    Unit = string.IsNullOrWhiteSpace(row.RangeUnit) ? row.ComponentUnit : row.RangeUnit,
                    Version = 1,
                    IsActive = true
                };

                _context.NormalRanges.Add(entity);
                existingRangeSet.Add(key);
                rangeInserted++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // ── Step 10: Seed feature toggles ──
            var defaultSetting = await _context.LabSettings.FirstOrDefaultAsync(cancellationToken);
            if (defaultSetting == null)
            {
                _context.LabSettings.Add(new LabSetting
                {
                    SettingKey = "System",
                    SettingValue = "System Settings",
                    EnforceStageGating = true,
                    EnableServerPrinting = false,
                    LastUpdatedAt = DateTime.UtcNow
                });
                _logger.LogInformation("Seeded default LabSetting with feature toggles.");
            }
            else
            {
                // Ensure default values if not explicitly handled elsewhere
                // Not overriding existing values to respect user changes if any, but since we are seeding,
                // we'll leave it as is if it exists, or maybe we don't change existing.
            }

            await _context.SaveChangesAsync(cancellationToken);

            // ── Step 11: Commit the transaction ──
            await transaction.CommitAsync(cancellationToken);

            // ── Step 12: Log summary ──
            _logger.LogInformation(
                "Test catalog seeding complete: Categories ({CatIns} inserted, {CatUpd} updated), " +
                "Groups ({GrpIns} inserted, {GrpUpd} updated), " +
                "TestTypes ({TypIns} inserted, {TypUpd} updated), " +
                "Components ({CompIns} inserted, {CompUpd} updated), " +
                "NormalRanges ({RngIns} inserted, {RngSkip} skipped).",
                catInserted, catUpdated,
                grpInserted, grpUpdated,
                typeInserted, typeUpdated,
                compInserted, compUpdated,
                rangeInserted, rangeSkipped);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Test catalog seeding failed. Transaction rolled back.");
            throw;
        }
    }
}

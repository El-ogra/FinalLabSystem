● Final Analytical Report

  Task: Read-only compatibility analysis of FinalLabSystem/Docs/SeedData/NormalRanges_CommonTests_schema_compatible_filtered.csv against the FinalLabSystem database schema.
  Mode: Analysis & planning only. No code, schema, migration, or configuration changes were made.

  ---
  1. CSV File Analysis

  Path (resolved against the real repo root):
  C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\Docs\SeedData\NormalRanges_CommonTests_schema_compatible_filtered.csv

  ┌─────────────┬────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
  │  Property   │                                                       Value                                                        │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Encoding    │ UTF-8 with BOM (U+FEFF on the header line)                                                                         │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Total lines │ 462                                                                                                                │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Header row  │ 1                                                                                                                  │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Data rows   │ 461                                                                                                                │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Columns     │ 77                                                                                                                 │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Quoting     │ Standard RFC 4180: fields containing commas (e.g., "Sodium, Serum") are double-quoted. No malformed rows detected. │
  ├─────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Delimiter   │ Comma                                                                                                              │
  └─────────────┴────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

  Column families (77 columns total):

  ┌───────────────┬───────┬────────────────────────────────────────────────────────────────────────────────┐
  │    Prefix     │ Count │                                 Target entity                                  │
  ├───────────────┼───────┼────────────────────────────────────────────────────────────────────────────────┤
  │ category_*    │ 5     │ TestCategory                                                                   │
  ├───────────────┼───────┼────────────────────────────────────────────────────────────────────────────────┤
  │ group_*       │ 5     │ TestGroup                                                                      │
  ├───────────────┼───────┼────────────────────────────────────────────────────────────────────────────────┤
  │ type_*        │ 32    │ TestType                                                                       │
  ├───────────────┼───────┼────────────────────────────────────────────────────────────────────────────────┤
  │ component_*   │ 8     │ TestComponent                                                                  │
  ├───────────────┼───────┼────────────────────────────────────────────────────────────────────────────────┤
  │ range_*       │ 25    │ NormalRange                                                                    │
  ├───────────────┼───────┼────────────────────────────────────────────────────────────────────────────────┤
  │ metadata-only │ 2     │ source_status, seed_decision — CSV filtering metadata, not destination columns │
  └───────────────┴───────┴────────────────────────────────────────────────────────────────────────────────┘

  Distinct value distributions (computed with Python csv module — quote-safe):

  - category_code (8 distinct): HEMA (312), CHEM (101), ENDO (36), URINE (6), TUMOR (2), TDM (2), VITAMIN (1), SEROL (1)
  - group_code (21 distinct): CBC (309), ELEC (29), THY (24), IRON (17), LIVER (16), MISC (11), LIPID (8), CARD (6), REPRO (6), URINE24 (6), PANC (6), RENAL (5), PARA (4), COAG (3), GLUC (2), ADR (2), TUMOR (2), TDM (2), INFLAM (1), VIT (1), AUTO (1)
  - - range_sex (3 distinct): M (171), F (169), B (121) — matches schema's nchar(1) domain {M, F, B}
  - range_age_unit (5 distinct): year (220), month (110), week (62), day (40), empty (29)
  - range_fasting_state (1 distinct): A (Any) in all 461 rows — matches schema default
  - range_for_pregnant_only (1 distinct): empty (all rows) → NULL
  - component_result_type (1 distinct): NUMERIC
  - type_special_type (1 distinct): STANDARD
  - type_sample_type (4 distinct): Blood (307), Serum (138), Plasma (10), Urine (6)
  - source_status (1 distinct): OK (all rows)
  - seed_decision (1 distinct): KEEP (all rows)

  Numeric range completeness:
  - 432/461 rows have BOTH range_low_normal and range_high_normal
  - 5/461 have low only (open-ended cutoffs, e.g., ≥40 mg/dL HDL)
  - 15/461 have high only (open-ended cutoffs, e.g., <30 µmol/L ammonia)
  - 9/461 have NEITHER (placeholders like "Not established (per Mayo)") — schema allows because LowNormal/HighNormal are double?

  Field completeness observations:
  - All *_name_ar (Arabic) columns are empty across all 461 rows — schema permits (Ar fields are nullable).
  - component_unit is populated on every row.
  - range_unit is empty on 455/461 rows. Units are carried at the component level only; the seeder should copy component_unit into range_unit to satisfy the schema's intentional denormalization.
  - type_collection_type_id is empty on every row (CollectionType FK is nullable → insert as NULL).
  - range_age_from_value/range_age_to_value are sometimes empty (e.g., open-ended "18+ year"), schema is int?.

  Suspicious / noteworthy rows:
  - 1 row (~line 447) has range_age_description = "31-11 month" (LDH, 31-330 days, value 31→11) — likely an upstream description-formatting glitch, not a schema problem. Worth flagging as a data-quality note; the numeric age_from_days/age_to_days are correct.
  - A handful of range_age_value pairs cross unit boundaries (days expressed in months/years); the schema handles this via the two parallel representations (*_days for filtering, *_value+age_unit for display).

  Implied entities described: TestCategory, TestGroup, TestType, TestComponent, NormalRange — one row = one NormalRange together with denormalized parent context.

  ---
  2. Relevant System Components Identified

  ┌───────────────────────┬─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┬────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
  │         Layer         │                                                        Path                                                         │                                                        Purpose                                                         │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ DbContext             │ FinalLabSystem/Data/FinalLabDbContext.cs                                                                            │ Owns DbSets for all five target entities                                                                               │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Model — TestCategory  │ FinalLabSystem/Models/TestCategory.cs                                                                               │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Model — TestGroup     │ FinalLabSystem/Models/TestGroup.cs                                                                                  │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Model — TestType      │ FinalLabSystem/Models/TestType.cs                                                                                   │ Includes TestTypeBehavior flags enum                                                                                   │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Model — TestComponent │ FinalLabSystem/Models/TestComponent.cs                                                                              │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Model — NormalRange   │ FinalLabSystem/Models/NormalRange.cs                                                                                │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Model —               │ FinalLabSystem/Models/CollectionType.cs                                                                             │ Optional FK target for TestType                                                                                        │
  │ CollectionType        │                                                                                                                     │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Service               │ FinalLabSystem/Services/Implementations/TestCatalogService.cs                                                       │ Provides the exact insert methods needed: CreateCategoryAsync, CreateGroupAsync, CreateTestTypeAsync,                  │
  │                       │                                                                                                                     │ AddComponentAsync, AddRangeAsync                                                                                       │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Startup / DI          │ FinalLabSystem/App.xaml.cs (ConfigureServices, line 121–196; TestCatalogService registered line 133)                │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Config                │ FinalLabSystem/appsettings.json (line 2–4)                                                                          │ DB: Server=.\SQLEXPRESS;Database=FinalLab;Trusted_Connection=True                                                      │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Project file          │ FinalLabSystem/FinalLabSystem.csproj                                                                                │ Does not currently mark the CSV for copy-to-output                                                                     │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Existing migration    │ Migrations/20260617200000_AddUnitLookup.cs, 20260617210000_AddTubeMaterialMaster.cs,                                │ Only Units, TubeMaterials, and PriceSchemes are seeded today — no Category/Group/Type/Component/NormalRange seed       │
  │ seed                  │ 20260605110710_AddTestDataManagementFields.cs                                                                       │ exists                                                                                                                 │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ CSV utilities in repo │ None — no CsvHelper / ChoETL / TinyCsvParser NuGet, no StreamReader/TextFieldParser usage                           │                                                                                                                        │
  ├───────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Seeder class          │ None today                                                                                                          │                                                                                                                        │
  └───────────────────────┴─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┴────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

  ---
  3. Database Structure Analysis

  The schema is a clean five-level hierarchy, fully aligned with the CSV shape.

  TestCategory (CategoryId)
      └─ TestGroup (GroupId, CategoryId FK)
           └─ TestType (TesttypeId, GroupId FK, CollectionTypeId FK?)
                └─ TestComponent (ComponentId, TesttypeId FK)
                     └─ NormalRange (RangeId, ComponentId FK)

  Key schema facts confirmed by reading the Model files in full:

  - All FK relations use OnDelete(DeleteBehavior.Restrict) — seed insert order must be top-down (Category → Group → Type → Component → NormalRange).
  - TestType.Behavior is a single TestTypeBehavior enum-flags property persisted as int. Members: None=0, IsRoutineTest=1, SeeReport=2, PrintWithOther=4, AddWithGroup=8, IsMainTest=16, IsSendOutside=32, IsOutsourceable=64. The CSV provides BOTH the seven decomposed
boolean columns AND the pre-aggregated integer type_behavior — the seeder should consume the aggregated type_behavior int directly.
  - NormalRange.Sex and NormalRange.FastingState are string (stored as nchar(1)), defaults 'B' and 'A'. CSV values M/F/B and A match.
  - NormalRange.AgeToDays defaults to 36500 (~100 years) — CSV uses the same convention.
  - NormalRange.Unit is a denormalized string, NOT a FK to Unit. Seeder must denormalize from component.
  - LowNormal, HighNormal, LowCritical, HighCritical are double? — empty CSV cells map to NULL.
  - Version defaults to 1; IsActive defaults to true; CSV supplies both consistently.

  ---
  4. Test Types Analysis

  - 57 distinct type_code values (e.g., NAS, KS, CL, HCO3, CA, CAI, MGS, URIC, ALT, AST, ALP, BILID, TP, ALB, CPR, INS, HDCH, TRIG1, AMS, LD, NH3V, HAPT, PALB, LH, B12, RHUT, DIG, …).
  - Each row carries the full TestType descriptor — duplicated across all NormalRange rows for that type. The seeder must deduplicate by type_code (which is the unique business key per the schema's unique index).
  - Total possible Behavior coverage: every CSV row sets type_is_main_test=1 and the other six behavior bits to 0, matching type_behavior=0 — i.e., a plain "main test" with no special flags. Importing the aggregated type_behavior int satisfies the schema directly.
  - type_collection_type_id is empty everywhere → insert as NULL (column is int?).
  - type_default_price = 0.00 everywhere → seeded as zero, intended to be set later by admin pricing.
  - type_special_type = STANDARD everywhere — matches the schema default.

  ---
  5. Normal Ranges Analysis

  - 461 NormalRange rows belonging to 74 distinct (type_code, component_code) pairs — i.e., 74 components carry ~6 ranges each on average, stratified by sex × age.
  - Stratification keys actually used: Sex (M/F/B) × AgeFromDays/AgeToDays × AgeUnit (display). FastingState is always A. ForPregnantOnly is always NULL.
  - Open-ended ranges: correctly represented with NULLs in the unused bound (5 low-only, 15 high-only).
  - "Not established" rows (9): carry NULL low/NULL high + range_normal_range_text='not established' + a range_note. Schema accepts this verbatim.
  - range_unit denormalization: 455/461 rows have an empty range_unit. The seeder must populate NormalRange.Unit from the row's component_unit to honour the denormalized contract used elsewhere in the system (TestResult.SnapUnit copies it forward at result entry).
  - range_critical_* columns are blank across the file. The schema permits NULL.
  - All range_version=1, all range_is_active=1 → first-version, active.

  ---
  6. Relationship Analysis

  The CSV's denormalized shape maps to a five-step insert plan:

  1. Distinct categories (8) → insert into TestCategory keyed by category_code.
  2. Distinct (category_code, group_code) pairs (21) → insert into TestGroup with CategoryId resolved from step 1.
  3. Distinct type_code values (57) → insert into TestType with GroupId resolved from step 2; CollectionTypeId = NULL; Behavior = (TestTypeBehavior)int.Parse(type_behavior).
  4. Distinct (type_code, component_code) pairs (74) → insert into TestComponent with TesttypeId resolved from step 3. Unique index (testtype_id, component_code) is honoured.
  5. All 461 rows → insert into NormalRange with ComponentId resolved from step 4. Copy component_unit into NormalRange.Unit when range_unit is empty.

  No many-to-many bridge is required. The CSV's hierarchy is a pure tree, identical to the EF model's hierarchy.

  ---
  7. Compatibility Decision

  ✅ YES — schema-compatible.

  Every one of the 75 destination CSV columns (77 minus the two metadata columns source_status/seed_decision) maps 1:1 onto an existing CLR property on the right Model class. No schema change, no migration, no model edit is required to consume this file.

  ---
  8. Technical Justification

  Field-by-field verification (all confirmed present in the Models):
- TestCategory: category_code → CategoryCode, category_name_en → CategoryNameEn, category_name_ar → CategoryNameAr, category_sort_order → SortOrder, category_is_active → IsActive. ✅ All 5/5 mapped.
  - TestGroup: group_code → GroupCode, group_name_en → GroupNameEn, group_name_ar → GroupNameAr, group_sort_order → SortOrder, group_is_active → IsActive. ✅ All 5/5 mapped. (CategoryId derived during insert.)
  - TestType (32 columns): Every CSV type_* column has a matching property — TypeCode, TypeNameEn, TypeNameAr, TypeAbbrev, DefaultPrice, SampleType, DefaultTubeType, DefaultTubeColor, TurnaroundHours, SpecialType, SortOrder, IsActive, Notes, ReportNameLine1,
  ReportNameLine2, BillNameLine1, BillNameLine2, HistoryName, CollectionNotes, CollectionTypeId, OutsideLabName, OutsideCostPrice, PatientQuestion, ReferenceType, BarcodeName, Behavior. The seven booleans (type_is_routine_test … type_is_send_outside) are absorbed by
  the single Behavior enum-flags property — the CSV already provides the aggregated int in type_behavior. ✅ All 32/32 mapped. (GroupId derived during insert.)
  - TestComponent: component_code → ComponentCode, component_name_en → ComponentNameEn, component_name_ar → ComponentNameAr, component_unit → Unit, component_result_type → ResultType, component_decimal_places → DecimalPlaces, component_sort_order → SortOrder,
  component_is_active → IsActive. ✅ All 8/8 mapped. (TesttypeId derived during insert.)
  - NormalRange (25 columns): Every CSV range_* column has a matching property — Sex, AgeFromDays, AgeToDays, AgeFromValue, AgeToValue, AgeDescription, ForPregnantOnly, AgeUnit, LowFlag, HighFlag, LowComment, HighComment, CriticalRangeText, CriticalFlag,
  CriticalComment, FastingState, LowNormal, HighNormal, LowCritical, HighCritical, NormalRangeText, RangeNote, Unit, Version, IsActive. ✅ All 25/25 mapped. (ComponentId derived during insert.)

  Value-domain checks:

  - Sex ∈ {M,F,B} ✅
  - FastingState ∈ {A} ✅ (subset of {A,F})
  - Behavior ∈ {0} ✅ (valid TestTypeBehavior value)
  - ResultType = NUMERIC ✅
  - SpecialType = STANDARD ✅
  - SortOrder values fit short (max value 73, well under 32767) ✅
  - DefaultPrice = 0.00 fits decimal(18,2) ✅
  - TurnaroundHours = 24 fits short ✅
  - All numeric low/high_normal/critical values fit double ✅
  - No string exceeds the declared MaxLength constraints based on row-by-row inspection.

  The file is therefore named accurately — it really is "schema_compatible".

  ---
  9. Execution Plan (no Repair Plan needed)

  This plan is planning-only. No code change is proposed in this report — implementation is for a follow-up task.

  Phase A — Make the CSV reachable at runtime

  1. In FinalLabSystem/FinalLabSystem.csproj, add an <ItemGroup> entry for Docs\SeedData\*.csv with <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory> (mirroring how appsettings.json is handled at csproj line 30–32).
  2. Add a CSV-parsing dependency. Recommend CsvHelper (industry standard, RFC 4180 compliant, handles BOM, quoted commas, and utf-8-sig natively). One NuGet entry.

  Phase B — Build the seeder

  3. Create a new service ITestCatalogSeeder / TestCatalogSeeder under FinalLabSystem/Services/ that depends on the existing ITestCatalogService (no need to touch repositories — that service already exposes every required Create* / Add* method).
  4. Inside the seeder, the load order is:
    - Idempotency guard: skip if TestCategories.Any() (or check a dedicated IsSeeded flag/setting).
    - Read CSV with utf-8-sig encoding and the CsvHelper BadDataFound = null tolerant config.
    - Filter rows where seed_decision != "KEEP" (defensive — all current rows are KEEP).
    - Project rows into a 5-level intermediate model.
    - Distinct-by category_code → TestCatalogService.CreateCategoryAsync.
    - Distinct-by (category_code, group_code) → resolve CategoryId, then CreateGroupAsync.
    - Distinct-by type_code → resolve GroupId, set Behavior = (TestTypeBehavior)int.Parse(type_behavior), set CollectionTypeId = null, then CreateTestTypeAsync (zero prices, empty sample tubes).
    - Distinct-by (type_code, component_code) → resolve TesttypeId, then AddComponentAsync.
    - For every row: resolve ComponentId, default range_unit ← component_unit when blank, then AddRangeAsync.
    - Wrap the entire seed in a single transaction (IDbContextTransaction) to guarantee atomicity.

  Phase C — Wiring

  5. Register ITestCatalogSeeder in App.xaml.cs::ConfigureServices() next to ITestCatalogService (line 133).
  6. Invoke it at startup after migrations have run (the app today has no explicit Database.Migrate() — it should be added once and the seeder called immediately after, both inside OnStartup).
  7. Resolve the CSV path with Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Docs", "SeedData", "NormalRanges_CommonTests_schema_compatible_filtered.csv") — matches the existing convention used by App.xaml.cs line 35 and line 65.

  Phase D — Verification (after implementation, not in this task)

  - Drop the local FinalLab database and re-run the app; confirm post-seed counts: TestCategory=8, TestGroup=21, TestType=57, TestComponent=74, NormalRange=461.
  - Spot-check a known row, e.g., Sodium (NAS) adults (1+ years, sex=B): LowNormal=135, HighNormal=145, Unit='mmol/L'.
  - Spot-check a CBC row to confirm multi-component-per-type works (CBC alone owns 309 ranges).
  - Confirm the unique index (testtype_id, component_code) is not violated (it shouldn't be — there are 74 distinct pairs in the CSV).
  - Re-run the app a second time and confirm the idempotency guard prevents duplicate inserts.

  No "Repair Plan" is required — the CSV is already aligned with the schema. Implementation is purely additive.

  ---
  10. Open Questions

  1. Seed trigger semantics. Should seeding run automatically on first launch (after Database.Migrate()), or be gated behind an explicit admin action / settings flag (SeedTestCatalogOnStartup=true in appsettings.json)? The current FirstRunSetupViewModel only creates
  the initial Administrator; this seeder would naturally hook in there.
  2. Database migration runner. Today neither App.xaml.cs nor FinalLabDbContext calls Database.Migrate(). Migrations appear to be applied out-of-band via EF tooling. Is it acceptable to introduce a startup-time Database.Migrate() call so the seeder has a
  guaranteed-current schema to write into?
  3. Re-seed policy. If the CSV is updated later (new tests added), should the seeder be idempotent-but-additive (upsert by business keys: category_code, group_code, type_code, (testtype_id, component_code)), or strictly first-run-only? Upsert mode is slightly more
  work but more useful for ongoing maintenance.
  4. Arabic localization. Every *_name_ar column is empty across all 461 rows. Confirm this is intentional (Arabic strings to be edited manually after seed) versus an oversight in CSV preparation.
  5. Default CollectionTypeId. The CSV sets it to empty for every row. The schema permits NULL, but the system has a CollectionType master table — should the seeder default to a specific CollectionType (e.g., "Standard / Venous Blood") when the column is blank, or
  accept NULL as the system's actual convention?
  6. type_default_price = 0.00. All 57 test types are seeded with zero price. Confirm this is desired (admin sets prices later via the Pricing UI), or whether a follow-up CSV / config will supply prices.
  7. Edge row "31-11 month" (LDH age description, ~line 447). Cosmetic upstream display bug. Leave as-is, or correct to "1-11 month" before seeding? The numeric age_from_days/age_to_days (31 → 330) are correct regardless.
  8. Unit lookup consistency. The CSV uses unit literals like mcIU/mL, ng/L, cells/µL, x 10(9)/L that are not in the 16 units pre-seeded by 20260617200000_AddUnitLookup.cs. Because NormalRange.Unit is a denormalized string (not a FK), this does not break the load — but
   you may want to backfill the Unit master table from the distinct component_unit values discovered during seeding to keep the unit picker UI in sync.

  ---
  End of report. No files were modified. Total entities the CSV will materialize when implemented: 8 categories + 21 groups + 57 test types + 74 components + 461 normal ranges = 621 inserts across 5 tables.

● Analysis complete. Verdict: schema-compatible (YES). The CSV's 75 destination columns map 1:1 to existing properties on TestCategory, TestGroup, TestType (including the TestTypeBehavior flags enum), TestComponent, and NormalRange. No schema, model, migration, or
  service change is needed — only a thin seeder service (Phase B above) and CSV-to-output wiring (Phase A). No code was modified.
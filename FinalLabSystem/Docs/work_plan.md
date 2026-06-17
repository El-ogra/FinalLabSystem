# Work Plan — Test Catalog & Reference Ranges

Output of Phase 1 (analysis-only) of the Test Catalog & Reference Ranges initiative. This document is the working plan for future phases. It must be read alongside `HANDOFF.md`.

**Status:** Phase 1 produced this file and `HANDOFF.md`. No source code was modified. No git operation was performed.

---

## Agent Understanding Confirmation (Step 4b)

In my own words, based on the actual files read in Phase 1 and the user's answers to the Step 4a clarifying questions:

1. **Single-component vs. multi-component tests.** A single-component test produces one number with one unit and one applicable reference range. A multi-component (panel) test is really a bundle of sub-tests — for example a CBC produces a hemoglobin value with its own unit and range, a WBC value with a different unit and range, a platelet count, and so on. Treating both shapes the same way at the data layer matters because (a) the result-entry, range-matching, flagging, and printing pipelines all key off the sub-result level, and (b) without a uniform shape, the UI and service code fork into special cases that drift.

2. **Units of measurement and reference ranges per category.** A single-component test has exactly one `TestComponent` row carrying its one unit; that component has its own conditional reference ranges. A multi-component test has one `TestComponent` per sub-result, each with its own unit and its own conditional reference ranges. `NormalRange.Unit` is a denormalized snapshot the result-entry path uses so that historical results retain the unit that was active when the result was generated.

3. **Why reference ranges vary by sex and/or age.** Lab reference intervals are not constants; they depend on patient physiology. Hemoglobin's normal interval is different for adult males vs. adult females; many enzymes have different intervals across childhood vs. adulthood; some analytes (e.g. AFP, glucose tolerance) need pregnancy-specific intervals. Encoding multiple ranges per component, scoped by sex and/or age and/or pregnancy state, lets the matcher pick the right one at result-entry time.

4. **What the codebase already provides.** The data model is in good shape: `TestType → TestComponent → NormalRange` exists, `TestComponent.Unit` exists, `NormalRange` carries sex / age-from-days / age-to-days / age-unit / for-pregnant-only / low-and-high normal / low-and-high critical / flags / comments / version / is-active / superseded-by-id. EF config is wired. The service layer has component and range CRUD methods that work in isolation. The result-entry path (`RoutineResultService`) already consumes ranges by `ComponentId` and produces flagged statuses (NORMAL / HIGH / LOW / HIGH_CRITICAL / LOW_CRITICAL).

5. **What is missing or broken and must eventually be built.** No UI for adding/editing `TestComponent` rows — neither the Test Type window nor the Normal Ranges window exposes the component axis. The "synthetic in-memory component" workaround used today by the Normal Ranges window cannot persist its child ranges, so single-component tests' ranges silently fail to save. The `NormalRange` versioning contract is documented on the entity but not honored by the service. The age value is double-converted on every save when an `AgeUnit` is set. A redundant "Range for" radio group exists alongside the real Sex radio group. The Reference Type combo has no value list. Many `NormalRange` fields (critical limits, fasting state, age description, range note, critical-text/flag/comment) are not bound to any UI. Several smaller wiring bugs round out the gap list (full inventory in Section A5 and Section B). No unit-test coverage exists for `TestCatalogService` or the seven affected ViewModels.

6. **No implementation in this session.** Phase 1 is analysis only. The only files produced are `HANDOFF.md` and this `work_plan.md`, both inside `FinalLabSystem/Docs/`. No source file was edited; no git command was run. Every later phase requires its own approval before implementation begins.

---

## Section A — Current State Summary

### A1 — Test Type Add/Edit window

**Files:** `Views/Settings/TestDataManagementWindow.xaml` (+ `.xaml.cs`), `ViewModels/Settings/TestDataManagementViewModel.cs`, `TestDetailViewModel.cs`, `TestListViewModel.cs`, `TestRowViewModel.cs`.

**Search row** — three search boxes bound to `TestList.SearchTestName / SearchGroupName / SearchTestId`, filter live. Count text reads `TestList.FilteredTests.Count`.

**Catalog DataGrid** — read-only, bound to `TestList.FilteredTests` with selection bound to `TestList.SelectedTest`. Columns: ID (TesttypeId), Arrange (SortOrder), Group Name (Ar), Test Name (En), Pat. P, Lab P, Out Lab Name, Out P, Barcode. Selection raises `SelectedTestChanged` → parent VM calls `TestDetail.LoadAsync(...)`.

**Left column (Test Information)**

| UI | Binding | Status |
|---|---|---|
| Test Name | `TypeNameEn` | OK |
| Test Code | `TypeCode` | OK |
| Report Name | `ReportNameLine1` | OK |
| Bill Name | `BillNameLine1` | OK |
| History Name | `HistoryName` | OK |
| Arabic Name | `TypeNameAr` | OK |
| Group Name (combo) | `Groups` / `GroupId` | OK |
| Collection (combo) | `CollectionTypes` / `SelectedCollectionTypeId` | OK |
| See Report / Print with other / Add with group / Main test | flag-enum getters | OK |

**Right column (Barcode + Outside Lab + Patient Question)**

| UI | Binding | Status |
|---|---|---|
| Barcode > Name | `BarcodeName` | OK |
| Tube 1 / 2 / 3 | `Tube1/2/3` + hardcoded `AvailableTubeTypes` | OK; tube list hardcoded (B8) |
| Sent outside Lab | `IsSendOutside` → `IsOutsideFieldsEnabled` | OK |
| Lab Name / Cost price | `OutsideLabName` / `OutsideCostPrice`, enable-gated | OK |
| Patient Question (check) | `HasPatientQuestion` | **B1 — not restored from entity on load** |
| Patient question text | `PatientQuestion`, enable-gated | Visible but disabled after load (B1 ripple) |

**Bottom row (pricing / timing)**

| UI | Binding | Status |
|---|---|---|
| Test time (Day) | `TurnaroundHours` | **B2 — label vs property unit mismatch** |
| Arrange No | `SortOrder` | OK |
| Reference type (editable combo) | `ReferenceType`; no `ItemsSource` | **B5 — empty dropdown** |
| Patient price | `PatientPrice` | OK |
| Lab to lab price | `LabToLabPrice` | OK |

**Buttons**

| Button | Command | Status |
|---|---|---|
| إضافة تحليل (Add) | `AddCommand` | OK |
| تعديل (Edit) | `EditCommand` | OK (but baseline is partial — see B3) |
| حفظ (Save) | `SaveCommand` | OK |
| تراجع (Cancel) | `CancelCommand` | **B3 — only restores 5 of ~25 fields** |
| حذف (Delete) | `DeleteCommand` | OK; soft-deletes when visits exist |
| القيم المرجعية (Normal ranges) | `TestDetail.OpenNormalRangesCommand` | OK |
| القائمة الرئيسية (Close) | `CloseCommand` | OK |

**Q1 — Multi-component support?** Model layer: yes (`TestType.TestComponents`). UI: no — there is no panel for adding/editing components here. The single-component synthetic injection lives only inside the Normal Ranges window.

**Q2 — Unit of measurement?** Captured per `TestComponent.Unit` and snapshotted on `NormalRange.Unit`. Not on `TestType`. No unit field exists on the Test Type window.

**Q3 — Non-responsive list:** see Section A5.

**Q4 — Conflict with `TestTypeSampleTube` (prior barcode work)?** No schema conflict. Two behavioral concerns: `UpdateTestTypeAsync` does `RemoveRange(existing.TestTypeSampleTubes)` then re-adds three (`TestCatalogService.cs:253-262`) — extra tubes a prior workflow inserted (>3) would be lost; and the VM hardcodes `TubeType="Default"` (`TestDetailViewModel.cs:378,381,384`) so any `TubeType` distinction the prior work used is flattened on every save through this window.

### A2 — Reference Range Add/Edit window

**Files:** `Views/Settings/NormalRangesWindow.xaml` (+ `.xaml.cs`), `ViewModels/Settings/NormalRangeWindowViewModel.cs`, `NormalRangeListViewModel.cs`, `NormalRangeDetailViewModel.cs`.

**Header** — Test name red badge bound to `ParentTest.TypeNameEn`. `ReferenceCount` shows ranges-for-selected-component count.

**DataGrid** — bound to `List.RangesForSelectedComponent`, selection to `List.SelectedRange`. Columns: Gender (Sex), From (AgeFromDays), To (AgeToDays), Age unit, Reference range (NormalRangeText), low/high limit, Test unit, Low/High flag. Selecting a row routes through `Detail.Load(value, SelectedComponent.Unit)`.

**Reference Setting form**

| UI | Binding | Status |
|---|---|---|
| Range for: For all / Female / Male (radios) | `IsRangeForAll/Female/Male` → `RangeFor` (VM-only) | **R3, R8 — redundant + dead control** |
| Sex: Male / Female / Both (radios) | `IsSexMale/Female/Both` → `Sex` (`RangeSex`) | OK on save; **R7 — default mismatch on Add** |
| Age from / to | `AgeFromDays` / `AgeToDays` | UI ok; **R5 — service double-converts on save** |
| Age unit | `AgeUnitOptions` / `AgeUnit` | UI ok; ripple into R5 |
| Reference range (textarea) | `NormalRangeText` | OK |
| Low / High limit | `LowNormal` / `HighNormal` (double?) | OK; Save validates Low ≤ High |
| Low / High flag (editable combos) | `LowFlag` / `HighFlag` | OK; presets L/LL/Low and H/HH/High |
| Test unit (editable combo) | `Unit` | OK; presets sec, mg/dL, mmol/L, g/dL, U/L, IU/L, %, fl, pg |
| For pregnant only | `ForPregnantOnly` | OK |
| Low / High comment (textareas) | `LowComment` / `HighComment` | OK |
| **Not surfaced anywhere** | `FastingState`, `LowCritical`, `HighCritical`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `AgeDescription`, `RangeNote` | **R6 — unbound** |

**Buttons**

| Button | Command | Status |
|---|---|---|
| إضافة مدى (Add range) | `List.AddRangeCommand` | Persistence broken for synthetic component (R9) |
| حفظ (Save) | `Detail.SaveCommand` | OK for existing components; **R2 — never persists new components**; **R10 — writes in place, violates versioning contract** |
| تراجع (Cancel) | `Detail.CancelCommand` | OK (reloads last snapshot) |
| حذف (Delete) | `List.DeleteRangeCommand` | OK |
| قائمة التحاليل (Back) | `BackToTestsCommand` | OK |

**Q1 — Does a `NormalRange` entity exist?** Yes — `Models/NormalRange.cs`. Field inventory listed in `HANDOFF.md` §1.4 and Section A3.

**Q2 — Multiple ranges scoped by sex/age?** Data layer: yes (`NormalRange` 1:N from `TestComponent`; matcher at `RoutineResultService.cs:81-117`). UI layer: only ranges of the auto-selected first component are reachable (R1).

**Q3 — For multi-component tests, where does the range attach?** Always to `TestComponent.ComponentId`. No direct `TestType → NormalRange` shortcut.

**Q4 — How are ranges displayed/edited?** Single full-width DataGrid for the selected component, inline form below. No per-range dialog. No component picker.

### A3 — Model layer findings

Entity chain:

```
TestGroup (1..N TestType)
  TestType (TesttypeId, TypeCode UQ)
    TestTypePrice (Patient Price + Lab-to-Lab Price rows)
    TestTypeSampleTube (cascade delete)
    TestComponent (ComponentId; UQ on (TesttypeId, ComponentCode); Unit nvarchar(30))
      NormalRange (RangeId; Sex char(1), AgeFromDays, AgeToDays default 36500,
                   AgeFromValue, AgeToValue, AgeDescription nvarchar(50),
                   ForPregnantOnly, AgeUnit nvarchar(10),
                   LowFlag/HighFlag nvarchar(20), LowComment/HighComment nvarchar(500),
                   CriticalRangeText nvarchar(200), CriticalFlag nvarchar(20), CriticalComment nvarchar(500),
                   FastingState char(1) default 'A',
                   LowNormal, HighNormal, LowCritical, HighCritical (double?),
                   NormalRangeText nvarchar(200), RangeNote nvarchar(200), Unit nvarchar(20),
                   Version int default 1, IsActive bool default true, SupersededById int?)
```

EF config references: `FinalLabDbContext.cs:487-569` (NormalRange), `:1094-1132` (TestComponent), `:1251-1362` (TestType), `:1365-1388` (TestTypePrice), `:1391-1427` (TestTypeSampleTube).

**Q1 — Exact current chain for multi-component:** `TestType → TestComponent → NormalRange`. `TestResult.ComponentId` joins results to components and `TestResult.NormalRangeId` snapshots the matched range.

**Q2 — Unit-of-measure entity?** No. Free text on `TestComponent.Unit` and `NormalRange.Unit`. The UI offers a hardcoded suggestion list of 9 units (XAML 218-227) and a hardcoded tube list of 7 entries (`TestDetailViewModel.cs:71`).

### A4 — Service layer findings

`ITestCatalogService` / `TestCatalogService` covers both halves. Methods, behaviors, and known issues are summarized in `HANDOFF.md` §2.4 — repeated here in brief:

- **Test type CRUD** — Create (transactional with prices+tubes), Update (wholesale tube replace, price upsert), Delete (soft-delete-if-visits-exist), GetAll, GetDetails, GetPaged.
- **Component CRUD** — Add, Update, Delete (manual child cleanup of ranges).
- **Range CRUD** — Add, Update, Delete, Save (upsert in place), GetForComponent, GetForTestType.
- **Lookups** — categories / groups / collection types CRUD.

**Issues to fix in later phases:**
- Versioning contract violated by `SaveRangeAsync` / `UpdateRangeAsync` / `AddRangeAsync` (R10).
- Age double-conversion on every save (R5).
- `CreateTestTypeAsync` does not auto-create the first `TestComponent` row (Q4a-3 decision).
- No transaction wraps "save a component plus its initial range" — currently unreachable but must be addressed when the component editor lands.

**Q1 — Test-type CRUD:** above.
**Q2 — Range CRUD:** above.
**Q3 — Multi-component support at service layer?** Methods exist and work in isolation. UI is what's missing.

### A5 — Non-Responsive Fields & Buttons (consolidated)

Same table as `HANDOFF.md` §3. Reproduced here for self-contained reading.

| # | UI element | File: line | Root cause |
|---|---|---|---|
| B1 | Patient Question checkbox + textarea on load | `TestDetailViewModel.cs:253-265, 286-307` | `HasPatientQuestion` is VM-only, never set from the loaded entity. |
| B2 | "Test time (Day)" vs `TurnaroundHours` | XAML:315-317; `TestDetailViewModel.cs:161-165`; `TestType.cs:48` | Unit mismatch. Decision (Q4a-6): fix the label, keep the property. |
| B3 | Cancel partial revert | `TestDetailViewModel.cs:395-412` | Baseline snapshots only Tube1/2/3 + ReferenceType + BarcodeName. |
| B4 | ReportNameLine2 / BillNameLine2 | XAML:147-167; `TestDetailViewModel.cs:131-147` | VM/entity present, XAML never binds. |
| B5 | Reference type combo | XAML:321-324; `TestDetailViewModel.cs:63` | No `ItemsSource`. Decision (Q4a-5): bind to the classification list. |
| B6 | No multi-component editor | `TestDataManagementWindow.xaml`, `TestDetailViewModel.cs` | Window omits component panel. |
| B7 | No unit-of-measure field on Test Type window | `TestDataManagementWindow.xaml` | Single-component unit unreachable from this window. |
| B8 | Hardcoded tube types | `TestDetailViewModel.cs:71` | Static VM array. |
| B9 | "By test ID" search semantics | XAML:77-80; `TestListViewModel.cs:96-97` | Box labeled "test ID" but filters TypeCode. |
| R1 | Component selector UI | `NormalRangesWindow.xaml`; `NormalRangeListViewModel.cs:31, 67-69, 107-132` | VM exposes components + commands; window has no UI for them. |
| R2 | Save never persists components | `NormalRangeWindowViewModel.cs:69`; `NormalRangeDetailViewModel.cs:285-311`; `NormalRangeListViewModel.cs:84-105` | Save routes to range-only; `SaveAllAsync` never invoked. |
| R3 | Two redundant Sex radio groups | XAML:117-130; `NormalRangeDetailViewModel.cs:48-95` | Decision (Q4a-1): remove the upper "Range for" group. |
| R4 | `IsSexEnabled` / `IsAgeEnabled` orphan | `NormalRangeDetailViewModel.cs:65-67`; XAML | No XAML binding. |
| R5 | Age double-conversion | `TestCatalogService.cs:341-345, 359-363, 539-547, 554-558` | Multiplies on every save when AgeUnit is set. |
| R6 | Unbound critical / fasting / age-description / range-note fields | `NormalRangeDetailViewModel.cs:125-219`; XAML | Decision (Q4a-4): in scope; surface in later phase. |
| R7 | New-range Sex default mismatch | `NormalRangeListViewModel.cs:139-149`; `NormalRangeDetailViewModel.cs:22-23, 87-95, 299-305` | Entity "Both" vs VM `RangeSex.M` (enum 0). |
| R8 | "Range for" radio group is dead | `NormalRangeDetailViewModel.cs:48-85, 299-305`; XAML:117-122 | Resolved by R3 decision. |
| R9 | Synthetic component cannot persist | `NormalRangeWindowViewModel.cs:79-90`; `NormalRangeListViewModel.cs:139-149`; `TestCatalogService.SaveRangeAsync` | Decision (Q4a-3): remove synthetic path; auto-create persisted component at test-type creation. |
| R10 | Versioning contract violated | `NormalRange.cs:6-12`; `TestCatalogService.cs:352-388, 550-599` | Decision (Q4a-2): implement insert-new-and-supersede. |

### A6 — Existing test coverage

Project: `FinalLabSystem.Tests` (net8.0-windows; xUnit 2.5.3; EFCore.InMemory 8.0.0; Moq 4.20.70).

Existing tests touch: `AuthService` (5), `PatientService` (3), entity `[Range]` annotation on `TestType.DefaultPrice` (1 of 3 in `EntityValidationTests`).

No coverage for: `TestCatalogService`, `RoutineResultService`, `TestDataManagementViewModel`, `TestDetailViewModel`, `TestListViewModel`, `TestRowViewModel`, `NormalRangeWindowViewModel`, `NormalRangeListViewModel`, `NormalRangeDetailViewModel`.

Test infrastructure is ready — adding the missing tests is mechanically straightforward.

---

## Section B — Gap Analysis

Same table as `HANDOFF.md` §4, reproduced for self-contained reading.

| Gap ID | Description | Affected Layer | Complexity |
|---|---|---|---|
| G-1 | Multi-component editing has no UI. | Views, ViewModels | High |
| G-2 | Per-component unit has no entry point on the Test Type window; on the Normal Ranges window it's entered per-range instead of per-component. **Decision (D-1): unit picker must source from a user-managed Units lookup table — adds a new lookup foundation.** | Model, Service, Views, ViewModels, Migration | Medium |
| G-3 | Sex/age conditional UX broken (R3/R5/R7/R8/R1 cluster). | View, ViewModel, Service | High |
| G-4 | `NormalRange` versioning contract ignored — implement per Q4a-2. | Service | Medium |
| G-5 | Synthetic component cannot persist — replace per Q4a-3 with auto-created row in `CreateTestTypeAsync`. | Service, ViewModel | Medium |
| G-6 | "Reference type" combo has no `ItemsSource` — bind classification list per Q4a-5. | View (+ small Model) | Low |
| G-7 | Patient Question not restored on load (B1). | ViewModel | Low |
| G-8 | Cancel reverts only 5 fields (B3). | ViewModel | Low |
| G-9 | ReportNameLine2 / BillNameLine2 unreachable (B4). **Decision (D-6): remove from `Models/TestType.cs`, `TestDetailViewModel.cs`, EF config; ship a migration to drop the DB columns. Audit reporting/printing surfaces for any reads first.** | Model, View, ViewModel, EF config, Migration | Low–Medium |
| G-10 | "Test time (Day)" label vs hours property — fix label per Q4a-6. | View | Low |
| G-11 | "By test ID" search hits TypeCode (B9). | View / ViewModel | Low |
| G-12 | Hardcoded tube list (B8). **Decision (D-8): source from a user-managed tube master table.** **Caveat:** existing `Models/SampleTube.cs` is per-visit, not a master — implementing phase must introduce a new master entity (recommended) or split `SampleTube`. | Model, Service, ViewModel, Migration | Medium |
| G-13 | Critical / fasting / age-description / range-note / critical-text/flag/comment unbound (R6) — in scope per Q4a-4. | View | Medium |
| G-14 | Upper Sex radio group + orphan enable flags — delete per Q4a-1. | View, ViewModel | Low |
| G-15 | No unit test coverage for `TestCatalogService`, `RoutineResultService`, six TestType/NormalRange ViewModels. | Tests | Medium |
| G-16 | Possible coupling between `UpdateTestTypeAsync`'s wholesale tube replace and prior barcode work (`TubeType="Default"`). | Service / cross-feature | Low–Medium (verify first) |

---

## Section C — Proposed Roadmap (sketch — NOT for execution this session)

Future phases are described at a high level only. Each phase that creates or modifies a Service file or a ViewModel file specifies its **paired unit-testing scope** (regression tests required before the change + new tests required after).

### Phase 2 — Lockdown regression net (testing only, no behavior change)

**Goal.** Lock current correct behavior of `TestCatalogService` and the six TestType/NormalRange ViewModels before any other phase touches them.

**Rough scope.** Add xUnit tests in `FinalLabSystem.Tests`:
- `TestCatalogService`: Create/Update/Delete test-type happy paths; soft-delete-when-visits; price-row upsert; tube replace; component CRUD; range CRUD; `GetTestTypeDetailsAsync` eager-load shape; `RoutineResultService` sex/age/pregnant matching.
- `TestDetailViewModel`: `Load` → field mapping; `Validate` rules; `BuildEntity`; `BuildTubes`. Document the current B3 (Cancel) behavior as a `Skip`/known-failing test.
- `TestListViewModel`: `ApplyFilter` combinations; `SelectedTestChanged` event.
- `TestRowViewModel`: derived properties (`DisplayName`, `GroupNameAr`, `PatientPrice`, `LabToLabPrice`, `TubeCount`).
- `NormalRangeDetailViewModel`: `Load` mapping; Sex enum round-trip; Save validation (Low ≤ High; Low ≤ High Critical).
- `NormalRangeListViewModel`: `LoadComponents`; `AddRange`; `DeleteRangeAsync` confirm-cancel path.
- `NormalRangeWindowViewModel`: `InitializeAsync` synthetic-component injection (this lock will be inverted in Phase 4/5).

**Complexity.** Medium.
**Dependencies.** None — must precede every behavior-change phase.
**Paired testing scope.** This phase is itself the regression net.

### Phase 3 — Quick wins (small low-risk fixes)

**Goal.** Fix the cheap correctness/UX bugs.

**Rough scope.** Edits to:
- `TestDetailViewModel.cs`: restore `HasPatientQuestion` from `EditableTest.PatientQuestion` in `LoadAsync` (B1); extend `SaveBaseline`/`CancelChanges` to cover the entity-backed fields (B3).
- `TestDataManagementWindow.xaml`: rename "Test time (Day)" → "Test time (Hours)" (B2); add bindings for `ReportNameLine2` / `BillNameLine2` if D-6 says surface them; relabel "By test ID" to "By code" OR change filter to numeric ID parse (B9 — pick once D-3-style preference is given).
- `TestListViewModel.cs`: same B9 fix path.
- `NormalRangeDetailViewModel.cs` + `NormalRangesWindow.xaml`: remove `RangeFor` enum + `IsRangeFor*` + the upper radio group XAML, remove `IsSexEnabled`/`IsAgeEnabled` orphan properties (R3/R4/R8 per Q4a-1).

**Complexity.** Low.
**Dependencies.** Phase 2 in place.
**Paired testing scope (Service + ViewModel).**
- *Before:* Phase 2 already locks `TestDetailViewModel.LoadAsync/BuildEntity`, `TestListViewModel.ApplyFilter`, `NormalRangeDetailViewModel.Load/Save`.
- *After:* New tests asserting: `HasPatientQuestion` populates from a loaded entity that has a non-null `PatientQuestion`; Cancel reverts every entity-backed field; "By code" search filters TypeCode correctly (or numeric ID filter parses correctly); Sex axis now lives in a single radio group with no NPE if `RangeFor` types are removed.

### Phase 4 — Fix the Reference Range data path (R5, R7, R9, R10)

**Goal.** Implement the versioning contract per Q4a-2, fix age double-conversion per R5, fix Sex default mismatch per R7, and remove the synthetic-component path per Q4a-3 (which also resolves R9 once Phase 5 lands).

**Rough scope.**
- `TestCatalogService`:
  - `SaveRangeAsync` becomes insert-new-and-supersede when `RangeId > 0`: copy the existing row's `Version`, deactivate it (`IsActive=false`), insert the new row (`Version=old+1`, `IsActive=true`, `SupersededById` set on the OLD row pointing to the new), wrap in a transaction.
  - `UpdateRangeAsync` either becomes a thin alias for `SaveRangeAsync` or is deleted.
  - `AddRangeAsync` becomes the explicit "create first version" path.
  - Conversion logic: stop converting at the service. Either (a) make the VM convert into days exactly once before calling the service, and store only days plus `AgeUnit` (display unit) — OR (b) introduce `AgeFromValue`/`AgeToValue` as the authoritative human-entered numbers and `AgeFromDays`/`AgeToDays` as derived (`AgeFromValue` × unit-factor) recomputed only when value or unit changes. (Choose during Phase 4 planning; the entity already carries the columns for option (b).)
- `CreateTestTypeAsync` becomes transactional with one auto-created `TestComponent` row (Q4a-3 single-component bootstrap). Component code defaults to the test's `TypeCode`; component name defaults to `TypeNameEn`; `ResultType="NUMERIC"`; `SortOrder=1`. This unblocks the kill of the synthetic path.
- `RoutineResultService`: confirm the matcher only selects `IsActive=true` ranges (today it filters by `ComponentId` only; once Phase 4 ships, supersede rows must not be selected).
- `NormalRangeWindowViewModel.InitializeAsync`: remove the synthetic-component injection (`:79-90`). After Phase 4 every test has a real first component on disk, so this code is no longer needed.
- `NormalRangeListViewModel.AddRange`: align entity initial `Sex` with VM `RangeSex` default — or set `_sex = RangeSex.B` on Add (fixes R7).

**Complexity.** Medium.
**Dependencies.** Phase 2 (regression net), Phase 3 (after the dead radio group is gone).
**Paired testing scope (Service + ViewModel).**
- *Before:* lock `SaveRangeAsync` numerical behavior with a deliberate "buggy-baseline" test that the new behavior is expected to flip; lock `RoutineResultService` matching against `(Sex, AgeFromDays, AgeToDays, ForPregnantOnly)` plus `IsActive=true`.
- *After:* round-trip test (load a range, save unchanged, reload — value unchanged); supersede test (edit a range, observe old `IsActive=false` + `SupersededById` set, new row `Version=old+1` + `IsActive=true`); historical-result anchoring test (an existing `TestResult.NormalRangeId` still resolves to the old row after supersede); `CreateTestTypeAsync` auto-creates exactly one `TestComponent`; deleting that last `TestComponent` is blocked.

### Phase 5 — Component editor + surface the missing range fields (G-1, G-2, R1, R6, D-3, D-5)

**Goal.** Add a real component panel to the Test Type window (per D-5), enable per-component unit entry, expose the in-scope `NormalRange` fields per Q4a-4, and support component reordering (D-3).

**Rough scope.**
- `TestDataManagementWindow.xaml`: insert a new panel/tab listing `Components` for the current Test Type with Add / Delete / Reorder buttons, plus per-row binding to `ComponentNameEn`, `ComponentCode`, `Unit` (soon to be lookup), `ResultType`, `DecimalPlaces`, and `SortOrder`.
- `TestDetailViewModel` and `TestCatalogService`: update to handle transactional save of `TestType` along with its `TestComponent`s, maintaining `SortOrder`.
- `NormalRangesWindow.xaml`: remove component management (creation/deletion/reordering) completely. The window should only display the components as read-only and allow managing reference ranges for the selected component.
- Add UI bindings for the previously-orphan fields: `FastingState`, `LowCritical`, `HighCritical`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `AgeDescription`, `RangeNote`.
- Update `NormalRangeWindowViewModel`'s Save aggregation.

**Complexity.** High.
**Dependencies.** Phase 4 must land first (versioning + bootstrap).
**Paired testing scope (ViewModel).**
- *Before:* lock `NormalRangeListViewModel.LoadComponents`; lock `NormalRangeWindowViewModel.InitializeAsync` (which now reads real components from disk); lock `TestDetailViewModel.LoadAsync/SaveAsync`.
- *After:* tests covering add/rename/delete `TestComponent` round-trip from the Test Type window; reorder updates `SortOrder`; add range under non-first component writes the right `ComponentId`; critical-limits round-trip; fasting state round-trip; age description and range note round-trip.

### Phase 6 — Units of Measurement Lookup Table (D-1)

**Goal.** Replace free-text unit entry with a dedicated, user-managed Units table to improve consistency.

**Rough scope.**
- `Models/Unit.cs`: create a new entity for Units.
- `FinalLabDbContext`: add EF configuration.
- `Migration`: backfill existing `TestComponent.Unit` and `NormalRange.Unit` values into the new table.
- `ITestCatalogService`: add CRUD methods for Units.
- `TestDataManagementWindow` / `NormalRangesWindow`: update unit textboxes to be editable comboboxes bound to the new lookup list, allowing users to select or add new units.

**Complexity.** Medium.
**Dependencies.** Phase 5 (easier if component panel is already in the Test Type window).
**Paired testing scope (Service + ViewModel).**
- *Before:* lock existing component and range save behavior.
- *After:* tests covering Unit CRUD; tests ensuring newly added units are available in the lookup; tests ensuring components and ranges save the correct selected unit.

### Phase 7 — Tube Master Table (D-8)

**Goal.** Replace the hardcoded tube list with a database-managed master table for tube types.

**Rough scope.**
- `Models/TubeMaterial.cs` (or equivalent): create a new master entity, as the existing `SampleTube` is per-visit.
- `FinalLabDbContext`: add EF configuration.
- `Migration`: seed the table with the existing 7 hardcoded tubes.
- `ITestCatalogService`: add CRUD methods for Tube Materials.
- UI: create a basic management window/dialog for Tube Materials.
- `TestDetailViewModel`: replace the hardcoded `AvailableTubeTypes` with a dynamic list fetched from the service.

**Complexity.** Medium.
**Dependencies.** None.
**Paired testing scope (Service + ViewModel).**
- *Before:* lock `TestDetailViewModel` tube selection behavior.
- *After:* tests covering TubeMaterial CRUD; tests ensuring `TestDetailViewModel` populates its tube dropdowns from the database.

### Phase 8 — Reference Type classification list (G-6)

**Goal.** Replace the dead "Reference type" combo on the Test Type window with the classification list per Q4a-5.

**Rough scope.**
- Decide between (a) a constant enum + small lookup `Models/TestReferenceClassification.cs` with seeded values (Numeric Range, Qualitative Result, Positive / Negative, Titer, Free Text, Mixed Text + Numeric), or (b) a string constant list. Tradeoff: enum is type-safe but harder to extend without migration; constants are easy to extend but allow drift.
- Bind `TestDetailViewModel.ReferenceType` to an `ItemsSource` exposing the list and keep the combo `IsEditable=False` (so values can't drift to arbitrary text).
- Service GET method (`GetReferenceClassificationsAsync`) on `ITestCatalogService` if option (a) is chosen.
- Decide whether existing free-text `TestType.ReferenceType` values need a migration backfill (likely yes — flag as a Phase-8 sub-task).

**Complexity.** Low–Medium.
**Dependencies.** None hard. Pleasant to bundle with Phase 3.
**Paired testing scope (Service + ViewModel).**
- *Before:* lock `TestDetailViewModel.ReferenceType` round-trip via Phase 2.
- *After:* lookup-population test; service GET test (if option (a)); save+reload yields the same classification value; arbitrary text input is rejected by the combo (UI test or property setter validation).

### Phase 9 — Tube source-of-truth alignment (G-16)

**Goal.** Reconcile `TubeType="Default"` flattening with prior barcode work, ensuring Phase 7's new master lookup interacts safely with it.

**Rough scope.** Small ViewModel / Service edits. Must not change the prior barcode workflow outside this window.

**Complexity.** Low.
**Dependencies.** Phase 7 and confirmation that prior barcode contracts are stable.
**Paired testing scope (ViewModel/Service).**
- *Before:* lock `BuildTubes` and `UpdateTestTypeAsync` tube-replace.
- *After:* tubes survive round-trip including any non-Default `TubeType` values already in the DB; deleting a referenced tube material is blocked.

### Phase 10 — Coverage backfill (G-15)

**Goal.** Close any test-coverage gaps not already addressed by paired scopes above (`TestRowViewModel`, `TestDataManagementViewModel` flow tests, `NormalRangeWindowViewModel.InitializeAsync` final shape).

**Complexity.** Low–Medium.
**Dependencies.** Phases 3-9.
**Paired testing scope.** This phase is the testing.

---

## Section D — Open Questions for the User

**All questions have been resolved.** The decisions below are now incorporated into the roadmap.

- **D-1. [ANSWERED] (Units of measurement):** Create a dedicated Units lookup table. Users select from the list and can add new units. Free-text entry is retired.
- **D-2. [ANSWERED] (Sex × age granularity):** Support highly granular sex-and-age-bounded ranges with user-configurable boundaries (day-precision), covering fine pediatric brackets.
- **D-3. [ANSWERED] (TestComponent ordering):** Provide user-controlled display order, stored persistently, and respected by printed reports and result displays.
- **D-4. [ANSWERED — Q4a-5]** Reference type is a classification of how the reference result is expressed/evaluated (Numeric Range / Qualitative Result / Positive-Negative / Titer / Free Text / Mixed Text + Numeric). It does NOT represent a clinical-guideline source (WHO/CLSI/etc.).
- **D-5. [ANSWERED] (Component editor location):** Component CRUD and reordering move to the **Test Type management interface**. The Reference Range window handles range editing only.
- **D-6. [ANSWERED] (ReportNameLine2 / BillNameLine2):** Remove from the model and UI entirely. Treat as legacy.
- **D-7. [ANSWERED — Q4a-1]** Remove the upper "Range for" radio group. No new semantics implemented for it.
- **D-8. [ANSWERED] (Tube list source):** Use a database-managed tube master table with user-managed CRUD, replacing hardcoded arrays.

**Step 4a resolutions captured here for the record (referenced from the corresponding gap entries):**

- **Q4a-1** Remove upper "Range for" radio group (G-14 / R3 / R8).
- **Q4a-2** `NormalRange` versioning is real: insert-new-and-supersede, old row `IsActive=false`, `SupersededById` set, new row `Version=old+1`, historical `TestResult` rows continue to reference the old `RangeId`, new results use the new row (G-4 / R10).
- **Q4a-3** Every `TestType` always has at least one real persisted `TestComponent`; auto-create that component during test-type creation; remove the synthetic in-memory component path entirely; never allow a test type to exist without its required `TestComponent` (G-5 / R9).
- **Q4a-4** `LowCritical`, `HighCritical`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `FastingState`, `AgeDescription`, `RangeNote` are all in scope, surface them in the UI in the appropriate phase (G-13 / R6).
- **Q4a-5** Reference type field = classification of how the reference RESULT is expressed (six values listed above). NOT a guideline source label (G-6 / D-4).
- **Q4a-6** Correct the UI label, keep the `TurnaroundHours` property/column (G-10 / B2).

---

## Section E — Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| **E-1.** Implementing versioning silently breaks `RoutineResultService` (e.g. matcher returns a superseded row alongside the new one). | Medium | High | Phase 2 regression net locks the current matcher behavior; Phase 4 adds an `IsActive=true` filter and a unit test for it. Have an integration test against a seeded multi-version range before deploy. |
| **E-2.** Auto-creating a `TestComponent` for every existing `TestType` requires a data migration; existing rows may already lack components. | Medium | Medium | Phase 4 ships paired with a migration that backfills one `TestComponent` per `TestType` lacking one, using `TypeCode`/`TypeNameEn` as the component's code/name. Validate with a count query before/after. |
| **E-3.** Removing the synthetic-component path before the migration runs will break tests with no real components today (cannot save ranges at all). | High | High | Migration must run before code change reaches production. In Phase 4 PR, gate the synthetic-path removal behind the migration committing successfully. |
| **E-4.** Age round-trip fix may corrupt rows whose `AgeFromDays`/`AgeToDays` were already inflated by historical double-conversion. | Medium | Medium | Before Phase 4 lands, run a one-off data audit query: any range with `AgeUnit="Years"` and `AgeFromDays > 36500` (>100y) is a corruption candidate; same for `AgeUnit="Months"` and `AgeFromDays > 1500` (>50y). Document the audit query in the Phase 4 PR. |
| **E-5.** Phase 3 label change "Test time (Hours)" may surprise users who have been treating the field as days. | Low | Low | Coordinate with whoever owns the lab's SOP/turnaround timing; communicate the label change in release notes. Data is unchanged. |
| **E-6.** Component panel (Phase 5) increases the surface area of the Test Type window; lab staff may need retraining. | Low | Medium | Stage the component panel behind a small UX walk-through; keep the existing flow visually unchanged for users managing legacy tests. |
| **E-7.** `TestCatalogService` is a scoped service holding the `DbContext`; multi-step save (test-type-then-component) in Phase 5 could span multiple DbContext instances if invoked from short-lived VMs. | Low | High | In Phase 5, wrap the multi-step save in a single service method that owns its own transaction (`BeginTransactionAsync`). Do not split test save and component save across separate service calls. |
| **E-8.** Concurrent edits to the same `NormalRange` after versioning lands could produce competing supersede chains (two new versions both pointing back to the same superseded row). | Low | Medium | Phase 4 includes a uniqueness consideration: enforce at the service that the row being superseded still has `IsActive=true` at supersede time (re-check inside the transaction); reject the second writer with a friendly error. |
| **E-9.** Replacing `Reference type` free text with a fixed classification may invalidate existing `TestType.ReferenceType` values. | Medium | Low | Phase 8 sub-task: data backfill / mapping table from existing free-text values to the new classification. Items with no clear mapping default to "Free Text". |
| **E-10.** Tube master change (Phase 7) may interact with the prior barcode work in non-obvious ways (G-16), managed in Phase 9. | Low | Medium | Verify prior barcode contracts first. Land a paired regression test that round-trips a tube with a non-Default `TubeType` through the Test Type window. |
| **E-11.** No unit tests for `TestCatalogService` or affected ViewModels today (G-15) means every later phase is high-risk until Phase 2 lands. | High (today) | High | Phase 2 is the hard gate. No behavior-changing phase should start before its regression net is in place. |
| **E-12.** (D-1 Unit Lookup) Historical unit strings may not map cleanly to the new Units table if they contain typos or variations. | Medium | Medium | Migration script must group by lowercased/trimmed values and generate a normalized distinct list of Units to insert, pointing old records to these normalized versions. |
| **E-13.** (D-6 Drop Columns) Dropping `ReportNameLine2` and `BillNameLine2` could break existing compiled reports or external integrations depending on the schema. | Low | High | Conduct a full codebase and report template search for these exact column names before running the dropping migration. |
| **E-14.** (D-5 Component Move) Existing workflow expectations will be disrupted; users accustomed to defining components in the Normal Ranges window will fail to find them. | High | Medium | Provide in-app guidance, release notes, and possibly a temporary redirect button in the Normal Ranges window pointing to the Test Type window. |
| **E-15.** (D-2 Granularity) Extremely fine age granularity (down to 1 day) might stress the `RoutineResultService` matcher logic if overlapping bounds are misconfigured. | Medium | Medium | Introduce validation logic ensuring no overlaps occur for the same Sex when ranges are saved. |

---

## Phase 1 Closure

This file and `HANDOFF.md` are the only artifacts of Phase 1. No source file was edited. No git command was run. Implementation, if approved, begins in Phase 2 in a separate session.

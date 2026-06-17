# HANDOFF — Test Catalog & Reference Ranges

Authored at the end of Phase 1 (analysis-only) of the Test Catalog & Reference Ranges initiative. This document is self-contained so it can be handed to any future coding agent even if `work_plan.md`'s session ends prematurely.

**Status:** No code changes have been made. No git operations were performed. Only this file and `work_plan.md` were created during Phase 1.

---

## 1. Feature Domain Summary

### 1.1 Two structural categories of tests

The system must represent two different shapes of laboratory test:

1. **Single-component test** — one result value, one unit of measurement, one set of reference ranges. Example: Creatinine (mg/dL).
2. **Multi-component (panel) test** — many sub-results reported together, each with its own unit and its own reference ranges. Examples: CBC (Hemoglobin, WBC, RBC, Platelets, differentials), Complete Urine Analysis, Complete Stool Analysis.

**Decision recorded in Phase 1:** Both shapes are modeled uniformly via `TestType → TestComponent → NormalRange`. Single-component tests get exactly one persisted `TestComponent` row, auto-created with the test type. The earlier "synthetic in-memory component" workaround is to be eliminated.

### 1.2 Units of measurement

- A single-component test stores its unit on its one `TestComponent.Unit` row.
- A multi-component test stores a unit per `TestComponent`, so different sub-components can use different units (e.g. Hemoglobin g/dL, WBC count cells/µL).
- `NormalRange.Unit` is a denormalized snapshot used by the result-entry path (`RoutineResultService`).

### 1.3 Sex / age conditional reference ranges

The same component can have multiple `NormalRange` rows, each scoped by:
- Sex (Male / Female / Both), AND/OR
- Age range (`AgeFromDays`..`AgeToDays`), AND/OR
- Pregnancy state (`ForPregnantOnly`)

The matcher in `RoutineResultService.cs:81-117` selects the row whose conditions match the patient at the moment of result entry, then snapshots its limits onto the `TestResult`.

### 1.4 Versioning contract for reference ranges (authoritative)

`NormalRange` rows are **immutable once saved**. Modification = insert-new-and-supersede:
- Old row → `IsActive = false`, `SupersededById` = new row's `RangeId`.
- New row → `Version = oldVersion + 1`, `IsActive = true`.
- Historical `TestResult` rows continue to reference the old `RangeId` (via `TestResult.NormalRangeId` + the snap fields). New results use the latest active row.

This contract is documented in `Models/NormalRange.cs:6-12` but the current `TestCatalogService` updates in place. Fixing this is in scope.

---

## 2. Current State (condensed)

### 2.1 Test Type Add/Edit window

`Views/Settings/TestDataManagementWindow.xaml` + `TestDataManagementWindow.xaml.cs` + `ViewModels/Settings/TestDataManagementViewModel.cs` + `TestDetailViewModel.cs` + `TestListViewModel.cs` + `TestRowViewModel.cs`.

Works correctly: search by name/group/code, filtered DataGrid, English/Arabic/Report/Bill/History names, Group/Collection comboboxes, behavior checkboxes (See Report / Print with other / Add with group / Main test), tubes 1-3, barcode name, outside-lab conditional fields, prices, Sort order, Add/Edit/Save/Delete/Close commands, opening the Normal Ranges window.

Does not work correctly (full list in §3): Patient Question state on load (B1); "Test time (Day)" label mismatch (B2); Cancel reverts only 5 fields (B3); `ReportNameLine2`/`BillNameLine2` unreachable (B4); Reference type dropdown has no items (B5); no multi-component editor (B6); no unit-of-measure field (B7); tube list hardcoded (B8); "By test ID" search uses TypeCode (B9).

### 2.2 Reference Range Add/Edit window

`Views/Settings/NormalRangesWindow.xaml` + `NormalRangesWindow.xaml.cs` + `ViewModels/Settings/NormalRangeWindowViewModel.cs` + `NormalRangeListViewModel.cs` + `NormalRangeDetailViewModel.cs`.

Works correctly: range DataGrid for the auto-selected component, Sex radios, Age from/to + unit dropdown, Reference range textarea, Low/High limits + flags + comments, Test unit combo, For-pregnant-only flag, Add range / Delete range / Save / Cancel / Back buttons.

Does not work correctly (full list in §3): no component selector (R1); Save never persists components (R2); two redundant Sex axes (R3); orphan enable-flags (R4); age value is double-converted on every save (R5); critical-limits / fasting / age-description / range-note / critical-text/flag/comment have no UI binding (R6); new-range Sex default mismatch (R7); "Range for" radios never persisted (R8); synthetic component cannot save its ranges (R9); versioning contract violated by every update (R10).

### 2.3 Model layer

Entity chain for both shapes is uniform:

```
TestGroup (1..N TestType)
  TestType (TesttypeId, TypeCode UQ)
    TestTypePrice (Patient Price, Lab-to-Lab Price)
    TestTypeSampleTube (cascade delete)
    TestComponent (ComponentId; UQ on (TesttypeId, ComponentCode); Unit)
      NormalRange (RangeId; Sex, AgeFromDays/ToDays, AgeUnit, ForPregnantOnly,
                   LowNormal, HighNormal, LowCritical, HighCritical, NormalRangeText,
                   LowFlag/HighFlag, LowComment/HighComment, FastingState,
                   CriticalRangeText/Flag/Comment, AgeDescription, RangeNote,
                   Version, IsActive, SupersededById)
```

EF config confirms: `NormalRange` has versioning columns wired (`FinalLabDbContext.cs:547-558`); `FK_NormalRange_Component` is `OnDelete(Restrict)`; `TestComponent` has unique `(TesttypeId, ComponentCode)`.

No `Unit` lookup entity. Unit is free text on `TestComponent.Unit` (nvarchar(30)) and `NormalRange.Unit` (nvarchar(20)).

### 2.4 Service layer

`Services/Interfaces/ITestCatalogService.cs` + `Services/Implementations/TestCatalogService.cs` cover **both** the catalog and the reference ranges. No separate `IReferenceRangeService`.

Methods present:
- Test type: `GetAllTestTypesAsync`, `GetTestTypeDetailsAsync`, `GetTestTypesPagedAsync`, `CreateTestTypeAsync` (transactional with prices+tubes), `UpdateTestTypeAsync` (wholesale tube replace, price upsert), `DeleteTestTypeAsync` (soft-delete-if-has-visits).
- Component: `AddComponentAsync`, `UpdateComponentAsync`, `DeleteComponentAsync` (manual child cleanup).
- Range: `AddRangeAsync`, `UpdateRangeAsync`, `DeleteRangeAsync`, `SaveRangeAsync` (upsert in place — violates versioning contract), `GetRangesForComponentAsync`, `GetRangesForTestTypeAsync`.
- Lookups: categories / groups / collection types CRUD.

Service-layer issues to be fixed:
- Versioning is never applied. `SaveRangeAsync`/`UpdateRangeAsync`/`AddRangeAsync` all write in place.
- Age conversion (`ConvertAgeToDays`) is applied unconditionally on every write whenever `AgeUnit` is set — load-and-save round-trip multiplies the value.
- `CreateTestTypeAsync` does not auto-create the mandatory first `TestComponent` — required by the Q4a-3 decision.
- No transaction wraps multi-step component+range writes (the unreached `NormalRangeListViewModel.SaveAllAsync`).

---

## 3. Non-Responsive Fields & Buttons — Root Causes

| # | UI element | File: line | Root cause |
|---|---|---|---|
| B1 | "Patient Question" checkbox + textarea after load | `TestDetailViewModel.cs:253-265, 286-307` | `HasPatientQuestion` is a VM-only flag never set from the loaded entity in `LoadAsync`. |
| B2 | "Test time (Day)" label vs `TurnaroundHours` | XAML:315-317; `TestDetailViewModel.cs:161-165`; `TestType.cs:48` | Label says days, property/column is hours. **Decision (Q4a-6): correct the label, keep `TurnaroundHours` as-is.** |
| B3 | Cancel button partial revert | `TestDetailViewModel.cs:395-412` | `SaveBaseline` snapshots only Tube1/2/3 + ReferenceType + BarcodeName. All other entity-backed edits remain. |
| B4 | `ReportNameLine2`, `BillNameLine2` | XAML:147-167; `TestDetailViewModel.cs:131-147` | VM/entity properties present, XAML never binds them — unreachable. |
| B5 | "Reference type" combo | XAML:321-324; `TestDetailViewModel.cs:63` | `IsEditable=True` + no `ItemsSource`. **Decision (Q4a-5): bind to the classification list (Numeric Range, Qualitative Result, Positive/Negative, Titer, Free Text, Mixed Text + Numeric).** |
| B6 | No multi-component editor | `TestDataManagementWindow.xaml`, `TestDetailViewModel.cs` | No component panel in the window — panel-type tests cannot be configured here. |
| B7 | No unit-of-measure field on the Test Type window | `TestDataManagementWindow.xaml` | Single-component unit cannot be entered from this window today. |
| B8 | Tube types hardcoded | `TestDetailViewModel.cs:71` | `AvailableTubeTypes` is a static VM array. Not sourced from any data table. |
| B9 | "By test ID" search semantics | XAML:77-80; `TestListViewModel.cs:96-97` | Box labeled "test ID" but filter calls `row.TypeCode.Contains(...)`. |
| R1 | Component selector UI on Normal Ranges window | `NormalRangesWindow.xaml`; `NormalRangeListViewModel.cs:31, 67-69, 107-132` | VM exposes Components + AddComponent/DeleteComponent commands; window has no list/buttons to render or invoke them. |
| R2 | Save button does not persist components | `NormalRangeWindowViewModel.cs:69`; `NormalRangeDetailViewModel.cs:285-311`; `NormalRangeListViewModel.cs:84-105` | `SaveCommand` only calls `Detail.SaveCommand` → `SaveRangeAsync`. `List.SaveAllAsync` is never invoked. New components stay at `ComponentId=0`; their ranges cannot be linked. |
| R3 | Two redundant Sex radio groups | XAML:117-130; `NormalRangeDetailViewModel.cs:48-95` | "Range for" vs "Sex" duplicate the same axis. **Decision (Q4a-1): remove the upper "Range for" group.** |
| R4 | `IsSexEnabled` / `IsAgeEnabled` orphan VM properties | `NormalRangeDetailViewModel.cs:65-67`; XAML | Defined and broadcast but no XAML binds them. |
| R5 | Age double-conversion on save | XAML:133-141; `TestCatalogService.cs:341-345, 359-363, 539-547, 554-558` | Service multiplies `AgeFromDays/ToDays` by 365/30/1 on every save whenever `AgeUnit` is set. A loaded "5 years" range (1825 days) re-saved becomes 1825×365. |
| R6 | Many entity/VM properties unbound | `NormalRangeDetailViewModel.cs:125-219`; XAML | `FastingState`, `LowCritical`, `HighCritical`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `AgeDescription`, `RangeNote` — all unreachable. **Decision (Q4a-4): all are in scope; surface in a later phase.** |
| R7 | New-range Sex default mismatch | `NormalRangeListViewModel.cs:139-149`; `NormalRangeDetailViewModel.cs:22-23, 87-95, 299-305` | Entity initialized `Sex="Both"`; VM `_sex` defaults to `RangeSex.M` (enum 0). UI shows "Male" while entity carries "Both". On Save the radio wins → "M" is silently written. |
| R8 | "Range for" radio group is a dead control | `NormalRangeDetailViewModel.cs:48-85, 299-305`; XAML:117-122 | Never written to the entity on Save. Resolved by R3 decision (remove). |
| R9 | Synthetic component cannot be persisted | `NormalRangeWindowViewModel.cs:79-90`; `NormalRangeListViewModel.cs:139-149`; `TestCatalogService.SaveRangeAsync` | Auto-injected in-memory `TestComponent` has `ComponentId=0`. New range references it via `ComponentId=0`; `SaveRangeAsync` violates `FK_NormalRange_Component`. **Decision (Q4a-3): remove the synthetic-component path entirely; auto-create one persisted `TestComponent` row at test-type creation time.** |
| R10 | Versioning contract violated | `NormalRange.cs:6-12`; `TestCatalogService.cs:352-388, 550-599` | Service edits in place; never sets `IsActive=false`, `SupersededById`, or bumps `Version`. **Decision (Q4a-2): implement insert-new-and-supersede.** |

---

## 4. Gap List

| Gap ID | Description | Affected Layer | Complexity |
|---|---|---|---|
| G-1 | Multi-component test editing has no UI (component selector missing from both windows). | Views, ViewModels | High |
| G-2 | Per-component unit of measurement has no entry point in the Test Type window; unit is entered per range rather than per component on the Normal Ranges window. **Decision (D-1): unit picker must source from a user-managed Units lookup table — adds a new lookup foundation.** | Model, Service, Views, ViewModels, Migration | Medium |
| G-3 | Sex/age-conditional ranges work at the data layer but UX is broken: redundant radios (R3, R8), double age conversion (R5), Sex default mismatch (R7), only first component reachable (R1). | View, ViewModel, Service | High |
| G-4 | `NormalRange` versioning contract is documented but ignored — fix per Q4a-2. | Service | Medium |
| G-5 | Synthetic in-memory `TestComponent` cannot be saved — replace with auto-created persisted row per Q4a-3. | Service, ViewModel | Medium |
| G-6 | "Reference type" combo has no `ItemsSource` — bind to classification list per Q4a-5. | View, Model (small) | Low |
| G-7 | Patient Question state not restored on load (B1). | ViewModel | Low |
| G-8 | Cancel reverts only 5 of ~25 editable fields (B3). | ViewModel | Low |
| G-9 | `ReportNameLine2`, `BillNameLine2` unreachable (B4). **Decision (D-6): remove from `Models/TestType.cs`, `TestDetailViewModel.cs`, EF config; ship a migration to drop the DB columns. Audit reporting/printing surfaces for any reads first.** | Model, View, ViewModel, EF config, Migration | Low–Medium |
| G-10 | "Test time (Day)" label vs `TurnaroundHours` unit — fix label per Q4a-6. | View | Low |
| G-11 | "By test ID" search hits TypeCode, not numeric ID (B9). | View / ViewModel | Low |
| G-12 | Tube list is a hardcoded VM array (B8); no master table backs it. **Decision (D-8): source from a user-managed tube master table.** **Caveat:** existing `Models/SampleTube.cs` is per-visit (carries `VisitId`/`BarcodeValue`/`CollectedAt`), not a master catalog — Phase 6 must either introduce a new master entity (recommended) or split `SampleTube`. | Model, Service, ViewModel, Migration | Medium |
| G-13 | `FastingState`, `LowCritical`, `HighCritical`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `AgeDescription`, `RangeNote` have no UI binding (R6) — in scope per Q4a-4. | View | Medium |
| G-14 | "Range for" radio group is a dead control (R8); orphan enable-flags (R4) — delete per Q4a-1. | View, ViewModel | Low |
| G-15 | Lack of unit test coverage for `TestCatalogService`, `RoutineResultService`, and all six TestType/NormalRange ViewModels. | Tests | Medium |
| G-16 | Possible behavioral coupling between `UpdateTestTypeAsync`'s wholesale `RemoveRange` of tubes and the prior barcode work (`TubeType` hardcoded to "Default"). | Service / cross-feature | Low–Medium (verify first) |

---

## 5. Load-Bearing Files for This Feature Area

**Views**
- `Views/Settings/TestDataManagementWindow.xaml`
- `Views/Settings/TestDataManagementWindow.xaml.cs`
- `Views/Settings/NormalRangesWindow.xaml`
- `Views/Settings/NormalRangesWindow.xaml.cs`

**ViewModels**
- `ViewModels/Settings/TestDataManagementViewModel.cs`
- `ViewModels/Settings/TestDetailViewModel.cs`
- `ViewModels/Settings/TestListViewModel.cs`
- `ViewModels/Settings/TestRowViewModel.cs`
- `ViewModels/Settings/NormalRangeWindowViewModel.cs`
- `ViewModels/Settings/NormalRangeListViewModel.cs`
- `ViewModels/Settings/NormalRangeDetailViewModel.cs`

**Services**
- `Services/Interfaces/ITestCatalogService.cs`
- `Services/Implementations/TestCatalogService.cs`
- `Services/Implementations/RoutineResultService.cs` (downstream consumer of `NormalRange` for result flagging)

**Models**
- `Models/TestType.cs`
- `Models/TestComponent.cs`
- `Models/NormalRange.cs`
- `Models/TestGroup.cs`
- `Models/CollectionType.cs`
- `Models/TestTypeSampleTube.cs`
- `Models/TestTypePrice.cs`

**Data**
- `Data/FinalLabDbContext.cs` — entity configuration at lines 487-569 (NormalRange), 1094-1132 (TestComponent), 1251-1362 (TestType), 1365-1388 (TestTypePrice), 1391-1427 (TestTypeSampleTube).

**App / DI**
- `App.xaml.cs:94-95` — navigation window registration.
- `App.xaml.cs:182-194` — DI registration for the seven ViewModels + two Windows.

**Mission file (for context)**
- `Docs/MISSION_TESTCATALOG.md`

---

## 6. MVVM Constraints (always apply)

- No `MessageBox.Show` in ViewModels. Use `IDialogService` (already injected into `TestDataManagementViewModel`, `TestDetailViewModel`, `NormalRangeListViewModel`, `NormalRangeDetailViewModel`).
- No `async void` except framework-required signatures (WPF event handlers, e.g. `Window_Loaded`). Sibling-VM event handlers like `OnSelectedTestChanged` and `OnOpenNormalRangesRequested` currently use `async void`; they sit on `EventHandler<T>` signatures and must wrap their body in try/catch (they already do).
- No business logic in code-behind. The current code-behinds (`TestDataManagementWindow.xaml.cs`, `NormalRangesWindow.xaml.cs`) hold only constructor wiring and a `Window_Loaded` that delegates to `IAsyncInitializable.InitializeAsync` — keep them that thin.

### 6.1 Protected files — must not be modified

- `Infrastructure/Security/PasswordHasher.cs`
- `Infrastructure/ViewModelBase.cs`
- `Infrastructure/Navigation/NavigationService.cs`
- Anything under `Migrations/`

---

## 7. Existing Test Coverage Summary

**Project:** `FinalLabSystem.Tests` (net8.0-windows; xUnit 2.5.3; EFCore.InMemory 8.0.0; Moq 4.20.70). Project reference to `FinalLabSystem.csproj`.

**Existing files**
- `Services/AuthServiceTests.cs` — 5 tests on `AuthService`/`Staff`.
- `Services/PatientServiceTests.cs` — 3 tests on `PatientService` search/paging.
- `Validation/EntityValidationTests.cs` — 3 tests; `TestType_WithNegativePrice_FailsDataAnnotationValidation` touches the `[Range]` annotation on `TestType.DefaultPrice` but does not exercise catalog or reference-range behavior.

**No coverage exists for** `TestCatalogService`, `RoutineResultService`, `TestDataManagementViewModel`, `TestDetailViewModel`, `TestListViewModel`, `TestRowViewModel`, `NormalRangeWindowViewModel`, `NormalRangeListViewModel`, `NormalRangeDetailViewModel`.

The test infrastructure (InMemory database + Moq) is proven and ready to use for the regression net in the next phase.

---

## 8. Open Questions — All Resolved

The mission's Step 4a was fully resolved before this file was first written. The remaining Section D product-review questions were resolved in a follow-up round documented at `Docs/MISSION_TESTCATALOG_DECISIONS_D.md`. **No questions remain pending user input.**

Decisions summary (full text in `work_plan.md` Section D):

- **D-1 (Units of measurement)** — Dedicated Units lookup table; users select from list and can add new units; free-text entry retired as primary mechanism.
- **D-2 (Sex × age granularity)** — Highly granular sex-and-age-bounded ranges with fully user-configurable age boundaries (day-precision); pediatric brackets such as 1-2 days / 2-3 days / 3-7 days / 7-14 days must be supportable.
- **D-3 (TestComponent ordering)** — User-controlled display order, stored persistently, respected by printed reports and result display screens.
- **D-4 (Reference Type concept)** — Resolved in Step 4a Q4a-5: classification of how the reference RESULT is expressed (Numeric Range / Qualitative Result / Positive-Negative / Titer / Free Text / Mixed Text + Numeric). NOT a clinical guideline source.
- **D-5 (Component editor location)** — Component CRUD + reorder lives in the **Test Type management interface**, not the Reference Range window. Reference Range window keeps range editing only.
- **D-6 (ReportNameLine2 / BillNameLine2)** — Remove from model + UI; treat as legacy.
- **D-7 ("Range for" radio group)** — Resolved in Step 4a Q4a-1: delete the upper radio group; no new semantics.
- **D-8 (Tube list source)** — Database-managed tube master table; user-managed CRUD; not hardcoded. **Caveat:** existing `Models/SampleTube.cs` is per-visit, not a master — the implementing phase must introduce a new master entity (recommended) or split `SampleTube`.

---

## 9. Phase 1 Output Inventory

This Phase 1 session produced exactly two files:
- `FinalLabSystem/Docs/HANDOFF.md` (this file)
- `FinalLabSystem/Docs/work_plan.md`

No existing source file was edited. No git operation was performed. No implementation work was done.

# Test Data Management — Work Plan (Revision 2)

> Target deliverable path requested by user: `FinalLabSystem/Docs/test_data_work_plan.md`.
> Plan mode restricts edits to this plan file; the final document will be created at the requested path upon plan approval.

---

## 1. Context

The lab system currently exposes one main module on the `MainWindow` toolbar — "المرضى" (Patients). A second module, **"إعدادات النظام" (System Settings)**, must be added to host configuration screens. Its first sub-screen is **"بيانات التحاليل" (Test Data Management)**, where lab administrators add and edit the catalogue of `TestType` records, the sample-tube containers required per test, the outside-lab routing configuration, the patient pre-test question, and the normal reference ranges.

The need is driven by:

- The existing `TestType` row is exposed only as read-only data inside `ITestCatalogService` — there is currently no UI to create, edit, or delete tests at runtime.
- Several presentation-layer, billing, workflow-flag, outside-lab, sample-container, and patient-question fields required by the receipt printer, the result-report header, the work-list filters, and the patient-registration flow do not yet exist on `TestType`.
- Reference ranges (`NormalRange`) are stored per `TestComponent` but cannot be authored without direct database access.
- `SampleTube` is currently a per-visit instance only; there is no configuration table describing which containers a `TestType` requires.

The intended outcome is a single dedicated window that lets staff manage the full test catalogue end-to-end, plus a separate window dedicated to normal-range editing, with the same window-lifecycle and MVVM conventions already used by the Patient Registration window. **All view models are split into focused, single-responsibility components — no god view model.**

---

## 2. Scope

### In scope

1. New "System Settings" toolbar button on `MainWindow` and a central menu (analogous to `PatientsMenuViewModel`) containing a "Test Data" button.
2. New `TestDataManagementWindow` (hides `MainWindow` on open, re-shows on close — handled automatically by the existing `NavigationService`).
3. Two-zone layout: left zone = filterable list of all tests; right zone = full editor for the selected test, organised into clearly separated zones (B, C, D, E).
4. **Separate `NormalRangesWindow`** opened by a "Normal Values" button in Zone B. Its own list-left / detail-right layout managing `TestComponent` and `NormalRange` rows for the test selected in the parent window.
5. Schema migration adding the missing presentation, workflow-flag, outside-lab, and patient-question columns to `TestType`; creating the new `TestTypeSampleTube` join table; and seeding two `PriceScheme` rows ("Patient", "LabToLab").
6. Extension of `ITestCatalogService` with create/update/delete methods for `TestType`, `TestTypeSampleTube`, `TestComponent`, and `NormalRange`.
7. Update to the receipt-print code (`ReceiptDialogViewModel`) so that line items sharing the same `BillNameLine1` are collapsed into a single receipt row.

### Out of scope (per owner decisions)

- **Branch / multi-site fields.** The application is a single-lab installation; no `Branch` entity, lookup, or column.
- **Log Group / instrument grouping.** Deferred until the work-order printing feature is built.
- **Test Time in days.** Existing `TurnaroundHours` (short) is kept as the storage unit; the UI labels it "Test Time (Hours)". The receipt's expected-result calculation already works in hours.
- **Result-report rendering changes.** Adding `ReportNameLine1/2` columns is in scope, but the report-rendering code is not changed by this work item.
- **`IsOutsourceable` field semantics.** It stays as a capability flag (not used anywhere downstream today). The new `IsSendOutside` flag represents the *active decision* to route the test to an external lab, which is semantically different.

---

## 3. Field-by-field mapping (user requirement → schema)

### 3.1 Existing fields reused

| User-facing field | Backing property | Status |
|---|---|---|
| Test Name | `TestType.TypeNameEn` | exists |
| Arabic Name | `TestType.TypeNameAr` | exists |
| Group Name | `TestType.GroupId` → `TestGroup` | exists |
| Arrange Number | `TestType.SortOrder` | exists |
| Test Time (Hours) | `TestType.TurnaroundHours` | exists (relabel in UI) |
| Generic Notes | `TestType.Notes` | exists (kept separate from `CollectionNotes`) |
| Patient Price | `TestTypePrice` row where `Scheme = "Patient"` | uses existing table; seed scheme |
| Lab-to-Lab Price | `TestTypePrice` row where `Scheme = "LabToLab"` | uses existing table; seed scheme |
| Components & Ranges | `TestComponent` + `NormalRange` | exists; managed in the separate Normal Ranges window |

### 3.2 New columns on `TestType` (Zone B — presentation, billing, history)

| Column | Type | Default / Null |
|---|---|---|
| `ReportNameLine1` | `nvarchar(200)` | nullable |
| `ReportNameLine2` | `nvarchar(200)` | nullable |
| `BillNameLine1` | `nvarchar(200)` | nullable |
| `BillNameLine2` | `nvarchar(200)` | nullable |
| `HistoryName` | `nvarchar(100)` | nullable |
| `CollectionNotes` | `nvarchar(1000)` | nullable |

### 3.3 New columns on `TestType` (workflow flags)

| Column | Type | Default |
|---|---|---|
| `IsRoutineTest` | `bit` | `0` (false) |
| `SeeReport` | `bit` | `0` (false) |
| `PrintWithOther` | `bit` | `1` (true) |
| `AddWithGroup` | `bit` | `1` (true) |
| `IsMainTest` | `bit` | `0` (false) |

### 3.4 New columns on `TestType` (Zone D — outside lab)

| Column | Type | Default / Null |
|---|---|---|
| `IsSendOutside` | `bit` | `0` (false) |
| `OutsideLabName` | `nvarchar(200)` | nullable |
| `OutsideCostPrice` | `decimal(18,2)` | nullable |

> Note: `OutsideLabName` is a free-text column per the explicit owner requirement. The existing `ExternalLab` table (`ExternalLabId`, `LabName`, etc.) is not referenced — using it as an FK could be a future refinement but is not in scope here.

### 3.5 New column on `TestType` (Zone E — patient question)

| Column | Type | Default / Null |
|---|---|---|
| `PatientQuestion` | `nvarchar(500)` | nullable |

### 3.6 New join table `TestTypeSampleTube` (Zone C — sample containers)

Replaces the earlier `HasBarcode` boolean idea, which the owner correctly identified as insufficient. A single test can require multiple distinct sample containers (e.g., one EDTA tube **and** one citrate tube); the existing `SampleTube` table is per-visit only and has no link to `TestType`.

| Column | Type | Notes |
|---|---|---|
| `TestTypeTubeId` | `int` (PK, identity) | |
| `TestTypeId` | `int` (FK → `TestType.TesttypeId`, cascade delete) | |
| `TubeType` | `nvarchar(50)` | matches existing `SampleTube.TubeType` lexicon |
| `TubeColor` | `nvarchar(30)` | nullable, matches `SampleTube.TubeColor` |
| `SampleType` | `nvarchar(50)` | nullable (e.g., "Blood", "Urine"); overrides `TestType.SampleType` when present |
| `Quantity` | `int` | default `1` |
| `SortOrder` | `smallint` | default `0` |
| `IsActive` | `bit` | default `1` |
| `Notes` | `nvarchar(500)` | nullable |

The barcode for an actual visit's tube continues to live on `SampleTube.BarcodeValue` (per-visit, generated at collection time). The new table only describes the *requirement* per test type.

### 3.7 Fields explicitly NOT added

- No `HasBarcode` boolean on `TestType` — replaced by the presence of one or more `TestTypeSampleTube` rows.
- No new `Branch` column or table.
- No new `LogGroup` column or table.
- No new `TestTimeDays` column.

All new columns are nullable or have safe defaults, preserving compatibility with existing rows.

---

## 4. Data layer changes

### 4.1 Model changes

**Edit** `FinalLabSystem/Models/TestType.cs` to add the 14 new properties listed in §3.2 / §3.3 / §3.4 / §3.5, plus a navigation collection:

```
public virtual ICollection<TestTypeSampleTube> TestTypeSampleTubes { get; set; } = new List<TestTypeSampleTube>();
```

**Create** `FinalLabSystem/Models/TestTypeSampleTube.cs` with the columns from §3.6 plus `public virtual TestType Testtype { get; set; } = null!;`.

### 4.2 DbContext

Edit `FinalLabSystem/Data/FinalLabDbContext.cs`:

- Add `public virtual DbSet<TestTypeSampleTube> TestTypeSampleTubes { get; set; }`.
- In `OnModelCreating`, configure the new columns on `TestType` (max-length constraints, defaults, indexes on `BillNameLine1` and `HistoryName`).
- Configure the new `TestTypeSampleTube` entity: table name, column names, FK with cascade delete to `TestType`, default values for `Quantity`, `SortOrder`, `IsActive`.
- Keep all existing `TestType` configuration untouched.

### 4.3 Migration

Create migration `<timestamp>_AddTestDataManagementFields` (after `20260605000200_AddVisitMedicalHistoryAndOutsideSamples`). It must:

1. Add the 14 new columns to `TestType` with nullable / default semantics that don't break existing rows.
2. Create the `TestTypeSampleTube` table with PK, FK to `TestType`, and an index on `(TestTypeId, SortOrder)`.
3. Seed two rows into `PriceScheme` if absent: codes `PATIENT` and `LAB2LAB`, names `"Patient Price"` / `"Lab-to-Lab Price"`. Use idempotent SQL (`IF NOT EXISTS`).
4. Provide a clean `Down()` that drops the new columns and the join table. The seeded `PriceScheme` rows are left in place to avoid orphaning prices written in the meantime.

---

## 5. Service layer changes

Extend `FinalLabSystem/Services/Interfaces/ITestCatalogService.cs` and its implementation (locate the concrete class during implementation; follow the existing naming convention):

Add async methods:

- `Task<List<TestType>> GetAllTestTypesAsync()` — eager-loads `Group`, `TestComponents`, `TestTypePrices.Scheme`, and `TestTypeSampleTubes`. Drives Zone A.
- `Task<int> CreateTestTypeAsync(TestType entity, double patientPrice, double labToLabPrice, IReadOnlyList<TestTypeSampleTube> tubes)` — inserts the test, two `TestTypePrice` rows, and the tube requirements in a single transaction.
- `Task UpdateTestTypeAsync(TestType entity, double patientPrice, double labToLabPrice, IReadOnlyList<TestTypeSampleTube> tubes)` — updates the row, upserts the two price rows, and replaces the tube-requirement set (delete-then-insert is acceptable).
- `Task DeleteTestTypeAsync(int testTypeId)` — soft delete by toggling `IsActive = false` if the test is referenced by any `VisitTest`; hard delete otherwise (cascade removes tube rows).
- `Task<int> AddComponentAsync(int testTypeId, TestComponent component)`
- `Task UpdateComponentAsync(TestComponent component)`
- `Task DeleteComponentAsync(int componentId)`
- `Task<List<NormalRange>> GetRangesForComponentAsync(int componentId)`
- `Task<int> AddRangeAsync(NormalRange range)`
- `Task UpdateRangeAsync(NormalRange range)`
- `Task DeleteRangeAsync(int rangeId)`

The service wraps mutations in a single `SaveChangesAsync` per operation. A private helper resolves the seeded `PriceScheme` IDs once per request, keeping the `Patient` / `LabToLab` codes out of every call site.

---

## 6. Navigation & toolbar wiring

### 6.1 MainWindow toolbar

Edit `FinalLabSystem/MainWindow.xaml`: add a second `<Button>` to the existing `<ToolBar>` right after the "المرضى" button — `Content="إعدادات النظام"`, bound to `ShowSystemSettingsMenuCommand`. Match existing styling.

### 6.2 MainViewModel

Edit `FinalLabSystem/ViewModels/MainViewModel.cs`:

- Add `ICommand ShowSystemSettingsMenuCommand`.
- Add `ICommand NavigateToTestDataCommand` that calls `_navigationService.OpenTaskWindow<TestDataManagementViewModel>()`.
- Add a private method `ShowSystemSettingsMenu()` that sets `CurrentView = new SystemSettingsMenuViewModel(NavigateToTestDataCommand)`.

### 6.3 System Settings menu

Create new files mirroring the `PatientsMenu` pair:

- `FinalLabSystem/ViewModels/Settings/SystemSettingsMenuViewModel.cs`
- `FinalLabSystem/Views/Settings/SystemSettingsMenuView.xaml` (UserControl) — central panel with one large button "بيانات التحاليل".

### 6.4 Window registration

Edit `App.xaml.cs` (or wherever DI / `NavigationService.RegisterWindow` calls already live):

- Register both new view models in the DI container with their service dependencies.
- `NavigationService.RegisterWindow<TestDataManagementViewModel, TestDataManagementWindow>()`.
- `NavigationService.RegisterWindow<NormalRangeWindowViewModel, NormalRangesWindow>()`.

The existing `NavigationService.OpenTaskWindow<T>()` already hides `MainWindow` and re-shows it on close. For the Normal Ranges window (opened from inside another task window) we use a thin show-modal-dialog helper inside the parent VM, leaving `MainWindow` hidden throughout.

---

## 7. View-model architecture (no god view model)

Five focused view models. Each has a single responsibility; the coordinator simply composes the children.

```
TestDataManagementViewModel  (thin coordinator — composition + window-level commands)
├── TestListViewModel        (Zone A: list + search + filtering)
└── TestDetailViewModel      (Zone B + C + D + E: fields editor + tubes grid + outside section + patient-question section)

NormalRangeWindowViewModel   (coordinator for the separate Normal Ranges window)
├── NormalRangeListViewModel   (left panel inside that window: list of TestComponents + their ranges)
└── NormalRangeDetailViewModel (right panel inside that window: edit one NormalRange)
```

### 7.1 `TestListViewModel` — Zone A

Responsibilities:

- Hold `ObservableCollection<TestRowViewModel> AllTests` and `FilteredTests`.
- Hold `SearchMode` (Code / Group / Name) and `SearchText`. Setters call `ApplyFilter()`.
- Hold `SelectedTest`. Raises an event (or exposes a bindable property the coordinator subscribes to) when it changes.
- Expose `RefreshCommand` for reloading from the service.

Filter logic:

- **Code** — exact case-insensitive match on `TypeCode`.
- **Group** — `Contains` on `GroupNameAr` or `GroupNameEn`.
- **Test name** — left-to-right `StartsWith` on `TypeNameAr` or `TypeNameEn`, case-insensitive.

### 7.2 `TestDetailViewModel` — Zone B + C + D + E

Responsibilities:

- Hold `EditableTest` — the working copy of the selected `TestType` (or a new instance for "New").
- Expose every Zone B field as a bindable property (Test Name, Arabic Name, History Name, Report Name Line 1, Report Name Line 2, Bill Name Line 1, Bill Name Line 2, Group, Arrange Number, Test Time Hours, Collection Notes, Patient Price, Lab-to-Lab Price, plus the 5 workflow flags).
- Hold `ObservableCollection<TestTypeSampleTubeRowViewModel> Tubes` for Zone C, with `AddTubeCommand`, `EditTubeCommand`, `DeleteTubeCommand`.
- Hold Zone D properties: `IsSendOutside`, `OutsideLabName`, `OutsideCostPrice`. `OutsideLabName` and `OutsideCostPrice` controls are enabled only when `IsSendOutside == true`.
- Hold Zone E property: `PatientQuestion` (bound to a multi-line `TextBox`).
- Expose `OpenNormalRangesCommand` that asks the coordinator to show the Normal Ranges window for the current `EditableTest`.
- Dirty-tracking flag drives the parent's Save button enablement.
- Validation (manual, in-VM, per existing project convention): `TypeCode` required and unique, `TypeNameEn` required, `GroupId` required, prices non-negative, `OutsideCostPrice` required when `IsSendOutside == true`.

### 7.3 `TestDataManagementViewModel` — coordinator

Responsibilities:

- Constructor takes `TestListViewModel`, `TestDetailViewModel`, `ITestCatalogService`, `INavigationService`, and a factory for `NormalRangeWindowViewModel`.
- Subscribes to `TestListViewModel.SelectedTest` changes → calls `TestDetailViewModel.LoadAsync(testTypeId)`.
- Exposes window-level commands: `NewCommand`, `SaveCommand`, `DeleteCommand`, `CloseCommand`.
- Handles `OpenNormalRangesCommand` from the detail VM by resolving a fresh `NormalRangeWindowViewModel` and showing the `NormalRangesWindow` as a modal dialog scoped to the currently selected `TestType`.
- Owns no field state of its own — purely orchestration.

### 7.4 `NormalRangeWindowViewModel` — coordinator for the ranges window

Responsibilities:

- Holds a reference to the parent `TestType` whose ranges are being edited.
- Composes `NormalRangeListViewModel` (left) and `NormalRangeDetailViewModel` (right).
- When the user has zero `TestComponent` rows for the test on first open, auto-creates a single default `TestComponent` in memory (`ComponentCode = TypeCode`, `ComponentNameEn = TypeNameEn`, `ResultType = "NUMERIC"`) so range entry can begin immediately. The component is persisted on Save.
- Exposes window-level commands: `SaveCommand`, `CloseCommand`.

### 7.5 `NormalRangeListViewModel` — left panel of the ranges window

Responsibilities:

- Holds `ObservableCollection<TestComponent> Components` and `SelectedComponent`.
- Holds `ObservableCollection<NormalRange> RangesForSelectedComponent` (refreshed when `SelectedComponent` changes).
- Exposes `AddComponentCommand`, `DeleteComponentCommand`, `AddRangeCommand`, `EditRangeCommand`, `DeleteRangeCommand`.
- When `SelectedRange` changes, passes it to the detail VM.

### 7.6 `NormalRangeDetailViewModel` — right panel of the ranges window

Responsibilities:

- Holds an `EditableRange` (`NormalRange` working copy) bound to the form: Sex (radio: M / F / Both), AgeFromDays, AgeToDays, AgeDescription, FastingState (radio: Any / Fasting), AppliesToPregnant (checkbox), LowNormal, HighNormal, LowCritical, HighCritical, NormalRangeText, RangeNote.
- Validation: when both `LowNormal` and `HighNormal` are set, `Low <= High`. Same for critical pair.
- Dirty-tracking; `ApplyCommand` returns the validated range to the list VM.

---

## 8. The two windows

### 8.1 `TestDataManagementWindow.xaml`

- `FlowDirection="RightToLeft"`, `WindowStartupLocation="CenterScreen"`, `Height=860 Width=1320`.
- Root `Grid` with two columns: left 40% (Zone A), right 60% (Zone B with sub-zones).
- Bottom action bar: New, Save, Delete, Refresh, Close — matching existing `PatientRegistrationWindow` styling.
- Zone B is a vertically scrollable `StackPanel` (or `Grid` with rows) divided into clearly labelled groups separated by `Separator`s:
  1. **Identity** — Test Name, Arabic Name, History Name.
  2. **Report names** — Report Name Line 1, Report Name Line 2.
  3. **Bill names** — Bill Name Line 1, Bill Name Line 2.
  4. **Classification** — Group (ComboBox bound to `Groups`), Arrange Number, Test Time (Hours).
  5. **Workflow flags** — five checkboxes: Is Routine Test, See Report, Print With Other, Add With Group, Is Main Test.
  6. **Notes** — Collection Notes (multiline TextBox); generic Notes (multiline TextBox).
  7. **Pricing** — Patient Price, Lab-to-Lab Price.
  8. **Zone C — Sample Tubes / Barcode** (small `DataGrid` over `Tubes`; columns: Sort, Tube Type, Tube Color, Sample Type, Quantity, Active, Notes; with Add / Edit / Delete buttons).
  9. **Zone D — Outside Lab** — `IsSendOutside` checkbox; when checked, enables `OutsideLabName` (TextBox) and `OutsideCostPrice` (decimal TextBox).
  10. **Zone E — Patient Question** — single multiline TextBox for the question to ask the patient before sample collection (e.g., "Patient must be fasting 12 hours" / "Has the patient taken anti-coagulants in the last 7 days?").
  11. **Normal Values** — a single button "القيم الطبيعية / Normal Values" that opens `NormalRangesWindow`.

### 8.2 Zone A — Test list (column definitions only)

```
| Column header   | Bound property         | Width  |
|-----------------|------------------------|--------|
| Arrange #       | SortOrder              | 60     |
| Group           | GroupNameAr            | 140    |
| Code            | TypeCode               | 80     |
| Test Name       | TypeNameAr / TypeNameEn| *      |
| Patient Price   | PatientPrice           | 90     |
| Lab-to-Lab      | LabToLabPrice          | 90     |
| Tubes           | TubeCount              | 60     |
```

Search bar above the grid: `ComboBox` (search mode) + `TextBox` (query). `TextBox.Text` is bound `Mode=TwoWay UpdateSourceTrigger=PropertyChanged` so filtering happens in real time.

### 8.3 `NormalRangesWindow.xaml`

- `FlowDirection="RightToLeft"`, `WindowStartupLocation="CenterOwner"`, `Height=720 Width=1100`, modal (`ShowDialog`) over `TestDataManagementWindow`.
- Title shows the parent test name: e.g., "القيم الطبيعية — Complete Blood Count".
- Root `Grid` with two columns: left 45% (list panel), right 55% (detail panel).
- Bottom action bar: Save All, Close.
- **Left panel (list)** — Two stacked sections:
  - Top: `ListBox` of `TestComponent` rows (component code + name + unit) with Add / Delete buttons.
  - Bottom: `DataGrid` of `NormalRange` rows for the selected component (columns below) with Add / Edit / Delete buttons.
- **Right panel (detail)** — form for the selected range: Sex (radio), Age From (days, with helper showing "≈ X years"), Age To (days), Age Description, Fasting State (radio), Applies to Pregnant (checkbox), Low Normal, High Normal, Low Critical, High Critical, Range Text, Note. Apply button.

NormalRange grid columns:

```
| Column header   | Bound property         | Width  |
|-----------------|------------------------|--------|
| Sex             | Sex                    | 60     |
| Age From (d)    | AgeFromDays            | 80     |
| Age To (d)      | AgeToDays              | 80     |
| Description     | AgeDescription         | 110    |
| Fasting         | FastingState           | 70     |
| Low Normal      | LowNormal              | 80     |
| High Normal     | HighNormal             | 80     |
| Low Critical    | LowCritical            | 80     |
| High Critical   | HighCritical           | 80     |
| Text            | NormalRangeText        | *      |
```

---

## 9. Receipt grouping (Bill Name Line 1)

Edit `FinalLabSystem/Models/DTOs/SelectedTestDto.cs` to add `string? BillNameLine1` and `string? BillNameLine2`.

Edit `FinalLabSystem/ViewModels/Patients/ReceiptDialogViewModel.cs`:

- In `LoadVisit()`, populate the new DTO fields from `vt.Testtype.BillNameLine1` / `BillNameLine2`.
- In `CreateDetailedDocument()`, replace the existing `foreach (var test in dto.SelectedTests)` with a grouped iteration:
  - Tests with a non-null `BillNameLine1` are grouped by that key; each group prints as a single row with display name = `BillNameLine1` (and `BillNameLine2` as the small sub-line if present) and price = sum of group prices.
  - Tests with `BillNameLine1 == null` print one row each, using `TestName`, exactly as today (backwards compatible).
- The receipt grand total is unchanged.
- The receipt summary `TextBlocks` in `ReceiptDialog.xaml` do not need to change.

---

## 10. Decisions resolved with the owner

| Topic | Decision |
|---|---|
| NormalRange linkage | Per `TestComponent`. If a `TestType` has none, auto-create one default component. Range editing happens in a **separate window**. |
| Pricing | Use existing `TestTypePrice` + `PriceScheme`. Seed two schemes (`Patient`, `LabToLab`). |
| Branch | Excluded entirely — single-lab system. |
| Log Group | Deferred until the work-order printing feature exists. |
| Test Time unit | Hours, via existing `TurnaroundHours`. UI label = "Test Time (Hours)". |
| Bill Name fields | Added to schema **and** receipt-grouping logic implemented in `ReceiptDialogViewModel`. |
| Report Name fields | Both Line 1 and Line 2 added to schema. No change to report-rendering code in this work item. |
| Workflow flags | Five new bit columns added with the requested defaults. |
| Outside Lab fields | Three new columns added (`IsSendOutside`, `OutsideLabName`, `OutsideCostPrice`). `OutsideLabName` is free text per the owner's request (existing `ExternalLab` table is not used as FK in this scope). |
| Patient Question | New `nvarchar(500)` column added; Zone E TextBox in editor. |
| Sample Tubes / Barcode | New join table `TestTypeSampleTube` (1 test → many required containers). `HasBarcode` boolean dropped — replaced by the presence of rows in this table. Zone C grid manages add/edit/delete. |
| ViewModel architecture | Five focused VMs as listed in §7. No god VM. |

No further owner decisions are open at this time.

---

## 11. Phased work plan

Each phase is independently committable. After every phase the project must build cleanly and the existing tests (where they exist) must still pass.

### Phase 1 — Data model & migration

- Add the 14 new properties to `TestType` (presentation/billing/history × 6, flags × 5, outside × 3, patient question × 1).
- Create the `TestTypeSampleTube` entity and its `DbSet`.
- Update `FinalLabDbContext` configuration.
- Generate the EF Core migration; verify `Up()` and `Down()` SQL by inspection.
- Add the `PriceScheme` seed SQL inside the migration.
- Run `dotnet ef database update` against a local dev DB and confirm columns, the new table, and the two schemes are present.

### Phase 2 — Service-layer CRUD

- Extend `ITestCatalogService` with the methods listed in §5.
- Implement them, including the tube-requirement upsert.
- Smoke-test through unit tests (only if a test project exists) or defer to Phase 6 UI verification.

### Phase 3 — Toolbar & menu

- Add the "System Settings" button to `MainWindow.xaml`.
- Add `ShowSystemSettingsMenuCommand` and `NavigateToTestDataCommand` to `MainViewModel`.
- Create `SystemSettingsMenuViewModel` / `SystemSettingsMenuView`.
- Register the new VMs and windows in DI / `NavigationService`.
- Verify clicking "System Settings" shows the menu and clicking "Test Data" opens a placeholder window while `MainWindow` is hidden, then re-shows it on close.

### Phase 4 — Test Data window (Zone A + Zone B/C/D/E)

- Build `TestListViewModel`, `TestDetailViewModel`, and the thin `TestDataManagementViewModel` coordinator.
- Build `TestDataManagementWindow.xaml`.
- Implement the three search modes and real-time filtering.
- Implement all field bindings: Zones B (groups 1–7), C (tubes grid), D (outside section), E (patient question).
- Implement Zone C add/edit/delete using a small modal dialog for the tube row, or inline grid editing — either is acceptable.
- Implement validation messages via a small custom modal (match `TodayPatientsDialog` convention, not `MessageBox.Show`).
- Wire `OpenNormalRangesCommand` to a stub (window opens empty).

### Phase 5 — Normal Ranges window

- Build `NormalRangeListViewModel`, `NormalRangeDetailViewModel`, `NormalRangeWindowViewModel`.
- Build `NormalRangesWindow.xaml` with the two-panel layout described in §8.3.
- Implement the auto-create-default-component behaviour when a test has none.
- Implement validation (Low ≤ High for normal and critical pairs).
- Verify Save All persists components and ranges via the service.

### Phase 6 — Receipt grouping

- Add `BillNameLine1` / `BillNameLine2` to `SelectedTestDto`.
- Update `ReceiptDialogViewModel.LoadVisit()` and `CreateDetailedDocument()` per §9.
- Manual verification against a visit that includes (a) tests with no `BillNameLine1`, (b) two tests sharing a `BillNameLine1`, (c) one test with a `BillNameLine2`.

### Phase 7 — End-to-end verification

See §12.

---

## 12. Verification plan

End-to-end manual test once all phases are complete:

1. **Build**: `dotnet build FinalLabSystem.sln` — clean, no new warnings.
2. **Migration**: `dotnet ef database update` on a dev DB. Confirm via SQL:
   - `SELECT TOP 1 ReportNameLine1, BillNameLine1, IsRoutineTest, IsSendOutside, OutsideLabName, PatientQuestion FROM TestType;`
   - `SELECT TOP 1 * FROM TestTypeSampleTube;` (initially empty)
   - `SELECT * FROM PriceScheme WHERE SchemeCode IN ('PATIENT','LAB2LAB');`
3. **Launch the app**. Login → MainWindow.
4. **Toolbar**: confirm "إعدادات النظام" sits next to "المرضى" with consistent styling.
5. **Menu**: click it; the central menu shows "بيانات التحاليل".
6. **Open Test Data window**: click it. `MainWindow` hides; the new window opens with the test list populated.
7. **Search**: try each mode (Code exact, Name `StartsWith`, Group `Contains`). Filtering is live.
8. **Select a test**: Zone B populates with every field including new ones. Tubes grid (Zone C) shows configured requirements. Zone D shows outside-lab config. Zone E shows patient question.
9. **Edit and save**: change fields including new ones, toggle flags, set `IsSendOutside = true` and fill outside fields, click Save, reopen — values persist.
10. **Sample tubes**: add two distinct tube requirements for a test (EDTA + Citrate). Save. Reopen — both rows are present.
11. **Add a new test**: New → fill in required fields → Save → it appears in Zone A.
12. **Normal Values window**: select a test, click "Normal Values". The separate window opens. If the test has no components, one is auto-created. Add a range (sex=F, age 0–18250 days, low=4, high=12). Save All. Close. Reopen — range present.
13. **Validation**: try saving with `IsSendOutside = true` and empty `OutsideCostPrice` → validation error. Try saving a range with `LowNormal > HighNormal` → validation error.
14. **Close**: `MainWindow` re-appears automatically.
15. **Receipt grouping**: in Patient Registration, create a visit with three tests where two share `BillNameLine1 = "Lipid Profile"`. Click Receipt. Confirm one "Lipid Profile" row whose price is the sum of the two, plus a separate row for the third test. Grand total unchanged.
16. **Regression**: open a visit whose tests have no `BillNameLine1`. Receipt is identical to pre-change behaviour.

---

## 13. Critical files touched (summary)

| File | Action |
|---|---|
| `FinalLabSystem/Models/TestType.cs` | add 14 properties + tubes navigation collection |
| `FinalLabSystem/Models/TestTypeSampleTube.cs` | **new** |
| `FinalLabSystem/Data/FinalLabDbContext.cs` | add `DbSet`, configure new columns + new entity |
| `FinalLabSystem/Migrations/<timestamp>_AddTestDataManagementFields.cs` | **new** migration + seed |
| `FinalLabSystem/Services/Interfaces/ITestCatalogService.cs` | add CRUD methods |
| `FinalLabSystem/Services/Implementations/TestCatalogService.cs` | implement CRUD |
| `FinalLabSystem/MainWindow.xaml` | add "System Settings" button |
| `FinalLabSystem/ViewModels/MainViewModel.cs` | add menu + navigation commands |
| `FinalLabSystem/ViewModels/Settings/SystemSettingsMenuViewModel.cs` | **new** |
| `FinalLabSystem/Views/Settings/SystemSettingsMenuView.xaml` | **new** |
| `FinalLabSystem/Views/Settings/TestDataManagementWindow.xaml` (+ `.cs`) | **new** |
| `FinalLabSystem/ViewModels/Settings/TestDataManagementViewModel.cs` | **new** (thin coordinator) |
| `FinalLabSystem/ViewModels/Settings/TestListViewModel.cs` | **new** |
| `FinalLabSystem/ViewModels/Settings/TestDetailViewModel.cs` | **new** |
| `FinalLabSystem/ViewModels/Settings/TestRowViewModel.cs` | **new** |
| `FinalLabSystem/ViewModels/Settings/TestTypeSampleTubeRowViewModel.cs` | **new** |
| `FinalLabSystem/Views/Settings/NormalRangesWindow.xaml` (+ `.cs`) | **new** |
| `FinalLabSystem/ViewModels/Settings/NormalRangeWindowViewModel.cs` | **new** (coordinator) |
| `FinalLabSystem/ViewModels/Settings/NormalRangeListViewModel.cs` | **new** |
| `FinalLabSystem/ViewModels/Settings/NormalRangeDetailViewModel.cs` | **new** |
| `FinalLabSystem/Models/DTOs/SelectedTestDto.cs` | add 2 properties |
| `FinalLabSystem/ViewModels/Patients/ReceiptDialogViewModel.cs` | grouping in `LoadVisit` + `CreateDetailedDocument` |
| `App.xaml.cs` (DI + window registration) | register new windows & VMs |

No existing tests are deleted; no existing behaviour is removed.

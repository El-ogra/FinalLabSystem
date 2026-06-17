# Work Plan — Barcode Label Printing Feature

> Implementation working document. Read `HANDOFF_BARCODE.md` first for
> context; this file is the executable plan.

---

## Agent Understanding Confirmation

**1. What the feature must do (full workflow).**
The Patient Registration window (`PatientRegistrationWindow`) supports
two scenarios with one UI. **Scenario A:** open the window → click
**إضافة** (Add) → form unlocks → staff enters personal info, required
tests, financial info, referral info, medical history → click **حفظ**
(Save) → data stays visible. Then click **الباركود** (Barcode) → a
popup opens showing one printable label per tube. **Scenario B:** load
an existing patient/visit through search or "قائمة المرضى" → no save
prompt → click **الباركود** → the same popup opens with that visit's
tubes. In the popup, **Print All** prints every label; double-clicking
one label prints that label only.

**2. Tube-grouping logic.**
A label represents a **tube**, not a test. Many distinct tests are
collected into the same tube (e.g., CBC + Blood Type + HbA1c all use
EDTA). Each label therefore lists the abbreviated codes of *every* test
in that tube. 7 tests across 4 tube types → 4 labels.

**3. Two cases for the Barcode button.**
- **Case A:** new patient was just saved — `CurrentVisitId` is set by
  `SaveAsync`.
- **Case B:** an existing patient is loaded — `CurrentVisitId` is set
  by `LoadVisitForEditAsync`.
Both paths populate `CurrentVisitId`, so the button's existing
`CanExecute = CurrentVisitId > 0` gate already covers both. **No
branching by case is required.**

**4. What I found in the codebase that is relevant.**
- The Barcode button, command, dialog, dialog VM, and service methods
  **already exist** but the dialog renders a plain `DataGrid` (not
  labels), the print path uses `FlowDocument` text (not a label
  printer), and `BarcodeValue` is a `Guid`-based string (not a patient
  ID).
- `ISampleTrackingService.GetTubesForVisitAsync` already eager-loads
  `Visit → Patient` and `VisitTests → Testtype` — enough to project
  labels from.
- `GenerateBarcodesForVisitAsync` groups by `(DefaultTubeType,
  DefaultTubeColor)` (both nullable strings) — will throw on NULL.
- **Confirmed by reading the test-management UI:** these legacy
  string fields are **not bound to any control** in
  `TestDataManagementWindow.xaml` and `TestDetailViewModel.BuildEntity()`
  does not assign them. For any test created or fully re-edited
  through that UI, `DefaultTubeType`, `DefaultTubeColor`, and
  `SampleType` will be NULL — so today's grouping logic is broken
  in practice, not just theoretically.
- The actual user-supplied tube data lives in
  `TestType.TestTypeSampleTubes` rows — populated by
  `TestDetailViewModel.BuildTubes()` from the three "Tube 1 / Tube 2
  / Tube 3" comboboxes. Each row's `SampleType` carries the picked
  value (Serum / Plasma / EDTA Blood / Citrate Blood / Urine / CSF /
  Other); `TubeType` is hardcoded `"Default"`, `SortOrder` is 1/2/3.
- The "Collection" combobox in the same UI is bound to
  `CollectionTypeId` — a higher-level category lookup
  (e.g., "Blood" vs "Urine"), distinct from the per-tube identity.
- `PatientInfoView` has no `IsEnabled` binding → form is always
  editable (contradicts the spec's "locked until Add").
- `SaveAsync` does not clear fields → matches spec.
- `LoadVisitForEditAsync` doesn't prompt for save → matches spec.
- No barcode-rendering library is referenced anywhere.

**5. What is missing and must be built.**
- A label-style XAML in `BarcodeDialog` (Arabic name, sex+age, tests
  list, barcode image, patient-id + tube footer).
- A `BarcodeLabel` view-model projection inside `BarcodeDialogViewModel`.
- A barcode-image renderer (library + WPF control or
  `IValueConverter`).
- A NULL-safe and deterministic `GenerateBarcodesForVisitAsync`.
- A real print path (label printer or correctly-sized paper printer)
  on `IPrintService` — replacing `NullPrintService` for labels.
- Double-click-to-print wiring.
- Form-locking behaviour for the registration window on first open
  (and possibly after save).

---

## Section A — Current State Summary

### A1 — Patient Registration Window

| Q | Answer |
|---|---|
| Q1 — Does a Barcode button exist? Label? Binding? | **Yes.** `PatientRegistrationWindow.xaml:94`. Content: `الباركود`. `Command="{Binding BarcodeCommand}"`. |
| Q2 — What does the bound command do? | `PatientRegistrationViewModel.BarcodeAsync` (lines 304-315): calls `_sampleTrackingService.GenerateBarcodesForVisitAsync(CurrentVisitId, staffId)`, resolves `BarcodeDialogViewModel` from DI, calls `LoadTubesAsync(CurrentVisitId)`, opens `BarcodeDialog` modally. |
| Q3 — Does an Add (إضافة) button exist? What does it do? | **Yes.** Line 88, content `إضافة`, bound to `AddNewCommand` → `AddNewAsync()` (lines 166-180): clears all child VMs, resets dates, generates a new patient code, sets `IsEditMode = false`, `HasUnsavedChanges = false`. **It does NOT toggle any IsEnabled / IsReadOnly state** — the form fields are not actually locked or unlocked because no such state exists. |
| Q4 — After Save, does data clear? | **No clear.** `SaveAsync` (lines 182-261) writes `CurrentPatientId`/`CurrentVisitId` from the saved entity, sets `IsEditMode = true`, `HasUnsavedChanges = false`, shows a dialog "تم حفظ بيانات المريض والزيارة." Field-bound properties on child VMs are untouched. ✓ Matches spec. |
| Q5 — How does the VM access current visit + selected tests? | `CurrentVisitId` (int, private setter), `CurrentPatientId` (int, private setter). Selected tests live in `TestSelection.SelectedTests` (`ObservableCollection<SelectedTestItem>`), retrieved as IDs via `TestSelection.GetSelectedTestTypeIds()`. |
| Q6 — How is an existing patient loaded? Is there a distinction between new-saved and existing-loaded? | Loaded via `LoadVisitForEditAsync(int visitId)` (lines 275-289), invoked from `EditAsync()` after the user picks a patient in `TodayPatientsDialog`. The method calls `_visitService.GetVisitFullDataAsync`, populates all child VMs, sets `IsEditMode = true`, `HasUnsavedChanges = false`. **There is no explicit "I just saved" vs "I just loaded" distinction** — both paths converge on the same end-state (`CurrentVisitId > 0`, `IsEditMode = true`). The Barcode button's `CanExecute = CurrentVisitId > 0` covers both. |

### A2 — BarcodeDialog.xaml

The file **exists** at `Views/Patients/BarcodeDialog.xaml`. It contains:

- A `Window` 420×620, RightToLeft, title `طباعة الباركود`.
- A `DataGrid` bound to `Tubes` / `SelectedTube` with three text
  columns: `BarcodeValue`, `TubeType` (`نوع الأنبوب`), `TubeColor`
  (`لون الأنبوب`).
- A `DataGridTemplateColumn` per row containing a `Button` "طباعة"
  bound to `PrintBarcodeCommand` with `CommandParameter="{Binding}"`.
- A footer `Button` "طباعة الكل" bound to `PrintAllCommand`.

It does NOT contain: patient name, sex/age, comma-joined test codes, a
barcode image, the patient-id+tube footer line, or a double-click
trigger.

### A3 — Existing Barcode Infrastructure

| File | Purpose |
|---|---|
| `Views/Patients/BarcodeDialog.xaml` | Popup with the bare `DataGrid` described above. |
| `Views/Patients/BarcodeDialog.xaml.cs` | Standard `DataContext = viewModel` wiring. No logic. |
| `ViewModels/Patients/BarcodeDialogViewModel.cs` | Holds `Tubes` (`ObservableCollection<SampleTube>`), `SelectedTube`, `PrintBarcodeCommand`, `PrintAllCommand`. `LoadTubesAsync(visitId)` calls `_sampleTrackingService.GetTubesForVisitAsync`. Printing goes through `PrintTubes(...)` which builds a `FlowDocument` of text paragraphs and shows a `PrintDialog`. Contains an unused `BuildZplStub` method. |
| `Services/Interfaces/ISampleTrackingService.cs` | `GenerateBarcodesForVisitAsync(visitId, staffId)`, `GetTubesForVisitAsync(visitId)`, `UpdateTestStageAsync`. |
| `Services/Implementations/SampleTrackingService.cs` | `GenerateBarcodesForVisitAsync` groups VisitTests by `(DefaultTubeType, DefaultTubeColor)` and creates one `SampleTube` per group. Assigns each `VisitTest.Tube`. Writes `BarcodeValue = TUBE-{visitId}-{Guid:N}`, `PrintedAt = UtcNow`, `PrintedBy = staffId`. `GetTubesForVisitAsync` eager-loads `Visit → Patient`, `VisitTests → Testtype`. |
| `Models/SampleTube.cs` | `TubeId`, `VisitId`, `TubeType` (NOT NULL), `TubeColor`, `BarcodeValue`, `CollectedAt`, `PrintedAt`, navigation to `Visit`, `VisitTests`. |
| `Models/VSampleTubeStatus.cs` | Keyless view-mapped DTO `V_SampleTubeStatus`. Not consumed by the dialog. |
| `Models/TestTypeSampleTube.cs` | Many-to-many link allowing a test to require multiple tubes (separate from the legacy single `DefaultTubeType`). Not consumed. |
| `Services/Interfaces/IPrintService.cs` + `NullPrintService.cs` | Generic `PrintAsync(string documentType, object data)` — only a no-op impl. Registered Singleton in `App.xaml.cs:147`. |

### A4 — Tube/Collection Type Model

**Three overlapping representations exist on `TestType`:**

```
VisitTest.TesttypeId ── FK ──> TestType.TesttypeId

TestType.DefaultTubeType  : string?  (legacy, NOT BOUND IN UI)
TestType.DefaultTubeColor : string?  (legacy, NOT BOUND IN UI)
TestType.SampleType       : string?  (legacy, NOT BOUND IN UI)

TestType.CollectionTypeId : int?     -> CollectionType.CollectionTypeId
                                       (TypeNameEn, TypeNameAr, SortOrder)
                                       — UI-bound; semantic category
                                       (e.g., "Blood" / "Urine"),
                                       not tube identity.

TestType.TestTypeSampleTubes : ICollection<TestTypeSampleTube>
                               (TestTypeTubeId, TestTypeId, TubeType,
                                TubeColor, SampleType, Quantity,
                                SortOrder, IsActive, Notes)
                               — UI-bound, 1..3 rows per test,
                               populated from "Tube 1/2/3" combos.

VisitTest.TubeId : int? -> SampleTube.TubeId  (one tube per visit-test;
                                               see new gap G-11)
SampleTube.TubeType  : string  (NOT NULL after generation)
SampleTube.TubeColor : string?
SampleTube.BarcodeValue : string  (UNIQUE)
```

**What the test-management UI actually fills (verified):**

- `TestDataManagementWindow.xaml` (lines 211-217) — the "Collection"
  combobox writes to `TestType.CollectionTypeId`. **This is a category
  lookup, not the tube identity.**
- `TestDataManagementWindow.xaml` (lines 247-265) — three comboboxes
  "Tube 1 / Tube 2 / Tube 3" inside the "Barcode" section bind to
  `TestDetail.Tube1 / Tube2 / Tube3`. The combos' items come from
  `TestDetailViewModel.AvailableTubeTypes` (a **hardcoded list**:
  `Serum, Plasma, EDTA Blood, Citrate Blood, Urine, CSF, Other`).
- `TestDetailViewModel.BuildTubes()` (lines 372-387) translates those
  three strings into `TestTypeSampleTube` rows where:
  - `SampleType` = the picked combo value (this is the **tube identity**).
  - `TubeType` = hardcoded `"Default"` (placeholder; **does not carry
    semantic value** in this UI).
  - `SortOrder` = 1, 2, 3 in the order tubes are filled.
  - `Quantity` = 1.
- `TestDetailViewModel.BuildEntity()` (line 359) writes
  `CollectionTypeId`. **It does NOT touch `DefaultTubeType`,
  `DefaultTubeColor`, or `SampleType`** — those legacy fields stay
  NULL on every fresh insert. `UpdateTestTypeAsync`
  (`TestCatalogService.cs` lines 196-197) carries forward whatever
  value was already there, so legacy data may persist, but no UI path
  populates these fields.

**Conclusion (resolves Q-01):**

The user-facing tube identity is **`TestTypeSampleTube.SampleType`**
(the string picked in "Tube 1/2/3"). `CollectionType` is an
orthogonal category for reporting. The legacy three string fields
on `TestType` are effectively dead UI-wise.

**The full chain that the new implementation must consume:**

```
VisitTest → Testtype (TestType) → TestTypeSampleTubes (rows)
                                          │
                                          └─ Order by SortOrder
                                          └─ SampleType = grouping key
                                                          + label text
```

The default-tube row (`SortOrder = 1`) drives the canonical tube for
that test. Tests sharing the same `SampleType` on their primary tube
row are grouped onto one label.

**Multi-tube tests** (a test that fills Tube 2 or Tube 3 too) produce
multiple `TestTypeSampleTube` rows — each must become its own label.
The current `VisitTest.TubeId` schema permits **one tube per
visit-test only**, which is a new gap (see G-11).

EF configuration (`Data/FinalLabDbContext.cs`):
- `SampleTube`: PK `TubeId`, unique index on `BarcodeValue`, FK to
  `Visit` (`ClientSetNull`), FKs to `Staff` for `CollectedBy` /
  `PrintedBy`.
- `TestType.CollectionType`: `HasOne(d => d.CollectionType).WithMany(p
  => p.TestTypes).HasForeignKey(d => d.CollectionTypeId)
  .OnDelete(Restrict)`.
- `TestTypeSampleTube`: PK `TestTypeTubeId`, FK to `TestType` cascade.

### A5 — Visit/Test Retrieval

- `IVisitService.GetVisitFullDataAsync(int visitId)` returns
  `VisitFullDto`. The query (`VisitService.cs:225-233`) includes
  `Patient`, `Referral`, `Payments`, `VisitTests → Testtype`.
  **Does NOT include tube / collection-type data.** `VisitFullDto.
  SelectedTests` carries `TestTypeId`, `TestCode`, `TestName`, `Price`,
  `SampleType` only.
- `ISampleTrackingService.GetTubesForVisitAsync(int visitId)` returns
  `List<SampleTube>` with `Visit → Patient` and `VisitTests → Testtype`
  included. This is the method the BarcodeDialog uses today. ✓
  Sufficient for label projection **as long as
  `GenerateBarcodesForVisitAsync` succeeds first**.
- Current-visit concept in `PatientRegistrationViewModel`: a single
  `CurrentVisitId` (private setter) plus `Financial.SetCurrentVisitId
  (...)` notification. No multi-visit collection.

### A6 — Print Infrastructure

- `IPrintService.PrintAsync(string documentType, object data)` —
  single generic method. No label-printing surface.
- `NullPrintService` — `Task.CompletedTask`; commented `// TODO:
  replace with real print/PDF implementation`.
- Registration: `services.AddSingleton<IPrintService,
  NullPrintService>();` at `App.xaml.cs:147`.
- No `ZplPrint` / `EscPos` / `LabelPrinter` files exist anywhere.
- No barcode-rendering library (ZXing.Net, BarcodeLib, etc.) is in
  the project — verified by grep across the solution.

For real label printing this layer needs either:
- A label-specific method on `IPrintService` (e.g. `PrintLabelsAsync
  (IEnumerable<BarcodeLabel>)`), and
- A concrete `LabelPrintService` (Windows printer + sized
  `FlowDocument` OR a ZPL/EPL transport).

### A7 — Dialog / Navigation Pattern

- `NavigationService` is for **task windows** (Main → child window
  swap), not dialogs.
- Dialogs follow this hand-rolled pattern, used by `BarcodeDialog`,
  `ReceiptDialog`, `TodayPatientsDialog`:

```csharp
var viewModel = App.ServiceProvider.GetRequiredService<XDialogViewModel>();
// (optional) await viewModel.LoadXAsync(...);
var dialog = new XDialog(viewModel)
{
    Owner = Application.Current.Windows
        .OfType<Window>()
        .FirstOrDefault(window => window.IsActive)
};
dialog.ShowDialog();
```

There is no shared helper or base class — each VM does this inline.
The new dialog must follow the same shape (the existing code in
`BarcodeAsync` already does).

---

## Section B — Gap Analysis

| Gap ID | Description | Affected Layer | Complexity |
|---|---|---|---|
| **G-01** | `BarcodeDialog.xaml` shows a `DataGrid` of tube rows. Needs a label-style `ItemsControl` with a per-label `DataTemplate` showing Arabic name, sex+age, comma-joined codes, barcode image, footer `PatientId / TubeName`. | View | **M** |
| **G-02** | `BarcodeDialogViewModel` exposes raw `SampleTube` rows. Needs a `BarcodeLabel` projection (record) with `PatientNameAr`, `SexAgeLine`, `TestCodesLine`, `BarcodePayload`, `PatientIdentifierLine`, `TubeName`. | VM | **S** |
| **G-03** | No barcode-image rendering. Need a library (recommend ZXing.Net) and either a WPF control or an `IValueConverter` that emits a `BitmapSource` from a string. | Infra / View | **M** |
| **G-04** | `SampleTrackingService.GenerateBarcodesForVisitAsync` groups by `(TestType.DefaultTubeType, DefaultTubeColor)` — fields that the test-management UI **never populates**. Every visit using tests created through the current UI will crash on `new SampleTube { TubeType = null! }`. Must be rewritten to group by `TestTypeSampleTube.SampleType` (with `SortOrder = 1` row as the primary tube). Also `BarcodeValue` is `TUBE-{visitId}-{Guid:N}` — doesn't match the spec's "encoding the Patient ID". Need a deterministic, scannable value (depends on Q-02). | Service | **M** |
| **G-05** | `IPrintService.PrintAsync` is generic and the only impl is `NullPrintService`. No real label-printing path. Need either an `ILabelPrintService` (or a label overload) + a concrete `LabelPrintService`. | Service / DI | **M** (depends on Q-03 — could be **L** for ZPL). |
| **G-06** | No double-click handler on the per-label item. Need a `MouseBinding`/`InputBinding` in the `DataTemplate` (or a code-behind `MouseDoubleClick` that calls a VM command). | View / VM | **XS** |
| **G-07** | Form is not locked on first open. Need an `IsFormUnlocked` state on `PatientRegistrationViewModel`, bind `IsEnabled` (or `IsReadOnly` on TextBoxes) on each child view, set `false` in constructor, `true` in `AddNewAsync` and `LoadVisitForEditAsync`. Re-lock behaviour after Save depends on Q-05. | VM / View | **M** |
| **G-08** | ~~Three competing tube-source representations on `TestType`.~~ **RESOLVED by Q-01 investigation:** the canonical source is `TestTypeSampleTube.SampleType`; `CollectionType` is a category lookup; the three legacy strings on `TestType` are dead in the UI. The `TubeResolver` (Phase 1) therefore has a single concrete rule (no multi-source fallbacks needed). Defensive fallback for legacy NULL rows: when a `TestType` has zero `TestTypeSampleTubes`, fall back to `CollectionType.TypeNameEn ?? "Unknown"` so the dialog still opens for legacy/imported data. | Service / Model | **S** |
| **G-09** | `SelectedTestDto.BillNameLine1` is declared but never populated by `VisitService.GetVisitFullDataAsync`. Cosmetic — flagged for future cleanup, not blocking. | Service | **XS** |
| **G-10** | `GenerateBarcodesForVisitAsync` is invoked even when tubes already exist for the visit (re-entry case). It guards via an early return — verify this idempotency stays correct after Q-02 changes the value scheme (don't regenerate over existing rows). | Service | **XS** |
| **G-11** | A test can have up to **3 tubes** (`Tube 1/2/3` in the UI). But `VisitTest.TubeId` is a single nullable FK — a `VisitTest` can be linked to **only one** `SampleTube`. Therefore today a multi-tube test (e.g., Urine + EDTA Blood) cannot be fully tracked. Either (a) accept that only the primary tube row (`SortOrder = 1`) drives barcoding for now and surface this as a known limitation, or (b) introduce a `VisitTestTube` link table so a `VisitTest` can reference N tubes (DB migration). **Recommend (a)** for v1 and capture (b) as future work. | Model / Schema | **L** (only if (b) is chosen) |

---

## Section C — Implementation Plan

Each phase ends with a passing build. Phases are sequenced so a later
phase can reference an earlier phase's types.

### Phase 1 — Add a `TubeResolver` helper (canonical source = `TestTypeSampleTube`)

**Goal.** Encapsulate the rule resolved by Q-01 investigation so the
rest of the codebase never has to look at three competing fields:
the **primary tube** for a `TestType` is the `TestTypeSampleTube` row
with the lowest `SortOrder` (i.e., the value picked in "Tube 1" of the
test-management UI). Defensive fallback: when no rows exist (legacy
data only), use `CollectionType.TypeNameEn` or `"Unknown"`.

**Files to CREATE.**
- `Services/Implementations/TubeResolver.cs` — internal static helper
  exposing two methods:
  - `string ResolvePrimaryTubeIdentity(TestType test)` →
    returns the `SampleType` of the lowest-`SortOrder`
    `TestTypeSampleTube`, falling back to `CollectionType.TypeNameEn`,
    then `"Unknown"`. Used as the **grouping key**.
  - `IReadOnlyList<string> ResolveAllTubes(TestType test)` →
    returns every `TestTypeSampleTube.SampleType` ordered by
    `SortOrder` (forward-compatible with G-11 future work, but the
    consumer in Phase 2 takes only the first element for v1).

**Files to MODIFY.** None yet (resolver is created in isolation).

**DB Migration.** No.
**Complexity.** XS.
**Depends on.** Nothing (Q-01 is resolved).

---

### Phase 2 — Rewrite `SampleTrackingService.GenerateBarcodesForVisitAsync`

**Goal.** Replace the broken `(DefaultTubeType, DefaultTubeColor)`
grouping (which crashes on every UI-created test because those fields
are never bound) with a `TubeResolver`-driven grouping; produce a
`BarcodeValue` that matches Q-02's choice; preserve idempotency.

**Files to CREATE.** None.

**Files to MODIFY.**
- `Services/Implementations/SampleTrackingService.cs`
  - `GenerateBarcodesForVisitAsync`:
    - Include `Patient` (so the new barcode payload can use patient
      code) and `Testtype → TestTypeSampleTubes` (so the resolver has
      its data) and `Testtype → CollectionType` (for the legacy
      fallback).
    - For each `VisitTest`, compute the primary tube identity via
      `TubeResolver.ResolvePrimaryTubeIdentity(vt.Testtype)`.
    - Group by that identity. Each group becomes one `SampleTube`.
    - Set `SampleTube.TubeType` to the grouping key (NOT NULL by
      construction now).
    - Set `TubeColor` to NULL for v1 (the UI no longer captures it;
      revisit when/if color is needed).
    - Compute `BarcodeValue` per Q-02 (currently
      `TUBE-{visitId}-{Guid:N}`; expected replacement is something
      like `{PatientCode}-{TubeOrdinal:00}` — finalize when Q-02 is
      answered).
    - Assign each `VisitTest.Tube = tube` for VisitTests in the group
      (single-tube v1 — see G-11).
  - Keep the existing-tubes early return for idempotency.

**DB Migration.** No.
**Complexity.** S.
**Depends on.** Phase 1, Q-02 answer.

---

### Phase 3 — Add a barcode-rendering library + WPF helper

**Goal.** Render a Code128 (or chosen symbology) image from a string.

**Files to CREATE.**
- `FinalLabSystem.csproj` — package reference to `ZXing.Net` (or the
  team's preferred lib; only one).
- `Infrastructure/Barcoding/BarcodeImageConverter.cs` — an
  `IValueConverter` taking `string` → `BitmapSource`. Lives in
  Infrastructure so it can be reused by future receipts/reports.
- `Infrastructure/Barcoding/BarcodeFormatOptions.cs` (optional) —
  central constants for height/width/symbology so the printed labels
  and the on-screen previews match.

**Files to MODIFY.** None yet.

**DB Migration.** No.
**Complexity.** S–M (mostly research + small wrapper).
**Depends on.** Nothing.

---

### Phase 4 — Build the `BarcodeLabel` projection in the dialog VM

**Goal.** Stop binding raw `SampleTube` rows. Expose a `BarcodeLabel`
record with everything the label needs, pre-computed once on load.

**Files to CREATE.** None.

**Files to MODIFY.**
- `ViewModels/Patients/BarcodeDialogViewModel.cs`
  - Add `public sealed record BarcodeLabel(string PatientNameAr,
    string SexAgeLine, string TestCodesLine, string BarcodePayload,
    string PatientIdentifierLine, string TubeName, SampleTube
    SourceTube);` (kept in the same file or split — your call).
  - Replace `Tubes` with `Labels` (or keep both;
    cleaner to replace).
  - In `LoadTubesAsync`, project each `SampleTube` into a
    `BarcodeLabel` using the helper rules below.
  - Update `PrintBarcodeCommand` / `PrintAllCommand` to take
    `BarcodeLabel` and forward to `IPrintService`.

**Label projection rules (locked in by spec).**
- `PatientNameAr` = `tube.Visit.Patient.FullNameAr`.
- `SexAgeLine`:
  - `"Male"` / `"Female"` / `""` based on `Sex` (`M`/`F`/`U`).
  - Append `" - {ApproxAge} {ApproxAgeUnit}"` (with unit pluralization
    decided by Q-06).
- `TestCodesLine` = `string.Join(", ",
  tube.VisitTests.Select(vt => vt.Testtype.TypeAbbrev ??
  vt.Testtype.TypeCode).Where(NotEmpty))`. (Final field choice
  depends on Q-07.)
- `BarcodePayload` = `tube.BarcodeValue` (after Phase 2's value
  scheme is in place).
- `PatientIdentifierLine` = `tube.Visit.Patient.PatientCode`.
- `TubeName` = `tube.TubeType` — after Phase 2 this is the
  `TestTypeSampleTube.SampleType` value picked by the user (e.g.,
  `Serum`, `EDTA Blood`). Matches the spec example footer ("Serum").

**DB Migration.** No.
**Complexity.** S.
**Depends on.** Phase 2.

---

### Phase 5 — Redesign `BarcodeDialog.xaml` as a label list

**Goal.** Replace the `DataGrid` with an `ItemsControl` of label
tiles that visually match the spec.

**Files to CREATE.** None.

**Files to MODIFY.**
- `Views/Patients/BarcodeDialog.xaml` — new layout:
  - Header: dialog title.
  - Body: `ItemsControl` `ItemsSource="{Binding Labels}"` with a
    `WrapPanel` `ItemsPanelTemplate`.
  - `ItemTemplate`: a `Border` (label-sized, ~300×120 px on screen,
    actual print size from Q-03) containing a vertical `StackPanel`:
    1. `TextBlock PatientNameAr` (right-aligned, Arabic font, bold).
    2. `TextBlock SexAgeLine`.
    3. `TextBlock TestCodesLine`.
    4. `Image Source="{Binding BarcodePayload, Converter=
       {StaticResource BarcodeImageConverter}}"`.
    5. `TextBlock` with `PatientIdentifierLine` + `TubeName`.
    - `InputBindings`: `MouseBinding Gesture="MouseDoubleClick"
      Command="{Binding DataContext.PrintBarcodeCommand,
      RelativeSource={RelativeSource AncestorType=Window}}"
      CommandParameter="{Binding}"`.
  - Footer: a "طباعة الكل" button bound to `PrintAllCommand`.
  - Register `BarcodeImageConverter` in `Window.Resources`.

**Files to MODIFY (cosmetic).**
- `Views/Patients/BarcodeDialog.xaml.cs` — leave as-is (no
  code-behind logic per MVVM).

**DB Migration.** No.
**Complexity.** M.
**Depends on.** Phase 3, Phase 4.

---

### Phase 6 — Real print integration

**Goal.** Replace the `FlowDocument`-of-text print path with a real
label printer call. Two paths depending on Q-03; the plan below
assumes "Windows PrintDialog on a sized FlowDocument" (most common
fallback). If ZPL is chosen, add a `ZebraZplPrintService` + a USB/IP
transport (this turns Phase 6 into **L**).

**Files to CREATE.**
- `Services/Interfaces/ILabelPrintService.cs` — `Task PrintLabelsAsync
  (IEnumerable<BarcodeLabel> labels);` (or fold into `IPrintService`
  via a `LabelDocumentType` constant; pick the less invasive route).
- `Services/Implementations/WpfLabelPrintService.cs` — builds a
  `FixedDocument` / `FlowDocument` with `PageWidth`/`PageHeight` set
  from Q-03 mm dimensions, draws each label as a `Grid` (same layout
  as the on-screen template), prints via `PrintDialog`.
  *(If Q-03 is ZPL: instead create `ZebraZplPrintService` that emits
  ZPL strings and writes to a `RawPrinterHelper` (PInvoke) or a TCP
  socket.)*

**Files to MODIFY.**
- `App.xaml.cs`
  - Replace `services.AddSingleton<IPrintService, NullPrintService>();`
    with the chosen label print service registration (keep
    `NullPrintService` for the generic `IPrintService` if other
    document types still need it).
- `ViewModels/Patients/BarcodeDialogViewModel.cs`
  - Inject `ILabelPrintService` (or new label method on `IPrintService`).
  - Remove the inline `FlowDocument` building; remove the unused
    `BuildZplStub`.
  - `PrintTube` → `await _labelPrintService.PrintLabelsAsync(new[]
    { label });`
  - `PrintAll` → `await _labelPrintService.PrintLabelsAsync
    (Labels);`

**DB Migration.** No.
**Complexity.** M (or L for ZPL).
**Depends on.** Phase 4, Phase 5, Q-03 + Q-08.

---

### Phase 7 — Wire double-click and confirm Barcode-button flow

**Goal.** Verify the end-to-end click path works for both Case A and
Case B and that double-click triggers single-label print.

**Files to CREATE.** None.

**Files to MODIFY.**
- `Views/Patients/BarcodeDialog.xaml` — already done in Phase 5
  (`MouseBinding`). Double-check `CommandParameter` resolves to the
  item under the cursor.
- `ViewModels/Patients/PatientRegistrationViewModel.cs` — no change
  required: `BarcodeAsync` already supports both cases via the
  `CurrentVisitId > 0` gate. Verify only — do **not** edit speculatively.

**DB Migration.** No.
**Complexity.** XS.
**Depends on.** Phase 5.

---

### Phase 8 — Form-locking behaviour (G-07)

**Goal.** Make the spec's "fields are LOCKED until Add" literally
true.

**Files to CREATE.** None.

**Files to MODIFY.**
- `ViewModels/Patients/PatientRegistrationViewModel.cs`
  - Add `bool IsFormUnlocked { get; private set; }` (initial `false`).
  - Set `true` at the end of `AddNewAsync()` and at the end of
    `LoadVisitForEditAsync()`.
  - Decision after `SaveAsync`: leave `IsFormUnlocked = true` (so the
    user can correct a field they spotted), or set `false` (display-
    only until next Add/Edit). **Both behaviours match the spec
    literally; pick per Q-05.**
- `Views/Patients/PatientInfoView.xaml`
  - Add `IsEnabled="{Binding DataContext.IsFormUnlocked,
    RelativeSource={RelativeSource AncestorType=Window}}"` on the
    root `Grid` (covers every child without per-control edits).
- `Views/Patients/ReferralSectionView.xaml`,
  `MedicalHistorySectionView.xaml`,
  `FinancialSectionView.xaml`,
  `TestSelectionView.xaml` — same root-level binding.

**DB Migration.** No.
**Complexity.** M (mostly XAML touch-ups).
**Depends on.** Q-05 answer.

---

### Phase 9 — End-to-end manual verification (no code)

Walk the verification script in `HANDOFF_BARCODE.md` §9. If anything
diverges from the spec, file a follow-up issue rather than patching
silently.

---

## Section D — Resolved Decisions

All questions are closed. No further user input is required before
implementation. Each row below is the final, authoritative answer
that every phase MUST follow.

| # | Decision area | Resolved value |
|---|---|---|
| **Q-01** | Canonical tube source | **`TestTypeSampleTube.SampleType`** — the row with the lowest `SortOrder` is the primary tube for v1. `CollectionType` is a higher-level category only. The legacy `TestType.DefaultTubeType` / `DefaultTubeColor` / `SampleType` fields are dead (never written by the UI) — **DO NOT read or write them.** Defensive fallback only when a `TestType` has zero `TestTypeSampleTubes` rows: use `CollectionType.TypeNameEn`, then `"Unknown"`. |
| **Q-02** | Barcode payload + label footer | The barcode image encodes **`PatientCode`** (the formatted string, e.g. `P20260617001`). **Not** the integer `PatientId`. Label footer text is exactly `"{PatientCode}   {TubeName}"` (three spaces between, matching the spec example). `SampleTube.BarcodeValue` must be regenerated to a deterministic, scannable `PatientCode`-based value (replacing the current `TUBE-{visitId}-{Guid:N}` scheme). |
| **Q-03** | Printer / label medium | **Thermal label printer** connected via a **standard Windows printer driver**. **No ZPL. No ESC/POS.** Physical label size: **38 mm wide × 25 mm tall**. Implementation: WPF `Visual` / `FixedDocument` printing sized to 38×25 mm sent to the Windows print queue. Phase 6 implements **`WpfLabelPrintService` only**. `ZebraZplPrintService` is **not** to be built. |
| **Q-04** | Print scope | **Current visit's tubes only.** Do not show tubes from any other visit for this patient. |
| **Q-05** | Form lock behaviour | Window opens **LOCKED** (not editable). Clicking **إضافة** (Add) **unlocks** the form. Clicking **حفظ** (Save) leaves the form **unlocked** (still editable). Loading an existing patient leaves the form **unlocked**. The form re-locks **only** when **إضافة** is clicked again to begin a new entry. Implementation: `IsFormUnlocked` starts `false`; set to `true` at the end of `AddNewAsync` and `LoadVisitForEditAsync`; **not changed** by `SaveAsync`. |
| **Q-06** | Age unit on label | Print the **actual** `ApproxAgeUnit` value verbatim — never normalize. Examples: `"Male - 35 Years"`, `"Female - 8 Months"`, `"Male - 12 Days"`. |
| **Q-07** | Abbreviation field on label | Use **`TypeCode`** as the primary abbreviation. Fallback chain: **`TypeCode` → `TypeAbbrev` → `TypeNameEn`** (use the first non-empty value in that order). Always read live from the database at print time. **Do NOT snapshot `TypeCode` onto `SampleTube`.** |
| **Q-08** | "Print All" behaviour | One **single multi-page print job**. All labels are emitted in one job, one label per page, each page sized **38 mm × 25 mm**. |

---

## Section E — Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| ~~Wrong tube-source choice~~ → Q-01 resolved. Residual risk: a test row exists in DB with **zero** `TestTypeSampleTubes` (legacy import) — the resolver's fallback kicks in but yields `"Unknown"` or a category name, so two unrelated tests end up on the same label. | Low | Medium (one label is wrong, easy to spot at the printer) | Resolver's `"Unknown"` fallback is loud (not silent). Phase 9 verification should include a query that lists `TestType` rows with zero tube rows so the catalog can be cleaned up. |
| Multi-tube test (Tube 2 or Tube 3 filled) cannot be barcoded because `VisitTest.TubeId` is single. (G-11) | Medium | Medium (label missing for the secondary tube) | v1 explicitly takes only the primary tube row (`SortOrder = 1`). Document this limitation in the dialog ("multi-tube tests show only the primary label") and capture the link-table refactor as future work. |
| NULL `DefaultTubeType` already in production data → `GenerateBarcodesForVisitAsync` crashes when Phase 2 runs against real data. | High | Medium (feature unusable until fixed) | Phase 2 is explicitly NULL-safe and no longer reads `DefaultTubeType` at all. Resolver fallback to `"Unknown"` keeps the dialog usable. |
| Chosen barcode-encoding library outputs an image whose width doesn't fit the physical label. | Medium | Medium (truncated barcode, scanners fail) | Centralize sizing constants in `BarcodeFormatOptions`; print a 1:1 PDF preview before going to the device. |
| ~~Printer type mismatch (ZPL vs WPF)~~ — **RESOLVED via Q-03.** Thermal Windows-driver printer confirmed. Phase 6 implements `WpfLabelPrintService` only. No ZPL service will be built. Risk closed. | N/A | N/A | Decision is final — see Section D Q-03. |
| Form-locking (Phase 8) breaks `TestSelectionView` because the user adds tests via interactive lists, not just textboxes — `IsEnabled=false` on the root would block selection. | Medium | Medium (regression in main workflow) | Bind `IsEnabled` at the root of each sub-view but visually verify each sub-view individually; offer a per-view override if needed. |
| Existing `BarcodeValue` rows in DB (from previous runs) violate the new value scheme chosen in Q-02. | Low–Medium | Low (idempotency guard prevents regeneration but stale values remain) | Decide whether to migrate existing rows. If yes, write a one-off data fix; otherwise document that pre-existing tubes keep their old codes. |
| `PrintDialog.ShowDialog()` blocks the UI thread; "Print All" with many labels feels frozen. | Low | Low | Either show a busy indicator or move printing to a background `Task` with a continuation that updates the dialog. |
| Adding a barcode library increases binary size / introduces a license obligation. | Low | Low | Prefer Apache-2.0/MIT-licensed libs (ZXing.Net is Apache-2.0). Document the dependency in `README` / NOTICE. |

---

## File-by-file change summary

| Phase | File | Action |
|---|---|---|
| 1 | `Services/Implementations/TubeResolver.cs` | **CREATE** — primary tube = `TestTypeSampleTubes` lowest-`SortOrder` row's `SampleType`; fallback to `CollectionType.TypeNameEn` then `"Unknown"`. |
| 2 | `Services/Implementations/SampleTrackingService.cs` | **MODIFY** — group via `TubeResolver`, include `Patient` + `Testtype → TestTypeSampleTubes` + `Testtype → CollectionType`, drop the broken `(DefaultTubeType, DefaultTubeColor)` grouping, apply Q-02 `BarcodeValue` scheme. |
| 3 | `FinalLabSystem.csproj` | **MODIFY** (add `ZXing.Net` or equivalent) |
| 3 | `Infrastructure/Barcoding/BarcodeImageConverter.cs` | **CREATE** |
| 3 | `Infrastructure/Barcoding/BarcodeFormatOptions.cs` | **CREATE** (optional) |
| 4 | `ViewModels/Patients/BarcodeDialogViewModel.cs` | **MODIFY** (introduce `BarcodeLabel`, drop `BuildZplStub`, route print through service) |
| 5 | `Views/Patients/BarcodeDialog.xaml` | **MODIFY** (replace DataGrid with label `ItemsControl`; register converter) |
| 6 | `Services/Interfaces/ILabelPrintService.cs` | **CREATE** — `Task PrintLabelsAsync(IEnumerable<BarcodeLabel> labels)`. |
| 6 | `Services/Implementations/WpfLabelPrintService.cs` | **CREATE** — Windows print queue, `FixedDocument` sized to 38 mm × 25 mm per page, one label per page, "Print All" emits a single multi-page job (Q-03 + Q-08). **No ZPL service is to be created.** |
| 6 | `App.xaml.cs` | **MODIFY** — register `ILabelPrintService → WpfLabelPrintService`. |
| 8 | `ViewModels/Patients/PatientRegistrationViewModel.cs` | **MODIFY** (add `IsFormUnlocked`; toggle in `AddNewAsync` / `LoadVisitForEditAsync` / `SaveAsync` per Q-05) |
| 8 | `Views/Patients/PatientInfoView.xaml` | **MODIFY** (`IsEnabled` binding on root) |
| 8 | `Views/Patients/ReferralSectionView.xaml` | **MODIFY** (same) |
| 8 | `Views/Patients/MedicalHistorySectionView.xaml` | **MODIFY** (same) |
| 8 | `Views/Patients/FinancialSectionView.xaml` | **MODIFY** (same) |
| 8 | `Views/Patients/TestSelectionView.xaml` | **MODIFY** (same — verify it doesn't break selection lists) |

**No DB migrations are required** for the planned phases. `SampleTube`
already exists; `BarcodeValue` is a `string` column wide enough for the
chosen encoding.

---

*End of work plan.*

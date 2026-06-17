# HANDOFF — Barcode Label Printing Feature

> Context handoff document. **Read this file first** before touching
> any code, then read `work_plan_BARCODE.md` for the phase-by-phase
> implementation plan. Every design question has been resolved — no
> further user input is required before implementation begins.

---

## 1. Feature Goal

The Patient Registration window (`PatientRegistrationWindow`) is used
for two distinct scenarios that share the same UI:

- **Case A — New patient just saved.** Staff opens the window, clicks
  **إضافة** (Add) to unlock the form, enters personal info + required
  tests + financial info + referral info + medical history, then clicks
  **حفظ** (Save). After save, all fields stay populated and visible;
  nothing clears.
- **Case B — Existing patient displayed.** Staff loads a previously
  saved patient/visit through search or "قائمة المرضى" (today list).
  The form simply shows the data; no save is required.

In **both** cases, clicking **الباركود** (Barcode) opens a popup that
shows one printable label **per tube** (not per test) for the visit:

- **Print All** prints every label in one job.
- **Double-clicking a single label** prints only that one label.

Each label looks like this (right-to-left Arabic text, LTR codes):

```
┌────────────────────────────────────────┐
│ أحمد إبراهيم صالح الزيني               │
│ Male - 35 Years                        │
│ FBG, SGPT, SGOT, Creat                 │
│ ║║║║║║║║║║║║║║║║║║║║║║║║║              │
│ 1016122510012   Serum                  │
└────────────────────────────────────────┘
```

Fields, top to bottom:

1. Patient full name in Arabic (`Patient.FullNameAr`).
2. Sex + age in the unit stored on the patient (e.g., `Male - 35 Years`, `Female - 8 Months`, `Male - 12 Days`).
3. Comma-separated test codes (`TypeCode`) for every test sharing this tube.
4. Barcode image encoding `PatientCode`.
5. Footer line `"{PatientCode}   {TubeName}"` (three spaces).

---

## 2. Tube-Grouping Logic

**A label represents one tube/container, not one test.** Many tests
share a tube. CBC, Blood Type, and HbA1c are three different tests but
are all drawn into a single EDTA tube — therefore **one label** that
lists all three abbreviated codes.

### Canonical tube source (resolved)

- **Canonical source:** `TestType.TestTypeSampleTubes` rows, ordered
  by `SortOrder`. The **`SortOrder = 1` row is the primary tube** for
  v1 — its `SampleType` value (e.g., `Serum`, `EDTA Blood`, `Urine`)
  is the **grouping key** and the **tube name** shown on the label.
- **`CollectionType`** is a higher-level category lookup (e.g.,
  `Blood` vs `Urine`) — **not** a tube identity. It is used only as
  a defensive fallback when a `TestType` has zero `TestTypeSampleTubes`
  rows (legacy/imported data).
- **`TestType.DefaultTubeType`**, **`DefaultTubeColor`**, and
  **`SampleType`** on `TestType` are legacy fields, **never written
  by the UI**. **Do NOT read or write them** in new code.

### Multi-tube tests (known v1 limitation)

A test may have up to 3 tube rows (`Tube 1 / Tube 2 / Tube 3` in the
test-management UI), but `VisitTest.TubeId` is a single nullable FK
— a `VisitTest` can be linked to only one `SampleTube`. **v1 uses
only the primary tube (`SortOrder = 1`); secondary tube rows
(`SortOrder = 2, 3`) are silently ignored.** This is documented as
**G-11** below. A full fix requires a `VisitTestTube` link table +
migration and is captured as future work, not part of this delivery.

---

## 3. All Resolved Decisions

Every design question has been answered. Implementation must follow
these values exactly.

| Decision | Resolved value |
|---|---|
| **Tube source** | `TestTypeSampleTube.SampleType`, lowest `SortOrder` row. Fallback to `CollectionType.TypeNameEn`, then `"Unknown"`. |
| **Barcode encodes** | `PatientCode` (formatted string, e.g. `P20260617001`). **Not** the integer `PatientId`. |
| **Label footer** | `"{PatientCode}   {TubeName}"` (three spaces between). |
| **Printer type** | Thermal label printer via standard Windows printer driver. WPF `Visual` / `FixedDocument` printing. **No ZPL. No ESC/POS.** |
| **Label size** | 38 mm × 25 mm. |
| **Print scope** | Current visit only. |
| **Form after Save** | Stays unlocked. Form re-locks only when **إضافة** is clicked again. |
| **Age unit on label** | Actual `ApproxAgeUnit` value verbatim (Years / Months / Days) — never normalize. |
| **Abbreviation field** | `TypeCode` → fallback `TypeAbbrev` → fallback `TypeNameEn`. Read live from DB at print time; **not** snapshotted on `SampleTube`. |
| **Print All behaviour** | One single multi-page job, one label per page, each page 38 mm × 25 mm. |

---

## 4. Current Codebase State

### Wired and working
- `PatientRegistrationWindow.xaml` has the **Barcode button** on line
  94, bound to `BarcodeCommand`, label "الباركود".
- `PatientRegistrationViewModel.BarcodeAsync()` already:
  1. Calls `_sampleTrackingService.GenerateBarcodesForVisitAsync(...)`.
  2. Resolves `BarcodeDialogViewModel` from DI.
  3. Calls `LoadTubesAsync(CurrentVisitId)`.
  4. Opens `BarcodeDialog` modally.
- `BarcodeCommand` is gated on `CurrentVisitId > 0` — **same gate
  satisfies Case A and Case B** (`SaveAsync` and `LoadVisitForEditAsync`
  both set `CurrentVisitId`).
- `ISampleTrackingService.GetTubesForVisitAsync` already eager-loads
  `Visit → Patient` and `VisitTests → Testtype` (good for label
  rendering).

### Exists but does not match the spec
- `Views/Patients/BarcodeDialog.xaml` — a `DataGrid` with three text
  columns (`BarcodeValue`, `TubeType`, `TubeColor`) + per-row "Print"
  button + "Print All" button. **No visual label rendering, no name/
  sex/age/codes, no barcode image, no double-click-to-print.**
- `ViewModels/Patients/BarcodeDialogViewModel.cs` — prints a plain
  `FlowDocument` with text-only fields via `PrintDialog`. Contains an
  unused `BuildZplStub` method. **No real barcode image and not used
  for a label printer.**
- `Services/Implementations/SampleTrackingService.GenerateBarcodesForVisitAsync`
  groups tubes by `(TestType.DefaultTubeType, TestType.DefaultTubeColor)`
  — fields that the test-management UI **never populates**.
  **Confirmed by reading `TestDataManagementWindow.xaml` +
  `TestDetailViewModel.cs`:** the UI only writes
  `CollectionTypeId` and `TestTypeSampleTube` rows. Therefore the
  current grouping logic **crashes** the moment a UI-created test is
  saved (`new SampleTube { TubeType = null! }` violates the NOT NULL
  contract). `BarcodeValue` is currently `TUBE-{visitId}-{Guid:N}` —
  does not encode the patient code.

### Print infrastructure
- `IPrintService.PrintAsync(string documentType, object data)` —
  single generic entry point. Only `NullPrintService` (no-op)
  implementation. Registered as `Singleton` in `App.xaml.cs:147`.
- **No label printer driver, no ZPL/EPL output, no barcode-rendering
  library** is referenced anywhere in the solution.

### Form-lock behaviour (spec says fields are LOCKED until "Add")
- `PatientInfoView.xaml` has no `IsEnabled` / `IsReadOnly` bindings
  on the data fields (only `PatientCode` is read-only). **Form is
  always editable.**
- `PatientRegistrationViewModel.InitializeAsync()` calls
  `AddNewAsync()` — so the window opens already cleared and editable.
  Spec says it should open locked. **Gap.**

### Save behaviour (spec: data stays visible after save)
- `SaveAsync` populates `CurrentPatientId` / `CurrentVisitId`, sets
  `IsEditMode = true`, and does **not** clear any fields. ✓ Matches
  spec — no change needed.

### Existing-patient load (spec: don't re-prompt to save)
- `LoadVisitForEditAsync` populates everything from `VisitFullDto`,
  sets `IsEditMode = true`, `HasUnsavedChanges = false`. ✓ Matches
  spec.

---

## 5. Load-bearing Files

| Layer | File | Why it matters |
|---|---|---|
| View | `Views/Patients/PatientRegistrationWindow.xaml` | Barcode button + command binding (line 94). |
| View | `Views/Patients/PatientInfoView.xaml` | Will need `IsEnabled` binding for lock gap (if we close it). |
| View | `Views/Patients/BarcodeDialog.xaml` | Will be **replaced** with a label-style ItemsControl layout. |
| Code-behind | `Views/Patients/BarcodeDialog.xaml.cs` | May add a double-click input event that forwards to a VM command. |
| VM | `ViewModels/Patients/PatientRegistrationViewModel.cs` | Hosts `BarcodeCommand`, `AddNewAsync`, `SaveAsync`, `LoadVisitForEditAsync`. |
| VM | `ViewModels/Patients/BarcodeDialogViewModel.cs` | The popup view-model — needs label projection + real printing. |
| Service iface | `Services/Interfaces/ISampleTrackingService.cs` | Tube generation + retrieval. |
| Service impl | `Services/Implementations/SampleTrackingService.cs` | Tube grouping logic — fragile NULL handling + wrong barcode-value. |
| Service iface | `Services/Interfaces/IPrintService.cs` | Single `PrintAsync` entry point — may need a label-specific overload. |
| Service impl | `Services/Implementations/NullPrintService.cs` | The only impl today; a real label printer will live alongside it. |
| Model | `Models/SampleTube.cs` | Tube DB entity — `TubeType` (string, NOT NULL), `TubeColor`, `BarcodeValue`, `VisitTests` collection. |
| Model | `Models/TestType.cs` | Tube source-of-truth fields (`DefaultTubeType`, `DefaultTubeColor`, `SampleType`, `CollectionTypeId`, `TypeAbbrev`). |
| Model | `Models/VisitTest.cs` | Links visit ↔ test type ↔ tube. |
| Model | `Models/CollectionType.cs` | Normalized lookup, not currently consumed by barcoding. |
| Data | `Data/FinalLabDbContext.cs` | EF config; `SampleTube` is keyed by `TubeId`, unique index on `BarcodeValue`. |
| DI | `App.xaml.cs` | Register/replace `IPrintService`; `BarcodeDialog`/`BarcodeDialogViewModel` are already registered transient. |

---

## 6. MVVM Constraints

- **No `MessageBox.Show` from a ViewModel.** Use `IDialogService` (`ShowError`, `ShowWarning`, `ShowMessage`, `ShowConfirmation`).
- **No `async void` except framework signatures** (e.g. `Window_Loaded`).
- **No business logic in code-behind.** The only acceptable code-behind
  is wiring (`DataContext = vm`) and forwarding an input event to a
  bound command. If a `MouseDoubleClick` handler is added, it must
  call into the VM via a command — no logic inline.
- **Protected files — do not touch:**
  - `Infrastructure/Security/PasswordHasher.cs`
  - `Infrastructure/ViewModelBase.cs`
  - `Infrastructure/Navigation/NavigationService.cs`
  - Anything under `Migrations/`

---

## 7. Gap List

See `work_plan_BARCODE.md` Section B for the full table. Headline
items, with current status:

- **G-01** `BarcodeDialog.xaml` has no label-style rendering, no name/
  sex/age/codes/barcode-image — needs a complete redesign.
- **G-02** `BarcodeDialogViewModel` projects only raw `SampleTube`
  rows. Needs a `BarcodeLabel` projection with patient name, sex+age,
  comma-joined test codes, barcode-encoded value, footer line.
- **G-03** No barcode-image rendering anywhere. Need a library
  (e.g., ZXing.Net) and a converter / control to render `Code128`
  (or chosen symbology) inside the dialog.
- **G-04** `SampleTrackingService.GenerateBarcodesForVisitAsync`
  groups by `(DefaultTubeType, DefaultTubeColor)` — **broken in
  practice** (confirmed: the test-management UI never writes those
  fields). Will crash on every UI-created test. Must be rewritten to
  group by `TestTypeSampleTube.SampleType` (lowest `SortOrder` row)
  and to produce a `PatientCode`-based `BarcodeValue`.
- **G-05** No `IPrintService` method for labels. Need
  `ILabelPrintService` + a concrete `WpfLabelPrintService` (Q-03 fixed
  this to Windows printer; no ZPL service needed).
- **G-06** No "double-click to print one label" wiring in the dialog.
- **G-07** Form-lock behaviour missing: window opens editable when
  spec says LOCKED until Add. Affects `PatientInfoView`,
  `ReferralSectionView`, `MedicalHistorySectionView`,
  `FinancialSectionView`, `TestSelectionView` (verify each — the
  test-selection sub-view may need per-control bindings instead of
  a root `IsEnabled`).
- **G-08** **RESOLVED** — `TubeResolver` uses `TestTypeSampleTubes`
  (lowest `SortOrder`) with `CollectionType.TypeNameEn` fallback.
  Single concrete rule, no multi-source logic.
- **G-09** `SelectedTestDto.BillNameLine1` is declared but never
  populated by `VisitService.GetVisitFullDataAsync`. Cosmetic —
  flagged for future cleanup, not blocking.
- **G-10** `GenerateBarcodesForVisitAsync` is idempotent (early
  return when tubes already exist for the visit). Verify this stays
  correct under the new `BarcodeValue` scheme.
- **G-11** **NEW** — Multi-tube tests: a test can have up to 3 tube
  rows, but `VisitTest.TubeId` is a single nullable FK. **v1 uses
  only the primary tube (`SortOrder = 1`); secondary tube rows are
  silently ignored.** A full fix would require a `VisitTestTube`
  link table + DB migration; captured as future work.

---

## 8. Implementation Phases Summary

All phases from `work_plan_BARCODE.md` Section C. Current status is
**Pending** for every phase until the user confirms a starting point.

| # | Phase | Status |
|---|---|---|
| 1 | Add `TubeResolver` helper (canonical source = `TestTypeSampleTube`) | Pending |
| 2 | Rewrite `SampleTrackingService.GenerateBarcodesForVisitAsync` | Pending |
| 3 | Add a barcode-rendering library + WPF helper | Pending |
| 4 | Build the `BarcodeLabel` projection in the dialog VM | Pending |
| 5 | Redesign `BarcodeDialog.xaml` as a label list | Pending |
| 6 | Real print integration (`WpfLabelPrintService`, 38×25 mm) | Pending |
| 7 | Wire double-click and confirm Barcode-button flow | Pending |
| 8 | Form-locking behaviour (G-07) | Pending |
| 9 | End-to-end manual verification (no code) | Pending |

---

## 9. How to Onboard a New Agent

A future coding agent picking up this work MUST follow this sequence:

1. **Read this file (`HANDOFF_BARCODE.md`) first** — full context,
   resolved decisions, gap list, phases summary.
2. **Read `work_plan_BARCODE.md`** for the phase-by-phase plan, file
   lists, and risk register.
3. **Ask the user which phase to start from** — phases are sequential
   and each ends with a passing build; do not assume Phase 1.
4. **Do NOT start implementing until the user confirms the starting
   phase.** Surface any ambiguity as a question, not a code change.
5. **After each phase:** build the project, confirm it compiles,
   report status (what was changed, what was verified) back to the
   user **before** beginning the next phase.
6. **Never touch protected files:**
   - `Infrastructure/Security/PasswordHasher.cs`
   - `Infrastructure/ViewModelBase.cs`
   - `Infrastructure/Navigation/NavigationService.cs`
   - Anything under `Migrations/`

Additional rules baked into the plan:
- All Section D decisions are **final**. If a phase appears to require
  re-opening one, stop and ask — do not silently deviate.
- Phase 6 implements `WpfLabelPrintService` **only**. **Do not** build
  a ZPL/ESC-POS service.
- v1 single-tube limitation (G-11) is intentional. Do not start the
  `VisitTestTube` link-table refactor without explicit user approval.

---

## 10. Verification Checklist

A reviewer should be able to:

1. Open `PatientRegistrationWindow`, click **إضافة**, enter a patient
   with 3+ tests across 2+ tube types, click **حفظ**, then click
   **الباركود** → dialog opens, shows 2+ rendered labels.
2. From `MainWindow → قائمة المرضى` (or any search), load that same
   patient → click **الباركود** → dialog opens with the same 2+
   labels, no save prompt.
3. Click **Print All** → all labels go to the printer in one action.
4. Double-click a single label → only that label prints.
5. Close the dialog and verify the registration window still shows
   the patient's data populated.

---

*End of handoff.*

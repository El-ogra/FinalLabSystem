# HANDOFF_CONTEXT.md

**Project:** FinalLabSystem (WPF / .NET 8 / EF Core 8 / MVVM — Arabic-language lab management system)
**Repo root:** `C:\Users\LAP LINK\source\repos\FinalLabSystem`
**Audit branch / HEAD:** `main` @ `3fcee16`
**Handoff date:** 2026-06-10
**Authoritative audit report:** `FinalLabSystem/Docs/Audit_TestDataManagement_NormalRanges_Remediation_Roadmap_v2.md`

This file exists so a fresh agent (or human) can pick up exactly where the prior audit session ended without re-reading the entire conversation. **No code was changed in the audit session.** Implementation has not started.

---

## 1. Files Read or Modified During the Audit Session

### 1.1 Files read directly (Read tool)

- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\App.xaml.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\FinalLabSystem.csproj`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\Views\Settings\TestDataManagementWindow.xaml`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\Views\Settings\NormalRangesWindow.xaml`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\TestDataManagementViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\TestListViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\TestDetailViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\TestRowViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\NormalRangeWindowViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\NormalRangeListViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\NormalRangeDetailViewModel.cs`
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\ViewModels\Settings\CategoriesGroupsViewModel.cs`
- `C:\Users\LAP LINK\.claude\plans\entire-audit-without-modifying-curried-puppy.md` (re-read after edits)

### 1.2 Files inspected via Bash (cat / ls / git)

- `C:\Users\LAP LINK\source\repos\FinalLabSystem\.gitignore`
- Repo root and `FinalLabSystem/` directory listings
- `git ls-files`, `git check-ignore -v` for `.csproj.user`

### 1.3 Files surveyed via Grep (paths only, content not fully read in main thread)

Grep matches surfaced these paths but the files were not opened in full in the main thread (subagents may have read them):

- `FinalLabSystem\Services\Interfaces\IAuditService.cs`
- `FinalLabSystem\Services\Implementations\AuditService.cs`
- `FinalLabSystem\ViewModels\Patients\PatientRegistrationViewModel.cs` (line refs only)
- `FinalLabSystem\ViewModels\Patients\FinancialViewModel.cs` (line refs only)
- `FinalLabSystem\Infrastructure\AsyncRelayCommand.cs` (line refs only)
- `FinalLabSystem\Docs\test_data_implementation_report.md`
- `FinalLabSystem\Docs\Test Data Management_ Work Plan.md`
- `FinalLabSystem\Docs\test_data_work_plan.md`
- `FinalLabSystem\Docs\Patient_Window_Context.md`
- `FinalLabSystem\Docs\FinalLab_ServiceLayer_MasterPlan.md`

### 1.4 Files read via Explore subagents (indirect — broader scan, not fully read in main thread)

Three parallel Explore agents covered architecture/data, UI, and cross-cutting concerns. They touched many additional files including:

- `FinalLabSystem/Data/FinalLabDbContext.cs` (2053 lines)
- `FinalLabSystem/Data/FinalLabDbContextFactory.cs`
- All files under `FinalLabSystem/Models/` (~48 entities)
- All files under `FinalLabSystem/Migrations/` (9 migrations)
- All 18 files under `FinalLabSystem/Services/Interfaces/`
- All 18 files under `FinalLabSystem/Services/Implementations/`
- All files under `FinalLabSystem/Infrastructure/` (ViewModelBase, RelayCommand, NavigationService, PasswordHasher, CurrentUserSession, PasswordBoxHelper, JsonUserSettingsService, etc.)
- Remaining ViewModels (Patients/*, Login, FirstRunSetup, Main, MedicalHistory, Referral, Financial, etc.)
- Remaining Views (Login, FirstRunSetup, MainWindow, PatientRegistration, TestResults, Delivery, PatientSearch, BarcodeDialog, ReceiptDialog, TodayPatientsDialog, etc.)
- All files under `FinalLabSystem/Docs/` (12 documents totaling ~450 KB)

Treat subagent-touched findings as **Reported** unless a finding's location is explicitly tagged *Verified* in §2 below.

### 1.5 Files modified (Write / Edit)

- `C:\Users\LAP LINK\.claude\plans\entire-audit-without-modifying-curried-puppy.md` — created (1 Write) and refined (7 Edits) during the audit. This is the plan file.
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem\Docs\Audit_TestDataManagement_NormalRanges_Remediation_Roadmap_v2.md` — created in the final handoff turn (full copy of the plan, saved alongside project docs).
- `C:\Users\LAP LINK\source\repos\FinalLabSystem\HANDOFF_CONTEXT.md` — this file.

**No `.cs`, `.xaml`, `.csproj`, `.sln`, or other project source/config files were modified.**

---

## 2. Findings F-01 through F-33 — Compact Index

| ID | Title | Severity | Layer | Phase | One-line root cause |
|---|---|---|---|---|---|
| F-01 | DbContext registered with `ServiceLifetime.Transient` | Critical | Infrastructure / DI | Immediate | EF unit-of-work is per-context; Transient creates a new DbContext per injection, splitting change-tracking across instances. **Verified** at `App.xaml.cs:55-59`. |
| F-02 | Hardcoded connection string with `TrustServerCertificate=True` | Critical | Infrastructure / Config | Immediate | No `appsettings.json`; conn string embedded in source; TLS validation disabled. **Verified.** |
| F-03 | No logging framework | Critical | Cross-cutting | Immediate | Logging never set up; exceptions either rethrow or surface via `MessageBox`. **Verified** (csproj has no logging packages). |
| F-04 | Zero automated tests | Critical | Quality | Immediate → Medium | No test project in solution; services tightly coupled to concrete DbContext. **Verified.** |
| F-05 | No CI / build validation | Critical | Process | Immediate → Short-term | No `.github/workflows/`, no `azure-pipelines.yml`. **Verified.** |
| F-06 | No input validation on entities | High | Domain / Data | Short-term | No data annotations on models; UI cannot pre-validate; users see raw SQL errors. *Reported.* |
| F-07 | `MessageBox.Show` in ViewModels | High | UI / MVVM | Short-term | 13 verified sites; UI primitive coupled into VMs, blocks unit testing. **Verified — 13 sites.** |
| F-08 | `async void` event handlers | High | UI / MVVM | Immediate | 3 verified handlers swallow exceptions and can crash the dispatcher. **Verified — 3 sites.** |
| F-09 | `AuditService` defined but unregistered and uncalled | High | Security / DI | Short-term | Service exists in code but not in DI; injection would throw at resolution. **Verified.** |
| F-10 | Unbounded queries on patient / hierarchy reads | High | Data / Performance | Short-term | No paging contract on `SearchPatientsAsync`, `GetFullHierarchyAsync`, `GetPatientByIdAsync`. *Reported.* |
| F-11 | No session timeout / idle lockout | High | Security | Short-term | `CurrentUserSession` persists until app close; no idle detector. *Reported.* |
| F-12 | `CurrentUserSession` singleton not thread-safe | High | Concurrency | Short-term | Plain field reads/writes; torn reads possible from threadpool. *Reported.* |
| F-13 | Code-behind business logic in `TodayPatientsDialog` | Medium | UI / MVVM | Short-term | Filter, search, selection logic live in `.xaml.cs`; no backing VM. *Reported.* |
| F-14 | Mouse-event handler in `TestSelectionView` code-behind | Medium | UI / MVVM | Short-term | Double-click invokes `AddTestCommand` from code-behind. *Reported.* |
| F-15 | Boolean column proliferation on `Visit` | Medium | Domain / Data | Medium-term | 16 medical-history + 6 outside-sample booleans on one entity. *Reported.* |
| F-16 | Boolean / flag proliferation on `TestType` | Medium | Domain / Data | Medium-term | Overlapping behavior flags (`IsSendOutside` vs `IsOutsourceable`, etc.). *Reported.* |
| F-17 | Type inconsistency in financial / age fields | Medium | Domain | Short-term | Currency stored as `double` in places; `Patient.ApproxAge` is `double`. *Reported.* |
| F-18 | String-typed state machines | Medium | Domain | Short-term | `VisitStatus`, `PaymentStatus`, `CurrentStage`, `PaymentMethod` stored as strings. *Reported.* |
| F-19 | `TestResult` lacks validation status | Medium | Domain / Workflow | Short-term | No "entered / reviewed / validated / released" field; signoff invisible. *Reported.* |
| F-20 | Dual storage `ResultValue` (string) + `ResultNumeric` (double) | Medium | Domain | Medium-term | Two columns hold the same datum; no invariant ties them. *Reported.* |
| F-21 | Specialized test entities as 3 optional 1:1 FKs on `VisitTest` | Medium | Domain / Data | Medium-term | TPT/TPC inheritance not used; three NULL FKs in dominant case. *Reported.* |
| F-22 | Fire-and-forget async initialization (6 sites) | Medium | UI / Async | Short-term | Constructor/setter `_ = LoadAsync();` swallows exceptions. **Verified — 6 sites.** |
| F-23 | No global exception handler | Medium | Cross-cutting | Immediate | `OnStartup` lacks try/catch; no `DispatcherUnhandledException` subscription. **Verified.** |
| F-24 | Snapshot fields on `TestResult` drift from `NormalRange` | Medium | Domain | Medium-term | Snapshots not versioned/linked back to the range row applied. *Reported.* |
| F-25 | Two competing outsourcing models | Medium | Domain | Medium-term | Inline `VisitTest` outsource fields vs `ExternalShipment` — neither authoritative. *Reported.* |
| F-26 | No README.md at repo root | Low | Documentation | Short-term | No setup/run/migrations orientation for a new developer. **Verified.** |
| F-27 | Sparse XML documentation comments | Low | Documentation | Medium-term | Service interfaces lack `/// <summary>` blocks. *Reported.* |
| F-28 | `AsyncRelayCommand.Execute` uses `async void` | Low | Infrastructure | Medium-term | Required by `ICommand`; needs contract docs or replacement with CommunityToolkit. *Reported.* |
| F-29 | Print logic in `PrintPreviewWindow` code-behind | Low | UI / MVVM | Medium-term (or won't-fix) | `PrintDialog` opened from click handler — inherently UI-bound. *Reported.* |
| F-30 | `Patient.ApproxAge` typed as `double` | Low | Domain | Medium-term | Ages are integers; `double` allows fractional values no caller uses. *Reported.* |
| F-31 | `NormalRangesWindow` binds to a non-existent `Unit` property | **High (new)** | UI / Data Binding | Short-term | Two XAML bindings reference a `Unit` member that does not exist on `NormalRange` or `NormalRangeDetailViewModel`; silent data loss. **Verified.** |
| F-32 | Dead UI controls (checkbox + button + missing items) | Medium (new) | UI | Short-term | Patient-Question checkbox and "تعديل" button have no bindings; user clicks do nothing. **Verified.** |
| F-33 | `NormalRangeDetailViewModel` `RangeFor` radio-button group broken | **High (new)** | UI / MVVM | Short-term | Four radios over a 3-value enum; two map to the same state, two map to wrong sex; no "Both" sex radio. Sex="Both" unreachable from UI. **Verified.** |

**Severity totals:** Critical 5 · High 8 · Medium 15 · Low 5 · Total 33.

---

## 3. Agreed Implementation Order

The user explicitly directed the following sequence at the end of the audit session:

1. **F-01 first** — DbContext lifetime: Transient → Scoped. Touches `App.xaml.cs:55-59` plus a scope-boundary review.
2. **F-08 next** — Fix the 3 `async void` event handlers in `CategoriesGroupsViewModel.cs:167`, `TestDataManagementViewModel.cs:160`, `TestDataManagementViewModel.cs:168`.
3. **One at a time, with manual approval between each change.** Do not batch findings into a single PR or commit. Do not start the next finding until the user has reviewed and explicitly approved the previous one.

After F-08 the next finding is at the user's discretion — Phase I still contains F-02, F-03, F-04, F-05, F-23. There is no auto-advance.

---

## 4. F-31 / F-32 / F-33 — Full Details

These are the three new findings surfaced during the re-verification pass. Each is documented here in full so the next agent does not need to consult the full audit report to act on them.

### F-31 — `NormalRangesWindow` binds to a `Unit` property that does not exist

- **Severity:** High
- **Layer:** UI / Data Binding
- **Complexity:** M
- **Phase:** Short-term

**Exact locations (verified):**

1. `FinalLabSystem/Views/Settings/NormalRangesWindow.xaml:89`
   ```xml
   <DataGridTextColumn Header="Test unit" Binding="{Binding Unit}" Width="80"/>
   ```
   The row item is a `NormalRange` entity. `NormalRange` has no `Unit` member.

2. `FinalLabSystem/Views/Settings/NormalRangesWindow.xaml:215-217`
   ```xml
   <ComboBox Grid.Column="1" Style="{StaticResource FieldComboBox}"
             IsEditable="True"
             Text="{Binding Detail.Unit, UpdateSourceTrigger=PropertyChanged}">
   ```
   `NormalRangeDetailViewModel` (file read end-to-end) exposes `AgeUnit`, `LowNormal`, `HighNormal`, `LowFlag`, `HighFlag`, `LowComment`, `HighComment`, `NormalRangeText`, `LowCritical`, `HighCritical`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `AgeDescription`, `RangeNote`, `ForPregnantOnly`, `Sex`, `RangeFor`, `IsSexMale`, `IsSexFemale`, `IsSexBoth`, `IsFastingAny`, `IsFasting` — **but no `Unit` property**.

**Root cause:** The "test unit" concept lives on `TestComponent.Unit`, not on `NormalRange` or the detail VM. The XAML form lets the user pick a unit per range (mg/dL, U/L, etc.) but the value has nowhere to go. WPF binding failures are silent unless `PresentationTraceSources.TraceLevel=High` is set; the user picks a value, sees no error, saves, the value vanishes.

**Recommended fix — pick one:**

- **(a) Range carries its own unit (recommended).** Add `Unit` (nullable string) to `NormalRange`. Steps:
  1. Add `public string? Unit { get; set; }` to `Models/NormalRange.cs`.
  2. Configure column in `FinalLabDbContext` Fluent API (`HasMaxLength(20)`, nullable).
  3. Add EF Core migration `AddNormalRangeUnit`.
  4. Add `public string? Unit { get => EditableRange.Unit; set => SetRangeProperty(...); }` to `NormalRangeDetailViewModel.cs` (modeled after `AgeUnit` at lines 173-177).
  5. Add `Unit` to `RaiseAllChanged()` and `CloneRange()`.
  6. No XAML change needed — bindings already point at `Detail.Unit` and `Unit` on the row.

- **(b) Unit belongs to the component.** Remove unit controls from the form; show the component's unit read-only (`Components/{component}.Unit`).

Lab convention often supports per-range units (glucose: mg/dL vs mmol/L for the same component, different methods), so **(a) is the safer bet** and matches the existing form intent.

### F-32 — Dead UI controls

- **Severity:** Medium
- **Layer:** UI
- **Complexity:** XS each, S total
- **Phase:** Short-term

**Exact locations (verified):**

1. `FinalLabSystem/Views/Settings/TestDataManagementWindow.xaml:289`
   ```xml
   <CheckBox Content="Patient Question" Margin="0,8,0,4"/>
   ```
   No `IsChecked` binding. The `<TextBox>` below it (line 290, bound to `TestDetail.PatientQuestion`) is always editable.

2. `FinalLabSystem/Views/Settings/NormalRangesWindow.xaml:281`
   ```xml
   <Button Content="تعديل" Style="{StaticResource ActionButtonStyle}"/>
   ```
   No `Command` binding. Styled but dead.

**Root cause:** UI was sketched faster than VM commands/properties caught up. Recent commits ("UI 100% complete") reflect visual completeness, not functional completeness for these specific controls.

**Recommended fix:**

- **Patient Question checkbox** — pick one:
  - Add `IsChecked="{Binding TestDetail.HasPatientQuestion}"` to gate the textarea's `Visibility` or `IsEnabled` (requires new `HasPatientQuestion` boolean on the VM), OR
  - Remove the checkbox entirely.
- **"تعديل" button** — pick one:
  - Wire to an edit command on `NormalRangeWindowViewModel` (e.g., toggle a `IsEditing` flag that enables/disables form fields), OR
  - Remove the button (the form is already editable).

The "تعديل" button is harder because the semantics aren't clear. Default to removal unless the team can define what "edit mode" means in this window.

### F-33 — `NormalRangeDetailViewModel` `RangeFor` radio-button group is broken

- **Severity:** High (data integrity)
- **Layer:** UI / MVVM
- **Complexity:** S
- **Phase:** Short-term

**Exact locations (verified):**

- `FinalLabSystem/ViewModels/Settings/NormalRangeDetailViewModel.cs:11` — enum `NormalRangeFor { Female, Male, Both }`.
- `FinalLabSystem/ViewModels/Settings/NormalRangeDetailViewModel.cs:69-91` — four properties over the three-value enum:
  ```csharp
  public bool IsRangeForAll        { get => RangeFor == NormalRangeFor.Both;   set { if (value) RangeFor = NormalRangeFor.Both;   } }
  public bool IsRangeForSexAndAge  { get => RangeFor == NormalRangeFor.Female; set { if (value) RangeFor = NormalRangeFor.Female; } }
  public bool IsRangeForSexOnly    { get => RangeFor == NormalRangeFor.Male;   set { if (value) RangeFor = NormalRangeFor.Male;   } }
  public bool IsRangeForAgeOnly    { get => RangeFor == NormalRangeFor.Both;   set { if (value) RangeFor = NormalRangeFor.Both;   } }
  ```
- `FinalLabSystem/Views/Settings/NormalRangesWindow.xaml:118-121` — four radio buttons bound to those four properties.
- `FinalLabSystem/Views/Settings/NormalRangesWindow.xaml:128-129` — only Male and Female sex radio buttons; no "Both" radio despite `IsSexBoth` existing on the VM (line 233-237 of the VM).
- `FinalLabSystem/ViewModels/Settings/NormalRangeDetailViewModel.cs:296-302` — save path writes `Sex` as `"M"`, `"F"`, or `"Both"` based on the unreachable `Sex` value.

**Three concrete bugs:**

1. **Duplicate states.** `IsRangeForAll` and `IsRangeForAgeOnly` are mechanically identical — both read/write `NormalRangeFor.Both`. The two radio buttons toggle the same state.
2. **Wrong semantics.** `IsRangeForSexAndAge` maps to `Female`; `IsRangeForSexOnly` maps to `Male`. "By sex AND age" and "By sex only" are orthogonal to which sex, not synonyms for it.
3. **Unreachable Sex=Both.** No XAML control binds `IsSexBoth`. New ranges created through this dialog can only persist `Sex="M"` or `Sex="F"`, never `"Both"` — even though the "For all" radio button suggests otherwise.

**Root cause:** The enum, properties, and XAML radio buttons were never reconciled into a coherent state machine.

**Recommended fix — pick one design:**

- **Option A — 3 buttons, match enum.** Drop `IsRangeForAgeOnly` from XAML; keep `IsRangeForAll = Both`; rename `IsRangeForSexAndAge` → `IsRangeForFemale`, `IsRangeForSexOnly` → `IsRangeForMale`; add a third sex radio button bound to `IsSexBoth`.
- **Option B — split into two independent concerns.** Replace `RangeFor` with two booleans (`AppliesBySex`, `AppliesByAge`) wired to two checkboxes. Independently pick `Sex` (M/F/B) and the age range. Matches what the XAML labels seem to be reaching for.

Either way: no schema change needed; the underlying `NormalRange.Sex` column already accepts `"M"/"F"/"Both"`. Just the UI ↔ VM mapping needs reconciliation.

---

## 5. Critical Warnings for a New Agent

### 5.1 Load-bearing files — do not break

- **`Infrastructure/Security/PasswordHasher.cs`** — PBKDF2-SHA256, 100,000 iterations, 16-byte salt, 32-byte hash, `CryptographicOperations.FixedTimeEquals`. Algorithm is correct. Do not modify the iteration count, hash algorithm, or comparison primitive. Changing any of these breaks all existing user passwords.
- **`Infrastructure/ViewModelBase.cs`** — base class for all 27 ViewModels. Do not change its public surface (`OnPropertyChanged`, `SetProperty<T>`).
- **`Infrastructure/Navigation/NavigationService.cs`** — type-safe `RegisterWindow<TVM,TWin>()`; `Closed` event subscribed and unsubscribed cleanly. Reuse this pattern when adding new windows; do not invent a parallel navigation mechanism.
- **`Data/FinalLabDbContext.cs`** — 2053 lines of Fluent API configuration. Many findings reference it. When making schema changes (F-15, F-16, F-17, F-19, F-24, F-31), always generate a new EF migration; never edit a previously committed migration.
- **`App.xaml.cs`** — DI composition root. Many findings touch lines 53-118. After F-01 lands, before adding new services double-check the new lifetime is correct for the dependency tree.

### 5.2 Verified vs Reported findings

A finding marked **Verified** in §2 has had its location re-confirmed by direct file read in the audit session. A finding marked **Reported** was identified by an Explore subagent but not independently re-read in the main thread.

- **Verified — act with confidence:** F-01, F-02, F-03, F-04, F-05, F-07, F-08, F-09, F-22, F-23, F-26, F-31, F-32, F-33.
- **Reported — re-read the cited file before high-cost remediation:** F-06, F-10, F-11, F-12, F-13, F-14, F-15, F-16, F-17, F-18, F-19, F-20, F-21, F-24, F-25, F-27, F-28, F-29, F-30.

For *Reported* findings, the cost of a 30-second re-read before a multi-day refactor is trivial compared to acting on stale information.

### 5.3 MVVM pattern constraints — non-negotiable

- **Every new ViewModel must inherit `ViewModelBase`** (`FinalLabSystem/Infrastructure/ViewModelBase.cs`). Do not introduce CommunityToolkit.Mvvm just because; it's an open consideration (F-28) but a separate decision.
- **No `MessageBox.Show` calls inside ViewModels.** Existing sites are tracked under F-07 and will be replaced with `IDialogService`. New code must use the dialog service once it lands; do not add new MessageBox sites.
- **No `async void` outside framework signatures.** Currently 3 defect sites (F-08) and 3 framework-required sites (`App.OnStartup`, `ICommand.Execute`×2 in `AsyncRelayCommand`). Any new event handler should be `async Task` (preferred), wrapped in `try/catch`, or routed through a command.
- **No fire-and-forget `_ = SomethingAsync()` in constructors.** F-22 catalogs the existing 6 sites. New ViewModels should expose `InitializeAsync()` and let the view's `Loaded` event invoke it. Setters that need to refresh data should wrap in a private `async Task` with try/catch.
- **Code-behind should be minimal.** New `.xaml.cs` files should contain only `InitializeComponent()` and the constructor that takes the ViewModel via DI. Filtering, business logic, and commands belong on the ViewModel.

### 5.4 Dependency order between findings

Some findings should be sequenced because later ones depend on the infrastructure earlier ones introduce:

- **F-03 (logging) must precede F-23 (global exception handler).** The handler needs `ILogger` to write fatal errors.
- **F-03 (logging) must precede the exception-handling parts of F-08 and F-22.** Both fixes route caught exceptions to the logger.
- **F-07 (IDialogService) is implicitly needed by F-08 and F-22 fixes.** Their `try/catch` blocks call `IDialogService.ShowError`. Either land F-07 first, or have F-08/F-22 ship with a temporary `MessageBox` fallback that's swapped out when F-07 lands.
- **F-04 (test infrastructure) must precede F-05 (CI tests-on-PR).** Phase I lists F-05 as build-only first; the test run is added once a test project exists.
- **F-01 (Scoped DbContext) should precede F-09 (SaveChanges override).** Overriding `SaveChangesAsync` to write audit rows assumes a consistent per-scope DbContext lifetime.
- **F-09 (AuditService DI registration) is the prerequisite step inside F-09 itself.** Register the service before any consumer takes a dependency on `IAuditService`.
- **F-02 (externalize connection string) pairs naturally with F-01.** Both touch `App.xaml.cs`. Land together to minimize churn on the composition root.
- **F-31, F-32, F-33 are mutually independent** and can be picked up in any order during Phase S.

### 5.5 Things the audit deliberately did NOT cover

Do not assume these are clean just because they're not flagged:

- Database performance under load (no query profiling, no real-data analysis).
- UI/UX quality, accessibility, Arabic copy correctness.
- Migrations applied vs authored (no live DB connection was made).
- Third-party security / SCA / vulnerability scan on NuGet packages.

If a remediation has implications in these areas, gate the change on additional review.

---

## 6. Quick-Reference Pointers

- **Authoritative audit report:** `FinalLabSystem/Docs/Audit_TestDataManagement_NormalRanges_Remediation_Roadmap_v2.md`
- **Source plan file (Claude internal):** `C:\Users\LAP LINK\.claude\plans\entire-audit-without-modifying-curried-puppy.md`
- **Existing project docs (Arabic, 450 KB):** `FinalLabSystem/Docs/`
- **Repo HEAD at audit time:** `main` @ `3fcee16` ("نافذه المعدلات الطبيعية 100% من ناحيه الشكل")
- **Next action:** F-01 (DbContext Scoped) — single small change in `App.xaml.cs:55-59`, then pause for user review.

*End of handoff.*

# FinalLabSystem — Comprehensive Audit Report & Remediation Plan

**Audit date:** 2026-06-10
**Auditor mode:** Read-only. No code modified, added, or deleted.
**Repository:** `C:\Users\LAP LINK\source\repos\FinalLabSystem`
**Branch / HEAD:** `main` @ `3fcee16`
**Stack:** WPF · .NET 8.0 (`net8.0-windows`) · EF Core 8 · SQL Server Express · MVVM
**Locale:** Arabic-first, RTL UI (Egyptian lab market)

---

## 1. Context

This audit was requested as an **entire** audit — every architectural layer, every cross-cutting concern, no implementation. The deliverable is a written report and a remediation plan; no code is changed. The repository is an Arabic-language lab management system at v4 with ~209 tracked files, 9 EF migrations in the last 5 days, and ~450 KB of Arabic functional documentation in `FinalLabSystem/Docs/`. Recent commits focus on the Normal Ranges and Test Data Management UI windows.

The intended outcome of this document:

1. Give a single, scannable picture of the codebase's health across architecture, data, UI, security, performance, and operability.
2. Catalog every finding with file references, severity, root cause, recommended fix, layer, complexity, and priority phase.
3. Group remediation into Immediate / Short-Term / Medium-Term phases the team can sequence against actual sprint capacity.
4. Document an explicit verification approach so each remediation can be proven done.

**Scope explicitly excluded from action:** Nothing in this document should be read as "fix this now." All recommendations are forward-looking; the team chooses what to act on.

---

## 2. Audit Methodology

- Three parallel Explore agents scanned (a) architecture + data layer, (b) UI layer (ViewModels/Views/App entry), (c) cross-cutting concerns (tests, CI, config, logging, security, perf, repo hygiene).
- Critical claims were re-verified by direct file read: `App.xaml.cs`, `FinalLabSystem.csproj`, `.gitignore`, `git ls-files`, `git check-ignore`.
- One agent finding (`.csproj.user` tracked in git) was **disproved by verification** — see §9 Corrections.
- No code was executed, no database touched.

**Confidence levels in this report:**

- **Verified** — confirmed by direct file read or git command in this session.
- **Reported** — surfaced by Explore agent inspecting code; not independently re-read. Treat as high-probability but worth confirming before acting on a high-cost remediation.

---

## 3. Executive Summary

### 3.1 Health snapshot

| Layer | Grade | One-line |
|---|---|---|
| Domain model | A− | 48 well-organized entities; rich coverage of routine / micro / blood bank / andrology workflows |
| Data layer (EF) | B | Fluent API throughout, good indexes; but DbContext lifetime is wrong and connection string is hardcoded |
| MVVM / UI | A− | 27 ViewModels on a clean `ViewModelBase`; ~90% of views have minimal code-behind |
| Services | B | 18 services, async-correct, transactions used; but tightly coupled to DbContext and untested |
| Security | B+ | PBKDF2/100k iterations, parameterized queries, integrated auth; but no audit-log usage and no session timeout |
| Cross-cutting | D | No logging, no tests, no CI, no centralized error handling, no externalized config |
| Repo hygiene | A− | `.gitignore` correct; `.csproj.user` properly ignored (one agent's claim of tracking was wrong) |
| Documentation | B | 450 KB Arabic functional specs; no README, sparse code XML docs |

### 3.2 Severity counts (this audit, after re-verification pass)

| Severity | Count |
|---|---|
| Critical (production-blocking or data-integrity-risking) | 5 |
| High (significant risk, fix soon) | 8 |
| Medium (technical debt, fix in normal flow) | 15 |
| Low (cosmetic, opportunistic) | 5 |
| **Total findings** | **33** |

> **Re-verification pass (§12):** four findings (F-07, F-08, F-09, F-22) were originally marked *Reported*; they have now been independently re-verified by direct file read. The scan during re-verification surfaced three new findings (F-31 to F-33), all in the Test Data Management / Normal Ranges windows. Counts above reflect the verified total.

### 3.3 What's done well (do not lose these)

- Modern stack: .NET 8 LTS, EF Core 8, async-throughout, nullable reference types enabled.
- `PBKDF2-SHA256 / 100_000 iterations / 16-byte salt / 32-byte hash` with `CryptographicOperations.FixedTimeEquals` — `Infrastructure/Security/PasswordHasher.cs`. Constant-time, format-versioned (`pbkdf2$iterations$salt$hash`). Keep this.
- `INavigationService` (`Infrastructure/Navigation/NavigationService.cs`) — type-safe `RegisterWindow<TVM,TWin>()`, lifecycle-managed (`Closed` event unsubscribed on return), three-window model (bootstrap → main → task). This is a healthy pattern; reuse it for new windows.
- Database-level audit triggers on `TestResult` (`TR_TestResult_AuditInsert`, `TR_TestResult_AuditUpdate`) + snapshot of normal-range bounds at result-entry time on `TestResult.Snap*` fields. Result history is recoverable.
- Custom `ViewModelBase` (`Infrastructure/ViewModelBase.cs`) with `SetProperty<T>` — lightweight, no toolkit dependency, used uniformly across all 27 VMs.
- Transactions used correctly for multi-entity saves (`VisitService.CreateVisitAsync`, `TestCatalogService.CreateTestTypeAsync`, `AuthService.CreateUserAsync`) — `BeginTransactionAsync` + `try/Commit/catch/Rollback/throw`.
- Strategic indexes (`IX_Patient_Name`, `IX_Patient_Phone`, `IX_Visit_Date`, composite `UQ_VisitTest` etc.) — search paths covered.
- RTL flow direction set on top-level windows; styles centralized in `Views/Shared/SharedStyles.xaml`.

---

## 4. Findings Catalog

> Each finding has: **ID · Title · Severity · Layer · Location · Root Cause · Recommended Fix · Complexity · Phase**.
> Complexity scale — XS: <1 hr · S: 1–4 hr · M: 4–16 hr · L: 2–5 days · XL: >1 week.
> Phase — **I**mmediate · **S**hort-term · **M**edium-term.

### CRITICAL

---

#### F-01 — DbContext registered with `ServiceLifetime.Transient`

- **Severity:** Critical
- **Layer:** Infrastructure / DI
- **Location:** `App.xaml.cs:55-59` *(Verified)*
- **Root cause:** Both the context lifetime and the options-builder lifetime are explicitly passed as `ServiceLifetime.Transient`. EF Core's unit-of-work, change tracking, and identity map are *per-context*; with Transient, every injected dependency in the same service-call graph gets a different DbContext instance. Two services touching the same entity in one logical operation will see two different identity maps and two different change-tracker states. SaveChanges on context A will not persist tracked changes on context B. Risk of duplicate inserts, lost updates, stale reads, and broken transaction semantics.
- **Recommended fix:** Change to `ServiceLifetime.Scoped` for both arguments. Audit `OnStartup` and any place that does not create an explicit `IServiceScope`: in WPF the natural scope is a "user task" — a window's lifetime or a single command execution. The existing pattern at `App.xaml.cs:41-45` (`ServiceProvider.CreateScope()`) is the right shape; apply it everywhere a service is resolved outside a scope. Verify that scoped services held by transient ViewModels do not leak across windows.
- **Complexity:** S (mechanical change, plus a careful review of scope boundaries in ViewModels/Navigation).
- **Phase:** Immediate.

---

#### F-02 — Hardcoded connection string with `TrustServerCertificate=True`

- **Severity:** Critical
- **Layer:** Infrastructure / Config
- **Location:** `App.xaml.cs:55-57` *(Verified)*; fallback in `Data/FinalLabDbContextFactory.cs:21-50` *(Reported)*
- **Root cause:** No `appsettings.json` exists in the project (verified: `ls *.json` returns nothing). The connection string `Server=.\\SQLEXPRESS;Database=FinalLab;Trusted_Connection=True;TrustServerCertificate=True;` is embedded in source. `TrustServerCertificate=True` disables TLS certificate validation between the client and SQL Server — acceptable for a single-machine SQL Express install on the same box, dangerous on any networked deployment.
- **Recommended fix:** Add `appsettings.json` (template, no secrets) + `appsettings.Development.json` (gitignored). Wire `Microsoft.Extensions.Configuration.Json` and read `ConnectionStrings:DefaultConnection`. Remove the hardcoded fallback in both `App.xaml.cs` and `FinalLabDbContextFactory.cs`. For production deployments, set `Encrypt=True; TrustServerCertificate=False` and provision a valid SQL Server certificate.
- **Complexity:** S.
- **Phase:** Immediate.

---

#### F-03 — No logging framework anywhere in the codebase

- **Severity:** Critical
- **Layer:** Cross-cutting
- **Location:** N/A — absent. No `ILogger<T>` injections, no `Serilog`/`NLog` references in `FinalLabSystem.csproj`. *(Verified — csproj has only EF, EF Tools, and DI packages.)*
- **Root cause:** Logging was never set up. Exception-handling sites either `throw` or call `MessageBox.Show(ex.Message)`. Production failures will be invisible; debugging will rely on user screenshots of dialog boxes.
- **Recommended fix:** Add `Microsoft.Extensions.Logging` + `Serilog.Extensions.Logging` + `Serilog.Sinks.File`. Configure rolling-file sink in `App.xaml.cs`. Register `services.AddLogging(...)`. Inject `ILogger<T>` into all services. Wire `Application.DispatcherUnhandledException` and `AppDomain.CurrentDomain.UnhandledException` to log fatal errors before the app dies. Define structured log events for: login, permission denials, data modifications (patient/visit/payment), service exceptions.
- **Complexity:** M.
- **Phase:** Immediate.

---

#### F-04 — Zero automated tests

- **Severity:** Critical
- **Layer:** Cross-cutting / Quality
- **Location:** Solution-wide. `FinalLabSystem.sln` contains exactly one project; no `*Tests.csproj`. *(Verified)*
- **Root cause:** No test project ever created. With services taking a concrete `FinalLabDbContext`, services are also hard to test in isolation — no repository or specification layer.
- **Recommended fix:** Add `FinalLabSystem.Tests` (xUnit). Start with the highest-blast-radius logic: `AuthService` (login + permission + first-admin), `FinancialService` (discount, payment, balance), `VisitService` (multi-table transaction). Use EF Core's `Microsoft.EntityFrameworkCore.InMemory` or SQLite-in-memory for service-level tests; reserve a real SQL Express for an integration test project later. Aim for ~60% coverage on critical paths before considering further refactors.
- **Complexity:** XL (the first 20% of coverage is M; reaching 60% is L–XL).
- **Phase:** Immediate (start) → Medium-term (achieve coverage).

---

#### F-05 — No CI / build validation

- **Severity:** Critical
- **Layer:** Cross-cutting / Process
- **Location:** No `.github/workflows/`, no `azure-pipelines.yml`. *(Verified — `ls -la` of root shows none.)*
- **Root cause:** Process gap, not code gap. PRs are not built or tested by an automated system. Schema-breaking changes can land on `main` undetected.
- **Recommended fix:** Add a GitHub Actions workflow that runs `dotnet restore` + `dotnet build -c Release` + `dotnet ef migrations script --idempotent` (catches broken migrations) on every PR. After tests exist (F-04), add `dotnet test`. Add `TreatWarningsAsErrors=true` once the warning baseline is clean.
- **Complexity:** S (workflow file is small) — but only useful after F-04 is started.
- **Phase:** Immediate (build-only) → Short-term (test on PR).

---

### HIGH

---

#### F-06 — No input validation on entities

- **Severity:** High
- **Layer:** Domain / Data
- **Location:** All entities under `Models/`. *(Reported)*
- **Root cause:** No data-annotation attributes (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`) on any model. Constraints exist only at the database level (Fluent API in `FinalLabDbContext.cs`). The UI cannot pre-validate; users learn about violations from raw SQL exception messages.
- **Recommended fix:** Choose one pattern and apply consistently: either (a) data annotations on entities + `INotifyDataErrorInfo` in ViewModels (already used in `FirstRunSetupViewModel:11`) or (b) `FluentValidation` validators per entity. Option (a) is lower-cost given the existing precedent. Apply to `Patient`, `Visit`, `TestType`, `Staff` first.
- **Complexity:** L.
- **Phase:** Short-term.

---

#### F-07 — `MessageBox.Show` called directly inside ViewModels

- **Severity:** High
- **Layer:** UI / MVVM
- **Location:** *(Verified — 13 sites in production code, not 7 as originally reported.)*
  - `PatientRegistrationViewModel.cs:169,176,243,280,326`
  - `FinancialViewModel.cs:283`
  - `TestDataManagementViewModel.cs:148,172`
  - `TestDetailViewModel.cs:402`
  - `NormalRangeListViewModel.cs:143`
  - `NormalRangeDetailViewModel.cs:286,292`
  - `CategoriesGroupsViewModel.cs:125`
- **Root cause:** ViewModels hold a hard reference to a WPF UI primitive, defeating the testability goal of MVVM. Errors, confirmations, and validation prompts are mixed at the same call site. The `Docs/test_data_implementation_report.md:53` notes this was a deliberate workaround "because the existing project does not expose a reusable generic validation dialog service."
- **Recommended fix:** Introduce `IDialogService` with `ShowMessage`, `ShowError`, `ShowConfirmation`, `ConfirmAsync` methods; register singleton; inject into the affected ViewModels. Concrete implementation wraps `MessageBox.Show`. Unblocks unit-testing those ViewModels and gives one place to swap for a custom modal later (as the docs envision).
- **Complexity:** S–M (interface + implementation + replace 13 call sites). Higher than original estimate due to verified site count.
- **Phase:** Short-term.

---

#### F-08 — `async void` event handlers (3 sites)

- **Severity:** High
- **Layer:** UI / MVVM
- **Location:** *(Verified — 3 production sites in ViewModels, not 1 as originally reported. Two more are framework-required signatures and excluded.)*
  - `CategoriesGroupsViewModel.cs:167` — `OnSelectedCategoryChanged` (originally reported)
  - `TestDataManagementViewModel.cs:160` — `OnSelectedTestChanged` (newly found)
  - `TestDataManagementViewModel.cs:168` — `OnOpenNormalRangesRequested` (newly found)
  - *Excluded — framework signatures, intentional:* `App.xaml.cs:23` (`OnStartup` override), `AsyncRelayCommand.cs:49,115` (ICommand.Execute adapter).
- **Root cause:** `async void` event handlers do not propagate exceptions to a `Task`; an unhandled exception will reach the dispatcher and terminate the app. All three handlers are wired to user-triggered events (grid selection, command request) that fire often. The first two are selection-driven and reload from the DB, so any transient connection failure crashes the app.
- **Recommended fix:** For each handler, either (a) wrap the body in `try/catch` and route to the dialog/error service (F-07) + the future logger (F-03), or (b) refactor the selection-driven reload into an `AsyncRelayCommand` triggered by the view. Apply the same fix-pattern to all three sites.
- **Complexity:** XS per site, S total.
- **Phase:** Immediate.

---

#### F-09 — `AuditService` is defined but unregistered AND uncalled

- **Severity:** High *(severity stands — the gap is slightly worse than originally reported)*
- **Layer:** Security / Compliance / DI
- **Location:** *(Verified by grep `IAuditService|AuditService` across the project.)*
  - `Services/Interfaces/IAuditService.cs:7` — interface defined.
  - `Services/Implementations/AuditService.cs:12-16` — implementation defined.
  - **Zero references elsewhere in production code.** Not injected into any other service. Not registered in `App.xaml.cs` DI block (lines 53-118 of `App.xaml.cs` register `IAuthService`, `IPatientService`, `IReferralService`, `IVisitService`, `IFinancialService`, `ITestCatalogService`, `ISampleTrackingService` — but not `IAuditService`).
  - Documentation references in `Docs/Patient_Window_Context.md:2661-2807` and `Docs/FinalLab_ServiceLayer_MasterPlan.md:214` describe the intended contract, but the wiring was never completed.
  - Database triggers on `TestResult` (`TR_TestResult_AuditInsert`, `TR_TestResult_AuditUpdate`) do log result changes, so the audit trail for **results** is intact via the DB. Everything else (Patient identity edits, Visit changes, Payments, Staff/permission changes) is unaudited.
- **Root cause:** The service was scaffolded but never integrated. Even if a service tried to inject `IAuditService`, the DI container would throw at resolution.
- **Recommended fix:** Two parts:
  1. **Register** `services.AddScoped<IAuditService, AuditService>();` in `App.xaml.cs`.
  2. **Wire** auditing into the write paths. Prefer overriding `SaveChangesAsync` in `FinalLabDbContext` to walk `ChangeTracker.Entries()` and emit `AuditLog` rows for any entity marked with an `[Auditable]` attribute. This avoids per-call discipline. Use `ICurrentUserSession.CurrentUser?.StaffId` for the actor; fall back to a system actor for unauthenticated paths (migrations, seed).
- **Complexity:** M.
- **Phase:** Short-term.

---

#### F-10 — Unbounded queries on patient / hierarchy reads

- **Severity:** High
- **Layer:** Data / Performance
- **Location:**
  - `PatientService.SearchPatientsAsync` returns all matches (no `.Take(N)` on the searched branch). `PatientService.cs:124-135` *(Reported)*
  - `TestCatalogService.GetFullHierarchyAsync` loads all categories → groups → types with no filter. `TestCatalogService.cs:24-31` *(Reported)*
  - `PatientService.GetPatientByIdAsync` eager-loads `Visits → VisitTests → TestType` — unbounded by patient's history.
- **Root cause:** No pagination contract in the service interfaces. The current dataset size masks the problem.
- **Recommended fix:** Add `PagedResult<T>` (Items + TotalCount + Page + PageSize). Update `ISearchPatients`, `IGetVisits`, list-style methods to take page parameters. Cap `Search` at e.g. 100. For patient detail, defer visits to a separate call with paging.
- **Complexity:** M.
- **Phase:** Short-term.

---

#### F-11 — No session timeout / idle lockout

- **Severity:** High
- **Layer:** Security
- **Location:** `Infrastructure/Session/CurrentUserSession.cs` *(Reported)*
- **Root cause:** `CurrentUserSession.SignIn` sets the user; no idle-timeout, no automatic lockout. A shared lab workstation left unattended remains authenticated.
- **Recommended fix:** Add an idle detector (`InputManager.PreProcessInput` or a `DispatcherTimer` reset on input). After N minutes of inactivity, call `SignOut` and route to `LoginWindow` via `INavigationService`. Make N configurable in lab settings.
- **Complexity:** S.
- **Phase:** Short-term.

---

#### F-12 — `CurrentUserSession` singleton is not thread-safe

- **Severity:** High
- **Layer:** Infrastructure / Concurrency
- **Location:** `Infrastructure/Session/CurrentUserSession.cs` *(Reported)*
- **Root cause:** Plain field reads/writes. WPF apps are mostly single-threaded on the UI thread, but async continuations and `Task.Run` (e.g., future logging or background sync) can run on the threadpool. Reading `CurrentUser` from non-UI threads risks torn reads.
- **Recommended fix:** Mark the backing field `volatile` or wrap with `Interlocked.Exchange`/`lock`. Document the threading model.
- **Complexity:** XS.
- **Phase:** Short-term.

---

### MEDIUM

---

#### F-13 — Code-behind business logic in `TodayPatientsDialog`

- **Severity:** Medium
- **Layer:** UI / MVVM
- **Location:** `TodayPatientsDialog.xaml.cs:30-72` *(Reported)*
- **Root cause:** Filter logic (`FilterPatient`), search reactivity, selection logic all sit in code-behind. Dialog is not bound to a ViewModel.
- **Recommended fix:** Create `TodayPatientsDialogViewModel` with `ObservableCollection<TodayPatientDto>` + `ICollectionView` for filtering + `SelectedPatient` + `SelectCommand`/`CancelCommand`. Keep only `InitializeComponent` and `DialogResult` set in code-behind.
- **Complexity:** S.
- **Phase:** Short-term.

---

#### F-14 — Mouse-event handler in `TestSelectionView` code-behind

- **Severity:** Medium
- **Layer:** UI / MVVM
- **Location:** `TestSelectionView.xaml.cs:13-17` *(Reported)*
- **Root cause:** Double-click on the available-tests list calls `AddTestCommand.Execute` from code-behind instead of XAML interaction.
- **Recommended fix:** Replace with `Microsoft.Xaml.Behaviors.Wpf` `InvokeCommandAction` on a `MouseDoubleClick` event trigger. Adds one NuGet but removes the code-behind handler.
- **Complexity:** XS.
- **Phase:** Short-term.

---

#### F-15 — Boolean column proliferation on `Visit`

- **Severity:** Medium
- **Layer:** Domain / Data
- **Location:** `Models/Visit.cs` — 16 medical-history booleans + 6 outside-sample booleans. *(Reported)*
- **Root cause:** Each new medical condition or outside-sample type requires a schema change. Migrations `AddVisitMedicalHistoryAndOutsideSamples` (20260605000200) and successors testify to this.
- **Recommended fix:** Normalize into `VisitMedicalFlag(VisitId, FlagCode, IsSet, Note)` and `VisitOutsideSample(VisitId, SampleTypeCode)`. Maintain backward-compat read views during transition. **Note:** because most flags are individually queried, this is a meaningful schema change — weigh against immediate UI cost.
- **Complexity:** L.
- **Phase:** Medium-term.

---

#### F-16 — Boolean / flag proliferation on `TestType`

- **Severity:** Medium
- **Layer:** Domain / Data
- **Location:** `Models/TestType.cs` — `IsRoutineTest`, `SeeReport`, `PrintWithOther`, `AddWithGroup`, `IsMainTest`, `IsSendOutside`, etc. *(Reported)*
- **Root cause:** Multiple closely related flags packed into one entity; semantics overlap (`IsSendOutside` vs `IsOutsourceable`).
- **Recommended fix:** Group into a `[Flags] enum TestTypeBehavior` stored as int — or split into `TestTypeReporting`, `TestTypeBilling` config tables. Document which flags are user-visible vs. system-internal. Lower-priority than F-15 because all values are well-defined.
- **Complexity:** M.
- **Phase:** Medium-term.

---

#### F-17 — Type inconsistency in financial / age fields

- **Severity:** Medium
- **Layer:** Domain
- **Location:** *(Reported)*
  - `TestType.DefaultPrice` is `double`, `TestType.OutsideCostPrice` is `decimal`.
  - `Visit.DiscountPercent` is `double`.
  - `Patient.ApproxAge` is `double`.
  - `VisitTest.PriceCharged` is `double`.
- **Root cause:** No type policy enforced. Currency on `double` is a known accounting hazard (binary-floating-point rounding).
- **Recommended fix:** Standardize: `decimal` for any currency, `int`/`byte` for counts and ages, `double` only for measured-quantity test values. Add a migration that converts the columns (lossless for currency since current values are small). Update model + Fluent API column types.
- **Complexity:** M.
- **Phase:** Short-term.

---

#### F-18 — String-typed state machines (`VisitStatus`, `PaymentStatus`, `CurrentStage`, etc.)

- **Severity:** Medium
- **Layer:** Domain
- **Location:** `Visit.VisitStatus`, `Visit.PaymentStatus`, `VisitTest.CurrentStage`, `Payment.PaymentMethod`. *(Reported)*
- **Root cause:** Status values stored as strings (`"OPEN"`, `"CLOSED"`, `"PENDING"`, `"CASH"`). No compile-time enforcement; typos in queries silently match nothing.
- **Recommended fix:** Define enums and use EF Core value converters (`HasConversion<string>()`) to keep DB representation stable. Map invalid values to a parse-fail at boundary.
- **Complexity:** S.
- **Phase:** Short-term.

---

#### F-19 — `TestResult` lacks a validation status

- **Severity:** Medium
- **Layer:** Domain / Workflow
- **Location:** `Models/TestResult.cs` *(Reported)*
- **Root cause:** Result has `ResultValue` (string), `ResultNumeric` (double), normal-range snapshot, and `EnteredBy`/`LastModifiedBy` — but no field representing "entered / reviewed / validated / released". A doctor's signoff step is invisible to the system.
- **Recommended fix:** Add `ValidationStatus` (enum) + `ValidatedBy` (FK Staff) + `ValidatedAt`. Update result-entry workflow to drive transitions. Consider also adding a derived `IsCritical` based on `Snap*` bounds for fast-path alerting.
- **Complexity:** M.
- **Phase:** Short-term.

---

#### F-20 — Dual storage of result values (`ResultValue` string + `ResultNumeric` double)

- **Severity:** Medium
- **Layer:** Domain
- **Location:** `Models/TestResult.cs` *(Reported)*
- **Root cause:** Two columns hold the same logical datum; nothing enforces they agree. A buggy entry path could populate `ResultValue` without `ResultNumeric`, breaking range-flag computations.
- **Recommended fix:** Decide: either keep both with a database CHECK / domain invariant that ties them, or drop one and compute the other on demand. If numeric tests dominate, store `ResultNumeric` and a separate `ResultText` for qualitative-only tests; let a computed property surface the unified string.
- **Complexity:** S–M (depending on path-2 vs path-1).
- **Phase:** Medium-term.

---

#### F-21 — Specialized test entities use three optional 1:1 FKs on `VisitTest`

- **Severity:** Medium
- **Layer:** Domain / Data
- **Location:** `VisitTest.CrossMatchTest?`, `MicrobiologyCulture?`, `SemenAnalysis?` — only one populated per visit-test. *(Reported)*
- **Root cause:** No TPT/TPC inheritance; specialized data scattered across optional siblings. Three NULL FKs per row in the dominant case.
- **Recommended fix:** Defer unless a concrete need emerges (e.g., new specialized test type). If you do refactor, prefer TPT (Table-Per-Type) keyed off `VisitTestId`. Low immediate value, real cost.
- **Complexity:** L.
- **Phase:** Medium-term (or never — document the decision).

---

#### F-22 — Fire-and-forget async initialization in 6 sites

- **Severity:** Medium
- **Layer:** UI / Async correctness
- **Location:** *(Verified — exactly 6 sites in production ViewModels.)*
  - **Constructor calls (5 sites):**
    - `PatientRegistrationViewModel.cs:67` — `_ = AddNewAsync();`
    - `PatientInfoViewModel.cs:33` — `_ = LoadTitlesAsync();`
    - `ReferralViewModel.cs:24` — `_ = LoadInitialAsync();`
    - `TestSelectionViewModel.cs:29` — `_ = LoadAsync();`
    - `TestDataManagementViewModel.cs:40` — `_ = InitializeAsync();`
  - **Setter-triggered call (1 site, separate sub-pattern):**
    - `NormalRangeListViewModel.cs:39` — `_ = RefreshRangesAsync();` inside `SelectedComponent.set`. Reloads ranges when the user changes the selected component. Same exception-swallowing risk, fires on every selection change.
- **Root cause:** Constructors and setters discard the returned `Task`. If `LoadAsync` throws, the exception is unobserved: UI silently shows empty state, no log, no user feedback. The constructor pattern means a startup-time DB failure is invisible.
- **Recommended fix:**
  - **For the 5 constructor sites:** expose a public `InitializeAsync` method, drop the fire-and-forget, and call it from the view's `Loaded` event with `try/catch` → `IDialogService.ShowError` (F-07) + `ILogger` (F-03).
  - **For the 1 setter site (`NormalRangeListViewModel:39`):** wrap the body in a private `async Task RunRefreshAsync()` with try/catch, then keep the fire-and-forget *only* with logged exceptions — there is no good place to await a setter.
- **Complexity:** S (small per site, 6 sites).
- **Phase:** Short-term.

---

#### F-23 — No global exception handler

- **Severity:** Medium
- **Layer:** Cross-cutting
- **Location:** `App.xaml.cs:23-51` *(Verified — `OnStartup` has no try/catch and no `DispatcherUnhandledException` subscription.)*
- **Root cause:** An exception during startup, a database connection failure, or an unobserved task exception terminates the app silently or with the .NET crash dialog.
- **Recommended fix:** Subscribe to `Application.DispatcherUnhandledException`, `TaskScheduler.UnobservedTaskException`, `AppDomain.CurrentDomain.UnhandledException`. Log + show a user-friendly dialog + decide whether to keep the app alive. Wrap `OnStartup` body in `try/catch` and show a clear startup-failure window.
- **Complexity:** S.
- **Phase:** Immediate.

---

#### F-24 — Snapshot fields on `TestResult` drift from `NormalRange`

- **Severity:** Medium
- **Layer:** Domain
- **Location:** `TestResult.Snap*` columns *(Reported)*
- **Root cause:** Snapshots capture range bounds at result-entry time — correct for audit trail. But if `NormalRange` is updated later (e.g., method change), there is no link back to the rule version that was applied.
- **Recommended fix:** Add `NormalRangeId` + `NormalRangeVersion` (or hash) to `TestResult`. Treat ranges as immutable + versioned (insert new row, mark old `IsActive=false`) to make audit reproducible.
- **Complexity:** M.
- **Phase:** Medium-term.

---

#### F-25 — Two competing outsourcing models

- **Severity:** Medium
- **Layer:** Domain
- **Location:** `VisitTest.IsOutsourced`/`ExternalLabId`/`OutsourceCost`/`OutsourceSentAt` vs. `ExternalShipment → ExternalShipmentItem → VisitTest`. *(Reported)*
- **Root cause:** Inline outsourcing fields on `VisitTest` predate the shipment model. Both can be populated; neither is authoritative.
- **Recommended fix:** Pick one. Shipment-based is richer (multi-test per shipment, tracking number, costs at shipment level). Migrate the legacy fields into shipment rows; deprecate the inline columns. Keep a read view for backward compat during transition.
- **Complexity:** M.
- **Phase:** Medium-term.

---

### LOW

---

#### F-26 — No README.md at root

- **Severity:** Low
- **Layer:** Documentation
- **Location:** Repo root *(Verified — no README present.)*
- **Root cause:** Functional docs exist in Arabic under `Docs/`, but a developer landing on the repo has no orientation: how to set up SQL Express, how to run migrations, where the entry point is.
- **Recommended fix:** Add a short bilingual README with: prerequisites, `dotnet ef database update` command, run-from-IDE instructions, link to the `Docs/` folder, screenshots of main UI states.
- **Complexity:** XS.
- **Phase:** Short-term.

---

#### F-27 — Sparse XML documentation comments

- **Severity:** Low
- **Layer:** Documentation
- **Location:** `Services/Interfaces/*.cs`, `Services/Implementations/*.cs` *(Reported)*
- **Root cause:** No `/// <summary>` blocks on service interfaces; intent must be inferred from method names.
- **Recommended fix:** Document the 18 service interfaces (the contract surface), not the implementations. Enable `GenerateDocumentationFile=true` once compliance is reasonable, to surface warnings on undocumented public APIs.
- **Complexity:** L (broad surface, but each method is small).
- **Phase:** Medium-term.

---

#### F-28 — `AsyncRelayCommand.Execute` uses `async void`

- **Severity:** Low
- **Layer:** Infrastructure
- **Location:** `Infrastructure/AsyncRelayCommand.cs:49` *(Reported)*
- **Root cause:** `ICommand.Execute` is a sync signature; `async void` is the standard adapter pattern. Internally wrapped in `try/catch` that raises `ErrorOccurred`. Risk is only that consumers must subscribe — silent failures otherwise.
- **Recommended fix:** Document the contract requirement to subscribe to `ErrorOccurred`. Or migrate to `CommunityToolkit.Mvvm`'s `AsyncRelayCommand` which handles the same shape with a published pattern (also unblocks `IAsyncRelayCommand`).
- **Complexity:** XS (doc) / S (migration).
- **Phase:** Medium-term.

---

#### F-29 — Print logic in `PrintPreviewWindow` code-behind

- **Severity:** Low
- **Layer:** UI / MVVM
- **Location:** `PrintPreviewWindow.xaml.cs:18-23` *(Reported)*
- **Root cause:** Click handler opens `PrintDialog` directly. This is pragmatic — print is inherently UI-bound.
- **Recommended fix:** Accept as-is, or expose an `IPrintService` if printing is needed from multiple places.
- **Complexity:** XS.
- **Phase:** Medium-term (or won't-fix).

---

#### F-30 — `ApproxAge` typed as `double`

- **Severity:** Low
- **Layer:** Domain
- **Location:** `Models/Patient.cs` *(Reported)*
- **Root cause:** Ages are expressed as integers with a separate `ApproxAgeUnit` (days/months/years). `double` introduces fractional semantics no caller uses.
- **Recommended fix:** Convert to `int`. Migration is lossless for any existing whole-number values; round on the way in for non-whole.
- **Complexity:** XS.
- **Phase:** Medium-term.

---

### NEW FINDINGS (surfaced during the re-verification pass)

---

#### F-31 — `NormalRangesWindow` binds to a `Unit` property that does not exist

- **Severity:** High
- **Layer:** UI / Data Binding
- **Location:** *(Verified by reading both files end-to-end.)*
  - `Views/Settings/NormalRangesWindow.xaml:89` — DataGrid column "Test unit" binds `{Binding Unit}` on each row item (a `NormalRange` entity). `NormalRange` has no `Unit` property.
  - `Views/Settings/NormalRangesWindow.xaml:215-217` — Reference Setting form has a ComboBox "Test unit" binding `{Binding Detail.Unit, UpdateSourceTrigger=PropertyChanged}`. `NormalRangeDetailViewModel.cs` (read in full) exposes properties for `AgeUnit`, `LowNormal`, `HighNormal`, `LowFlag`, `HighFlag`, `LowComment`, `HighComment`, `NormalRangeText`, etc. — but **no `Unit` property**.
- **Root cause:** The "test unit" concept lives on `TestComponent.Unit`, not on `NormalRange` or the detail VM. The XAML form lets the user pick a unit per range (mg/dL, U/L, etc.) but the value has nowhere to go. WPF binding failures are silent unless `PresentationTraceSources.TraceLevel=High` is set; the user picks a value, sees no error, saves, the value vanishes.
- **Recommended fix:** Decide which design is correct:
  - (a) **Range carries its own unit.** Add `Unit` to `NormalRange` (column + EF config + migration + property on `NormalRangeDetailViewModel`). Likely the intended design since the form is explicitly soliciting input.
  - (b) **Unit belongs to the component.** Remove the unit controls from the range form; show the component's unit read-only.
  - Lab convention often supports per-range units (e.g., glucose mg/dL vs mmol/L), so (a) is the safer bet.
- **Complexity:** M.
- **Phase:** Short-term (silent data loss in production-facing form).

---

#### F-32 — Dead UI controls in Test Data Management & Normal Ranges windows

- **Severity:** Medium
- **Layer:** UI
- **Location:** *(Verified.)*
  - `Views/Settings/TestDataManagementWindow.xaml:289` — `<CheckBox Content="Patient Question" ... />` has no `IsChecked` binding. The textarea below it (`TestDetail.PatientQuestion`) is always editable regardless of checkbox state. The checkbox does nothing.
  - `Views/Settings/NormalRangesWindow.xaml:281` — `<Button Content="تعديل" Style="..."/>` has no `Command` binding. It's a styled button with no behavior on click.
- **Root cause:** UI was sketched faster than VM commands/properties caught up. The recent commits ("UI 100% complete" on these windows) reflect visual completeness, not functional completeness for these specific controls.
- **Recommended fix:** Either delete or wire up:
  - For the Patient Question checkbox: decide whether it gates `IsEnabled` / `Visibility` on the textarea, or remove.
  - For the "تعديل" button: the rest of the form is already editable. Either remove the button or define what "edit mode" means in this context (e.g., enter-vs-browse states like in `TestDataManagementViewModel`).
- **Complexity:** XS each, S total.
- **Phase:** Short-term.

---

#### F-33 — `NormalRangeDetailViewModel` `RangeFor` radio-button group is broken

- **Severity:** High (data-integrity)
- **Layer:** UI / MVVM
- **Location:** *(Verified.)*
  - `ViewModels/Settings/NormalRangeDetailViewModel.cs:69-91` defines four properties — `IsRangeForAll`, `IsRangeForSexAndAge`, `IsRangeForSexOnly`, `IsRangeForAgeOnly` — over a three-value enum `NormalRangeFor { Female, Male, Both }`.
  - `Views/Settings/NormalRangesWindow.xaml:118-121` binds four radio buttons to these properties.
  - **Bug #1 (duplicate states):** `IsRangeForAll` and `IsRangeForAgeOnly` both read/write `NormalRangeFor.Both` — identical behavior. The "For all" and "By age only" radio buttons are mechanically the same option.
  - **Bug #2 (wrong semantics):** `IsRangeForSexAndAge` maps to `NormalRangeFor.Female`; `IsRangeForSexOnly` maps to `NormalRangeFor.Male`. "By sex and age" and "by sex only" are orthogonal to whether the sex is male or female — the mapping is nonsensical.
  - **Bug #3 (unreachable Both sex):** The form exposes "Male" / "Female" radio buttons (`IsSexMale`, `IsSexFemale`) but no "Both" radio button, even though `IsSexBoth` exists on the VM. A user creating a new range cannot pick "Both" through the UI; the saved `EditableRange.Sex` will be whatever was last clicked.
  - **Net effect:** When saving (`NormalRangeDetailViewModel.cs:296-302`), `Sex` is written as `"M"`, `"F"`, or `"Both"` based on the unreachable `Sex` value. New ranges will never persist `"Both"` from this UI — but the very first radio button ("For all") suggests they will.
- **Root cause:** The enum + property model wasn't reconciled with the four-button XAML layout.
- **Recommended fix:** Pick one design and apply it end-to-end:
  - **Option A (3 buttons, match enum):** Drop `IsRangeForAgeOnly` from XAML; keep `IsRangeForAll = Both`, rename `IsRangeForSexAndAge` to `IsRangeForFemale`, `IsRangeForSexOnly` to `IsRangeForMale`. Add a third sex radio button bound to `IsSexBoth`.
  - **Option B (split into two independent concerns):** Replace `RangeFor` with two booleans — `AppliesBySex` and `AppliesByAge` — wired to two checkboxes. Independently pick `Sex` (M/F/B) and the age range. This matches what the XAML labels seem to be reaching for.
- **Complexity:** S (UI + VM mapping; no schema change).
- **Phase:** Short-term.

---

## 5. Remediation Roadmap

Three phases. Each finding is placed once; the table is the master plan.

### Phase I — Immediate (before next production deployment)

Goal: stop the bleeding. Anything in this phase represents data-integrity, security, or operability risk that compounds with every day of delay.

| ID | Title | Complexity | Notes |
|---|---|---|---|
| F-01 | DbContext lifetime → Scoped | S | Highest-impact single change; verify scope wiring |
| F-02 | Externalize connection string | S | Pair with F-01; both touch `App.xaml.cs` |
| F-03 | Add logging framework | M | Unlocks observability for everything below |
| F-04 | Test infrastructure (bootstrap only) | M | One xUnit project + ~5 sanity tests on `AuthService`; expand later |
| F-05 | CI: build-on-PR | S | Simple `dotnet build` workflow; add tests once F-04 exists |
| F-08 | Fix `async void` event handler | XS | Quick win; pair with F-23 |
| F-23 | Global exception handler | S | Pair with F-03 — both go in `App.xaml.cs` |

**Phase I estimated effort:** ~1.5–2 weeks of focused work for one engineer.

---

### Phase S — Short-term (next 1–2 sprints)

Goal: close the high-risk gaps that don't block tomorrow's deploy but will hurt in production.

| ID | Title | Complexity |
|---|---|---|
| F-06 | Input validation on entities | L |
| F-07 | `IDialogService` to replace MessageBox-in-VM | S |
| F-09 | Wire `AuditService` (prefer `SaveChangesAsync` override) | M |
| F-10 | Paging on search / list services | M |
| F-11 | Idle session timeout | S |
| F-12 | `CurrentUserSession` thread-safety | XS |
| F-13 | `TodayPatientsDialog` → ViewModel | S |
| F-14 | `TestSelectionView` double-click → behavior | XS |
| F-17 | Standardize money on `decimal`, age on `int` | M |
| F-18 | String-typed status fields → enums + value converters | S |
| F-19 | `TestResult.ValidationStatus` workflow | M |
| F-22 | Replace fire-and-forget init with `InitializeAsync` on `Loaded` | S |
| F-26 | README.md | XS |
| F-31 | Add `Unit` to `NormalRange` (or remove unit controls) | M |
| F-32 | Wire or remove dead controls (Patient-Question checkbox; "تعديل" button) | XS |
| F-33 | Fix `RangeFor` radio-button group + add Both-sex option | S |
| F-04 | Test coverage growth (continuation) | L |
| F-05 | CI: run tests on PR (continuation) | S |

**Phase S estimated effort:** ~4–6 weeks.

---

### Phase M — Medium-term (next quarter)

Goal: structural improvements that pay off over time but are safe to schedule against feature work.

| ID | Title | Complexity |
|---|---|---|
| F-15 | Normalize `Visit` medical-history flags | L |
| F-16 | Group `TestType` behavior flags into enum/config | M |
| F-20 | Reconcile `ResultValue` ↔ `ResultNumeric` | S–M |
| F-21 | Decide on TPT for specialized tests (or document defer) | L |
| F-24 | Version `NormalRange` for audit-trail integrity | M |
| F-25 | Consolidate outsourcing model on `ExternalShipment` | M |
| F-27 | XML docs on service interfaces | L |
| F-28 | Replace local `AsyncRelayCommand` with CommunityToolkit | S |
| F-29 | `IPrintService` (or accept as-is) | XS |
| F-30 | `ApproxAge` → `int` | XS |

**Phase M estimated effort:** opportunistic; folded into normal feature work.

---

## 6. Verification Approach

When findings are implemented, each can be verified as follows. (This section is the test plan for the remediation, not for the audit itself.)

### Per-finding verification

- **F-01** — Add a service-level test: resolve `IPatientService` and `IVisitService` from the same scope, modify a shared `Patient`, call `SaveChanges` via one — assert the other sees the change. Pre-fix: this fails. Post-fix: passes.
- **F-02** — Run the app with a deliberately wrong `appsettings.json`; expect a clear startup error pointing at the connection string, not a generic SQL exception.
- **F-03** — Tail the rolling log file while exercising login + a deliberate exception path; assert log entries with correct structured fields.
- **F-04 / F-05** — `dotnet test` from CLI; CI workflow goes green on PR.
- **F-06** — Submit a `Patient` form with required fields empty; assert `INotifyDataErrorInfo` surfaces the error, save command is disabled, no DB call is made.
- **F-07** — Mock `IDialogService`; assert that error paths in `PatientRegistrationViewModel` call `ShowError`.
- **F-09** — Modify a patient via UI; query `AuditLog` and assert an entry with the actor's `StaffId`, the entity name, the column-level diff, and a timestamp.
- **F-10** — Generate 1,000 test patients; assert search returns ≤100 rows with `HasMore=true`.
- **F-11** — Sign in, wait the configured idle interval, assert window returns to `LoginWindow`.
- **F-17** — Compute a discount on a price that exercises floating-point rounding (e.g., 0.1 × 0.2); assert decimal preserves precision.
- **F-23** — Throw a deliberate unhandled exception from a background task; assert it's logged and the app remains alive.

### Cross-cutting verification (end-to-end)

Once Phase I + the bulk of Phase S are done, exercise the golden path end-to-end:

1. First-run setup → admin created.
2. Login → main shell.
3. Register a patient (Arabic name) → barcode → save.
4. Open a visit → add 3 tests (one routine, one micro, one semen) → record payment.
5. Enter results for the routine test → validate → release.
6. Reopen patient history → assert all artifacts present.
7. Sign out → idle timeout → re-login.
8. Check `AuditLog` for actor entries on every mutating step.
9. Check the rolling log file for matching structured events.

If all of those work, Phase I + S are effectively delivered.

---

## 7. Critical Files (reference for any later implementation)

For any future remediation, these are the load-bearing files:

- `App.xaml.cs` — DI composition root; touched by F-01, F-02, F-03, F-23.
- `Data/FinalLabDbContext.cs` — 2053 lines; entity config + relationships; touched by F-09 (SaveChanges override) and any schema change.
- `Data/FinalLabDbContextFactory.cs` — design-time factory; touched by F-02.
- `Infrastructure/ViewModelBase.cs` — base for all 27 ViewModels; do not break this.
- `Infrastructure/AsyncRelayCommand.cs` — touched by F-28.
- `Infrastructure/Navigation/NavigationService.cs` — keep its pattern when adding new windows.
- `Infrastructure/Security/PasswordHasher.cs` — do not modify; algorithm + iteration count are correct.
- `Infrastructure/Session/CurrentUserSession.cs` — touched by F-11, F-12.
- `Models/Visit.cs` — touched by F-15, F-17, F-18.
- `Models/TestType.cs` — touched by F-16, F-17.
- `Models/TestResult.cs` — touched by F-19, F-20, F-24.
- `Models/Patient.cs` — touched by F-17, F-30, F-06.
- `Services/Implementations/PatientService.cs` — touched by F-09, F-10.
- `Services/Implementations/AuditService.cs` — touched by F-09 (becomes a real participant).
- `Services/Implementations/AuthService.cs` — first test target for F-04.
- `ViewModels/Settings/CategoriesGroupsViewModel.cs` — touched by F-08.
- `Views/Patients/TodayPatientsDialog.xaml.cs` — touched by F-13.
- `Views/Patients/TestSelectionView.xaml.cs` — touched by F-14.

---

## 8. Reusable patterns already in the codebase

Before introducing new abstractions during remediation, prefer to reuse what already exists:

- **MVVM base** — `Infrastructure/ViewModelBase.cs` with `SetProperty<T>`. Use for any new ViewModel.
- **Commands** — `Infrastructure/RelayCommand.cs`, `AsyncRelayCommand.cs`. Subscribe to `ErrorOccurred`.
- **Validation** — `INotifyDataErrorInfo` precedent in `FirstRunSetupViewModel:11,21,107-176`. Reuse for F-06.
- **Navigation** — `Infrastructure/Navigation/NavigationService.cs`. Use `RegisterWindow<TVM,TWin>` for new windows.
- **Transactions** — `VisitService.CreateVisitAsync`, `TestCatalogService.CreateTestTypeAsync` show the right `BeginTransactionAsync` shape.
- **Scoped DbContext usage** — `App.xaml.cs:41-45` (`ServiceProvider.CreateScope()`) shows the correct pattern to repeat after F-01.
- **Password handling** — `PasswordHasher.cs` + `PasswordBoxHelper`. Do not invent alternatives.
- **Shared styles** — `Views/Shared/SharedStyles.xaml`. New windows merge it via `App.xaml`.

---

## 9. Corrections to the underlying audit pass

Two items surfaced by the Explore agents were re-verified and **disproved** during this audit. Recording them here so future readers don't act on them:

- **Claim:** `FinalLabSystem/FinalLabSystem.csproj.user` is tracked in git despite `.gitignore`.
  **Reality:** *Not tracked.* `git ls-files | grep csproj.user` returns nothing (exit 1). `git check-ignore -v` confirms it's ignored by `.gitignore:12` (`*.user`). The file exists on disk locally but never entered the index. No action needed.

- **Claim:** `AsyncRelayCommand` is "implemented locally; consider CommunityToolkit.Mvvm" was flagged as a concern.
  **Reality:** Lowered to F-28 (Low). The local implementation is correct; replacing it is a stylistic preference, not a defect.

---

## 10. What this audit did NOT cover

For honesty, things deliberately out of scope:

- **Database performance under load** — no query profiling, no `SHOWPLAN`, no index utilization analysis on real data. F-10 is based on code inspection.
- **UI/UX quality** — no usability review, no accessibility audit (RTL was checked only as a technical setting).
- **Arabic copy correctness** — UI strings were treated as opaque.
- **Migrations applied vs. authored** — I did not connect to SQL Server to verify schema drift between migrations and the live DB.
- **Third-party security** — only NuGet packages already referenced were considered; no SCA / vulnerability scan was run.

These are appropriate follow-ups if the team wants a deeper second-pass audit.

---

## 11. UI Completeness Tables

Two recent-work windows enumerated control-by-control, with each binding traced to a backing ViewModel property or command. **Legend:** ✓ Bound (resolves to a real property/command) · ⚠ Partial (bound but with caveat) · ❌ Broken (binding path does not resolve to any member) · ⛔ Unbound (no binding declared — control is decorative or dead).

> Both tables were produced by reading the XAML files and the backing ViewModel(s) end-to-end in this session. Issues surfaced here are the source of findings F-31, F-32, F-33.

### 11.1 `TestDataManagementWindow.xaml`

Backing ViewModel: `TestDataManagementViewModel` (composes `TestList: TestListViewModel`, `TestDetail: TestDetailViewModel`). DataGrid rows are `TestRowViewModel`.

#### Row 0 — Search bar

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 69 | TextBox `SearchTestNameBox` | `TestList.SearchTestName` | `TestListViewModel.SearchTestName` | ✓ |
| 73 | TextBox `SearchGroupNameBox` | `TestList.SearchGroupName` | `TestListViewModel.SearchGroupName` | ✓ |
| 77 | TextBox `SearchTestIdBox` | `TestList.SearchTestId` | `TestListViewModel.SearchTestId` | ✓ |
| 83 | TextBlock (count) | `TestList.FilteredTests.Count` (StringFormat) | `TestListViewModel.FilteredTests` | ✓ |

#### Row 1 — DataGrid

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 90-91 | DataGrid `TestsDataGrid` | `ItemsSource=TestList.FilteredTests`, `SelectedItem=TestList.SelectedTest` | `TestListViewModel` | ✓ |
| 102 | Col "ID" | `TesttypeId` | `TestRowViewModel.TesttypeId` | ✓ |
| 103 | Col "Arrange" | `SortOrder` | `TestRowViewModel.SortOrder` | ✓ |
| 104 | Col "Group Name" | `GroupNameAr` | `TestRowViewModel.GroupNameAr` | ✓ |
| 105 | Col "Test Name" | `TypeNameEn` | `TestRowViewModel.TypeNameEn` | ✓ |
| 106 | Col "Pat. P" | `PatientPrice` | `TestRowViewModel.PatientPrice` (derived) | ✓ |
| 107 | Col "Lab P" | `LabToLabPrice` | `TestRowViewModel.LabToLabPrice` (derived) | ✓ |
| 108 | Col "Out Lab Name" | `OutsideLabName` | `TestRowViewModel.OutsideLabName` | ✓ |
| 109 | Col "Out P" | `OutsideCostPrice` | `TestRowViewModel.OutsideCostPrice` | ✓ |
| 110 | Col "Barcode" | `Barcode` | `TestRowViewModel.Barcode` | ✓ |

#### Row 3 — Test Information form (left column)

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 139 | TextBox "Test Name" | `TestDetail.TypeNameEn` | `TestDetailViewModel.TypeNameEn` | ✓ |
| 142 | TextBox "Test Code" | `TestDetail.TypeCode` | `TestDetailViewModel.TypeCode` | ✓ |
| 153 | TextBox "Report Name" | `TestDetail.ReportNameLine1` | `TestDetailViewModel.ReportNameLine1` | ✓ |
| 164 | TextBox "Bill Name" | `TestDetail.BillNameLine1` | `TestDetailViewModel.BillNameLine1` | ✓ |
| 175 | TextBox "History Name" | `TestDetail.HistoryName` | `TestDetailViewModel.HistoryName` | ✓ |
| 186 | TextBox "Arabic Name" | `TestDetail.TypeNameAr` | `TestDetailViewModel.TypeNameAr` | ✓ |
| 197-201 | ComboBox "Group Name" | `ItemsSource=TestDetail.Groups`, `SelectedValue=TestDetail.GroupId` | `TestDetailViewModel.Groups` / `GroupId` | ✓ |
| 211-215 | ComboBox "Collection" | `ItemsSource=TestDetail.CollectionTypes`, `SelectedValue=TestDetail.SelectedCollectionTypeId` | `TestDetailViewModel.CollectionTypes` / `SelectedCollectionTypeId` | ✓ |
| 221 | CheckBox "See Report" | `TestDetail.SeeReport` | `TestDetailViewModel.SeeReport` | ✓ |
| 222 | CheckBox "Print with other" | `TestDetail.PrintWithOther` | `TestDetailViewModel.PrintWithOther` | ✓ |
| 223 | CheckBox "Add with group" | `TestDetail.AddWithGroup` | `TestDetailViewModel.AddWithGroup` | ✓ |
| 224 | CheckBox "Main test" | `TestDetail.IsMainTest` | `TestDetailViewModel.IsMainTest` | ✓ |

> **Missing from this column vs. the VM surface:** `TestDetail.IsRoutineTest` exists on `TestDetailViewModel` but no XAML control binds to it. Not a defect — just an unused property. Worth verifying intent.

#### Row 3 — Right column (Barcode + Outside + Patient Question)

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 243 | TextBox "Name" (barcode) | `TestDetail.BarcodeName` | `TestDetailViewModel.BarcodeName` | ✓ |
| 246-248 | ComboBox "Tube 2" | `ItemsSource=TestDetail.AvailableTubeTypes`, `SelectedItem=TestDetail.Tube2` | ✓ | ✓ |
| 258-260 | ComboBox "Tube 1" | same / `Tube1` | ✓ | ✓ |
| 262-264 | ComboBox "Tube 3" | same / `Tube3` | ✓ | ✓ |
| 270 | CheckBox "Sent outside Lab." | `TestDetail.IsSendOutside` | `TestDetailViewModel.IsSendOutside` | ✓ |
| 279-281 | TextBox "Lab Name" | `TestDetail.OutsideLabName` + `IsEnabled=TestDetail.IsOutsideFieldsEnabled` | ✓ | ✓ |
| 283-285 | TextBox "Cost price" | `TestDetail.OutsideCostPrice` + `IsEnabled=TestDetail.IsOutsideFieldsEnabled` | ✓ | ✓ |
| **289** | **CheckBox "Patient Question"** | *(no `IsChecked` binding)* | — | **⛔ DEAD — see F-32** |
| 290 | TextBox (Patient Question area) | `TestDetail.PatientQuestion` | `TestDetailViewModel.PatientQuestion` | ✓ |

#### Row 4 — Prices / Timing strip

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 313 | TextBox "Test time (Day)" | `TestDetail.TurnaroundHours` | `TestDetailViewModel.TurnaroundHours` | ✓ |
| 316 | TextBox "Arrange No" | `TestDetail.SortOrder` | `TestDetailViewModel.SortOrder` | ✓ |
| **319-321** | **ComboBox "Reference type"** | `Text=TestDetail.ReferenceType` (`IsEditable=True`, **no ItemsSource**) | `TestDetailViewModel.ReferenceType` | **⚠ Bound but no choices — see F-32 note below** |
| 323 | TextBox "Patient price" | `TestDetail.PatientPrice` | `TestDetailViewModel.PatientPrice` | ✓ |
| 326 | TextBox "Lab to lab price" | `TestDetail.LabToLabPrice` | `TestDetailViewModel.LabToLabPrice` | ✓ |

> The "Reference type" ComboBox is `IsEditable=True` with no `ItemsSource`. Bound to `ReferenceType` (string), so the value flows correctly — but the user can only type free-form text; no curated choices are offered. Either intentional (free-text) or an unfinished ItemsSource. Lower priority than F-32's dead controls; noting here, not raising as a separate finding.

#### Row 5 — Button bar

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 334-336 | Button "إضافة تحليل" | `AddCommand` + `Visibility=IsBrowsing` | `TestDataManagementViewModel.AddCommand` / `IsBrowsing` | ✓ |
| 337-339 | Button "تعديل" | `EditCommand` + `Visibility=IsBrowsing` | `TestDataManagementViewModel.EditCommand` | ✓ |
| 340 | Button "حفظ" | `SaveCommand` | `TestDataManagementViewModel.SaveCommand` | ✓ |
| 342 | Button "تراجع" | `CancelCommand` | `TestDataManagementViewModel.CancelCommand` | ✓ |
| 344 | Button "حذف" | `DeleteCommand` | `TestDataManagementViewModel.DeleteCommand` | ✓ |
| 346 | Button "القيم المرجعية" | `TestDetail.OpenNormalRangesCommand` | `TestDetailViewModel.OpenNormalRangesCommand` | ✓ |
| 348 | Button "القائمة الرئيسية" | `CloseCommand` | `TestDataManagementViewModel.CloseCommand` | ✓ |

**`TestDataManagementWindow.xaml` summary:** 47 controls audited; 45 ✓ Bound · 1 ⚠ Partial (Reference-type ComboBox without ItemsSource) · 1 ⛔ Dead (Patient-Question checkbox) · 0 ❌ Broken.

---

### 11.2 `NormalRangesWindow.xaml`

Backing ViewModel: `NormalRangeWindowViewModel` (composes `List: NormalRangeListViewModel`, `Detail: NormalRangeDetailViewModel`). DataGrid rows are `NormalRange` entities.

#### Row 0 — Header

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 4 | Window `Title` | `Title` | `NormalRangeWindowViewModel.Title` | ✓ |
| 60 | TextBlock (test name) | `ParentTest.TypeNameEn` | `NormalRangeWindowViewModel.ParentTest.TypeNameEn` | ✓ |
| 65 | TextBlock (ref count) | `ReferenceCount` (StringFormat) | `NormalRangeWindowViewModel.ReferenceCount` | ✓ |

#### Row 1 — DataGrid

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 72-73 | DataGrid | `ItemsSource=List.RangesForSelectedComponent`, `SelectedItem=List.SelectedRange` | `NormalRangeListViewModel` | ✓ |
| 82 | Col "Gender" | `Sex` | `NormalRange.Sex` | ✓ |
| 83 | Col "From" | `AgeFromDays` | `NormalRange.AgeFromDays` | ✓ |
| 84 | Col "To" | `AgeToDays` | `NormalRange.AgeToDays` | ✓ |
| 85 | Col "Age unit" | `AgeUnit` | `NormalRange.AgeUnit` | ✓ |
| 86 | Col "Reference range" | `NormalRangeText` | `NormalRange.NormalRangeText` | ✓ |
| 87 | Col "low limit" | `LowNormal` | `NormalRange.LowNormal` | ✓ |
| 88 | Col "high limit" | `HighNormal` | `NormalRange.HighNormal` | ✓ |
| **89** | **Col "Test unit"** | **`Unit`** | **none — no `Unit` member on `NormalRange`** | **❌ BROKEN — see F-31** |
| 90 | Col "Low flag" | `LowFlag` | `NormalRange.LowFlag` | ✓ |
| 91 | Col "High flag" | `HighFlag` | `NormalRange.HighFlag` | ✓ |

#### Row 2 — Reference Setting form (Range-for + Sex + Age)

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 118 | RadioButton "For all" | `Detail.IsRangeForAll` | `NormalRangeDetailViewModel.IsRangeForAll` (→ `RangeFor=Both`) | ⚠ See F-33 |
| 119 | RadioButton "By sex and age" | `Detail.IsRangeForSexAndAge` | `IsRangeForSexAndAge` (→ `RangeFor=Female`, wrong semantics) | ⚠ See F-33 |
| 120 | RadioButton "By sex only" | `Detail.IsRangeForSexOnly` | `IsRangeForSexOnly` (→ `RangeFor=Male`, wrong semantics) | ⚠ See F-33 |
| 121 | RadioButton "By age only" | `Detail.IsRangeForAgeOnly` | `IsRangeForAgeOnly` (→ `RangeFor=Both`, **duplicates "For all"**) | ⚠ See F-33 |
| 128 | RadioButton "Male" | `Detail.IsSexMale` | `NormalRangeDetailViewModel.IsSexMale` | ✓ |
| 129 | RadioButton "Female" | `Detail.IsSexFemale` | `NormalRangeDetailViewModel.IsSexFemale` | ✓ |
| **missing** | (no "Both" radio button) | — | `IsSexBoth` exists on VM, **no XAML control** | **⛔ See F-33** |
| 133 | TextBox "Age from" | `Detail.AgeFromDays` | `NormalRangeDetailViewModel.AgeFromDays` | ✓ |
| 136 | TextBox "Age to" | `Detail.AgeToDays` | `NormalRangeDetailViewModel.AgeToDays` | ✓ |
| 139-141 | ComboBox "Age unit" | `ItemsSource=Detail.AgeUnitOptions`, `SelectedItem=Detail.AgeUnit` | ✓ | ✓ |

#### Row 2 — Reference Setting form (Range + Limits + Flags)

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 163 | TextBox "Reference range" | `Detail.NormalRangeText` | `NormalRangeDetailViewModel.NormalRangeText` | ✓ |
| 178 | TextBox "Low limit" | `Detail.LowNormal` | `NormalRangeDetailViewModel.LowNormal` | ✓ |
| 181 | TextBox "High limit" | `Detail.HighNormal` | `NormalRangeDetailViewModel.HighNormal` | ✓ |
| 192-194 | ComboBox "Low flag" | `Text=Detail.LowFlag` (editable, hard-coded items L/LL/Low) | `NormalRangeDetailViewModel.LowFlag` | ✓ |
| 200-202 | ComboBox "High flag" | `Text=Detail.HighFlag` (editable, hard-coded items H/HH/High) | `NormalRangeDetailViewModel.HighFlag` | ✓ |
| **215-217** | **ComboBox "Test unit"** | **`Text=Detail.Unit`** | **none — no `Unit` property on `NormalRangeDetailViewModel`** | **❌ BROKEN — see F-31** |
| 228 | CheckBox "For pregnant only" | `Detail.ForPregnantOnly` | `NormalRangeDetailViewModel.ForPregnantOnly` | ✓ |
| 243 | TextBox "Low comment" | `Detail.LowComment` | `NormalRangeDetailViewModel.LowComment` | ✓ |
| 259 | TextBox "High comment" | `Detail.HighComment` | `NormalRangeDetailViewModel.HighComment` | ✓ |

> **Properties exposed on `NormalRangeDetailViewModel` not bound in this window:** `LowCritical`, `HighCritical`, `AgeDescription`, `RangeNote`, `CriticalRangeText`, `CriticalFlag`, `CriticalComment`, `IsFastingAny`, `IsFasting`. These exist on the VM (and the underlying `NormalRange` entity has the columns thanks to migration `AddNormalRangeFlagsAndCritical`) but are not surfaced to the user in this window. Not a defect — could be deliberate scope reduction — but worth confirming since the schema went through the trouble of adding them.

#### Row 3 — Button bar

| Line | Control | Binding | Backing | Status |
|---|---|---|---|---|
| 272-274 | Button "قائمة التحاليل" | `BackToTestsCommand` (with window param) | `NormalRangeWindowViewModel.BackToTestsCommand` | ✓ |
| 275 | Button "حذف" | `DeleteRangeCommand` | `NormalRangeListViewModel.DeleteRangeCommand` (via passthrough) | ✓ |
| 277 | Button "تراجع" | `CancelCommand` | `NormalRangeDetailViewModel.CancelCommand` (via passthrough) | ✓ |
| 279 | Button "حفظ" | `SaveCommand` | `NormalRangeDetailViewModel.SaveCommand` (via passthrough) | ✓ |
| **281** | **Button "تعديل"** | *(no `Command` binding)* | — | **⛔ DEAD — see F-32** |
| 282 | Button "إضافة مدى" | `AddRangeCommand` | `NormalRangeListViewModel.AddRangeCommand` (via passthrough) | ✓ |

**`NormalRangesWindow.xaml` summary:** 27 controls audited; 22 ✓ Bound · 4 ⚠ Partial (radio buttons broken by F-33) · 2 ❌ Broken (`Unit` bindings) · 1 ⛔ Dead ("تعديل" button) · 1 missing control (Both-sex radio button absent).

---

## 12. Re-verification Trace

This section records what was re-checked, with what commands/reads, against the original *Reported* findings. Anyone acting on the plan can confirm the verification themselves with the same commands.

### 12.1 F-07 (MessageBox in ViewModels)

- **Method:** `Grep -n MessageBox\.Show` across `FinalLabSystem/`.
- **Original claim:** 7 sites, in PatientRegistrationVM (×5), CategoriesGroupsVM (×1), TestDataManagementVM (×1).
- **Verified result:** **13 sites in production code** (excluding 3 doc-file mentions). Original count undercounted by 6.
- **All sites:** see F-07 location list. The doc reference in `Docs/test_data_implementation_report.md:53` confirms the pattern was knowingly chosen as a temporary workaround.

### 12.2 F-08 (`async void` event handlers)

- **Method:** `Grep -n "async void"` across `FinalLabSystem/`.
- **Original claim:** 1 site (`CategoriesGroupsViewModel:167`).
- **Verified result:** **3 sites in ViewModels** + 2 framework signatures that are not defects:
  - **Defect:** `CategoriesGroupsViewModel.cs:167` — `OnSelectedCategoryChanged`
  - **Defect (new):** `TestDataManagementViewModel.cs:160` — `OnSelectedTestChanged`
  - **Defect (new):** `TestDataManagementViewModel.cs:168` — `OnOpenNormalRangesRequested`
  - **Excluded:** `App.xaml.cs:23` — `protected override async void OnStartup` (framework signature).
  - **Excluded:** `AsyncRelayCommand.cs:49,115` — `public async void Execute(object?)` (ICommand contract).

### 12.3 F-09 (AuditLog unused)

- **Method:** `Grep -n "_auditService|IAuditService|AuditService"` across `FinalLabSystem/`.
- **Original claim:** "Method exists, not called from PatientService/VisitService/FinancialService."
- **Verified result:** **Worse than reported.** No reference outside the declaring files themselves:
  - `Services/Interfaces/IAuditService.cs:7` — interface
  - `Services/Implementations/AuditService.cs:12,16` — class + constructor
  - Doc mentions in `Docs/Patient_Window_Context.md` and `Docs/FinalLab_ServiceLayer_MasterPlan.md`
  - **Not registered** in `App.xaml.cs:53-118`. If any service tried `ctor(IAuditService a)`, DI would throw `InvalidOperationException` at startup.

### 12.4 F-22 (Fire-and-forget initialization)

- **Method:** `Grep -n "_ = \w+Async\(\)|_ = Load|_ = Refresh|_ = Initialize"` across `FinalLabSystem/`.
- **Original claim:** 5 sites in ViewModel constructors (+ 1 maybe at `NormalRangeListViewModel:140`).
- **Verified result:** **6 sites total**, exactly matching the broader claim:
  - 5 in constructors: `PatientRegistrationViewModel:67`, `PatientInfoViewModel:33`, `ReferralViewModel:24`, `TestSelectionViewModel:29`, `TestDataManagementViewModel:40`.
  - 1 in a setter: `NormalRangeListViewModel:39` (`SelectedComponent.set` → `_ = RefreshRangesAsync();`).
  - The setter case is a slightly different sub-pattern (selection-change reload) and gets its own recommended fix in F-22.

### 12.5 Net effect on the audit

| Finding | Original severity | After re-verification | Change |
|---|---|---|---|
| F-07 | High | High | Site count revised up (7 → 13); recommended-fix scope grows |
| F-08 | High | High | Site count revised up (1 → 3); same recommendation, more sites |
| F-09 | High | High | Gap is worse than reported (also unregistered in DI); fix gains a step |
| F-22 | Medium | Medium | Confirmed exactly as broader claim; sub-pattern documented |
| F-31 | — | **High (new)** | Surfaced by UI scan |
| F-32 | — | **Medium (new)** | Surfaced by UI scan |
| F-33 | — | **High (new)** | Surfaced by VM re-read |

The first four are now ready to act on with confidence. The three new findings extend the scope of "Short-term" work but do not change Phase I.

---

*End of audit.*

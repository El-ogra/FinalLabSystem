# MiMo V2.5 — Context & Knowledge Base

**Last updated:** 2026-06-20
**Purpose:** Living document for AI assistants working on FinalLabSystem. Every fact here is verified. Update this file after every feature, fix, or lesson learned.

---

## 1. Project Overview

- **Name:** FinalLabSystem
- **Stack:** WPF (.NET 8) / EF Core 8 / SQL Server / MVVM
- **Language:** Arabic-first UI (RTL)
- **Repo root:** `C:\Users\LAP LINK\source\repos\FinalLabSystem`
- **Solution file:** `FinalLabSystem.sln`
- **Main project:** `FinalLabSystem/FinalLabSystem.csproj`
- **Test project:** `FinalLabSystem.Tests/FinalLabSystem.Tests.csproj`

---

## 2. Directory Structure

```
FinalLabSystem/
├── App.xaml.cs                  ← DI composition root (services registered here)
├── Data/
│   └── FinalLabDbContext.cs     ← EF Core DbContext (~2200 lines, Fluent API)
├── Infrastructure/
│   ├── ViewModelBase.cs         ← Base class for ALL ViewModels (INotifyPropertyChanged + INotifyDataErrorInfo)
│   ├── AsyncRelayCommand.cs     ← ICommand implementation for async commands
│   ├── Session/
│   │   └── CurrentUserSession.cs ← ICurrentUserSession interface + CurrentUserSession implementation
│   └── Navigation/              ← NavigationService for window management
├── Models/
│   ├── Patient.cs, Visit.cs, Staff.cs, TestType.cs, TestGroup.cs, etc.
│   ├── ReceiptPrintLog.cs       ← Audit entity for receipt printing
│   └── DTOs/
│       ├── VisitFullDto.cs      ← Full visit data DTO
│       └── SelectedTestDto.cs   ← Test selection DTO (includes GroupId, GroupName)
├── Services/
│   ├── Interfaces/              ← 23 service interfaces
│   └── Implementations/         ← 25 service implementations
├── ViewModels/
│   ├── Patients/
│   │   ├── PatientRegistrationViewModel.cs  ← Main patient window VM
│   │   ├── ReceiptDialogViewModel.cs        ← Receipt dialog VM (DI-injected)
│   │   ├── BarcodeDialogViewModel.cs        ← Barcode dialog VM
│   │   ├── FinancialViewModel.cs
│   │   └── ...
│   └── Settings/                ← Test catalog, normal ranges VMs
├── Views/
│   ├── Patients/
│   │   ├── PatientRegistrationWindow.xaml    ← Main patient window
│   │   ├── ReceiptDialog.xaml               ← Receipt popup dialog
│   │   ├── ReceiptDialog.xaml.cs            ← Code-behind (DI constructor only)
│   │   ├── BarcodeDialog.xaml               ← Barcode popup dialog
│   │   └── PrintPreviewWindow.xaml          ← Shared print preview
│   └── Settings/
├── Migrations/                  ← EF Core migrations
└── Docs/                        ← Project documentation

FinalLabSystem.Tests/
├── Services/
│   ├── AuthServiceTests.cs
│   ├── ReceiptServiceTests.cs   ← 13 tests for ReceiptService
│   └── ...
├── ViewModels/
│   └── ReceiptDialogViewModelTests.cs  ← 14 tests for ReceiptDialogViewModel
└── Validation/
    └── EntityValidationTests.cs
```

---

## 3. Key Architectural Decisions

### 3.1 MVVM Pattern (Non-Negotiable)

- Every ViewModel inherits `ViewModelBase` (`Infrastructure/ViewModelBase.cs`).
- No `MessageBox.Show` in ViewModels — use `IDialogService`.
- No `async void` outside framework-required signatures.
- No fire-and-forget `_ = SomethingAsync()` in constructors. Use `InitializeAsync()` pattern instead.
- Code-behind (`.xaml.cs`) contains only `InitializeComponent()` and constructor that accepts ViewModel via DI.

### 3.2 Dependency Injection

- DI composition root: `App.xaml.cs` (lines ~53-118).
- Services registered with `AddScoped<IService, Service>()`.
- ViewModels resolved via `App.ServiceProvider.GetRequiredService<T>()`.
- Dialog windows receive ViewModel as constructor parameter.

### 3.3 Service Layer Pattern

- Each feature has its own service interface in `Services/Interfaces/`.
- Each implementation is in `Services/Implementations/`.
- Services inject `FinalLabDbContext` and `ILogger<T>` (or other services).
- Services return DTOs (not entities) to ViewModels.

### 3.4 Entity Pattern

- Entities are `partial class` (not `sealed`) — allows `virtual` navigation properties.
- DbContext uses Fluent API configuration (not data annotations) for most constraints.
- New entities must be registered as `DbSet<T>` in `FinalLabDbContext.cs`.
- New entity requires new EF Core migration.

---

## 4. Testing Rules (STRICT)

### 4.1 Test Framework

- **Framework:** xUnit 2.5.3
- **Mocking:** Moq 4.20.70
- **In-memory DB:** Microsoft.EntityFrameworkCore.InMemory 8.0.0
- **Target:** `net8.0-windows` (but `UseWPF` is `false` in test project — cannot test WPF UI directly)

### 4.2 Test Quality Requirements

- **Tests MUST be real** — test actual business logic, not mocks of mocks.
- **Tests MUST be comprehensive** — cover happy path, edge cases, and failure scenarios.
- **Tests MUST cover both Service layer AND ViewModel layer** for each feature.
- **Tests MUST NOT be weak** — verify actual outcomes, not just that methods were called.
- **No fake/mock-only tests** — use InMemory database for data access tests, Moq only for external dependencies.

### 4.3 Test Patterns

**Service tests (InMemory DB):**
```csharp
private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
    => new DbContextOptionsBuilder<FinalLabDbContext>()
        .UseInMemoryDatabase(dbName)
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options;
```

**ViewModel tests (Moq):**
```csharp
var mockService = new Mock<IReceiptService>();
var mockSession = new Mock<ICurrentUserSession>();
var mockDialog = new Mock<IDialogService>();
// Setup mocks, create ViewModel, call methods, assert outcomes.
```

### 4.4 After Every Feature/Fix

1. Write unit tests for the new/changed Service.
2. Write unit tests for the new/changed ViewModel.
3. Run `dotnet test --no-restore -v n` — all tests must pass.
4. Run `dotnet build --no-restore -v q` — 0 Warnings, 0 Errors.
5. Update this file with any lessons learned.

---

## 5. Feature: Receipt Printing

### 5.1 Files Involved

| File | Purpose |
|---|---|
| `Models/ReceiptPrintLog.cs` | Audit entity (LogId, VisitId, StaffId, PrintedAt, Format, ShowBreakdown, financial snapshot) |
| `Data/FinalLabDbContext.cs` | `DbSet<ReceiptPrintLog>` + Fluent API config (lines ~162, 2191-2235) |
| `Migrations/20260619213822_AddReceiptPrintLog.cs` | EF Core migration |
| `Services/Interfaces/IReceiptService.cs` | Interface + `ReceiptGroupedTest` DTO |
| `Services/Implementations/ReceiptService.cs` | Print-once logic, grouping logic, audit logging |
| `Models/DTOs/SelectedTestDto.cs` | Added `GroupId`, `GroupName` properties |
| `Services/Implementations/VisitService.cs` | `GetVisitFullDataAsync` includes `.ThenInclude(t => t.Group)`, `BillNameLine1`, `BillNameLine2`, `GroupId`, `GroupName` (lines 234-322) |
| `ViewModels/Patients/ReceiptDialogViewModel.cs` | Full ViewModel with DI, format selection, document builders |
| `Views/Patients/ReceiptDialog.xaml` | UI: format selector, breakdown toggle, patient info, print button |
| `Views/Patients/ReceiptDialog.xaml.cs` | Code-behind (DI constructor only) |
| `ViewModels/Patients/PatientRegistrationViewModel.cs` | `ReceiptAsync()` method triggers receipt flow (lines 339-354) |
| `App.xaml.cs` | `IReceiptService` registration (line 146) |

### 5.2 Receipt Printing Flow

1. User clicks "ايصال" (Receipt) button in `PatientRegistrationWindow`.
2. `PatientRegistrationViewModel.ReceiptAsync()` is called.
3. It calls `_visitService.GetVisitFullDataAsync(CurrentVisitId)` to get full visit data.
4. It resolves `ReceiptDialogViewModel` from DI.
5. It calls `viewModel.InitializeAsync(dto)` which:
   - Sets `VisitData` property.
   - Calls `_receiptService.CanPrintReceiptAsync(visitId, staffId)`.
   - Calls `_receiptService.GetGroupedTestsForReceiptAsync(visitId)`.
6. If `CanPrint` is false → shows warning "تم طباعة الإيصال مسبقاً لهذا الحالة المالية." and returns.
7. If `CanPrint` is true → opens `ReceiptDialog` as a modal dialog.
8. User selects format (A4 or Thermal), toggles breakdown, clicks Print.
9. `ReceiptDialogViewModel.PrintAsync()` builds the `FlowDocument` and opens `PrintPreviewWindow`.
10. After print, `LogPrintEventAsync` is called with financial snapshot.
11. `CanPrint` is re-evaluated after printing.

### 5.3 Print-Once Policy

- **Admin staff** (`Staff.IsAdmin == true`): Can always print (unlimited reprints).
- **Regular staff**: Can only print once per financial state. If the visit's financial data (Subtotal, Discount, TotalAfterDiscount, TotalPaid, BalanceDue) changes, they can print again.
- The comparison is: last log's financial fields vs current visit's financial fields.

### 5.4 Test Grouping Rule

- If ALL active tests in a `TestGroup` are present in the visit → summarized single line (e.g., "وظائف كلى (3) — Creatinine, BUN, Uric Acid").
- If only SOME tests from a group are present → individual lines per test.
- System does NOT track how tests were added (profile vs individual) — grouping is result-based.

### 5.5 Receipt Formats

- **A4:** Full-width document with patient info, breakdown table, financial summary.
- **Thermal:** Narrow-width document (`ColumnWidth = 280`) with Courier New font, separator lines.
- Both use `FlowDocument` + `PrintDialog` (user selects printer).
- No barcode in any receipt format.

---

## 6. Feature: Barcode Printing

### 6.1 Files Involved

| File | Purpose |
|---|---|
| `ViewModels/Patients/BarcodeDialogViewModel.cs` | ViewModel for barcode dialog |
| `Views/Patients/BarcodeDialog.xaml` | UI for barcode dialog |
| `Views/Patients/BarcodeDialog.xaml.cs` | Code-behind (DI constructor only) |

### 6.2 Pattern

Same as receipt: dialog has its own ViewModel, resolved from DI, passed to the dialog constructor. The barcode dialog is triggered from `PatientRegistrationViewModel` via a command.

---

## 7. Build & Test Commands

```bash
# Build (0 warnings, 0 errors required)
dotnet build --no-restore -v q

# Run all tests
dotnet test --no-restore -v n

# Run specific tests
dotnet test --filter "ReceiptServiceTests|ReceiptDialogViewModelTests" --no-restore -v n
```

---

## 8. Lessons Learned

### 8.1 Guid Format Interpolation Bug

**Problem:** `$"T_{Guid.NewGuid():N[..6]}"` throws `FormatException` at runtime. The `:N` format specifier for Guid only accepts D, N, P, B, X — not slicing syntax.

**Fix:** Use `$"T_{Guid.NewGuid().ToString("N")[..6]}"` — call `.ToString("N")` first, then slice the resulting string.

**Verified:** 2026-06-20, ReceiptServiceTests.

### 8.2 WPF Types in Tests

**Problem:** Test project has `UseWPF = false`. Cannot test code that uses WPF types (`FlowDocument`, `PrintDialog`, `Application.Current`, `CommandManager`).

**Solution:** Test business logic separately from UI. `BuildA4Document` and `BuildThermalDocument` are `static` methods that can be tested without WPF runtime if they don't access `Application.Current`. Properties like `CanPrint` use `CommandManager.InvalidateRequerySuggested()` which is WPF-specific — mock or ignore in tests.

### 8.3 `ICurrentUserSession` Location

**Namespace:** `FinalLabSystem.Infrastructure.Session`
**Interface:** `ICurrentUserSession` — property `Staff? CurrentUser { get; }`
**Implementation:** `CurrentUserSession` (thread-safe singleton with lock)

### 8.4 `Patient` Entity Has No `IsActive` Property

The `Patient` entity does NOT have an `IsActive` field. Do not set it in test data factories.

### 8.5 `Paragraph` Constructor Limitation

WPF `Paragraph` constructor only accepts a single `Inline`. For multiple inlines, use `para.Inlines.Add()`. This affects receipt document builders.

### 8.6 `FindAsync` Returns Nullable

`await ctx.Visits.FindAsync(id)` returns `Visit?`. Always null-check before accessing properties. Use `Assert.NotNull()` in tests before dereferencing.

---

## 9. Critical Files — Do Not Break

| File | Why |
|---|---|
| `Infrastructure/ViewModelBase.cs` | Base class for all 27+ ViewModels. Public surface: `OnPropertyChanged`, `SetProperty<T>`, `AddError`, `ClearErrors`. |
| `Infrastructure/Session/CurrentUserSession.cs` | Thread-safe singleton session. Do not change lock pattern. |
| `Data/FinalLabDbContext.cs` | ~2200 lines of Fluent API. Never edit a committed migration — always create new one. |
| `App.xaml.cs` | DI composition root. Check service lifetimes before adding new registrations. |
| `Infrastructure/Security/PasswordHasher.cs` | PBKDF2-SHA256, 100k iterations. Do not change algorithm or iteration count — breaks all existing passwords. |

---

## 10. Pre-existing Issues (Not From Our Changes)

These issues existed before receipt printing was implemented:

- `Decimal` precision warnings on `ContractInvoice`/`Payment` entities (build warnings, not errors).
- F-31: `NormalRangesWindow` binds to non-existent `Unit` property.
- F-32: Dead UI controls (Patient Question checkbox, "تعديل" button).
- F-33: `NormalRangeDetailViewModel` radio-button group broken.

---

## 11. Future Work Guidelines

When implementing a new feature:

1. **Plan first** — define files, interfaces, and test strategy before coding.
2. **Service layer** — create `I{Name}Service` and `{Name}Service`.
3. **ViewModel** — create `{Name}ViewModel` inheriting `ViewModelBase`.
4. **View** — create `{Name}.xaml` + `{Name}.xaml.cs` (DI constructor only).
5. **DI registration** — register service and ViewModel in `App.xaml.cs`.
6. **Tests** — write Service tests (InMemory DB) and ViewModel tests (Moq) BEFORE marking feature complete.
7. **Build check** — `dotnet build --no-restore -v q` must yield 0 Warnings, 0 Errors.
8. **Test check** — `dotnet test --no-restore -v n` must pass all tests.
9. **Update this file** — add any new lessons, patterns, or verified facts.

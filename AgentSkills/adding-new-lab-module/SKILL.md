---
name: adding-new-lab-module
description: Add a brand-new lab module to FinalLabSystem end-to-end (entity → migration → service+interface → ViewModel → View → DI registration → navigation registration → permission → menu entry → tests). Trigger on any request to add a new domain area (e.g. "add a Vaccination module", "track Equipment maintenance"). Do NOT use for adding a single field to an existing entity, fixing a bug in an existing screen, or service-only refactors — those use the relevant targeted skill (ef-core-migration-safety, wpf-mvvm-conventions, di-and-navigation-registration). Stops the moment a step is skipped: an entity without a migration, a VM without DI registration, a Window without a navigation entry, or a service without an interface.
---

# Adding a new lab module to FinalLabSystem

A "module" is a coherent lab domain (e.g. Equipment maintenance, Vaccinations, Reagent QC). This skill gives you the exact checklist so nothing is forgotten.

## Why a checklist

The codebase has 50+ entities, 60+ services, 60+ ViewModels, 44 Views, and 30+ migrations. Every module follows the same skeleton; missing one step (typically DI registration or navigation registration) produces a runtime null reference and a hard-to-diagnose crash.

## The 11 steps

For a new module called `Equipment`:

### 1. Entity in `Models/`

```csharp
// FinalLabSystem/Models/Equipment.cs
using FinalLabSystem.Data;
namespace FinalLabSystem.Models;

[Auditable]
public partial class Equipment
{
    public int EquipmentId { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string Status { get; set; } = "Active";
}
```

Always add `[Auditable]` for tables whose rows must appear in audit history. See `audit-logging-conventions` skill.

### 2. View models/DTOs in `Models/DTOs/`

```csharp
// FinalLabSystem/Models/DTOs/EquipmentDto.cs
namespace FinalLabSystem.Models.DTOs;
public record EquipmentDto(int EquipmentId, string Code, string NameAr, string? NameEn, DateOnly PurchaseDate, string Status);
```

DTOs are records, separate from entities. Services return DTOs; ViewModels map to/from DTOs.

### 3. EF migration

```bash
cd FinalLabSystem
dotnet ef migrations add AddEquipment --project FinalLabSystem.csproj --startup-project FinalLabSystem.csproj
dotnet ef migrations script --idempotent -o Migrations/equipment-rollout.sql  # for prod review
```

Never edit the auto-generated migration unless seeding data is needed. See `ef-core-migration-safety` skill.

### 4. DbSet registration in `Data/FinalLabDbContext.cs`

```csharp
public DbSet<Equipment> Equipments => Set<Equipment>();
```

Alphabetize new entries with existing `DbSet` declarations.

### 5. Service interface in `Services/Interfaces/`

```csharp
// FinalLabSystem/Services/Interfaces/IEquipmentService.cs
namespace FinalLabSystem.Services.Interfaces;
public interface IEquipmentService
{
    Task<List<EquipmentDto>> GetAllAsync();
    Task<EquipmentDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(EquipmentDto dto, int staffId);
    Task UpdateAsync(int id, EquipmentDto dto, int staffId);
    Task DeleteAsync(int id, int staffId);
}
```

### 6. Service implementation in `Services/Implementations/`

```csharp
// FinalLabSystem/Services/Implementations/EquipmentService.cs
public sealed class EquipmentService : IEquipmentService
{
    private readonly FinalLabDbContext _db;
    private readonly ICurrentUserSession _session;
    private readonly IAuditService _audit;
    private readonly ILogger<EquipmentService> _log;

    public EquipmentService(FinalLabDbContext db, ICurrentUserSession session,
        IAuditService audit, ILogger<EquipmentService> log)
    { _db = db; _session = session; _audit = audit; _log = log; }

    public async Task<List<EquipmentDto>> GetAllAsync() =>
        await _db.Equipments.AsNoTracking()
            .OrderBy(e => e.Code)
            .Select(e => new EquipmentDto(e.EquipmentId, e.Code, e.NameAr, e.NameEn, e.PurchaseDate, e.Status))
            .ToListAsync();

    public async Task<int> CreateAsync(EquipmentDto dto, int staffId)
    {
        var entity = new Equipment { /* map */ };
        _db.Equipments.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogActionAsync(nameof(Equipment), entity.EquipmentId, "INSERT", staffId);
        return entity.EquipmentId;
    }
    // ... other methods follow same pattern with audit on writes
}
```

Conventions:
- `sealed` class, primary constructors not used (project uses traditional ctors for DI clarity).
- Inject `FinalLabDbContext`, `ICurrentUserSession`, `IAuditService`, `ILogger<T>` — at minimum.
- Every write method calls `IAuditService.LogActionAsync` with action `INSERT|UPDATE|DELETE`.
- Use `AsNoTracking()` for read-only paths.

### 7. ViewModel in `ViewModels/Settings/`

```csharp
// FinalLabSystem/ViewModels/Settings/EquipmentWindowViewModel.cs
public sealed class EquipmentWindowViewModel : ViewModelBase
{
    private readonly IEquipmentService _service;
    public ObservableCollection<EquipmentDto> Items { get; } = new();
    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveAsyncCommand { get; }
    // ... ctors follow wpf-mvvm-conventions
}
```

Follow `wpf-mvvm-conventions` skill for the full skeleton.

### 8. View in `Views/Settings/`

`EquipmentWindow.xaml` and `.xaml.cs` per the View skeleton in `wpf-mvvm-conventions`.

### 9. DI registration in `App.xaml.cs`

In `ConfigureServices`, after the existing service registrations:

```csharp
services.AddScoped<IEquipmentService, EquipmentService>();
services.AddTransient<EquipmentWindowViewModel>();
services.AddTransient<Views.Settings.EquipmentWindow>();
```

The codebase uses `AddScoped` for services (DbContext is scoped) and `AddTransient` for VMs/Windows.

### 10. Navigation registration in `App.xaml.cs`

In the same place as the other `RegisterWindow` calls:

```csharp
navigation.RegisterWindow<EquipmentWindowViewModel, EquipmentWindow>();
```

Without this, calling `OpenTaskWindow<EquipmentWindowViewModel>()` throws at runtime.

### 11. Menu entry

Add a menu item in `MainWindow.xaml` (or the appropriate menu VM) that calls `OpenTaskWindow<EquipmentWindowViewModel>()`. For Settings screens, follow the pattern in `ViewModels/Menu/` (e.g. `AccountsMenuViewModel`).

### 12. Permission code (optional but recommended)

Insert into `Permission` table:

```sql
INSERT INTO Permissions (PermissionCode, PermissionName, PermissionGroup, Description)
VALUES ('EQUIPMENT.MANAGE', 'Equipment Management', 'EQUIPMENT', 'Manage lab equipment records');
```

Then guard the menu item and VM actions with `await _auth.HasPermissionAsync(staffId, "EQUIPMENT.MANAGE")`.

### 13. Tests

Add three test classes following project patterns:

- `Tests/Services/EquipmentServiceRegistrationTests.cs` — asserts `IEquipmentService` resolves from DI.
- `Tests/Services/EquipmentServiceTests.cs` — uses `InMemory` provider, exercises CRUD + audit calls (mock `IAuditService`).
- `Tests/Integration/EquipmentEndToEndTests.cs` — full workflow through the service layer.

See `FinalLabSystem.Tests/` for examples; the `InMemory` provider is configured in `Tests/Helpers/InMemoryDbContextFactory.cs` if present, otherwise construct manually in each test.

## Order matters

Do steps 1 → 6 in this exact order so the project compiles at every step. Steps 7 → 11 require step 9 to compile (the VM constructor depends on the service). Steps 12 → 13 are independent.

## Quick checklist

- [ ] `Models/<Entity>.cs` with `[Auditable]`
- [ ] `Models/DTOs/<Entity>Dto.cs`
- [ ] Migration generated and reviewed
- [ ] `DbSet<<Entity>>` added to `FinalLabDbContext`
- [ ] `Services/Interfaces/I<Entity>Service.cs`
- [ ] `Services/Implementations/<Entity>Service.cs` with audit calls
- [ ] `ViewModels/<Area>/<Screen>ViewModel.cs`
- [ ] `Views/<Area>/<Screen>Window.xaml(.cs)`
- [ ] DI: `AddScoped<I<Entity>Service, ...>`, `AddTransient<...ViewModel>()`, `AddTransient<...Window>()`
- [ ] Navigation: `RegisterWindow<<Screen>ViewModel, <Screen>Window>()`
- [ ] Menu item wired
- [ ] Permission code added (if needed)
- [ ] 3 test classes added
- [ ] Build clean, tests green
# Second-Opinion Audit Report — SqlTransaction Error

**Error:** "This SqlTransaction has completed; it is no longer usable."  
**Trigger:** حفظ (Save) in PatientRegistrationWindow  
**Audited against:** [Report_for_root_cause.md](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Docs/Report_for_root_cause.md)  
**Files independently verified:**
- [PatientRegistrationViewModel.cs](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs)
- [VisitService.cs](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs)
- [FinalLabDbContext.cs](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Data/FinalLabDbContext.cs)
- [App.xaml.cs](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/App.xaml.cs)
- [NavigationService.cs](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Infrastructure/Navigation/NavigationService.cs)

---

## Section 1 — Agreement / Disagreement Summary

### Finding 1: Masked Error (Original Error Lost)

**Verdict: ✅ CONFIRMED**

| Claim | Evidence |
|-------|----------|
| The visible "SqlTransaction completed" error is **secondary**, masking the real error | The catch block at [VisitService.cs:218–222](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L218-L222) catches **all** exceptions with a bare `catch { }`, calls `RollbackAsync()`, and then `throw;` |
| If `RollbackAsync()` itself throws, the **original** exception is permanently lost | Confirmed: when `RollbackAsync()` throws, the new exception propagates. The `throw;` on line 221 is **never reached**. The exception from `RollbackAsync()` replaces the original. |
| User can never see the real error when rollback fails | Confirmed: [PatientRegistrationViewModel.cs:278–280](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs#L278-L280) displays `ex.Message` — which is the rollback exception, not the original |

**Where exactly does the original error get lost:**
1. An exception occurs during one of the `SaveChangesAsync` calls (e.g., line 213)
2. Control enters the `catch` block at line 218
3. `await transaction.RollbackAsync()` at line 220 throws because SQL Server already terminated the transaction (e.g., batch-abort from a trigger error, severity ≥ 16)
4. The new `InvalidOperationException` ("SqlTransaction completed") propagates out, **replacing** the original `DbUpdateException`/`SqlException`
5. `throw;` at line 221 is **dead code** in this scenario

> [!IMPORTANT]
> The error-masking is **guaranteed** whenever the underlying SQL error causes SQL Server to auto-abort the transaction (severity ≥ 16 errors inside triggers do this). The `RollbackAsync()` then operates on a dead transaction and throws.

---

### Finding 2: Double RollbackAsync Risk

**Verdict: ⚠️ PARTIALLY CONFIRMED — but it is NOT the primary issue**

| Claim | Evidence |
|-------|----------|
| `using var transaction` generates a compiler `finally` calling `Dispose()` | Correct. [VisitService.cs:119](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L119): `using var transaction = await _context.Database.BeginTransactionAsync();` |
| `Dispose()` may attempt a second rollback | Technically possible but **unlikely** to be the error source |

**My assessment differs from the report here.** The report overstates the double-rollback risk:

- **EF Core's `IDbContextTransaction.Dispose()`** wraps `DbTransaction.Dispose()`. After `RollbackAsync()` **succeeds**, the internal `SqlTransaction` state is set to "completed" and `Dispose()` does nothing. **Safe.**
- After `RollbackAsync()` **throws** (because the transaction was already terminated by SQL Server), `Dispose()` runs in the compiler `finally`. But `SqlTransaction` internally already knows the transaction is completed (the server told it so), so `Dispose()` is also a no-op. **Safe.**
- The real danger is not double-rollback; it's that **RollbackAsync() itself throws** on an already-terminated transaction, and this exception masks the original.

**Under what exact conditions does the "transaction completed" error appear:**
The `RollbackAsync()` throws `InvalidOperationException("This SqlTransaction has completed; it is no longer usable.")` when:
1. An error with severity ≥ 16 occurred inside the SQL batch (e.g., inside a trigger)
2. SQL Server auto-aborted the transaction
3. The C# `catch` block tries to roll back a transaction that SQL Server already killed

---

### Finding 3: Duplicate UpdateVisitTestsInternalAsync Call

**Verdict: ✅ CONFIRMED**

| Claim | Evidence |
|-------|----------|
| Called twice for existing visits | [VisitService.cs:172](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L172) (inside `else` for `visit.VisitId != 0`) **AND** [VisitService.cs:177](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L177) (separate `if (visit.VisitId != 0)`) |
| Unique constraint `UQ_VisitTest` on `(visit_id, testtype_id)` exists | [FinalLabDbContext.cs:1867](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Data/FinalLabDbContext.cs#L1867): `entity.HasIndex(e => new { e.VisitId, e.TesttypeId }, "UQ_VisitTest").IsUnique();` |
| Can cause duplicate key violation | **Yes** — when tests change during an edit. The second call to [UpdateVisitTestsInternalAsync](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L435-L464) adds new `VisitTest` entities for test IDs that were already added by the first call (both have `VisitTestId = 0` / state `Added`). |

**Critical nuance the report correctly identified:**  
- For **new visits** (VisitId == 0 initially): Only called once (at line 177, after SaveChangesAsync assigns the VisitId). **No duplicate.**
- For **existing visits** (VisitId > 0): Called at **both** line 172 and line 177. **Duplicate!**
- The duplicate **only causes a constraint violation** when the test set changes (new tests are being added). If the user saves an existing visit with the exact same tests, `toAdd` is empty in both calls, and no violation occurs.

> [!WARNING]
> **For new patients (first save), `VisitId` starts as 0.** The first call at line 172 is skipped (inside `else` block for existing visits). The second call at line 177 fires after SaveChangesAsync sets the generated `VisitId`. So the duplicate call bug does **NOT** affect the first save of a new patient. **This means the duplicate call is NOT the cause of the error for new patient saves.**

---

### Finding 4: DbContext Lifetime Problem

**Verdict: ✅ CONFIRMED**

| Claim | Evidence |
|-------|----------|
| DbContext registered as `Scoped` | [App.xaml.cs:123–126](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/App.xaml.cs#L123-L126): `services.AddDbContext<FinalLabDbContext>(..., ServiceLifetime.Scoped, ServiceLifetime.Scoped)` |
| `BuildServiceProvider()` used (no `IHost`) | [App.xaml.cs:76](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/App.xaml.cs#L76): `ServiceProvider = services.BuildServiceProvider();` |
| Scoped ≈ Singleton in WPF without explicit scopes | Confirmed. Only one `CreateScope()` exists — [App.xaml.cs:99](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/App.xaml.cs#L99), used **only** for `IAuthService.HasAnyAdministratorAsync()` at startup. |
| Windows/VMs resolve from root provider | [NavigationService.cs:59](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Infrastructure/Navigation/NavigationService.cs#L59): `_serviceProvider.GetService(windowType)` — resolves from the root `IServiceProvider` |
| All services share the same DbContext instance | Confirmed: `VisitService`, `PatientService`, `TestCatalogService` are all `Scoped`, resolved from the root provider → all receive the **same single** `FinalLabDbContext` instance for the entire application lifetime |

**Can a dirty ChangeTracker cause failures?**
- **After initialization:** The ChangeTracker holds `TestCategory`, `TestGroup`, `TestType`, and `TestProfile` entities (all `Unchanged`). These don't directly conflict with Patient/Visit/VisitTest.
- **After a failed save:** Entities added to the ChangeTracker in `SavePatientVisitAsync` (Patient, Visit, VisitTest, etc.) with state `Added` remain in the ChangeTracker even after `RollbackAsync()`. **EF Core does NOT clear the ChangeTracker on transaction rollback.** This means the **second** save attempt will try to re-add the same entities, potentially causing tracking conflicts (`InvalidOperationException: The instance of entity type 'X' cannot be tracked because another instance with the same key value is already being tracked`).
- **This explains the "persists across multiple attempts" behavior** within a single session. However, the user reports it also persists across app restarts, which means the root cause is hit on the **very first attempt** too.

> [!IMPORTANT]
> The Scoped-as-Singleton pattern is a serious architectural flaw that causes cascading failures, but it is **not the original trigger** — it's an **amplifier** that makes recovery from the first error impossible without restarting the app.

---

## Section 2 — Most Probable Root Cause

### My conclusion: **Scenario B is correct — but I can narrow the original error further**

The most probable causal chain is:

```
1. User clicks حفظ (Save) for a NEW patient (first time ever)

2. SavePatientVisitAsync begins transaction (VisitService.cs:119)

3. SaveChangesAsync #1–#3 succeed (referral, patient, visit saves)

4. UpdateVisitTestsInternalAsync (line 177) adds VisitTest entities to ChangeTracker

5. SaveChangesAsync #4 (line 213) sends the batch to SQL Server:
   - VisitTest INSERTs → TR_VisitTest_SyncBalance fires
     → SP_RecalculateVisitTotals runs
     → UPDATE Visit SET subtotal = @Subtotal, ... payment_status = @PayStatus
     
   - Payment INSERT (if amountPaid > 0) → TR_Payment_SyncBalance fires
     → SP_RecalculateVisitTotals runs AGAIN
     → UPDATE Visit SET ... (same row, second time)

6. ★ THE ORIGINAL ERROR OCCURS HERE ★
   Most likely candidates:
   
   a. EF Core concurrency conflict: The SP updated Visit columns (subtotal,
      total_after_discount, total_paid, balance_due, payment_status) via triggers
      WITHIN the same batch. EF Core sent its own UPDATE statements AND the
      triggers also UPDATEd the same row. If EF Core validates affected rows
      count and the trigger's UPDATE causes an unexpected row count, it could
      throw DbUpdateConcurrencyException.
      
   b. The _context.Patients.Update(patient) at line 141 for an EXISTING patient
      (where PatientId > 0 but the entity is UNTRACKED — created by ToPatient())
      calls Update() which marks ALL properties as Modified. If the ChangeTracker
      already has a tracked Patient with the same key (from a previous operation
      on the shared singleton DbContext), this throws:
      "The instance of entity type 'Patient' cannot be tracked because another
      instance with the same key value for {'PatientId'} is already being tracked."
      
   c. String truncation on Patient.FullNameAr > 150 chars (unlikely for typical 
      Arabic names but possible).

7. The exception propagates to the catch block (line 218)

8. RollbackAsync() (line 220) throws "This SqlTransaction has completed"
   because SQL Server already auto-aborted the transaction due to the
   severity of the error

9. The ORIGINAL error is lost. User sees the rollback error.
```

### Why I believe this is a first-save problem too

For the **very first save** (new patient, `PatientId == 0`):
- The code takes the `Add` path at line 136, so the Update/tracking conflict (6b) doesn't apply
- The duplicate `UpdateVisitTestsInternalAsync` doesn't apply (VisitId was 0)
- **But the triggers still fire** during SaveChangesAsync #4

**The trigger-based failure (6a) is my primary suspect for new patients.** Here's why:

The `SP_RecalculateVisitTotals` does `UPDATE Visit SET payment_status = @PayStatus` where `@PayStatus` uses **UPPERCASE** values ('PAID', 'PARTIAL', 'PENDING'). But the EF Core enum conversion at [FinalLabDbContext.cs:1806–1810](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Data/FinalLabDbContext.cs#L1806-L1810) stores **PascalCase** values ('Paid', 'PartiallyPaid', 'Pending'). While this doesn't cause an immediate SQL error (the UPDATE succeeds), if EF Core tries to **read back** the SP-modified value (e.g., during the subsequent `GetVisitSummaryAsync` at line 216), the `Enum.Parse("PARTIAL")` will throw because there is no `PaymentStatus.PARTIAL` — the correct value is `PaymentStatus.PartiallyPaid`.

> [!CAUTION]
> **However**, line 216 (`GetVisitSummaryAsync`) runs AFTER `CommitAsync()` on line 214. If the commit succeeds, the transaction error wouldn't apply. The read-back error would be a different exception ("Requested value 'PARTIAL' was not found"). Unless... the commit itself encounters the stale-state issue.

**My revised most-probable chain for new patients:**

After further analysis, I believe the most likely trigger is an **unhandled SQL error from within a trigger** (severity 16+ causes automatic transaction abortion by SQL Server). The SP uses `FLOAT` arithmetic and writes to `DECIMAL(18,2)` columns. While normal values are safe, even a rounding edge case or an unexpected NULL in the trigger path could produce an error that aborts the transaction. The C# code then tries `RollbackAsync()` on the dead transaction → the "completed" error masks everything.

---

## Section 3 — Priority Fix Order

| Priority | Fix | Reason |
|----------|-----|--------|
| **1** | **Restructure the catch block to capture and log the original exception before attempting rollback** | This is the **#1 blocker to diagnosis**. Without seeing the real error, every other fix is guesswork. Wrap `RollbackAsync()` in its own try-catch, log the original exception first, and re-throw the original — not the rollback error. |
| **2** | **Remove the explicit `RollbackAsync()` from the catch block entirely** | With `using var transaction`, `Dispose()` already handles rollback. The explicit `RollbackAsync()` is redundant and is the direct cause of the error-masking. Removing it lets the original exception propagate cleanly through `throw;`, and `Dispose()` handles cleanup. |
| **3** | **Fix the duplicate `UpdateVisitTestsInternalAsync` call** | The logic at lines 163–178 should be restructured so the method is called exactly once regardless of new/existing visit path. This eliminates unique constraint violations during edit saves. |
| **4** | **Implement proper DI scoping** | Replace `BuildServiceProvider()` with `IHost`-based hosting or manually create `IServiceScope` for each window/operation. This prevents DbContext from accumulating stale state across the app lifetime and makes failed save attempts recoverable without app restart. |
| **5** | **Fix the SP/C# enum value mismatch** | Align `SP_RecalculateVisitTotals` to use the same casing as EF Core ('Paid', 'PartiallyPaid', 'Pending') or change the C# conversion to be case-insensitive and handle 'PARTIAL' → `PartiallyPaid`. |
| **6** | **Fix the SaveChangesAsync override audit logging bug** | Capture entity states/values **before** `base.SaveChangesAsync()`, then create audit entries using the captured data. Currently the second `base.SaveChangesAsync()` is always a no-op. |

---

## Section 4 — Anything the Previous Report Missed

### 4.1 The `_context.Patients.Update(patient)` Tracking Bomb (SIGNIFICANT)

The report did not emphasize a critical issue at [VisitService.cs:141](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L141):

```csharp
_context.Patients.Update(patient);
```

The `patient` object is created fresh by `PatientInfo.ToPatient()` at [PatientRegistrationViewModel.cs:213](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs#L213), with `PatientId` manually assigned at line 214. This is a **detached entity** (not tracked by the ChangeTracker).

`DbContext.Update()` on a detached entity with a non-zero key will:
1. Start tracking it with state `Modified`
2. Mark ALL properties as modified

**If** any prior operation (e.g., `GetVisitFullDataAsync` during edit mode, or a previous failed save) left a `Patient` entity with the **same** `PatientId` still tracked in the singleton DbContext, then `Update()` throws:

> "The instance of entity type 'Patient' cannot be tracked because another instance with the same key value for {'PatientId'} is already being tracked."

This is a **guaranteed failure** for the second and subsequent edit saves of the same patient within one app session, due to the singleton DbContext. The report mentions this generically in Section 9 but does not call it out as a specific, concrete failure path.

### 4.2 The `_context.Visits.Update(visit)` Has the Same Problem

[VisitService.cs:170](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L170):
```csharp
_context.Visits.Update(visit);
```

The `visit` object is also created fresh in the ViewModel (line 218–259), with `VisitId = CurrentVisitId`. Same detached-entity-with-key problem. After `GetVisitFullDataAsync` loads the Visit (with tracking), a subsequent save creates a new Visit object with the same key → `Update()` → tracking conflict.

### 4.3 ChangeTracker Pollution After Failed Transaction Rollback

The report notes that the DbContext is effectively Singleton but does not fully explore what happens to the ChangeTracker after a transaction rollback:

- `RollbackAsync()` rolls back the **database** transaction
- It does **NOT** revert the ChangeTracker state
- Entities added during the failed transaction (Patient with `Added` state, Visit with `Added` state, VisitTest entities, etc.) **remain in the ChangeTracker**
- On the next save attempt, EF Core tries to `Add`/`Update` entities that conflict with the stale tracked entities
- This creates a **cascade of failures** where every subsequent save attempt fails differently

This is a consequence of the Singleton DbContext but is a distinct failure mode the original report did not explicitly trace.

### 4.4 The `hasAdmin` Scope Creates a Separate DbContext

At [App.xaml.cs:99–103](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/App.xaml.cs#L99-L103):
```csharp
using (var scope = ServiceProvider.CreateScope())
{
    var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
    hasAdmin = await auth.HasAnyAdministratorAsync();
}
```

This creates a **scoped** DbContext for the admin check, which is correctly disposed. However, all subsequent resolutions from the root `ServiceProvider` (line 59 of NavigationService) get the **root-scoped** (singleton) DbContext. This is not a bug per se, but it means the startup admin check does NOT pollute the main DbContext's ChangeTracker. **This is actually fine**, but worth noting.

### 4.5 No Explicit Error Logging in SavePatientVisitAsync

The `catch` block at [VisitService.cs:218–222](file:///c:/Users/LAP%20LINK/source/repos/FinalLabSystem/FinalLabSystem/Services/Implementations/VisitService.cs#L218-L222) does **not** log the exception despite having an `ILogger<VisitService>` injected (line 18). The bare `catch { }` with no `Exception ex` parameter means:
1. The original exception is not logged to Serilog
2. If `RollbackAsync()` throws, neither exception is logged
3. The only evidence is the user-facing dialog message

This is a critical observability gap not mentioned in the original report.

---

## Summary

The previous agent's diagnosis is **largely correct and thorough**. Scenario B (SaveChangesAsync failure → rollback failure masks original error) is indeed the most probable explanation. The single most important action is to **restructure the error handling so the original exception is captured and logged before any rollback attempt**. Without that, the true root cause remains hidden behind the "SqlTransaction completed" mask.

The additional risks I identified (entity tracking conflicts from `Update()` on detached entities, ChangeTracker pollution after rollback, and the missing error logging) are **amplifiers** that make the problem worse on repeated attempts but are not the original trigger on first save. The original trigger is most likely a SQL-level error (from triggers or constraint violations) that causes SQL Server to auto-abort the transaction.

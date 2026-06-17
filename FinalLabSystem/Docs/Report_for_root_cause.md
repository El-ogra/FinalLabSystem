# Report for Root Cause — SqlTransaction Error in Patient Save Flow

**Error observed:** "This SqlTransaction has completed; it is no longer usable."  
**Location:** PatientRegistrationWindow → حفظ (Save)  
**Frequency:** Every time, persists across app restarts  
**Status:** Diagnosis only — zero code changes

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Complete Save Flow Map](#2-complete-save-flow-map)
3. [Full Call Chain — Method by Method](#3-full-call-chain--method-by-method)
4. [Database Triggers and Stored Procedures](#4-database-triggers-and-stored-procedures)
5. [All String Column Sizes — Full SQL Results](#5-all-string-column-sizes--full-sql-results)
6. [Transaction Lifecycle Analysis](#6-transaction-lifecycle-analysis)
7. [SaveChangesAsync Override — Deep Analysis](#7-savechangesasync-override--deep-analysis)
8. [Catch/Finally Block Analysis](#8-catchfinally-block-analysis)
9. [DbContext Lifetime and Reuse Analysis](#9-dbcontext-lifetime-and-reuse-analysis)
10. [Duplicate UpdateVisitTestsInternalAsync Bug](#10-duplicate-updatevisittestsinternalasync-bug)
11. [Model/Database Schema Mismatches](#11-modeldatabase-schema-mismatches)
12. [Enum-to-String Conversion Analysis](#12-enum-to-string-conversion-analysis)
13. [SP_RecalculateVisitTotals — Full Analysis](#13-sp_recalculatevisittotals--full-analysis)
14. [Initialization Sequence and ChangeTracker State](#14-initialization-sequence-and-changetracker-state)
15. [Root Cause Failure Sequences](#15-root-cause-failure-sequences)
16. [Open Questions Requiring Runtime Observation](#16-open-questions-requiring-runtime-observation)

---

## 1. Architecture Overview

### Technology Stack
- **.NET 8** WPF application
- **MVVM** pattern with ViewModels registered as Transient
- **Entity Framework Core 8** with SQL Server provider
- **DbContext:** `FinalLabDbContext` (registered as Scoped, but effectively Singleton in WPF)
- **Services:** Registered as Scoped (effectively Singleton in WPF)
- **ViewModels:** Registered as Transient

### Key Files in the Save Flow

| File | Role |
|------|------|
| `ViewModels/Patients/PatientRegistrationViewModel.cs` | Orchestrates the save; calls `IVisitService.SavePatientVisitAsync` |
| `Services/Interfaces/IVisitService.cs` | Interface for visit operations |
| `Services/Implementations/VisitService.cs` | Contains `SavePatientVisitAsync` — the core save method with transaction |
| `Data/FinalLabDbContext.cs` | DbContext with `SaveChangesAsync` override for audit logging |
| `Data/AuditableAttribute.cs` | Marker attribute for entities that should be audit-logged |
| `Models/Patient.cs` | Patient entity (`[Auditable]`) |
| `Models/Visit.cs` | Visit entity (`[Auditable]`) |
| `Models/VisitTest.cs` | VisitTest entity (not `[Auditable]`) |
| `Models/Payment.cs` | Payment entity (`[Auditable]`) |
| `Models/PatientMedicalHistory.cs` | Medical history (not `[Auditable]`) |
| `Models/ReferralSource.cs` | Referral source (not `[Auditable]`) |
| `Models/AuditLog.cs` | Audit log entity (not `[Auditable]`) |
| `App.xaml.cs` | DI registration; DbContext as Scoped |

---

## 2. Complete Save Flow Map

### Numbered Sequence of Every Database Operation in `SavePatientVisitAsync`

All operations occur inside a single transaction (begun at `VisitService.cs:119`) unless noted otherwise.

| # | Action | File:Line | Table(s) Written/Read | SaveChanges Called? | Transaction? | Can Throw SqlException? |
|---|--------|-----------|----------------------|-------------------|-------------|------------------------|
| 1 | `BeginTransactionAsync` | `VisitService.cs:119` | — | No | Starts transaction | Yes |
| 2 | Add + Save referral (if present) | `VisitService.cs:127-128` | ReferralSource INSERT | **Yes** (→ override) | Yes | Yes |
| 3a | **New patient:** Add + Save | `VisitService.cs:136-137` | Patient INSERT | **Yes** (→ override) | Yes | Yes |
| 3b | **Existing patient:** Update + Save | `VisitService.cs:141-142` | Patient UPDATE | **Yes** (→ override) | Yes | Yes |
| 4 | Query test types (read) | `VisitService.cs:146-148` | TestType SELECT | No | Yes | Yes |
| 5a | **New visit:** Add + Save | `VisitService.cs:165-166` | Visit INSERT | **Yes** (→ override) | Yes | Yes |
| 5b | **Existing visit:** Update + Save + `UpdateVisitTestsInternalAsync` (1st) | `VisitService.cs:170-172` | Visit UPDATE, VisitTest read/write (ChangeTracker) | **Yes** (→ override) at line 171 | Yes | Yes |
| 6 | `UpdateVisitTestsInternalAsync` (2nd for edit / 1st for new) | `VisitService.cs:177` | VisitTest read (query) + write (ChangeTracker) | No (ChangeTracker only) | Yes | Yes (on query) |
| 7 | Query old PatientMedicalHistory + RemoveRange | `VisitService.cs:180-183` | PatientMedicalHistory SELECT + DELETE (ChangeTracker) | No | Yes | Yes (on query) |
| 8 | Add new PatientMedicalHistory entries | `VisitService.cs:185-192` | PatientMedicalHistory INSERT (ChangeTracker) | No | Yes | No |
| 9 | Query old Payments + RemoveRange | `VisitService.cs:194-197` | Payment SELECT + DELETE (ChangeTracker) | No | Yes | Yes (on query) |
| 10 | Add new Payment (if amountPaid > 0) | `VisitService.cs:199-211` | Payment INSERT (ChangeTracker) | No | Yes | No |
| **11** | **SaveChangesAsync — final save** | **`VisitService.cs:213`** | **VisitTest INSERT, PatientMedicalHistory INSERT/DELETE, Payment INSERT/DELETE + triggers** | **Yes** (→ override) | **Yes** | **YES — primary suspect** |
| 12 | `CommitAsync` | `VisitService.cs:214` | — | No | Commits transaction | Yes |
| 13 | `GetVisitSummaryAsync` (read-back) | `VisitService.cs:216` | Visit + related SELECT | No | **No** (after commit) | Yes |

### What Each SaveChangesAsync Call Sends (Inside the Override)

Each call to `SaveChangesAsync` triggers the override at `FinalLabDbContext.cs:32`, which does:

1. Capture `auditableEntries` — entities with `[Auditable]` + Added/Modified/Deleted state (line 34-37)
2. **Call 1:** `base.SaveChangesAsync()` (line 39) — sends the actual INSERT/UPDATE/DELETE to SQL Server
3. Check if audit logging is needed (line 41)
4. Loop through auditable entries to create `AuditLog` rows (lines 47-78)
5. **Call 2:** `base.SaveChangesAsync()` (line 80) — sends AuditLog INSERTs to SQL Server

**Critical finding:** The loop at step 4 NEVER creates audit entries because after step 2, all entity states are `Unchanged` and `IsModified` flags are reset. This means step 5 is always a no-op.

---

## 3. Full Call Chain — Method by Method

### 3.1 PatientRegistrationViewModel.SaveAsync (`PatientRegistrationViewModel.cs:196-282`)

```
196  private async Task SaveAsync()
211  try {
213    var patient = PatientInfo.ToPatient();
214    patient.PatientId = CurrentPatientId;
215    var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;
216    patient.CreatedBy = staffId;
218    var visit = new Visit { ... 40 properties ... };
261    var referralToSave = Referral.ShouldSaveReferral ? Referral.ToReferralSource() : null;
262    var savedVisit = await _visitService.SavePatientVisitAsync(
263      patient, visit, selectedTestIds, Financial.AmountPaid,
264      staffId, MedicalHistory.ToMedicalHistoryList(), referralToSave);
271    CurrentPatientId = savedVisit.PatientId;
272    CurrentVisitId = savedVisit.VisitId;
276    _dialogService.ShowMessage("تم حفظ بيانات المريض والزيارة.", "حفظ");
278  } catch (Exception ex) {
280    _dialogService.ShowError(ex.Message);  // ← USER SEES THIS
281  }
```

**Line 280 is where the error message appears.** The `ex.Message` is whatever exception propagates from `SavePatientVisitAsync` after its own catch block re-throws.

### 3.2 VisitService.SavePatientVisitAsync (`VisitService.cs:110-223`)

```
119  using var transaction = await _context.Database.BeginTransactionAsync();
121  try {
123-130  // Save referral if present
132-143  // Save or update patient (2 SaveChangesAsync calls)
145-148  // Query test types
150-161  // Calculate visit financials
163-178  // Save visit + UpdateVisitTestsInternalAsync
180-192  // Replace PatientMedicalHistory
194-211  // Replace Payments
213     await _context.SaveChangesAsync();
214     await transaction.CommitAsync();
216     return (await GetVisitSummaryAsync(visit.VisitId))!;
218  } catch {
220     await transaction.RollbackAsync();
221     throw;
222  }
```

The `using var transaction` at line 119 generates a compiler `finally` block that calls `transaction.Dispose()`.

### 3.3 PatientInfoViewModel.ToPatient (`PatientInfoViewModel.cs:244-264`)

Creates a `Patient` with values from the form. No validation for DB column lengths.

### 3.4 MedicalHistoryViewModel.ToMedicalHistoryList (`MedicalHistoryViewModel.cs:266-278`)

Creates `PatientMedicalHistory` objects. `Description` can be the user-entered text or the history type name.

### 3.5 FinalLabDbContext.SaveChangesAsync override (`FinalLabDbContext.cs:32-82`)

```
32   public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
34     var auditableEntries = ChangeTracker.Entries()
35       .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
36         && e.Entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), false).Length > 0)
37       .ToList();
39     var result = await base.SaveChangesAsync(cancellationToken);  // CALL 1
41     if (auditableEntries.Count == 0 || _session is null)
42       return result;
44     var staffId = _session.CurrentUser?.StaffId;
47     foreach (var entry in auditableEntries) {
49       var tableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
51       var key = entry.Metadata.FindPrimaryKey();
55       foreach (var property in entry.Properties) {
57         if (!property.IsModified && entry.State != EntityState.Added)  // ← ALWAYS TRUE AFTER CALL 1
58           continue;  // ← SKIPS EVERYTHING
60         AuditLogs.Add(...);
61       }
62     }
80     await base.SaveChangesAsync(cancellationToken);  // CALL 2 — never saves anything
81     return result;
```

---

## 4. Database Triggers and Stored Procedures

### 4.1 TR_Payment_SyncBalance (on Payment AFTER INSERT, UPDATE, DELETE)

```sql
CREATE TRIGGER TR_Payment_SyncBalance ON Payment AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @vid INT;
    DECLARE vid_cur CURSOR FAST_FORWARD FOR
        SELECT DISTINCT visit_id FROM (
            SELECT visit_id FROM inserted
            UNION
            SELECT visit_id FROM deleted
        ) x;
    OPEN vid_cur;
    FETCH NEXT FROM vid_cur INTO @vid;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC SP_RecalculateVisitTotals @vid;
        FETCH NEXT FROM vid_cur INTO @vid;
    END;
    CLOSE vid_cur;
    DEALLOCATE vid_cur;
END;
```

**Fires during SaveChangesAsync #4** (line 213) when the Payment INSERT is sent to SQL Server.

### 4.2 TR_VisitTest_SyncBalance (on VisitTest AFTER INSERT, UPDATE, DELETE)

```sql
CREATE TRIGGER TR_VisitTest_SyncBalance ON VisitTest AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @vid INT;
    DECLARE vid_cur CURSOR FAST_FORWARD FOR
        SELECT DISTINCT visit_id FROM (
            SELECT visit_id FROM inserted
            UNION
            SELECT visit_id FROM deleted
        ) x;
    OPEN vid_cur;
    FETCH NEXT FROM vid_cur INTO @vid;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC SP_RecalculateVisitTotals @vid;
        FETCH NEXT FROM vid_cur INTO @vid;
    END;
    CLOSE vid_cur;
    DEALLOCATE vid_cur;
END;
```

**Fires during SaveChangesAsync #4** (line 213) when VisitTest INSERTs are sent to SQL Server.

### 4.3 TR_TestResult_AuditInsert / TR_TestResult_AuditUpdate

These fire on the TestResult table. **Not relevant to the save flow** — TestResult is not written during patient registration.

### 4.4 SP_RecalculateVisitTotals

```sql
CREATE PROCEDURE SP_RecalculateVisitTotals @VisitID INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Subtotal            FLOAT = 0;
    DECLARE @DiscountAmount      FLOAT = 0;
    DECLARE @DiscountPercent     FLOAT = 0;
    DECLARE @TotalAfterDiscount  FLOAT = 0;
    DECLARE @TotalPaid           FLOAT = 0;
    DECLARE @BalanceDue          FLOAT = 0;
    DECLARE @PayStatus           NVARCHAR(10);

    SELECT @Subtotal = ISNULL(SUM(price_charged), 0)
    FROM VisitTest
    WHERE visit_id = @VisitID AND current_stage <> 'CANCELLED';

    SELECT @DiscountAmount  = discount_amount,
           @DiscountPercent = discount_percent
    FROM Visit
    WHERE visit_id = @VisitID;

    SET @TotalAfterDiscount = @Subtotal
                            - @DiscountAmount
                            - (@Subtotal * @DiscountPercent / 100.0);
    IF @TotalAfterDiscount < 0 SET @TotalAfterDiscount = 0;

    SELECT @TotalPaid = ISNULL(SUM(
        CASE WHEN payment_type = 'PAYMENT' THEN  amount
             WHEN payment_type = 'REFUND'  THEN -amount
             ELSE 0
        END), 0)
    FROM Payment WHERE visit_id = @VisitID;

    SET @BalanceDue = @TotalAfterDiscount - @TotalPaid;
    IF @BalanceDue < 0 SET @BalanceDue = 0;

    SET @PayStatus = CASE
        WHEN @TotalPaid >= @TotalAfterDiscount AND @TotalAfterDiscount > 0 THEN 'PAID'
        WHEN @TotalPaid > 0                                                THEN 'PARTIAL'
        ELSE                                                                    'PENDING'
    END;

    UPDATE Visit SET
        subtotal             = @Subtotal,
        total_after_discount = @TotalAfterDiscount,
        total_paid           = @TotalPaid,
        balance_due          = @BalanceDue,
        payment_status       = @PayStatus,
        updated_at           = SYSDATETIME()
    WHERE visit_id = @VisitID;
END;
```

**Key observations about this SP:**
- Uses `FLOAT` for financial calculations (not `DECIMAL`) — potential rounding/overflow risk
- `@PayStatus` is `NVARCHAR(10)` — values 'PAID', 'PARTIAL', 'PENDING' (different case from C# enum values 'Paid', 'PartiallyPaid', 'Pending')
- The `UPDATE Visit` modifies the same row that was just inserted/updated by the C# code
- The SP executes within the same transaction as the C# code

---

## 5. All String Column Sizes — Full SQL Results

### Query: All nchar/nvarchar/char/varchar columns in write tables

```
TABLE_NAME     | COLUMN_NAME            | DATA_TYPE | MAX_LENGTH | NULLABLE
---------------|------------------------|-----------|------------|---------
AuditLog       | action                 | nchar     | 1          | NO
AuditLog       | field_name             | nvarchar  | 100        | YES
AuditLog       | new_value              | nvarchar  | -1 (max)   | YES
AuditLog       | notes                  | nvarchar  | 500        | YES
AuditLog       | old_value              | nvarchar  | -1 (max)   | YES
AuditLog       | session_info           | nvarchar  | 200        | YES
AuditLog       | table_name             | nvarchar  | 100        | NO
NormalRange    | age_description        | nvarchar  | 50         | YES
NormalRange    | age_unit               | nvarchar  | 10         | YES
NormalRange    | critical_comment       | nvarchar  | 500        | YES
NormalRange    | critical_flag          | nvarchar  | 20         | YES
NormalRange    | critical_range_text    | nvarchar  | 200        | YES
NormalRange    | fasting_state          | nchar     | 1          | NO
NormalRange    | high_comment           | nvarchar  | 500        | YES
NormalRange    | high_flag              | nvarchar  | 20         | YES
NormalRange    | low_comment            | nvarchar  | 500        | YES
NormalRange    | low_flag               | nvarchar  | 20         | YES
NormalRange    | normal_range_text      | nvarchar  | 200        | YES
NormalRange    | range_note             | nvarchar  | 200        | YES
NormalRange    | sex                    | nchar     | 1          | NO
NormalRange    | unit                   | nvarchar  | 20         | YES
Patient        | address                | nvarchar  | 250        | YES
Patient        | approx_age_unit        | nvarchar  | 10         | YES
Patient        | blood_type             | nvarchar  | 5          | YES
Patient        | email                  | nvarchar  | 100        | YES
Patient        | full_name_ar           | nvarchar  | **150**    | NO
Patient        | full_name_en           | nvarchar  | **150**    | YES
Patient        | national_id            | nvarchar  | 20         | YES
Patient        | notes                  | nvarchar  | -1 (max)   | YES
Patient        | patient_code           | nvarchar  | 30         | NO
Patient        | patient_type           | nvarchar  | 20         | NO
Patient        | phone                  | nvarchar  | 20         | YES
Patient        | phone2                 | nvarchar  | 20         | YES
Patient        | sex                    | nchar     | 1          | NO
Patient        | title                  | nvarchar  | **20**     | YES
Payment        | notes                  | nvarchar  | 200        | YES
Payment        | payment_method         | nvarchar  | 20         | NO
Payment        | payment_type           | nvarchar  | **10**     | NO
Payment        | reference_number       | nvarchar  | 100        | YES
SampleTube     | barcode_value          | nvarchar  | 100        | NO
SampleTube     | notes                  | nvarchar  | 200        | YES
SampleTube     | tube_color             | nvarchar  | 30         | YES
SampleTube     | tube_type              | nvarchar  | 50         | NO
Visit          | notes                  | nvarchar  | -1 (max)   | YES
Visit          | payment_status         | nvarchar  | 30         | NO
Visit          | visit_code             | nvarchar  | 30         | NO
Visit          | visit_status           | nvarchar  | 30         | NO
VisitTest      | current_stage          | nvarchar  | 30         | NO
```

### Critical Model-vs-Database Length Mismatches

| Model Property | Model `[StringLength]` | DB Column Type | Risk |
|---------------|----------------------|----------------|------|
| `Patient.FullNameAr` | 200 (`Patient.cs:24`) | `nvarchar(150)` (`DbContext.cs:638`) | **Truncation error if > 150 chars** |
| `Patient.FullNameEn` | 200 (`Patient.cs:27`) | `nvarchar(150)` (`DbContext.cs:641`) | **Truncation error if > 150 chars** |
| `Patient.Title` | 50 (`Patient.cs:20`) | `nvarchar(20)` (`DbContext.cs:668`) | **Truncation error if > 20 chars** |

### AuditLog Table Content

```
Query: SELECT TOP 5 * FROM AuditLog ORDER BY audit_id DESC
Result: 0 rows — AuditLog is EMPTY
```

**Interpretation:** No auditable save has ever succeeded through the `SaveChangesAsync` override. Either:
1. Every save has failed before reaching the audit logging step, OR
2. The audit logging logic is broken (confirmed — see Section 7), so even successful saves would leave the table empty

---

## 6. Transaction Lifecycle Analysis

### 6a. Where is `BeginTransactionAsync` called?

**File:** `VisitService.cs:119`
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
```

Also at:
- `VisitService.cs:28` (in `CreateVisitAsync`)
- `VisitService.cs:330` (in `CancelVisitAsync`)

### 6b. Where is `CommitAsync` called?

**File:** `VisitService.cs:214`
```csharp
await transaction.CommitAsync();
```

Also at:
- `VisitService.cs:92` (in `CreateVisitAsync`)
- `VisitService.cs:351` (in `CancelVisitAsync`)

### 6c. Where is `RollbackAsync` called? Is it in catch or finally?

**File:** `VisitService.cs:220`
```csharp
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**No explicit `finally` block.** However, the `using var transaction` declaration at line 119 causes the C# compiler to generate a `finally` block that calls `transaction.Dispose()`.

### 6d. Is double-RollbackAsync possible?

**YES — this is a critical finding.**

The execution sequence when an exception occurs in the `try` block is:

1. **`catch` block runs** → `await transaction.RollbackAsync()` is called (line 220)
2. **If RollbackAsync succeeds:** `throw;` re-throws the original exception → the `using` generated `finally` runs → `transaction.Dispose()` is called
3. **If RollbackAsync fails:** The exception from RollbackAsync propagates (the `throw;` is NOT reached) → the `using` generated `finally` runs → `transaction.Dispose()` is called

In case 3, `Dispose()` checks if the transaction is still pending. If `RollbackAsync` partially completed or failed without updating the internal state, `Dispose()` will attempt to roll back again → **throws "This SqlTransaction has completed; it is no longer usable."**

### 6e. How many times is `SaveChangesAsync` called inside the transaction?

**4 explicit calls**, each triggering the override which calls `base.SaveChangesAsync()` twice:

| Order | Line | What Changes Are Saved | Override Call 1 (base) | Override Call 2 (base) |
|-------|------|----------------------|----------------------|----------------------|
| 1 | 128 | ReferralSource INSERT | Saves referral | No-op (no audit entries created) |
| 2 | 137 | Patient INSERT/UPDATE | Saves patient | No-op |
| 3 | 166 | Visit INSERT/UPDATE | Saves visit | No-op |
| 4 | 213 | VisitTest INSERT, PatientMedicalHistory INSERT/DELETE, Payment INSERT/DELETE | **Saves all remaining + triggers fire** | No-op |

**Total `base.SaveChangesAsync()` calls: up to 8**

### 6f. Is the same DbContext used for primary save AND audit log?

**Yes.** Both the primary entity changes and the attempted audit log entries use the same `FinalLabDbContext` instance — the override's `AuditLogs.Add()` (line 60) adds to the same context that's being used for the transaction.

### 6g. Does the override call `base.SaveChangesAsync` more than once?

**Yes.** Lines 39 and 80. The second call is intended to save audit log entries, but **it never has anything to save** because the audit logging loop is broken (see Section 7).

---

## 7. SaveChangesAsync Override — Deep Analysis

### The Bug: Entity State Check Fails After First Save

```csharp
// Line 34-37: Captured BEFORE the save
var auditableEntries = ChangeTracker.Entries()
    .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
        && e.Entity.GetType().GetCustomAttributes(...).Length > 0)
    .ToList();

// Line 39: SAVE HAPPENS HERE
var result = await base.SaveChangesAsync(cancellationToken);

// After this save:
//   Added entities → state becomes Unchanged
//   Modified entities → state becomes Unchanged
//   Deleted entities → state becomes Detached
//   All property.IsModified → false

// Line 47-78: Loop through captured entries
foreach (var entry in auditableEntries)       // ← entry is the SAME object, but its State has changed
{
    foreach (var property in entry.Properties)
    {
        // Line 57: THIS CHECK FAILS FOR EVERY PROPERTY
        if (!property.IsModified && entry.State != EntityState.Added)
            continue;  // ← ALWAYS EXECUTES

        // Line 60: NEVER REACHED
        AuditLogs.Add(...);
    }
}

// Line 80: NEVER SAVES ANYTHING
await base.SaveChangesAsync(cancellationToken);
```

**After `base.SaveChangesAsync()` (line 39):**

| Original State | New State | IsModified | Condition `!IsModified && state != Added` | Result |
|---------------|-----------|------------|------------------------------------------|--------|
| Added | `Unchanged` | `false` | `true && true` = **true** | **Skip** |
| Modified | `Unchanged` | `false` | `true && true` = **true** | **Skip** |
| Deleted | `Detached` | `false` | `true && true` = **true** | **Skip** |

**Conclusion:** The audit logging loop at lines 55-77 NEVER creates any audit entries. The second `base.SaveChangesAsync()` at line 80 is always a no-op and never causes a database round-trip. **This bug does NOT cause the "transaction completed" error** — it just means no audit entries are ever written.

### Does This Bug Affect the Transaction?

Since no changes are pending after the first `base.SaveChangesAsync()`, the second call returns immediately with result = 0. It does NOT interact with the database at all. **It is harmless from a transaction perspective.**

The empty `AuditLog` table confirms this behavior.

---

## 8. Catch/Finally Block Analysis

### 8a-e: Full Analysis of Error Handling

**8a. Does the catch block call RollbackAsync?**  
Yes — `VisitService.cs:220`.

**8b. Does the finally block also call RollbackAsync?**  
There is no explicit `finally` block. But the `using var transaction` at `VisitService.cs:119` generates a compiler-created `finally` that calls `transaction.Dispose()`.

**8c. Is double-RollbackAsync possible?**  

The `using` pattern:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try { ... commit; }
catch { await transaction.RollbackAsync(); throw; }
```

The compiler expands this to:
```csharp
var transaction = await _context.Database.BeginTransactionAsync();
try
{
    try { ... commit; }
    catch { await transaction.RollbackAsync(); throw; }
}
finally
{
    if (transaction != null)
        ((IDisposable)transaction).Dispose();
}
```

`IDbContextTransaction.Dispose()` internally calls `SqlTransaction.Dispose()`, which:
- Checks if the transaction has already been committed or rolled back
- If still pending: calls `Rollback()` (sync)
- If already completed: does nothing

**Scenario where double-Rollback throws the error:**

1. The first `RollbackAsync()` succeeds → `Dispose()` sees transaction is rolled back → no-op. **Safe.**
2. The first `RollbackAsync()` THROWS (because the transaction was already terminated by SQL Server) → `Dispose()` runs → if the transaction object's internal state still says "pending", `Dispose()` calls `Rollback()` → "This SqlTransaction has completed." **THIS IS THE ERROR.**

**8d. After RollbackAsync, does the catch block do any further DB operation?**  
No. The catch block only calls `RollbackAsync()` and then `throw;` (line 221). No database operations.

**8e. Does the ViewModel's catch block do anything with the DB?**  
No. `PatientRegistrationViewModel.cs:278-281` only calls `_dialogService.ShowError(ex.Message)`. No database operations.

---

## 9. DbContext Lifetime and Reuse Analysis

### 9a. How is DbContext registered in DI?

**File:** `App.xaml.cs:123-126`
```csharp
services.AddDbContext<FinalLabDbContext>(options =>
    options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped,     // context lifetime
    ServiceLifetime.Scoped);    // options lifetime
```

### 9b. Effective lifetime in WPF

In a WPF application using `services.BuildServiceProvider()` (no `IHost`, no `IServiceScopeFactory`), **Scoped services resolved from the root provider behave as Singletons**. The same `FinalLabDbContext` instance is shared across:
- All windows (PatientRegistrationWindow, MainWindow, etc.)
- All ViewModels
- All Services (VisitService, PatientService, TestCatalogService, etc.)
- The entire application lifetime

**This is critically important:** When the user opens PatientRegistrationWindow, fills in data, and clicks Save, the DbContext used already contains entities loaded from previous operations (initialization, test catalog loading, patient code generation, etc.).

### 9c. Can a corrupted ChangeTracker from a failed operation affect later saves?

**YES.** Because the DbContext is effectively Singleton:

1. If a previous SaveChangesAsync fails (e.g., from a prior save attempt), the ChangeTracker retains entities in inconsistent states
2. Tracked entities may have incorrect `State` values (e.g., `Added` after a failed save, or `Detached` when they should be `Unchanged`)
3. EF Core may throw `InvalidOperationException` when trying to track entities with duplicate keys
4. The accumulated tracked entities (from `TestSelectionViewModel.InitializeAsync()`, `GeneratePatientCodeAsync()`, etc.) accumulate in the `Unchanged` state and are never removed

However, since the error persists across **application restarts**, a corrupted ChangeTracker from a prior run is NOT the cause (the context is recreated on restart). The issue must be reproducible within a single session.

### 9d. What entities are loaded into the ChangeTracker before the save?

During `PatientRegistrationViewModel.InitializeAsync()` (`PatientRegistrationViewModel.cs:74-87`):

1. `PatientInfoViewModel.InitializeAsync()` → calls `PatientService.GetPatientTitlesAsync()` → `_context.Patients.Where(...).Select(p => p.Title).Distinct().ToListAsync()` — **projection query, no entity tracking**
2. `TestSelectionViewModel.InitializeAsync()` → calls `TestCatalogService.GetFullHierarchyAsync()` — loads `TestCategory`, `TestGroup`, `TestType` entities — **tracking queries, entities ARE tracked**
3. `ClearFormAsync()` → calls `PatientService.GeneratePatientCodeAsync()` → `_context.Patients.Where(...).Select(p => p.PatientCode).FirstOrDefaultAsync()` — **projection, no tracking**

After initialization, the ChangeTracker contains: **TestCategory, TestGroup, TestType, and TestProfile entities** (all `Unchanged`).

During save, entities are added/updated for: Patient, Visit, VisitTest, Payment, PatientMedicalHistory, ReferralSource. These do NOT conflict with the already-tracked entities.

**Verdict:** The accumulated tracked entities do NOT directly cause the transaction error.

---

## 10. Duplicate UpdateVisitTestsInternalAsync Bug

### The Bug

For **existing visits** (edit mode), `UpdateVisitTestsInternalAsync` is called **TWICE** — once at line 172 and once at line 177 — without an intervening `SaveChangesAsync()`.

```csharp
// Lines 168-178
if (visit.VisitId == 0)
{
    _context.Visits.Add(visit);
    await _context.SaveChangesAsync();   // Save #3
}
else
{
    _context.Visits.Update(visit);
    await _context.SaveChangesAsync();   // Save #3
    await UpdateVisitTestsInternalAsync(visit.VisitId, uniqueTestIds);  // FIRST CALL
}

if (visit.VisitId != 0)
{
    await UpdateVisitTestsInternalAsync(visit.VisitId, uniqueTestIds);  // SECOND CALL
}
```

For **new visits** (VisitId == 0): The code at line 175 checks `visit.VisitId != 0` which is true AFTER `SaveChangesAsync` at line 166 generates the VisitId. So `UpdateVisitTestsInternalAsync` is called once at line 177. **No duplicate for new visits.**

For **existing visits** (VisitId > 0): Called at BOTH line 172 AND line 177. **Duplicate!**

### Why This Is Dangerous

`UpdateVisitTestsInternalAsync` at `VisitService.cs:435-464`:

```csharp
private async Task UpdateVisitTestsInternalAsync(int visitId, List<int> testTypeIds)
{
    var existing = await _context.VisitTests
        .Where(vt => vt.VisitId == visitId).ToListAsync();     // Query DB

    var existingIds = existing.Select(vt => vt.TesttypeId).ToHashSet();
    var desiredIds = testTypeIds.ToHashSet();

    var toRemove = existing.Where(vt => !desiredIds.Contains(vt.TesttypeId)).ToList();
    _context.VisitTests.RemoveRange(toRemove);

    var toAdd = desiredIds.Except(existingIds).ToList();
    var tests = await _context.TestTypes
        .Where(t => toAdd.Contains(t.TesttypeId)).ToListAsync();

    foreach (var test in tests)
        _context.VisitTests.Add(new VisitTest { ... });
}
```

**First call:** Queries the DB → gets current VisitTests → modifies ChangeTracker (adds/removes)
**Second call:** Queries the DB again → **DB still has old data** (changes not saved yet) → EF Core identity resolution returns tracked entities (including those marked Deleted)

**Concrete scenario for an existing visit with tests [1, 2] changing to [2, 3]:**

First call:
- Query: returns VisitTest with TesttypeId=1, TesttypeId=2
- `existingIds` = {1, 2}, `desiredIds` = {2, 3}
- `toRemove` = {TesttypeId=1} → marked Deleted
- `toAdd` = {3} → added as new VisitTest (state Added)

Second call:
- Query: DB still has {1, 2}, but identity resolution returns tracked entities
- `existing` = {TesttypeId=1 (Deleted), TesttypeId=2 (Unchanged)}
- `existingIds` = {1, 2} (Deleted entities are still in the collection)
- `desiredIds` = {2, 3}
- `toRemove` = {TesttypeId=1 (Deleted)} — already Deleted, RemoveRange is a no-op
- `toAdd` = {3} — but 3 was ALREADY ADDED in the first call! Now a SECOND VisitTest with TesttypeId=3 is added (both with VisitTestId=0)

**Result:** Two VisitTest entities with the same `(VisitId, TesttypeId)` = `(visitId, 3)` are added. When `SaveChangesAsync` at line 213 runs, the unique constraint `UQ_VisitTest` (on `visit_id + testtype_id`) is violated → SQL Server throws `Cannot insert duplicate key`.

**This would cause a `DbUpdateException` during SaveChangesAsync #4.** The exception propagates to the catch block, which calls `RollbackAsync()`. The rollback should succeed. The user would see the unique constraint violation error, NOT "transaction completed."

**However**, the duplicate call for exact same test sets (no change in tests) does NOT cause this issue — `toAdd` would be empty in both calls.

### Impact Assessment

- For edit mode with test changes: **HIGH** — causes `DbUpdateException` on unique constraint
- The `DbUpdateException` triggers the catch → `RollbackAsync()` → should work fine → user sees constraint violation message, NOT "transaction completed"
- **BUT**: If during this constraint violation the trigger `TR_VisitTest_SyncBalance` fires or the transaction becomes doomed, the subsequent `RollbackAsync` could fail with "transaction completed" (see Section 15)

---

## 11. Model/Database Schema Mismatches

### 11.1 Critical Length Mismatches

| Property | C# Model `[StringLength]` | DB Column | Effect If Exceeded |
|----------|--------------------------|-----------|-------------------|
| `Patient.FullNameAr` | 200 (`Patient.cs:24`) | `nvarchar(150)` (`DbContext.cs:638`) | **SqlException: String/binary data would be truncated** |
| `Patient.FullNameEn` | 200 (`Patient.cs:27`) | `nvarchar(150)` (`DbContext.cs:641`) | **SqlException: String/binary data would be truncated** |
| `Patient.Title` | 50 (`Patient.cs:20`) | `nvarchar(20)` (`DbContext.cs:668`) | **SqlException: String/binary data would be truncated** |

### 11.2 Mismatch Mechanism

The C# `[StringLength]` attribute:
- Is only used for **client-side validation** (e.g., `HasErrors` check in `PatientInfoViewModel`)
- Is NOT enforced by EF Core's migration system — the DB schema is defined by `HasMaxLength()` in `OnModelCreating`
- The model says "up to 200 chars" but the DB rejects anything over 150 chars

If the user enters a `FullNameAr` longer than 150 characters:
1. C# validation allows it (max 200)
2. `PatientInfoViewModel.ToPatient()` creates a Patient with the long name
3. `SaveChangesAsync` tries to INSERT into `nvarchar(150)` column
4. SQL Server returns: **"String or binary data would be truncated"** (SqlException)
5. The catch block calls `RollbackAsync()` — this should work
6. The user sees the truncation error message, NOT "transaction completed"

### 11.3 PatientMedicalHistory Description Column

`MedicalHistoryViewModel.cs:272-273`:
```csharp
Description = string.IsNullOrWhiteSpace(item.Description) ? item.HistoryType : item.Description.Trim(),
```

The `PatientMedicalHistory.Description` column has **no explicit max length** in either the model or the DB Fluent API. It defaults to `nvarchar(max)` in EF Core. **No truncation risk.**

### 11.4 AuditLog Action Column

The previously fixed issue (`AuditLog.action nchar(1)`) now maps correctly:
```csharp
Action = entry.State switch
{
    EntityState.Added => "A",
    EntityState.Modified => "M",
    EntityState.Deleted => "D",
    _ => "U"
};
```

All values are single characters. **No truncation risk here.**

---

## 12. Enum-to-String Conversion Analysis

### 12.1 Visit.PaymentStatus

```csharp
// Fluent API (DbContext.cs:1806-1810)
.HasConversion(v => v.ToString(), v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v ?? "Pending", true))
.HasMaxLength(30)
```

C# values: `PaymentStatus.Pending` → "Pending", `PaymentStatus.Paid` → "Paid", `PaymentStatus.PartiallyPaid` → "PartiallyPaid"

DB column: `nvarchar(30)` — all values fit. **Safe.**

SP values: PENDING, PAID, PARTIAL — C# and SP use different casing. **Data inconsistency but no error.**

### 12.2 Visit.VisitStatus

```csharp
.HasConversion(v => v.ToString(), v => (VisitStatus)Enum.Parse(...))
.HasMaxLength(30)
```

C# value: `VisitStatus.Open` → "Open". DB column: `nvarchar(30)`. **Safe.**

### 12.3 VisitTest.CurrentStage

```csharp
.HasConversion(v => v.ToString(), v => (TestStage)Enum.Parse(...))
.HasMaxLength(30)
```

C# value: `TestStage.Pending` → "Pending". DB column: `nvarchar(30)`. **Safe.**

### 12.4 Payment.PaymentMethod

```csharp
.HasConversion(v => v.ToString(), v => (PaymentMethod)Enum.Parse(...))
.HasMaxLength(20)
```

C# value: `PaymentMethod.Cash` → "Cash". DB column: `nvarchar(20)`. **Safe.**

### 12.5 Payment.PaymentType

Hardcoded as `"PAYMENT"` (`VisitService.cs:207`). DB column: `nvarchar(10)`. "PAYMENT" is 7 chars. **Safe.**

---

## 13. SP_RecalculateVisitTotals — Full Analysis

### 13.1 When It Executes

During `SaveChangesAsync #4` (line 213), the following SQL batch is sent:

1. VisitTest INSERTs → fire `TR_VisitTest_SyncBalance` → `SP_RecalculateVisitTotals` → `UPDATE Visit`
2. Payment INSERT (and DELETE) → fire `TR_Payment_SyncBalance` → `SP_RecalculateVisitTotals` → `UPDATE Visit`
3. PatientMedicalHistory INSERT/DELETE (no triggers)

**Both triggers UPDATE the same Visit row, within the same transaction.**

### 13.2 Potential Failure Points

**a. FLOAT → DECIMAL conversion:** The SP uses `FLOAT` for financial calculations. `@Subtotal` is `FLOAT` but `subtotal` in the Visit table is `decimal(18,2)`. When converting from `FLOAT` to `DECIMAL(18,2)`:
- If the float value has more than 18 digits before the decimal point → **overflow error**
- Normal test prices (hundreds or thousands) are safe
- But `SUM(price_charged)` could theoretically overflow if thousands of tests are summed

**b. PayStatus NVARCHAR(10) → NVARCHAR(30):** The SP declares `@PayStatus NVARCHAR(10)` and sets it to 'PAID', 'PARTIAL', or 'PENDING'. The target column `payment_status` is `NVARCHAR(30)`. **Safe.**

**c. Cursor iteration:** The SP uses a cursor, but for a single visit there's only one iteration. **Safe for single-visit flows.**

**d. Race condition with Visit UPDATE:** The SP's `UPDATE Visit` modifies the same row that was just inserted/updated by the C# code. Since both are in the same transaction, this is: (1) allowed by SQL Server (nested DML is fine), (2) potentially confusing for the ChangeTracker but not causing DB errors.

### 13.3 Concurrency Concern

The `SP_RecalculateVisitTotals` updates `Visit.updated_at = SYSDATETIME()`. The C# code previously set `visit.UpdatedAt = DateTime.UtcNow`. After the SP runs, the DB has a different value than the ChangeTracker. This means the ChangeTracker is now stale. **This does NOT cause an error** unless there's a concurrency token (RowVersion) — which there isn't.

---

## 14. Initialization Sequence and ChangeTracker State

### Full Initialization Path

```
App.OnStartup
  → navigation.ShowLogin()         // Login window
  → navigation.ShowMain()          // MainWindow
  → navigation.OpenTaskWindow<PatientRegistrationViewModel>()
      → _serviceProvider.GetService(PatientRegistrationWindow)
          → DI resolves PatientRegistrationViewModel (Transient)
              → DI resolves PatientInfoViewModel (Transient)
              → DI resolves ReferralViewModel (Transient)
              → DI resolves MedicalHistoryViewModel (Transient)
              → DI resolves TestSelectionViewModel (Transient)
                  → DI resolves ITestCatalogService (Scoped → effectively Singleton)
                      → gets FinalLabDbContext (Scoped → effectively Singleton)
              → DI resolves IVisitService (Scoped → effectively Singleton)
                  → gets THE SAME FinalLabDbContext instance
      → Window.Show()
          → PatientRegistrationViewModel.InitializeAsync()
              → PatientInfo.InitializeAsync()
                  → PatientService.GetPatientTitlesAsync()
                      → Projection query: NOT tracked in ChangeTracker
              → TestSelection.InitializeAsync()
                  → TestCatalogService.GetFullHierarchyAsync()
                      → LOADS TestCategory, TestGroup, TestType → TRACKED
                  → TestCatalogService.GetActiveProfilesAsync()
                      → LOADS TestProfile entities → TRACKED
                  → TestCatalogService.GetProfileTestsAsync()
                      → LOADS TestType entities (again) → TRACKED
              → ClearFormAsync()
                  → PatientService.GeneratePatientCodeAsync()
                      → Projection query: NOT tracked
```

### State of ChangeTracker When User Clicks Save

**Tracked entities (all Unchanged):**
- `TestCategory` entities (from hierarchy loading)
- `TestGroup` entities (from hierarchy loading)
- `TestType` entities (from hierarchy loading)
- `TestProfile` entities (if any)

**These do NOT conflict with the new entities being created during save** (Patient, Visit, VisitTest, Payment, PatientMedicalHistory, ReferralSource all have different entity types or different keys).

**Verdict:** The initialization does not leave the ChangeTracker in a state that would cause the transaction error.

---

## 15. Root Cause Failure Sequences

### Scenario A: SaveChangesAsync Fails → RollbackAsync Throws (MOST LIKELY)

This is the most probable root cause. The error message "This SqlTransaction has completed; it is no longer usable." is the SECOND exception — it masks the ORIGINAL exception.

```
1. PatientRegistrationViewModel.SaveAsync (PatientRegistrationViewModel.cs:262)
     calls VisitService.SavePatientVisitAsync

2. Transaction begins at VisitService.cs:119

3. SaveChangesAsync #1 (line 128): saves referral → succeeds
4. SaveChangesAsync #2 (line 137): saves patient → succeeds
5. SaveChangesAsync #3 (line 166): saves visit → succeeds
6. UpdateVisitTestsInternalAsync (line 177): prepares VisitTest entities

7. SaveChangesAsync #4 (line 213):
   a. The override calls base.SaveChangesAsync() (FinalLabDbContext.cs:39)
   b. SQL Server batch executes:
      - VisitTest INSERTs → TR_VisitTest_SyncBalance fires
        → SP_RecalculateVisitTotals executes
        → UPDATE Visit (succeeds)
      - PatientMedicalHistory INSERT/DELETE (no triggers)
      - Payment INSERT → TR_Payment_SyncBalance fires
        → SP_RecalculateVisitTotals executes
        → UPDATE Visit (succeeds)
   c. ALL OPERATIONS IN THE BATCH SUCCEED
   d. The override loop runs (lines 47-78) — creates 0 audit entries (state bug)
   e. Second base.SaveChangesAsync() (line 80) — no-op
   f. Returns normally

8. transaction.CommitAsync() (VisitService.cs:214) IS CALLED
   --- BUT WHAT IF THIS FAILS? ---
   
   Alternative 8a: CommitAsync fails because:
      - Network issue
      - SQL Server internal error
      - The transaction was already doomed by a previous error (but steps 3-7 succeeded)
   → CommitAsync throws SqlException
   
9. catch block (line 218-221): await transaction.RollbackAsync()
   → If CommitAsync already terminated the transaction, RollbackAsync
     throws "This SqlTransaction has completed; it is no longer usable."
   → throw; never reached
   → The RollbackAsync exception propagates to the ViewModel

10. ViewModel's catch (PatientRegistrationViewModel.cs:278-281):
    _dialogService.ShowError(ex.Message);
    → User sees: "This SqlTransaction has completed; it is no longer usable."
```

**Key question:** Why would CommitAsync fail if all SaveChangesAsync calls succeeded?

**Possible answers:**
- The triggers (TR_VisitTest_SyncBalance, TR_Payment_SyncBalance) do UPDATE Visit within the transaction. These UPDATES succeed during SaveChangesAsync. But `CommitAsync` itself should be trivial (just writes the transaction log). Very unlikely to fail.
- More likely: ONE of the statements in SaveChangesAsync #4 actually FAILED, but the error handling in the override OR the service catch block transforms the exception.

### Scenario B: A SqlException During SaveChangesAsync #4 → Error Cascade

```
1-6. Same as Scenario A

7. SaveChangesAsync #4 (line 213):
   a. base.SaveChangesAsync() sends the batch
   b. One statement FAILS:
      - Option 1: Patient.FullNameAr > 150 chars → truncation error
      - Option 2: Duplicate VisitTest (from UpdateVisitTestsInternalAsync double call)
                  → unique constraint violation on UQ_VisitTest
      - Option 3: SP_RecalculateVisitTotals encounters FLOAT overflow
                  during SUM(price_charged) → arithmetic overflow
   c. SQL Server returns SqlException
   d. The override at FinalLabDbContext.cs:32 propagates the exception
      (the catch at line 41 is for auditableEntries.Count == 0 || _session is null, not for exceptions)
      → Wait, there is NO try-catch in the override! The exception propagates directly.

8. catch block (VisitService.cs:218-221):
   await transaction.RollbackAsync();
   
   If the SqlException from step 7b had severity high enough or if the
   trigger fired a ROLLBACK within the batch, the transaction is now TERMINATED.
   → RollbackAsync() throws "This SqlTransaction has completed..."
   → This exception propagates to the ViewModel
   
9. ViewModel shows: "This SqlTransaction has completed; it is no longer usable."
   The ORIGINAL error (truncation, constraint violation, or overflow) is LOST.
```

### Scenario C: Double Rollback via Dispose (LESS LIKELY)

```
1-7. Same as Scenario A or B — exception occurs

8. catch block (VisitService.cs:220):
   await transaction.RollbackAsync();  ← succeeds (transaction was open)

9. throw; re-throws the original exception

10. The using statement's compiler-generated finally runs:
    transaction.Dispose();
    
    SqlTransaction.Dispose() checks internal state:
    - Was RollbackAsync() called? YES → transaction state is "rolled back"
    - Should be a no-op in normal conditions
    
    BUT: What if the RollbackAsync in step 8 updated the server-side state
    but didn't properly update the client-side SqlTransaction object?
    → Dispose() sees "pending" and calls Rollback() again
    → "This SqlTransaction has completed; it is no longer usable."
    → This exception replaces the original exception
```

### Scenario D: SP_RecalculateVisitTotals UPDATE Fails → Transaction Doomed

```
1-6. Same as Scenario A

7. During SaveChangesAsync #4:
   VisitTest INSERT fires TR_VisitTest_SyncBalance → SP_RecalculateVisitTotals
   
   Inside SP_RecalculateVisitTotals:
   SELECT @Subtotal = ISNULL(SUM(price_charged), 0) FROM VisitTest WHERE ...
   → @Subtotal = large FLOAT value
   → UPDATE Visit SET subtotal = @Subtotal
   → Subtotal column is decimal(18,2)
   → If @Subtotal exceeds decimal(18,2) range, arithmetic overflow
   → SqlException with level 16+
   
   The trigger fails → the VisitTest INSERT fails → SaveChangesAsync fails
   → The transaction might be doomed (depends on severity)
   → RollbackAsync in the catch block → "transaction completed"
```

### Summary of Likelihood

| Scenario | Likelihood | Explanation |
|----------|-----------|-------------|
| **A** (CommitAsync failure) | Low | CommitAsync rarely fails if SaveChangesAsync succeeded |
| **B** (SaveChangesAsync failure → rollback failure) | **HIGH** | Most likely — the original error is masked by the rollback error |
| **C** (Double rollback via Dispose) | Medium | Depends on SqlTransaction internal state management |
| **D** (SP overflow) | Low | Unlikely with normal test prices |

**The most probable root cause is Scenario B:** An error occurs during `SaveChangesAsync #4` (line 213), and the subsequent `RollbackAsync()` fails because the transaction was already terminated, producing the "transaction completed" message that **masks the original error**.

---

## 16. Open Questions Requiring Runtime Observation

These questions CANNOT be answered from static code analysis. They require runtime debugging, SQL Server Profiler, or EF Core logging.

### Q1: What is the ORIGINAL exception?

The most critical question. Enable `Microsoft.EntityFrameworkCore` logging at `Debug` or `Information` level in `appsettings.json` (or add it programmatically). This will show:
- The exact SQL batch sent during SaveChangesAsync #4
- Any SQL Server errors received
- The exception that triggers the catch block

**Recommended logging configuration:**
```xml
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.EntityFrameworkCore": "Information",
    "Microsoft.EntityFrameworkCore.Database.Transaction": "Debug"
  }
}
```

### Q2: Which statement in SaveChangesAsync #4 fails?

Use SQL Server Profiler to capture the exact batch sent. Look for:
- `INSERT INTO VisitTest ...` — check for duplicate (VisitId, TesttypeId) pairs
- `INSERT INTO Payment ...` — check for data type issues
- `UPDATE Visit ...` (from triggers) — check for data type or constraint violations

### Q3: Does the duplicate `UpdateVisitTestsInternalAsync` call cause a unique constraint violation?

Run the save flow for an existing visit where tests are changed. Enable Profiler and check if a duplicate key violation occurs on `UQ_VisitTest`.

### Q4: Does `SP_RecalculateVisitTotals` execute without errors?

Execute manually in SSMS:
```sql
EXEC SP_RecalculateVisitTotals @VisitID = <test_visit_id>;
```

Check for errors, especially arithmetic overflow from FLOAT-to-DECIMAL conversion.

### Q5: Does the `IDbContextTransaction.RollbackAsync()` + `Dispose()` sequence ever produce the error?

Add a temporary trace around the catch block to log:
1. When `RollbackAsync()` is called
2. Whether it throws
3. What exception is thrown
4. What the ORIGINAL exception was

### Q6: Is `ICurrentUserSession.CurrentUser` null?

In `FinalLabDbContext.cs:44`, `_session.CurrentUser?.StaffId` is used. If `CurrentUser` is null, `staffId` is null, which is fine for the `ChangedBy` column (int? allows null). But check if `_session` itself could be null.

### Q7: Are there additional triggers not discovered?

Some triggers might be defined with `sp_helptext` or `modify` methods not captured by the standard query. Run:
```sql
SELECT * FROM sys.triggers WHERE parent_class_desc = 'OBJECT_OR_COLUMN';
```

### Q8: Does the `Update Visit` in `SP_RecalculateVisitTotals` conflict with EF Core change tracking?

After the SP updates the Visit row, the ChangeTracker still has the old values. The next `CommitAsync` might detect a conflict if there were any concurrency checks. But currently there are none. Still worth verifying.

### Q9: What is the exact message AND HResult of the exception reaching the ViewModel?

The ViewModel catches `Exception ex` and displays `ex.Message`. Use a debugger or log `ex.ToString()` to get the full exception details including HResult, InnerException, and StackTrace.

### Q10: Does the error occur the FIRST time the user clicks Save (before any prior failed attempt)?

If the error occurs on the FIRST save attempt (not after a prior failed attempt), then the ChangeTracker corruption theory (Section 9) is ruled out. If it only occurs on the second+ attempt, ChangeTracker state is more likely.

---

## Appendix: Files Referenced

| File | Lines of Code | Key Role |
|------|--------------|----------|
| `ViewModels/Patients/PatientRegistrationViewModel.cs` | 369 | Entry point — SaveAsync calls SavePatientVisitAsync |
| `Services/Implementations/VisitService.cs` | 465 | Core save logic with transaction |
| `Services/Interfaces/IVisitService.cs` | 84 | Interface for VisitService |
| `Data/FinalLabDbContext.cs` | 2184 | DbContext + SaveChangesAsync override + Fluent API |
| `Data/AuditableAttribute.cs` | 8 | Marker attribute for audit logging |
| `Models/Patient.cs` | 71 | Patient entity (`[Auditable]`) |
| `Models/Visit.cs` | 113 | Visit entity (`[Auditable]`) |
| `Models/VisitTest.cs` | 90 | VisitTest entity |
| `Models/Payment.cs` | 33 | Payment entity (`[Auditable]`) |
| `Models/AuditLog.cs` | 31 | Audit log entity |
| `Models/PatientMedicalHistory.cs` | 34 | Medical history entity |
| `Models/ReferralSource.cs` | 38 | Referral source entity |
| `ViewModels/Patients/PatientInfoViewModel.cs` | 267 | Creates Patient from form data |
| `ViewModels/Patients/MedicalHistoryViewModel.cs` | 305 | Creates PatientMedicalHistory list |
| `ViewModels/Patients/TestSelectionViewModel.cs` | 251 | Loads test catalog, builds test selection |
| `App.xaml.cs` | 203 | DI registration |
| `Infrastructure/Navigation/NavigationService.cs` | 106 | Window resolution from DI |

---

*End of Report — Diagnosis Only, Zero Code Changes*

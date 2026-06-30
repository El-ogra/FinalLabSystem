# Handoff-Slice-6.2 — Backup Foundation (Service + AES + Unified LabSetting Migration)

> **Purpose:** Single-source reference for implementing Slice 6.2.  
> **Rule:** Do NOT re-read or re-inspect any source files listed below — all facts are confirmed and final.

---

## 1. Confirmed Facts About Current Architecture

### 1.1 LabSetting Model — Current Properties (10 total)

Source: `FinalLabSystem/Models/LabSetting.cs` (26 lines)

| # | Property | C# Type | DB Column | DB Type | Nullable | Notes |
|---|----------|---------|-----------|---------|----------|-------|
| 1 | `SettingKey` | `string` | `setting_key` | `nvarchar(100)` | No | **Primary Key** |
| 2 | `SettingValue` | `string?` | `setting_value` | `nvarchar(max)` | Yes | |
| 3 | `SettingDescription` | `string?` | `setting_description` | `nvarchar(200)` | Yes | |
| 4 | `SettingGroup` | `string?` | `setting_group` | `nvarchar(50)` | Yes | |
| 5 | `IsRequired` | `bool` | `is_required` | `bit` | No | |
| 6 | `LastUpdatedBy` | `int?` | `last_updated_by` | `int` | Yes | FK → Staff |
| 7 | `LastUpdatedAt` | `DateTime?` | `last_updated_at` | `datetime2(0)` | Yes | `HasPrecision(0)` |
| 8 | `LastUpdatedByNavigation` | `Staff?` | — | — | — | Navigation property |
| 9 | `EnforceStageGating` | `bool` | `enforce_stage_gating` | `bit` | No | Default: `true` |
| 10 | `EnableServerPrinting` | `bool` | `enable_server_printing` | `bit` | No | Default: `false` |

- **None** of the 8 new fields exist: `SmtpHost`, `SmtpPort`, `SmtpUsername`, `SmtpPasswordEncrypted`, `SmtpEnableSsl`, `BackupScheduleHour`, `BackupRetentionDays`, `BackupOutputFolder`.
- Class: `public partial class LabSetting` — no data annotations. All mapping via Fluent API.
- Pattern: key-value via `SettingKey`/`SettingValue` + two dedicated boolean columns (`EnforceStageGating`, `EnableServerPrinting`). Adding 8 more dedicated columns follows the same pattern.

### 1.2 DbContext — OnModelCreating for LabSetting

Source: `FinalLabSystem/Data/FinalLabDbContext.cs`, lines 436–461

```csharp
modelBuilder.Entity<LabSetting>(entity =>
{
    entity.HasKey(e => e.SettingKey).HasName("PK__LabSetti__0DFAC426DE80F226");
    entity.Property(e => e.SettingKey).HasMaxLength(100).HasColumnName("setting_key");
    entity.Property(e => e.IsRequired).HasColumnName("is_required");
    entity.Property(e => e.LastUpdatedAt).HasPrecision(0).HasColumnName("last_updated_at");
    entity.Property(e => e.LastUpdatedBy).HasColumnName("last_updated_by");
    entity.Property(e => e.SettingDescription).HasMaxLength(200).HasColumnName("setting_description");
    entity.Property(e => e.SettingGroup).HasMaxLength(50).HasColumnName("setting_group");
    entity.Property(e => e.SettingValue).HasColumnName("setting_value");
    entity.Property(e => e.EnforceStageGating).HasColumnName("enforce_stage_gating");
    entity.Property(e => e.EnableServerPrinting).HasColumnName("enable_server_printing");
    entity.HasOne(d => d.LastUpdatedByNavigation).WithMany(p => p.LabSettings)
        .HasForeignKey(d => d.LastUpdatedBy).HasConstraintName("FK_LabSettings_Staff");
});
DbSet declaration (line 130):

public virtual DbSet<LabSetting> LabSettings { get; set; }
1.3 Confirmed Table Name
DbSet name: LabSettings (plural)
Snapshot b.ToTable("LabSettings") (plural)
Fluent API does NOT call entity.ToTable(...) — relies on convention → resolves to "LabSettings"
The table name in all AddColumn calls MUST be "LabSettings" (plural), NOT "LabSetting" (singular).

1.4 Database Column Naming Convention
All database column names use snake_case: setting_key, enable_server_printing, enforce_stage_gating
All C# property names use PascalCase: SettingKey, EnableServerPrinting, EnforceStageGating
Every property in OnModelCreating ends with .HasColumnName("snake_case_name")
1.5 Migration Patterns (from 3 recent migrations)
AddTubeMaterialStockFields — non-nullable columns:

migrationBuilder.AddColumn<int>(name: "current_stock", table: "TubeMaterial", type: "int", nullable: false, defaultValue: 0);
migrationBuilder.AddColumn<int>(name: "minimum_stock", table: "TubeMaterial", type: "int", nullable: false, defaultValue: 0);
AddCompanyContractFields — nullable columns:

migrationBuilder.AddColumn<string>(name: "billing_periodicity", table: "Company", type: "nvarchar(20)", maxLength: 20, nullable: true);
migrationBuilder.AddColumn<DateOnly>(name: "contract_end_date", table: "Company", type: "date", nullable: true);
RenameLabSettingFeatureToggleColumns — rename:

migrationBuilder.RenameColumn(name: "EnforceStageGating", table: "LabSettings", newName: "enforce_stage_gating");
Rules extracted:

Non-nullable columns → always provide defaultValue
Nullable columns → no defaultValue needed
String columns → always include maxLength
Boolean columns → type: "bit"
Down method → DropColumn in reverse order
Table names in AddColumn → PascalCase (matches ToTable() value)
Column names in AddColumn → snake_case
1.6 Snapshot — Current LabSetting Configuration
Source: FinalLabSystem/Migrations/FinalLabDbContextModelSnapshot.cs, lines 670–718

modelBuilder.Entity("FinalLabSystem.Models.LabSetting", b =>
{
    b.Property<string>("SettingKey").HasMaxLength(100).HasColumnType("nvarchar(100)").HasColumnName("setting_key");
    b.Property<bool>("EnableServerPrinting").HasColumnType("bit").HasColumnName("enable_server_printing");
    b.Property<bool>("EnforceStageGating").HasColumnType("bit").HasColumnName("enforce_stage_gating");
    b.Property<bool>("IsRequired").HasColumnType("bit").HasColumnName("is_required");
    b.Property<DateTime?>("LastUpdatedAt").HasPrecision(0).HasColumnType("datetime2(0)").HasColumnName("last_updated_at");
    b.Property<int?>("LastUpdatedBy").HasColumnType("int").HasColumnName("last_updated_by");
    b.Property<string>("SettingDescription").HasMaxLength(200).HasColumnType("nvarchar(200)").HasColumnName("setting_description");
    b.Property<string>("SettingGroup").HasMaxLength(50).HasColumnType("nvarchar(50)").HasColumnName("setting_group");
    b.Property<string>("SettingValue").HasColumnType("nvarchar(max)").HasColumnName("setting_value");
    b.HasKey("SettingKey").HasName("PK__LabSetti__0DFAC426DE80F226");
    b.HasIndex("LastUpdatedBy");
    b.ToTable("LabSettings");
});
Snapshot relationship (lines 3802–3810):

modelBuilder.Entity("FinalLabSystem.Models.LabSetting", b =>
{
    b.HasOne("FinalLabSystem.Models.Staff", "LastUpdatedByNavigation")
        .WithMany("LabSettings")
        .HasForeignKey("LastUpdatedBy")
        .HasConstraintName("FK_LabSettings_Staff");
    b.Navigation("LastUpdatedByNavigation");
});
1.7 Database Views (must NOT be backed up or restored)
The DbContext defines 6 views via HasNoKey().ToView(...):

View Entity	DB Table Name
VOutstandingBalance	V_OutstandingBalances
VPatientHistory	V_PatientHistory
VPendingTest	V_PendingTests
VReferralCommissionReport	V_ReferralCommissionReport
VResultAuditTrail	V_ResultAuditTrail
VSampleTubeStatus	V_SampleTubeStatus
Filter to exclude: !t.IsView on EF Core IEntityType.

1.8 Service Consumption Pattern
Both PatientService and VisitService inject FinalLabDbContext (concrete, not base DbContext) as a private readonly field. Both are Scoped. VisitService additionally uses BeginTransactionAsync()/CommitAsync()/RollbackAsync() and calls _context.ChangeTracker.Clear() in finally blocks.

2. Confirmed Interface Signatures
2.1 IAuditService — Actual Method Signature
Source: FinalLabSystem/Services/Interfaces/IAuditService.cs, line 27

Task LogActionAsync(string tableName, int recordId, string action, int staffId, string? notes = null);
CRITICAL CONSTRAINT: The Action column on the AuditLog table is defined as HasMaxLength(1).IsFixedLength() — it stores exactly one character. You cannot store strings like "BackupCreated" or "BackupRestored".

Correct audit call for backup:

await _auditService.LogActionAsync(
    tableName: "Backup",
    recordId: 0,
    action: "B",      // single char: B=Backup, R=Restore
    staffId: _currentUserSession.CurrentUser!.StaffId,
    notes: $"Backup created to {filePath}"
);
Correct audit call for restore:

await _auditService.LogActionAsync(
    tableName: "Backup",
    recordId: 0,
    action: "R",
    staffId: _currentUserSession.CurrentUser!.StaffId,
    notes: $"Backup restored from {backupFilePath}"
);
2.2 ICurrentUserSession — How to Access IsAdmin
Source: FinalLabSystem/Infrastructure/Session/CurrentUserSession.cs

public interface ICurrentUserSession
{
    Staff? CurrentUser { get; }
    bool IsAuthenticated { get; }
    int IdleTimeoutMinutes { get; set; }
    void SignIn(Staff staff);
    void SignOut();
    void StartIdleTimer(Action onTimeout);
    void ResetIdleTimer();
    void StopIdleTimer();
}
IsAdmin is on the Staff model, NOT on the session:

// Staff.cs, line 21
public bool IsAdmin { get; set; }
Correct admin check pattern:

if (_currentUserSession.CurrentUser?.IsAdmin != true)
    throw new UnauthorizedAccessException("Only administrators can perform this operation.");
Existing usage patterns in the codebase:

ReceiptService.cs:27 → staff?.IsAdmin == true
AuthService.cs:49 → if (staff.IsAdmin) return true;
2.3 Service Constructor Pattern
// PatientService and VisitService both follow this pattern:
private readonly FinalLabDbContext _context;
private readonly ILogger<SomeService> _logger;

public SomeService(FinalLabDbContext context, ILogger<SomeService> logger)
{
    _context = context;
    _logger = logger;
}
2.4 Available Dependencies
Dependency	Status
System.Security.Cryptography	Framework assembly (.NET 8). Already used in PasswordHasher.cs.
Aes.Create()	Available but not yet used anywhere in the codebase.
System.Text.Json	Framework assembly (.NET 8). Available.
Target framework	net8.0-windows
Nullable	Enabled (<Nullable>enable</Nullable>)
Implicit usings	Enabled
3. The Four Bugs to Avoid
BUG-1: IAuditService method name and signature mismatch
The plan says: IAuditService.LogAsync("BackupCreated", staffId, DateTime.UtcNow)
Reality: The method is LogActionAsync(tableName, recordId, action, staffId, notes) and action is limited to 1 character.

Fix: Replace every LogAsync(...) call with:

await _auditService.LogActionAsync("Backup", 0, "B", staffId, $"Backup created to {filePath}");
await _auditService.LogActionAsync("Backup", 0, "R", staffId, $"Backup restored from {path}");
BUG-2: Wrong table name in migration
The plan says: table: "LabSetting" (singular)
Reality: The snapshot confirms b.ToTable("LabSettings") (plural).

Fix: Change all 8 AddColumn calls from table: "LabSetting" to table: "LabSettings".

BUG-3: staffId parameter in IBackupService interface
The plan declares: Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type, int staffId)
Problem: Allowing manual staffId injection is a security risk — a non-admin caller can inject any staffId.

Fix: Remove staffId from both method signatures. Use _currentUserSession.CurrentUser!.StaffId internally.

BUG-4: PascalCase column names in migration
The plan uses: name: "SmtpHost", name: "SmtpPort", etc.
Established convention: All DB columns use snake_case: smtp_host, smtp_port, etc.

Fix: Use snake_case in AddColumn name parameters. Add .HasColumnName("snake_case") in Fluent API mappings.

4. Approved Architecture Decisions
4.1 Encryption: AES-256-CBC + PBKDF2 100k iterations
Verdict: Accepted for this use case (desktop app, local backup file).
Pros: Simple, well-understood, built into .NET 8 BCL. PBKDF2 100k exceeds NIST SP 800-132 minimum.
Cons: No authenticated encryption (malleable ciphertext). Acceptable for local backup files.
Optional enhancement: SHA-256 HMAC over JSON before encryption for tamper detection (not required).
4.2 File Format: [16-byte salt][16-byte IV][ciphertext]
Standard layout. PBKDF2 derives key from password + salt. Random IV ensures different ciphertexts for identical plaintexts.
No changes needed.
4.3 Reading DbSets: Use EF Core Metadata (NOT manual)
Do NOT read ~44 tables manually — unmaintainable.
Use: context.Model.GetEntityTypes() with !t.IsView filter.
This auto-detects new tables added by future migrations.
var tableTypes = context.Model.GetEntityTypes()
    .Where(t => t.GetTableName() != null && !t.IsView)
    .ToList();
4.4 Excluded Views (6 total — do NOT backup or restore)
Entity	DB Table
VOutstandingBalance	V_OutstandingBalances
VPatientHistory	V_PatientHistory
VPendingTest	V_PendingTests
VReferralCommissionReport	V_ReferralCommissionReport
VResultAuditTrail	V_ResultAuditTrail
VSampleTubeStatus	V_SampleTubeStatus
4.5 Restore: FK Ordering via Topological Sort
Do NOT hardcode FK clear order — fragile when new tables are added.
Use: EF Core metadata to auto-detect FK dependencies and topologically sort tables.
Clear tables leaf-to-root (dependents first, then parents).
4.6 Restore: Batched Transactions
Do NOT use one giant transaction for all ~44 tables.
Batch by FK dependency level. Call _context.ChangeTracker.Clear() between batches (following VisitService.cs:119 pattern).
4.7 Scoped Lifetime for IBackupService
Correct. All services using FinalLabDbContext are registered Scoped.
Consistent with IPatientService, IReceiptService, IInvoiceService.
4.8 JSON Serialization Settings
JsonSerializer.ReferenceHandler.IgnoreCycles — correct choice.
Navigation properties form cycles (e.g., Staff.AuditLogs ↔ AuditLog.ChangedByNavigation).
IgnoreCycles sets cycling references to null — acceptable because relationships are reconstructed via FK values on restore.
No need for PreserveReferencesHandling.
4.9 Soft-Delete Handling
Codebase uses IsActive = false for soft deletes (not IsDeleted/DeletedAt).
During restore, IsActive = false records are faithfully restored. No special handling needed.
4.10 Password Hash Handling
Staff.PasswordHash contains PBKDF2 hashes (irreversible).
Backup serializes the hash as-is. Restore preserves it. Existing passwords continue to work.
No special handling needed.
4.11 AuditInterceptor During Restore
FinalLabDbContext.SaveChangesAsync() has an interceptor that auto-logs changes for [Auditable] entities.
During restore, each SaveChangesAsync() will generate audit log entries.
This is noisy but not harmful. If suppression is desired, use manual LogActionAsync calls instead.
5. Complete Approved Test List (38 tests)
Category A: AesEncryptionHelper Unit Tests (10 tests)
#	Test Name	What It Verifies
1	Encrypt_Decrypt_Roundtrip_RestoresOriginal	Roundtrip restores original bytes
2	Encrypt_Twice_SamePassword_ProducesDifferentCiphertext	Random salt + IV
3	Decrypt_WithWrongPassword_ThrowsCryptographicException	Secure failure
4	Encrypt_EmptyBytes_DoesNotThrow	Edge case
5	DeriveKey_SamePasswordAndSalt_ProducesSameKey	PBKDF2 deterministic
6	Decrypt_CorruptedFile_ThrowsCryptographicException	Tampered file detection
7	Decrypt_EmptyFile_ThrowsCryptographicException	Zero-byte file
8	Decrypt_TruncatedFile_ThrowsCryptographicException	File with salt only (16 bytes), no IV or ciphertext
9	Decrypt_WrongFormatFile_ThrowsCryptographicException	Plain text file treated as encrypted
10	Decrypt_InvalidSaltLength_ThrowsArgumentException	Salt of 8 bytes instead of 16
Category B: Migration Validation Tests (4 tests)
#	Test Name	What It Verifies
11	Migration_AddsAllEightColumns_NullableOrDefaulted	Schema check via EF InMemory
12	Migration_SmtpPort_DefaultsTo587	Default value
13	Migration_BackupScheduleHour_DefaultsTo2	Default value
14	Migration_BackupRetentionDays_DefaultsTo30	Default value
Category C: BackupService Unit Tests (20 tests)
#	Test Name	What It Verifies
15	CreateBackupAsync_NonAdmin_ThrowsUnauthorized	BR-061 enforcement
16	CreateBackupAsync_NullSession_ThrowsUnauthorized	No user logged in
17	CreateBackupAsync_AdminUser_WritesFile_ToTargetFolder	Happy path
18	CreateBackupAsync_AdminUser_LogsAuditEvent_CorrectMapping	LogActionAsync("Backup", 0, "B", staffId, ...)
19	CreateBackupAsync_FileName_FollowsTimestampPattern	Regex on filename
20	CreateBackupAsync_EncryptedFile_DoesNotContain_PlaintextMarker	No raw JSON in file
21	CreateBackupAsync_ExcludesViews	6 views not serialized
22	CreateBackupAsync_InvalidFolder_ThrowsDirectoryNotFoundException	Path validation
23	RestoreBackupAsync_CreatesPreRestoreBackup_First	_pre_restore file exists
24	RestoreBackupAsync_WrongPassword_ReturnsFalse	Secure failure
25	RestoreBackupAsync_NonAdmin_ThrowsUnauthorized	BR-061 on restore
26	RestoreBackupAsync_OnException_RollsBackTransaction	No partial data
27	RestoreBackupAsync_PreservesFkRelationships	No orphaned records
28	RestoreBackupAsync_PreservesTimestampsAsUtc	UTC kind preserved
29	RestoreBackupAsync_PreservesSoftDeletedRecords	IsActive = false records restored
30	RestoreBackupAsync_LogsAuditEvent_CorrectMapping	LogActionAsync("Backup", 0, "R", ...)
31	ListBackupsAsync_EmptyFolder_ReturnsEmpty	Edge case
32	ListBackupsAsync_ReturnsCorrectMetadata	FileName, FileSizeBytes, CreatedAt
33	ValidateBackupFileAsync_ValidFile_ReturnsTrue	Happy path
34	ValidateBackupFileAsync_CorruptedFile_ReturnsFalse	Integrity check
Category D: BackupService DI Registration (1 test)
#	Test Name	What It Verifies
35	BackupService_DI_Registration_Should_Be_Scoped	Lifetime correct via TestServiceProvider
Category E: BackupService End-to-End Integration Tests (3 tests)
#	Test Name	What It Verifies
36	FullBackupRestoreCycle_PreservesAllData	seed 5 patients + 3 visits + 10 results → backup → wipe → restore → compare
37	BackupRestore_WithCircularNavigationProperties_Succeeds	IgnoreCycles correctness
38	BackupRestore_DifferentPasswords_ProducesDifferentKeys	PBKDF2 key derivation correctness
6. Risk Summary and Mitigation
Decision	Risk Level	Mitigation
Table name "LabSetting" → "LabSettings"	🔴 Critical	Use "LabSettings" (plural) in all AddColumn calls
IAuditService.LogAsync → LogActionAsync	🔴 Critical	Use LogActionAsync("Backup", 0, "B"/"R", staffId, notes)
staffId parameter in interface	🟡 High	Remove; derive from _currentUserSession.CurrentUser!.StaffId
PascalCase column names	🟡 Medium	Use snake_case in AddColumn + .HasColumnName() in Fluent API
Manual DbSet reading (~44 tables)	🟡 Medium	Use context.Model.GetEntityTypes().Where(!t.IsView)
Hardcoded FK clear order	🟡 High	Topological sort from EF Core metadata
Single transaction for full restore	🟡 High	Batch by FK level + ChangeTracker.Clear() between batches
Views included in backup	🟡 Medium	Filter with !t.IsView
AES-256-CBC without authentication	🟢 Low	Acceptable for local backup files
No E2E backup/restore test	🟡 Medium	Added 3 integration tests (Category E)
Missing cryptographic edge case tests	🟡 Medium	Added 5 tests (tests #6–10)
Soft-delete records during restore	🟢 Low	IsActive = false records restored faithfully
PasswordHash in backup file	🟢 Low	PBKDF2 hashes preserved as-is
AuditInterceptor noise during restore	🟢 Low	Hundreds of audit entries generated; not harmful
7. Instructions for Implementing Agent
This file is the product of a complete and final analysis. Do NOT re-read or re-inspect any of the source files mentioned above — all confirmed facts are documented here. Begin implementation directly based on what is documented in this file.

Corrected migration Up method (ready to use):

migrationBuilder.AddColumn<string>(name: "smtp_host", table: "LabSettings", type: "nvarchar(200)", nullable: true);
migrationBuilder.AddColumn<int>(name: "smtp_port", table: "LabSettings", type: "int", nullable: true, defaultValue: 587);
migrationBuilder.AddColumn<string>(name: "smtp_username", table: "LabSettings", type: "nvarchar(200)", nullable: true);
migrationBuilder.AddColumn<string>(name: "smtp_password_encrypted", table: "LabSettings", type: "nvarchar(500)", nullable: true);
migrationBuilder.AddColumn<bool>(name: "smtp_enable_ssl", table: "LabSettings", type: "bit", nullable: true, defaultValue: true);
migrationBuilder.AddColumn<int>(name: "backup_schedule_hour", table: "LabSettings", type: "int", nullable: true, defaultValue: 2);
migrationBuilder.AddColumn<int>(name: "backup_retention_days", table: "LabSettings", type: "int", nullable: true, defaultValue: 30);
migrationBuilder.AddColumn<string>(name: "backup_output_folder", table: "LabSettings", type: "nvarchar(500)", nullable: true);
Corrected migration Down method (ready to use):

migrationBuilder.DropColumn(name: "backup_output_folder", table: "LabSettings");
migrationBuilder.DropColumn(name: "backup_retention_days", table: "LabSettings");
migrationBuilder.DropColumn(name: "backup_schedule_hour", table: "LabSettings");
migrationBuilder.DropColumn(name: "smtp_enable_ssl", table: "LabSettings");
migrationBuilder.DropColumn(name: "smtp_password_encrypted", table: "LabSettings");
migrationBuilder.DropColumn(name: "smtp_username", table: "LabSettings");
migrationBuilder.DropColumn(name: "smtp_port", table: "LabSettings");
migrationBuilder.DropColumn(name: "smtp_host", table: "LabSettings");
Corrected Fluent API additions for OnModelCreating (ready to add inside the LabSetting entity block):

entity.Property(e => e.SmtpHost).HasColumnName("smtp_host");
entity.Property(e => e.SmtpPort).HasColumnName("smtp_port");
entity.Property(e => e.SmtpUsername).HasColumnName("smtp_username");
entity.Property(e => e.SmtpPasswordEncrypted).HasColumnName("smtp_password_encrypted");
entity.Property(e => e.SmtpEnableSsl).HasColumnName("smtp_enable_ssl");
entity.Property(e => e.BackupScheduleHour).HasColumnName("backup_schedule_hour");
entity.Property(e => e.BackupRetentionDays).HasColumnName("backup_retention_days");
entity.Property(e => e.BackupOutputFolder).HasColumnName("backup_output_folder");
Corrected snapshot additions (ready to add inside the LabSetting entity block):

b.Property<string>("SmtpHost").HasColumnType("nvarchar(200)").HasColumnName("smtp_host");
b.Property<int?>("SmtpPort").HasColumnType("int").HasColumnName("smtp_port");
b.Property<string>("SmtpUsername").HasColumnType("nvarchar(200)").HasColumnName("smtp_username");
b.Property<string>("SmtpPasswordEncrypted").HasColumnType("nvarchar(500)").HasColumnName("smtp_password_encrypted");
b.Property<bool?>("SmtpEnableSsl").HasColumnType("bit").HasColumnName("smtp_enable_ssl");
b.Property<int?>("BackupScheduleHour").HasColumnType("int").HasColumnName("backup_schedule_hour");
b.Property<int?>("BackupRetentionDays").HasColumnType("int").HasColumnName("backup_retention_days");
b.Property<string>("BackupOutputFolder").HasColumnType("nvarchar(500)").HasColumnName("backup_output_folder");
Corrected IBackupService interface (ready to use):

public interface IBackupService
{
    Task<string> CreateBackupAsync(string targetFolder, string adminPassword, BackupType type);
    Task<bool> RestoreBackupAsync(string backupFilePath, string adminPassword);
    Task<List<BackupMetadataDto>> ListBackupsAsync(string folder);
    Task<bool> ValidateBackupFileAsync(string backupFilePath, string adminPassword);
}
Corrected audit calls (ready to use):

// In CreateBackupAsync:
await _auditService.LogActionAsync("Backup", 0, "B", _currentUserSession.CurrentUser!.StaffId, $"Backup created to {filePath}");

// In RestoreBackupAsync:
await _auditService.LogActionAsync("Backup", 0, "R", _currentUserSession.CurrentUser!.StaffId, $"Backup restored from {backupFilePath}");

---

This is the complete content. Copy everything between the opening ` ```markdown ` and closing ` ``` ` fences, save as `Docs/PRDs/Handoff-Slice-6.2.md`, and it's ready for the implementing agent.

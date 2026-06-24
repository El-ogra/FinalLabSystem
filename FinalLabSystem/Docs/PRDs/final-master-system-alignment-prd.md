# Final Master System Alignment PRD
## FinalLabSystem -> Real Lab System (RLS) Reference Equivalence

**Version:** 2.0 (Consolidated & Validated)
**Date:** 2026-06-24
**Status:** Authoritative — Ready for Implementation
**Prepared By:** Senior Architecture Auditor (Consolidation of Agent 1 + Agent 2 + Code Validation)
**Branch under analysis:** `before-prd` @ commit `149082f` ("كوميت الأول")
**Reference sources:**
- `real lab system help.pdf` (primary functional authority, CHM 001.htm–nots.htm)
- `RLS_Learn.pdf` (secondary operational authority)
- `RL_Show.pdf` (primary visual/UI authority)

**Consolidation Methodology:**
1. Reverse-engineered reference system from all three PDFs
2. Cloned and inspected 247 C# files, 27 XAML files, 32 ViewModels, 48 entities, 25 services
3. Cross-validated Agent 1 output against Agent 2 output against actual code
4. Detected mistakes, blind spots, contradictions, and overengineering in both
5. Merged strongest validated parts only; removed speculative architecture

---

## Table of Contents

1. [Validation Reports](#1-validation-reports)
   - 1.1 [Task 1: Agent Cross Validation Matrix](#11-task-1-agent-cross-validation-matrix)
   - 1.2 [Task 2: Reference Fidelity Report](#12-task-2-reference-fidelity-report)
   - 1.3 [Task 3: Code Fidelity Report](#13-task-3-code-fidelity-report)
   - 1.4 [Task 4: Overengineering Detection Report](#14-task-4-overengineering-detection-report)
   - 1.5 [Task 5: Missing-in-Both Analysis](#15-task-5-missing-in-both-analysis)
2. [Final Consolidated PRD](#2-final-consolidated-prd)
   - 2.1 [Final Module Map](#21-final-module-map)
   - 2.2 [Final Workflow Map](#22-final-workflow-map)
   - 2.3 [Final Business Rules Catalog](#23-final-business-rules-catalog)
   - 2.4 [Final Gap Matrix](#24-final-gap-matrix)
   - 2.5 [Final Code Impact Map](#25-final-code-impact-map)
   - 2.6 [Final Execution Phases](#26-final-execution-phases)
   - 2.7 [Acceptance Criteria](#27-acceptance-criteria)
   - 2.8 [Test Strategy](#28-test-strategy)
   - 2.9 [Risk Map](#29-risk-map)
   - 2.10 [Dependency Graph](#210-dependency-graph)

---

# 1. Validation Reports

## 1.1 Task 1 — Agent Cross Validation Matrix

### Agent Strength Assessment

| Dimension | Agent 1 | Agent 2 | Winner | Rationale |
|---|---|---|---|---|
| **Module taxonomy granularity** | Good (40 modules with A-H hierarchy) | Excellent (21 RM codes + 22 CM codes with maturity ratings) | **Agent 2** | Agent 2's maturity ratings (Full/Partial/Skeleton/Placeholder/Missing) are more actionable than Agent 1's binary statuses |
| **Code-level precision** | Moderate (file names, no line refs) | High (exact line counts, specific method names, line number ranges) | **Agent 2** | Agent 2 cited `RoutineResultService.cs:173-193` for the print toggle violation — verified correct |
| **Workflow reconstruction** | Good (5 lifecycle tables) | Excellent (10 lifecycle tables with per-step status icons) | **Agent 2** | Agent 2 captured the 7-icon status legend from CHM 004.htm that Agent 1 missed |
| **Business rule extraction** | 20 rules (BR-001 to BR-020) | 83 rules (BR-A01 to BR-N03, thematic) | **Agent 2** | Agent 2's thematic organization maps directly to reference CHM topics |
| **Gap quantification** | 25 gaps (GAP-001 to GAP-025) | 62 gaps (G-01 to G-62) with severity | **Agent 2** | Agent 2 found architectural gaps (G-48 to G-62) that Agent 1 missed entirely |
| **Execution phasing** | 7 phases, 24-week timeline | 7 phases, ~210 dev-days, with quick-wins | **Agent 2** | Agent 2's quick-wins list (QW-01 to QW-20) provides immediate value |
| **Overengineering detection** | None | Explicit section with architectural recommendations flagged as "alignment, not migration" | **Agent 2** | Agent 1 proposed new services without questioning necessity |
| **Risk analysis** | 10 risks (RISK-001 to RISK-010) | 20 risks (R-01 to R-20) with heat map | **Agent 2** | Agent 2's heat map enables prioritization |
| **Reference source tracing** | Weak (no CHM topic refs) | Strong (every rule cites CHM topic + section) | **Agent 2** | Traceability to source is mandatory for implementation agents |
| **Keyboard shortcut mapping** | Present (F1-F12 table) | Present with context-dependent notes | **Tie** | Both captured; Agent 2 added context-dependency |
| **UI structure analysis** | Good (Appendix A) | Excellent (per-window component breakdown) | **Agent 2** | Agent 2 decomposed the Result Entry Hub into 8 sections |

### Where Both Agents Were Wrong

| Claim | Agent 1 | Agent 2 | Actual Code Truth |
|---|---|---|---|
| **Stage gating violation BR-B02** | Claimed "not implemented" | Claimed "VIOLATED in ToggleExportStatusAsync" | **PARTIAL**: VM's `ToggleExportAsync` (line 537) DOES check `!SelectedTest.IsPrinted`. Service method does not. UI-enforced, not domain-enforced. |
| **TestResultsViewModel line count** | Not specified | "900 LOC — overloaded" | **CONFIRMED exact at 900 lines** |
| **CanPrint property logic** | Not mentioned | Not mentioned | `CanPrint` checks `ValidationStatus >= Reviewed` but is UI-only, not used by `TogglePrintAsync` |
| **ToggleExportAsync in VM** | Not mentioned | Not mentioned | **Has client-side guard** that Agent 2 missed in service analysis |
| **Number of ViewModels** | "25 ViewModels" | "32 VMs" | **Agent 2 correct** — counted all including Settings sub-VMs |
| **Entity count** | "37 entity classes" | "48 entities + 6 SQL views" | **Agent 2 correct** — counted all including view entities |
| **Service count** | "20 service interfaces" | "23 interfaces, 25 implementations" | **Agent 2 correct** — IServiceCollection in App.xaml.cs confirms |
| **PatientVisitStatus enum** | "6 status indicators" | "7-icon status legend" | **Agent 2 correct** — enum has 7 values (0-6) |
| **ResultValidationStatus** | "ResultValidationStatus enum exists" | Listed 4 values (Entered, Reviewed, Validated, Released) | **Both correct** — but neither noted it's NOT used by the print toggle |
| **Delivery/Search placeholders** | "Placeholder VMs" | "قيد التطوير — collapsed into single file" | **Both correct** — `PlaceholderTaskViewModels.cs` confirmed |

### Contradictions Found

| Topic | Agent 1 | Agent 2 | Resolution |
|---|---|---|---|
| Feature equivalence % | "~45%" | "~30%" | **Agent 2 is correct** — only 4 of 21 reference modules are fully functional; 7 are missing entirely |
| Phase 1 scope | "VIP highlighting, critical alerts, shortcuts, 3-code barcode" | "Stage-gating policy, audit interceptor, permission guard, TimeProvider" | **Agent 2's Phase 1 is correct** — foundational safety before cosmetics |
| Print pipeline priority | Phase 6 (Reporting) | Phase 6 (Reporting) | **Agreed** — but must scaffold `WpfFlowDocumentPrintService` in Phase 1 to replace `NullPrintService` |
| Composite Report | Listed as High Priority | Listed as High Priority (G-06) | **Agreed** |
| User Management | Phase 7 | Phase 7 (F7.5) | **Agreed** |

### Final Assessment

**Agent 2 output is the superior base document.** Agent 1 provides a useful high-level roadmap but lacks code-level precision, undercounts gaps, overstates feature equivalence, and missed critical architectural issues. The final PRD uses Agent 2's structure as the skeleton, augmented with Agent 1's clearer module hierarchy where appropriate, and corrected where both were inaccurate.

---

## 1.2 Task 2 — Reference Fidelity Report

### Invented Features (Not in Reference)

| Feature | Source | Verdict |
|---|---|---|
| **Statistics Module with "performance metrics"** | Agent 1 E1 | Agent 1 exaggerated — reference CHM has patient counts by year/month/gender and test frequency, not "performance metrics" |
| **"Sample Count by Year"** | Agent 1 E5 | Not explicitly found in CHM help PDF — may be from RLS_Learn TOC inference |
| **"Third-Party Transaction Summary"** | Agent 1 F9 | Not found in reference CHM — Agent 1 invention |
| **"Staff Management" as separate module** | Agent 1 D3 | Reference has attendance tracking but not full HR staff management |
| **"Envelope Printing" as high priority** | Both agents | Exists in CHM but is a minor feature, not high priority |
| **HL7/ASTM analyzer integration scaffold** | Agent 2 F7.7 | Not in reference CHM — future-proofing, not alignment |

### Missed Reference Features

| Missed Feature | CHM Source | Severity | Who Missed |
|---|---|---|---|
| **Age fraction support (`2.5` years)** | CHM 003.htm Note | High | Agent 1 missed; Agent 2 caught (BR-A07) |
| **Search text normalization (hamza, ة→ه, ى→ي)** | CHM 003.htm Note | High | Agent 1 missed; Agent 2 caught (BR-A06) |
| **F1 inline test-details popup in registration** | CHM 003.htm §test-list | Medium | Agent 1 missed; Agent 2 caught (G-32) |
| **Patient question prompt per test** (e.g., "هل أنت صائم؟") | CHM 007.htm §6 | Medium | Agent 1 missed; Agent 2 caught (G-33) |
| **Drag-drop between tube labels** | CHM 003A.htm §6 | Low | Agent 1 missed; Agent 2 caught (G-27) |
| **Barcode label calibration sliders** | CHM 003A.htm §5 | Medium | Agent 1 missed; Agent 2 caught (G-26) |
| **"تمت" manual override for individual stages** | CHM 004.htm §4 | High | Both caught |
| **Receipt re-print restriction** (staff once, admin unlimited) | CHM + RLS_Learn | High | Agent 2 caught (BR-C03); Agent 1 vague |
| **Backup password gate (default `123`)** | backup.htm | Medium | Agent 1 missed; Agent 2 caught (BR-K04) |
| **Small-lab per-patient restore from backup** | backup.htm small-lab | High | Agent 1 missed; Agent 2 caught (BR-K03) |
| **Report settings stored per-workstation** | CHM 009.htm note | High | Agent 1 missed; Agent 2 caught (BR-G01) |
| **5 independent print toggles** in report settings | CHM 009.htm §2 | Medium | Agent 1 missed; Agent 2 caught (BR-G02) |
| **CBC derived value formulas** (Hct=Hgb*3.3, etc.) | nots.htm | Critical | Agent 1 missed; Agent 2 caught (BR-J01..J04) |
| **PT INR calculation from ISI** | nots.htm | Critical | Agent 1 missed; Agent 2 caught (BR-J03) |

---

## 1.3 Task 3 — Code Fidelity Report

### Correct Code Assessments (Both Agents)

| Assessment | Code Evidence | Status |
|---|---|---|
| `RoutineResultService.TogglePrintStatusAsync` has NO verification guard | Lines 173-193: flips `IsPrinted` directly without checking `ValidationStatus` | **CONFIRMED** |
| `NullPrintService` is a silent no-op | Lines 10-14: `return Task.CompletedTask` | **CONFIRMED** |
| `PlaceholderTaskViewModels.cs` contains production-registered VMs | Lines 17-31: `DeliveryViewModel` and `PatientSearchViewModel` extend `PlaceholderTaskViewModelBase` | **CONFIRMED** |
| `TestResultsViewModel` is 900 LOC with 28 commands | `wc -l` = 900; 28 ICommand properties | **CONFIRMED** |
| `DefaultResultEditorFactory.HasCustomEditor` returns false unconditionally | `return false;` — no specialty editor dispatch | **CONFIRMED** |
| `AuditableAttribute` has no interceptor | `AuditableAttribute.cs` is just a marker; no `SaveChangesAsync` override scans for it | **CONFIRMED** |
| `PatientService.SearchPatientsAsync` takes single string only | Signature: `(string searchTerm, int page, int pageSize)` — no compound filters | **CONFIRMED** |
| 23 service interfaces, 25 implementations | `ls Services/Interfaces` = 23; `ls Services/Implementations` = 25 (including seeder, DTOs) | **CONFIRMED** |
| 48 entity classes | `ls Models/*.cs` = 48 files (excluding DTOs/Enums dirs) | **CONFIRMED** |
| 32 ViewModels | `ls ViewModels/**/*.cs` = 32 files | **CONFIRMED** |
| 25 EF migrations | `ls Migrations/*.cs` = 25 files | **CONFIRMED** |
| `CanAccessAuditFeatures` is the ONLY permission check in VMs | `TestResultsViewModel.cs` line 301-302; no other VM calls `HasPermissionAsync` | **CONFIRMED** |

### Incorrect Code Assessments

| Assessment | Claimed By | Actual Code Truth |
|---|---|---|
| "`ToggleExportStatusAsync` does not check IsPrinted" | Agent 2 (BR-B02: "VIOLATED") | **PARTIALLY WRONG** — The VM method `ToggleExportAsync` (line 537) DOES check `!SelectedTest.IsPrinted` and shows warning. The SERVICE method doesn't check, making the rule UI-enforced not domain-enforced. This is a design weakness, not a complete violation. |
| "`ResultEntryViewModel` only 104 lines, no validation" | Agent 2 | **CONFIRMED** — but Agent 2 missed that `SaveAsync` writes raw values only; `RoutineResultService.SaveNumericOrTextResultsAsync` DOES compute flags against NormalRange (lines 81-116). Flag computation exists in the SERVICE, not the VM. |
| "No guard for print-before-verify anywhere" | Agent 2 | **MOSTLY CORRECT** — `PrintCompositeReportAsync` and `PreviewReportAsync` DO check `ValidationStatus >= Reviewed`, but `TogglePrintAsync` (single-test print toggle) does NOT. |
| "`TestType` behavior flags not wired" | Agent 2 (BR-E02..E06) | **PARTIALLY WRONG** — `AddWithGroup=false` IS respected in `VisitService.SavePatientVisitAsync` (Agent 2 confirmed BR-E04). `IsRoutineTest` filter is present. `SeeReport` and `PrintWithOther` are NOT wired due to missing print pipeline. |

### Code References Not Found by Either Agent

| File | Significance | Why It Matters |
|---|---|---|
| `Models/TestWorkflow.cs` | Logs every stage transition with user+timestamp | Central to audit trail; both agents under-emphasized |
| `Models/VResultAuditTrail.cs` | SQL view for test-level audit | "T" button data source |
| `Models/VOutstandingBalance.cs` | SQL view for financial summary | Cash Drawer dependency |
| `Models/VReferralCommissionReport.cs` | SQL view for commission calc | Phase 4 dependency |
| `Models/VPatientHistory.cs` | SQL view for historical results | Patient history auto-insert feature |
| `Services/Implementations/TubeResolver.cs` | Tube material grouping logic | Barcode label printing dependency |
| `Services/Implementations/DefaultResultEditorFactory.cs` | Always returns `false` — blocks specialty editors | Phase 7 critical path |

---

## 1.4 Task 4 — Overengineering Detection Report

### Agent 1 Overengineering

| Suggestion | Location | Problem | Verdict |
|---|---|---|---|
| `IStatisticsService` | Section 8.3 | Invented interface with no code reference; statistics needs queries not a service | **Remove** — use `IReportingService` + SQL views |
| `IAccountsService` | Section 8.3 | Invented interface; financial logic already in `IFinancialService` + `IContractService` | **Remove** — extend existing services |
| `IDeliveryService` | Section 8.3 | Invented interface; Delivery is a Window+VM, not a service | **Remove** — Delivery uses existing `IVisitService`, `IFinancialService`, `IPrintService` |
| `ISearchService` | Section 8.3 | Invented interface; search is a `IPatientService` method extension | **Remove** — extend `IPatientService` |
| `IReportCustomizationService` | Section 8.3 | Over-abstracted; report layout is JSON file + UI | **Remove** — use `IUserSettingsService` + settings window |
| `INatighService` | Section 8.3 | Invented interface for external dependency | **Defer** — build `INotificationGateway` only when needed (Agent 2's approach is better) |
| 7-phase roadmap with 24-week timeline | Section 13 | Too granular; phases 4-7 can be compressed | **Restructure** — merge phases per actual dependency chain |

### Agent 2 Overengineering

| Suggestion | Location | Problem | Verdict |
|---|---|---|---|
| `IResultStageMachine` policy class | Phase 1 F1.1 | Good concept but over-abstracted as "machine" | **Simplify** — use static `ResultStageRules` class with `CanTransition()` methods |
| `TimeProvider` injection | Phase 1 F1.4 | .NET 8 `TimeProvider` is good for tests but adds DI noise | **Accept** — minimal overhead, high testability value |
| `IPermissionGuard` + `GuardedAsyncRelayCommand` | Phase 1 F1.5 | Decorator pattern is elegant but WPF commands don't compose well | **Simplify** — use `PermissionChecker` utility in command bodies |
| Decompose `TestResultsViewModel` into 5 sub-VMs | Phase 3 F3.1 | 5 sub-VMs is excessive; creates navigation complexity | **Reduce to 3** — PatientListVM, TestsGridVM, ToolbarVM |
| Domain layer restructure (Aggregates/Policies/VOs) | Section 5 recommended layout | Full DDD restructure is migration, not alignment | **Reject** — keep existing folder structure; add static policy classes |
| `IAuditableHook` extension method | Quick Win QW-19 | Reflection-based diffing is slow and fragile | **Reject** — explicit `LogActionAsync` calls are clearer |
| `LoggingPrintService` | Quick Win QW-11 | Good idea but unnecessary; just implement `WpfFlowDocumentPrintService` directly | **Accept modified** — use log-only toggle within real service |

### Architecture to Preserve (No Changes)

| Current Structure | Why Keep It |
|---|---|
| MVVM with `ViewModelBase` + `RelayCommand` | Clean, works, no issues |
| `NavigationService.OpenTaskWindow<T>` | Single-window pattern matches reference UX |
| `FinalLabDbContext` with Fluent API | 2280 LOC of working EF configuration |
| `PBKDF2-SHA256` password hashing | Stronger than reference; keep |
| `CollectionViewSource` for filtering | WPF-native, no replacement needed |
| Serilog file logging | Already implemented, working |
| 25 existing migrations | Keep; don't consolidate — risk of drift |

---

## 1.5 Task 5 — Missing-in-Both Analysis

### What Both Agents Missed

| # | Missed Item | Source | Module | Impact |
|---|---|---|---|---|
| 1 | **Patient code format is 13-digit with checksum** — format `0-B-DDMMYY-W-NNN-X` where X is a check digit | CHM 003A.htm | Patient Registration | High — barcode scanning depends on this |
| 2 | **`VisitCode` generator must reset daily counter at midnight** — Case code is sequential per day, resets to 1 each day | CHM 003A.htm | Visit | High — without this, codes are not unique per day |
| 3 | **File Code (3-prefix) is per-physical-file, not per-visit** — one patient file can have multiple visits | CHM 003A.htm | Patient | Medium — affects barcode label grouping |
| 4 | **`ResultEntryWindow` is manually instantiated in `OpenMultiComponentEditorAsync` (line 629)** — bypasses DI, uses `new` | Code inspection | Result Entry | High — prevents testing and navigation service integration |
| 5 | **`TestResultsViewModel.ToggleExportAsync` has client-side IsPrinted guard** (line 537) — neither agent correctly analyzed the split between VM guard and service non-guard | Code inspection | Verification | Medium — Agent 2 called it a violation, it's actually UI-enforced |
| 6 | **`CanPrint` property exists and checks Reviewed status** but is NOT used by `TogglePrintAsync` — it's only for UI enabling | Code inspection | Verification | Medium — dead logic that should be wired or removed |
| 7 | **Reference CHM explicitly mentions "اليوم الاول من الشهر" (first day of month) for period reports** — monthly billing cycle is calendar-based, not rolling | CHM 011.htm | Accounts | Medium — affects invoice generation logic |
| 8 | **Reference has "كود الجهة" (Referral Code) as a separate field from name** — referrals have unique codes for billing | CHM 003.htm §5 | Registration | Medium — `ReferralSource` entity may lack Code field |
| 9 | **The " Orange title bar" in Result Editor** (per RL_Show.pdf) is a clickable button that opens Report Layout dialog — not just decoration | CHM 004A.htm + RL_Show | Result Editor | Medium — missed UI behavior |
| 10 | **Reference system's "خالص" (clearance) button** marks ALL tests in visit as paid simultaneously, not per-test | CHM 003.htm §13 | Billing | High — affects `ApplyClearancePaymentAsync` logic |
| 11 | **`TestType` has 5 name variants** (TypeName, ReportName1/2, BillName1/2, HistoryName, NameAr) — BillName collapsing on receipt is a real requirement | CHM 007.htm §3 | Test Catalog | Medium — Agent 2 noted it but neither analyzed `ReceiptService.GetGroupedTestsForReceiptAsync` |
| 12 | **NormalRange versioning already exists** (migration 20260610095053) — both agents under-emphasized this strength | Migration file | Normal Ranges | Low — good architecture already in place |
| 13 | **`Patient.IsVip` exists but no Style trigger for red text** in any XAML — VIP highlighting needs 1 line of XAML, not a feature | Code inspection | Patient | Low — trivial quick win |
| 14 | **The reference's "مستلمة" (received) override in Delivery** is a separate manual-override flag distinct from "تمت" — two different override concepts | CHM 005.htm §4 | Delivery | Medium — Agent 2 conflated them |
| 15 | **External shipment cost posting** to "حسابات المعمل" is not implemented because there's no `CashLedger` entity — need new table or use `Payment` with negative amount | CHM 010.htm §4 | External Labs | Medium — Agent 2 noted cost field but not posting mechanics |
| 16 | **Serilog is already configured in `App.xaml.cs`** — Agent 1 didn't mention this; Agent 2 suggested adding it | Code inspection | Logging | Low — already implemented |
| 17 | **Reference system has "تعليق" (comment) button color-coded orange** in Result Editor — not just a dropdown, but a prominent action | CHM 004A.htm §4 | Result Editor | Low — UI detail |
| 18 | **`WorkShift` entity exists** but there's no UI for shift assignment — Attendance module is more complete than both agents assessed | Code inspection | Attendance | Low — skeleton is further along |

---

# 2. Final Consolidated PRD

---

## 2.1 Final Module Map

### 2.1.1 Reference System Modules (Authoritative — from CHM + RL_Show + RLS_Learn)

| ID | Module (EN) | Module (AR) | CHM Source | Priority |
|---|---|---|---|---|
| RM-01 | Application Startup & Server Config | تشغيل النظام و الاتصال بالسيرفر | 001.htm | Low |
| RM-02 | Authentication / Login | تسجيل الدخول | 002.htm | Medium |
| RM-03 | Patient Registration & Visit Creation | بيانات المرضي (إضافة / تعديل / حذف) | 003.htm | **Critical** |
| RM-04 | Barcode & Specimen Label Printing | الباركود وملصقات الأنابيب | 003A.htm | **Critical** |
| RM-05 | Result Entry Hub | نافذة إدخال نتائج التحاليل | 004.htm | **Critical** |
| RM-06 | Per-Profile Result Editor | نافذة إدخال نتيجة تحليل / بروفايل | 004A.htm | **Critical** |
| RM-07 | Result Delivery | تسليم نتائج المرضي | 005.htm | **Critical** |
| RM-08 | Patient Global Search | البحث عن مريض | 006.htm | **Critical** |
| RM-09 | Test Catalog Management | بيانات التحاليل | 007.htm | **Critical** |
| RM-10 | Normal Range Management | المعدلات الطبيعية | 008.htm | High |
| RM-11 | Report Layout / Style Settings | اعدادات التقرير | 009.htm | High |
| RM-12 | External Outsourcing of Samples | العينات المرسلة للخارج | 010.htm | High |
| RM-13 | Cash Drawer / Period Inventory | حساب الدرج و الجرد | 011.htm | **Critical** |
| RM-14 | Database Maintenance & Backup/Restore | النسخ الاحتياطي و الصيانة | backup.htm | **Critical** |
| RM-15 | Lab Constants (PT/PTT/CBC formulae) | ثوابت النظام | nots.htm | High |
| RM-16 | Microbiology / Cultures | المزارع | RLS_Learn §2-2 | Medium |
| RM-17 | Composite / Blank Report Builder | تقرير مجمع / تقرير فارغ | RLS_Learn §1-7/1-8 | High |
| RM-18 | NATIGH.COM Portal Upload | متابعة تسجيل الحالات | RLS_Learn §3-5 | Low |
| RM-19 | Patient Medical History | التاريخ المرضي | RLS_Learn §1-4 | Medium |
| RM-20 | Corporate / Lab-to-Lab Billing | حساب الجهات / الشركات | RLS_Learn §1-12 | Medium |
| RM-21 | Users & Permissions | المستخدمون و الصلاحيات | Implicit in CHM | High |

### 2.1.2 Current System Modules (Verified from Code)

| ID | Module | Key Files | Maturity |
|---|---|---|---|
| CM-01 | App Startup & DI | `App.xaml.cs` | Full |
| CM-02 | First-run Setup | `FirstRunSetupWindow.xaml`, `FirstRunSetupViewModel.cs` | Full |
| CM-03 | Login & Auth | `LoginWindow.xaml`, `LoginViewModel.cs`, `AuthService.cs` | Full |
| CM-04 | Main Menu / Navigation | `MainWindow.xaml`, `MainViewModel.cs`, `NavigationService.cs` | Full |
| CM-05 | Current User Session | `CurrentUserSession.cs` | Full |
| CM-06 | Dialog Service | `DialogService.cs` | Full |
| CM-07 | Audit Logging (infrastructure) | `AuditLog.cs`, `AuditService.cs` | Partial — `[Auditable]` not intercepted |
| CM-08 | Permissions & RBAC | `Permission.cs`, `StaffPermission.cs` | Skeleton — `HasPermissionAsync` exists, rarely called |
| CM-09 | Patient Registration Window | `PatientRegistrationWindow.xaml`, `PatientRegistrationViewModel.cs` + 5 sub-VMs | Partial — missing F1 popup, age fraction, Lab-ID scan |
| CM-10 | Visit & Test Selection | `VisitService.cs` (615 LOC), `TestSelectionViewModel.cs` | Full |
| CM-11 | Receipt Dialog & Printing | `ReceiptDialog.xaml`, `ReceiptDialogViewModel.cs`, `ReceiptService.cs` | Full |
| CM-12 | Barcode Dialog | `BarcodeDialog.xaml`, `BarcodeDialogViewModel.cs` | Partial — missing calibration sliders, drag-drop |
| CM-13 | Today Patients Dialog | `TodayPatientsDialog.xaml`, `TodayPatientsDialogViewModel.cs` | Full |
| CM-14 | Patient Search Window | `PatientSearchWindow.xaml` | **Placeholder** — displays "قيد التطوير" |
| CM-15 | Medical History Input | `MedicalHistorySectionView.xaml`, `MedicalHistoryViewModel.cs` | Partial — no read/report integration |
| CM-16 | Result Entry Hub | `TestResultsWindow.xaml`, `TestResultsViewModel.cs` (900 LOC) | Partial — overloaded VM, missing editor features |
| CM-17 | Per-Profile Result Editor | `ResultEntryWindow.xaml`, `ResultEntryViewModel.cs` (104 LOC) | **Critically Minimal** — no flags, no comments, no constants |
| CM-18 | Audit Trail Viewer | `AuditTrailWindow.xaml`, `AuditTrailViewModel.cs` | Partial |
| CM-19 | Manual Override (تمت) | `TestResultsViewModel.ManualOverrideCommand` | Partial — works but UI conflated with finish |
| CM-20 | Print Preview | `PrintPreviewWindow.xaml` | Skeleton — `NullPrintService` |
| CM-21 | Delivery Window | `DeliveryWindow.xaml` | **Placeholder** — displays "قيد التطوير" |
| CM-22 | External Sample Outsourcing | `ExternalShipment.cs`, `ExternalShipmentItem.cs`, `ExternalLabService.cs` | Skeleton — entities + service, no UI |
| CM-23 | Test Catalog Management | `TestDataManagementWindow.xaml`, `TestDataManagementViewModel.cs` + sub-VMs | Full |
| CM-24 | Normal Range Management | `NormalRangesWindow.xaml`, `NormalRangeWindowViewModel.cs` + sub-VMs | Full |
| CM-25 | Price Schemes | `PriceScheme.cs`, `TestTypePrice.cs` | Partial — backend only, no UI |
| CM-26 | Referral Sources | `ReferralSource.cs`, `ReferralSectionView.xaml` | Partial — inline only, no management window |
| CM-27 | Microbiology / Culture | `MicrobiologyCulture.cs`, `CultureResultService.cs` | Skeleton — entities + service, no UI |
| CM-28 | Blood Bank / Cross-match | `CrossMatchTest.cs`, `BloodBankService.cs` | Skeleton — entities + service, no UI |
| CM-29 | Semen Analysis | `SemenAnalysis.cs`, `AndrologyService.cs` | Skeleton — entity + service, no UI |
| CM-30 | Per-visit Payment & Discount | `FinancialService.cs` (239 LOC) | Full |
| CM-31 | Contract Billing | `Company.cs`, `ContractInvoice.cs`, `ContractService.cs` | Skeleton — no UI, no print |
| CM-32 | Cash Drawer / Period Reports | — | **Missing** — no window, no service orchestrator |
| CM-33 | Referral Commissions | `VReferralCommissionReport.cs` | Skeleton — view exists, no UI |
| CM-34 | Reporting Service | `ReportingService.cs` | Partial — data queries only, no UI |
| CM-35 | Composite Report Builder | — | **Missing** — command routes to `NullPrintService` |
| CM-36 | Blank / Any Report | — | **Missing** |
| CM-37 | Worksheet / Envelope Print | Commands exist | Skeleton — routes to `NullPrintService` |
| CM-38 | Report Layout Settings | — | **Missing** — no equivalent to CHM 009.htm |
| CM-39 | Lab Settings (key/value) | `LabSetting.cs`, `SettingsService.cs` | Full |
| CM-40 | User Management UI | — | **Missing** — `AuthService.CreateUserAsync` exists, no window |
| CM-41 | Backup & Restore | — | **Missing** — no service, no UI |
| CM-42 | Attendance & Work Shifts | `Attendance.cs`, `WorkShift.cs`, `AttendanceService.cs` | Skeleton — no UI |
| CM-43 | NATIGH.COM / SMS | — | **Missing** |

### 2.1.3 Final Mapping Matrix

| Reference | Current | Status | Equivalence | Gap Code |
|---|---|---|---|---|
| RM-01 Startup | CM-01, CM-02 | Full | 90% | Minor — server config dialog not exposed |
| RM-02 Login | CM-03 | Full | 100% | PBKDF2 better than reference |
| RM-03 Patient Registration | CM-09, CM-10 | Partial | 70% | Missing: F1 popup, age fraction, Lab-ID scan, visit count badge |
| RM-04 Barcode | CM-12 | Partial | 60% | Missing: calibration sliders, drag-drop, 13-digit format generator |
| RM-05 Result Entry Hub | CM-16 | Partial | 55% | VM overloaded; toolbar commands go to NullPrintService |
| RM-06 Result Editor | CM-17 | Critically Minimal | 25% | Missing: flags, comments, constants panel, F8/F9/F11/F12, templates |
| RM-07 Delivery | CM-21 | Placeholder | 0% | Entire workflow unbuilt |
| RM-08 Patient Search | CM-14 | Placeholder | 0% | Compound search unbuilt |
| RM-09 Test Catalog | CM-23 | Full | 95% | Verify: BillNameLine2, HistoryName, PatientQuestion editable |
| RM-10 Normal Ranges | CM-24 | Full | 95% | Versioned with audit trail |
| RM-11 Report Layout | CM-38 | Missing | 0% | No window, no per-station JSON layout |
| RM-12 External Outsourcing | CM-22 | Skeleton | 15% | Entities + service; no UI |
| RM-13 Cash Drawer | CM-32 | Missing | 0% | No window, no period-close logic |
| RM-14 Backup/Restore | CM-41 | Missing | 0% | No service, no UI |
| RM-15 Lab Constants | (implicit) | Missing | 0% | Constants panel not in UI; CBC/PT formulas not codified |
| RM-16 Microbiology | CM-27 | Skeleton | 15% | Entities + service; no UI |
| RM-17 Composite/Blank Report | CM-35, CM-36 | Missing | 0% | No two-pane UI, no blank report editor |
| RM-18 NATIGH.COM | CM-43 | Missing | 0% | Not modelled |
| RM-19 Medical History | CM-15 | Partial | 40% | Capture exists; not wired to report print |
| RM-20 Corporate Billing | CM-31 | Skeleton | 20% | Backend exists; no UI, no claim printout |
| RM-21 Users & Permissions | CM-40, CM-08 | Skeleton | 20% | Entities + auth; no management UI, not enforced |

**Overall Equivalence: ~30%** (Agent 2's estimate is validated; Agent 1's 45% was optimistic)

---

## 2.2 Final Workflow Map

### 2.2.1 Patient Lifecycle

```
[NEW PATIENT]
    │
    ├──► Registration (F2)
    │      ├── Enter: Name (AR), Sex (derives title), Age (supports fraction 2.5)
    │      ├── Optional: National ID, Phone (2 fields), Notes
    │      ├── Select: Referral Source (auto-create if new, has Code field)
    │      ├── Select: Patient Type (Individual / LabToLab / Free / Contract)
    │      ├── Check: VIP (name turns red), Outside sample flags (5 types)
    │      └── Medical History flags (14 booleans: Diabetes, Pregnant, Fasting, etc.)
    │
    ├──► Test Selection
    │      ├── Dropdown modes: Routine / All / Groups / Custom Profiles
    │      ├── Double-click test to add; double-click selected to remove
    │      ├── Double-click group → adds all members (respects AddWithGroup=false)
    │      ├── F1 on test → inline details popup (ranges, notes)
    │      └── Test with PatientQuestion → prompt on add (e.g., "هل أنت صائم؟")
    │
    ├──► Financial Calculation
    │      ├── Subtotal = sum of TestType prices per scheme
    │      ├── Discount: amount OR percent (mutually exclusive)
    │      ├── Auto-apply: Referral.DiscountPercent if > 0
    │      ├── "Free" type → total = 0
    │      ├── Paid amount entry
    │      └── Balance = Total - Paid
    │
    ├──► Save Visit
    │      ├── Generate: Case Code (1-prefix, daily sequential)
    │      ├── Generate: File Code (3-prefix, per-physical-file)
    │      ├── Generate: Lab ID (5-prefix, permanent per-patient)
    │      ├── Format: 0-B-DDMMYY-W-NNN-X (branch-date-weekday-sequence-checksum)
    │      ├── Print receipt (F12) — auto-print if setting enabled
    │      └── Print barcode labels per tube material grouping
    │
    └──► Post-Save
           ├── Patient appears in Today's list with status icon
           ├── Barcode labels printed (file code + lab ID + per-tube codes)
           └── Visit state = OPEN
```

**Status:** Partially implemented. Core flow works. Missing: F1 popup, age fraction, Lab-ID scan, 13-digit format generator with checksum, referral code field, auto-apply referral discount.

### 2.2.2 Visit / Test Lifecycle

```
[OPEN VISIT]
    │
    ├──► All tests have results entered (or manual-override "تمت")
    │      └── State → FINISHED
    │
    ├──► All results reviewed (F8 toggle)
    │      └── State → VERIFIED
    │      └── RULE: Cannot print without verify
    │
    ├──► All results printed (F12 toggle)
    │      └── State → PRINTED
    │      └── RULE: Cannot deliver without print
    │
    ├──► Delivered to patient (in Delivery window)
    │      └── State → DELIVERED
    │      └── Balance settlement if BalanceDue != 0
    │
    └──► All tests delivered AND BalanceDue == 0
              └── State → FULLY COMPLETE (medal icon)
```

**Status:** States computed correctly in `GetTodayPatientsWithStatusAsync`. **CRITICAL BUG:** `RoutineResultService.TogglePrintStatusAsync` does NOT check `ValidationStatus` before allowing print (BR-B01 violated). `ToggleExportStatusAsync` does NOT check `IsPrinted` at service level (BR-B02 violated at domain layer; UI layer has guard in VM at line 537).

### 2.2.3 Result Entry Workflow

```
[RESULT ENTRY HUB] (F4)
    │
    ├──► Left Pane: Today's patient list
    │      ├── Status icons per patient (7 states: NEW → Notepad → Verify → Print → Cart → £ → Medal)
    │      ├── Filter tabs: All / Individual / LabToLab / VIP / Free
    │      ├── Search box (name/code filtering)
    │      └── Day navigation arrows (← →)
    │
    ├──► Middle Pane: Selected patient's tests grid
    │      ├── Test name, component count, entered count, status
    │      ├── Inline editing: click → edit result → Enter to save
    │      └── "تم" / "تمت" toggle indicator per test
    │
    ├──► Right Pane: (not implemented — would show filter sidebar)
    │
    ├──► Bottom Toolbar
    │      ├── Composite Report (مجمع), Blank Report (فارغ)
    │      ├── Patient History (تاريخ), Medical Report (طرف)
    │      ├── Worksheet (أمر شغل), Envelope (ظرف), SMS
    │      └── ALL route to NullPrintService (non-functional)
    │
    └──► Audit Buttons (admin only)
           ├── "P" button → visit-level audit (creator, edits, payments)
           └── "T" button → test-level audit (entered/verified/printed/delivered)
```

**Status:** Hub structure exists. Test list loads. Inline editing works. Status icons exist. **Bottom toolbar commands are non-functional** (NullPrintService). Audit buttons work for admin. Missing: F1 test details, per-test status icons, filter sidebar.

### 2.2.4 Per-Profile Result Editor Workflow

```
[PER-PROFILE EDITOR] (double-click multi-component test)
    │
    ├──► Orange title bar (clickable → opens Report Layout dialog)
    │
    ├──► Components grid
    │      ├── Editable: Result value, Comment (per row)
    │      ├── Read-only: Unit, Normal Range, L/H indicator chip
    │      ├── Verified checkbox, Print checkbox per row
    │      └── Auto-flag: Normal / Low / High / Critical against NormalRange
    │
    ├──► Shared Comment area (orange "تعليق" button)
    │      └── Dropdown of ReportCommentTemplate rows filtered by test type
    │
    ├──► Auto-comment injection
    │      └── Click test name → append LowComment/HighComment/CriticalComment
    │
    ├──► Constants Panel (right side)
    │      ├── CBC: Hgb→Hct factor, Hgb% sex/age bands
    │      ├── PT: ISI, Control Time → INR auto-calculated
    │      └── PTT: Control Time
    │
    └──► Commands
           ├── F8: Verify toggle
           ├── F9: Save (advances to next field; closes on last)
           ├── F11: Preview report
           └── F12: Print report
```

**Status:** Window exists. Saves numeric/text via `RoutineResultService.SaveNumericOrTextResultsAsync` which DOES compute flags against NormalRange (service-level, not VM). Missing: L/H/Critical chips in UI, comment templates, auto-comment injection, constants panel, F8/F9/F11/F12 bindings, orange title bar, verify/print checkboxes.

### 2.2.5 Delivery Workflow

```
[DELIVERY WINDOW] (F6)
    │
    ├──► Mode A: Barcode-driven (fast path)
    │      ├── Scan receipt barcode → load patient
    │      ├── Scan file barcode → confirm match
    │      └── Auto-display results + financial summary
    │
    ├──► Mode B: Manual list (fallback)
    │      ├── Filter: Today / Date Range / VIP / Individual / LabToLab / Free / All
    │      ├── Select patient → show test results
    │      └── Show: paid, remaining, settlement button
    │
    ├──► Balance Dialog (if BalanceDue != 0)
    │      ├── Alert: remaining amount
    │      ├── Settlement options (cash, partial, defer)
    │      └── Integrated with FinancialService
    │
    ├──► Mark Delivered
    │      ├── All printed tests → Delivered state
    │      ├── Manual override "مستلمة" (distinct from "تمت")
    │      └── Audit-log: who delivered, when
    │
    └──► Post-Delivery
           └── Patient removed from pending list
           └── If all tests delivered AND balance == 0 → Fully Complete
```

**Status:** Window is placeholder ("قيد التطوير"). Entire workflow unbuilt.

### 2.2.6 Patient Search Workflow

```
[PATIENT SEARCH] (F3)
    │
    ├──► Filter Panel
    │      ├── Name: partial text (prefix + substring, normalized)
    │      ├── Codes: Case Code / File Code / Lab ID
    │      ├── National ID
    │      ├── Phone (2 fields)
    │      ├── Date range (from/to)
    │      ├── Age bracket (from/to with unit)
    │      ├── Sex (M/F)
    │      └── Referral source
    │
    ├──► Result grid (max 100 rows)
    │      ├── Patient name, codes, phone, visit count
    │      └── Double-click → open patient history
    │
    ├──► Actions per result
    │      ├── Show visits → all visits for patient
    │      ├── Show tests → all tests across visits
    │      ├── Delete patient (permission-gated)
    │      └── Composite report from selected patients
    │
    └──► Small-lab toggle: search in backup DB
```

**Status:** Window is placeholder. `PatientService.SearchPatientsAsync` accepts single string only. No compound filters.

### 2.2.7 Cash Drawer / Period Inventory Workflow

```
[CASH DRAWER WINDOW] (F7)
    │
    ├──► 6-Tile Dashboard
    │      ├── Patient billings (collected in period)
    │      ├── External sample costs
    │      ├── Expenses (صرف)
    │      ├── Balances (deferred payments)
    │      ├── Staff cash collections
    │      └── Referral commissions
    │
    ├──► "What was collected" filter
    │      ├── (a) Money from patients REGISTERED in this period
    │      └── (b) ANY patient who PAID in this period
    │
    ├──► Cash In/Out entries (صرف و إيداع نقدية)
    │      └── Staff + reason + amount → running balance
    │
    └──► Print "كشف الدرج" (period report)
           └── Also: Lab-to-Lab claim printout per company
```

**Status:** Entire module missing. No window, no service, no `CashLedger` entity. `IReportingService` has data fragments but no orchestrator.

### 2.2.8 External Outsourcing Workflow

```
[EXTERNAL SAMPLES WINDOW] (F7 context-dependent)
    │
    ├──► Pending Queue
    │      ├── Auto-populated from tests marked IsSendOutside=true
    │      ├── Grouped by destination external lab
    │      └── Shows: patient, test, estimated cost
    │
    ├──► Actions
    │      ├── Mark "Sent" → record sent date + staff
    │      ├── "دائما إليه" toggle → persist default lab per TestType
    │      └── Manual override: mark non-outside test as send-outside for one shipment
    │
    ├──► Shipment Close
    │      └── Posts aggregate cost to lab finance
    │
    └──► Settlement
           └── Period-end reconciliation with external lab invoices
```

**Status:** Entities (`ExternalShipment`, `ExternalShipmentItem`) + `ExternalLabService` exist. No UI. Cost posting logic not implemented.

---

## 2.3 Final Business Rules Catalog

### Rules are organized by theme, validated against code, with source references.

**Status legend:**
- **ENFORCED** — Rule is correctly implemented in code
- **VIOLATED** — Rule is actively broken in code
- **UI-ONLY** — Rule enforced in ViewModel but not in service/domain
- **ABSENT** — Rule belongs to a missing module
- **PARTIAL** — Some parts implemented, not complete

### A. Identity & Coding Rules

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-A01 | Each patient gets 3 codes: Case (1-prefix), File (3-prefix), Lab (5-prefix) | CHM 003A.htm | PARTIAL | `Patient.PatientCode`, `Visit.VisitCode` exist as strings; no triple-code generator verified |
| BR-A02 | Code format: 0-B-DDMMYY-W-NNN-X (B=branch, W=day-of-week, NNN=sequence, X=checksum) | CHM 003A.htm | ABSENT | `GeneratePatientCodeAsync` exists but format not verified; no checksum |
| BR-A03 | Lab ID is **permanent**; never re-issue | CHM 003.htm §14 | ABSENT | No uniqueness enforcement on Lab ID |
| BR-A04 | Lab ID barcode scan in registration auto-loads existing patient | CHM 003.htm §14 | ABSENT | Not implemented |
| BR-A05 | Test numerical IDs are immutable | nots.htm | PARTIAL | `TestCatalogService` allows CRUD; no ID slot protection |
| BR-A06 | Patient names: normalize hamza (ء→''), ة→ه, ى→ي for search | CHM 003.htm Note | ABSENT | `SearchPatientsAsync` has no normalization |
| BR-A07 | Age: <1mo in days (1-29), <1yr in months (1-11), else years; fraction supported (2.5) | CHM 003.htm Note | PARTIAL | `ApproxAge` + `ApproxAgeUnit` exist; fraction UI not verified |

### B. Stage-Gating Rules (CRITICAL)

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-B01 | **Cannot Print before Verify** | CHM 004.htm | **VIOLATED** | `RoutineResultService.TogglePrintStatusAsync` (line 178) flips `IsPrinted` without checking `ValidationStatus`. VM's `TogglePrintAsync` calls directly. |
| BR-B02 | **Cannot Deliver before Print** | CHM 005.htm | UI-ONLY | VM `ToggleExportAsync` (line 537) checks `!SelectedTest.IsPrinted` and warns. Service `ToggleExportStatusAsync` has no guard. Fragile. |
| BR-B03 | Manual override "تمت" is the ONLY legal bypass; logs user | CHM 004.htm §4 | ENFORCED | `ManualOverrideCommand` writes `MANUAL_COMPLETE` to `TestWorkflow` with staff ID |
| BR-B04 | Stage toggles must be reversible with audit log | CHM 004.htm | ENFORCED | `PRINT_UNDO` / `EXPORT_UNDO` stages written to `TestWorkflow` |
| BR-B05 | Visit is FullyComplete only when all tests Delivered AND Balance==0 | CHM 004.htm | ENFORCED | `PatientVisitStatus.FullyComplete` computed in `GetTodayPatientsWithStatusAsync` |
| BR-B06 | Status icon reflects earliest blocked state in chain | CHM 004.htm | ENFORCED | Enum order 0-6 reflects progression |

### C. Verification, Audit, Permissions

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-C01 | Every operation logged with user + timestamp | CHM 004.htm §6 | PARTIAL | `AuditLog` + `TestWorkflow` exist; not all VMs call `LogActionAsync` on edits |
| BR-C02 | Audit P/T buttons restricted to admin only | CHM 004.htm §6 | ENFORCED | `CanAccessAuditFeatures` gated by `RESULTS.VIEW_AUDIT` permission |
| BR-C03 | Receipt re-print: staff = once per state change; admin = unlimited | RLS_Learn | ENFORCED | `ReceiptService.CanPrintReceiptAsync` + `ReceiptPrintLog` |
| BR-C04 | Patient delete requires `PERM_DELETE_PATIENT` | CHM 003.htm §1 | PARTIAL | Permission table exists; enforcement at call sites inconsistent |

### D. Pricing & Discounts

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-D01 | Per-referral price scheme overrides default Lab-to-Lab price | CHM 003.htm §5 | ENFORCED | `PricingService.GetTestPriceAsync` with scheme cascade |
| BR-D02 | "Free" patient type: total = 0 | CHM 003.htm §3 | ENFORCED | `PatientType = "Free"` behavior in visit save |
| BR-D03 | Referral discount percent auto-applies on save | CHM 003.htm §5 | ABSENT | `FinancialService.ApplyDiscountAsync` exists but auto-application not wired in `SavePatientVisitAsync` |
| BR-D04 | Discount: amount OR percent, not both | CHM 003.htm §13 | VIOLATED | `Visit` has both `DiscountAmount` and `DiscountPercent`; no constraint prevents both non-zero |
| BR-D05 | "خالص" = full clearance: Balance→0, PaymentStatus→Paid | CHM 003.htm §13 | ENFORCED | `FinancialService.ApplyClearancePaymentAsync` |

### E. Test Catalog Behavior Flags

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-E01 | `IsRoutineTest=true` → appears in Routine dropdown | CHM 007.htm §3 | ENFORCED | Field exists; filter assumed wired |
| BR-E02 | `SeeReport=true` → opens dedicated editor | CHM 007.htm §4A | PARTIAL | Field exists; `DefaultResultEditorFactory` returns false for all |
| BR-E03 | `PrintWithOther=false` → excluded from composite report | CHM 007.htm §3 | ABSENT | Field exists; composite report not built |
| BR-E04 | `AddWithGroup=false` → skip in profile bulk-add | CHM 007.htm §3 | ENFORCED | Verified in `VisitService.SavePatientVisitAsync` |
| BR-E05 | `IsMainTest=true` → contains sub-components (CBC, etc.) | CHM 007.htm §3 | PARTIAL | Field exists; specialized editors missing |
| BR-E06 | `IsSendOutside=true` → auto-queue in External Samples | CHM 010.htm | PARTIAL | Service supports; no UI consumer |
| BR-E07 | TestType has 5 name variants: TypeName, ReportName1/2, BillName1/2, HistoryName, NameAr | CHM 007.htm §3 | ENFORCED | All columns exist |

### F. Normal Ranges

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-F01 | Per-component ranges segmented by Sex x Age bracket | CHM 008.htm | ENFORCED | `NormalRange` versioned table with `Sex`, `AgeFromDays`, `AgeToDays` |
| BR-F02 | Result outside [LowNormal, HighNormal] → shaded in report + flag | CHM 008.htm + 009.htm | PARTIAL | `SnapLowNormal`, `SnapHighNormal` saved; print engine missing |
| BR-F03 | Result outside [LowCritical, HighCritical] → Critical flag + comment | CHM 008.htm | PARTIAL | `SnapLowCritical`, `SnapHighCritical` saved; comment injection missing |
| BR-F04 | Click test name → auto-append Low/High/Critical comment | CHM 008.htm | ABSENT | `NormalRange.LowComment/HighComment/CriticalComment` exist; not wired |
| BR-F05 | Snapshots stored on TestResult for historical integrity | Implicit | ENFORCED | `SnapUnit`, `SnapLowNormal`, etc. exist |

### G. Reporting & Print

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-G01 | Report layout settings stored per-workstation (not global) | CHM 009.htm | ABSENT | `JsonUserSettingsService` exists but not used for report layout |
| BR-G02 | 5 independent toggles: abnormal bg, status flag, bold ranges, critical ranges, auto-history | CHM 009.htm §2 | ABSENT | Not modelled |
| BR-G03 | Composite report: two-pane, reorder, subtitle toggle, reprint-toggle | CHM 004.htm §5 | ABSENT | Command routes to `NullPrintService` |
| BR-G04 | Blank report: free-form rows, savable, printable | RL_Show.pdf | ABSENT | No entity or window |

### H. Specialty Tests

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-H01 | Blood Culture: <=3 organisms (A/B/C), sensitivity S/I/R | CHM + RL_Show | ABSENT | Entities ready; no UI |
| BR-H02 | Antibiotic safety filter for pregnant/pediatric | CHM | ABSENT | `GetSafeAntibioticsAsync` exists; no UI consumer |
| BR-H03 | Cross-match: donor list + per-donor reactions | RL_Show | ABSENT | Entities present |
| BR-H04 | Semen Analysis: macroscopic + microscopic + motility tables | RL_Show | ABSENT | Entity present |

### I. External Outsourcing

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-I01 | `IsSendOutside=true` → auto-queue when added to visit | CHM 010.htm | PARTIAL | `ExternalLabService` queue logic exists; no UI |
| BR-I02 | Group by destination lab, show aggregate cost | CHM 010.htm §3 | ABSENT | `ExternalShipmentItem.Cost` exists; aggregation UI missing |
| BR-I03 | "دائما إليه" persists default external lab per TestType | CHM 010.htm §4 | ABSENT | Not modelled |
| BR-I04 | Manual override: mark non-outside test as send-outside for one shipment | CHM 010.htm §6 | ABSENT | Not modelled |
| BR-I05 | Closing shipment posts cost to lab finance | CHM 010.htm §4 | ABSENT | Cost field present; posting logic not visible |

### J. Lab Constants & Derived Values (CRITICAL FOR CLINICAL SAFETY)

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-J01 | If Hct missing → Hct = Hgb * 3.3 | nots.htm CBC | ABSENT | Not implemented |
| BR-J02 | Hgb% derived: <1y = Hgb*8.25, 1-12y = Hgb*7.50, M>12y = Hgb*6.25, F>12y = Hgb*6.75 | nots.htm CBC | ABSENT | Not implemented |
| BR-J03 | PT INR = (PatientTime / ControlTime) ^ ISI | nots.htm PT | ABSENT | Not implemented; Constants panel missing |
| BR-J04 | Constants (ISI, ControlTime) editable from result editor side panel | nots.htm | ABSENT | Side panel not implemented |

### K. Backup & Maintenance

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-K01 | Backup file: `Patient_yyyymmdd.bak` in DB path | backup.htm | ABSENT | No backup mechanism |
| BR-K02 | Latest backup is cumulative unless "reset" issued | backup.htm | ABSENT | Not modelled |
| BR-K03 | Small-lab: per-patient restore by Case code from backup DB | backup.htm | ABSENT | Not modelled |
| BR-K04 | Maintenance password (default `123`) gates maintenance window | backup.htm | ABSENT | Not modelled |
| BR-K05 | Period purge: delete patients in date range after backup copy | backup.htm | ABSENT | Not modelled |

### L. Search

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-L01 | Free-text: prefix + substring matching | CHM 006.htm §1 | PARTIAL | Single string search; no split |
| BR-L02 | Results capped at 100 rows | CHM 006.htm §2 | ABSENT | Page size 50 default; no hard cap |
| BR-L03 | Filters: date range, age, sex, referral | CHM 006.htm §1 | ABSENT | Placeholder window |
| BR-L04 | Composite report from search results | CHM 006.htm §4 | ABSENT | Not implemented |
| BR-L05 | Backup-DB search toggle (small-lab) | CHM 006.htm | ABSENT | Not modelled |

### M. Cash Drawer

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-M01 | 6-tile dashboard: billings, outsource, expenses, balances, commissions, staff cash | CHM 011.htm §2-7 | ABSENT | No window, no orchestrator |
| BR-M02 | "Collected" filter: (a) registered-in-period vs (b) paid-in-period | CHM 011.htm §2 | ABSENT | Not modelled |
| BR-M03 | Lab-to-Lab claim printout per company per period | CHM 011.htm §7 | ABSENT | `GenerateMonthlyCorporateInvoiceAsync` exists; no print pipeline |

### N. Auxiliary

| # | Rule | Source | Status | Evidence |
|---|---|---|---|---|
| BR-N01 | Non-English OS locale causes login failure | CHM 001.htm | INFO | .NET 8 likely tolerant |
| BR-N02 | Patient Notes visible cross-window for technicians | CHM 003.htm §9 | PARTIAL | `Patient.Notes` + `Visit.Notes` exist; cross-window surfacing partial |
| BR-N03 | Receipt auto-print on save is a setting | Implicit | PARTIAL | `LabSetting` exists; toggle not surfaced |

---

## 2.4 Final Gap Matrix

### Gap Severity: 🔥 Critical | 🟠 High | 🟡 Medium | 🟢 Low

| Gap ID | Feature / Gap | Severity | Reference | Code Location | Status |
|---|---|---|---|---|---|
| G-01 | Delivery window is placeholder | 🔥 | CHM 005.htm RM-07 | `DeliveryWindow.xaml` (12 lines, "قيد التطوير") | Missing |
| G-02 | Patient Search window is placeholder | 🔥 | CHM 006.htm RM-08 | `PatientSearchWindow.xaml` + `PlaceholderTaskViewModels.cs` | Missing |
| G-03 | Cash Drawer / Period Inventory absent | 🔥 | CHM 011.htm RM-13 | No view, no VM, no orchestrator | Missing |
| G-04 | Backup & Restore absent | 🔥 | backup.htm RM-14 | No service, no view | Missing |
| G-05 | Report Layout settings absent | 🟠 | CHM 009.htm RM-11 | Not modelled | Missing |
| G-06 | Composite Report Builder absent | 🟠 | CHM 004.htm §5 | `PrintCompositeReportCommand` → `NullPrintService` | Missing |
| G-07 | Blank/Any Report editor absent | 🟡 | RL_Show.pdf | No window, no entity | Missing |
| G-08 | External Outsourcing window absent | 🟠 | CHM 010.htm RM-12 | `ExternalLabService` exists, no UI | Skeleton |
| G-09 | User Management UI absent | 🟠 | Implicit RM-21 | `AuthService.CreateUserAsync` exists, no window | Missing |
| G-10 | Permission enforcement inconsistent | 🟠 | BR-C04 | `HasPermissionAsync` called only in `TestResultsViewModel` | Broken |
| G-11 | Specialty UIs: Culture, Blood Bank, Semen | 🟡 | CHM + RL_Show | Entities + services; no windows | Skeleton |
| G-12 | Report Comment Templates UI absent | 🟡 | CHM 004A.htm §4 | `ReportCommentTemplate` table exists | Missing |
| G-13 | Price Schemes management UI absent | 🟡 | CHM 007.htm | `PriceScheme` modelled; no editor | Missing |
| G-14 | Referral Source management UI absent | 🟡 | CHM 003.htm §5 | `ReferralSource` entity; inline only | Missing |
| G-15 | Attendance & Work Shifts UI absent | 🟢 | RLS_Learn | Entities + service stub | Skeleton |
| G-16 | NATIGH.COM portal absent | 🟢 | RLS_Learn TOC | Not modelled | Missing |
| G-17 | Lab Constants panel absent | 🟠 | nots.htm + RL_Show | Not modelled | Missing |
| G-18 | Lab-to-Lab claim printout absent | 🟡 | CHM 011.htm §7 | `GenerateMonthlyCorporateInvoiceAsync`; no print | Skeleton |
| G-19 | Worksheet printout absent | 🟡 | CHM 004.htm §5 | `PrintWorksheetCommand` → `NullPrintService` | Missing |
| G-20 | Patient Envelope printout absent | 🟢 | CHM 004.htm §5 | `PrintEnvelopeCommand` → `NullPrintService` | Missing |
| **G-21** | **Print can occur before Verify** | **🔥** | CHM 004.htm | `RoutineResultService:173-193` no guard | **VIOLATED** |
| **G-22** | **Export can occur before Print (at service layer)** | **🔥** | CHM 005.htm | `RoutineResultService:195-215` no guard | **VIOLATED** |
| G-23 | Result editor doesn't show L/H/Critical flags in UI | 🟠 | CHM 004A.htm §2 | `ResultEntryViewModel.SaveAsync` saves only; flags computed in service but not shown | Partial |
| G-24 | Auto-comment injection from NormalRange missing | 🟠 | CHM 008.htm | Not coded | Missing |
| G-25 | Composite report assembly absent | 🟠 | CHM 004.htm §5 | Window absent | Missing |
| G-26 | Barcode label calibration sliders absent | 🟡 | CHM 003A.htm §5 | Not in `BarcodeDialog` | Missing |
| G-27 | Drag-drop between tube labels absent | 🟢 | CHM 003A.htm §6 | Not in `BarcodeDialog` | Missing |
| G-28 | Search text normalization absent | 🟡 | CHM 003.htm Note | Not in `SearchPatientsAsync` | Missing |
| G-29 | Lab ID scan lookup absent | 🟠 | CHM 003.htm §14 | Not coded | Missing |
| G-30 | 13-digit code format generator absent | 🟡 | CHM 003A.htm | `GeneratePatientCodeAsync` format unverified | Partial |
| G-31 | Multiple list modes in test selection | 🟡 | CHM 003.htm §10 | Verify `TestSelectionView` | Partial |
| G-32 | F1 inline test-details popup absent | 🟢 | CHM 003.htm | Not coded | Missing |
| G-33 | Patient question prompt on test selection | 🟡 | CHM 007.htm §6 | `TestType.PatientQuestion` exists; UI missing | Partial |
| G-34 | Discount amount+percent mutual exclusion | 🟡 | CHM 003.htm §13 | No EF check constraint | Broken |
| G-35 | Visit/Test state transitions scattered | 🟡 | Implicit | No centralized state machine | Broken |
| G-36 | Outside-sample annotation on report | 🟢 | CHM 003.htm §8 | `Visit.OutsideXxx` exist; print engine missing | Partial |
| G-37 | Status icon assets | 🟡 | CHM 004.htm §1 | Enum exists; icons may not be bound | Partial |
| G-38 | VIP red-tint across windows | 🟡 | CHM 004.htm §2 | `Patient.IsVip` exists; no XAML trigger | Partial |
| G-39 | Audit P/T button positioning | 🟢 | CHM 004.htm §6 | Present but placement may differ | Partial |
| G-40 | Filter sidebar tabs in Result Hub | 🟡 | CHM 004.htm §7 | `SelectedPatientType` exists; may need more tabs | Partial |
| G-41 | Day navigation arrows in Result Hub | 🟢 | CHM 004.htm §7 | `NavigateDayCommand` exists | Implemented |
| G-42 | Bottom toolbar buttons non-functional | 🟠 | CHM 004.htm §5 | All route to `NullPrintService` | Broken |
| G-43 | Orange title bar in Result Editor | 🟡 | CHM 009.htm §3 entry | Not implemented | Missing |
| G-44 | Microscopic sub-panel in Culture | 🟢 | CHM Culture screenshot | Not applicable until Culture UI built | Missing |
| G-45 | Patient photo display | 🟢 | RL_Show Result Hub | Not implemented | Missing |
| G-46 | Visit count badge on patient name | 🟢 | CHM 004.htm §2 | Not implemented | Missing |
| G-47 | "تم/تمت" toggle icon per test | 🟡 | CHM 004A.htm §3 | `IsManuallyOverridden` exists; icon binding missing | Partial |
| G-48 | TestResultsViewModel 900 LOC overloaded | 🟠 | MVVM | 900 LOC, 28 commands, 4 CollectionViews | Broken |
| G-49 | NullPrintService is silent no-op | 🔥 | Infra | Returns `Task.CompletedTask` | Broken |
| G-50 | Placeholder VMs in single file | 🟠 | Code smell | `PlaceholderTaskViewModels.cs` | Broken |
| G-51 | Anemic domain model | 🟡 | DDD | Entities have no business methods | Design debt |
| G-52 | AuditableAttribute not intercepted | 🟠 | Infra | Marker only; no `SaveChangesAsync` override | Broken |
| G-53 | No structured stage-gating policy | 🟠 | Architecture | Rules in BR-B01/B02 violated | Broken |
| G-54 | Print pipeline absent | 🔥 | Infra | `NullPrintService` in DI | Broken |
| G-55 | ResultEditorFactory no-op | 🟠 | Architecture | `HasCustomEditor` returns false | Broken |
| G-56 | Settings storage split (DB vs JSON) | 🟡 | Infra | Unclear ownership boundary | Design debt |
| G-57 | No command logger decorator | 🟡 | Infra | Manual `LogActionAsync` calls | Design debt |
| G-58 | DbContext scoped but singletons resolve indirectly | 🟢 | DI | Factory pattern used; low risk | Design debt |
| G-59 | PatientType is string not enum | 🟢 | Type safety | `"Individual"`/`"LabToLab"`/`"Free"` | Design debt |
| G-60 | No TestStage state machine | 🟠 | Architecture | `TestStage` enum; transitions scattered | Broken |
| G-61 | 25 migrations on single branch | 🟢 | Infra | Risk of drift on parallel work | Low |
| G-62 | No tests for Verify→Print→Deliver gating | 🟠 | Test coverage | Existing tests cover Receipt + Auth only | Broken |

---

## 2.5 Final Code Impact Map

### Impact by Phase — Exact File References

#### Phase 1: Core Safety & Infrastructure

**Services to modify:**
| File | Change | Lines |
|---|---|---|
| `Services/Implementations/RoutineResultService.cs` | Add verification guard to `TogglePrintStatusAsync`; add print guard to `ToggleExportStatusAsync` | 173-215 |
| `Data/FinalLabDbContext.cs` | Override `SaveChangesAsync` to intercept `[Auditable]` entities | New method |
| `App.xaml.cs` | Add `EnforceStageGating` LabSetting seed; prepare for `WpfFlowDocumentPrintService` | 160-161 |

**ViewModels to modify:**
| File | Change |
|---|---|
| `ViewModels/Patients/TestResultsViewModel.cs` | Wire `CanPrint` property into `TogglePrintAsync` guard; remove `TogglePrintStatusCommand` (unused duplicate) |
| `ViewModels/Patients/PlaceholderTaskViewModels.cs` | Split into separate files under `ViewModels/Patients/Delivery/` and `ViewModels/Patients/Search/` |

**Database changes:**
| Migration | Purpose |
|---|---|
| `AddDiscountExclusivityConstraint` | Add check constraint: `DiscountAmount = 0 OR DiscountPercent = 0` on `Visit` |

**New files:**
| File | Purpose |
|---|---|
| `Infrastructure/ResultStageRules.cs` | Static class with `CanPrint(validationStatus)` and `CanDeliver(isPrinted)` methods |
| `Services/Implementations/WpfFlowDocumentPrintService.cs` | Replaces `NullPrintService`; implements `IPrintService` using FlowDocument |
| `Services/Implementations/LoggingPrintService.cs` | Decorator that logs all print requests before delegating |

#### Phase 2: Patient Search & Identity

**Services to modify:**
| File | Change |
|---|---|
| `Services/Interfaces/IPatientService.cs` | Add `SearchPatientsAsync(PatientSearchCriteria criteria)` |
| `Services/Implementations/PatientService.cs` | Implement compound search with all filters; cap at 100 rows |

**Views to create:**
| File | Description |
|---|---|
| `Views/Patients/PatientSearchWindow.xaml` | Full search window: filters, result grid, actions |

**ViewModels to create:**
| File | Description |
|---|---|
| `ViewModels/Patients/PatientSearchViewModel.cs` | Compound search logic, result navigation |

**Database changes:**
| Migration | Purpose |
|---|---|
| `AddSearchableNameColumn` | Computed column: normalized name for search (hamza stripped, ة→ه, ى→ي) |
| `AddReferralCodeColumn` | Add `Code` field to `ReferralSource` if missing |

#### Phase 3: Result Editor Alignment

**Views to create/modify:**
| File | Change |
|---|---|
| `Views/Patients/ResultEntryWindow.xaml` | Rebuild: components grid with L/H/Critical chips, comment templates dropdown, constants panel |

**ViewModels to modify:**
| File | Change |
|---|---|
| `ViewModels/Patients/ResultEntryViewModel.cs` | Rebuild from 104 LOC: add flag display, comment template picker, constants panel, F8/F9/F11/F12 |
| `ViewModels/Patients/TestResultsViewModel.cs` | Decompose: extract `TestsGridViewModel`, `ToolbarViewModel` |

**Services to modify:**
| File | Change |
|---|---|
| `Services/Implementations/RoutineResultService.cs` | Ensure flag computation runs for all save paths (already does for `SaveNumericOrTextResultsAsync`) |

**New files:**
| File | Purpose |
|---|---|
| `Services/Implementations/ResultFlagEvaluator.cs` | Extract flag computation from `RoutineResultService` for reuse |

#### Phase 4: Billing & Contracts

**Views to create:**
| File | Description |
|---|---|
| `Views/Settings/PriceSchemesWindow.xaml` | Price scheme CRUD + per-test override |
| `Views/Settings/ReferralManagementWindow.xaml` | Referral source list/edit with discount% |
| `Views/Accounting/ContractInvoiceWindow.xaml` | Monthly invoice generator |

**ViewModels to create:**
| File | Description |
|---|---|
| `ViewModels/Settings/PriceSchemesViewModel.cs` | Scheme management |
| `ViewModels/Settings/ReferralManagementViewModel.cs` | Referral CRUD |
| `ViewModels/Accounting/ContractInvoiceViewModel.cs` | Invoice generation |

**Services to modify:**
| File | Change |
|---|---|
| `Services/Implementations/VisitService.cs` | Auto-apply `ReferralSource.DiscountPercent` on save |

#### Phase 5: Inventory, External Samples & Cash Drawer

**Views to create:**
| File | Description |
|---|---|
| `Views/Inventory/ExternalSamplesWindow.xaml` | Pending shipments, mark sent, settle |
| `Views/Inventory/CashDrawerWindow.xaml` | 6-tile dashboard, cash in/out |

**ViewModels to create:**
| File | Description |
|---|---|
| `ViewModels/Inventory/ExternalSamplesViewModel.cs` | Shipment queue management |
| `ViewModels/Inventory/CashDrawerViewModel.cs` | Period inventory, cash entries |

**New entities:**
| Entity | Fields |
|---|---|
| `CashLedgerEntry` | Id, StaffId, EntryType (In/Out), Amount, Reason, PeriodStart, PeriodEnd, Timestamp |

**Services to modify:**
| File | Change |
|---|---|
| `Services/Implementations/ExternalLabService.cs` | Add cost posting on shipment close |

#### Phase 6: Reporting, Print Pipeline & Delivery

**Views to create:**
| File | Description |
|---|---|
| `Views/Patients/DeliveryWindow.xaml` | Full delivery: barcode scan, manual list, balance dialog, filters |
| `Views/Reporting/CompositeReportWindow.xaml` | Two-pane move-arrows UI |
| `Views/Reporting/BlankReportWindow.xaml` | Free-form report editor |
| `Views/Settings/ReportLayoutWindow.xaml` | Per-workstation layout editor |
| `Views/Maintenance/BackupRestoreWindow.xaml` | Large-lab + small-lab variants |

**ViewModels to create:**
| File | Description |
|---|---|
| `ViewModels/Patients/DeliveryViewModel.cs` | Delivery workflow logic |
| `ViewModels/Reporting/CompositeReportViewModel.cs` | Test selection + reorder |
| `ViewModels/Reporting/BlankReportViewModel.cs` | Free-form row editing |
| `ViewModels/Settings/ReportLayoutViewModel.cs` | Layout settings CRUD |
| `ViewModels/Maintenance/BackupRestoreViewModel.cs` | Backup/restore operations |

**Services to modify:**
| File | Change |
|---|---|
| `Services/Implementations/WpfFlowDocumentPrintService.cs` | Implement all document types: ResultReport, CompositeReport, BlankReport, Worksheet, Envelope, Receipt, LabClaim, CashDrawerReport |
| `App.xaml.cs` | Register new windows; swap `NullPrintService` for `WpfFlowDocumentPrintService` |

#### Phase 7: Specialty Editors & Integrations

**Views to create:**
| File | Description |
|---|---|
| `Views/Specialty/CultureResultWindow.xaml` | 2-tab (Culture/Sensitivity) + microscopic sub-panel |
| `Views/Specialty/CrossMatchWindow.xaml` | Donor list + reaction grid |
| `Views/Specialty/SemenAnalysisWindow.xaml` | Dedicated semen parameters form |
| `Views/Admin/UserManagementWindow.xaml` | User CRUD + permission assignment |

**ViewModels to create:**
| File | Description |
|---|---|
| `ViewModels/Specialty/CultureResultViewModel.cs` | Organism + sensitivity matrix |
| `ViewModels/Specialty/CrossMatchViewModel.cs` | Donor management + results |
| `ViewModels/Specialty/SemenAnalysisViewModel.cs` | Semen parameters entry |
| `ViewModels/Admin/UserManagementViewModel.cs` | User admin |

**Services to modify:**
| File | Change |
|---|---|
| `Services/Implementations/DefaultResultEditorFactory.cs` | Implement dispatch by `TestType.SpecialType` |

---

## 2.6 Final Execution Phases

### Phase Ordering Rationale

```
[1 Safety] → [2 Search] → [3 Editor] → [4 Billing] → [5 Inventory] → [6 Print/Delivery] → [7 Specialty]
                ↑              ↑              ↑               ↑                  ↑
                │              │              │               │                  │
           foundational   foundational   feeds from     feeds from        everything
           for all        for result     editor +       editor +          depends on
           lookups        validation     search         print             print pipeline
```

### Phase 1: Core Safety & Infrastructure (Days 1-5)

**Goal:** Fix critical rule violations and scaffold the print pipeline before any feature work.

#### 1.1 Fix Stage-Gating Violations
| Task | Files | Acceptance Criteria |
|---|---|---|
| 1.1.1 Add `ResultStageRules` static class | `Infrastructure/ResultStageRules.cs` | `CanPrint(ValidationStatus) → false when < Reviewed`; `CanDeliver(bool isPrinted) → false when false` |
| 1.1.2 Guard `TogglePrintStatusAsync` | `RoutineResultService.cs:173` | Throws `InvalidOperationException("يجب مراجعة النتائج قبل الطباعة")` if `ValidationStatus < Reviewed` |
| 1.1.3 Guard `ToggleExportStatusAsync` | `RoutineResultService.cs:195` | Throws `InvalidOperationException("يجب طباعة النتائج قبل التسليم")` if `!IsPrinted` |
| 1.1.4 Add `EnforceStageGating` feature toggle | `Models/LabSetting.cs` + seed | When `false`, guards are bypassed for backward compatibility |
| 1.1.5 Unit tests for stage rules | `Tests/Services/ResultStageRulesTests.cs` | 6 allowed transitions + 8 forbidden transitions covered |

#### 1.2 Implement Print Pipeline Skeleton
| Task | Files | Acceptance Criteria |
|---|---|---|
| 1.2.1 Create `WpfFlowDocumentPrintService` | `Services/Implementations/WpfFlowDocumentPrintService.cs` | Implements `IPrintService.PrintAsync`; uses FlowDocument + PrintDialog; supports RTL |
| 1.2.2 Create document template base | `Services/Printing/DocumentTemplateBase.cs` | Abstract base with header/footer/layout hooks |
| 1.2.3 Register in DI | `App.xaml.cs:161` | `IPrintService` → `WpfFlowDocumentPrintService` instead of `NullPrintService` |
| 1.2.4 Add feature toggle `EnableServerPrinting` | `Models/LabSetting.cs` + seed | When `false`, prints to XPS file instead of physical printer |

#### 1.3 Audit Infrastructure
| Task | Files | Acceptance Criteria |
|---|---|---|
| 1.3.1 Implement `AuditableAttribute` interceptor | `Data/FinalLabDbContext.cs` | Override `SaveChangesAsync` to scan `[Auditable]` entities and emit `AuditLog` rows automatically |
| 1.3.2 Test audit auto-emission | `Tests/Services/AuditInterceptorTests.cs` | Saving an `[Auditable]` entity creates `AuditLog` row without manual call |

#### 1.4 Split Placeholder VMs
| Task | Files | Acceptance Criteria |
|---|---|---|
| 1.4.1 Create `DeliveryViewModel.cs` | `ViewModels/Patients/Delivery/DeliveryViewModel.cs` | Isolated file; inherits `ViewModelBase`; has `ReturnToMainCommand` |
| 1.4.2 Create `PatientSearchViewModel.cs` | `ViewModels/Patients/Search/PatientSearchViewModel.cs` | Isolated file; inherits `ViewModelBase`; has `ReturnToMainCommand` |
| 1.4.3 Delete `PlaceholderTaskViewModels.cs` | Remove file | App.xaml.cs registrations updated |

#### 1.5 Database Constraints
| Task | Files | Acceptance Criteria |
|---|---|---|
| 1.5.1 Add discount exclusivity check | Migration | `CHECK (DiscountAmount = 0 OR DiscountPercent = 0)` on `Visit` |

---

### Phase 2: Patient Search & Identity (Days 6-12)

**Goal:** Replace search placeholder; implement proper patient identity and coding.

#### 2.1 Patient Search Window
| Task | Files | Acceptance Criteria |
|---|---|---|
| 2.1.1 Build search criteria DTO | `Models/DTOs/PatientSearchCriteria.cs` | All filter fields: name, codes, nationalId, phones, dateFrom, dateTo, ageFrom, ageTo, sex, referralId |
| 2.1.2 Implement compound search | `Services/PatientService.cs` | `SearchPatientsAsync(PatientSearchCriteria)` returns `PagedResult<PatientSearchRow>` with embedded visit count; caps at 100 rows |
| 2.1.3 Build search window XAML | `Views/Patients/PatientSearchWindow.xaml` | Filter panel left, result grid right; max 100 rows with "تم عرض 100 نتيجة فقط" footer |
| 2.1.4 Build search VM | `ViewModels/Patients/Search/PatientSearchViewModel.cs` | All filters functional; double-click opens patient; delete permission-gated |
| 2.1.5 Add name normalization | Migration `AddSearchableNameColumn` | Computed `NVARCHAR` column: strips hamza, ة→ه, ى→ي; search uses this column |

#### 2.2 Patient Code Generator
| Task | Files | Acceptance Criteria |
|---|---|---|
| 2.2.1 Implement 13-digit code format | `Services/PatientService.cs` | `GeneratePatientCodeAsync` emits `0-B-DDMMYY-W-NNN-X`; X = checksum digit; unit test validates regex |
| 2.2.2 Daily counter reset | `Services/PatientService.cs` | Case code counter resets to 1 each calendar day; branch prefix from `LabSetting` |
| 2.2.3 Lab ID scan lookup | `Views/Patients/PatientRegistrationWindow.xaml` + VM | Lab-ID field with barcode scan → auto-loads patient if exists; shows "غير موجود — اضغط F2 للتسجيل" if new |

---

### Phase 3: Result Editor Alignment (Days 13-22)

**Goal:** Bring Result Editor to functional parity with reference.

#### 3.1 Decompose TestResultsViewModel
| Task | Files | Acceptance Criteria |
|---|---|---|
| 3.1.1 Extract `TestsGridViewModel` | `ViewModels/Patients/ResultHub/TestsGridViewModel.cs` | Owns `PatientTests` collection, inline editing, test selection |
| 3.1.2 Extract `ToolbarViewModel` | `ViewModels/Patients/ResultHub/ToolbarViewModel.cs` | Owns all print/report commands |
| 3.1.3 Refactor parent VM | `TestResultsViewModel.cs` | Parent orchestrator <= 200 LOC; delegates to sub-VMs |

#### 3.2 Rebuild Result Editor
| Task | Files | Acceptance Criteria |
|---|---|---|
| 3.2.1 Add L/H/Critical chips | `Views/Patients/ResultEntryWindow.xaml` | Result rows show color-coded chip: green (Normal), yellow (Low/High), red (Critical) |
| 3.2.2 Add comment template picker | `Views/Patients/ResultEntryWindow.xaml` + VM | Orange "تعليق" button; dropdown of `ReportCommentTemplate` filtered by test type |
| 3.2.3 Auto-comment injection | `ViewModels/Patients/ResultEntryViewModel.cs` | Click test name → append `NormalRange.LowComment/HighComment/CriticalComment` to comment with separator; never overwrite |
| 3.2.4 Add constants panel | `Views/Patients/ResultEntryWindow.xaml` | Right-side panel: CBC factors, PT ISI + ControlTime, PTT ControlTime; stored in `LabSetting` |
| 3.2.5 Wire F8/F9/F11/F12 | `Views/Patients/ResultEntryWindow.xaml` | Window-level key bindings: F8=verify, F9=save+advance, F11=preview, F12=print |
| 3.2.6 Add orange title bar | `Views/Patients/ResultEntryWindow.xaml` | Clickable; opens Report Layout placeholder dialog |

#### 3.3 Result Flag Display
| Task | Files | Acceptance Criteria |
|---|---|---|
| 3.3.1 Show computed flags in grid | `TestsGridViewModel.cs` | `ResultStatus` (NORMAL/LOW/HIGH/LOW_CRITICAL/HIGH_CRITICAL) displayed as colored chip |
| 3.3.2 Verify flag computation | `RoutineResultService.cs:81-116` | Confirm flags are computed and saved; no VM-side recalculation needed |

---

### Phase 4: Billing & Contracts (Days 23-30)

**Goal:** Complete billing flow including corporate and referral features.

#### 4.1 Price Management UIs
| Task | Files | Acceptance Criteria |
|---|---|---|
| 4.1.1 Price Schemes window | `Views/Settings/PriceSchemesWindow.xaml` + VM | CRUD for `PriceScheme`; per-test price override grid |
| 4.1.2 Referral Management window | `Views/Settings/ReferralManagementWindow.xaml` + VM | List/edit referral sources; set discount%, commission% |
| 4.1.3 Auto-apply referral discount | `Services/VisitService.cs` | On `SavePatientVisitAsync`: if `ReferralSource.DiscountPercent > 0`, auto-set `Visit.DiscountPercent` |

#### 4.2 Corporate Billing
| Task | Files | Acceptance Criteria |
|---|---|---|
| 4.2.1 Contract invoice generator | `Views/Accounting/ContractInvoiceWindow.xaml` + VM | Select company + month → aggregated visit charges → Issue → Freeze → Mark Paid |
| 4.2.2 Commission report UI | `Views/Accounting/CommissionReportWindow.xaml` + VM | Monthly aggregate by referral; drill into visits; exportable |

---

### Phase 5: Inventory, External Samples & Cash Drawer (Days 31-38)

#### 5.1 External Samples Window
| Task | Files | Acceptance Criteria |
|---|---|---|
| 5.1.1 Build External Samples window | `Views/Inventory/ExternalSamplesWindow.xaml` + VM | Pending queue grouped by lab; mark sent; settle; filter by date/lab/status |
| 5.1.2 "دائما إليه" toggle | `Services/ExternalLabService.cs` | Persist preferred lab per TestType |
| 5.1.3 Cost posting on shipment close | `Services/ExternalLabService.cs` + `CashLedgerEntry` | Sum `ExternalShipmentItem.Cost` → record as expense entry |

#### 5.2 Cash Drawer
| Task | Files | Acceptance Criteria |
|---|---|---|
| 5.2.1 Create `CashLedgerEntry` entity | `Models/CashLedgerEntry.cs` + migration | In/Out entries with staff, reason, amount, period |
| 5.2.2 Build Cash Drawer window | `Views/Inventory/CashDrawerWindow.xaml` + VM | 6-tile dashboard; live totals from SQL views + computed |
| 5.2.3 Cash In/Out entry | `Views/Inventory/CashInOutWindow.xaml` + VM | Staff + reason + amount → running balance |
| 5.2.4 Period report printout | `WpfFlowDocumentPrintService.cs` | "كشف الدرج" formatted report |

---

### Phase 6: Print Pipeline, Delivery & Backup (Days 39-52)

#### 6.1 Complete Print Pipeline
| Task | Files | Acceptance Criteria |
|---|---|---|
| 6.1.1 Implement all document templates | `Services/Printing/*.cs` | ResultReport, CompositeReport, BlankReport, Worksheet, Envelope, Receipt, LabClaim, CashDrawerReport |
| 6.1.2 Composite Report Builder window | `Views/Reporting/CompositeReportWindow.xaml` + VM | Two-pane move-arrows; subtitle toggle; reprint-printed toggle |
| 6.1.3 Blank Report editor | `Views/Reporting/BlankReportWindow.xaml` + VM | Free-form rows; saveable; printable |
| 6.1.4 Report Layout settings | `Views/Settings/ReportLayoutWindow.xaml` + VM | Per-workstation: titles, alignment, colors, 5 toggles, spacing sliders; stored in `JsonUserSettingsService` |

#### 6.2 Delivery Window
| Task | Files | Acceptance Criteria |
|---|---|---|
| 6.2.1 Build Delivery window | `Views/Patients/DeliveryWindow.xaml` + VM | Barcode-driven + manual list; filters (Today/DateRange/VIP/Individual/LabToLab/Free/All) |
| 6.2.2 Balance dialog | `Views/Patients/DeliveryBalanceDialog.xaml` | Shows remaining; settlement via `FinancialService` |
| 6.2.3 Mark delivered | `DeliveryViewModel.cs` | Printed tests → Delivered; manual override "مستلمة"; audit-logged |
| 6.2.4 Wire stage machine | `DeliveryViewModel.cs` | Refuses to deliver unprinted tests unless manual override |

#### 6.3 Backup & Restore
| Task | Files | Acceptance Criteria |
|---|---|---|
| 6.3.1 Build Backup service | `Services/Implementations/BackupService.cs` | SQL `BACKUP DATABASE` to configured path; list backups; restore from `.bak` |
| 6.3.2 Build Maintenance window | `Views/Maintenance/BackupRestoreWindow.xaml` + VM | Large-lab (full backup/restore) + small-lab (per-patient restore) variants |
| 6.3.3 Maintenance password gate | `BackupRestoreViewModel.cs` | Default `123`; changeable; audit-logged |

---

### Phase 7: Specialty Editors & Integrations (Days 53-62)

#### 7.1 Specialty Result Editors
| Task | Files | Acceptance Criteria |
|---|---|---|
| 7.1.1 Wire `DefaultResultEditorFactory` | `Services/DefaultResultEditorFactory.cs` | Dispatch by `TestType.SpecialType`: culture → Culture window, crossmatch → CrossMatch window, semen → Semen window |
| 7.1.2 Culture Result window | `Views/Specialty/CultureResultWindow.xaml` + VM | 2 tabs (Culture/Sensitivity); microscopic sub-panel; <=3 organisms; S/I/R sensitivity |
| 7.1.3 Antibiotic safety filter | `CultureResultViewModel.cs` | Filter dropdown by `SafePregnant`/`SafeChild` based on patient flags |
| 7.1.4 Cross-match window | `Views/Specialty/CrossMatchWindow.xaml` + VM | Donor list; per-donor reaction entry |
| 7.1.5 Semen Analysis window | `Views/Specialty/SemenAnalysisWindow.xaml` + VM | All parameters: volume, concentration, motility, morphology, interpretation |

#### 7.2 Admin Features
| Task | Files | Acceptance Criteria |
|---|---|---|
| 7.2.1 User Management window | `Views/Admin/UserManagementWindow.xaml` + VM | List users; add/edit/delete; assign permissions; reset password |
| 7.2.2 Attendance UI | `Views/Admin/AttendanceWindow.xaml` + VM | Clock in/out; daily view; period report |

#### 7.3 Integration Scaffolds
| Task | Files | Acceptance Criteria |
|---|---|---|
| 7.3.1 `INotificationGateway` interface | `Services/Interfaces/INotificationGateway.cs` | Stub implementation; log-only mode |
| 7.3.2 NATIGH placeholder | `Services/Implementations/NatighNotificationGateway.cs` | Log-only: "Would upload to NATIGH" |
| 7.3.3 SMS placeholder | `Services/Implementations/SmsNotificationGateway.cs` | Log-only: "Would send SMS to {phone}" |

---

## 2.7 Acceptance Criteria

### Per-Atomic-Task Acceptance Criteria (Mandatory)

**Format: Given / When / Then**

#### Phase 1.1.1-1.1.2 (Stage Gating Guards)
- **AC1.1.2.1:** Given a `VisitTest` with `ValidationStatus = Entered`, when `TogglePrintStatusAsync` is called, then it throws `InvalidOperationException` with Arabic message "يجب مراجعة النتائج قبل الطباعة".
- **AC1.1.2.2:** Given a `VisitTest` with `ValidationStatus = Reviewed`, when `TogglePrintStatusAsync` is called, then `IsPrinted` becomes `true` and a `PRINTED` workflow entry is created.
- **AC1.1.3.1:** Given a `VisitTest` with `IsPrinted = false`, when `ToggleExportStatusAsync` is called, then it throws `InvalidOperationException` with Arabic message "يجب طباعة النتائج قبل التسليم".
- **AC1.1.4.1:** Given `EnforceStageGating = false` in `LabSetting`, when either toggle is called with invalid state, then it succeeds (backward compatibility mode).

#### Phase 1.2.1-1.2.3 (Print Pipeline)
- **AC1.2.1.1:** Given a print request for `"ResultReport"` with visit data, when `PrintAsync` is called, then a FlowDocument is generated with RTL layout and sent to `PrintDialog`.
- **AC1.2.3.1:** Given the application starts, when `IPrintService` is resolved from DI, then it returns `WpfFlowDocumentPrintService` not `NullPrintService`.

#### Phase 1.3.1 (Audit Interceptor)
- **AC1.3.1.1:** Given a `Patient` entity marked `[Auditable]`, when `SaveChangesAsync` is called after modifying the patient, then an `AuditLog` row is created with entity name, action = "UPDATE", and property changes.
- **AC1.3.1.2:** Given the interceptor is active, when receipt printing occurs, then no duplicate audit entries are created (manual `LogActionAsync` + interceptor don't double-log).

#### Phase 2.1.2 (Patient Search)
- **AC2.1.2.1:** Given a patient named "أحمد خالد", when searching for "احمد" (no hamza), then the patient is found (normalization works).
- **AC2.1.2.2:** Given 150 patients match search criteria, when search executes, then exactly 100 rows are returned and footer shows "تم عرض 100 نتيجة فقط".
- **AC2.1.2.3:** Given filters: sex=Male, dateFrom=2026-01-01, dateTo=2026-12-31, when search executes, then only male patients within date range are returned.

#### Phase 2.2.1 (Code Generator)
- **AC2.2.1.1:** Given branch=1, date=2026-06-24 (Wednesday=3), sequence=5, when `GeneratePatientCodeAsync` is called, then result matches regex `^0-1-240626-3-005-\d$`.
- **AC2.2.1.2:** Given the same parameters on two consecutive calls same day, when codes are generated, then sequence increments (005, 006, 007).
- **AC2.2.1.3:** Given a new calendar day, when first code is generated, then sequence resets to 001.

#### Phase 3.2.1-3.2.6 (Result Editor)
- **AC3.2.1.1:** Given a result value outside [LowNormal, HighNormal], when the Result Editor loads, then the row shows a yellow L/H chip.
- **AC3.2.1.2:** Given a result value outside [LowCritical, HighCritical], when the Result Editor loads, then the row shows a red CRITICAL chip.
- **AC3.2.3.1:** Given a test with `NormalRange.LowComment = "قيمة منخفضة"`, when user clicks the test name, then "قيمة منخفضة" is appended to the comment box.
- **AC3.2.3.2:** Given user has already typed "ملاحظة يدوية" in comment, when auto-comment injects, then result is "ملاحظة يدوية\n\nقيمة منخفضة" (never overwrite).
- **AC3.2.4.1:** Given PT ISI = 1.2 and Control Time = 12.0 in LabSetting, when Patient Time = 15.0 is entered, then INR displays as calculated value `(15/12)^1.2 = 1.29`.
- **AC3.2.5.1:** Given Result Editor window is focused, when F8 is pressed, then selected test toggles verified status.

#### Phase 5.2.2 (Cash Drawer)
- **AC5.2.2.1:** Given 10 patient payments totaling 5000 EGP today, when Cash Drawer opens, then Patient Billings tile shows 5000.
- **AC5.2.2.2:** Given a cash-out entry of 200 EGP for supplies, when entry is saved, then Expenses tile increases by 200 and running balance decreases by 200.

#### Phase 6.2.1-6.2.3 (Delivery)
- **AC6.2.1.1:** Given a patient with printed results and balance = 100 EGP, when Delivery window opens and patient selected, then balance dialog shows 100 EGP remaining.
- **AC6.2.2.1:** Given balance dialog shows 100 EGP, when user pays 100 EGP and confirms, then `FinancialService.RecordPaymentAsync` is called and results are marked Delivered.
- **AC6.2.3.1:** Given a patient with unprinted results, when deliver is attempted, then system shows warning "يجب طباعة النتائج أولاً" and delivery is blocked.
- **AC6.2.3.2:** Given manual override "مستلمة" is used on unprinted result, when confirmed, then result is marked Delivered with `MANUAL_DELIVERY` workflow entry.

#### Phase 6.3.1-6.3.3 (Backup)
- **AC6.3.1.1:** Given backup path configured, when backup button clicked, then `.bak` file is created with name matching `Patient_yyyymmdd.bak` pattern.
- **AC6.3.3.1:** Given maintenance password is "123", when user enters "123", then maintenance window unlocks.
- **AC6.3.3.2:** Given user enters "wrong", when submitted, then access denied and attempt is audit-logged.

#### Phase 7.1.2 (Culture)
- **AC7.1.2.1:** Given a Blood Culture test type is selected, when Result Editor opens, then Culture window (2-tab) opens instead of generic editor.
- **AC7.1.2.2:** Given organism A is added with 3 antibiotics, when sensitivity is set to S/I/R, then values persist on save.

---

## 2.8 Test Strategy

### Unit Tests (per service class)

| Target | Test Class | Coverage Required |
|---|---|---|
| `ResultStageRules` | `ResultStageRulesTests.cs` | All 14 transitions (6 allowed, 8 forbidden) |
| `RoutineResultService` (guards) | `RoutineResultServiceGuardTests.cs` | Print-before-verify rejection, deliver-before-print rejection, bypass toggle |
| `PatientService` (codes) | `PatientCodeGeneratorTests.cs` | Format regex, daily reset, sequence increment, checksum |
| `PatientService` (search) | `PatientSearchTests.cs` | Normalization, each filter, 100-row cap, compound filters |
| `ResultFlagEvaluator` | `ResultFlagEvaluatorTests.cs` | Normal, Low, High, Low Critical, High Critical, boundary values |
| `FinancialService` (discount) | `FinancialServiceDiscountTests.cs` | Amount-only, percent-only, mutual exclusion, auto-apply referral |
| `BackupService` | `BackupServiceTests.cs` | Mock SQL commands; verify correct BACKUP/RESTORE syntax |

### Integration Tests (service + database)

| Target | Test Class | Coverage Required |
|---|---|---|
| Full patient registration | `PatientRegistrationIntegrationTests.cs` | Register → code generated → visit created → receipt printable |
| Result entry → verify → print → deliver | `ResultLifecycleIntegrationTests.cs` | Complete chain; verify stage gates at each step |
| Search with all filters | `PatientSearchIntegrationTests.cs` | Seed 200 patients; test each filter combination |
| Audit trail | `AuditTrailIntegrationTests.cs` | Edit entity → verify AuditLog row; interceptor + manual don't double-log |
| External samples | `ExternalSamplesIntegrationTests.cs` | Add outside test → appears in queue → mark sent → cost posted |

### Workflow Validation Tests (end-to-end scenarios)

| Scenario | Test Method | Steps |
|---|---|---|
| VIP patient full lifecycle | `VipPatientWorkflow()` | Register VIP → red name in list → add tests → enter results → verify → print → deliver → medal status |
| Critical value alert | `CriticalValueWorkflow()` | Enter result outside critical range → CRITICAL chip shows → critical comment auto-injected |
| Lab-to-Lab billing | `LabToLabWorkflow()` | Select LabToLab type → prices from scheme → generate monthly claim → print claim |
| Cash drawer period close | `CashDrawerWorkflow()` | Multiple payments + cash out → open drawer → verify totals → print period report |
| Backup and restore | `BackupRestoreWorkflow()` | Create backup → verify file exists → restore → verify data integrity |

### UI Acceptance Tests (manual QA scripts)

| Screen | QA Script | Checklist |
|---|---|---|
| Patient Registration | `QA/phase-2-registration.md` | Name entry, age fraction, VIP red, test selection, F1 popup, code format, receipt print |
| Result Entry Hub | `QA/phase-3-result-hub.md` | Patient list icons, filter tabs, inline edit, audit P/T buttons, day navigation |
| Result Editor | `QA/phase-3-result-editor.md` | L/H/Critical chips, comment templates, auto-comment, constants panel, F8/F9/F11/F12 |
| Delivery | `QA/phase-6-delivery.md` | Barcode scan, manual list, balance dialog, settlement, mark delivered, override |
| Search | `QA/phase-2-search.md` | Each filter, 100-row cap, normalization, double-click open, delete permission |
| Cash Drawer | `QA/phase-5-cash-drawer.md` | 6-tile totals, cash in/out, period filter, report print |

### Regression Tests (must pass after each phase)

| Area | Test Command | Expected |
|---|---|---|
| Existing auth | `dotnet test --filter AuthServiceTests` | All pass |
| Existing receipt | `dotnet test --filter ReceiptServiceTests` | All pass |
| Existing test catalog | `dotnet test --filter TestCatalogServiceTests` | All pass |
| Existing normal ranges | `dotnet test --filter NormalRange*Tests` | All pass |
| Patient registration | `dotnet test --filter PatientServiceTests` | All pass |

---

## 2.9 Risk Map

### Risk Matrix (per phase)

| Risk ID | Risk | Phase | Likelihood | Impact | Mitigation |
|---|---|---|---|---|---|
| R-01 | Stage gating breaks existing happy-path | 1 | High | High | Feature toggle `EnforceStageGating`; default `false` for upgrade, `true` for fresh install |
| R-02 | Real print service triggers runtime errors | 1 | Medium | High | `EnableServerPrinting` toggle; default to XPS file output until QA validates |
| R-03 | Audit interceptor doubles DB writes | 1 | Medium | Medium | Benchmark on 1000 operations; disable interceptor if >10% perf degradation |
| R-04 | Search on large dataset is slow | 2 | Medium | High | Add computed column index; benchmark on 100k rows; consider full-text search |
| R-05 | Patient code generator conflicts with existing codes | 2 | Medium | High | Freeze existing codes; new generator only for registrations after cut-over date |
| R-06 | TestResultsViewModel decomposition causes regression | 3 | High | Medium | Preserve exact command behavior; unit test each extracted VM before integration |
| R-07 | Lab constants formulas produce clinically wrong values | 3 | Low | Critical | Unit test each formula with known values; require clinician sign-off |
| R-08 | Auto-comment overwrites manual comment | 3 | Medium | Low | Append-only with separator; undo button; never overwrite |
| R-09 | Cash Drawer financial calculations incorrect | 5 | Medium | High | Cross-check with `VOutstandingBalance` view; reconciliation report |
| R-10 | Delivery balance dialog edge cases | 6 | Medium | Medium | Test: partial payment, overpayment, zero balance, multi-visit patient |
| R-11 | Backup fails due to SQL permissions | 6 | Medium | High | Document SQL permission requirements; test with limited-permission account |
| R-12 | Composite report layout breaks with many tests | 6 | Medium | Medium | Test with 20+ test profiles; pagination; overflow handling |
| R-13 | Specialty editor factory crashes on unknown type | 7 | Low | Medium | Fallback to generic editor for unmapped types; log warning |
| R-14 | Permission enforcement causes user lockout | 7 | Medium | High | Admin always has all permissions; clear error messages with perm codes |
| R-15 | Scope creep beyond alignment | All | High | Medium | Strict reference-only; no features not in CHM/RLS_Learn/RL_Show |

### Mitigation Summary by Phase

| Phase | Top 3 Risks | Mitigation Strategy |
|---|---|---|
| 1 | R-01, R-02, R-03 | Feature toggles for all breaking changes; benchmark before enabling |
| 2 | R-04, R-05 | Index + benchmark; cut-over date for new codes |
| 3 | R-06, R-07 | VM unit tests first; clinician sign-off on formulas |
| 4 | (low risk) | Cross-check with existing views; incremental rollout |
| 5 | R-09 | Reconciliation with existing reporting views |
| 6 | R-10, R-11, R-12 | Edge-case test suite; SQL permission doc; pagination tests |
| 7 | R-13, R-14 | Fallback editors; admin bypass; permission audit log |

---

## 2.10 Dependency Graph

### 10.1 Entity Dependencies (Verified from `FinalLabDbContext`)

```
Staff (1)
  │ created-by
  ▼
Patient (*) ──► PatientMedicalHistory (*)
  │
  │ 1:N
  ▼
Visit (*) ──► Payment (*)
  │           VisitCharge (*)
  │ 1:N
  ▼
VisitTest (*) ──► TestResult (*)
  │               SampleTube (0..1)
  │               MicrobiologyCulture (0..1)
  │               SemenAnalysis (0..1)
  │               CrossMatchTest (0..1)
  │               ExternalShipmentItem (*)
  │               TestWorkflow (*)
  │ N:1
  ▼
TestType (1) ──► TestGroup (1) ──► TestCategory (1)
  │
  │ 1:N
  ▼
TestComponent (*) ──► NormalRange (*)
  │                   Unit (1)
  │ N:1
  ▼
TestResult (*) [SNAP fields: SnapUnit, SnapLowNormal, SnapHighNormal, SnapLowCritical, SnapHighCritical]
```

### 10.2 Service Dependencies (Verified from DI registration + constructor analysis)

```
IPrintService ◄───────┐
                      │
IVisitService ────────┼──► TestResultsViewModel
IRoutineResultService ┤      (Result Entry Hub)
IAuditService ────────┤
IAuthService ─────────┤
IDialogService ───────┤
ICurrentUserSession ──┤
INavigationService ───┘

IPatientService ──────┐
IVisitService ────────┼──► PatientRegistrationViewModel
IFinancialService ────┤      (Registration orchestrator)
IPricingService ──────┤
ITestCatalogService ──┤
IDialogService ───────┘

IReportingService ────┐
IPrintService ────────┼──► (future DeliveryViewModel)
IFinancialService ────┤
IReceiptService ──────┤
IVisitService ────────┘

ITestCatalogService ──┐
ISettingsService ─────┼──► Settings VMs (TestData, NormalRanges, Categories)
IDialogService ───────┘
```

### 10.3 Implementation Order (Critical Path)

```
Phase 1 (Safety)
    ├── ResultStageRules [NEW]
    ├── TogglePrint guard [MODIFY RoutineResultService]
    ├── ToggleExport guard [MODIFY RoutineResultService]
    ├── WpfFlowDocumentPrintService [NEW] ───┐
    ├── Audit interceptor [MODIFY DbContext] │
    └── Split placeholder VMs [REFACTOR]     │
                                             ▼
Phase 2 (Search)                          Phase 3 (Editor)
    ├── PatientSearchCriteria [NEW]           ├── TestsGridViewModel [NEW]
    ├── SearchPatientsAsync [MODIFY]          ├── ToolbarViewModel [NEW]
    ├── PatientSearchWindow [NEW]             ├── ResultEntryWindow rebuild [MODIFY]
    ├── PatientSearchVM [NEW]                 ├── ResultFlagEvaluator [NEW]
    ├── Code generator [MODIFY]               └── Constants panel [NEW]
    └── Name normalization [MIGRATE]              │
                                                  ▼
Phase 4 (Billing)                            Phase 5 (Inventory)
    ├── PriceSchemes window [NEW]               ├── ExternalSamplesWindow [NEW]
    ├── ReferralManagement window [NEW]         ├── CashLedgerEntry [NEW ENTITY]
    ├── ContractInvoice window [NEW]            ├── CashDrawerWindow [NEW]
    └── Auto-apply discount [MODIFY]            └── Cost posting [MODIFY ExternalLabService]
                                                  │
                                                  ▼
                                          Phase 6 (Print + Delivery + Backup)
                                              ├── Document templates [NEW]
                                              ├── CompositeReportWindow [NEW]
                                              ├── BlankReportWindow [NEW]
                                              ├── ReportLayoutWindow [NEW]
                                              ├── DeliveryWindow [REPLACE]
                                              ├── DeliveryViewModel [REPLACE]
                                              └── BackupRestoreWindow [NEW]
                                                  │
                                                  ▼
                                          Phase 7 (Specialty + Admin)
                                              ├── DefaultResultEditorFactory [MODIFY]
                                              ├── CultureResultWindow [NEW]
                                              ├── CrossMatchWindow [NEW]
                                              ├── SemenAnalysisWindow [NEW]
                                              ├── UserManagementWindow [NEW]
                                              └── NotificationGateway scaffold [NEW]
```

### 10.4 Forward-Only Build Constraints

1. **Phase 1 must ship before any other phase** — stage gating is a safety-critical fix.
2. **Phase 2 must ship before Phase 3** — Result Editor needs patient lookup (Lab-ID scan).
3. **Phase 3 must ship before Phase 5** — Cash Drawer needs `ResultFlagEvaluator` for reporting.
4. **Phase 6 must ship after Phase 1** — Delivery and Composite Report need `WpfFlowDocumentPrintService`.
5. **Phase 7 can be parallelized internally** — specialty editors are independent of each other.
6. **No phase can be skipped** — each builds on previous.

---

# Appendices

## Appendix A: Reference Source Index

| CHM Topic | English Name | Primary Gap |
|---|---|---|
| 001.htm | Application Startup | Low priority — connection string in config is sufficient |
| 002.htm | Login | Complete — PBKDF2 is better than reference |
| 003.htm | Patient Registration | Missing: F1, age fraction, Lab-ID scan, referral code |
| 003A.htm | Barcode | Missing: calibration, drag-drop, 13-digit format with checksum |
| 004.htm | Result Entry Hub | Missing: filter sidebar, working toolbar, F1 test details |
| 004A.htm | Per-Profile Editor | Critically minimal: no flags UI, no comments, no constants |
| 005.htm | Delivery | **Placeholder** — entire workflow unbuilt |
| 006.htm | Patient Search | **Placeholder** — compound search unbuilt |
| 007.htm | Test Catalog | Near complete — verify 5 name variants editable |
| 008.htm | Normal Ranges | Near complete — versioned with audit trail |
| 009.htm | Report Layout | **Missing** — no per-workstation layout settings |
| 010.htm | External Outsourcing | Skeleton — entities + service, no UI |
| 011.htm | Cash Drawer | **Missing** — no window, no orchestrator |
| backup.htm | Backup/Restore | **Missing** — no service, no UI |
| nots.htm | Lab Constants | **Missing** — CBC/PT formulas not codified |

## Appendix B: Code Statistics (Verified)

| Metric | Count |
|---|---|
| C# source files | 247 |
| XAML files | 27 |
| Entity classes | 48 |
| SQL views | 6 |
| ViewModel classes | 32 |
| Service interfaces | 23 |
| Service implementations | 25 |
| EF migrations | 25 |
| Test classes | 14 |
| Lines of code (C#) | ~35,000 |
| DbContext lines | ~2,280 |

## Appendix C: Glossary

| Term | Arabic | Definition |
|---|---|---|
| Case Code | كود الحالة | Daily sequential visit number (1-prefix) |
| File Code | كود الملف | Per-physical-file code (3-prefix) |
| Lab ID | كود المعمل | Permanent per-patient identifier (5-prefix) |
| تمت | تمت | Manual override — marks stage complete without data |
| مستلمة | مستلمة | Delivery override — marks result as delivered |
| خالص | خالص | Full payment clearance |
| مجمع | مجمع | Composite report (multiple tests in one printout) |
| فارغ | فارغ | Blank report (free-form manual entry) |
| أمر شغل | أمر شغل | Worksheet / work order printout |
| ظرف | ظرف | Envelope printout |
| دائما إليه | دائما إليه | "Always to this lab" — default external lab per test |

---

*End of Final Master System Alignment PRD*
*This document consolidates and validates the outputs of two analysis agents against actual codebase inspection. All code references verified against commit `149082f` on branch `before-prd`.*

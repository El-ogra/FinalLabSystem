Phase 3 Work Plan — Result Editor Alignment
Pre-Execution Summary
Files to modify:FinalLabSystem/Models/DTOs/TestComponentResultDto.cs
FinalLabSystem/Models/ReportCommentTemplate.cs
FinalLabSystem/Data/FinalLabDbContext.cs
FinalLabSystem/Services/Interfaces/ITestCatalogService.cs
FinalLabSystem/Services/Interfaces/IRoutineResultService.cs
FinalLabSystem/Services/Interfaces/IResultEntryDialogService.cs
FinalLabSystem/Services/Implementations/TestCatalogService.cs
FinalLabSystem/Services/Implementations/RoutineResultService.cs
FinalLabSystem/Services/Implementations/ResultEntryDialogService.cs
FinalLabSystem/ViewModels/Patients/ResultEntryViewModel.cs
FinalLabSystem/ViewModels/Patients/TestSelectionViewModel.cs
FinalLabSystem/ViewModels/Menu/ReportSettingsMenuViewModel.cs
FinalLabSystem/ViewModels/Menu/TestDataMenuViewModel.cs
FinalLabSystem/Views/Patients/PatientRegistrationWindow.xaml.cs
FinalLabSystem/Views/Patients/ResultEntryWindow.xaml
FinalLabSystem/Views/Patients/TestSelectionView.xaml
FinalLabSystem/MainWindow.xaml
FinalLabSystem/App.xaml.cs

Files to create:FinalLabSystem/Models/Enums/ResultClinicalStatus.cs
FinalLabSystem/Models/Enums/ReportCommentTrigger.cs
FinalLabSystem/Services/Interfaces/IReportCommentEngine.cs
FinalLabSystem/Services/Interfaces/IReportCommentTemplateService.cs
FinalLabSystem/Services/Implementations/ReportCommentEngine.cs
FinalLabSystem/Services/Implementations/ReportCommentTemplateService.cs
FinalLabSystem/ViewModels/Settings/TestProfileWindowViewModel.cs
FinalLabSystem/ViewModels/Settings/TestProfileRowViewModel.cs
FinalLabSystem/ViewModels/Settings/TestProfileItemRowViewModel.cs
FinalLabSystem/ViewModels/Settings/ReportCommentTemplateViewModel.cs
FinalLabSystem/Views/Settings/TestProfileWindow.xaml
FinalLabSystem/Views/Settings/TestProfileWindow.xaml.cs
FinalLabSystem/Views/Settings/ReportCommentTemplateWindow.xaml
FinalLabSystem/Views/Settings/ReportCommentTemplateWindow.xaml.cs
FinalLabSystem/Views/Patients/ProfileSelectionDialog.xaml
FinalLabSystem/Views/Patients/ProfileSelectionDialog.xaml.cs
FinalLabSystem/Views/Converters/ValidationStatusToBrushConverter.cs
Phase 3 test files listed under Task 3.6

Files to delete: none
Migrations required:AddReportCommentTemplate_TriggerCondition

Estimated effort: 9 days
Execution Order
#	Task	Depends On	Risk	Estimated Days
1	Task 3.3 — ReportCommentTemplate Management Foundation/UI	None	High	1.5
2	Task 3.4 — Auto-Comment Injection Logic	3.3	High	1
3	Task 3.2 — TestProfile Management UI	None	Medium	1.5
4	Task 3.5 — TestProfile Expansion on Patient Registration	3.2 shared service methods	Medium	1
5	Task 3.1 — ResultEntryWindow Full Integration	3.4 for comments	High	2
6	Task 3.6 — Phase 3 Complete Test Suite	3.1-3.5	Medium	2

Task 3.1 — ResultEntryWindow Full Integration
Current State
ResultEntryViewModel currently saves non-empty Components through IRoutineResultService.SaveNumericOrTextResultsAsync, exposes SaveCommand, CancelCommand, SaveCompleted, and RequestClose.
ResultEntryWindow.xaml directly hosts a DataGrid bound to Components, with result, unit, range, verified, print, and status columns. It has only Save and Cancel buttons.
RoutineResultService computes persisted TestResult.ResultStatus strings during save, but no live UI validation exists.
IResultEditorFactory and DefaultResultEditorFactory exist and are registered in App.xaml.cs. Per binding decision Q3, the interface must remain unchanged and specialty editors are deferred.
What Needs to Change
Add ResultClinicalStatus for live clinical abnormality state. Extend TestComponentResultDto with clinical status and row save-selection state.
Update ResultEntryViewModel to accept patientAgeDays, patientGender, and isPregnant, plus IDialogService. Perform live validation in memory using existing snapshot fields on TestComponentResultDto: SnapLowNormal, SnapHighNormal, SnapLowCritical, SnapHighCritical. Do not add INormalRangeService.
Update IResultEntryDialogService.OpenAsync to carry the same patient context into ResultEntryDialogService: Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName, ObservableCollection<TestComponentResultDto> components, int patientAgeDays, string patientGender, bool isPregnant). ResultEntryDialogService must pass these values to ResultEntryViewModel. The caller that opens result entry from the patient/visit context, including PatientRegistrationWindow code-behind or the current result-entry caller, must supply patientAgeDays, patientGender, and isPregnant from loaded Patient/Visit data rather than querying a new service.
Before save, detect critical rows using ResultStatus values HIGH_CRITICAL or LOW_CRITICAL and call IDialogService.ShowConfirmation. If declined, abort save.
Add partial save behavior so only selected rows are passed to SaveNumericOrTextResultsAsync.
Add Save and Review behavior: save first, then call RoutineResultService.ToggleReviewStatusAsync(int visitTestId, int staffId) on success.
ToggleReviewStatusAsync contract: Task<bool> ToggleReviewStatusAsync(int visitTestId, int staffId). It updates TestResult.ValidationStatus from Entered (0) to Reviewed (1) for all results belonging to the visitTestId, and returns true when at least one row was updated, otherwise false.
Keep the existing DataGrid as the default editor and add an empty ContentPresenter placeholder for Phase 7.
Subtasks
3.1a - Add ResultClinicalStatus and live validation in ResultEntryViewModel using snapshot fields only.
3.1b - Add row save-selection state and partial-save filtering.
3.1c - Add Save and Review command plus ToggleReviewStatusAsync service contract/implementation.
3.1d - Add critical-value confirmation through IDialogService.ShowConfirmation before save.
3.1e - Add ContentPresenter placeholder while preserving the existing DataGrid and IResultEditorFactory contract.
Files to Modify
FinalLabSystem/Models/DTOs/TestComponentResultDto.cs — add live clinical status and partial-save selection state.
FinalLabSystem/ViewModels/Patients/ResultEntryViewModel.cs — add constructor context, in-memory status resolver, critical confirmation, partial save, save-and-review command.
FinalLabSystem/Services/Interfaces/IRoutineResultService.cs — add ToggleReviewStatusAsync.
FinalLabSystem/Services/Interfaces/IResultEntryDialogService.cs — extend OpenAsync signature with patientAgeDays, patientGender, and isPregnant.
FinalLabSystem/Services/Implementations/RoutineResultService.cs — implement review toggle using workflow ResultValidationStatus.
FinalLabSystem/Services/Implementations/ResultEntryDialogService.cs — pass patient context into the view model.
FinalLabSystem/Views/Patients/PatientRegistrationWindow.xaml.cs — provide patientAgeDays, patientGender, and isPregnant when opening the result entry dialog from patient/visit context.
FinalLabSystem/Views/Patients/ResultEntryWindow.xaml — add save checkbox column, save-and-review button, clinical-status styling, and empty editor placeholder.
Files to Create
FinalLabSystem/Models/Enums/ResultClinicalStatus.cs — Normal, Low, High, Critical.
FinalLabSystem/Views/Converters/ValidationStatusToBrushConverter.cs — visual mapping for live validation.
Verification Criteria

Low/high/critical/normal statuses resolve from snapshot fields without DB calls.

Critical save prompts with IDialogService.ShowConfirmation.

Cancelled critical confirmation prevents save.

Partial save submits only selected rows.

Save and Review saves first, then marks entered results reviewed.

ToggleReviewStatusAsync returns true only when at least one TestResult row changes from Entered to Reviewed, and false when no rows are eligible.

IResultEditorFactory interface remains unchanged.

Existing DataGrid behavior still works.
Risks & Mitigations
Risk: Clinical abnormality could be confused with workflow validation.

Mitigation: Use ResultClinicalStatus; do not reuse ResultValidationStatus.

Risk: Live validation may drift from save-time logic.

Mitigation: Use the same snapshot threshold semantics as persisted save status.

Task 3.2 — TestProfile Management UI
Current State
TestProfile and TestProfileItem exist. FinalLabDbContext exposes both DbSets and maps singular tables.
ITestCatalogService currently has only GetActiveProfilesAsync and GetProfileTestsAsync for profile reads. No profile CRUD UI exists.
TestDataMenuViewModel currently opens test data and category/group windows only.
What Needs to Change
Extend catalog service with profile CRUD and item management. Add a management window with master-detail behavior for profiles and profile items. Use soft delete by setting IsActive=false.
Add menu integration through TestDataMenuViewModel and navigation registration. TestDataMenuViewModel must add NavigateToProfilesCommand that calls INavigationService.OpenTaskWindow<TestProfileWindowViewModel>(). App.xaml.cs must register TestProfileWindowViewModel/TestProfileWindow and call navigationService.RegisterWindow<TestProfileWindowViewModel, TestProfileWindow>().
Files to Modify
FinalLabSystem/Services/Interfaces/ITestCatalogService.cs — add profile CRUD and item ordering methods.
FinalLabSystem/Services/Implementations/TestCatalogService.cs — implement profile CRUD, soft delete, add/remove item, reorder.
FinalLabSystem/ViewModels/Menu/TestDataMenuViewModel.cs — add NavigateToProfilesCommand for profile management via OpenTaskWindow<TestProfileWindowViewModel>().
FinalLabSystem/App.xaml.cs — register view models/windows and navigation, including navigationService.RegisterWindow<TestProfileWindowViewModel, TestProfileWindow>().
FinalLabSystem/MainWindow.xaml — add button/menu entry if required by current menu layout.
Files to Create
FinalLabSystem/ViewModels/Settings/TestProfileWindowViewModel.cs — master-detail orchestration.
FinalLabSystem/ViewModels/Settings/TestProfileRowViewModel.cs — profile row state.
FinalLabSystem/ViewModels/Settings/TestProfileItemRowViewModel.cs — item row state and sort order.
FinalLabSystem/Views/Settings/TestProfileWindow.xaml — profile management UI.
FinalLabSystem/Views/Settings/TestProfileWindow.xaml.cs — constructor/data context wiring.
Verification Criteria

Create profile persists active profile.

Update profile persists edits.

Delete performs soft delete.

Add/remove profile item updates TestProfileItem.

Reorder updates SortOrder.

Inactive profiles do not appear in active profile lookup.
Risks & Mitigations
Risk: Deleting profiles could affect historical visits.
Mitigation: Implement soft delete only.
Task 3.3 — ReportCommentTemplate Management UI
Current State
ReportCommentTemplate exists and is mapped to singular table ReportCommentTemplate. It has no TriggerCondition.
ReportSettingsMenuViewModel currently only shows a placeholder through IDialogService.
No comment-template service or UI exists.
What Needs to Change
Add trigger support using ReportCommentTrigger values: None, Low, High, Critical, Manual.
Add migration for nullable TriggerCondition on singular table ReportCommentTemplate, with default/backfill "Manual". The model property must be string? TriggerCondition, new records must default to "Manual" when no trigger is explicitly supplied, and existing rows must be backfilled to "Manual".
Create template CRUD service and management UI. Change ReportSettingsMenuViewModel to depend on INavigationService and open the template management window through OpenTaskWindow<ReportCommentTemplateViewModel>(). App.xaml.cs must register ReportCommentTemplateViewModel/ReportCommentTemplateWindow and call navigationService.RegisterWindow<ReportCommentTemplateViewModel, ReportCommentTemplateWindow>().
Files to Modify
FinalLabSystem/Models/ReportCommentTemplate.cs — add nullable string? TriggerCondition.
FinalLabSystem/Data/FinalLabDbContext.cs — map TriggerCondition and default value "Manual".
FinalLabSystem/ViewModels/Menu/ReportSettingsMenuViewModel.cs — replace placeholder dialog with INavigationService command opening ReportCommentTemplateViewModel.
FinalLabSystem/App.xaml.cs — register service, VM/window, navigation, including navigationService.RegisterWindow<ReportCommentTemplateViewModel, ReportCommentTemplateWindow>().
Files to Create
FinalLabSystem/Models/Enums/ReportCommentTrigger.cs — official trigger values.
FinalLabSystem/Services/Interfaces/IReportCommentTemplateService.cs — query and CRUD contract.
FinalLabSystem/Services/Implementations/ReportCommentTemplateService.cs — active-template queries, matching, create/update, soft delete.
FinalLabSystem/ViewModels/Settings/ReportCommentTemplateViewModel.cs — filter and CRUD UI state.
FinalLabSystem/Views/Settings/ReportCommentTemplateWindow.xaml — template management UI.
FinalLabSystem/Views/Settings/ReportCommentTemplateWindow.xaml.cs — constructor/data context wiring.
EF migration AddReportCommentTemplate_TriggerCondition — add nullable TriggerCondition to ReportCommentTemplate with defaultValue "Manual" and backfill existing records to "Manual".
Verification Criteria

Migration adds nullable TriggerCondition to ReportCommentTemplate.

Existing records backfill/default to Manual.

New templates can be created for test/component/trigger combinations.

Matching favors component-specific templates where applicable.

Delete is soft delete.
Risks & Mitigations
Risk: Existing rows lack trigger semantics.
Mitigation: Treat legacy records as Manual.
Task 3.4 — Auto-Comment Injection Logic
Current State
RoutineResultService.SaveNumericOrTextResultsAsync computes TestResult.ResultStatus and saves results. TestResult.Comment exists.
No code reads ReportCommentTemplate during save.
What Needs to Change
Add IReportCommentEngine and implementation. Map from TestResult.ResultStatus string to ReportCommentTrigger per binding decision Q2.
Hook comment engine into RoutineResultService after result status calculation and before save. Do not overwrite non-empty comments. Do not create explicit AuditLog rows; rely on existing FinalLabDbContext audit interceptor.
Files to Modify
FinalLabSystem/Services/Implementations/RoutineResultService.cs — inject and call comment engine.
FinalLabSystem/App.xaml.cs — register engine service.
Files to Create
FinalLabSystem/Services/Interfaces/IReportCommentEngine.cs — comment resolution/application contract.
FinalLabSystem/Services/Implementations/ReportCommentEngine.cs — status mapping and template resolution.
Verification Criteria

Low result injects low matching template.

High result injects high matching template.

Critical high/low injects critical matching template.

Normal result injects nothing.

Existing manual comment is preserved.

Save still succeeds if no matching template exists.
Risks & Mitigations
Risk: Auto-comment could overwrite manual clinical text.
Mitigation: Skip when Comment is non-empty.
Task 3.5 — TestProfile Expansion on Patient Registration
Current State
TestSelectionViewModel already loads active profiles and flattens profile tests into the existing "Profiles" filter. This behavior must remain.
TestSelectionView.xaml has no Add Profile button. No ApplyProfileAsync exists.
What Needs to Change
Add IDialogService dependency to TestSelectionViewModel. Add ApplyProfileAsync(int profileId) that loads profile tests, skips duplicates, uses TestType.DefaultPrice, and recalculates selected count/total state.
Create ProfileSelectionDialog as a small modal dialog showing GetActiveProfilesAsync() results. Open it directly from TestSelectionViewModel, following the existing simple modal pattern used elsewhere.
Files to Modify
FinalLabSystem/ViewModels/Patients/TestSelectionViewModel.cs — add dialog dependency, profile command, ApplyProfileAsync.
FinalLabSystem/Views/Patients/TestSelectionView.xaml — add Add Profile button.
FinalLabSystem/App.xaml.cs — register dialog if needed by constructor pattern.
Files to Create
FinalLabSystem/Views/Patients/ProfileSelectionDialog.xaml — profile list and select/cancel actions.
FinalLabSystem/Views/Patients/ProfileSelectionDialog.xaml.cs — modal selection result handling.
Verification Criteria

Existing "Profiles" filter behavior remains.

Applying a profile adds all missing tests.

Existing selected tests are not duplicated.

Prices use TestType.DefaultPrice.

Dialog supports select button, double-click select, and cancel.
Risks & Mitigations
Risk: Profile application could disturb discounts or existing selected tests.
Mitigation: Skip existing tests and do not overwrite selected rows.
Task 3.6 — Phase 3 Complete Test Suite
Current State
Existing tests cover current result save behavior, catalog service behavior, registration shortcuts, menus, and result status calculation. Phase 3-specific test files are absent.
Baseline is 280 per binding decision Q12. Do not use the PRD’s 258 baseline.
What Needs to Change
Add focused tests for new services, view models, and integration behavior. Preserve existing tests.
Files to Modify
Existing test helpers may be extended only where needed for shared setup.
Files to Create
FinalLabSystem.Tests/Services/RoutineResultServiceLiveValidationTests.cs
FinalLabSystem.Tests/Services/ReportCommentEngineTests.cs
FinalLabSystem.Tests/Services/ReportCommentTemplateServiceTests.cs
FinalLabSystem.Tests/Services/TestCatalogService_ProfileCrudTests.cs
FinalLabSystem.Tests/ViewModels/Settings/TestProfileWindowViewModelTests.cs
FinalLabSystem.Tests/ViewModels/Settings/ReportCommentTemplateViewModelTests.cs
FinalLabSystem.Tests/ViewModels/Patients/ResultEntryViewModelLiveValidationTests.cs
FinalLabSystem.Tests/ViewModels/Patients/TestSelectionViewModelProfileApplyTests.cs
FinalLabSystem.Tests/Integration/AutoCommentEndToEndTests.cs
Verification Criteria

Existing 280-test baseline remains green.

New tests cover scenario intent, not exact PRD count.

Auto-comment tests verify no manual overwrite.

Profile apply tests verify duplicate skipping.

Result editor tests verify live validation and critical confirmation.
Risks & Mitigations
Risk: WPF view-model tests may need dispatcher-safe setup.
Mitigation: Keep most coverage in services/view models; avoid fragile visual assertions.
Confirmation: All 6 tasks analyzed, no remaining ambiguities. Pre-answered decisions incorporated. Plan is ready for execution.


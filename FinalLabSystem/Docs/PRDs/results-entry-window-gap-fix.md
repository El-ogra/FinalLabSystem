# PRD: إصلاح وتحسين نافذة إدخال نتائج التحاليل

**المشروع:** FinalLabSystem
**التاريخ:** 2026-06-24
**الحالة:** جاهز للتنفيذ
**المرجع:** Docs/ReferenceSystem/results-entry-window-reference.md

---

## Problem Statement

نافذة إدخال نتائج التحاليل الحالية تحتوي على أخطاء حرجة في الـ Data Binding تسبب crash وحفظ بيانات خاطئة، بالإضافة إلى فجوات كبيرة مقارنة بالنظام المرجعي تؤثر على سرعة ودقة عمل موظفي المختبر.

## Solution

إصلاح الأخطاء الحرجة أولاً، ثم تنفيذ الميزات المفقودة تدريجياً حسب الأولوية.

## User Stories

### Critical (يجب إصلاحها فوراً)

1. As a lab technician, I want to enter test results without the application crashing, so that I can complete my work without interruptions
2. As a lab technician, I want multi-component tests to not accept inline results from the main grid, so that data integrity is maintained
3. As a lab technician, I want multi-component tests to show "See Report" instead of an empty result field, so that I can distinguish them from single-component tests

### High Priority (مهم لسرعة العمل)

4. As a lab technician, I want keyboard shortcuts (F2-F12, Enter, Ctrl, Esc) to navigate quickly between sections, so that I can work faster without using the mouse
5. As a lab technician, I want multi-component tests to have a distinct visual style (different row color), so that I can identify them at a glance
6. As a lab technician, I want the multi-component test window to include L/H indicators, Verified checkbox, Print checkbox, and Comment field, so that I can complete the result entry in one place
7. As a lab technician, I want the patient visit counter to show the correct number of visits (not number of tests), so that I can track patient history accurately

### Medium Priority (تحسينات مهمة)

8. As a lab technician, I want VIP patients to be highlighted in red, so that I can give them priority service
9. As a lab technician, I want the bottom toolbar to include Preview and SMS buttons, so that I can access all required functions
10. As a lab technician, I want the search box to support attendance number search, so that I can find patients faster
11. As a lab technician, I want the "Export" button to check that results are printed before allowing delivery, so that business rules are enforced

### Low Priority (تحسينات إضافية)

12. As a lab technician, I want the patient photo area to display the actual photo (not just a default icon), so that I can visually confirm patient identity
13. As a lab technician, I want the patient status icons to be clearly defined and assigned based on ComputedStatus, so that I can understand patient status at a glance

## Implementation Decisions

### Module Changes

**Models/DTOs/VisitTestItemDto.cs:**
- Add `IsMultiComponent` property (computed from `TotalComponents > 1`)
- Add `SeeReportText` property (returns "See Report" if multi-component, empty otherwise)
- Fix `SingleComponentResultValue` to not be a computed property with TwoWay binding issue

**Views/Patients/TestResultsWindow.xaml:**
- Add `DataTrigger` on `IsMultiComponent` to change row color and show "See Report"
- Disable `TextBox` in Result column when `TotalComponents > 1`
- Add `KeyBinding` for F2, F3, F4, F5, F6, F7, F8, F9, F12, Esc, Ctrl
- Add `DataTrigger` on `IsVip` to highlight patient name in red
- Change visit counter binding from `SelectedTests.Count` to `VisitCount`
- Add Preview and SMS buttons to bottom toolbar

**Views/Patients/ResultEntryWindow.xaml:**
- Add L/H column
- Add Verified checkbox column
- Add Print checkbox column
- Add Comment field
- Add Constants Panel (if applicable)

**ViewModels/Patients/TestResultsViewModel.cs:**
- Add F-key navigation commands
- Add Preview command
- Add SMS command
- Fix Export command to check Print status first

**ViewModels/Patients/ResultEntryViewModel.cs:**
- Add Comment property and save logic
- Add Verified/Print toggle logic

### Schema Changes

None required — all changes are UI/ViewModel layer.

### Architectural Decisions

- Follow existing MVVM pattern (ViewModelBase, AsyncRelayCommand)
- No MessageBox in ViewModels (use IDialogService)
- No async void outside framework signatures
- Code-behind remains minimal

## Testing Decisions

- Unit tests for VisitTestItemDto new properties
- Unit tests for TestResultsViewModel new commands
- Manual testing for keyboard shortcuts
- Manual testing for multi-component test flow

## Out of Scope

- Blood Culture window (Section 14 in reference)
- Combined Report window (Section 12 in reference)
- Empty Report window (Section 13 in reference)
- Database schema changes
- Print/SMS actual implementation (only UI buttons)

## Further Notes

- The reference system has 7 patient status icons — the current implementation has the enum but needs icon assignment logic
- The "See Report" text should appear in the Result column for multi-component tests
- Multi-component test rows should have a distinct background color (orange/green per reference)
- The constants panel in multi-component window is for values like ISI=1.5 for PT tests

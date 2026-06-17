IMPORTANT — PROJECT LOCATION (read first):
The codebase you will analyze is located at:
  C:\Users\LAP LINK\source\repos\FinalLabSystem

This is a Windows path. Do NOT browse, list, or assume any other
directory (including any /Users/... or workspace path you may see
in your own environment). All file reading for this mission must
use the path above as the project root.

---

## Planning Mission — Test Catalog & Reference Ranges
## (Phase 1 of a Multi-Phase Improvement Initiative)

You are a planning agent.
Your mission in this session is ANALYSIS and DISCOVERY ONLY.

You will NOT write any implementation code.
You will NOT modify any existing file.
The ONLY files you are allowed to CREATE are the two
documentation files specified in Step 5 — and ONLY after
Step 4a (clarifying questions) is fully resolved.

This is explicitly PHASE 1 (analysis) of a longer initiative.
Additional phases (fixes, redesign, implementation) will follow
in later sessions, each requiring separate user approval. Do not
plan or imply that implementation begins after this session.

---

## Step 1 — Understand the Feature Domain Completely

Read this section IN FULL and confirm your understanding before
touching any file.

### 1.1 — The Two Categories of Test Types (critical domain concept)

Medical lab tests fall into two structural categories:

1. **Single-component tests** — the test result is one single
   value with one unit of measurement. Example: Creatinine
   (الكرياتينين) — one result, one unit (e.g., mg/dL).

2. **Multi-component (panel) tests** — the test is actually a
   group of several sub-results reported together, where EACH
   sub-component may have its OWN unit of measurement and its
   OWN normal range. Examples:
   - CBC (صورة الدم الكاملة): Hemoglobin, WBC count, RBC count,
     Platelet count, and differential counts of each WBC subtype
     — dozens of sub-components, each with its own unit and
     normal range.
   - Complete Urine Analysis (تحليل البول الكامل): multiple
     sub-components.
   - Complete Stool Analysis (تحليل البراز الكامل): multiple
     sub-components.

The system MUST be able to represent both categories correctly:
a single-component test has one unit + one set of normal ranges
attached directly to the test type; a multi-component test has
a list of named sub-components, each with its own unit and its
own normal ranges.

### 1.2 — Units of Measurement

- A single-component test has exactly one unit of measurement.
- A multi-component test has one unit of measurement PER
  sub-component (different sub-components can have different
  units — e.g., Hemoglobin in g/dL, WBC count in cells/µL).

### 1.3 — Normal (Reference) Ranges Vary by Patient Sex and Age

The SAME test (or the SAME sub-component within a panel test)
can have MULTIPLE valid normal ranges depending on:
  - Patient sex alone, OR
  - Patient age alone, OR
  - A combination of patient sex AND age (e.g., a different
    range for "Male, 18-60 years" vs "Female, 18-60 years" vs
    "Child, 1-12 years").

The system must support attaching multiple conditional reference
ranges to a single test (or single sub-component), each scoped
by sex, age range, or both.

### 1.4 — Scope of This Analysis

This analysis covers TWO related windows/features:
  A. The window(s) used to ADD a new test type and EDIT an
     existing test type's data.
  B. The window(s) used to ADD new normal/reference ranges for
     a test type (or its sub-components) and EDIT existing
     reference ranges.

For BOTH windows, you must reach a complete, field-by-field and
button-by-button understanding: what each field is for, what
data it expects, what each button does, and — critically —
identify and explain any field or button that does NOT currently
respond to user input or does not function as expected, including
the root cause found in the actual code.

---

## Step 2 — Mandatory Discovery Tasks
## (READ ONLY — do NOT modify any file)

Perform a thorough, deep, and precise investigation of the
entire codebase relevant to test types and reference ranges.
Read every relevant file completely. Do not skim. Build
everything in Step 3 from what you actually find — do not
assume or invent.

### 2.1 — Test Type Add/Edit Window

Locate and read completely (file names may differ from these
guesses — find the actual ones):
  - Views/Settings/TestDataManagementWindow.xaml (already known
    from prior work) and any related views (e.g., a separate
    add/edit dialog, a sub-component editor view, a units editor)
  - ViewModels/Settings/TestDetailViewModel.cs (already known)
    and any related ViewModels

For EVERY field and EVERY button in this window, document:
  - Field/button name and label (Arabic + English if shown)
  - What it is bound to (property/command name)
  - What it currently does when used
  - Whether it currently responds to user input correctly
  - If NOT responsive: read the binding, the property, and any
    IsEnabled/IsReadOnly logic to find the exact root cause

Answer explicitly:
  Q1: Does the current model/UI support multi-component tests
      at all? If yes, how (what entity/collection)? If no,
      confirm it is single-component-only today.
  Q2: How is the unit of measurement captured? Is it one field
      per test, or can it vary per sub-component?
  Q3: List every field/button found to be non-responsive, with
      the exact file, line, and root cause for each.
  Q4: How does this window relate to TestTypeSampleTube (the
      tube/barcode feature we built previously)? Confirm nothing
      here conflicts with that prior work.

### 2.2 — Reference Range (Normal Range) Add/Edit Window

Locate and read completely the window(s) and ViewModel(s) used
to add/edit normal ranges. Search the solution for terms like:
  ReferenceRange, NormalRange, RangeMin, RangeMax, AgeFrom,
  AgeTo, Gender, Sex, ReferenceRangeViewModel

For EVERY field and EVERY button found, document the same as
2.1 (name, binding, behavior, responsiveness, root cause if
broken).

Answer explicitly:
  Q1: Does a ReferenceRange (or equivalent) entity already exist
      in the model layer? Show its exact fields.
  Q2: Can a single test (or sub-component) have MULTIPLE
      reference ranges scoped by sex, age, or both? Show the
      exact mechanism if yes, or confirm the gap if no.
  Q3: For multi-component tests, is the reference range attached
      to the parent test type or to each individual sub-component?
  Q4: How are reference ranges currently displayed/edited in the
      UI — list, grid, separate dialog per range?

### 2.3 — Model Layer

Read completely:
  - Models/TestType.cs
  - Any TestComponent.cs / TestSubType.cs / PanelComponent.cs
    (find the actual file name for sub-component representation,
    or confirm none exists)
  - Any ReferenceRange.cs / NormalRange.cs (find actual name,
    or confirm none exists)
  - Models/TestTypeSampleTube.cs (for cross-reference only —
    do not redesign, just confirm no conflict)
  - Data/FinalLabDbContext.cs — read the full configuration for
    all of the above entities (keys, FKs, relationships)

Answer:
  Q1: Draw the exact current entity relationship chain for a
      multi-component test, from TestType down to its individual
      sub-component results and their ranges. If no such chain
      exists, state that clearly.
  Q2: Is there any existing concept of "Unit of Measurement" as
      its own entity/lookup, or is it just a free-text string
      field? Where exactly?

### 2.4 — Service Layer

Read completely:
  - Services/Interfaces/ITestTypeService.cs (or equivalent)
  - Services/Implementations/TestTypeService.cs (or equivalent)
  - Any IReferenceRangeService / ReferenceRangeService

Answer:
  Q1: What CRUD methods exist today for test types?
  Q2: What CRUD methods exist today for reference ranges?
  Q3: Do any of these methods already support multi-component
      structures, or are they single-component-only?

### 2.5 — Existing Test Coverage

Search the solution for any existing unit test project(s)
(e.g., a `*.Tests` project). If found, read the tests that
already cover TestType, test detail ViewModels, or reference
ranges (if any). Answer:
  Q1: Does any unit test project exist in the solution?
  Q2: Is there ANY existing test coverage for the service or
      ViewModel files identified in 2.1, 2.2, and 2.4? If yes,
      list the test files. If no, confirm the gap.

---

## Step 3 — Planning Report

Produce a detailed report with EXACTLY these sections:

### Section A — Current State Summary
  A1 — Test Type Add/Edit window (full field/button audit + Q1-Q4)
  A2 — Reference Range Add/Edit window (full field/button audit + Q1-Q4)
  A3 — Model layer findings (entity chain, unit-of-measure findings)
  A4 — Service layer findings
  A5 — Complete list of every non-responsive field/button found,
       with root cause for each (this is a required, standalone
       list — do not bury it in prose)
  A6 — Existing test coverage findings (from Step 2.5)

### Section B — Gap Analysis
Table format (Gap ID | Description | Affected Layer | Complexity),
covering AT MINIMUM:
  - Multi-component test support (or lack thereof)
  - Per-component unit of measurement support (or lack thereof)
  - Sex/age-conditional reference ranges support (or lack thereof)
  - Every non-responsive field/button from A5
  - Any data model inconsistencies found
  - Lack of unit test coverage for affected service/ViewModel files

### Section C — Proposed Roadmap (Future Phases — NOT for execution now)
A high-level, multi-phase roadmap for how the fixes COULD be
sequenced in future sessions. This is a roadmap sketch only —
do not produce detailed file-by-file implementation steps in
this session. Each future phase should have: goal, rough scope,
complexity estimate, and dependencies on other phases.

For EVERY future phase that creates or modifies a Service or
ViewModel file, explicitly include a paired unit-testing scope:
  - Which existing behaviors must be covered by a regression
    test BEFORE that phase's changes begin (to lock in current
    correct behavior).
  - Which new behaviors introduced by that phase must be covered
    by new unit tests once implemented.
  - This applies specifically to the Service layer and the
    ViewModel layer (and any file in those layers being created
    or modified) — not to XAML views.

### Section D — Open Questions for the User
Every design decision that requires the user's domain expertise
before any fix can be planned in detail. Examples to investigate
(add more as found):
  - Should units of measurement become a proper lookup table
    (reusable across tests) instead of free text?
  - How many sex/age range tiers does the user need supported
    per test (e.g., is "Male/Female x Adult/Child" enough, or
    are finer age brackets needed)?
  - Should sub-components be reorderable/numbered for display
    on printed results?
  - Any other gap-specific decisions found during discovery.

### Section E — Risk Assessment
Table format (Risk | Likelihood | Impact | Mitigation).

---

## Step 4 — Confirm Understanding, THEN Ask Before Writing Anything

### Step 4a — Clarifying Questions (MANDATORY GATE)

Before creating ANY file, review everything you found in Step 2
and everything you are about to write in Section D. If you have
ANY open question, ambiguity, or need for clarification — about
the codebase, about the domain, or about how to phrase a gap or
a roadmap item — STOP HERE and present ALL such questions to the
user in a single consolidated list.

Do NOT create HANDOFF.md or work_plan.md yet if any such question
exists. Wait for the user's answers. Only after the user has
answered every question you raised should you proceed to Step 4b.

If, after full discovery, you have NO open questions beyond what
is already captured in Section D (which is expected to be
answered later by the user as part of normal plan review — not
a blocker to writing the files), state explicitly: "No additional
clarification needed beyond Section D" and proceed directly to
Step 4b.

### Step 4b — Agent Understanding Confirmation

Write a short section titled "Agent Understanding Confirmation"
summarizing in your own words:
  1. The difference between single-component and multi-component
     tests, and why it matters
  2. How units of measurement and reference ranges relate to
     each of the two test categories
  3. Why reference ranges can vary by sex and/or age
  4. What you found in the codebase that is relevant
  5. What is missing and must eventually be built
  6. Confirmation that this session produces NO implementation —
     only analysis and a roadmap sketch

---

## Step 5 — Create the Two Documentation Files

Only after Step 4a is fully resolved (either no questions, or
the user has answered all raised questions), create BOTH files
in this exact folder:
  C:\Users\LAP LINK\source\repos\FinalLabSystem\
  FinalLabSystem\Docs\

### File 1: HANDOFF.md
A context file for any future coding agent — written so it can
be shared/referenced on its own if usage limits are hit before
work_plan.md's consuming session finishes. Must contain:
  - Feature domain summary (the two test categories, units,
    conditional reference ranges)
  - Current state findings (condensed from Section A)
  - The non-responsive fields/buttons list with root causes
  - The gap list (Section B)
  - Load-bearing files for this feature area
  - MVVM constraints that apply (same as prior projects)
  - Existing test coverage summary (from A6)
  - Open questions still pending user answers (if any survived
    Step 4a — there should normally be none left unanswered)

### File 2: work_plan.md
The working document for future planning/implementation
sessions. Must contain:
  - The Agent Understanding Confirmation (Step 4b)
  - Full Section A (current state summary)
  - Full Section B (gap analysis)
  - Full Section C (proposed roadmap sketch, WITH the paired
    unit-testing scope for every Service/ViewModel phase)
  - Full Section D (open questions, with answers incorporated
    if resolved in Step 4a)
  - Full Section E (risk assessment)

---

## Step 6 — STOP

After creating the two files, STOP completely.

Do NOT implement anything.
Do NOT modify any existing source file.
Do NOT run any git command.

Report to the user:
  1. Confirmation that both files were created, with full paths
  2. A clear list of every non-responsive field/button found and
     its root cause
  3. Whether multi-component tests and conditional reference
     ranges are currently supported at all (yes/no/partial)
  4. Whether any unit test coverage currently exists for the
     affected service/ViewModel files
  5. The open questions that need the user's domain-expert answer
     before any fix can be planned in detail

Then wait for the user's review and approval.

---

## Global Constraints (always apply)

1. MVVM — non-negotiable:
   - No MessageBox.Show in ViewModels
   - No async void except framework-required signatures
   - No business logic in code-behind files

2. Protected files — DO NOT TOUCH for any reason:
   - Infrastructure/Security/PasswordHasher.cs
   - Infrastructure/ViewModelBase.cs
   - Infrastructure/Navigation/NavigationService.cs
   - Any file under Migrations/

3. This session = ZERO code changes, ZERO modifications to
   existing files. Only HANDOFF.md and work_plan.md in Docs\
   may be created — and only after Step 4a is fully resolved.

4. No git operations of any kind.

5. All findings must come from ACTUAL file reading. Never assume
   or invent what a file contains.

Begin now with Step 1. Read and confirm your understanding of
the feature domain before opening any file.
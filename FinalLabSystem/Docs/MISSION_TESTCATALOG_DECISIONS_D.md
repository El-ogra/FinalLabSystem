Additional Domain Decisions (D-Series)

This document records the final domain decisions for the remaining open design questions identified during Phase 1.

---

D-1. Units of Measurement

Final Decision

Units of measurement must be managed through a dedicated database lookup table.

Requirements

- The system shall maintain a dedicated Units table.
- Users shall select units from the predefined list whenever possible.
- Users shall be able to create new units when the required unit does not already exist.
- Newly created units shall be saved and become available for future use.
- Free-text unit entry should not be the primary mechanism.

Business Rationale

This is a professional laboratory information system. Centralized unit management improves consistency, prevents duplicate spellings, simplifies reporting, and supports future validation and standardization.

---

D-2. Sex × Age Granularity

Final Decision

The system must support highly granular reference ranges based on both sex and age.

Requirements

- Users must be able to define sex-specific reference ranges.
- Users must be able to define minimum age and maximum age boundaries.
- Age boundaries must remain fully configurable by the user.
- The design must support extremely fine-grained pediatric reference ranges.

Examples

- 1 day to 2 days
- 2 days to 3 days
- 3 days to 7 days
- 7 days to 14 days
- etc.

Business Rationale

Laboratory reference ranges frequently vary by both sex and age, especially in neonates, infants, children, and adolescents. The system must not be limited to broad categories such as Adult, Child, Male, or Female.

---

D-3. TestComponent Ordering

Final Decision

Component ordering is important and must be supported.

Requirements

- Users must be able to control the display order of TestComponents.
- The configured order must be stored persistently.
- Printed reports must respect the configured order.
- Result display screens must respect the configured order.

Business Rationale

The presentation order of component results is clinically and operationally important and should remain under user control.

---

D-5. Component Management Location

Final Decision

TestComponent management belongs in the Test Type definition workflow and not in the Reference Range window.

Requirements

- Create Component
- Edit Component
- Delete Component
- Reorder Component

must all be performed from the Test Type management interface.

The Reference Range window should only manage reference ranges associated with existing components.

Business Rationale

Component definition and reference-range management are separate responsibilities and should remain separated in the user interface.

---

D-6. ReportNameLine2 / BillNameLine2

Final Decision

These fields should be removed.

Requirements

- Remove ReportNameLine2.
- Remove BillNameLine2.
- Do not expose them in the UI.
- Treat them as unnecessary legacy fields.

Business Rationale

These fields are not required by the laboratory workflow and provide no business value.

---

D-8. Tube List Source

Final Decision

Tube types must be managed through a SampleTube master table in the database.

Requirements

- Use SampleTube as the authoritative source of tube definitions.
- Users must be able to create new tube types.
- Users must be able to maintain existing tube definitions.
- Newly added tube types must become available for future test definitions.
- Tube definitions must not remain hardcoded in application code.

Business Rationale

Laboratory tube definitions vary between laboratories and may evolve over time. Database-driven management provides flexibility and maintainability.

---

Intent of These Decisions

These decisions represent final business and domain decisions provided after Phase 1 closure and should be incorporated into future planning and implementation activities where appropriate.
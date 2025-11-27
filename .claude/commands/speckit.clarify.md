# Claude Code Clarification Workflow

This document outlines a **structured ambiguity-detection process** for feature specifications. Its core function is to identify underspecified areas before planning, using a targeted questioning approach (maximum 5 questions per session).

## Key Process Elements

**Prerequisite Check**: Scripts validate the feature environment and return paths to the spec file via JSON.

**Ambiguity Taxonomy**: The workflow scans eight dimensions—functional scope, data models, UX flows, quality attributes, dependencies, edge cases, constraints, and terminology—marking each as Clear, Partial, or Missing.

**Prioritized Questions**: Candidates are ranked by impact and uncertainty. Only questions materially affecting architecture, data design, testing, UX, or compliance advance to the user.

**Interactive Loop**: Questions present one at a time. Multiple-choice options include a **recommended** selection with reasoning. Short-answer prompts suggest best-practice defaults. Users accept ("yes"), select alternatives, or provide custom answers (constrained to ≤5 words for open-ended queries).

**Incremental Spec Updates**: After each accepted answer, the workflow:
- Creates a `## Clarifications` section (if absent) with `### Session YYYY-MM-DD` subheadings
- Appends clarification bullets
- Applies changes to relevant spec sections (Functional Requirements, Data Model, Non-Functional Attributes, etc.)
- Writes updates atomically to disk

**Termination & Reporting**: Stops at 5 questions, user signal, or when critical ambiguities resolve. Final report lists questions asked, touched sections, and a coverage matrix showing each taxonomy category's status (Resolved, Deferred, Clear, or Outstanding).

**Safety Constraints**: Avoids speculative questions; respects early termination; prevents duplicate clarifications; validates markdown structure and terminology consistency.

# Summary

This document is a **procedural guide** for updating a project constitution file (`.specify/memory/constitution.md`) within a structured governance system. It is not a constitution itself, but rather instructions for creating one.

## Key Points

The guide instructs users to:

1. **Load and identify placeholders** in the constitution template (tokens like `[PROJECT_NAME]`, `[PRINCIPLE_1_NAME]`).

2. **Populate values** from user input, repository context, or inference, respecting the specified number of principles.

3. **Version the constitution** using semantic versioning:
   - *MAJOR*: Incompatible governance changes
   - *MINOR*: New principles or expanded guidance
   - *PATCH*: Clarifications and non-semantic refinements

4. **Propagate consistency** by reviewing and updating dependent templates (plan, spec, tasks, and command files).

5. **Generate a Sync Impact Report** documenting version changes, modified principles, affected templates, and deferred items.

6. **Validate** that no unexplained bracketed tokens remain, dates are ISO-formatted, and principles are declarative and testable.

7. **Write the completed constitution** back to the source file and provide a summary with commit message guidance.

The methodology emphasizes maintaining alignment across governance artifacts and using structured semantic versioning to track constitutional evolution.

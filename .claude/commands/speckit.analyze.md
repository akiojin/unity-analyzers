# Claude Code Analysis: /speckit.analyze Command Specification

## Summary

This markdown document defines `/speckit.analyze`, a **read-only cross-artifact consistency validation command** for the Specify framework. It performs structural and semantic analysis on three core project files (`spec.md`, `plan.md`, `tasks.md`) after task generation completes.

## Key Functional Elements

**Execution Flow:**
1. Validate prerequisites and file existence via bash script
2. Load artifacts progressively (minimal context per file)
3. Build semantic models (requirements inventory, task mapping, constitution rules)
4. Execute six detection passes (duplication, ambiguity, underspecification, constitution alignment, coverage gaps, inconsistency)
5. Assign severity ratings (CRITICAL → LOW)
6. Output structured Markdown report with findings table
7. Provide remediation recommendations

**Critical Operating Constraint:**
The specification emphasizes "STRICTLY READ-ONLY" execution—no file modifications permitted during analysis. Constitution violations automatically escalate to CRITICAL severity and require explicit adjustment outside this command.

## Deliverable Format

Findings are reported in a markdown table with columns: ID, Category, Severity, Location(s), Summary, and Recommendation. Supporting tables include coverage mapping and unmapped task identification. Metrics tracked encompass total requirements, tasks, coverage percentage, and issue counts.

## Scope Boundaries

- **In scope**: Inconsistencies within spec/plan/tasks, constitution alignment, coverage gaps
- **Out of scope**: Implementation logic, file editing, automatic remediation
- **Token efficiency**: Maximum 50 findings in main table; aggregate overflow separately

This command serves as a quality gate before implementation, surfacing design conflicts early without altering source artifacts.

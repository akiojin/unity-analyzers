# Claude Code: Checklist Generation Guide

This documentation outlines how to generate **requirement quality checklists**—unit tests for specifications, not implementation verification.

## Core Principle

Checklists validate that *requirements are well-written*, complete, unambiguous, and ready for implementation. They test the spec itself, not whether code matches it.

**Invalid approach**: "Verify the button clicks correctly"
**Valid approach**: "Are interactive element requirements consistently defined?"

## Key Execution Flow

1. **Prerequisites**: Run bash script to extract feature directory and available docs
2. **Clarify Intent**: Generate up to 3 contextual questions derived from user input and spec signals (scope, risk, depth, audience, boundaries, scenarios)
3. **Load Context**: Read spec.md, plan.md, and tasks.md progressively as needed
4. **Generate Checklist**: Create files in `FEATURE_DIR/checklists/` with domain-specific names (e.g., `ux.md`, `api.md`)
5. **Structure Items**: Use format `- [ ] CHK### <requirement quality question> [Dimension, References]`

## Checklist Item Requirements

**Must test requirement quality**:
- Completeness (all necessary requirements present?)
- Clarity (unambiguous and specific?)
- Consistency (alignment across sections?)
- Measurability (objectively verifiable?)
- Coverage (all scenarios/edge cases?)

**Must include traceability**: ≥80% of items reference spec sections or use markers like `[Gap]`, `[Ambiguity]`, `[Conflict]`

**Prohibited patterns**: "Verify," "Test," "Confirm" + implementation behavior; code execution references; "displays correctly," "works properly"

**Required patterns**: "Are [requirement] defined/specified...?"; "Is [vague term] quantified...?"; "Can [requirement] be objectively measured?"

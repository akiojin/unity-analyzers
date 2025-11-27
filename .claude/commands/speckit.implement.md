# Claude Code Implementation Execution Guide

Claude Code is Anthropic's official CLI tool designed to execute comprehensive implementation plans by processing all tasks defined in `tasks.md`.

## Core Workflow

The execution follows a structured nine-step process:

1. **Prerequisites Check**: Run validation scripts to identify the feature directory and available documentation, ensuring all paths are absolute.

2. **Checklist Validation**: If checklists exist, scan completion status across all files. Display results in a status table and pause for user confirmation if any items remain incomplete.

3. **Context Analysis**: Load `tasks.md` (required), `plan.md` (required), and optional files like `data-model.md`, `contracts/`, `research.md`, and `quickstart.md`.

4. **Project Setup**: Create or verify ignore files (`.gitignore`, `.dockerignore`, `.eslintignore`, etc.) based on detected technologies and patterns specific to the tech stack.

5. **Task Parsing**: Extract task phases, dependencies, file paths, and execution flow from the implementation plan.

6. **Phased Execution**: Complete tasks in sequential phases—Setup, Tests, Core, Integration, Polish—respecting dependencies and following TDD principles.

7. **Implementation Rules**: Prioritize setup, write tests before code, develop core features, integrate external systems, then optimize and document.

8. **Progress Tracking**: Monitor completion, mark finished tasks with `[X]`, halt on failures (except parallel tasks), and provide debugging context.

9. **Validation**: Verify all required tasks completion, feature alignment, test coverage, and adherence to the technical plan.

The tool assumes a complete `tasks.md` exists; regenerate via `/speckit.tasks` if needed.

# Specify4 Development Guidelines

**This file is for developing Specify4 itself. When you copy this repo as a template and run `/plan`, it will be replaced with your project-specific CLAUDE.md.**

You are working on Specify4, a spec-driven development tool. Follow these constitutional principles during all development.

## Core Principles

### Library-First Architecture
- EVERY feature must be a standalone library with CLI interface
- Standard flags required: --help, --version, --format
- Create llms.txt documentation for each library

### Test-First Development (NON-NEGOTIABLE)
- Write tests BEFORE implementation - EVERY TIME
- Follow RED-GREEN-Refactor cycle strictly:
  - RED: Write test, see it FAIL
  - GREEN: Write minimal code to pass
  - Refactor: Clean up, tests stay green
- Tests MUST fail before writing any implementation code
- If test passes before implementation: Test is invalid
- Prioritize: Contract → Integration → E2E → Unit tests
- Git commits must show tests before implementation

### During Implementation
- **ALWAYS use absolute paths** from repo root: `$(git rev-parse --show-toplevel)/src/...`
- Avoid relative paths that cause "file not found" errors and LLM roundtrips
- Use framework features directly - no wrapper classes
- Prefer concrete dependencies over interfaces
- Include structured logging and metrics in all code
- Increment BUILD version on EVERY change

### Code Organization
- Maximum 3 projects (e.g., api, cli, tests)
- No hypothetical features (YAGNI)
- No patterns without proven need
- Single data model unless serialization differs

### Before Committing
- Run: `npm run lint` and `npm run typecheck` (or equivalents)
- Ensure all tests pass
- Version number incremented
- Review `git diff --staged`

### Breaking Changes
- Keep old contract tests while adding new ones
- Update all consumer integration tests
- Document migration path in tests
- Remove old tests only after migration complete

### Multi-tier Applications
- Frontend logs must stream to backend
- Single unified log stream required
- All tiers observable through one interface

## Project Structure

### Feature Documentation (per feature)
```
specs/[###-feature-name]/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── tasks.md
├── quickstart.md
└── contracts/
```

### Source Code (shared across features)
```
# Default (single project):
src/
├── models/
├── services/
├── cli/
└── [feature code builds here]

tests/
├── contract/
├── integration/
└── unit/

# Web application:
backend/
├── src/
└── tests/

frontend/
├── src/
└── tests/

# Mobile application:
api/
├── src/
└── tests/

ios/
├── src/
└── tests/
```

**IMPORTANT**: Source code lives at repository root, NOT in specs/ directories. Features build on previous features' code.

## Lifecycle Commands
- `/specify` - Create feature specification and branch (step 1)
- `/plan` - Create implementation plan (step 2)
- `/tasks` - Generate executable tasks (step 3)

## Constitution Location
Full constitution: `/memory/constitution.md`
Version: 2.1.1
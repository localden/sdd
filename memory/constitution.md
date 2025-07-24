# Specify3 Constitution (Brief)

## Core Principles

### I. Library-First
- Every feature starts as a standalone library
- Libraries must be self-contained, independently testable, documented (llms.txt format for AI agents)
- Clear purpose required - no organizational-only libraries
- Start with single library per feature (avoid premature splitting)

### II. CLI Interface
- Every library exposes functionality via CLI
- Text in/out protocol: stdin/args → stdout, errors → stderr
- Support JSON + human-readable formats
- Standard flags: --help, --version, --format

### III. Test-First (NON-NEGOTIABLE)
- TDD mandatory: Tests written → User approved → Tests fail → Then implement
- Red-Green-Refactor cycle strictly enforced
- Complete test suite approved before ANY implementation

### IV. Integration Testing
Focus areas requiring integration tests:
- New library: Contract tests for its CLI/API interface
- Contract changes: Tests for both producer and consumers
- Inter-service communication: Full interaction flow tests
- Shared schemas: Consumer compatibility tests

Integration test guidelines:
- Prioritize over unit tests for library boundaries
- Use real dependencies (databases, services) not mocks
- Must pass before merging any changes

### V. Observability
- Text I/O ensures debuggability
- Structured logging, metrics, error reporting required
- Single source of truth: frontend logs must stream to backend for unified observability
- All tiers (web, mobile, backend) must consolidate logs into one observable stream

### VI. Versioning & Breaking Changes
Every change requires version increment:
- MAJOR.MINOR.BUILD format (e.g., 2.1.47)
- Increment BUILD for any change (ensures log traceability)
- Increment MINOR for new features
- Increment MAJOR for breaking changes

Breaking changes require:
- Keep old contract tests, add new ones (both must pass)
- Update all consumer integration tests to use new API
- All e2e tests must pass with new implementation
- Tests serve as migration guide for LLMs
- Remove old contract tests once all consumers migrated

### VII. Simplicity
- Start simple, add complexity only when proven necessary
- YAGNI: No hypothetical features
- Max 3 initial projects
- Complex patterns prohibited without proven need
- Use framework features directly (no wrappers)
- Depend on concrete implementations, not interfaces
- Add abstractions only when 2+ implementations exist

## Governance
- Constitution supersedes all other practices
- Amendments require documentation, approval, migration plan
- All PRs/reviews must verify compliance
- Complexity must be justified
- Use CLAUDE.md for runtime development guidance

**Version**: 2.1.1 | **Ratified**: 2025-06-13 | **Last Amended**: 2025-07-16
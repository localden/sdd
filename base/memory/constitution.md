# Specify3 Constitution

## Preamble

This constitution establishes the immutable core principles that govern the development and evolution of Specify2. These principles are designed to ensure consistency, maintainability, and clarity throughout the project's lifecycle. All contributors, features, and architectural decisions must adhere to these foundational rules.

## Article I: The Library-First Principle

**Section 1.1: Feature Genesis**
Every feature in Specify2 MUST begin its existence as a standalone library. No feature shall be implemented directly within application code without first being abstracted into a reusable library component.

**Section 1.2: Library Independence**
Each library MUST be:
- Self-contained with minimal external dependencies
- Capable of independent testing and validation
- Documented with clear API specifications
- Versioned independently when appropriate

**Section 1.3: Separation of Concerns**
Libraries MUST separate business logic from presentation, infrastructure, and application-specific concerns.

**Section 1.4: Library Justification**
Each library MUST have a clear, distinct purpose that justifies its existence:
- Libraries shall not be created for organizational purposes alone
- A library must have potential for reuse or provide significant isolation benefits
- Monolithic implementations are preferred until modularization benefits are proven

## Article II: The CLI Interface Mandate

**Section 2.1: Universal CLI Access**
Every library MUST provide a command-line interface that exposes its core functionality. This CLI interface serves as the primary contract for observability and testability.

**Section 2.2: Text Input/Output Protocol**
All CLI interfaces MUST:
- Accept text as input (via stdin, arguments, or files)
- Produce text as output (via stdout)
- Use stderr exclusively for error reporting and logging
- Support JSON format for structured data exchange
- Provide human-readable output formats where applicable

**Section 2.3: CLI Standardization**
All CLI tools MUST implement standard patterns:
- `--help` flag for usage information
- `--version` flag for version reporting
- `--format` flag for output format selection (json, text, etc.)
- Consistent exit codes (0 for success, non-zero for errors)

## Article III: The Test-First Imperative

**Section 3.1: Absolute Test-First Development**
This is NON-NEGOTIABLE: All implementation MUST follow strict Test-Driven Development (TDD). No implementation code shall be written before:
1. Unit tests are written
2. Tests are validated and approved by the user
3. Tests are confirmed to FAIL (Red phase)

Only after tests fail may implementation begin.

**Section 3.2: Red-Green-Refactor Cycle**
Every feature implementation MUST follow this exact sequence:
1. **RED**: Write failing tests that define expected behavior
2. **GREEN**: Write minimal implementation code to make tests pass
3. **REFACTOR**: Improve code quality while maintaining passing tests
4. **REPEAT**: Continue cycle for each new behavior

**Section 3.3: Test-First Validation**
Before ANY implementation code is written:
- Complete test suite MUST be presented to user for approval
- Tests MUST comprehensively define all expected behaviors
- Test execution MUST demonstrate failure (proving tests are meaningful)
- User MUST explicitly approve test suite before implementation proceeds

**Section 3.4: Implementation Plan Requirements**
Every implementation plan MUST include:
- Complete test specifications BEFORE implementation steps
- Test-first development timeline showing red-green-refactor cycles
- Shared test utilities strategy and cross-library pattern identification
- Detailed test scenarios and expected outcomes
- Test data requirements and setup procedures
- Performance benchmarks where applicable
- Edge case handling specifications

**Section 3.5: Test Comprehensiveness Principle**
Tests MUST be comprehensive and meaningful, covering all intended behaviors and edge cases. The definition of "comprehensive" shall be determined by project standards, but the principle of thorough testing before implementation remains inviolate.

**Section 3.6: Integration Test Priority**
Test development MUST prioritize integration and contract tests:
- API contract tests before endpoint implementation
- Integration tests at service boundaries before unit tests
- End-to-end tests for complete user workflows
- Unit tests for complex business logic and algorithms

## Article IV: Implementation Governance

**Section 4.1: Constitutional Supremacy**
These principles supersede all other development practices, style guides, and architectural decisions. When conflicts arise, constitutional principles take precedence.

**Section 4.2: Amendment Process**
Modifications to this constitution require:
- Explicit documentation of the rationale for change
- Review and approval by project maintainers
- Backwards compatibility assessment
- Migration plan for existing code

**Section 4.3: Enforcement**
- All pull requests MUST demonstrate adherence to constitutional principles
- CI/CD pipelines MUST validate constitutional compliance
- Code reviews MUST verify constitutional conformance

**Section 4.4: Complexity Review**
All implementations MUST undergo complexity review:
- Number of projects/libraries must be justified
- Each abstraction layer must demonstrate value
- Patterns must solve actual, not theoretical problems
- Simpler alternatives must be documented and rejected with cause

## Article V: Observability and Transparency

**Section 5.1: The Visibility Principle**
The text-based input/output requirement exists to ensure that all system behavior is observable, debuggable, and understandable by human operators.

**Section 5.2: Logging and Monitoring**
All libraries MUST provide:
- Structured logging capabilities
- Performance metrics collection
- Error reporting and debugging information
- Usage analytics (when appropriate)

## Article VI: Evolution and Adaptation

**Section 6.1: Continuous Improvement**
While these principles are immutable in spirit, their implementation may evolve to incorporate new best practices, technologies, and learnings.

**Section 6.2: Principle Application**
These principles apply to:
- All new features and libraries
- Major refactoring of existing code
- Third-party integrations and dependencies
- Documentation and specification artifacts

## Article VII: The Simplicity Imperative

**Section 7.1: Start Simple, Evolve Gradually**
All implementations MUST begin with the simplest possible architecture that could work. Complexity may only be added when:
- Performance metrics demonstrate a need
- User feedback requires additional capabilities
- Scale requirements are proven through actual usage

**Section 7.2: YAGNI (You Aren't Gonna Need It)**
Features, abstractions, and architectural patterns MUST NOT be added based on hypothetical future needs. Every architectural decision must be justified by current, concrete requirements.

**Section 7.3: Minimal Project Structure**
New implementations MUST start with the minimum viable number of projects:
- Maximum 3 projects for initial implementation (API, UI, Tests)
- Additional projects require documented justification based on proven needs
- Consolidation is preferred over separation

**Section 7.4: Pattern Justification**
Complex patterns (Repository, Unit of Work, CQRS, etc.) are PROHIBITED unless:
- The problem cannot be solved with simpler approaches
- Performance benchmarks justify the complexity
- The pattern provides measurable benefits over framework features

## Article VIII: The Anti-Abstraction Principle

**Section 8.1: Framework Trust**
Implementations MUST use framework features directly rather than wrapping them in custom abstractions unless:
- The framework genuinely lacks required functionality
- Multiple framework implementations must be supported
- Proven portability requirements exist

**Section 8.2: Single Model Principle**
Data MUST be represented using the minimum number of models:
- Use framework models directly when possible
- Add transfer objects only when serialization requirements differ significantly
- Prohibit parallel model hierarchies without proven need

**Section 8.3: Direct Dependencies**
Libraries and services SHOULD depend on concrete implementations rather than interfaces unless:
- Multiple implementations actually exist
- Testing requires mocking (and even then, prefer integration tests)
- The abstraction provides clear, documented benefits

## Article IX: The Integration-First Testing Mandate

**Section 9.1: Integration Over Unit Tests**
While unit tests remain important, integration tests MUST be prioritized:
- API contract tests are mandatory before implementation
- End-to-end user workflow tests required for all features
- Integration tests at component boundaries take precedence

**Section 9.2: Realistic Test Environments**
Tests MUST use realistic environments:
- Prefer real databases over mocks for data layer tests
- Use actual service instances over stubs where feasible
- In-memory databases acceptable for fast feedback loops

**Section 9.3: Contract-First Development**
All API boundaries MUST:
- Define contracts before implementation using industry-standard formats
- Generate client/server code from contracts where possible
- Include contract tests that verify implementation matches specification

---

*This constitution serves as the foundational law of Specify2. It ensures that our specification-driven development platform remains maintainable, testable, and transparent as it evolves.*

**Ratified**: 2025-06-13  
**Amended**: 2025-06-20 (Test-Architecture-First Principle)  
**Amended**: 2025-06-22 (Test-First Imperative - TDD Non-Negotiable)  
**Amended**: 2025-06-22 (Simplicity Imperative, Anti-Abstraction Principle, Integration-First Testing)  
**Version**: 2.0.0
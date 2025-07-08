# Implementation Plan: [FEATURE NAME]

**Feature Branch**: `[###-feature-name]`  
**Created**: [DATE]  
**Specification**: Link to feature specification document  

---

## ⚡ Quick Guidelines

**Note**: This document serves two purposes:
1. **As a template** - For AIs/humans creating implementation plans
2. **As a guide** - For AIs/humans executing the implementation

Instructions marked *(for execution)* apply when implementing the feature.
Instructions marked *(when creating this plan)* apply when filling out this template.

- ✅ Mark all technical decisions that need clarification
- ✅ Use [NEEDS CLARIFICATION: question] for any assumptions
- ❌ Don't guess at technical choices without context
- ❌ Don't include actual code - use pseudocode or references
- 📋 The review checklist acts as "unit tests" for this plan
- 📁 Extract details to `implementation-details/` files

### For AI Generation *(when creating this plan document)*
When generating this plan from a feature spec:
1. **NO CODE IN THIS DOCUMENT**: 
   - Use pseudocode or high-level descriptions only
   - Extract any detailed code fragments to `implementation-details/` files
   - Reference the detail files instead of embedding code
2. **Mark missing context**: Use [NEEDS CLARIFICATION] for:
   - Unspecified technical stack choices
   - Missing performance requirements
   - Unclear integration points
   - Ambiguous testing requirements
3. **Common areas needing clarification**:
   - Language/framework versions
   - Database technology choices
   - Authentication/authorization approach
   - Deployment environment
   - Third-party service integrations
4. **Don't assume**: If the spec doesn't specify it, mark it
5. **Use implementation-details/ folder**:
   - Research results → `implementation-details/00-research.md`
   - API design details → `implementation-details/03-api-contracts.md`
   - Complex algorithms → `implementation-details/04-algorithms.md`
   - Integration specifics → `implementation-details/05-integrations.md`

### Implementation Details File Examples

**00-research.md**: Technical investigations, spike results, performance benchmarks
**02-data-model.md**: Entity definitions, relationships, validation rules, state machines
**03-api-contracts.md**: OpenAPI specs, request/response schemas, error codes
**06-contract-tests.md**: Test scenarios for each endpoint, edge cases, error conditions
**08-inter-library-tests.md**: Library interaction contracts, boundary tests

---

## Executive Summary *(mandatory)*

[2-3 sentence overview of what this feature does and its core technical approach]

## Requirements *(mandatory)*

**Minimum Versions**: [Language/Runtime constraints or NEEDS CLARIFICATION]  
**Dependencies**: [Core framework only, others must be justified]  
**Technology Stack**: [Database, caching, messaging, etc. or NEEDS CLARIFICATION]  
**Feature Spec Alignment**: [ ] All requirements addressed

---

## Constitutional Compliance *(mandatory)*

*Note: The Constitution articles referenced below can be found in `/memory/constitution.md`. AI agents should read this file to understand the specific requirements of each article.*

### Simplicity Declaration (Articles VII & VIII)
- **Project Count**: [number] (maximum 3)
- **Model Strategy**: [ ] Single model / [ ] Multiple (justify: [reason])
- **Framework Usage**: [ ] Direct / [ ] Abstracted (justify: [reason])
- **Patterns Used**: [ ] None / [ ] List: [patterns] (justify each)

### Testing Strategy (Articles III & IX)
- **Test Order**: Contract → Integration → E2E → Unit
- **Contract Location**: `/contracts/`
- **Real Environments**: [ ] Yes / [ ] Mocks (justify: ___)
- **Coverage Target**: 80% minimum, 100% critical paths

### Library Organization (Articles I & II)
- **Libraries**: [List each with purpose]
- **CLI Interfaces**: [List commands per library]
- **CLI Standards**: All CLIs implement --help, --version, --format
- **Inter-Library Contracts**: [Define how libraries interact]

### Observability (Article V)
- [ ] Structured logging planned
- [ ] Error reporting defined
- [ ] Metrics collection (if applicable)

---

## Project Structure *(mandatory)*

```
[feature-name]/
├── implementation-plan.md          # This document (HIGH-LEVEL ONLY)
├── manual-testing.md              # Step-by-step validation instructions
├── implementation-details/         # Detailed specifications
│   ├── 00-research.md             # Research findings
│   ├── 01-environment-setup.md    # Setup instructions
│   ├── 02-data-model.md           # Detailed schemas
│   ├── 03-api-contracts.md        # API specifications
│   ├── 04-algorithms.md           # Complex logic details
│   ├── 05-integrations.md         # External system details
│   ├── 06-contract-tests.md       # Test specifications
│   ├── 07-integration-tests.md    # Test scenarios
│   └── 08-inter-library-tests.md  # Library boundary tests
├── contracts/                      # API contracts (FIRST)
├── src/
│   └── [main application/library]
└── tests/
    ├── contract/                   # Contract tests (FIRST)
    ├── integration/                # Integration tests
    ├── inter-library/              # Cross-library tests
    └── unit/                       # Unit tests (LAST)
```

### File Creation Order
1. Create directory structure
2. Create `implementation-details/00-research.md` (if research needed)
3. Create `contracts/` with API specifications
4. Create `implementation-details/03-api-contracts.md`
5. Create test files in order: contract → integration → e2e → unit
6. Create source files to make tests pass
7. Create `manual-testing.md` for E2E validation

**IMPORTANT**: This implementation plan should remain high-level and readable. Any code samples, detailed algorithms, or extensive technical specifications must be placed in the appropriate `implementation-details/` file and referenced here.

---

## Implementation Phases *(mandatory)*

### Phase -1: Pre-Implementation Gates

#### Technical Unknowns
- [ ] Complex areas identified: [list areas or NEEDS CLARIFICATION]
- [ ] Research completed (if needed)
*Research findings: implementation-details/00-research.md*

#### Simplicity Gate (Article VII)
- [ ] Using ≤3 projects?
- [ ] No future-proofing?
- [ ] No unnecessary patterns?

#### Anti-Abstraction Gate (Article VIII)
- [ ] Using framework directly?
- [ ] Single model representation?
- [ ] Concrete classes by default?

#### Integration-First Gate (Article IX)
- [ ] Contracts defined?
- [ ] Contract tests written?
- [ ] Integration plan ready?

**📝 During plan creation**: Document any gates that would fail in Complexity Tracking section
**⚠️ During implementation**: STOP if any gate fails without documented justification

#### Gate Failure Handling
**When creating this plan:**
- If Simplicity Gate would fail → Add to Complexity Tracking with justification
- If Research needed → Note in Technical Unknowns as [NEEDS CLARIFICATION]
- If Integration unclear → Mark as [NEEDS CLARIFICATION] and plan research

**When executing this plan:**
- If gate fails → Check if documented in Complexity Tracking
- If not documented → STOP and escalate to human
- If research reveals blockers → Update plan before proceeding

### Verification: Phase -1 Complete *(execution checkpoint)*
- [ ] All gates passed or exceptions documented in Complexity Tracking
- [ ] Research findings documented if applicable
- [ ] Ready to create directory structure

### Phase 0: Contract & Test Setup

**Prerequisites** *(for execution)*: Phase -1 verification complete
**Deliverables** *(from execution)*: Failing contract tests, API specifications, test strategy

1. **Define API Contracts**
   ```pseudocode
   Create contracts/api specification
   Define all endpoints, requests, responses
   ```
   *Details: implementation-details/03-api-contracts.md*
   
   Example with clarifications needed:
   - POST /users [NEEDS CLARIFICATION: auth method - JWT, session, API key?]
   - GET /reports [NEEDS CLARIFICATION: pagination approach?]

2. **Write Contract Tests**
   ```pseudocode
   Create failing tests that verify API matches contracts
   These must fail (no implementation yet)
   ```
   *Detailed test scenarios: implementation-details/06-contract-tests.md*

3. **Design Integration Tests**
   ```pseudocode
   Plan user workflow tests
   Plan service boundary tests
   Plan inter-library integration tests (if multiple libraries)
   ```
   *Test strategy details: implementation-details/07-integration-tests.md*
   *Inter-library tests: implementation-details/08-inter-library-tests.md*

4. **Create Manual Testing Guide**
   - Map each user story to validation steps
   - Document setup/build/run instructions
   - Create step-by-step validation procedures
   *Output: manual-testing.md*
   
   Example structure:
   ```markdown
   # Manual Testing Guide
   ## Setup
   1. Build: [commands]
   2. Configure: [steps]
   3. Run: [commands]
   
   ## User Story Validation
   ### US-001: [Story name]
   1. Start application
   2. Navigate to [location]
   3. Perform [action]
   4. Verify [expected result]
   ```

### Verification: Phase 0 Complete *(execution checkpoint)*
- [ ] API contracts exist in `/contracts/`
- [ ] Contract tests written and failing
- [ ] Integration test plan documented
- [ ] Manual testing guide created
- [ ] All detail files referenced exist

### Phase 1: Core Implementation

**Prerequisites** *(for execution)*: Phase 0 verification complete
**Deliverables** *(from execution)*: Working implementation passing all contract tests

1. **Single Model Design**
   - Define data model (use framework features)
   - No separate DTOs unless serialization differs
   *Detailed schema and relationships: implementation-details/02-data-model.md*

2. **API Implementation**
   - Implement endpoints to pass contract tests
   - Use framework features directly
   - Add CLI interface per Article II

3. **Integration Implementation**
   - Connect components
   - Verify integration tests pass
   - Add E2E workflow tests

### Phase 2: Refinement

**Prerequisites** *(for execution)*: Phase 1 complete, all contract/integration tests passing
**Deliverables** *(from execution)*: Production-ready code with full test coverage

1. **Unit Tests** (only for complex logic)
2. **Performance Optimization** (only if metrics show need)
3. **Documentation Updates**
4. **Manual Testing Execution**
   - Follow manual-testing.md procedures
   - Verify all user stories work E2E
   - Document any issues found

### Verification: Phase 2 Complete *(execution checkpoint)*
- [ ] All tests passing (contract, integration, unit)
- [ ] Manual testing completed successfully
- [ ] Performance metrics meet requirements
- [ ] Documentation updated

---

## Complexity Tracking *(optional - only if deviations exist)*

Document any exceptions to Constitutional principles here. This section should only exist if Phase -1 gates failed with justification.

### Deviations from Constitutional Defaults

| Item | Why Needed | Simpler Option Rejected |
|------|------------|------------------------|
| [4th project] | [Current need] | [What was tried] |
| [Abstraction] | [Framework gap] | [Direct usage] |
| [Pattern] | [Problem it solves] | [Simple approach] |

### Dependency Justifications

| Package | Problem | Why Framework Insufficient |
|---------|---------|---------------------------|
| [Name] | [Issue] | [Gap] |

---

## Success Criteria *(mandatory)*

1. **Constitutional**: All gates passed or justified
2. **Functional**: Feature requirements met
3. **Testing**: Contract/Integration tests comprehensive
4. **Performance**: Meets specified targets
5. **Simplicity**: No unjustified complexity

---


## Review & Acceptance Checklist

### Plan Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] All mandatory sections completed
- [ ] Technology stack fully specified
- [ ] Dependencies justified or none added

### Constitutional Alignment
- [ ] All Phase -1 gates passed or exceptions documented
- [ ] Deviations recorded in Complexity Tracking section

### Technical Readiness
- [ ] Phase 0 verification complete
- [ ] Phase 1 implementation path clear
- [ ] Success criteria measurable

### Risk Management
- [ ] Complex areas identified and researched
- [ ] Integration points clearly defined
- [ ] Performance requirements specified
- [ ] Security considerations addressed

### Implementation Clarity
- [ ] All phases have clear prerequisites and deliverables
- [ ] No speculative or "might need" features
- [ ] Manual testing procedures defined

---

*This plan follows Constitution v2.0.0 (see `/memory/constitution.md`) emphasizing simplicity, framework trust, and integration-first testing.*
Create an implementation plan for an existing feature specification.

Given the implementation details "$ARGUMENTS", I need you to:

1. Find the repository root using `git rev-parse --show-toplevel`
2. Get the current git branch name using `git rev-parse --abbrev-ref HEAD`
3. Locate the feature specification file at `specs/[current-branch]/feature-spec.md`
4. Read and analyze the feature specification to understand:
   - The feature requirements and user stories
   - Functional and non-functional requirements
   - Success criteria and acceptance criteria
   - Any technical constraints or dependencies mentioned
5. Read the constitution at `/memory/constitution.md` to understand constitutional requirements
6. Copy the implementation plan template from `templates/implementation-plan-template.md` to `specs/[current-branch]/implementation-plan.md`
7. Create the implementation-details directory: `specs/[current-branch]/implementation-details/`
8. Create a comprehensive implementation plan by:
   - Following the template structure and sections precisely
   - Using [NEEDS CLARIFICATION: question] markers for any technical choices not specified in the feature spec
   - Translating business requirements from the feature spec into technical architecture
   - Defining specific libraries needed based on constitutional principles
   - Specifying technical decisions, dependencies, and integration points
   - Using pseudocode or high-level descriptions (NO actual code in the plan)
   - Extracting any detailed specifications to implementation-details/ files:
     * 00-research.md for technical investigations
     * 02-data-model.md for entity definitions and schemas
     * 03-api-contracts.md for detailed API specifications
     * 06-contract-tests.md for test scenarios
     * 08-inter-library-tests.md for library interaction tests
   - Incorporating the user-provided implementation details from $ARGUMENTS
   - Ensuring full constitutional compliance (Library-first, CLI-enabled, Test-architecture-first)
   - Completing all Phase -1 gate checks (Simplicity, Anti-Abstraction, Integration-First)
   - Documenting any necessary deviations in the Complexity Tracking section
9. Create manual-testing.md with step-by-step validation procedures for each user story
10. Verify the implementation plan:
    - Addresses all feature spec requirements
    - Has no remaining [NEEDS CLARIFICATION] markers unless truly unknown
    - References all created implementation-details files
    - Passes the Review & Acceptance Checklist
11. Confirm creation with branch name, file paths, and list of created files

Use absolute paths with the repository root for all file operations to avoid path issues.
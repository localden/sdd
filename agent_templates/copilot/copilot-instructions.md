# GitHub Copilot Instructions for Specification-Driven Development

This file contains the SDD workflow commands adapted for GitHub Copilot. Use these instructions with GitHub Copilot Chat or Copilot CLI.

## Usage Note

In GitHub Copilot, you can reference these commands in your prompts:
- "Follow the @specify workflow to add user authentication"
- "Use the @plan process with JWT tokens and bcrypt"
- "Execute the @tasks breakdown for backend API endpoints"

---

## @specify

Start a new feature by creating a specification and feature branch.

This is the first step in the Spec-Driven Development lifecycle.

Given the feature description provided as an argument, I need you to:

1. Run the feature creation script and capture output:
   ```bash
   REPO_ROOT=$(git rev-parse --show-toplevel)
   OUTPUT=$($REPO_ROOT/scripts/create-new-feature.sh "$ARGUMENTS")
   BRANCH_NAME=$(echo "$OUTPUT" | grep "BRANCH_NAME:" | cut -d' ' -f2)
   SPEC_FILE=$(echo "$OUTPUT" | grep "SPEC_FILE:" | cut -d' ' -f2)
   ```

2. Use the template as a format guide to write a feature specification:
   - Follow the template structure and sections
   - Replace placeholders with appropriate content based on the feature description
   - Use the template as a guide for writing a comprehensive feature spec

3. Confirm creation with branch name and file path

Use absolute paths with the repository root for all file operations to avoid path issues.

---

## @plan

Plan how to implement the specified feature.

This is the second step in the Spec-Driven Development lifecycle.

Given the implementation details provided as an argument, I need you to:

1. Setup implementation plan structure and get paths:
   ```bash
   REPO_ROOT=$(git rev-parse --show-toplevel)
   PATHS=$($REPO_ROOT/scripts/setup-plan.sh)
   FEATURE_SPEC=$(echo "$PATHS" | grep "FEATURE_SPEC:" | cut -d' ' -f2)
   IMPL_PLAN=$(echo "$PATHS" | grep "IMPL_PLAN:" | cut -d' ' -f2)
   SPECS_DIR=$(echo "$PATHS" | grep "SPECS_DIR:" | cut -d' ' -f2)
   ```

2. Read and analyze the feature specification to understand:
   - The feature requirements and user stories
   - Functional and non-functional requirements
   - Success criteria and acceptance criteria
   - Any technical constraints or dependencies mentioned

3. Read the constitution at `/memory/constitution.md` to understand constitutional requirements

4. Execute the implementation plan template:
   - Load `/templates/implementation-plan-template.md` (already copied to $IMPL_PLAN)
   - Set Input path to $FEATURE_SPEC
   - Run the Execution Flow (main) function steps 1-10
   - The template is self-contained and executable
   - Follow error handling and gate checks as specified
   - Let the template guide artifact generation in $SPECS_DIR:
     * Phase 0 generates research.md
     * Phase 1 generates data-model.md, contracts/, quickstart.md
     * Phase 2 generates tasks.md
   - Incorporate user-provided details from arguments into Technical Context
   - Update Progress Tracking as you complete each phase

5. Verify execution completed:
   - Check Progress Tracking shows all phases complete
   - Ensure all required artifacts were generated
   - Confirm no ERROR states in execution

6. Report results with branch name, file paths, and generated artifacts

Use absolute paths with the repository root for all file operations to avoid path issues.

---

## @tasks

Break down the plan into executable tasks.

This is the third step in the Spec-Driven Development lifecycle.

Given the context provided as an argument, I need you to:

1. Check prerequisites and find available documents:
   ```bash
   REPO_ROOT=$(git rev-parse --show-toplevel)
   RESULT=$($REPO_ROOT/scripts/check-task-prerequisites.sh)
   
   if [[ $? -ne 0 ]]; then
     echo "$RESULT"
     exit 1
   fi
   
   FEATURE_DIR=$(echo "$RESULT" | grep "FEATURE_DIR:" | cut -d':' -f2)
   echo "Generating tasks for: $FEATURE_DIR"
   echo "$RESULT" | grep -A20 "AVAILABLE_DOCS:"
   ```

2. Load and analyze available design documents:
   - Always read plan.md for tech stack and libraries
   - IF EXISTS: Read data-model.md for entities
   - IF EXISTS: Read contracts/ for API endpoints  
   - IF EXISTS: Read research.md for technical decisions
   - IF EXISTS: Read quickstart.md for test scenarios
   
   Note: Not all projects have all documents. For example:
   - CLI tools might not have contracts/
   - Simple libraries might not need data-model.md
   - Generate tasks based on what's available

3. Generate tasks following the template:
   - Use `/templates/tasks-template.md` as the base
   - Replace example tasks with actual tasks based on:
     * **Setup tasks**: Project init, dependencies, linting
     * **Test tasks [P]**: One per contract, one per integration scenario
     * **Core tasks**: One per entity, service, CLI command, endpoint
     * **Integration tasks**: DB connections, middleware, logging
     * **Polish tasks [P]**: Unit tests, performance, docs

4. Task generation rules:
   - Each contract file → contract test task marked [P]
   - Each entity in data-model → model creation task marked [P]
   - Each endpoint → implementation task (not parallel if shared files)
   - Each user story → integration test marked [P]
   - Different files = can be parallel [P]
   - Same file = sequential (no [P])

5. Order tasks by dependencies:
   - Setup before everything
   - Tests before implementation (TDD)
   - Models before services
   - Services before endpoints
   - Core before integration
   - Everything before polish

6. Include parallel execution examples:
   - Group [P] tasks that can run together
   - Show actual Task agent commands

7. Create $FEATURE_DIR/tasks.md with:
   - Correct feature name from implementation plan
   - Numbered tasks (T001, T002, etc.)
   - Clear file paths for each task
   - Dependency notes
   - Parallel execution guidance

The tasks.md should be immediately executable - each task should be specific enough that an LLM can complete it without additional context.

---

## Quick Reference

| Claude Code | GitHub Copilot | Description |
|-------------|----------------|-------------|
| `/specify "feature desc"` | "Follow @specify workflow for [feature desc]" | Create feature specification |
| `/plan "implementation details"` | "Use @plan process with [implementation details]" | Generate implementation plan |
| `/tasks "context"` | "Execute @tasks breakdown for [context]" | Break down into tasks |

## Example Usage

```
# Start a new feature
"Follow the @specify workflow to add user authentication with JWT tokens"

# Plan the implementation  
"Use the @plan process with Express.js, bcrypt for password hashing, and jsonwebtoken library"

# Generate tasks
"Execute the @tasks breakdown focusing on API endpoints and database integration"
```

## Copilot Chat Integration

You can also use these workflows directly in Copilot Chat:

```
@workspace Follow the SDD @specify workflow to create a specification for user authentication
```

```
@workspace Use the SDD @plan process to create an implementation plan using JWT and Express.js
```

```
@workspace Execute the SDD @tasks breakdown to generate actionable tasks for the current feature
```

## Notes

- All commands expect to be run from within a Git repository
- Commands use shell scripts in the `/scripts/` directory
- Templates are located in the `/templates/` directory
- Constitutional memory is stored in `/memory/constitution.md`
- File paths are automatically resolved from the repository root
- Use `@workspace` in Copilot Chat to ensure full repository context

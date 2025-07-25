Start a new feature by creating a specification and feature branch.

This is the first step in the Spec-Driven Development lifecycle.

Given the feature description "$ARGUMENTS", I need you to:

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
Create a new feature specification with automatic branch creation and numbering.

Given the feature description "$ARGUMENTS", I need you to:

1. Find the repository root using `git rev-parse --show-toplevel`
2. Examine the `specs/` directory to find the highest numbered feature (e.g., if you see `001-chat` and `002-test-cleanup`, the highest is 002)
3. Generate the next feature number by incrementing by 1 with zero-padding (e.g., 002 → 003)
4. Transform the feature description into a branch name by:
   - Converting to lowercase
   - Replacing spaces and special characters with hyphens
   - Limiting to 2-3 meaningful words
   - Combining with feature number: `003-meaningful-name`
5. Create and switch to the new git branch
6. Create the feature directory: `specs/[branch-name]/`
7. Copy the template from `templates/feature-spec-template.md` to `specs/[branch-name]/feature-spec.md`
8. Use the template as a format guide to write a feature specification:
   - Follow the template structure and sections
   - Replace placeholders with appropriate content based on the feature description
   - Use the template as a guide for writing a comprehensive feature spec
9. Confirm creation with branch name and file path

Use absolute paths with the repository root for all file operations to avoid path issues.
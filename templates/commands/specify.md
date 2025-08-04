---
name: specify
description: "Start a new feature by creating a specification and feature branch. This is the first step in the Spec-Driven Development lifecycle."
---

Start a new feature by creating a specification and feature branch.

This is the first step in the Spec-Driven Development lifecycle.

Given the feature description provided as an argument, I need you to:

1. **First, run the feature creation script** to create the branch and spec file:
   ```bash
   REPO_ROOT=$(git rev-parse --show-toplevel)
   OUTPUT=$($REPO_ROOT/scripts/create-new-feature.sh "{ARGS}" 2>&1)
   
   # Verify the script succeeded by checking for required outputs
   if [[ $OUTPUT == *"BRANCH_NAME:"* && $OUTPUT == *"SPEC_FILE:"* ]]; then
       echo "✅ Feature creation successful!"
       BRANCH_NAME=$(echo "$OUTPUT" | grep "BRANCH_NAME:" | cut -d' ' -f2)
       SPEC_FILE=$(echo "$OUTPUT" | grep "SPEC_FILE:" | cut -d' ' -f2)
       echo "Branch: $BRANCH_NAME"
       echo "Spec file: $SPEC_FILE"
   else
       echo "❌ Feature creation failed. Output:"
       echo "$OUTPUT"
       exit 1
   fi
   ```

2. **Then, read the spec template** to understand the structure:
   ```bash
   cat $REPO_ROOT/templates/spec-template.md
   ```

3. **Finally, write the feature specification** using the template structure:
   - Follow the template structure and sections exactly
   - Replace placeholders with appropriate content based on the feature description
   - Write directly to the spec file created in step 1

4. **Confirm creation** with branch name and file path:
   ```bash
   echo "🎉 SPECIFICATION COMPLETE! 🎉"
   echo "✅ Branch created: $BRANCH_NAME"
   echo "✅ Specification file: $SPEC_FILE"
   echo "✅ Ready for development phase"
   ```

Use absolute paths with the repository root for all file operations to avoid path issues.

**Note**: This command creates a new Git branch and specification file.

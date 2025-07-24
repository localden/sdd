#!/bin/bash
# Setup a git worktree for parallel Claude Code development
# Usage: ./worktree-setup.sh feature-name

set -e

FEATURE_NAME="$1"
if [ -z "$FEATURE_NAME" ]; then
    echo "Usage: $0 <feature-name>"
    echo "Example: $0 003-user-auth"
    exit 1
fi

REPO_ROOT=$(git rev-parse --show-toplevel)
WORKTREE_DIR="../$FEATURE_NAME-worktree"

# Create worktree
echo "Creating worktree at $WORKTREE_DIR..."
git worktree add "$WORKTREE_DIR" -b "$FEATURE_NAME"

# Copy project CLAUDE.md if it exists
if [ -f "$REPO_ROOT/CLAUDE.md" ]; then
    echo "Copying project CLAUDE.md to worktree..."
    cp "$REPO_ROOT/CLAUDE.md" "$WORKTREE_DIR/"
fi

# Create feature-specific context
cat > "$WORKTREE_DIR/CLAUDE-FEATURE.md" << EOF
# Feature Context: $FEATURE_NAME

This worktree is dedicated to implementing feature $FEATURE_NAME.

## Active Feature
- Branch: $FEATURE_NAME
- Spec: specs/$FEATURE_NAME/spec.md
- Plan: specs/$FEATURE_NAME/plan.md
- Tasks: specs/$FEATURE_NAME/tasks.md

## Worktree Notes
- This is an isolated git worktree
- Changes here don't affect other worktrees
- Run Claude Code from this directory for this feature
- Main project CLAUDE.md has been copied here

## Quick Commands
\`\`\`bash
# Return to this worktree
cd $WORKTREE_DIR

# Check worktree status
git worktree list

# Remove when done
git worktree remove $WORKTREE_DIR
\`\`\`
EOF

echo "✅ Worktree created at: $WORKTREE_DIR"
echo "📝 Feature context at: $WORKTREE_DIR/CLAUDE-FEATURE.md"
echo ""
echo "Next steps:"
echo "1. cd $WORKTREE_DIR"
echo "2. claude"
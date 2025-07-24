#!/bin/bash
# Setup implementation plan structure for current branch
# Returns paths needed for implementation plan generation

set -e

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

# Get all paths
eval $(get_feature_paths)

# Check if on feature branch
check_feature_branch "$CURRENT_BRANCH" || exit 1

# Create specs directory if it doesn't exist
mkdir -p "$FEATURE_DIR"

# Copy plan template if it exists
TEMPLATE="$REPO_ROOT/templates/plan-template.md"
if [ -f "$TEMPLATE" ]; then
    cp "$TEMPLATE" "$IMPL_PLAN"
fi

# Output all paths for LLM use
echo "FEATURE_SPEC: $FEATURE_SPEC"
echo "IMPL_PLAN: $IMPL_PLAN"
echo "SPECS_DIR: $FEATURE_DIR"
echo "BRANCH: $CURRENT_BRANCH"
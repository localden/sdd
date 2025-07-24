#!/bin/bash
# Check that implementation plan exists and find optional design documents

set -e

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

# Get all paths
eval $(get_feature_paths)

# Check if on feature branch
check_feature_branch "$CURRENT_BRANCH" || exit 1

# Check if feature directory exists
if [[ ! -d "$FEATURE_DIR" ]]; then
    echo "ERROR: Feature directory not found: $FEATURE_DIR"
    echo "Run /specify first to create the feature structure."
    exit 1
fi

# Check for implementation plan (required)
if [[ ! -f "$IMPL_PLAN" ]]; then
    echo "ERROR: plan.md not found in $FEATURE_DIR"
    echo "Run /plan first to create the plan."
    exit 1
fi

# List available design documents (optional)
echo "FEATURE_DIR:$FEATURE_DIR"
echo "AVAILABLE_DOCS:"

# Use common check functions
check_file "$RESEARCH" "research.md"
check_file "$DATA_MODEL" "data-model.md"
check_dir "$CONTRACTS_DIR" "contracts/"
check_file "$QUICKSTART" "quickstart.md"

# Always succeed - task generation should work with whatever docs are available
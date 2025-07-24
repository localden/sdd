#!/bin/bash
# Incrementally update CLAUDE.md based on new feature plan
# O(1) operation - only reads current CLAUDE.md and new plan.md
#
# Algorithm:
# 1. Current CLAUDE.md represents cumulative state up to feature N
# 2. New plan.md contains tech for feature N+1
# 3. Diff and merge only the NEW additions
# 4. No need to re-read all previous plans (O(N) avoided)

set -e

REPO_ROOT=$(git rev-parse --show-toplevel)
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
FEATURE_DIR="$REPO_ROOT/specs/$CURRENT_BRANCH"
NEW_PLAN="$FEATURE_DIR/plan.md"
CLAUDE_MD="$REPO_ROOT/CLAUDE.md"

if [ ! -f "$NEW_PLAN" ]; then
    echo "ERROR: No plan.md found at $NEW_PLAN"
    exit 1
fi

echo "=== Updating CLAUDE.md for feature $CURRENT_BRANCH ==="

# Extract tech from new plan
NEW_LANG=$(grep "^**Language/Version**: " "$NEW_PLAN" 2>/dev/null | head -1 | sed 's/^**Language\/Version**: //' | grep -v "NEEDS CLARIFICATION" || echo "")
NEW_FRAMEWORK=$(grep "^**Primary Dependencies**: " "$NEW_PLAN" 2>/dev/null | head -1 | sed 's/^**Primary Dependencies**: //' | grep -v "NEEDS CLARIFICATION" || echo "")
NEW_TESTING=$(grep "^**Testing**: " "$NEW_PLAN" 2>/dev/null | head -1 | sed 's/^**Testing**: //' | grep -v "NEEDS CLARIFICATION" || echo "")
NEW_DB=$(grep "^**Storage**: " "$NEW_PLAN" 2>/dev/null | head -1 | sed 's/^**Storage**: //' | grep -v "N/A" | grep -v "NEEDS CLARIFICATION" || echo "")
NEW_PROJECT_TYPE=$(grep "^**Project Type**: " "$NEW_PLAN" 2>/dev/null | head -1 | sed 's/^**Project Type**: //' || echo "")

# Create temp file for new CLAUDE.md
TEMP_FILE=$(mktemp)

# If CLAUDE.md doesn't exist, create from template
if [ ! -f "$CLAUDE_MD" ]; then
    echo "Creating new CLAUDE.md for your project..."
    
    # Check if this is the Specify4 repo itself
    if [ -f "$REPO_ROOT/.claude/commands/specify.md" ] && [ -f "$REPO_ROOT/templates/CLAUDE-template.md" ]; then
        echo "Note: Replacing Specify4's CLAUDE.md with project-specific version"
    fi
    
    cp "$REPO_ROOT/templates/CLAUDE-template.md" "$TEMP_FILE"
    
    # Replace placeholders
    sed -i.bak "s/\[PROJECT NAME\]/$(basename $REPO_ROOT)/" "$TEMP_FILE"
    sed -i.bak "s/\[DATE\]/$(date +%Y-%m-%d)/" "$TEMP_FILE"
    sed -i.bak "s/\[EXTRACTED FROM ALL PLAN.MD FILES\]/- $NEW_LANG + $NEW_FRAMEWORK ($CURRENT_BRANCH)/" "$TEMP_FILE"
    
    # Add project structure based on type
    if [[ "$NEW_PROJECT_TYPE" == *"web"* ]]; then
        sed -i.bak "s|\[ACTUAL STRUCTURE FROM PLANS\]|backend/\nfrontend/\ntests/|" "$TEMP_FILE"
    else
        sed -i.bak "s|\[ACTUAL STRUCTURE FROM PLANS\]|src/\ntests/|" "$TEMP_FILE"
    fi
    
    # Add minimal commands
    if [[ "$NEW_LANG" == *"Python"* ]]; then
        COMMANDS="cd src && pytest && ruff check ."
    elif [[ "$NEW_LANG" == *"Rust"* ]]; then
        COMMANDS="cargo test && cargo clippy"
    elif [[ "$NEW_LANG" == *"JavaScript"* ]] || [[ "$NEW_LANG" == *"TypeScript"* ]]; then
        COMMANDS="npm test && npm run lint"
    else
        COMMANDS="# Add commands for $NEW_LANG"
    fi
    sed -i.bak "s|\[ONLY COMMANDS FOR ACTIVE TECHNOLOGIES\]|$COMMANDS|" "$TEMP_FILE"
    
    # Add code style
    sed -i.bak "s|\[LANGUAGE-SPECIFIC, ONLY FOR LANGUAGES IN USE\]|$NEW_LANG: Follow standard conventions|" "$TEMP_FILE"
    
    # Add recent changes
    sed -i.bak "s|\[LAST 3 FEATURES AND WHAT THEY ADDED\]|- $CURRENT_BRANCH: Added $NEW_LANG + $NEW_FRAMEWORK|" "$TEMP_FILE"
    
    rm "$TEMP_FILE.bak"
else
    echo "Updating existing CLAUDE.md..."
    
    # Extract manual additions
    MANUAL_START=$(grep -n "<!-- MANUAL ADDITIONS START -->" "$CLAUDE_MD" | cut -d: -f1)
    MANUAL_END=$(grep -n "<!-- MANUAL ADDITIONS END -->" "$CLAUDE_MD" | cut -d: -f1)
    
    if [ ! -z "$MANUAL_START" ] && [ ! -z "$MANUAL_END" ]; then
        sed -n "${MANUAL_START},${MANUAL_END}p" "$CLAUDE_MD" > /tmp/manual_additions.txt
    fi
    
    # Parse existing CLAUDE.md and create updated version
    python3 - << EOF
import re
import sys
from datetime import datetime

# Read existing CLAUDE.md
with open("$CLAUDE_MD", 'r') as f:
    content = f.read()

# Check if new tech already exists
tech_section = re.search(r'## Active Technologies\n(.*?)\n\n', content, re.DOTALL)
if tech_section:
    existing_tech = tech_section.group(1)
    
    # Add new tech if not already present
    new_additions = []
    if "$NEW_LANG" and "$NEW_LANG" not in existing_tech:
        new_additions.append(f"- $NEW_LANG + $NEW_FRAMEWORK ($CURRENT_BRANCH)")
    if "$NEW_DB" and "$NEW_DB" not in existing_tech and "$NEW_DB" != "N/A":
        new_additions.append(f"- $NEW_DB ($CURRENT_BRANCH)")
    
    if new_additions:
        updated_tech = existing_tech + "\n" + "\n".join(new_additions)
        content = content.replace(tech_section.group(0), f"## Active Technologies\n{updated_tech}\n\n")

# Update project structure if needed
if "$NEW_PROJECT_TYPE" == "web" and "frontend/" not in content:
    struct_section = re.search(r'## Project Structure\n```\n(.*?)\n```', content, re.DOTALL)
    if struct_section:
        updated_struct = struct_section.group(1) + "\nfrontend/src/      # Web UI"
        content = re.sub(r'(## Project Structure\n```\n).*?(\n```)', 
                        f'\\1{updated_struct}\\2', content, flags=re.DOTALL)

# Add new commands if language is new
if "$NEW_LANG" and f"# {NEW_LANG}" not in content:
    commands_section = re.search(r'## Commands\n```bash\n(.*?)\n```', content, re.DOTALL)
    if commands_section:
        new_commands = commands_section.group(1)
        if "Python" in "$NEW_LANG":
            new_commands += "\n\n# Python\ncd src && pytest && ruff check ."
        elif "Rust" in "$NEW_LANG":
            new_commands += "\n\n# Rust\ncargo test && cargo clippy"
        elif "JavaScript" in "$NEW_LANG" or "TypeScript" in "$NEW_LANG":
            new_commands += "\n\n# JavaScript/TypeScript\nnpm test && npm run lint"
        
        content = re.sub(r'(## Commands\n```bash\n).*?(\n```)', 
                        f'\\1{new_commands}\\2', content, flags=re.DOTALL)

# Update recent changes (keep only last 3)
changes_section = re.search(r'## Recent Changes\n(.*?)(\n\n|$)', content, re.DOTALL)
if changes_section:
    changes = changes_section.group(1).strip().split('\n')
    changes.insert(0, f"- $CURRENT_BRANCH: Added $NEW_LANG + $NEW_FRAMEWORK")
    # Keep only last 3
    changes = changes[:3]
    content = re.sub(r'(## Recent Changes\n).*?(\n\n|$)', 
                    f'\\1{chr(10).join(changes)}\\2', content, flags=re.DOTALL)

# Update date
content = re.sub(r'Last updated: \d{4}-\d{2}-\d{2}', 
                f'Last updated: {datetime.now().strftime("%Y-%m-%d")}', content)

# Write to temp file
with open("$TEMP_FILE", 'w') as f:
    f.write(content)
EOF

    # Restore manual additions if they exist
    if [ -f /tmp/manual_additions.txt ]; then
        # Remove old manual section from temp file
        sed -i.bak '/<!-- MANUAL ADDITIONS START -->/,/<!-- MANUAL ADDITIONS END -->/d' "$TEMP_FILE"
        # Append manual additions
        cat /tmp/manual_additions.txt >> "$TEMP_FILE"
        rm /tmp/manual_additions.txt "$TEMP_FILE.bak"
    fi
fi

# Move temp file to final location
mv "$TEMP_FILE" "$CLAUDE_MD"

echo "✅ CLAUDE.md updated successfully"
echo ""
echo "Summary of changes:"
if [ ! -z "$NEW_LANG" ]; then
    echo "- Added language: $NEW_LANG"
fi
if [ ! -z "$NEW_FRAMEWORK" ]; then
    echo "- Added framework: $NEW_FRAMEWORK"
fi
if [ ! -z "$NEW_DB" ] && [ "$NEW_DB" != "N/A" ]; then
    echo "- Added database: $NEW_DB"
fi
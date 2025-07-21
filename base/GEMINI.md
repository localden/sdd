# Project Gryphon Instructions

## Writing Style

When writing markdown documents:
- Use short sentences (under 20 words)
- Use simple words (avoid "utilize", "leverage", "synergize")
- Use active voice
- No corporate jargon

## Frequently Used Commands

```bash
# Generate Word document (provide filename without extension)
./gen.sh gryphon-plan

# Run tests (TBD - will add when test framework is set up)
# npm test

# Lint code (TBD - will add when linting is configured)
# npm run lint
```

## Auto-Generation Workflow

**MANDATORY**: When editing or creating any .md file in the root directory, follow this exact checklist:

### Checklist for .md File Changes
- [ ] 1. Edit the .md file
- [ ] 2. Generate the corresponding .docx file: `./gen.sh <filename-without-extension>`
- [ ] 3. Commit both the .md and .docx files together
- [ ] 4. Always commit the .md changes even if docx generation fails

### Scope and Rules
- **Root directory only**: Only .md files in the root directory require docx generation
- **Always commit .md**: Commit markdown changes regardless of docx generation success
- **Generation timing**: Generate docx BEFORE committing, then commit both files together

### Example Workflow
```bash
# After editing gryphon-plan.md:
./gen.sh gryphon-plan                    # Generate docx/gryphon-plan.docx
git add gryphon-plan.md docx/gryphon-plan.docx
git commit -m "Update gryphon plan and generate docx"
```

## Code Conventions

- Clear variable names that explain purpose
- Functions do one thing
- Add tests for new features
- Comment complex logic

## Project Structure

```
/
├── gryphon-plan.md  # Main project plan (renamed from plan.md)
├── overview.md      # Original notes
├── gen.sh           # Markdown to Word converter (now accepts filename parameter)
├── template.docx    # Word formatting template
└── docx/            # Generated Word documents
    └── *.docx       # Auto-generated .docx files
```

## Important Notes

- Focus on VS Code Coding Agent first
- GitHub Copilot Agent comes later
- Learning from M365/MAI teams' existing processes
- First milestone: 2-3 week manual evaluation
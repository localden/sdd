# Specification-Driven Development (SDD) Repository

This repository contains the foundational documentation and tools for adopting and implementing Spec-Driven Development (SDD) - a methodology that inverts the traditional relationship between specifications and code by making specifications the primary artifact that generates implementation(s).

## 🎯 Overview

SDD represents a fundamental shift in software development where:

- **Specifications drive code** (not the other way around)
- **PRDs become executable** (generating implementation directly)
- **Documentation and code stay synchronized** (through continuous regeneration)
- **Changes propagate systematically** (from specification to implementation)

## 📁 Repository Structure

```text
/
├── README.md                    # This file - how to use the repository
├── CLAUDE.md                    # Claude Code specific instructions
├── sdd.md                       # Core SDD methodology document
├── specify-proposal.md          # Proposal for the Specify tool
├── gen.sh                       # Markdown to Word converter
└── base/                        # Core templates and constitution
    ├── memory/
    │   └── constitution.md      # Immutable development principles
    └── templates/
        ├── feature-spec-template.md      # Template for feature specifications
        └── implementation-plan-template.md  # Template for implementation plans
```

>[!NOTE]
>The `gen.sh` script references `template.docx` and outputs to a `docx/` directory, but these don't currently exist in the repository.

## 🚀 Quick Start for Claude Code Users

### 1. Understanding the Methodology

Start by reading the core documents in order:

1. **`sdd.md`** - The complete SDD methodology.
2. **`specify-proposal.md`** - Vision for tooling to support SDD.
3. **`base/memory/constitution.md`** - Core development principles that **must be followed by Claude Code**.

### 2. Using the Templates

The `base/templates/` directory contains structured templates for:

- **Feature Specifications** - Business requirements and user stories
- **Implementation Plans** - Technical architecture and development approach

### 3. Document Generation Workflow

This repository includes an automated workflow for generating Word documents:

```bash
# Generate Word document from any markdown file
./gen.sh filename-without-extension

# Example: Generate Word doc from sdd.md
./gen.sh sdd
```

**Important**: When editing root-level `.md` files, always:

1. Edit the markdown file
2. Run `./gen.sh <filename>` to generate the Word document
3. Commit both files together

## 🤖 Claude Code Integration

### Key Claude Code Patterns

When working with this repository through Claude Code:

#### Creating Feature Specifications

```text
Use the feature-spec-template.md as your starting point.
Focus on WHAT users need and WHY, not HOW to implement.
Mark all ambiguities with [NEEDS CLARIFICATION: specific question].
```

#### Creating Implementation Plans

```text
Use the implementation-plan-template.md as your guide.
Mark technical decisions that need clarification.
Extract detailed code to implementation-details/ files.
Use pseudocode in the main plan document.
```

#### Document Generation

```text
After editing any root-level .md file:
1. Run: ./gen.sh <filename-without-extension>
2. Commit both .md and .docx files together
```

## 📋 Workflow Examples

### Example 1: Creating a New Feature

```bash
# 1. Copy the template
cp base/templates/feature-spec-template.md new-feature-spec.md

# 2. Edit the specification (focus on WHAT and WHY)
# Edit new-feature-spec.md

# 3. Generate Word document
./gen.sh new-feature-spec

# 4. Create implementation plan
cp base/templates/implementation-plan-template.md new-feature-implementation.md

# 5. Edit implementation plan (focus on HOW)
# Edit new-feature-implementation.md

# 6. Generate Word document
./gen.sh new-feature-implementation

# 7. Commit all files
git add new-feature-spec.md new-feature-implementation.md docx/new-feature-spec.docx docx/new-feature-implementation.docx
git commit -m "Add new feature specification and implementation plan"
```

### Example 2: Updating Core Documentation

```bash
# 1. Edit the core document
# Edit sdd.md

# 2. Generate updated Word document
./gen.sh sdd

# 3. Commit both files
git add sdd.md docx/sdd.docx
git commit -m "Update SDD methodology documentation"
```

## 🎨 Writing Style Guidelines

When creating or editing documents in this repository:

- **Use short sentences** (under 20 words)
- **Use simple words** (avoid "utilize", "leverage", "synergize")
- **Use active voice**
- **No corporate jargon**
- **Be specific and concrete**
- **Focus on clarity over cleverness**

## 🔧 Prerequisites

To use this repository effectively:

1. **Pandoc** - Required for document generation

   ```bash
   # Install pandoc (varies by OS)
   # Windows: Use chocolatey or download from pandoc.org
   # Mac: brew install pandoc
   # Linux: apt-get install pandoc
   ```

2. **Bash Shell** - Required for running `gen.sh`
   - Windows: Use Git Bash, WSL, or PowerShell with bash
   - Mac/Linux: Built-in

3. **Word Template** - `template.docx` (currently not in repository - needed for `gen.sh` to work)
   - The `gen.sh` script expects a `template.docx` file for Word formatting
   - Will also create a `docx/` directory for output files when first run

## 🧠 Core Principles (From Constitution)

1. **Library-First Principle**: Every feature must begin as a standalone library
2. **CLI Interface Mandate**: Universal CLI access for all functionality
3. **Specification-First Development**: Code serves specifications, not vice versa
4. **Continuous Refinement**: Specifications evolve through feedback loops
5. **Research-Driven Context**: Decisions backed by investigation and analysis

## 📚 Additional Resources

- **Core Methodology**: Read `sdd.md` for the complete SDD approach
- **Tool Vision**: Read `specify-proposal.md` for the future of SDD tooling
- **Constitution**: Read `base/memory/constitution.md` for immutable principles
- **Templates**: Use files in `base/templates/` as starting points

## 🤝 Contributing

When contributing to this repository:

1. **Read the constitution** (`base/memory/constitution.md`) first
2. **Follow the writing style** guidelines above
3. **Use the templates** provided in `base/templates/`
4. **Generate Word documents** for all root-level markdown changes
5. **Commit both markdown and Word files** together

## ❓ Questions?

For questions about:

- **SDD Methodology**: Review `sdd.md` and `specify-proposal.md`
- **Development Process**: Check `base/memory/constitution.md`
- **Document Templates**: Examine files in `base/templates/`
- **Claude Code Usage**: Reference `CLAUDE.md` for AI-specific guidelines

---

*This repository represents a living implementation of Specification-Driven Development. The methodology, tools, and processes will evolve based on real-world usage and feedback.*

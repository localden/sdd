# Specification-Driven Development (SDD) Repository

>[!NOTE]
>This is heavily based on [**work done by John Lam**](https://github.com/jflam/sdd).

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

## 🤖 Claude Code Integration

### Key Claude Code Patterns

You can use this repository as a template to get started with Claude Code and SDD.

1. Clone this repository.
1. Launch Claude Code from inside the cloned folder (context should be in the root, where `CLAUDE.md` is located).

## 🔧 Prerequisites

To use this repository effectively:

- **Linux** (can be used via [WSL2](https://learn.microsoft.com/windows/wsl/install)) or **macOS** operating system
- [Claude Code](https://www.anthropic.com/claude-code)

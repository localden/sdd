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
├── sdd.md                       # Core SDD methodology document
├── specify-proposal.md          # Proposal for the Specify tool
├── gen.sh                       # Markdown to Word converter
└── base/                        # Core templates and constitution
    ├── CLAUDE.md                # Claude Code specific instructions
    ├── memory/
    │   └── constitution.md      # Immutable development principles
    └── templates/
        ├── feature-spec-template.md         # Template for feature specifications
        └── implementation-plan-template.md  # Template for implementation plans
```

## 🚀 Quick Start for Claude Code Users

### 1. Understanding the Methodology

Start by reading the core documents to familiarize yourself with the principles:

1. **`sdd.md`** - The complete SDD methodology.
2. **`specify-proposal.md`** - Vision for tooling to support SDD.
3. **`base/memory/constitution.md`** - Core development principles that **must be followed by Claude Code**.

### 2. Using the Templates

The `base/templates/` directory contains structured templates for:

- **Feature Specifications** - Business requirements and user stories
- **Implementation Plans** - Technical architecture and development approach

Similarly, the `base/.claude` directory contains Claude Code-specific `/` (slash) commands - `new_feature` and `generate_plan`, that can be used
to bootstrap a new feature or create an execution plan for Claude Code to follow based on a feature definition.

## 🤖 Claude Code Integration

### Key Claude Code Patterns

You can use this repository as a template to get started with Claude Code and SDD.

1. Clone this repository.
1. Launch Claude Code from inside the `base` folder (where `CLAUDE.md` and `.claude` are located).
1. Start iterating on the capabilities of your project.

#### New features

The first step should be creating a new feature. Use `/new_feature` and then specify the concrete requirements for the project you want to develop.

![GIF showing Claude Code execution in an Ubuntu terminal](media/claude-code-starter.gif)

## 🔧 Prerequisites

To use this repository effectively:

- **Linux** (can be used via [WSL2](https://learn.microsoft.com/windows/wsl/install)) or **macOS** operating system
- [Claude Code](https://www.anthropic.com/claude-code)

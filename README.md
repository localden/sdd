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

## 🔧 Prerequisites

To use this repository effectively:

- **Linux** (can be used via [WSL2](https://learn.microsoft.com/windows/wsl/install)) or **macOS** operating system
- [Claude Code](https://www.anthropic.com/claude-code)

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

You can use this repository as a template to get started with Claude Code and SDD.

1. Clone this repository.
1. Launch Claude Code from inside the `base` folder (where `CLAUDE.md` and `.claude` are located).
1. Start iterating on the capabilities of your project.

### STEP 1: Bootstrap the project

The first step should be creating a new project scaffolding. Use `/new_feature` and then specify the concrete requirements for the project you want to develop.

![GIF showing Claude Code execution in an Ubuntu terminal](media/claude-code-starter.gif)

>[!IMPORTANT]
>Be as explicit as possible about what you are trying to build.

An example prompt, as described by John Lam, can be:

```text
Develop Taskify, a team productivity platform. It should allow users to create projects, add team members,
assign tasks, comment and move tasks between boards in Kanban style. In this initial phase for this feature,
let's call it "Create Taskify," let's have multiple users but the users will be declared ahead of time, predefined.
I want five users in two different categories, one product manager and four engineers. Let's create three
different sample projects. Let's have the standard Kanban columns for the status of each task, such as "To Do,"
"In Progress," "In Review," and "Done." There will be no login for this application as this is just the very
first testing thing to ensure that our basic features are set up. For each task in the UI for a task card,
you should be able to change the current status of the task between the different columns in the Kanban work board.
You should be able to leave an unlimited number of comments for a particular card. You should be able to, from that task
card, assign one of the valid users. When you first launch Taskify, it's going to give you a list of the five users to pick
from. There will be no password required. When you click on a user, you go into the main view, which displays the list of
projects. When you click on a project, you open the Kanban board for that project. You're going to see the columns.
You'll be able to drag and drop cards back and forth between different columns. You will see any cards that are
assigned to you, the currently logged in user, in a different color from all the other ones, so you can quickly
see yours. You can edit any comments that you make, but you can't edit comments that other people made. You can
delete any comments that you made, but you can't delete comments anybody else made.
```

After this prompt is entered, you should see Claude Code kick off the planning and spec drafting process.

![Claude Code performing the planning and spec writing steps](media/new-feature-process.png)

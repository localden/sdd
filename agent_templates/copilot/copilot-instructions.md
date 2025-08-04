# GitHub Copilot Instructions for Specification-Driven Development

This file contains the SDD workflow commands adapted for GitHub Copilot. Use these instructions with GitHub Copilot Chat or Copilot CLI.

## Usage Note

In GitHub Copilot, you can reference these commands in your prompts:
- "Follow the @specify workflow to add user authentication"
- "Use the @plan process with JWT tokens and bcrypt"
- "Execute the @tasks breakdown for backend API endpoints"

## Quick Reference

| Claude Code | GitHub Copilot | Description |
|-------------|----------------|-------------|
| `/specify "feature desc"` | "Follow @specify workflow for [feature desc]" | Create feature specification |
| `/plan "implementation details"` | "Use @plan process with [implementation details]" | Generate implementation plan |
| `/tasks "context"` | "Execute @tasks breakdown for [context]" | Break down into tasks |

## Example Usage

```
# Start a new feature
"Follow the @specify workflow to add user authentication with JWT tokens"

# Plan the implementation  
"Use the @plan process with Express.js, bcrypt for password hashing, and jsonwebtoken library"

# Generate tasks
"Execute the @tasks breakdown focusing on API endpoints and database integration"
```

## Copilot Chat Integration

You can also use these workflows directly in Copilot Chat:

```
@workspace Follow the SDD @specify workflow to create a specification for user authentication
```

```
@workspace Use the SDD @plan process to create an implementation plan using JWT and Express.js
```

```
@workspace Execute the SDD @tasks breakdown to generate actionable tasks for the current feature
```

## Notes

- All commands expect to be run from within a Git repository
- Commands use shell scripts in the `/scripts/` directory
- Templates are located in the `/templates/` directory
- Constitutional memory is stored in `/memory/constitution.md`
- Detailed command instructions are in `.github/prompts/` folder
- File paths are automatically resolved from the repository root
- Use `@workspace` in Copilot Chat to ensure full repository context

# Implementation Plan: ADO Toolkit MVP Roadmap

**Branch**: `001-mvp-roadmap` | **Date**: 2026-04-07 | **Spec**: [/Users/john/Source/repos/xelseor/ado-toolkit/specs/001-mvp-roadmap/spec.md](/Users/john/Source/repos/xelseor/ado-toolkit/specs/001-mvp-roadmap/spec.md)
**Input**: Feature specification from `/specs/001-mvp-roadmap/spec.md`

## Summary

Build the MVP as a single .NET 10 console application that unifies the legacy
`workitems` and `Commentor` capabilities behind one config model, one command
surface, and one menu system. The implementation should front-load setup and
storage foundations, then land work item retrieval, then pull request
retrieval plus persistent thread-state workflows, then parity and polish work
needed to satisfy the constitution.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: Spectre.Console for terminal UX, Azure DevOps REST integrations, System.Text.Json for config and artifact serialization  
**Storage**: Local file-based storage with a unified config index plus referenced artifact files  
**Testing**: .NET test project with contract, integration, and unit coverage  
**Target Platform**: macOS-first local developer CLI with supportable cross-platform packaging  
**Project Type**: Single-project CLI application  
**Performance Goals**: Typical setup and list operations should feel immediate; retrieval commands should stream clear progress and complete within reasonable interactive CLI expectations  
**Constraints**: Menu and direct command parity is mandatory; AI-facing JSON contracts must be explicit and testable; PR thread review state must persist safely across separate commands; secrets must not leak in logs or rendered outputs  
**Scale/Scope**: Single-user local developer tool consolidating the current work item and pull request apps into one MVP

## Constitution Check

*GATE: Must pass before implementation planning proceeds. Re-check after design.*

- Pass: MVP is organized around stable command contracts and artifact schemas.
- Pass: The plan preserves direct-command equivalents for menu workflows.
- Pass: Work item retrieval scope includes first-order relationships and comments.
- Pass: Setup and local storage are treated as core MVP capabilities, not afterthoughts.
- Pass: Contract and integration testing are required for retrieval and output behavior.
- Pass: PR workflow is treated as a persisted thread-state model rather than an in-memory-only review loop.
- Watch item: Confirm the final console UI library before implementation starts if `Spectre.Console` is not the intended package.

## Project Structure

### Documentation (this feature)

```text
specs/001-mvp-roadmap/
├── plan.md
└── spec.md
```

### Source Code (repository root)

```text
src/
├── cli/
├── commands/
├── config/
├── models/
├── services/
├── integrations/
└── presentation/

tests/
├── contract/
├── integration/
└── unit/

contracts/
└── app-config.schema.json
```

**Structure Decision**: Use a single-project CLI with internal separation by
responsibility so menu handlers, direct commands, config/index management,
Azure DevOps integrations, and output contracts can evolve without turning the
project into a junk drawer with assembly metadata.

## MVP Feature Breakdown

### F1. Application Foundation And Unified Config

**Purpose**: Create the executable shell, command root, local storage root,
unified config/index model, and config management services.

**Includes**:
- app bootstrap and dependency wiring
- unified config load/save behavior
- current org/project context management
- artifact index model for work items and pull requests
- config schema validation wiring

**Why first**: Every other feature depends on storage, context, and command
bootstrap.

### F2. Guided Setup And Context Selection

**Purpose**: Deliver `ado setup`, `config set-pat`, `config set-org`, and
`config set-project` with both direct and interactive selection flows.

**Includes**:
- first-run setup workflow
- PAT persistence
- org discovery by PAT
- project discovery by org + PAT
- repository name/path persistence
- configuration reset behavior that preserves stored artifact indexes
- path configuration for outputs and prompts

**Depends on**: F1

### F3. Work Item Retrieval And Export

**Purpose**: Consolidate and normalize the `workitems` capability into the new
toolkit command model.

**Includes**:
- `work-item get`
- first-order relationship traversal
- comment retrieval
- normalized JSON contract output
- generated markdown and/or JSON artifact writing
- config index updates for stored work items

**Depends on**: F1, F2

### F4. Stored Work Item History And View

**Purpose**: Let users list and view stored work item artifacts within the
current org/project context.

**Includes**:
- `work-item last`
- `work-item view`
- filtered stored history behavior
- in-console artifact rendering

**Depends on**: F3

### F5. Pull Request Import And Session Storage

**Purpose**: Consolidate the Commentor retrieval side into the new toolkit.

**Includes**:
- `pr list-active`
- `pr get`
- repository context resolution
- PR thread retrieval
- pre-plan PR artifact generation
- local session persistence
- persistent thread-state file creation
- config index updates for stored pull requests

**Depends on**: F1, F2

### F6. Pull Request Review, Refresh, And Prompt Generation

**Purpose**: Deliver the full PR review workflow that makes Commentor valuable.

**Includes**:
- `pr refresh`
- `pr review`
- `pr threads`
- `pr thread <prId> <threadId>`
- `pr thread <prId> <threadId> set fix|no-fix`
- `pr generate-prompt`
- persisted fix/no-fix decisions
- optional developer notes
- prompt-generation validation for unreviewed threads
- generated prompt artifact writing

**Depends on**: F5

### F7. Stored Pull Request History And View

**Purpose**: Let users list and view stored PR artifacts within the current
context.

**Includes**:
- `pr list-stored`
- `pr view`
- view of stored thread-state data
- filtered stored history behavior
- in-console viewing of session/prompt artifacts

**Depends on**: F5, F6

### F8. Menu System And Direct Command Parity

**Purpose**: Ensure every MVP workflow is reachable both interactively and by
direct command.

**Includes**:
- main menu tree
- menu routing into shared handlers
- parity checks between menu and command execution
- help text and discoverability

**Depends on**: F2, F4, F7

### F9. Shared Header, Presentation, And MVP Polish

**Purpose**: Enforce the cross-cutting UX expectations that make the app feel
deliberate instead of improvised.

**Includes**:
- shared ASCII art header
- command output consistency
- error and status presentation
- last-mile usability polish for core flows

**Depends on**: F8

## Recommended MVP Delivery Order

1. F1 Application Foundation And Unified Config
2. F2 Guided Setup And Context Selection
3. F3 Work Item Retrieval And Export
4. F4 Stored Work Item History And View
5. F5 Pull Request Import And Session Storage
6. F6 Pull Request Review, Refresh, And Prompt Generation
7. F7 Stored Pull Request History And View
8. F8 Menu System And Direct Command Parity
9. F9 Shared Header, Presentation, And MVP Polish

## Suggested MVP Milestones

### Milestone A - Usable Work Item Bridge

Ship when F1 through F4 are complete.

**Value**:
- clean setup
- one config model
- AI-friendly work item retrieval
- reusable stored work item artifacts

### Milestone B - Pull Request Review Consolidation

Ship when F5 through F7 are complete.

**Value**:
- PR retrieval inside the same toolkit
- stored PR sessions
- review decisions and prompt generation

### Milestone C - Unified Product Experience

Ship when F8 and F9 are complete.

**Value**:
- true menu/direct parity
- cohesive terminal UX
- polished MVP surface ready for broader use

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Single config index plus separate artifact files | Keeps one source of truth while supporting view/list performance | Storing everything inline in one giant config would become brittle and hard to inspect |
| Separate work item and PR feature slices inside one app | Preserves migration clarity from the two legacy apps | Treating MVP as one giant feature would hide dependencies and slow delivery |

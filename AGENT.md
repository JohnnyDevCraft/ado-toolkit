# AGENT.md

## Project Context
- Project: `ado-toolkit`
- Repository root: `/Users/john/Source/repos/xelseor/ado-toolkit`
- Active mode: `SpecKit`
- Current phase: constitution and project definition
- Implementation status: no product code yet
- Constitution status: template exists at `.specify/memory/constitution.md` and requires definition
- Primary workflow artifacts:
  - `.specify/memory/constitution.md`
  - future feature artifacts: `spec.md`, `plan.md`, `tasks.md`

## Agent Activity Log

### 2026-04-07
- Initialized Spec Kit in-place with Codex integration using:
  - `uvx --from git+https://github.com/github/spec-kit.git specify init . --ai codex`
- Generated hidden project scaffolding under:
  - `.specify/`
  - `.agents/skills/`
- Confirmed Spec Kit integration metadata:
  - AI assistant: `codex`
  - Script type: `sh`
  - Spec Kit version: `0.5.1.dev0`
- Confirmed project intake mode selection:
  - `SpecKit`
- Started constitution-first workflow for project definition
- Removed DevCraft-style context documents that do not belong in `SpecKit` mode:
  - `ARCH.md`
  - `DISCOVERY.md`
  - `MODEL.md`
  - `THEME.md`

## Working Agreements
- Default mode is `SpecKit`.
- Constitution work comes before feature specification, planning, tasks, or implementation.
- Keep `SpecKit` artifacts updated as the source of truth.
- Track future feature design documents under `/features`.
- Track future bug design documents under `/Bugs`.

## Release And Versioning Rules
- A version increment is not complete when code changes are only local.
- Any feature or fix that bumps the shipped application version must explicitly
  include the release execution steps in its plan and tasks.
- When the version is incremented, the agent must treat the following as part of
  the required delivery workflow unless the user explicitly scopes the work to
  documentation or local-only preparation:
  - commit and push the application repository changes
  - create and push the matching release tag in the application repository
  - run or verify the GitHub release workflow that publishes the release assets
  - verify the generated Homebrew formula and release archives for that version
  - update, commit, and push the `JohnnyDevCraft/homebrew-ado-toolkit` tap
    repository with the new `Formula/ado-toolkit.rb`
- A version bump should not be presented as ready for Homebrew use until the app
  repository release and the tap repository update are both complete.
- If a feature spec includes a version bump, the implementation close-out should
  explicitly state whether these release steps were executed or are still
  pending.

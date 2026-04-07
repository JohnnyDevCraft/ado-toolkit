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

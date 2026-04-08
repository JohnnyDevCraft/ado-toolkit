# Implementation Plan: Setup Authentication Diagnostics

**Branch**: `003-setup-auth-diagnostics` | **Date**: 2026-04-08 | **Spec**: [/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/spec.md](/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/spec.md)
**Input**: Feature specification from `/specs/003-setup-auth-diagnostics/spec.md`

## Summary

Refine the guided setup flow so PAT entry immediately triggers a validation
probe, shows a clear success or failure result, verifies the token can support
the toolkit's required Azure DevOps capabilities, and only persists the PAT
after those checks pass. The implementation should also add an explicit
acknowledgement pause after the result is shown and advance the app version to
`0.1.1` across user-visible version surfaces.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: Spectre.Console for terminal UX, System.Net.Http for
Azure DevOps requests, System.Text.Json for config persistence  
**Storage**: Local JSON config file plus referenced stored artifact files  
**Testing**: .NET test project with integration and contract coverage, plus
targeted unit coverage if validation classification logic is extracted  
**Target Platform**: macOS-first local developer CLI with supportable
cross-platform packaging  
**Project Type**: Single-project CLI application  
**Performance Goals**: PAT validation feedback should appear in one setup step
without forcing the user through downstream org or project selection when the
credential is not usable  
**Constraints**: PATs must never be saved before validation passes; setup and
menu flows must stay aligned; failure output must be actionable without leaking
secrets; capability checks should exercise the real Azure DevOps calls already
used by the application where practical  
**Scale/Scope**: Focused slice on setup, Azure DevOps context validation,
console messaging, and version metadata

## Constitution Check

*GATE: Must pass before implementation planning proceeds. Re-check after design.*

- Pass: The feature keeps setup behavior explicit and user-facing rather than
  hiding PAT validation behind later failures.
- Pass: The change improves first-run trust and preserves the constitution's
  setup-first requirement.
- Pass: The feature can be verified through deterministic integration coverage
  without changing the broader command surface.
- Pass: The design keeps secrets out of logs and output while still surfacing
  actionable diagnostics.
- Pass: Direct command and guided setup behavior remain unified through the same
  setup workflow service.

## Project Structure

### Documentation (this feature)

```text
specs/003-setup-auth-diagnostics/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── setup-pat-validation.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── commands/
├── config/
├── integrations/
├── models/
├── presentation/
└── services/

tests/
├── contract/
├── integration/
└── unit/
```

**Structure Decision**: Keep the implementation inside the existing single
project CLI. Classify Azure DevOps validation outcomes in the integration and
service layers, keep persistence changes in config and context services, and
keep acknowledgement prompts in the setup or presentation workflow so the
feature does not smear console behavior across unrelated commands.

## Feature Breakdown

### F001. PAT Validation Result Classification

**Purpose**: Distinguish invalid credentials, under-scoped credentials,
connectivity failures, and service-side failures from the first PAT test.

**Includes**:
- typed validation result or exception mapping
- response and transport error classification
- user-facing remediation guidance
- safe redaction of raw Azure DevOps failures

### F002. Capability Verification Before Persistence

**Purpose**: Prove the PAT can support the toolkit's required setup and runtime
features before it becomes the saved active PAT.

**Includes**:
- immediate PAT test after entry
- minimum capability checks for setup and primary toolkit features
- distinct reporting when the PAT authenticates but lacks required access

### F003. Setup Flow Persistence And Acknowledgement Control

**Purpose**: Save the PAT only after successful validation, and require an
explicit key press after the validation result is shown.

**Includes**:
- transient PAT handling before save
- success pause before continuing setup
- failure pause before ending the app
- preservation of previous valid config on failed retry

### F004. Version Surface Alignment

**Purpose**: Advance the shipped application version to `0.1.1` for this
feature delivery.

**Includes**:
- project version metadata
- CLI version output
- splash or header metadata under the ASCII art
- packaging or release-facing version expectations touched by automated tests

## Delivery Order

1. F001 PAT Validation Result Classification
2. F002 Capability Verification Before Persistence
3. F003 Setup Flow Persistence And Acknowledgement Control
4. F004 Version Surface Alignment

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Real Azure DevOps capability probing instead of format-only PAT validation | The spec requires proving the PAT is actually usable for this app | Syntax-only checks would still allow setup to save unusable tokens |

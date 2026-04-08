# Tasks: Homebrew Distribution

**Input**: Design documents from `/specs/002-homebrew-distribution/`
**Prerequisites**: spec.md (required for user stories)

**Tests**: Include tests according to the governing constitution and feature
specification. For this feature, contract and integration coverage focus on
release asset naming, formula generation, and documented release flow behavior.

**Organization**: Tasks are grouped by user story to enable independent
implementation and review of each distribution slice.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this belongs to (e.g., `US1`, `US2`)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/`, `scripts/`, `packaging/`, `docs/` at repository root
- Paths below assume the existing ADO Toolkit repository layout

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish release-packaging folders, version metadata alignment, and baseline documentation anchors.

- [ ] T001 Create Homebrew packaging and release documentation folders in `/Users/john/Source/repos/xelseor/ado-toolkit/packaging/homebrew/`, `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/`, `/Users/john/Source/repos/xelseor/ado-toolkit/docs/`, and `/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/`
- [ ] T002 Add or normalize release metadata in `/Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj` so version, description, and repository URL are explicit and usable by packaging flows
- [ ] T003 [P] Add Homebrew distribution overview sections to `/Users/john/Source/repos/xelseor/ado-toolkit/README.md` and `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md` with approval-gated notes for external publish actions

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core release infrastructure that MUST be complete before any user-story packaging or tap work can proceed.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Create a reusable local packaging script in `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh` that reads version metadata, publishes the app, packages archives, computes checksums, and writes outputs to `dist/homebrew/`
- [ ] T005 Create a formula template in `/Users/john/Source/repos/xelseor/ado-toolkit/packaging/homebrew/ado-toolkit.rb.template` that installs the `ado` executable and validates `ado --help`
- [ ] T006 [P] Add release workflow scaffolding in `/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/release.yml` for tagged builds, archive packaging, formula generation, and release asset upload
- [ ] T007 [P] Add testable helper logic or validation scripts for archive naming, checksum substitution, and formula generation in `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh` and `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`

**Checkpoint**: Release packaging foundations are ready for story-level implementation

---

## Phase 3: User Story 1 - Generate Reviewable Homebrew Assets Locally (Priority: P1) 🎯 MVP

**Goal**: Let the maintainer generate archives and a formula locally for review before any publish action happens.

**Independent Test**: Run the local packaging script from the repo and verify it creates versioned archives plus a reviewable `ado-toolkit.rb` formula without touching any external repository.

### Tests for User Story 1 ⚠️

- [ ] T008 [P] [US1] Add integration coverage for local packaging outputs and archive naming in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/HomebrewPackagingIntegrationTests.cs`
- [ ] T009 [P] [US1] Add contract coverage for generated formula content and checksum placeholder substitution in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/HomebrewFormulaContractTests.cs`

### Implementation for User Story 1

- [ ] T010 [US1] Implement macOS archive packaging logic in `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh` for the initial supported targets and output naming contract
- [ ] T011 [US1] Implement formula generation in `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh` and `/Users/john/Source/repos/xelseor/ado-toolkit/packaging/homebrew/ado-toolkit.rb.template`
- [ ] T012 [US1] Document the local review flow, expected outputs, and non-publish boundary in `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md` and `/Users/john/Source/repos/xelseor/ado-toolkit/README.md`

**Checkpoint**: Local packaging is functional and reviewable without publishing anything

---

## Phase 4: User Story 2 - Publish Repeatable GitHub Release Assets (Priority: P1)

**Goal**: Make tagged releases reproduce the same archives and formula automatically.

**Independent Test**: Inspect the workflow and validate that a tagged run would build archives, generate the formula from actual checksums, and upload the assets consistently.

### Tests for User Story 2 ⚠️

- [ ] T013 [P] [US2] Add workflow validation coverage or fixture-based checks for release archive naming and formula artifact generation in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/HomebrewReleaseWorkflowTests.cs`

### Implementation for User Story 2

- [ ] T014 [US2] Implement tagged release build steps in `/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/release.yml` for supported macOS runners and self-contained publish outputs
- [ ] T015 [US2] Implement workflow steps that reuse the packaging script or equivalent logic to generate `ado-toolkit.rb` from actual archive checksums in `/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/release.yml`
- [ ] T016 [US2] Document the release-tag process, artifact expectations, and formula handoff steps in `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`

**Checkpoint**: Tagged release automation is defined and testable without creating the tap repo yet

---

## Phase 5: User Story 3 - Maintain a Dedicated Tap Repository (Priority: P2)

**Goal**: Define the dedicated tap repository shape and handoff so the formula has a clean long-term home.

**Independent Test**: Review the documented tap repo structure and verify it supports `Formula/ado-toolkit.rb` under the repo name `homebrew-ado-toolkit`.

### Tests for User Story 3 ⚠️

- [ ] T017 [P] [US3] Add documentation validation checks or review checklist coverage for tap repo naming and formula placement in `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`

### Implementation for User Story 3

- [ ] T018 [US3] Document the dedicated tap repository name `homebrew-ado-toolkit` and the required `Formula/ado-toolkit.rb` layout in `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`
- [ ] T019 [US3] Document the maintainer handoff steps for copying or publishing the generated formula into the tap repository in `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`
- [ ] T020 [US3] Add user-facing tap install instructions that assume the dedicated tap repo exists in `/Users/john/Source/repos/xelseor/ado-toolkit/README.md`

**Checkpoint**: Tap repo design is fully specified and reviewable before any external repo creation

---

## Phase 6: User Story 4 - Preserve Approval Boundaries Before Publishing (Priority: P2)

**Goal**: Keep external repo creation and publish actions behind explicit owner approval.

**Independent Test**: Review the scripts and docs to confirm they separate local generation from publish-time actions and do not silently create external resources.

### Tests for User Story 4 ⚠️

- [ ] T021 [P] [US4] Add verification coverage for approval-gated publish boundaries in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/HomebrewApprovalBoundaryTests.cs`

### Implementation for User Story 4

- [ ] T022 [US4] Update `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh` so it only performs local asset generation and does not create or mutate external repositories
- [ ] T023 [US4] Update `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md` and `/Users/john/Source/repos/xelseor/ado-toolkit/README.md` to make approval gates explicit before tap repo creation or publish-time changes
- [ ] T024 [US4] Add maintainer notes in `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md` clarifying that creation of `homebrew-ado-toolkit` happens only after separate approval

**Checkpoint**: The distribution design preserves the review boundary you requested

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final cleanup, consistency checks, and review prep across the full Homebrew feature

- [ ] T025 [P] Add a short release artifact matrix and naming examples to `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`
- [ ] T026 [P] Run full verification for packaging-related tests and fix any cross-story regressions in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/`
- [ ] T027 Validate the local packaging command, release docs, and formula template together in `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh`, `/Users/john/Source/repos/xelseor/ado-toolkit/packaging/homebrew/ado-toolkit.rb.template`, and `/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational and delivers the first safe reviewable packaging slice
- **User Story 2 (P1)**: Depends on User Story 1 so release automation matches the local packaging contract
- **User Story 3 (P2)**: Depends on User Story 1 and User Story 2 so the tap repo is designed around stable artifact outputs
- **User Story 4 (P2)**: Depends on all earlier stories because it enforces the approval boundary across the final workflow

### Within Each User Story

- Tests MUST be written and fail before implementation work for that story is considered complete
- Packaging logic comes before documentation that references exact artifact names
- Release automation comes after local packaging behavior is stable
- Tap repo design is documented before any implementation that would create external resources

### Parallel Opportunities

- T003 can run in parallel with T001-T002
- T006 and T007 can run in parallel after foundational folders and metadata exist
- T008 and T009 can run in parallel within User Story 1
- T013 can run in parallel with T016 after workflow structure is chosen
- T017 and T021 can run in parallel late in the cycle
- T025 and T026 can run in parallel during polish

---

## Implementation Strategy

### Review-First Delivery

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and REVIEW**: Confirm local packaging outputs and formula naming look right
5. Complete Phase 4: User Story 2
6. Complete Phase 5: User Story 3
7. Complete Phase 6: User Story 4
8. Finish Phase 7: Polish

### Safe Rollout Order

1. Local packaging only
2. Tagged release automation
3. Tap repository documentation and structure
4. Explicit approval before any external repo creation or tap mutation

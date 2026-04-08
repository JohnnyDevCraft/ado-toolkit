# Tasks: Setup Authentication Diagnostics

**Input**: Design documents from `/specs/003-setup-auth-diagnostics/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Integration and contract coverage are required for this feature
because setup behavior, validation classification, safe persistence, and
version surfaces are all externally observable.

**Organization**: Tasks are grouped by user story to enable independent
implementation and testing.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish feature scaffolding and shared validation primitives used
by all stories.

- [ ] T001 Create PAT validation design artifacts in `/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/plan.md`, `/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/research.md`, `/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/data-model.md`, `/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/contracts/setup-pat-validation.md`, and `/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/quickstart.md`
- [ ] T002 Define or add validation result model types for PAT classification and capability checks in `/Users/john/Source/repos/xelseor/ado-toolkit/src/models/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the shared validation and capability-checking flow before
changing the guided setup UX.

- [ ] T003 [P] Add Azure DevOps validation classification coverage for auth, permission, connectivity, and service outcomes in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/unit/` or `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`
- [ ] T004 Implement PAT validation and capability probe orchestration in `/Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsContextClient.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/IAzureDevOpsContextClient.cs`
- [ ] T005 Implement service-layer handling for transient PAT validation before persistence in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/CurrentContextService.cs`

**Checkpoint**: Validation outcomes can be computed and reported without yet
rewiring the full guided setup flow.

---

## Phase 3: User Story 1 - Fail Setup Gracefully On Invalid Or Under-Scoped PATs (Priority: P1) 🎯 MVP

**Goal**: Show an immediate PAT test result with clear classification and do
not continue setup with an unusable credential.

**Independent Test**: Run `ado setup` with invalid and under-scoped PATs and
verify setup shows an actionable result, does not save the PAT, and ends only
after acknowledgement.

### Tests for User Story 1

- [ ] T006 [P] [US1] Add integration coverage for invalid PAT failure messaging and safe non-persistence in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`
- [ ] T007 [P] [US1] Add integration coverage for under-scoped PAT classification and capability reporting in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`
- [ ] T008 [P] [US1] Add contract coverage for setup PAT validation result categories and guidance text expectations in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/`

### Implementation for User Story 1

- [ ] T009 [US1] Implement invalid-credential and insufficient-permission result mapping in `/Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsContextClient.cs`
- [ ] T010 [US1] Update setup result rendering and failure-path messaging in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/SetupWorkflowService.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/ConsoleOutputService.cs`
- [ ] T011 [US1] Ensure failed PAT validation does not write the new token or claim setup success in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/SetupWorkflowService.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/CurrentContextService.cs`

**Checkpoint**: Invalid and under-scoped PATs are handled clearly and safely.

---

## Phase 4: User Story 2 - Validate PATs Before Advancing Setup (Priority: P1)

**Goal**: Test the PAT immediately after entry, save it only on success, and
pause for acknowledgement before moving into the next setup step.

**Independent Test**: Run `ado setup` with a valid PAT and verify the success
result appears immediately, requires acknowledgement, persists the PAT only
after success, and then continues into organization and project selection.

### Tests for User Story 2

- [ ] T012 [P] [US2] Add integration coverage for successful PAT validation, acknowledgement pause, and post-success persistence in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`
- [ ] T013 [P] [US2] Add integration coverage for connectivity and service failure classification during PAT validation in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`

### Implementation for User Story 2

- [ ] T014 [US2] Rework the guided setup PAT step to validate immediately and continue only after acknowledgement in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/SetupWorkflowService.cs`
- [ ] T015 [US2] Add explicit key-press acknowledgement handling for PAT test results in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/SetupWorkflowService.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/`
- [ ] T016 [US2] Preserve existing valid config when a replacement PAT fails validation in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/CurrentContextService.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`

**Checkpoint**: Setup advances only after a successful acknowledged PAT test.

---

## Phase 5: User Story 3 - Preserve Safe Config State During Failed Setup (Priority: P2)

**Goal**: Keep config trustworthy across retries and make the setup recovery
path clear.

**Independent Test**: Retry setup after both success and failure cases and
verify config remains accurate, previous valid state survives failed retries,
and rerunning setup behaves consistently.

### Tests for User Story 3

- [ ] T017 [P] [US3] Add integration coverage for retry behavior after failed PAT validation from both clean and previously configured states in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs`

### Implementation for User Story 3

- [ ] T018 [US3] Refine setup flow state transitions and retry-safe config handling in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/SetupWorkflowService.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/src/config/AppConfigService.cs`
- [ ] T019 [US3] Update any relevant setup help or remediation messaging in `/Users/john/Source/repos/xelseor/ado-toolkit/src/commands/HelpCommand.cs` and `/Users/john/Source/repos/xelseor/ado-toolkit/README.md`

**Checkpoint**: Failed setup leaves the app in a safe and understandable state.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Align version metadata and clean up feature-wide consistency.

- [ ] T020 [P] Advance the application version to `0.1.1` in `/Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj`
- [ ] T021 [P] Update CLI version output, splash metadata under the ASCII art, and any version-pinned packaging assertions to `0.1.1` in `/Users/john/Source/repos/xelseor/ado-toolkit/src/commands/CommandRouter.cs`, `/Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/AppHeaderRenderer.cs`, `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/HomebrewPackagingIntegrationTests.cs`, and `/Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/HomebrewFormulaContractTests.cs`
- [ ] T022 [P] Add or update presentation coverage for the splash metadata lines in `/Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/PresentationConsistencyTests.cs`
- [ ] T023 Run setup quickstart validation and confirm all PAT result paths match `/Users/john/Source/repos/xelseor/ado-toolkit/specs/003-setup-auth-diagnostics/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup completion and blocks story work
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on Foundational completion and should be
  integrated after the main classification behavior from User Story 1
- **User Story 3 (Phase 5)**: Depends on the earlier setup flow changes
- **Polish (Phase 6)**: Depends on all story work being complete

### Parallel Opportunities

- T003 can run in parallel with model-shape setup once task boundaries are clear
- T006, T007, and T008 can be prepared in parallel
- T012 and T013 can be prepared in parallel
- T020 and T021 can run in parallel once implementation is stable

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2
2. Complete Phase 3 to get safe invalid and under-scoped PAT handling
3. Validate the setup failure path before expanding the success-path pause and
   continuation behavior

### Incremental Delivery

1. Land validation classification and safe persistence
2. Land acknowledgement-controlled setup progression
3. Land retry-state polish and version alignment

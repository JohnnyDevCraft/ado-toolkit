# Tasks: ADO Toolkit MVP Roadmap

**Input**: Design documents from `/specs/001-mvp-roadmap/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: Include tests according to the governing constitution and feature
specification. For features with required contract, integration, or unit test
coverage, those tasks are mandatory and should be generated explicitly.

**Organization**: Tasks are grouped by user story to enable independent
implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Paths below assume the single-project CLI structure defined in `plan.md`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the repo structure, executable shell, and baseline test/config assets.

- [X] T001 Create the ADO Toolkit solution, console entrypoint, and baseline folders in /Users/john/Source/repos/xelseor/ado-toolkit/src/Program.cs, /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj, /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/, /Users/john/Source/repos/xelseor/ado-toolkit/src/config/, /Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/, /Users/john/Source/repos/xelseor/ado-toolkit/src/models/, /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/, /Users/john/Source/repos/xelseor/ado-toolkit/src/services/, /Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/, /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/, and /Users/john/Source/repos/xelseor/ado-toolkit/tests/unit/
- [X] T002 Add .NET 10 package references and shared test project wiring in /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj and /Users/john/Source/repos/xelseor/ado-toolkit/tests/ado-toolkit.tests.csproj
- [X] T003 [P] Add baseline serialization and config contract validation helpers in /Users/john/Source/repos/xelseor/ado-toolkit/src/config/JsonFileStore.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/config/SchemaValidationService.cs
- [X] T004 [P] Create reusable command parsing and app host scaffolding in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/CommandRouter.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/services/AppHost.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story can be implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Create the unified config domain models based on /Users/john/Source/repos/xelseor/ado-toolkit/contracts/app-config.schema.json in /Users/john/Source/repos/xelseor/ado-toolkit/src/models/AppConfig.cs, /Users/john/Source/repos/xelseor/ado-toolkit/src/models/CurrentContext.cs, /Users/john/Source/repos/xelseor/ado-toolkit/src/models/WorkItemEntry.cs, /Users/john/Source/repos/xelseor/ado-toolkit/src/models/PullRequestEntry.cs, /Users/john/Source/repos/xelseor/ado-toolkit/src/models/ArtifactRef.cs, and /Users/john/Source/repos/xelseor/ado-toolkit/src/models/PullRequestThreadState.cs
- [X] T006 Port and unify local path conventions from /Users/john/Source/repos/xelseor/workitems/Services/AppPaths.cs and /Users/john/Source/repos/xelseor/Commentor/Services/CommentorPaths.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/config/AppPaths.cs
- [X] T007 Implement unified config load/save/index update behavior by adapting /Users/john/Source/repos/xelseor/workitems/Services/ConfigService.cs and /Users/john/Source/repos/xelseor/Commentor/Services/LocalStorageService.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/config/AppConfigService.cs
- [X] T008 [P] Implement shared Azure DevOps auth and HTTP request helpers by consolidating /Users/john/Source/repos/xelseor/workitems/Services/AzureDevOpsClient.cs and /Users/john/Source/repos/xelseor/Commentor/Services/AzureDevOpsClient.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsHttpClientFactory.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsAuthService.cs
- [X] T009 [P] Create the shared console presentation layer with header rendering, status output, and error formatting in /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/AppHeaderRenderer.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/ConsoleOutputService.cs
- [X] T010 Add contract tests for unified config serialization and schema alignment in /Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/AppConfigSchemaTests.cs
- [X] T011 Add integration tests for config persistence and storage-root path behavior in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/AppConfigServiceTests.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - First-Run Setup And Working Context (Priority: P1) 🎯 MVP

**Goal**: Let a first-time user configure PAT, organization, project, and local paths into one valid working context.

**Independent Test**: Run `ado setup`, `ado config set-org`, and `ado config set-project` from a clean local state and verify the config file is created and updated correctly.

### Tests for User Story 1 ⚠️

- [X] T012 [P] [US1] Add integration tests for first-run setup and path capture in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/SetupWorkflowTests.cs
- [X] T013 [P] [US1] Add integration tests for organization and project selection flows in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/ContextSelectionCommandTests.cs

### Implementation for User Story 1

- [X] T014 [US1] Implement project and organization listing methods by reusing the project-listing logic from /Users/john/Source/repos/xelseor/workitems/Services/AzureDevOpsClient.cs and /Users/john/Source/repos/xelseor/Commentor/Services/AzureDevOpsClient.cs in /Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsContextClient.cs
- [X] T015 [US1] Implement the guided setup workflow and persisted settings updates in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/SetupCommand.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/services/SetupWorkflowService.cs
- [X] T016 [US1] Implement `ado config set-pat`, `ado config set-org`, and `ado config set-project` direct commands plus interactive fallbacks in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/ConfigCommands.cs
- [X] T017 [US1] Implement current-context selection and config save behavior for PAT/org/project/path settings in /Users/john/Source/repos/xelseor/ado-toolkit/src/services/CurrentContextService.cs
- [X] T017A [US1] Implement `ado config set-repo <RepoName> <RepoPath>` and `ado config reset` so repository path is persisted and active context can be cleared without deleting stored indexes in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/ConfigCommands.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/services/CurrentContextService.cs

**Checkpoint**: Setup and working context are fully functional and testable independently

---

## Phase 4: User Story 2 - Work Item Retrieval And Local Artifact Indexing (Priority: P1)

**Goal**: Retrieve a complete work item context graph, save artifacts, and index them locally for later use.

**Independent Test**: Run `ado work-item get <id>` against a representative work item and verify relationship traversal, comment capture, artifact creation, and config index updates.

### Tests for User Story 2 ⚠️

- [X] T018 [P] [US2] Add contract tests for the normalized work-item JSON output in /Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/WorkItemGraphContractTests.cs
- [X] T019 [P] [US2] Add integration tests for work-item retrieval, relation traversal, and comment capture in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/WorkItemRetrievalIntegrationTests.cs

### Implementation for User Story 2

- [X] T020 [P] [US2] Port the legacy work-item models needed for graph retrieval from /Users/john/Source/repos/xelseor/workitems/Models/NormalizedWorkItem.cs, /Users/john/Source/repos/xelseor/workitems/Models/RelatedWorkItem.cs, /Users/john/Source/repos/xelseor/workitems/Models/RetrievalResult.cs, /Users/john/Source/repos/xelseor/workitems/Models/WorkItemComment.cs, and /Users/john/Source/repos/xelseor/workitems/Models/WorkItemReference.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/models/workitems/
- [X] T021 [US2] Port and adapt the relationship and comment retrieval logic from /Users/john/Source/repos/xelseor/workitems/Services/AzureDevOpsClient.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsWorkItemClient.cs
- [X] T022 [US2] Port and adapt the reference parsing and first-order related-item discovery logic from /Users/john/Source/repos/xelseor/workitems/Services/ReferenceParser.cs and /Users/john/Source/repos/xelseor/workitems/Services/WorkItemRetrievalService.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemReferenceParser.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemRetrievalService.cs
- [X] T023 [US2] Port and adapt markdown export behavior from /Users/john/Source/repos/xelseor/workitems/Services/MarkdownExportService.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemArtifactWriter.cs
- [X] T024 [US2] Implement `ado work-item get` command handling, artifact indexing, and `--out` override behavior in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/WorkItemCommands.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemIndexService.cs

**Checkpoint**: Work item retrieval and artifact indexing are fully functional and testable independently

---

## Phase 5: User Story 3 - Pull Request Retrieval, Review, And Prompt Generation (Priority: P1)

**Goal**: Import pull requests, persist thread-state files, classify threads one command at a time, and generate prompt artifacts after all threads are reviewed.

**Independent Test**: Run `ado pr get`, `ado pr threads`, `ado pr thread ... set fix|no-fix`, and `ado pr generate-prompt` against a representative PR and verify state survives across separate command invocations.

### Tests for User Story 3 ⚠️

- [X] T025 [P] [US3] Add contract tests for stored PR session and thread-state serialization in /Users/john/Source/repos/xelseor/ado-toolkit/tests/contract/PullRequestSessionContractTests.cs
- [X] T026 [P] [US3] Add integration tests for PR retrieval, thread listing, thread updates, and prompt-generation gating in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/PullRequestWorkflowIntegrationTests.cs

### Implementation for User Story 3

- [X] T027 [P] [US3] Port the legacy PR models needed for retrieval and stored review state from /Users/john/Source/repos/xelseor/Commentor/Models/AdoAuthenticatedUser.cs, /Users/john/Source/repos/xelseor/Commentor/Models/AdoProjectInfo.cs, /Users/john/Source/repos/xelseor/Commentor/Models/AdoRepositoryInfo.cs, /Users/john/Source/repos/xelseor/Commentor/Models/AdoPullRequestInfo.cs, /Users/john/Source/repos/xelseor/Commentor/Models/PullRequestThreadRecord.cs, /Users/john/Source/repos/xelseor/Commentor/Models/PullRequestCommentRecord.cs, and /Users/john/Source/repos/xelseor/Commentor/Models/PullRequestSession.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/models/pullrequests/
- [X] T028 [US3] Port and adapt PR retrieval logic from /Users/john/Source/repos/xelseor/Commentor/Services/AzureDevOpsClient.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsPullRequestClient.cs
- [X] T029 [US3] Implement persistent PR session and thread-state storage by adapting /Users/john/Source/repos/xelseor/Commentor/Services/LocalStorageService.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestStorageService.cs
- [X] T030 [P] [US3] Port and adapt prompt-generation logic from /Users/john/Source/repos/xelseor/Commentor/Services/PromptBuilder.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestPromptBuilder.cs
- [X] T031 [P] [US3] Port and adapt repository excerpt generation from /Users/john/Source/repos/xelseor/Commentor/Services/CodeExcerptService.cs into /Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestCodeExcerptService.cs
- [X] T032 [US3] Implement `ado pr get`, `ado pr list-active`, and pre-plan PR artifact generation in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/PullRequestCommands.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestImportService.cs
- [X] T033 [US3] Implement `ado pr threads`, `ado pr thread <prId> <threadId>`, and `ado pr thread <prId> <threadId> set fix|no-fix --instruction ...` in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/PullRequestThreadCommands.cs
- [X] T034 [US3] Implement `ado pr refresh`, `ado pr review`, and `ado pr generate-prompt` with unreviewed-thread validation in /Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestReviewService.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/PullRequestReviewCommands.cs

**Checkpoint**: Pull request import, thread-by-thread review, and prompt generation are fully functional and testable independently

---

## Phase 6: User Story 4 - Unified Menu And Direct Command Parity (Priority: P2)

**Goal**: Ensure the same capabilities are reachable through menu workflows and direct commands using shared handlers.

**Independent Test**: Verify a representative setup, work-item, and PR thread-review flow can be completed through both menu and direct command routes with equivalent persisted results.

### Tests for User Story 4 ⚠️

- [X] T035 [P] [US4] Add integration tests that compare direct-command and menu-driven results for setup, work-item retrieval, and PR thread updates in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/MenuCommandParityTests.cs

### Implementation for User Story 4

- [X] T036 [US4] Implement the main menu tree and routing over shared application services in /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/MainMenuWorkflow.cs
- [X] T037 [US4] Refactor command handlers and menu actions to use shared workflow services for setup, work-items, PR import, thread updates, and prompt generation in /Users/john/Source/repos/xelseor/ado-toolkit/src/services/CommandWorkflowBridge.cs
- [X] T038 [US4] Implement consolidated help output documenting both interactive and direct command paths in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/HelpCommand.cs

**Checkpoint**: Menu and direct command parity are functional and testable independently

---

## Phase 7: User Story 5 - Filtered Stored History And In-Console Viewing (Priority: P2)

**Goal**: Let users list and view stored work-item and PR artifacts filtered by current org/project context.

**Independent Test**: Save artifacts across multiple contexts, switch current context, and verify the list and view commands only show relevant items by default.

### Tests for User Story 5 ⚠️

- [X] T039 [P] [US5] Add integration tests for filtered stored-history and in-console view behavior in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/StoredHistoryFilteringTests.cs

### Implementation for User Story 5

- [X] T040 [US5] Implement filtered work-item history, `ado work-item last`, and `ado work-item view` in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/WorkItemHistoryCommands.cs
- [X] T041 [US5] Implement filtered PR history, `ado pr list-stored`, and `ado pr view` in /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/PullRequestHistoryCommands.cs
- [X] T042 [US5] Implement artifact viewing and missing-file recovery messaging for stored markdown, JSON, session, and prompt files in /Users/john/Source/repos/xelseor/ado-toolkit/src/services/ArtifactViewerService.cs

**Checkpoint**: Stored history and console viewing are functional and testable independently

---

## Phase 8: User Story 6 - Cohesive Presentation And Discoverability (Priority: P3)

**Goal**: Apply the shared header and final UX consistency rules across every page and direct command.

**Independent Test**: Run help, version, setup, work-item, PR, and view flows and verify consistent header and presentation behavior.

### Tests for User Story 6 ⚠️

- [X] T043 [P] [US6] Add integration tests for header rendering and presentation consistency across direct and menu flows in /Users/john/Source/repos/xelseor/ado-toolkit/tests/integration/PresentationConsistencyTests.cs

### Implementation for User Story 6

- [X] T044 [US6] Implement the final shared ASCII art header and page framing behavior in /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/AppHeaderRenderer.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/PageLayout.cs
- [X] T045 [US6] Apply consistent status, error, and completion presentation across setup, work-item, PR, help, and view commands in /Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/ConsoleOutputService.cs and /Users/john/Source/repos/xelseor/ado-toolkit/src/Program.cs

**Checkpoint**: Presentation is cohesive and testable independently

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T046 [P] Document the consolidated legacy-code reuse map and migration notes in /Users/john/Source/repos/xelseor/ado-toolkit/specs/001-mvp-roadmap/reuse-notes.md
- [X] T047 Validate the Homebrew-friendly executable flow, direct-command help output, and setup quick path in /Users/john/Source/repos/xelseor/ado-toolkit/README.md and /Users/john/Source/repos/xelseor/ado-toolkit/src/commands/HelpCommand.cs
- [X] T048 Run full contract, integration, and unit test suites and fix any cross-story regressions in /Users/john/Source/repos/xelseor/ado-toolkit/tests/

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependencies on other stories
- **User Story 2 (P1)**: Depends on User Story 1 for working context and config services
- **User Story 3 (P1)**: Depends on User Story 1 for working context and config services
- **User Story 4 (P2)**: Depends on User Stories 1, 2, and 3 to provide the shared handlers
- **User Story 5 (P2)**: Depends on User Stories 2 and 3 to provide stored artifacts
- **User Story 6 (P3)**: Depends on User Stories 1 through 5 for consistent final presentation coverage

### Within Each User Story

- Tests MUST be written and fail before implementation work for that story is considered complete
- Ported legacy models and services come before the command handlers that depend on them
- Artifact indexing and persistence must exist before list/view flows
- PR retrieval must exist before thread review and prompt generation

### Parallel Opportunities

- T003 and T004 can run in parallel after project scaffolding exists
- T008 and T009 can run in parallel after the core models are defined
- T012 and T013 can run in parallel within User Story 1
- T018 and T019 can run in parallel within User Story 2
- T020 can run in parallel with T018 and T019
- T025 and T026 can run in parallel within User Story 3
- T027, T030, and T031 can run in parallel once the foundational models and storage services are in place
- T039, T043, and documentation work in T046 can run in parallel late in the cycle

---

## Parallel Example: User Story 3

```bash
# Launch PR contract and integration test work together:
Task: "Add contract tests for stored PR session and thread-state serialization in tests/contract/PullRequestSessionContractTests.cs"
Task: "Add integration tests for PR retrieval, thread listing, thread updates, and prompt-generation gating in tests/integration/PullRequestWorkflowIntegrationTests.cs"

# Launch reusable PR support services together once storage foundations exist:
Task: "Port and adapt prompt-generation logic into src/services/PullRequestPromptBuilder.cs"
Task: "Port and adapt repository excerpt generation into src/services/PullRequestCodeExcerptService.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1-3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Complete Phase 4: User Story 2
5. Complete Phase 5: User Story 3
6. **STOP and VALIDATE**: Confirm the toolkit can set context, retrieve work items, import PRs, classify threads, and generate prompts

### Incremental Delivery

1. Complete Setup + Foundational
2. Add User Story 1 for working context
3. Add User Story 2 for work-item retrieval
4. Add User Story 3 for PR retrieval and persistent thread review
5. Add User Story 4 for menu/direct parity
6. Add User Story 5 for stored-history usability
7. Add User Story 6 for final presentation polish

### Reuse-First Strategy

1. Port and adapt proven retrieval logic from `/Users/john/Source/repos/xelseor/workitems`
2. Port and adapt proven PR/thread/prompt logic from `/Users/john/Source/repos/xelseor/Commentor`
3. Replace duplicate infrastructure with unified config, path, and command abstractions
4. Preserve tested behavior before introducing new structure or UX polish

---

## Notes

- [P] tasks = different files, no dependencies
- [US#] labels map tasks to specific user stories for traceability
- Legacy code reuse is required, especially for work-item relationship traversal, comment retrieval, PR thread retrieval, local session storage, and prompt generation
- Verify tests cover contract stability before implementation is considered done
- Avoid rewriting proven Azure DevOps traversal logic unless adaptation is required by the unified app model

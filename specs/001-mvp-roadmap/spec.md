# Feature Specification: ADO Toolkit MVP Roadmap

**Feature Branch**: `001-mvp-roadmap`  
**Created**: 2026-04-07  
**Status**: Draft  
**Input**: User description: "Plan out the ADO Toolkit MVP as a set of implementation features that unify the existing work item and pull request tools into one SpecKit-driven product roadmap."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First-Run Setup And Working Context (Priority: P1)

As a software engineer, I can install ADO Toolkit, configure my PAT, choose my
organization and project, and leave the app with a valid default working
context.

**Why this priority**: Nothing else is usable until the toolkit can establish a
trusted working context and local storage model.

**Independent Test**: On a clean machine or clean home-directory state, run the
toolkit setup flow and verify that the config file is created, defaults are
persisted, and subsequent commands can resolve organization and project context
without re-prompting.

**Acceptance Scenarios**:

1. **Given** no local toolkit configuration, **When** the user runs `ado setup`,
   **Then** the app collects and saves PAT, organization, project, and path
   settings in one unified config model.
2. **Given** a saved PAT and no selected organization, **When** the user runs
   `ado config set-org`, **Then** the app can list accessible organizations for
   selection.
3. **Given** a selected organization and saved PAT, **When** the user runs
   `ado config set-project`, **Then** the app can list accessible projects for
   selection.

---

### User Story 2 - Work Item Retrieval And Local Artifact Indexing (Priority: P1)

As a software engineer or AI workflow, I can retrieve a work item and receive a
complete first-order context package plus indexed local artifacts that I can
view again later.

**Why this priority**: This is the core AI-context bridge for specification and
implementation work.

**Independent Test**: Run `ado work-item get <id>` for a known work item and
verify the output includes the root item, first-order relations, comments, and
stored artifact references that can later be listed and viewed.

**Acceptance Scenarios**:

1. **Given** a valid working context and work item ID, **When** the user runs
   `ado work-item get <id>`, **Then** the toolkit retrieves the work item,
   parents, children, first-level related items, and comments for included
   items.
2. **Given** a completed work item retrieval, **When** the toolkit saves local
   artifacts, **Then** the config index stores the work item metadata, org,
   project, and artifact paths.
3. **Given** a previously stored work item, **When** the user runs
   `ado work-item view <id>`, **Then** the toolkit prints the stored artifact
   contents in the console with the standard header.

---

### User Story 3 - Pull Request Retrieval, Review, And Prompt Generation (Priority: P1)

As a software engineer, I can retrieve my pull requests, triage threads,
capture notes, and generate an AI-ready prompt from a stored PR session using
persistent thread state that survives across separate command invocations.

**Why this priority**: This is the second major capability being consolidated
from the existing tools and directly supports PR-driven engineering work.

**Independent Test**: Run the PR flows end to end for a known pull request and
verify the toolkit can import session data, store a persistent thread-state
file, update thread decisions one command at a time, and generate a prompt
artifact only after all threads are classified.

**Acceptance Scenarios**:

1. **Given** a valid working context, **When** the user runs `ado pr list-active`,
   **Then** the toolkit lists active PRs created by the authenticated user.
2. **Given** a pull request ID and repository context, **When** the user runs
   `ado pr get <id>`, **Then** the toolkit imports the PR session, writes a
   pre-plan PR artifact with stored thread records, and indexes it in unified
   local storage.
3. **Given** a stored PR session, **When** the user runs `ado pr threads <id>`
   and `ado pr thread <id> <threadId> set fix|no-fix`, **Then** the toolkit
   persists the updated thread review state to disk immediately.
4. **Given** a stored PR session with unreviewed threads, **When** the user runs
   `ado pr generate-prompt <id>`, **Then** the toolkit fails clearly and reports
   which thread IDs still require classification.
5. **Given** a stored PR session with all threads classified, **When** the user
   runs `ado pr generate-prompt <id>`, **Then** the toolkit writes the final
   prompt artifact using only threads marked `fix` while preserving all thread
   decisions in storage.

---

### User Story 4 - Unified Menu And Direct Command Parity (Priority: P2)

As a software engineer or automation agent, I can reach the same capabilities
through either the guided menu or direct commands without behavioral drift.

**Why this priority**: This is a core constitution rule and reduces maintenance
risk as the toolkit grows.

**Independent Test**: Choose a representative setup action, work-item flow, and
PR flow, then verify that each can be completed both interactively and by direct
command using the same underlying handlers and outcome.

**Acceptance Scenarios**:

1. **Given** a menu action exists, **When** the user drills into that action,
   **Then** there is an equivalent direct command path for the same behavior.
2. **Given** a direct command completes successfully, **When** the corresponding
   menu path is used, **Then** it produces equivalent stored artifacts and index
   updates.
3. **Given** a PR thread can be marked interactively, **When** the same thread
   is updated by direct command, **Then** both paths use the same persistent
   thread-state model and produce equivalent review results.

---

### User Story 5 - Filtered Stored History And In-Console Viewing (Priority: P2)

As a software engineer, I can list the work items and pull requests I have
already worked with, filtered to my current org and project, and open them in
the console without hunting for files manually.

**Why this priority**: This is where the unified toolkit becomes easier to live
with than the two legacy tools.

**Independent Test**: Save multiple work items and pull requests across at least
two contexts, switch the current org/project, and verify the default list and
view behavior reflects only the active context.

**Acceptance Scenarios**:

1. **Given** stored work items and pull requests from multiple contexts, **When**
   the user changes the current organization or project, **Then** default list
   views show only matching entries for the active context.
2. **Given** a stored artifact exists, **When** the user selects a view action
   in the menu or runs a `view` command, **Then** the toolkit prints the stored
   content directly in the console.

---

### User Story 6 - Cohesive Presentation And Discoverability (Priority: P3)

As a software engineer, I get a consistent, polished terminal experience with a
shared header, help output, and navigable menus across every execution path.

**Why this priority**: It raises trust and usability, but the app still has
core value before full polish lands.

**Independent Test**: Run help, version, setup, work-item, PR, and view flows
and verify the same header and navigation conventions appear consistently.

**Acceptance Scenarios**:

1. **Given** any menu page, **When** it renders, **Then** the standard ASCII art
   header appears legibly.
2. **Given** any direct command, **When** it runs, **Then** the same header
   appears before command output.

### Edge Cases

- What happens when the PAT is invalid, expired, or lacks access to the
  selected organization or project?
- What happens when a work item or pull request exists but related comments or
  linked items cannot be retrieved due to permissions?
- What happens when the current project changes but stored artifacts from the
  previous project still exist locally?
- How does the app behave when a stored artifact path exists in config but the
  file has been deleted or moved?
- How does the app behave when a PR ID is known but repository context is
  missing?
- What happens when two commands attempt to update the same stored PR thread
  file in quick succession?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide one unified local configuration model for
  setup, work item retrieval, pull request retrieval, review workflows, and
  artifact indexing.
- **FR-002**: System MUST allow the user to set the Azure DevOps PAT directly.
- **FR-003**: System MUST allow the user to set the current organization either
  by direct org name or by choosing from organizations visible to the PAT.
- **FR-004**: System MUST allow the user to set the current project either by
  direct project identifier or by choosing from projects visible to the PAT in
  the selected organization.
- **FR-005**: System MUST retrieve work item context including the root work
  item, parent items, child items, first-level related items, and comments for
  each included item.
- **FR-006**: System MUST persist generated work item artifacts and index them by
  work item ID, organization, and project.
- **FR-007**: System MUST support viewing stored work item artifacts directly in
  the console.
- **FR-008**: System MUST list active pull requests created by the authenticated
  user.
- **FR-009**: System MUST retrieve and store pull request sessions including
  comment thread data and generated prompt artifacts.
- **FR-010**: System MUST persist pull request thread review state to disk so PR
  triage can be advanced one command at a time.
- **FR-011**: System MUST support listing stored threads for a pull request and
  showing one specific stored thread in the console.
- **FR-012**: System MUST support updating one thread at a time to `fix` or
  `no-fix`, with an optional fix instruction when the selected state is `fix`.
- **FR-013**: System MUST support generating and saving an AI-ready prompt from a
  stored pull request session only after all threads are classified.
- **FR-014**: System MUST support viewing stored pull request artifacts directly
  in the console.
- **FR-015**: System MUST filter default stored work item and pull request list
  views by the currently selected organization and project.
- **FR-016**: System MUST provide direct-command equivalents for all preserved
  menu workflows in MVP scope.
- **FR-017**: System MUST present a consistent ASCII art header across menu
  pages and direct command execution paths.
- **FR-018**: System MUST keep AI-facing JSON contracts stable and explicitly
  versioned when changed.

### Key Entities *(include if feature involves data)*

- **AppConfig**: Unified local configuration and artifact index for the
  application.
- **CurrentContext**: Active organization, project, and optional repository
  selection that scopes default behavior.
- **WorkItemEntry**: Indexed local record for a retrieved work item and its
  generated artifacts.
- **PullRequestEntry**: Indexed local record for a retrieved pull request session
  and its generated artifacts.
- **ArtifactRef**: Reference to a generated local file that can be listed or
  rendered later.
- **WorkItemGraph**: Normalized work item retrieval result with relationships,
  comments, and provenance.
- **PullRequestSession**: Stored representation of PR threads, review decisions,
  developer notes, and prompt-generation state.
- **PullRequestThreadState**: Persistent per-thread record containing the
  thread payload, review decision, optional fix instruction, and review
  timestamps.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time user can complete setup and establish a valid
  organization/project context in under 5 minutes without manual file editing.
- **SC-002**: The MVP can retrieve and store a valid work item context package
  and a valid pull request session using one shared config model.
- **SC-003**: A stored pull request can be reviewed one thread at a time across
  multiple command invocations without losing state.
- **SC-004**: Every MVP menu workflow has a documented direct-command
  equivalent.
- **SC-005**: Stored work-item and pull-request listings can be filtered to the
  current org/project context with no manual file navigation required.
- **SC-006**: Work-item and pull-request view commands render stored artifacts
  successfully in the console for representative happy-path samples.

## Assumptions

- The initial MVP targets one primary engineer per local install and does not
  need shared multi-user storage.
- The first implementation will be a .NET 10 console application using a
  console UI library, currently assumed to be `Spectre.Console`.
- Local file-based storage is acceptable for MVP and no database is required.
- Work item and pull request IDs are stable enough to act as local index keys
  within their org/project context.
- Repository selection may be required for some PR workflows even when org and
  project are already selected.

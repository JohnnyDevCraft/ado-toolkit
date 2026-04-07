<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Modified principles:
  - [PRINCIPLE_1_NAME] -> I. AI-Ready Contract Outputs
  - [PRINCIPLE_2_NAME] -> II. Dual-Surface CLI Experience
  - [PRINCIPLE_3_NAME] -> III. Complete Azure DevOps Context Retrieval
  - [PRINCIPLE_4_NAME] -> IV. Setup-First Engineer Experience
  - [PRINCIPLE_5_NAME] -> V. Secure, Observable Local Operation
- Added sections:
  - Architecture Constraints
  - Delivery Workflow
- Added command inventory:
  - Consolidated legacy workflow and command surface from `workitems` and `Commentor`
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ /Users/john/Source/repos/xelseor/ado-toolkit/.specify/templates/plan-template.md
  - ✅ /Users/john/Source/repos/xelseor/ado-toolkit/.specify/templates/tasks-template.md
  - ✅ /Users/john/Source/repos/xelseor/ado-toolkit/.specify/templates/spec-template.md
- Follow-up TODOs:
  - Confirm the intended console UI package. Current assumption: Spectre.Console.
-->

# ADO Toolkit Constitution

## Core Principles

### I. AI-Ready Contract Outputs
Every command that exists to support AI-assisted delivery MUST produce stable,
documented, machine-ingestible output shapes. JSON output is the source-of-truth
contract for automation paths, and human-readable output is a convenience layer.
Contract changes MUST be versioned, called out explicitly, and protected by
tests so downstream AI workflows do not break silently.

### II. Dual-Surface CLI Experience
The product MUST support both guided engineer workflows and direct automation.
Every important capability MUST be reachable through a discoverable menu-driven
experience and through explicit commands and arguments suitable for AI or shell
automation. Menu flows MUST map cleanly to underlying commands rather than
implementing separate business logic paths. If a workflow exists in the menu,
the toolkit MUST expose an equivalent direct command path for that workflow.

### III. Complete Azure DevOps Context Retrieval
Work-item retrieval features MUST gather the full first-order context required
for implementation work. When retrieving a work item, the toolkit MUST support
including the selected item, its parent items, its child items, first-level
related items, and comments attached to each included item. Output contracts
MUST make relationship type, provenance, and comment ownership explicit so AI
consumers can reason about the graph without guessing.

### IV. Setup-First Engineer Experience
The toolkit MUST be straightforward for a new engineer to install, configure,
and trust. Installation and bootstrap workflows MUST be optimized for local
developer use, including package installation, a setup command, validation of
required Azure DevOps configuration, and clear remediation guidance when setup
is incomplete. The first successful run for a new user MUST leave behind a
predictable, documented local configuration footprint. That configuration
footprint MUST unify prior per-app local storage into one application-level
configuration model.

### V. Secure, Observable Local Operation
Personal access tokens and other sensitive configuration MUST be handled with
least exposure in mind. Secrets MUST never be logged, written to output
artifacts, or echoed back in diagnostics. Commands that reach external systems
MUST emit enough structured telemetry and error context for troubleshooting
without leaking credentials or private work-item data beyond the requested
scope.

## Architecture Constraints

ADO Toolkit is a single-deployable .NET 10 console application. The initial
delivery model is a monolithic single-project application, but the internal code
structure MUST preserve clear separation between command surface, application
orchestration, Azure DevOps integration, local configuration management, and
output serialization.

The primary runtime is the command line. Interactive experiences SHOULD be
built on a console UI framework that supports polished menus, prompts, and help
flows. Current assumption: `Spectre.Console`; confirm before implementation
plans lock dependency choices.

The toolkit MUST be installable through Homebrew for engineer self-service.
Commands intended for AI use MUST remain scriptable, deterministic, and safe to
invoke non-interactively.

## Delivery Workflow

Work proceeds through SpecKit artifacts before implementation. Constitution,
feature specification, plan, and tasks remain the governing documents for new
capabilities.

The initial consolidated toolkit scope is based on two existing applications:
- `/Users/john/Source/repos/xelseor/workitems`
- `/Users/john/Source/repos/xelseor/Commentor`

The first ADO Toolkit command surface MUST preserve and unify the currently
implemented execution paths from those applications.

### Unified Command Inventory To Preserve And Consolidate

The unified toolkit should collapse shared configuration behavior into one
application-wide setup surface. One PAT serves the whole application, and the
default working assumption is that a user usually operates within one selected
organization and one selected project at a time.

Required direct commands:
- `ado --help`
- `ado --version`
- `ado setup`
- `ado config set-pat <PAT>`
- `ado config set-org <OrgName>`
- `ado config set-org`
- `ado config set-project <ProjectId|ProjectName>`
- `ado config set-project`
- `ado work-item get <WorkItemId>`
- `ado work-item get <WorkItemId> --out <OutputPath>`
- `ado work-item last`
- `ado work-item view <WorkItemId>`
- `ado pr list-active`
- `ado pr list-stored`
- `ado pr get <PullRequestId>`
- `ado pr refresh <PullRequestId>`
- `ado pr review <PullRequestId>`
- `ado pr generate-prompt <PullRequestId>`
- `ado pr view <PullRequestId>`

Required command behavior:
- `ado setup`
  - guided first-run workflow that ensures PAT, organization, project, and
    output/prompt paths are configured
- `ado config set-pat <PAT>`
  - stores the Azure DevOps PAT used by all commands in the application
- `ado config set-org <OrgName>`
  - sets the default Azure DevOps organization directly when the name is known
- `ado config set-org`
  - uses the configured PAT to discover accessible organizations and lets the
    user choose one interactively
- `ado config set-project <ProjectId|ProjectName>`
  - sets the default project directly when the identifier or name is known
- `ado config set-project`
  - uses the configured PAT and selected organization to list visible projects
    and lets the user choose one interactively
- `ado work-item get <WorkItemId>`
  - retrieves the work item graph and exports the normalized result
- `ado work-item get <WorkItemId> --out <OutputPath>`
  - performs the same retrieval using a one-off output path override
- `ado work-item last`
  - opens or displays the last export result reference
- `ado work-item view <WorkItemId>`
  - shows the contents of a previously stored work item export directly in the
    console
- `ado pr list-active`
  - lists active pull requests created by the authenticated user within the
    current organization, project, and repository selection flow
- `ado pr list-stored`
  - lists locally stored pull request sessions
- `ado pr get <PullRequestId>`
  - imports or refreshes a pull request by explicit ID using the current
    organization, project, and repository context
- `ado pr refresh <PullRequestId>`
  - refreshes the locally stored pull request comment and thread data
- `ado pr review <PullRequestId>`
  - walks the thread triage workflow and captures `fix` or `no fix` decisions
    plus optional developer notes
- `ado pr generate-prompt <PullRequestId>`
  - generates and writes the AI handoff prompt from the current stored pull
  request session, forcing a fresh review when required by workflow rules
- `ado pr view <PullRequestId>`
  - shows the contents of a stored pull request session and related generated
    prompt or export details directly in the console

Required interactive menu-equivalent workflows:
- guided setup
- PAT configuration
- organization selection
- project selection
- work item retrieval
- view last export result
- view stored work item content
- list active pull requests
- list stored pull requests
- get pull request by ID
- refresh pull request comments
- review pull request threads
- generate prompt and write prompt file
- view stored pull request content

### Menu Tree To Preserve

```text
Main Menu
├── Setup
│   ├── Run Guided Setup
│   ├── Set PAT
│   ├── Set Organization
│   │   ├── Enter Organization Name
│   │   └── Choose From Accessible Organizations
│   ├── Set Project
│   │   ├── Enter Project Name Or ID
│   │   └── Choose From Accessible Projects
│   └── Set Output And Prompt Paths
├── Work Items
│   ├── Get Work Item
│   ├── View Work Item
│   └── View Last Export Result
├── Pull Requests
│   ├── List My Active PRs
│   ├── List Stored PRs
│   │   ├── Review Threads
│   │   ├── Refresh Comments
│   │   └── Generate Prompt
│   └── Get PR By ID
│       ├── Review Threads
│       ├── Refresh Comments
│       └── Generate Prompt
│   └── View Pull Request
├── Help
└── Exit
```

### Unified Command Design Rules

- The new toolkit MUST define a consolidated command model that covers both
  legacy products without forcing users to know the old application names.
- Every workflow preserved from the interactive menus above MUST have a direct
  non-interactive command form, even if the legacy app only exposed it
  interactively.
- Direct commands SHOULD be organized by capability area rather than by legacy
  repository of origin.
- The menu system MUST be a discoverability layer over the same underlying
  command handlers used by direct invocation.

### Unified Local Configuration And Storage Rules

- The application MUST use one unified local storage root and one unified
  configuration file for both work item and pull request workflows.
- The unified configuration MUST store:
  - the Azure DevOps personal access token
  - the currently selected organization
  - the currently selected project
  - output and prompt path settings
  - a list of downloaded work items
  - a list of downloaded pull requests
- Each stored work item entry MUST include:
  - work item ID
  - title when available
  - organization
  - project
  - paths to generated artifacts such as markdown or JSON files
  - timestamps sufficient to sort and display recent activity
- Each stored pull request entry MUST include:
  - pull request ID
  - title when available
  - organization
  - project
  - repository identity when applicable
  - paths to generated artifacts such as prompt files, session files, markdown,
    or JSON outputs
  - timestamps sufficient to sort and display recent activity
- Stored work items and stored pull requests MUST be filterable by the current
  organization and project so the default list views reflect the active working
  context.
- The app MUST support showing the contents of stored artifacts directly in the
  console when the user selects a view action from the menu or invokes the
  equivalent direct command.

### Console Presentation Rules

- The application MUST present a deliberate, high-quality ASCII art header.
- That header MUST appear on every page of the interactive menu experience.
- The same header MUST also be rendered for direct command execution before the
  command output is shown.
- Header presentation MUST be consistent across help, version, setup, work
  item, pull request, and view commands.
- If terminal capabilities are limited, the header MUST degrade gracefully
  rather than becoming unreadable noise.

Every feature that introduces or changes an AI-facing output contract MUST
define:
- the JSON shape
- required and optional fields
- relationship semantics
- failure modes
- compatibility expectations

Quality gates for feature work:
- contract tests for AI-facing JSON outputs are mandatory
- integration tests are mandatory for Azure DevOps retrieval and auth-sensitive
  flows
- help text and menu affordances MUST be updated when commands change
- setup and bootstrap behavior MUST be validated from a fresh-user perspective

## Governance
This constitution supersedes lower-level project habits when conflicts arise.
All plans, tasks, reviews, and implemented features MUST be checked against
these principles.

Amendments require:
- a documented reason for change
- updates to impacted workflow artifacts and contracts
- an explicit version bump using semantic versioning for the constitution itself

Versioning policy:
- MAJOR for principle removals or materially incompatible governance changes
- MINOR for new principles or materially expanded requirements
- PATCH for clarification-only edits that do not change expected behavior

Compliance review expectations:
- every feature plan MUST include a constitution check
- every contract change MUST identify downstream AI impact
- every release candidate MUST verify setup, command help, and JSON contract
  behavior for affected commands

**Version**: 1.0.0 | **Ratified**: 2026-04-07 | **Last Amended**: 2026-04-07

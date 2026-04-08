# Feature Specification: Homebrew Distribution

**Feature Branch**: `002-homebrew-distribution`  
**Created**: 2026-04-07  
**Status**: Draft  
**Input**: User description: "Create a dedicated SpecKit feature for packaging ADO Toolkit for Homebrew so we can review the approach before creating a tap repo, release automation, or other shipping assets."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate Reviewable Homebrew Assets Locally (Priority: P1)

As the release engineer, I can generate the Homebrew package artifacts and
formula locally so I can review exactly what would be shipped before anything
is published.

**Why this priority**: This creates a safe approval boundary before any GitHub
repository, release, or tap automation is touched.

**Independent Test**: Run the local packaging flow from the repository and
verify it produces versioned release archives plus a formula file that matches
the chosen package name and executable contract.

**Acceptance Scenarios**:

1. **Given** a buildable ADO Toolkit workspace, **When** the maintainer runs
   the local packaging command, **Then** the system generates versioned Homebrew
   release archives for the supported macOS targets.
2. **Given** generated release archives, **When** the packaging flow computes
   checksums, **Then** the system generates a reviewable formula for the package
   name `ado-toolkit` that installs the `ado` executable.
3. **Given** the generated formula, **When** the maintainer reviews it,
   **Then** it clearly references the intended release asset names, homepage,
   test command, and package description.

---

### User Story 2 - Publish Repeatable GitHub Release Assets (Priority: P1)

As the release engineer, I can publish a tagged release that produces the same
Homebrew assets automatically so shipping does not depend on manual packaging.

**Why this priority**: Homebrew distribution is only trustworthy if the release
artifacts can be reproduced consistently from tags.

**Independent Test**: Trigger the release workflow for a version tag in a safe
environment and verify it produces both release archives and the matching
formula artifact.

**Acceptance Scenarios**:

1. **Given** a version tag for ADO Toolkit, **When** the release workflow runs,
   **Then** it builds the supported macOS distribution archives automatically.
2. **Given** the release archives, **When** the workflow completes,
   **Then** it also produces the matching formula file from the actual archive
   checksums.
3. **Given** a completed release workflow, **When** the maintainer inspects the
   outputs, **Then** the archive names and formula content align with the same
   version and package naming rules.

---

### User Story 3 - Maintain a Dedicated Tap Repository (Priority: P2)

As the release engineer, I can manage a dedicated Homebrew tap repository named
`homebrew-ado-toolkit` so users have a stable install source that is separate
from the application repository.

**Why this priority**: A dedicated tap is the clean shipping surface, but it
should only be created after the packaging and release behavior are approved.

**Independent Test**: Review the tap repository structure and confirm it can
host the generated `Formula/ado-toolkit.rb` file and installation instructions
without requiring manual restructuring later.

**Acceptance Scenarios**:

1. **Given** an approved packaging approach, **When** the maintainer creates the
   tap repository, **Then** the repository name MUST be `homebrew-ado-toolkit`.
2. **Given** the tap repository exists, **When** the maintainer places the
   generated formula into it, **Then** the formula path and repo naming follow
   standard Homebrew tap conventions.
3. **Given** a user wants to install the tool, **When** they follow the tap
   instructions, **Then** the installation path is documented clearly in the
   product documentation.

---

### User Story 4 - Preserve Approval Boundaries Before Publishing (Priority: P2)

As the project owner, I can review the Homebrew distribution design before any
new repositories, workflows, or publishing actions are triggered.

**Why this priority**: You explicitly want a review checkpoint before we create
   anything you do not want.

**Independent Test**: Review the spec and confirm it separates design,
generation, and publish-time actions with explicit approval gates.

**Acceptance Scenarios**:

1. **Given** the Homebrew feature is still in planning, **When** implementation
   has not been approved, **Then** no new GitHub repository is created.
2. **Given** the Homebrew feature is still in planning, **When** implementation
   has not been approved, **Then** no tap repository automation is executed.
3. **Given** the Homebrew feature is approved for implementation, **When** work
   begins, **Then** the implementation plan can clearly separate local asset
   generation from publish-time actions.

### Edge Cases

- What happens when the packaged executable name and the Homebrew formula name
  differ?
- What happens when one supported macOS target builds successfully and another
  one fails?
- How does the workflow behave when the release tag version does not match the
  application version metadata?
- What happens if the generated formula checksum does not match the uploaded
  release asset?
- How do we handle a future need to support Linux bottles or source builds
  without disrupting the initial tap design?
- What happens when the tap repository exists but the formula has not yet been
  updated for the latest release?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define a Homebrew distribution workflow as a separate
  feature from the core MVP implementation.
- **FR-002**: System MUST support local generation of versioned Homebrew release
  archives for the supported macOS targets before any publish action occurs.
- **FR-003**: System MUST generate a Homebrew formula for package name
  `ado-toolkit` that installs the `ado` executable.
- **FR-004**: System MUST ensure the generated formula includes a meaningful
  package description, homepage, version, archive URLs, checksums, install
  behavior, and a verification test.
- **FR-005**: System MUST define a repeatable tagged release workflow that
  generates the same archives and formula artifacts automatically.
- **FR-006**: System MUST keep release archive naming consistent across local
  packaging and CI packaging flows.
- **FR-007**: System MUST define the dedicated tap repository name as
  `homebrew-ado-toolkit`.
- **FR-008**: System MUST place the generated formula in a tap-compatible
  structure using `Formula/ado-toolkit.rb`.
- **FR-009**: System MUST document the maintainer workflow for generating,
  reviewing, and publishing Homebrew artifacts.
- **FR-010**: System MUST document the user installation path for the tap-based
  Homebrew package.
- **FR-011**: System MUST prevent implementation assumptions that create or
  mutate external GitHub repositories before explicit user approval.
- **FR-012**: System MUST keep the release packaging feature compatible with the
  existing .NET 10 console application layout and executable contract.

### Key Entities *(include if feature involves data)*

- **ReleaseArchive**: Versioned packaged binary artifact for a supported target
  platform.
- **HomebrewFormula**: Generated Ruby formula that describes how Homebrew
  installs and verifies ADO Toolkit.
- **ReleaseWorkflow**: Automated CI flow that builds release artifacts and the
  corresponding formula from a tagged version.
- **TapRepository**: Dedicated GitHub repository named `homebrew-ado-toolkit`
  that stores `Formula/ado-toolkit.rb`.
- **ReleaseVersion**: The version identifier shared across application metadata,
  archive names, formula content, and tag naming.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A maintainer can generate the Homebrew release archives and the
  formula locally in one documented workflow without editing files manually.
- **SC-002**: A tagged release can reproduce the same archive naming and
  formula structure automatically.
- **SC-003**: The generated formula can be reviewed independently before any tap
  repository creation or publish action occurs.
- **SC-004**: The tap repository naming and formula placement conventions are
  fully specified before implementation begins.

## Assumptions

- The first Homebrew shipping target is macOS, not Linux.
- The formula package name remains `ado-toolkit` even though the installed
  executable is `ado`.
- The dedicated tap will live in a separate GitHub repository rather than the
  main application repository.
- The tap repository will be named `homebrew-ado-toolkit`, following your
  requested naming convention.
- Actual GitHub repository creation, release publication, and tap mutation are
  out of scope for this spec review phase and only happen after approval.

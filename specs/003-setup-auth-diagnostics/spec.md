# Feature Specification: Setup Authentication Diagnostics

**Feature Branch**: `003-setup-auth-diagnostics`  
**Created**: 2026-04-07  
**Status**: Draft  
**Input**: User description: "When running `ado setup`, entering a PAT can fail with `Unable to resolve the authenticated Azure DevOps profile. HTTP 401:`. Create a new spec to address that bug so we can review it tomorrow."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fail Setup Gracefully On Invalid Or Under-Scoped PATs (Priority: P1)

As a software engineer, I can run `ado setup` with an invalid, expired,
insufficient, or under-scoped Azure DevOps PAT and immediately see a clear test
result before anything is saved.

**Why this priority**: This is a first-run blocker. If setup fails unclearly,
the toolkit feels broken before the user ever reaches its value.

**Independent Test**: Run `ado setup` with known bad and under-scoped PATs and
verify the app reports a friendly validation result immediately after PAT entry,
explains likely causes or missing permissions, does not persist the PAT, and
waits for an explicit key press before ending the failed setup run.

**Acceptance Scenarios**:

1. **Given** the user enters an invalid PAT during `ado setup`, **When** Azure
   DevOps returns `401 Unauthorized`, **Then** the toolkit reports that the PAT
   could not authenticate and suggests likely causes such as expiration,
   incorrect token value, or missing scopes.
2. **Given** the user enters a PAT with insufficient permissions, **When** the
   toolkit performs its capability test, **Then** the toolkit explains which
   required application capability could not be verified and that the token may
   not have the required Azure DevOps access.
3. **Given** setup fails during PAT validation, **When** the command exits,
   **Then** the toolkit does not claim setup succeeded.
4. **Given** PAT validation fails, **When** the failure message is shown,
   **Then** the toolkit waits for a key press before ending the app so the user
   can read the result.

---

### User Story 2 - Validate PATs Before Advancing Setup (Priority: P1)

As a software engineer, I can have the PAT tested immediately after entry so
the app does not continue into organization or project selection with an
unusable credential.

**Why this priority**: Early validation keeps setup predictable and prevents
confusing downstream failures.

**Independent Test**: Run `ado setup` with both valid and invalid PAT values
and verify the setup flow shows the PAT test result immediately, only persists
the PAT after successful validation, and only advances past PAT collection when
authentication and required permission checks actually succeed.

**Acceptance Scenarios**:

1. **Given** the user enters a valid PAT, **When** the toolkit validates it,
   **Then** setup shows a success result, persists the PAT, waits for a key
   press acknowledgement, and can continue into organization and project
   discovery.
2. **Given** the user enters an invalid PAT, **When** PAT validation fails,
   **Then** setup stops before organization selection, does not persist the PAT,
   surfaces a clear failure, and waits for a key press before ending the run.
3. **Given** the toolkit cannot reach the Azure DevOps auth endpoint,
   **When** PAT validation is attempted, **Then** the toolkit distinguishes
   connectivity or service failure from credential failure.
4. **Given** the user enters a PAT that authenticates but lacks required Azure
   DevOps permissions for this application, **When** the toolkit performs its
   capability checks, **Then** setup stops before organization selection and
   reports that the token is authenticated but not sufficient for toolkit use.

---

### User Story 3 - Preserve Safe Config State During Failed Setup (Priority: P2)

As a software engineer, I can retry setup after an auth failure without ending
up with a misleading partial configuration.

**Why this priority**: Setup should be recoverable and should not store a bad
PAT as if it were working.

**Independent Test**: Run setup with a bad PAT after a clean start and after an
existing valid config, then verify saved configuration remains accurate and
retriable, and that a newly entered PAT is saved only after it passes the test.

**Acceptance Scenarios**:

1. **Given** there is no existing config, **When** setup fails during PAT
   validation, **Then** the toolkit does not persist a misleading “configured”
   state.
2. **Given** there is an existing valid config, **When** setup is retried with a
   bad PAT, **Then** the toolkit does not silently destroy the previously valid
   working context unless the new PAT is successfully validated.
3. **Given** PAT validation fails, **When** the user reruns setup, **Then** the
   recovery path is clear and consistent.
4. **Given** there is no existing config, **When** a PAT passes validation,
   **Then** the toolkit persists the tested PAT before continuing to later setup
   steps.

### Edge Cases

- What happens when Azure DevOps returns `401` with an empty response body?
- What happens when the PAT contains accidental whitespace from copy/paste?
- What happens when the PAT authenticates to one endpoint but fails on a later
  organization or project discovery call because of missing scopes?
- What happens when the network call times out or DNS fails during PAT
  validation?
- How should setup behave if Azure DevOps is temporarily unavailable and
  returns `5xx` errors?
- Which minimum Azure DevOps capabilities should be tested up front to prove the
  PAT can use the toolkit's setup and work-item or pull-request flows?
- What key or prompt copy should be used for the required acknowledgement pause
  after a PAT test result is shown?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST validate the Azure DevOps PAT during `ado setup`
  before advancing to organization or project selection.
- **FR-002**: System MUST report authentication failures with a user-facing
  explanation instead of only surfacing a raw `401` response.
- **FR-003**: System MUST distinguish invalid credentials from connectivity or
  service availability failures where possible.
- **FR-004**: System MUST trim user-entered PAT input before validation.
- **FR-005**: System MUST avoid reporting setup success when PAT validation has
  failed.
- **FR-006**: System MUST preserve a safe configuration state when setup fails
  during authentication.
- **FR-007**: System MUST avoid overwriting an existing valid working context
  with an invalid PAT unless the new PAT is successfully validated.
- **FR-008**: System MUST provide actionable guidance for retrying setup after a
  PAT validation failure.
- **FR-009**: System MUST test the PAT immediately after the user enters it and
  show the test result before moving to the next setup step.
- **FR-010**: System MUST verify that the PAT has the minimum permissions
  required for the toolkit's supported Azure DevOps features before persisting
  it as the active PAT.
- **FR-011**: System MUST save a newly entered PAT only after the PAT has passed
  authentication and required capability checks.
- **FR-012**: System MUST require an explicit key press after displaying the PAT
  test result, both on success before continuing and on failure before ending
  the app.
- **FR-013**: System MUST report when a PAT is authenticated but lacks required
  permissions as a distinct outcome from invalid credentials and connectivity
  failures.
- **FR-014**: System MUST describe which application capability checks passed or
  failed during PAT validation in a form the user can act on.
- **FR-015**: Delivering this feature MUST advance the application version from
  `0.1.0` to `0.1.1` consistently across application version surfaces used by
  the CLI and packaging workflow.
- **FR-016**: Delivering this feature MUST update the ASCII-art splash or header
  presentation to display `Designed By: JohnnyDevCraft | Xelseor LLC 2026` and
  `Version: 0.1.1 | Build Date: 04/08/2026`.

### Key Entities *(include if feature involves data)*

- **SetupWorkflowState**: The transient state collected during setup before it is
  committed to config.
- **AuthenticationCheckResult**: The outcome of PAT validation, including
  success, failure category, verified capabilities, and user-facing guidance.
- **AppConfig**: The persisted toolkit configuration that must remain accurate
  when setup fails.
- **PatCapabilityCheck**: A required Azure DevOps operation or permission probe
  used to verify the PAT can support the toolkit's setup and runtime features.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A failed PAT validation during `ado setup` produces an actionable
  message that identifies authentication as the problem.
- **SC-002**: Setup does not advance into org/project selection when PAT
  validation fails.
- **SC-003**: Failed setup does not leave the toolkit in a misleading
  “configured” state.
- **SC-004**: A valid PAT still allows the setup flow to continue without
  regression.
- **SC-005**: A newly entered PAT is not written to persisted config until the
  PAT test succeeds.
- **SC-006**: The PAT test result is shown immediately after token entry and the
  user must acknowledge it before setup continues or exits.
- **SC-007**: Under-scoped PATs produce a distinct result that identifies the
  missing toolkit capability instead of being reported only as a generic auth
  failure.
- **SC-008**: The application splash or header shows the requested designer,
  company, version, and build-date lines consistently with the `0.1.1`
  release.

## Assumptions

- The current failure happens during the first Azure DevOps authenticated
  profile lookup in the setup flow.
- The setup bug should be fixed inside the existing .NET 10 console
  application, not by changing the external Azure DevOps service contract.
- We want better diagnostics and safer state handling, not a redesign of the
  overall setup UX in this slice.
- The toolkit can determine a practical minimum permission check by exercising
  the Azure DevOps calls already needed during setup and the first-order
  application features rather than by reading PAT scope metadata directly.
- The version bump for this feature should be treated as part of the same
  delivery slice so user-visible version output and release metadata stay in
  sync.

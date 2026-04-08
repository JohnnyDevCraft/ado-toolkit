# Feature Specification: Setup Authentication Diagnostics

**Feature Branch**: `003-setup-auth-diagnostics`  
**Created**: 2026-04-07  
**Status**: Draft  
**Input**: User description: "When running `ado setup`, entering a PAT can fail with `Unable to resolve the authenticated Azure DevOps profile. HTTP 401:`. Create a new spec to address that bug so we can review it tomorrow."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fail Setup Gracefully On Invalid PATs (Priority: P1)

As a software engineer, I can run `ado setup` with an invalid, expired, or
insufficient Azure DevOps PAT and get a clear actionable error instead of a
bare 401 message.

**Why this priority**: This is a first-run blocker. If setup fails unclearly,
the toolkit feels broken before the user ever reaches its value.

**Independent Test**: Run `ado setup` with a known bad PAT and verify the app
reports a friendly auth failure, explains likely causes, and does not leave the
user confused about the next step.

**Acceptance Scenarios**:

1. **Given** the user enters an invalid PAT during `ado setup`, **When** Azure
   DevOps returns `401 Unauthorized`, **Then** the toolkit reports that the PAT
   could not authenticate and suggests likely causes such as expiration,
   incorrect token value, or missing scopes.
2. **Given** the user enters a PAT with insufficient permissions, **When** the
   first authenticated profile lookup fails, **Then** the toolkit explains which
   operation failed and that the token may not have the required Azure DevOps
   access.
3. **Given** setup fails during PAT validation, **When** the command exits,
   **Then** the toolkit does not claim setup succeeded.

---

### User Story 2 - Validate PATs Before Advancing Setup (Priority: P1)

As a software engineer, I can have the PAT checked early in setup so the app
does not continue into organization or project selection with an unusable
credential.

**Why this priority**: Early validation keeps setup predictable and prevents
confusing downstream failures.

**Independent Test**: Run `ado setup` with both valid and invalid PAT values
and verify the setup flow only advances past PAT collection when authentication
actually succeeds.

**Acceptance Scenarios**:

1. **Given** the user enters a valid PAT, **When** the toolkit validates it,
   **Then** setup can continue into organization and project discovery.
2. **Given** the user enters an invalid PAT, **When** PAT validation fails,
   **Then** setup stops before organization selection and returns a clear
   failure.
3. **Given** the toolkit cannot reach the Azure DevOps auth endpoint,
   **When** PAT validation is attempted, **Then** the toolkit distinguishes
   connectivity or service failure from credential failure.

---

### User Story 3 - Preserve Safe Config State During Failed Setup (Priority: P2)

As a software engineer, I can retry setup after an auth failure without ending
up with a misleading partial configuration.

**Why this priority**: Setup should be recoverable and should not store a bad
PAT as if it were working.

**Independent Test**: Run setup with a bad PAT after a clean start and after an
existing valid config, then verify saved configuration remains accurate and
retriable.

**Acceptance Scenarios**:

1. **Given** there is no existing config, **When** setup fails during PAT
   validation, **Then** the toolkit does not persist a misleading “configured”
   state.
2. **Given** there is an existing valid config, **When** setup is retried with a
   bad PAT, **Then** the toolkit does not silently destroy the previously valid
   working context unless the new PAT is successfully validated.
3. **Given** PAT validation fails, **When** the user reruns setup, **Then** the
   recovery path is clear and consistent.

### Edge Cases

- What happens when Azure DevOps returns `401` with an empty response body?
- What happens when the PAT contains accidental whitespace from copy/paste?
- What happens when the PAT authenticates to one endpoint but fails on a later
  organization or project discovery call because of missing scopes?
- What happens when the network call times out or DNS fails during PAT
  validation?
- How should setup behave if Azure DevOps is temporarily unavailable and
  returns `5xx` errors?

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

### Key Entities *(include if feature involves data)*

- **SetupWorkflowState**: The transient state collected during setup before it is
  committed to config.
- **AuthenticationCheckResult**: The outcome of PAT validation, including
  success, failure category, and user-facing guidance.
- **AppConfig**: The persisted toolkit configuration that must remain accurate
  when setup fails.

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

## Assumptions

- The current failure happens during the first Azure DevOps authenticated
  profile lookup in the setup flow.
- The setup bug should be fixed inside the existing .NET 10 console
  application, not by changing the external Azure DevOps service contract.
- We want better diagnostics and safer state handling, not a redesign of the
  overall setup UX in this slice.

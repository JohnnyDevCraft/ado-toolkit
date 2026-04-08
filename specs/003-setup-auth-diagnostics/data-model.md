# Data Model: Setup Authentication Diagnostics

## AuthenticationCheckResult

**Purpose**: Captures the outcome of validating a PAT before setup persists it.

**Fields**:
- `IsSuccess`: boolean summary of whether setup may continue
- `FailureCategory`: enum-like value such as `InvalidCredentials`,
  `InsufficientPermissions`, `ConnectivityFailure`, or `ServiceFailure`
- `SummaryMessage`: concise user-facing result shown immediately after entry
- `Guidance`: actionable next-step text for retry or remediation
- `CapabilityChecks`: ordered collection of capability results

**Rules**:
- Must never include the PAT value
- Must always include a summary and guidance on failure
- Success implies all required capability checks passed

## PatCapabilityCheck

**Purpose**: Describes one required Azure DevOps operation used to prove the PAT
can support the toolkit.

**Fields**:
- `Name`: stable label for the capability being tested
- `Operation`: human-readable operation description
- `Passed`: whether the capability was verified
- `FailureDetail`: sanitized explanation when the capability fails

**Rules**:
- The set should include setup-required access and first-order toolkit access
- Failure details should be specific enough to act on without leaking secrets

## SetupWorkflowState

**Purpose**: Tracks transient setup values before they are committed to config.

**Fields**:
- `EnteredPat`: trimmed PAT held only long enough for validation
- `ValidationResult`: latest `AuthenticationCheckResult`
- `ShouldContinue`: whether setup may proceed to organization and project steps

**Rules**:
- Entered PAT must not be written to persisted config until validation passes
- Existing persisted config remains authoritative when validation fails

## AppVersionSurface

**Purpose**: Represents user-visible version metadata that must move to `0.1.1`
as part of this feature.

**Fields**:
- `ProjectVersion`: project and assembly metadata
- `CliDisplayVersion`: `ado --version` output
- `PackagingExpectations`: any release or packaging tests that encode the
  shipped version

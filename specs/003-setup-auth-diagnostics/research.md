# Research: Setup Authentication Diagnostics

## Decision 1: Validate the PAT through live Azure DevOps calls before saving it

**Rationale**: The failure the user sees today comes from a real authenticated
profile lookup, and the spec now requires immediate proof that the token works
for the toolkit before persistence. Using real Azure DevOps calls keeps the
result trustworthy.

**Alternatives considered**:
- Validate only that the PAT string is non-empty: rejected because it does not
  prove the token works.
- Save first and let later steps fail: rejected because it creates misleading
  config state.

## Decision 2: Treat capability verification as a small ordered probe sequence

**Rationale**: The app needs more than "token is accepted." It needs confidence
that the PAT supports setup and the toolkit's core Azure DevOps flows. A small
ordered sequence lets the app report which capability failed instead of showing
one generic message.

**Alternatives considered**:
- Probe every supported API surface in setup: rejected because it adds cost and
  complexity beyond the first-order capability checks this feature needs.
- Depend on PAT scope metadata introspection: rejected because the app already
  knows how to prove capability by calling the APIs it uses.

## Decision 3: Persist the PAT only after a successful validation result

**Rationale**: This directly satisfies the safe-state requirements in the spec
and avoids overwriting an existing valid context with a bad token.

**Alternatives considered**:
- Save to config and roll back on failure: rejected because rollback paths are
  easier to get wrong and create more opportunities for partial state.

## Decision 4: Pause on both success and failure with an explicit user action

**Rationale**: The user wants the validation result to be visible and
acknowledged before the flow continues or exits. That behavior belongs to the
guided setup UX, not as a side effect of a later crash or return path.

**Alternatives considered**:
- Auto-advance on success and auto-exit on failure: rejected because the result
  can be missed in a fast terminal flow.
- Pause only on failure: rejected because the user explicitly wants the result
  visible in both outcomes.

## Decision 5: Include the version bump in the same feature slice

**Rationale**: The user wants this feature to ship as `0.1.1`, and the project
already has multiple version surfaces that must stay aligned.

**Alternatives considered**:
- Handle the version bump in a later release-only task: rejected because it can
  drift from the code and tests changed by this feature.

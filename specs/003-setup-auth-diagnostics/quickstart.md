# Quickstart: Setup Authentication Diagnostics

## Scenario 1: Invalid PAT

1. Start from a clean config state.
2. Run `ado setup`.
3. Enter a known invalid PAT.
4. Verify the app immediately shows a failed PAT test result with guidance.
5. Verify the app waits for a key press before ending.
6. Verify config does not store the new PAT.

## Scenario 2: Under-Scoped PAT

1. Start from a clean config state or an existing valid config.
2. Run `ado setup`.
3. Enter a PAT that authenticates but cannot satisfy the toolkit's required
   capability checks.
4. Verify the app reports the token as insufficient for toolkit use rather than
   as a generic invalid credential.
5. Verify the app waits for a key press before ending.
6. Verify any previous valid config remains intact.

## Scenario 3: Valid PAT

1. Start from a clean config state.
2. Run `ado setup`.
3. Enter a valid PAT with the required capabilities.
4. Verify the app immediately shows a successful PAT test result.
5. Verify the app waits for a key press before continuing.
6. Verify the PAT is persisted only after the successful test.
7. Verify setup continues into organization and project selection.

## Scenario 4: Connectivity Failure

1. Simulate a network or service connectivity failure during PAT validation.
2. Run `ado setup`.
3. Enter a PAT.
4. Verify the app reports connectivity or service failure distinctly from
   invalid credentials.
5. Verify the app waits for a key press before ending and does not persist the
   PAT.

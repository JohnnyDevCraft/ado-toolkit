# Contract: Setup PAT Validation Result

## Trigger

The contract is exercised immediately after the user enters a PAT during
`ado setup`.

## Inputs

- Trimmed PAT value supplied through the guided setup prompt
- Existing `AppConfig` state

## Required Behavior

1. The app runs the PAT validation and capability checks before persisting the
   new PAT.
2. The app renders a result summary immediately after the test completes.
3. The app requires an explicit key press after the result is rendered.
4. On success:
   - the PAT is saved
   - setup may proceed to organization selection
5. On failure:
   - the PAT is not saved
   - existing valid config remains unchanged
   - setup ends after acknowledgement

## Result Categories

- `Success`
- `InvalidCredentials`
- `InsufficientPermissions`
- `ConnectivityFailure`
- `ServiceFailure`

## User-Facing Output Expectations

- Must identify the high-level result category
- Must include actionable guidance
- Must mention failed capabilities when permissions are insufficient
- Must not include the PAT value

## Version Alignment

This feature's implementation must leave the app reporting version `0.1.1`
through its user-visible version surfaces.

The splash or header presentation associated with the ASCII art must also show:
- `Designed By: JohnnyDevCraft | Xelseor LLC 2026`
- `Version: 0.1.1 | Build Date: 04/08/2026`

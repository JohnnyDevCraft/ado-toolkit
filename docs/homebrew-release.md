# Homebrew Release Flow

## Current Approved Scope

The currently approved slice covers local package generation and review:

- a packaging script at `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh`
- a formula template at `/Users/john/Source/repos/xelseor/ado-toolkit/packaging/homebrew/ado-toolkit.rb.template`
- versioned release archive naming rules
- a reviewable `ado-toolkit.rb` formula generated from local archive checksums

This slice packages self-contained macOS binaries for:

- `osx-arm64`
- `osx-x64`

It packages them as tarballs, computes checksums, and generates a formula locally. It does not create or mutate any external repository.

## Recommended Tap Setup

Create a dedicated tap repository:

- `JohnnyDevCraft/homebrew-ado-toolkit`

This repository is documented here for review only. It should not be created or modified until you explicitly approve that follow-on phase.

Homebrew users would install with:

```bash
brew tap JohnnyDevCraft/homebrew-ado-toolkit
brew install ado-toolkit
```

Store the generated formula in the tap as:

- `Formula/ado-toolkit.rb`

## Local Review Steps

1. Make sure `/Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj` has the correct version.
2. Commit the release changes.
3. Run the local packaging script.
4. Review the generated archives and `ado-toolkit.rb`.
5. Stop here and approve the next phase before any release or tap mutation happens.

## Local Dry Run

Run:

```bash
chmod +x /Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh
/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh
```

This will generate the same tarball names and a tap-ready formula locally.

## Notes

- The formula installs the `ado` executable.
- The formula test validates `ado --help`.
- `config reset` behavior and the menu-driven workflow are included in the shipped binary.
- The dedicated tap repository name is `homebrew-ado-toolkit`.
- Publishing tags, GitHub release automation, and tap repo updates are intentionally held for the next review phase.

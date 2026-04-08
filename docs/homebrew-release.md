# Homebrew Release Flow

## Scope

The currently approved slice covers local package generation and review:

- a packaging script at `/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh`
- a formula template at `/Users/john/Source/repos/xelseor/ado-toolkit/packaging/homebrew/ado-toolkit.rb.template`
- versioned release archive naming rules
- a reviewable `ado-toolkit.rb` formula generated from local archive checksums

The packaging and release design targets these macOS binaries first:

- `osx-arm64`
- `osx-x64`

The local packaging flow packages them as tarballs, computes checksums, and generates a formula locally. The release workflow then reproduces the same archive names and formula outputs from a version tag. Tap repository creation and formula publication remain explicit maintainer actions.

## Release Artifact Matrix

| Target | .NET RID | Archive Name |
|--------|----------|--------------|
| Apple Silicon macOS | `osx-arm64` | `ado-toolkit-<version>-macos-arm64.tar.gz` |
| Intel macOS | `osx-x64` | `ado-toolkit-<version>-macos-x64.tar.gz` |

Generated formula:

- `ado-toolkit.rb`

## Release Workflow

The repository includes release automation at:

- `/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/release.yml`

That workflow is designed to:

1. build self-contained binaries for both supported macOS targets
2. package the versioned archives
3. generate `ado-toolkit.rb` from the archive checksums
4. upload the release artifacts to the tagged GitHub release

The workflow assumes:

- application version in `/Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj`
- git tag format `v<version>`
- release archive names match the local packaging contract

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

## Tap Maintainer Handoff

Once a tagged release has produced the final formula:

1. open or clone `JohnnyDevCraft/homebrew-ado-toolkit`
2. place the generated formula at `Formula/ado-toolkit.rb`
3. commit the updated formula in the tap repository
4. push the tap repository update after reviewing the archive URLs and checksums

This handoff is intentionally explicit. The ADO Toolkit repository does not automatically create or mutate the tap repository.

## Local Review Steps

1. Make sure `/Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj` has the correct version.
2. Commit the release changes.
3. Run the local packaging script.
4. Review the generated archives and `ado-toolkit.rb`.
5. Stop here and approve the next phase before any release or tap mutation happens.

## Tagged Release Steps

1. confirm `/Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj` contains the intended version
2. commit the release changes in the application repository
3. push the application repository changes to the remote branch that will carry the release
4. create and push a tag like `v0.1.0`
5. wait for `/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/release.yml` to finish or trigger it deliberately and confirm it completed successfully
6. review the uploaded release archives and the generated `ado-toolkit.rb`
7. copy the generated formula into `JohnnyDevCraft/homebrew-ado-toolkit` under `Formula/ado-toolkit.rb`
8. commit and push the tap repository update after verifying URLs and checksums
9. only then treat the version as ready for `brew upgrade ado-toolkit`

## Project Release Rule

For this project, a version bump is not considered shipped when it exists only
in local code or local commits. Homebrew readiness requires both repositories
and the release automation to be updated:

1. the application repository code is pushed
2. the release tag is pushed
3. the GitHub Actions release workflow has produced the release assets
4. the Homebrew tap repository has the updated formula committed and pushed

If any one of those steps is missing, `brew upgrade ado-toolkit` may still
resolve to the previous version.

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
- The local packaging script does not create or mutate external repositories.
- Tap repository creation and updates are still explicit maintainer actions, even with release automation in place.

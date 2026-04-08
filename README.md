# ADO Toolkit

ADO Toolkit is a .NET 10 console application that bridges Azure DevOps context into engineer-friendly and AI-friendly workflows. It combines the proven work-item retrieval from `workitems` and the pull-request review workflow from `Commentor` into one command surface and one local storage model.

## What It Does

- Guided setup for PAT, organization, project, and repository context
- Rich work-item retrieval with parents, children, related items, and comments
- Pull-request import with persistent thread review state
- Thread-by-thread `fix` or `no-fix` decisions with optional fix instructions
- Prompt generation once every review thread has been classified
- Interactive menu access and direct command parity over the same workflow services

## Quick Start

```bash
dotnet run --project /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj -- setup
dotnet run --project /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj -- config set-pat <PAT>
dotnet run --project /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj -- config set-org
dotnet run --project /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj -- config set-project
dotnet run --project /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj -- config set-repo <RepoName> <RepoPath>
```

Run `ado` with no arguments to open the interactive menu.

## Homebrew Packaging

The repo is prepared to ship as a tap-friendly Homebrew package with macOS release archives, a generated formula, and a documented tap handoff to `homebrew-ado-toolkit`.

Local packaging:

```bash
chmod +x /Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh
/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh
```

That produces:

- `dist/homebrew/ado-toolkit-<version>-macos-arm64.tar.gz`
- `dist/homebrew/ado-toolkit-<version>-macos-x64.tar.gz`
- `dist/homebrew/ado-toolkit.rb`

Local packaging boundary:

- these outputs are generated locally for review first
- no tap repository is created by this packaging flow
- no GitHub release is created by this packaging flow

Release automation:

- tag a release as `v<version>`
- GitHub Actions builds both macOS archives
- the workflow generates a Homebrew formula from the archive checksums
- the workflow uploads both tarballs and the formula as release assets

Tap handoff:

- dedicated tap repo name: `homebrew-ado-toolkit`
- formula path in that repo: `Formula/ado-toolkit.rb`
- tap updates remain a deliberate maintainer step

User install path:

```bash
brew tap JohnnyDevCraft/homebrew-ado-toolkit
brew install ado-toolkit
```

Tap flow details live in [homebrew-release.md](/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md).

## Core Commands

```text
ado setup
ado config set-pat <PAT>
ado config set-org [OrgName]
ado config set-project [ProjectId|ProjectName]
ado config set-repo <RepoName> <RepoPath>
ado config reset
ado work-item get <WorkItemId> [--out <OutputPath>]
ado work-item view <WorkItemId>
ado work-item last
ado pr list-active
ado pr list-stored
ado pr get <PullRequestId>
ado pr refresh <PullRequestId>
ado pr review <PullRequestId>
ado pr threads <PullRequestId>
ado pr thread <PullRequestId> <ThreadId>
ado pr thread <PullRequestId> <ThreadId> set fix [--instruction <Instruction>]
ado pr thread <PullRequestId> <ThreadId> set no-fix
ado pr generate-prompt <PullRequestId>
ado pr view <PullRequestId>
```

## Local Storage

The toolkit keeps one unified config/index in the user storage root and stores generated artifacts separately:

- config and index metadata
- work-item JSON and markdown exports
- pull-request session files
- pull-request thread-state files
- generated prompt files

`config reset` clears only the active PAT and context. It does not delete stored work-item or pull-request artifacts.

## Verification

```bash
dotnet build /Users/john/Source/repos/xelseor/ado-toolkit/src/ado-toolkit.csproj
dotnet test /Users/john/Source/repos/xelseor/ado-toolkit/tests/ado-toolkit.tests.csproj
```

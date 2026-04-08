# Legacy Reuse Notes

## Overview

ADO Toolkit intentionally reuses the already-proven Azure DevOps retrieval logic from the legacy `workitems` and `Commentor` repositories. The goal is to preserve working integration knowledge while unifying command shape, storage, and user experience.

## `workitems` Reuse Map

- `Services/AzureDevOpsClient.cs`
  Reused as the base for work-item fetch and comment retrieval in `/Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsWorkItemClient.cs`.
- `Services/ReferenceParser.cs`
  Reused for first-order textual relation parsing in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemReferenceParser.cs`.
- `Services/WorkItemRetrievalService.cs`
  Reused as the backbone of parent, child, and related-item traversal in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemRetrievalService.cs`.
- `Services/MarkdownExportService.cs`
  Reused for markdown artifact composition in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/WorkItemArtifactWriter.cs`.
- `Models/*.cs`
  Reused and normalized in `/Users/john/Source/repos/xelseor/ado-toolkit/src/models/workitems/`.

## `Commentor` Reuse Map

- `Services/AzureDevOpsClient.cs`
  Reused for PR repository lookup, active PR listing, PR title retrieval, and thread/comment fetch in `/Users/john/Source/repos/xelseor/ado-toolkit/src/integrations/AzureDevOpsPullRequestClient.cs`.
- `Services/LocalStorageService.cs`
  Reused for persisted session and prompt storage in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestStorageService.cs`.
- `Services/PromptBuilder.cs`
  Reused for fix-plan prompt generation in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestPromptBuilder.cs`.
- `Services/CodeExcerptService.cs`
  Reused for repository excerpt extraction in `/Users/john/Source/repos/xelseor/ado-toolkit/src/services/PullRequestCodeExcerptService.cs`.
- `Workflows/MainMenuWorkflow.cs`
  Reused conceptually for the menu tree and stored PR actions in `/Users/john/Source/repos/xelseor/ado-toolkit/src/presentation/MainMenuWorkflow.cs`.
- `Models/*.cs`
  Reused and adapted into `/Users/john/Source/repos/xelseor/ado-toolkit/src/models/pullrequests/`.

## Consolidation Decisions

- PAT is unified at the application level instead of per legacy tool.
- One config/index file tracks both work items and pull requests.
- Interactive workflows and direct commands are routed through the same workflow bridge.
- PR review state is persisted per thread so AI and humans can advance the workflow one command at a time.
- Repository path is stored in current context and on indexed PR entries so code excerpts can be regenerated or refreshed.

using AdoToolkit.Commands;
using AdoToolkit.Config;
using AdoToolkit.Integrations;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;
using AdoToolkit.Models.WorkItems;
using AdoToolkit.Presentation;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Integration;

public sealed class MenuCommandParityTests
{
    [Fact]
    public async Task Workflow_bridge_and_direct_commands_produce_same_pr_review_outcome()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(home);
        var configService = new AppConfigService(appPaths, new JsonFileStore(), new SchemaValidationService());
        var config = await configService.LoadOrCreateAsync();
        config.Pat = "pat";
        config.CurrentContext.Organization = "org-one";
        config.CurrentContext.Project = new ProjectRef { Id = "proj-1", Name = "Project One" };
        config.CurrentContext.Repository = new RepositoryRef { Name = "repo-one", LocalPath = home };
        await configService.SaveAsync(config);

        var storage = new PullRequestStorageService(configService, new JsonFileStore(), appPaths, new PullRequestPromptBuilder());
        var importService = new PullRequestImportService(new FakePullRequestClient(), storage, new PullRequestCodeExcerptService());
        var reviewService = new PullRequestReviewService(storage);
        var command = new PullRequestCommands(importService, storage, reviewService, new ArtifactViewerService());
        var history = new PullRequestHistoryCommands(configService, storage, new ArtifactViewerService());
        var bridge = new CommandWorkflowBridge(
            new SetupCommand(new SetupWorkflowService(new CurrentContextService(configService, new FakeContextClient()), configService, new ConsoleOutputService(), new FakeSetupInteraction())),
            new ConfigCommands(new CurrentContextService(configService, new FakeContextClient())),
            new WorkItemCommands(new WorkItemRetrievalService(new FakeWorkItemClient(), new WorkItemReferenceParser()), new WorkItemArtifactWriter(), new JsonFileStore(), new WorkItemIndexService(configService)),
            new WorkItemHistoryCommands(new ArtifactViewerService()),
            command,
            history);

        var directSession = await command.GetAsync(config, 101);
        await command.SetThreadDecisionAsync(config, directSession, 1, "fix", "Use a guard clause");

        var bridgeSession = await bridge.LoadPullRequestSessionAsync(history.Find(config, 101));
        await bridge.SetThreadDecisionAsync(config, bridgeSession, 2, "no-fix");
        var prompt = await bridge.GeneratePromptAsync(config, bridgeSession);

        Assert.Contains("Thread ID: 1", prompt);
        Assert.DoesNotContain("Thread ID: 2", prompt);

        var reloaded = await configService.LoadOrCreateAsync();
        var entry = Assert.Single(reloaded.PullRequests);
        Assert.Equal("prompt-generated", entry.ReviewState);
    }

    [Fact]
    public async Task Workflow_bridge_and_direct_commands_produce_same_work_item_artifacts()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(home);
        var configService = new AppConfigService(appPaths, new JsonFileStore(), new SchemaValidationService());
        var config = await configService.LoadOrCreateAsync();
        config.Pat = "pat";
        config.CurrentContext.Organization = "org-one";
        config.CurrentContext.Project = new ProjectRef { Id = "proj-1", Name = "Project One" };
        await configService.SaveAsync(config);

        var workItemCommands = new WorkItemCommands(
            new WorkItemRetrievalService(new FakeWorkItemClient(), new WorkItemReferenceParser()),
            new WorkItemArtifactWriter(),
            new JsonFileStore(),
            new WorkItemIndexService(configService));

        var bridge = new CommandWorkflowBridge(
            new SetupCommand(new SetupWorkflowService(new CurrentContextService(configService, new FakeContextClient()), configService, new ConsoleOutputService(), new FakeSetupInteraction())),
            new ConfigCommands(new CurrentContextService(configService, new FakeContextClient())),
            workItemCommands,
            new WorkItemHistoryCommands(new ArtifactViewerService()),
            new PullRequestCommands(
                new PullRequestImportService(new FakePullRequestClient(), new PullRequestStorageService(configService, new JsonFileStore(), appPaths, new PullRequestPromptBuilder()), new PullRequestCodeExcerptService()),
                new PullRequestStorageService(configService, new JsonFileStore(), appPaths, new PullRequestPromptBuilder()),
                new PullRequestReviewService(new PullRequestStorageService(configService, new JsonFileStore(), appPaths, new PullRequestPromptBuilder())),
                new ArtifactViewerService()),
            new PullRequestHistoryCommands(configService, new PullRequestStorageService(configService, new JsonFileStore(), appPaths, new PullRequestPromptBuilder()), new ArtifactViewerService()));

        var directArtifacts = await workItemCommands.GetAsync(config, 321);
        var bridgeArtifacts = await bridge.GetWorkItemAsync(config, 321);

        Assert.True(File.Exists(directArtifacts.JsonPath));
        Assert.True(File.Exists(directArtifacts.MarkdownPath));
        Assert.Equal(await File.ReadAllTextAsync(directArtifacts.JsonPath), await File.ReadAllTextAsync(bridgeArtifacts.JsonPath));
        Assert.Equal(await File.ReadAllTextAsync(directArtifacts.MarkdownPath), await File.ReadAllTextAsync(bridgeArtifacts.MarkdownPath));
    }

    private sealed class FakeContextClient : IAzureDevOpsContextClient
    {
        public Task<AuthenticationCheckResult> ValidatePatAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthenticationCheckResult
            {
                IsSuccess = true,
                FailureCategory = AuthenticationFailureCategory.None,
                SummaryMessage = "PAT test succeeded.",
                Guidance = "Continue.",
                CapabilityChecks = [new PatCapabilityCheck { Name = "Authenticated profile lookup", Operation = "Resolve the authenticated Azure DevOps profile", Passed = true }],
                Organizations = [new AdoOrganizationInfo { Name = "org-one" }]
            });

        public Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoOrganizationInfo>>([new AdoOrganizationInfo { Name = "org-one" }]);

        public Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProjectRef>>([new ProjectRef { Id = "proj-1", Name = "Project One" }]);
    }

    private sealed class FakePullRequestClient : IAzureDevOpsPullRequestClient
    {
        public Task<AdoAuthenticatedUser?> GetAuthenticatedUserAsync(string organization, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<AdoAuthenticatedUser?>(new AdoAuthenticatedUser { Id = "user-1", DisplayName = "Tester", UniqueName = "tester@example.com" });

        public Task<IReadOnlyList<AdoRepositoryInfo>> GetRepositoriesAsync(string organization, string project, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoRepositoryInfo>>([new AdoRepositoryInfo { Id = "repo-1", Name = "repo-one" }]);

        public Task<IReadOnlyList<AdoPullRequestInfo>> GetActivePullRequestsCreatedByAsync(string organization, string project, string repositoryId, string pat, AdoAuthenticatedUser? authenticatedUser, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoPullRequestInfo>>([new AdoPullRequestInfo { Id = 101, Title = "PR 101", Status = "active", CreatedById = authenticatedUser?.Id }]);

        public Task<string?> GetPullRequestTitleAsync(string organization, string project, string repositoryId, int pullRequestId, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("PR 101");

        public Task<List<PullRequestThreadRecord>> GetPullRequestThreadsAsync(string organization, string project, string repositoryId, int pullRequestId, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<PullRequestThreadRecord>
            {
                new() { ThreadId = 1, FilePath = "/src/File1.cs", Line = 10, Comments = [new PullRequestCommentRecord { ThreadId = 1, CommentId = 1, Author = "Reviewer", Content = "Fix this" }] },
                new() { ThreadId = 2, FilePath = "/src/File2.cs", Line = 20, Comments = [new PullRequestCommentRecord { ThreadId = 2, CommentId = 2, Author = "Reviewer", Content = "Leave it" }] }
            });
    }

    private sealed class FakeWorkItemClient : IAzureDevOpsWorkItemClient
    {
        public Task<NormalizedWorkItem> GetWorkItemAsync(string organization, string pat, int workItemId, CancellationToken cancellationToken = default)
            => Task.FromResult(new NormalizedWorkItem
            {
                Id = workItemId,
                Title = "Add parity tests",
                ProjectName = "Project One",
                ProjectId = "proj-1",
                Organization = organization,
                WorkItemType = "User Story",
                State = "Active",
                DescriptionFields = [new DescriptionField { ReferenceName = "System.Description", DisplayName = "Description", Value = "Description" }],
                Comments = [new WorkItemComment { CommentId = "1", RenderedText = "Comment one" }]
            });

        public Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(string organization, string projectName, string pat, int workItemId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkItemComment>>([new WorkItemComment { CommentId = "1", RenderedText = "Comment one" }]);
    }

    private sealed class FakeSetupInteraction : ISetupInteraction
    {
        public string PromptSecret(string prompt) => "pat";

        public string PromptText(string prompt) => "/tmp";

        public void WaitForAcknowledgement(string message)
        {
        }
    }
}

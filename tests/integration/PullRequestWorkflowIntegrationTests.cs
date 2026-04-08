using AdoToolkit.Commands;
using AdoToolkit.Config;
using AdoToolkit.Integrations;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Integration;

public sealed class PullRequestWorkflowIntegrationTests
{
    [Fact]
    public async Task Pull_request_review_workflow_persists_thread_state_and_generates_prompt()
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
        var commands = new PullRequestCommands(importService, storage, reviewService, new ArtifactViewerService());

        var session = await commands.GetAsync(config, 101);
        Assert.Equal(2, session.Threads.Count);

        await commands.SetThreadDecisionAsync(config, session, 1, "fix", "Update the null handling");
        await commands.SetThreadDecisionAsync(config, session, 2, "no-fix");

        var prompt = await commands.GeneratePromptAsync(config, session);
        Assert.Contains("Thread ID: 1", prompt);
        Assert.DoesNotContain("Thread ID: 2", prompt);

        var reloaded = await configService.LoadOrCreateAsync();
        var entry = Assert.Single(reloaded.PullRequests);
        Assert.Equal("prompt-generated", entry.ReviewState);
        Assert.False(string.IsNullOrWhiteSpace(entry.ThreadsFilePath));
        Assert.Contains(entry.Artifacts, artifact => artifact.Kind == "pull-request-prompt");
    }

    [Fact]
    public async Task Generate_prompt_fails_when_unreviewed_threads_remain()
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
        var commands = new PullRequestCommands(importService, storage, reviewService, new ArtifactViewerService());

        var session = await commands.GetAsync(config, 101);
        await commands.SetThreadDecisionAsync(config, session, 1, "fix");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => commands.GeneratePromptAsync(config, session));
        Assert.Contains("Unreviewed threads remain", ex.Message);
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
                new()
                {
                    ThreadId = 1,
                    Status = "active",
                    FilePath = "/src/File1.cs",
                    Line = 10,
                    Comments = [new PullRequestCommentRecord { ThreadId = 1, CommentId = 1, Author = "Reviewer", Content = "Fix this thing" }]
                },
                new()
                {
                    ThreadId = 2,
                    Status = "active",
                    FilePath = "/src/File2.cs",
                    Line = 20,
                    Comments = [new PullRequestCommentRecord { ThreadId = 2, CommentId = 2, Author = "Reviewer", Content = "This is okay" }]
                }
            });
    }
}

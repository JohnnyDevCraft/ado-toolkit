using AdoToolkit.Commands;
using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Integration;

public sealed class StoredHistoryFilteringTests
{
    [Fact]
    public async Task Stored_history_filters_to_current_org_and_project()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(home);
        var configService = new AppConfigService(appPaths, new JsonFileStore(), new SchemaValidationService());
        var config = await configService.LoadOrCreateAsync();
        config.CurrentContext.Organization = "org-a";
        config.CurrentContext.Project = new ProjectRef { Name = "proj-a" };

        var wiA = Path.Combine(home, "wi-a.md");
        var wiB = Path.Combine(home, "wi-b.md");
        var prA = Path.Combine(home, "pr-a.txt");
        var prB = Path.Combine(home, "pr-b.txt");
        await File.WriteAllTextAsync(wiA, "work item a");
        await File.WriteAllTextAsync(wiB, "work item b");
        await File.WriteAllTextAsync(prA, "pull request a");
        await File.WriteAllTextAsync(prB, "pull request b");

        config.WorkItems.AddRange(
        [
            new WorkItemEntry
            {
                Id = 1,
                Title = "WI A",
                Organization = "org-a",
                Project = new ProjectRef { Name = "proj-a" },
                Artifacts = [new ArtifactRef { Kind = "markdown", Path = wiA, CreatedAt = DateTimeOffset.UtcNow }]
            },
            new WorkItemEntry
            {
                Id = 2,
                Title = "WI B",
                Organization = "org-b",
                Project = new ProjectRef { Name = "proj-b" },
                Artifacts = [new ArtifactRef { Kind = "markdown", Path = wiB, CreatedAt = DateTimeOffset.UtcNow }]
            }
        ]);

        config.PullRequests.AddRange(
        [
            new PullRequestEntry
            {
                Id = 11,
                Title = "PR A",
                Organization = "org-a",
                Project = new ProjectRef { Name = "proj-a" },
                Repository = new RepositoryRef { Name = "repo-a" },
                Artifacts = [new ArtifactRef { Kind = "pull-request-prompt", Path = prA, CreatedAt = DateTimeOffset.UtcNow }]
            },
            new PullRequestEntry
            {
                Id = 22,
                Title = "PR B",
                Organization = "org-b",
                Project = new ProjectRef { Name = "proj-b" },
                Repository = new RepositoryRef { Name = "repo-b" },
                Artifacts = [new ArtifactRef { Kind = "pull-request-prompt", Path = prB, CreatedAt = DateTimeOffset.UtcNow }]
            }
        ]);

        await configService.SaveAsync(config);

        var workItemHistory = new WorkItemHistoryCommands(new ArtifactViewerService());
        var pullRequestHistory = new PullRequestHistoryCommands(configService, new PullRequestStorageService(configService, new JsonFileStore(), appPaths, new PullRequestPromptBuilder()), new ArtifactViewerService());

        var workItems = workItemHistory.List(config);
        var pullRequests = pullRequestHistory.List(config);

        Assert.Single(workItems);
        Assert.Equal(1, workItems[0].Id);
        Assert.Single(pullRequests);
        Assert.Equal(11, pullRequests[0].Id);
        Assert.Equal("work item a", await workItemHistory.ViewAsync(config, 1));
        Assert.Equal("pull request a", await pullRequestHistory.ViewAsync(pullRequests[0]));
    }
}

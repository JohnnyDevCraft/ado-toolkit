using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Integration;

public sealed class SetupWorkflowTests
{
    [Fact]
    public async Task Set_pat_and_context_values_persist_to_config()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configService = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());
        var contextService = new CurrentContextService(configService, new FakeContextClient());
        var config = await configService.LoadOrCreateAsync();

        await contextService.SetPatAsync(config, "pat-value");
        await contextService.SetOrganizationAsync(config, "org-one");
        await contextService.SetProjectAsync(config, new ProjectRef { Id = "1", Name = "Project One" });

        var reloaded = await configService.LoadOrCreateAsync();
        Assert.Equal("pat-value", reloaded.Pat);
        Assert.Equal("org-one", reloaded.CurrentContext.Organization);
        Assert.Equal("Project One", reloaded.CurrentContext.Project?.Name);
    }

    [Fact]
    public async Task Reset_clears_active_context_but_keeps_indexes()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configService = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());
        var contextService = new CurrentContextService(configService, new FakeContextClient());
        var commands = new AdoToolkit.Commands.ConfigCommands(contextService);
        var config = await configService.LoadOrCreateAsync();

        config.Pat = "pat-value";
        config.CurrentContext.Organization = "org-one";
        config.CurrentContext.Project = new ProjectRef { Id = "1", Name = "Project One" };
        config.CurrentContext.Repository = new RepositoryRef { Name = "repo-one", LocalPath = "/tmp/repo-one" };
        config.WorkItems.Add(new WorkItemEntry
        {
            Id = 1,
            Organization = "org-one",
            Project = new ProjectRef { Id = "1", Name = "Project One" },
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        config.PullRequests.Add(new PullRequestEntry
        {
            Id = 10,
            Organization = "org-one",
            Project = new ProjectRef { Id = "1", Name = "Project One" },
            Repository = new RepositoryRef { Name = "repo-one", LocalPath = "/tmp/repo-one" },
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await configService.SaveAsync(config);

        await commands.ResetAsync(config);

        var reloaded = await configService.LoadOrCreateAsync();
        Assert.Null(reloaded.Pat);
        Assert.Null(reloaded.CurrentContext.Organization);
        Assert.Null(reloaded.CurrentContext.Project);
        Assert.Null(reloaded.CurrentContext.Repository);
        Assert.Single(reloaded.WorkItems);
        Assert.Single(reloaded.PullRequests);
    }

    private sealed class FakeContextClient : AdoToolkit.Integrations.IAzureDevOpsContextClient
    {
        public Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoOrganizationInfo>>([new() { Name = "org-one" }]);

        public Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProjectRef>>([new() { Id = "1", Name = "Project One" }]);
    }
}

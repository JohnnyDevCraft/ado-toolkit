using AdoToolkit.Commands;
using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Integration;

public sealed class ContextSelectionCommandTests
{
    [Fact]
    public async Task Set_project_uses_matching_name_or_id()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configService = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());
        var currentContextService = new CurrentContextService(configService, new FakeContextClient());
        var commands = new ConfigCommands(currentContextService);
        var config = await configService.LoadOrCreateAsync();

        config.Pat = "pat";
        config.CurrentContext.Organization = "org-one";

        await commands.SetProjectAsync(config, "Project Two");

        Assert.Equal("Project Two", config.CurrentContext.Project?.Name);
        Assert.Equal("2", config.CurrentContext.Project?.Id);
    }

    [Fact]
    public async Task Set_repo_stores_repository_name_and_path()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configService = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());
        var currentContextService = new CurrentContextService(configService, new FakeContextClient());
        var commands = new ConfigCommands(currentContextService);
        var config = await configService.LoadOrCreateAsync();

        await commands.SetRepositoryAsync(config, "repo-one", "/tmp/repo-one");

        Assert.Equal("repo-one", config.CurrentContext.Repository?.Name);
        Assert.Equal("/tmp/repo-one", config.CurrentContext.Repository?.LocalPath);
    }

    private sealed class FakeContextClient : AdoToolkit.Integrations.IAzureDevOpsContextClient
    {
        public Task<AuthenticationCheckResult> ValidatePatAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthenticationCheckResult
            {
                IsSuccess = true,
                FailureCategory = AuthenticationFailureCategory.None,
                SummaryMessage = "PAT test succeeded.",
                Guidance = "Continue.",
                CapabilityChecks = [],
                Organizations = [new AdoOrganizationInfo { Name = "org-one" }]
            });

        public Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoOrganizationInfo>>([new() { Name = "org-one" }]);

        public Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProjectRef>>(
                [
                    new() { Id = "1", Name = "Project One" },
                    new() { Id = "2", Name = "Project Two" }
                ]);
    }
}

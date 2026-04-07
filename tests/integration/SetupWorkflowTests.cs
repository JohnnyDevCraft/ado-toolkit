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

    private sealed class FakeContextClient : AdoToolkit.Integrations.IAzureDevOpsContextClient
    {
        public Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoOrganizationInfo>>([new() { Name = "org-one" }]);

        public Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProjectRef>>([new() { Id = "1", Name = "Project One" }]);
    }
}


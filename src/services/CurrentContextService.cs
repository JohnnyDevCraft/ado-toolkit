using AdoToolkit.Config;
using AdoToolkit.Integrations;
using AdoToolkit.Models;
using Spectre.Console;

namespace AdoToolkit.Services;

public sealed class CurrentContextService
{
    private readonly AppConfigService _configService;
    private readonly IAzureDevOpsContextClient _contextClient;

    public CurrentContextService(AppConfigService configService, IAzureDevOpsContextClient contextClient)
    {
        _configService = configService;
        _contextClient = contextClient;
    }

    public async Task SetPatAsync(AppConfig config, string pat, CancellationToken cancellationToken = default)
    {
        config.Pat = pat.Trim();
        await _configService.SaveAsync(config, cancellationToken);
    }

    public async Task SetOrganizationAsync(AppConfig config, string organization, CancellationToken cancellationToken = default)
    {
        config.CurrentContext.Organization = organization.Trim();
        config.CurrentContext.Project = null;
        config.CurrentContext.Repository = null;
        await _configService.SaveAsync(config, cancellationToken);
    }

    public async Task SetProjectAsync(AppConfig config, ProjectRef project, CancellationToken cancellationToken = default)
    {
        config.CurrentContext.Project = project;
        config.CurrentContext.Repository = null;
        await _configService.SaveAsync(config, cancellationToken);
    }

    public async Task SetRepositoryAsync(AppConfig config, RepositoryRef repository, CancellationToken cancellationToken = default)
    {
        config.CurrentContext.Repository = repository;
        await _configService.SaveAsync(config, cancellationToken);
    }

    public async Task ResetContextAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        config.Pat = null;
        config.CurrentContext.Organization = null;
        config.CurrentContext.Project = null;
        config.CurrentContext.Repository = null;
        await _configService.SaveAsync(config, cancellationToken);
    }

    public async Task<IReadOnlyList<AdoOrganizationInfo>> LoadOrganizationsAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.Pat))
        {
            throw new InvalidOperationException("Set a PAT before selecting an organization.");
        }

        return await _contextClient.ListOrganizationsAsync(config.Pat, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectRef>> LoadProjectsAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.CurrentContext.Organization))
        {
            throw new InvalidOperationException("Set an organization before selecting a project.");
        }

        if (string.IsNullOrWhiteSpace(config.Pat))
        {
            throw new InvalidOperationException("Set a PAT before selecting a project.");
        }

        return await _contextClient.ListProjectsAsync(config.CurrentContext.Organization, config.Pat, cancellationToken);
    }

    public AdoOrganizationInfo PromptForOrganization(IReadOnlyList<AdoOrganizationInfo> organizations)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose an Azure DevOps organization")
                .AddChoices(organizations.Select(item => item.Name)));

        return organizations.First(item => string.Equals(item.Name, choice, StringComparison.OrdinalIgnoreCase));
    }

    public ProjectRef PromptForProject(IReadOnlyList<ProjectRef> projects)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose an Azure DevOps project")
                .AddChoices(projects.Select(item => item.Name ?? string.Empty)));

        return projects.First(item => string.Equals(item.Name, choice, StringComparison.OrdinalIgnoreCase));
    }
}

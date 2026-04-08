using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class ConfigCommands
{
    private readonly CurrentContextService _currentContextService;

    public ConfigCommands(CurrentContextService currentContextService)
    {
        _currentContextService = currentContextService;
    }

    public Task SetPatAsync(AppConfig config, string pat, CancellationToken cancellationToken = default)
    {
        return _currentContextService.SetPatAsync(config, pat, cancellationToken);
    }

    public async Task SetOrganizationAsync(AppConfig config, string? organization, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(organization))
        {
            await _currentContextService.SetOrganizationAsync(config, organization, cancellationToken);
            return;
        }

        var organizations = await _currentContextService.LoadOrganizationsAsync(config, cancellationToken);
        if (organizations.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure DevOps organizations were found for the configured PAT.");
        }

        var choice = _currentContextService.PromptForOrganization(organizations);
        await _currentContextService.SetOrganizationAsync(config, choice.Name, cancellationToken);
    }

    public async Task SetProjectAsync(AppConfig config, string? projectInput, CancellationToken cancellationToken = default)
    {
        var projects = await _currentContextService.LoadProjectsAsync(config, cancellationToken);
        if (projects.Count == 0)
        {
            throw new InvalidOperationException("No visible Azure DevOps projects were found for the current organization.");
        }

        if (!string.IsNullOrWhiteSpace(projectInput))
        {
            var match = projects.FirstOrDefault(project =>
                string.Equals(project.Id, projectInput, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(project.Name, projectInput, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                throw new InvalidOperationException($"No visible project matched '{projectInput}'.");
            }

            await _currentContextService.SetProjectAsync(config, match, cancellationToken);
            return;
        }

        var choice = _currentContextService.PromptForProject(projects);
        await _currentContextService.SetProjectAsync(config, choice, cancellationToken);
    }

    public Task SetRepositoryAsync(AppConfig config, string repositoryName, string repositoryPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repositoryName))
        {
            throw new InvalidOperationException("Repository name is required.");
        }

        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            throw new InvalidOperationException("Repository path is required.");
        }

        var repository = new RepositoryRef
        {
            Name = repositoryName.Trim(),
            LocalPath = repositoryPath.Trim()
        };

        return _currentContextService.SetRepositoryAsync(config, repository, cancellationToken);
    }

    public Task ResetAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        return _currentContextService.ResetContextAsync(config, cancellationToken);
    }
}

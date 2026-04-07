using AdoToolkit.Config;
using AdoToolkit.Models;
using Spectre.Console;

namespace AdoToolkit.Services;

public sealed class SetupWorkflowService
{
    private readonly CurrentContextService _currentContextService;
    private readonly AppConfigService _configService;

    public SetupWorkflowService(CurrentContextService currentContextService, AppConfigService configService)
    {
        _currentContextService = currentContextService;
        _configService = configService;
    }

    public async Task RunAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.Pat))
        {
            var pat = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter your Azure DevOps PAT")
                    .Secret());
            await _currentContextService.SetPatAsync(config, pat, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(config.CurrentContext.Organization))
        {
            var organizations = await _currentContextService.LoadOrganizationsAsync(config, cancellationToken);
            if (organizations.Count > 0)
            {
                var organization = _currentContextService.PromptForOrganization(organizations);
                await _currentContextService.SetOrganizationAsync(config, organization.Name, cancellationToken);
            }
        }

        if (config.CurrentContext.Project is null || string.IsNullOrWhiteSpace(config.CurrentContext.Project.Name))
        {
            var projects = await _currentContextService.LoadProjectsAsync(config, cancellationToken);
            if (projects.Count > 0)
            {
                var project = _currentContextService.PromptForProject(projects);
                await _currentContextService.SetProjectAsync(config, project, cancellationToken);
            }
        }

        if (string.IsNullOrWhiteSpace(config.Settings.OutputRootPath))
        {
            config.Settings.OutputRootPath = AnsiConsole.Prompt(new TextPrompt<string>("Default output path"));
        }

        if (string.IsNullOrWhiteSpace(config.Settings.PromptRootPath))
        {
            config.Settings.PromptRootPath = AnsiConsole.Prompt(new TextPrompt<string>("Default prompt path"));
        }

        await _configService.SaveAsync(config, cancellationToken);
    }
}


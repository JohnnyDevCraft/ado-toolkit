using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Presentation;
using Spectre.Console;

namespace AdoToolkit.Services;

public sealed class SetupWorkflowService
{
    private readonly CurrentContextService _currentContextService;
    private readonly AppConfigService _configService;
    private readonly ConsoleOutputService _output;
    private readonly ISetupInteraction _interaction;

    public SetupWorkflowService(
        CurrentContextService currentContextService,
        AppConfigService configService,
        ConsoleOutputService output,
        ISetupInteraction interaction)
    {
        _currentContextService = currentContextService;
        _configService = configService;
        _output = output;
        _interaction = interaction;
    }

    public async Task RunAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AdoOrganizationInfo>? organizationsFromPatValidation = null;

        if (string.IsNullOrWhiteSpace(config.Pat))
        {
            var pat = _interaction.PromptSecret("Enter your Azure DevOps PAT");
            var validation = await _currentContextService.ValidatePatAsync(pat, cancellationToken);

            RenderPatValidationResult(validation);

            if (!validation.IsSuccess)
            {
                _interaction.WaitForAcknowledgement("Press enter to end setup.");
                throw new InvalidOperationException(validation.SummaryMessage);
            }

            _interaction.WaitForAcknowledgement("Press enter to save the PAT and continue setup.");
            await _currentContextService.SaveValidatedPatAsync(config, pat, cancellationToken);
            organizationsFromPatValidation = validation.Organizations;
        }

        if (string.IsNullOrWhiteSpace(config.CurrentContext.Organization))
        {
            var organizations = organizationsFromPatValidation ?? await _currentContextService.LoadOrganizationsAsync(config, cancellationToken);
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
            config.Settings.OutputRootPath = _interaction.PromptText("Default output path");
        }

        if (string.IsNullOrWhiteSpace(config.Settings.PromptRootPath))
        {
            config.Settings.PromptRootPath = _interaction.PromptText("Default prompt path");
        }

        await _configService.SaveAsync(config, cancellationToken);
    }

    private void RenderPatValidationResult(AuthenticationCheckResult validation)
    {
        if (validation.IsSuccess)
        {
            _output.WriteSuccess(validation.SummaryMessage);
        }
        else
        {
            _output.WriteError(validation.SummaryMessage);
        }

        foreach (var check in validation.CapabilityChecks)
        {
            var status = check.Passed ? "PASS" : "FAIL";
            var detail = check.Passed ? check.Operation : $"{check.Operation} - {check.FailureDetail}";
            _output.WriteInfo($"[{status}] {detail}");
        }

        if (!string.IsNullOrWhiteSpace(validation.Guidance))
        {
            _output.WriteWarning(validation.Guidance);
        }
    }
}

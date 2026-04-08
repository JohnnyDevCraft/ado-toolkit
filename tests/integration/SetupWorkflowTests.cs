using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Presentation;
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
    public async Task Guided_setup_validates_pat_before_persisting_and_waits_for_acknowledgement()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configService = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());
        var contextClient = new FakeContextClient
        {
            ValidationResult = new AuthenticationCheckResult
            {
                IsSuccess = true,
                FailureCategory = AuthenticationFailureCategory.None,
                SummaryMessage = "PAT test succeeded.",
                Guidance = "Press enter to continue.",
                CapabilityChecks =
                [
                    new PatCapabilityCheck
                    {
                        Name = "Authenticated profile lookup",
                        Operation = "Resolve the authenticated Azure DevOps profile",
                        Passed = true
                    }
                ],
                Organizations = [new AdoOrganizationInfo { Name = "org-one" }]
            }
        };
        var contextService = new CurrentContextService(configService, contextClient);
        var interaction = new FakeSetupInteraction
        {
            SecretResponses = new Queue<string>(["  good-pat  "]),
            TextResponses = new Queue<string>(["/tmp/out", "/tmp/prompts"])
        };
        var workflow = new SetupWorkflowService(contextService, configService, new ConsoleOutputService(), interaction);
        var config = await configService.LoadOrCreateAsync();
        config.CurrentContext.Organization = "org-one";
        config.CurrentContext.Project = new ProjectRef { Id = "1", Name = "Project One" };

        await workflow.RunAsync(config);

        var reloaded = await configService.LoadOrCreateAsync();
        Assert.Equal("good-pat", reloaded.Pat);
        Assert.Equal("org-one", reloaded.CurrentContext.Organization);
        Assert.Equal("Project One", reloaded.CurrentContext.Project?.Name);
        Assert.Equal(1, interaction.AcknowledgementCount);
        Assert.Equal(1, contextClient.ValidatePatCalls);
    }

    [Fact]
    public async Task Guided_setup_does_not_persist_failed_pat_and_waits_before_exiting()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configService = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());
        var contextClient = new FakeContextClient
        {
            ValidationResult = new AuthenticationCheckResult
            {
                IsSuccess = false,
                FailureCategory = AuthenticationFailureCategory.InsufficientPermissions,
                SummaryMessage = "PAT test failed.",
                Guidance = "Use a stronger PAT.",
                CapabilityChecks =
                [
                    new PatCapabilityCheck
                    {
                        Name = "Organization discovery",
                        Operation = "List accessible Azure DevOps organizations",
                        Passed = false,
                        FailureDetail = "Azure DevOps returned 403 during organization discovery."
                    }
                ]
            }
        };
        var contextService = new CurrentContextService(configService, contextClient);
        var interaction = new FakeSetupInteraction
        {
            SecretResponses = new Queue<string>(["bad-pat"])
        };
        var workflow = new SetupWorkflowService(contextService, configService, new ConsoleOutputService(), interaction);
        var config = await configService.LoadOrCreateAsync();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => workflow.RunAsync(config));

        var reloaded = await configService.LoadOrCreateAsync();
        Assert.Null(reloaded.Pat);
        Assert.Equal("PAT test failed.", error.Message);
        Assert.Equal(1, interaction.AcknowledgementCount);
        Assert.Equal(1, contextClient.ValidatePatCalls);
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
        public AuthenticationCheckResult ValidationResult { get; set; } = new()
        {
            IsSuccess = true,
            FailureCategory = AuthenticationFailureCategory.None,
            SummaryMessage = "PAT test succeeded.",
            Guidance = "Continue.",
            CapabilityChecks = [],
            Organizations = [new AdoOrganizationInfo { Name = "org-one" }]
        };

        public int ValidatePatCalls { get; private set; }

        public Task<AuthenticationCheckResult> ValidatePatAsync(string pat, CancellationToken cancellationToken = default)
        {
            ValidatePatCalls++;
            return Task.FromResult(ValidationResult);
        }

        public Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AdoOrganizationInfo>>([new() { Name = "org-one" }]);

        public Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProjectRef>>([new() { Id = "1", Name = "Project One" }]);
    }

    private sealed class FakeSetupInteraction : ISetupInteraction
    {
        public Queue<string> SecretResponses { get; init; } = new();

        public Queue<string> TextResponses { get; init; } = new();

        public int AcknowledgementCount { get; private set; }

        public string PromptSecret(string prompt)
        {
            return SecretResponses.Dequeue();
        }

        public string PromptText(string prompt)
        {
            return TextResponses.Dequeue();
        }

        public void WaitForAcknowledgement(string message)
        {
            AcknowledgementCount++;
        }
    }
}

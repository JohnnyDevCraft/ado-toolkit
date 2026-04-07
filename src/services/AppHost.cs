using AdoToolkit.Config;
using AdoToolkit.Integrations;
using AdoToolkit.Presentation;

namespace AdoToolkit.Services;

public sealed class AppHost
{
    public AppHost()
    {
        Paths = new AppPaths();
        JsonFileStore = new JsonFileStore();
        SchemaValidationService = new SchemaValidationService();
        ConfigService = new AppConfigService(Paths, JsonFileStore, SchemaValidationService);
        AuthService = new AzureDevOpsAuthService();
        HttpClientFactory = new AzureDevOpsHttpClientFactory();
        ContextClient = new AzureDevOpsContextClient(HttpClientFactory, AuthService);
        HeaderRenderer = new AppHeaderRenderer();
        Output = new ConsoleOutputService();
        CurrentContextService = new CurrentContextService(ConfigService, ContextClient);
        SetupWorkflowService = new SetupWorkflowService(CurrentContextService, ConfigService);
    }

    public AppPaths Paths { get; }

    public JsonFileStore JsonFileStore { get; }

    public SchemaValidationService SchemaValidationService { get; }

    public AppConfigService ConfigService { get; }

    public AzureDevOpsAuthService AuthService { get; }

    public AzureDevOpsHttpClientFactory HttpClientFactory { get; }

    public IAzureDevOpsContextClient ContextClient { get; }

    public AppHeaderRenderer HeaderRenderer { get; }

    public ConsoleOutputService Output { get; }

    public CurrentContextService CurrentContextService { get; }

    public SetupWorkflowService SetupWorkflowService { get; }
}


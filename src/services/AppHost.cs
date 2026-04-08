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
        WorkItemClient = new AzureDevOpsWorkItemClient(HttpClientFactory);
        PullRequestClient = new AzureDevOpsPullRequestClient(HttpClientFactory);
        HeaderRenderer = new AppHeaderRenderer();
        PageLayout = new PageLayout(HeaderRenderer);
        Output = new ConsoleOutputService();
        CurrentContextService = new CurrentContextService(ConfigService, ContextClient);
        SetupWorkflowService = new SetupWorkflowService(CurrentContextService, ConfigService);
        WorkItemReferenceParser = new WorkItemReferenceParser();
        WorkItemRetrievalService = new WorkItemRetrievalService(WorkItemClient, WorkItemReferenceParser);
        WorkItemArtifactWriter = new WorkItemArtifactWriter();
        WorkItemIndexService = new WorkItemIndexService(ConfigService);
        PullRequestPromptBuilder = new PullRequestPromptBuilder();
        PullRequestStorageService = new PullRequestStorageService(ConfigService, JsonFileStore, Paths, PullRequestPromptBuilder);
        PullRequestCodeExcerptService = new PullRequestCodeExcerptService();
        PullRequestImportService = new PullRequestImportService(PullRequestClient, PullRequestStorageService, PullRequestCodeExcerptService);
        PullRequestReviewService = new PullRequestReviewService(PullRequestStorageService);
        ArtifactViewerService = new ArtifactViewerService();
        var setupCommand = new Commands.SetupCommand(SetupWorkflowService);
        var configCommands = new Commands.ConfigCommands(CurrentContextService);
        var workItemCommands = new Commands.WorkItemCommands(WorkItemRetrievalService, WorkItemArtifactWriter, JsonFileStore, WorkItemIndexService);
        var workItemHistoryCommands = new Commands.WorkItemHistoryCommands(ArtifactViewerService);
        var pullRequestCommands = new Commands.PullRequestCommands(PullRequestImportService, PullRequestStorageService, PullRequestReviewService, ArtifactViewerService);
        var pullRequestHistoryCommands = new Commands.PullRequestHistoryCommands(ConfigService, PullRequestStorageService, ArtifactViewerService);
        WorkflowBridge = new CommandWorkflowBridge(setupCommand, configCommands, workItemCommands, workItemHistoryCommands, pullRequestCommands, pullRequestHistoryCommands);
        MainMenuWorkflow = new MainMenuWorkflow(PageLayout, WorkflowBridge, Output);
    }

    public AppPaths Paths { get; }

    public JsonFileStore JsonFileStore { get; }

    public SchemaValidationService SchemaValidationService { get; }

    public AppConfigService ConfigService { get; }

    public AzureDevOpsAuthService AuthService { get; }

    public AzureDevOpsHttpClientFactory HttpClientFactory { get; }

    public IAzureDevOpsContextClient ContextClient { get; }

    public IAzureDevOpsWorkItemClient WorkItemClient { get; }

    public IAzureDevOpsPullRequestClient PullRequestClient { get; }

    public AppHeaderRenderer HeaderRenderer { get; }

    public PageLayout PageLayout { get; }

    public ConsoleOutputService Output { get; }

    public CurrentContextService CurrentContextService { get; }

    public SetupWorkflowService SetupWorkflowService { get; }

    public WorkItemReferenceParser WorkItemReferenceParser { get; }

    public WorkItemRetrievalService WorkItemRetrievalService { get; }

    public WorkItemArtifactWriter WorkItemArtifactWriter { get; }

    public WorkItemIndexService WorkItemIndexService { get; }

    public PullRequestPromptBuilder PullRequestPromptBuilder { get; }

    public PullRequestStorageService PullRequestStorageService { get; }

    public PullRequestCodeExcerptService PullRequestCodeExcerptService { get; }

    public PullRequestImportService PullRequestImportService { get; }

    public PullRequestReviewService PullRequestReviewService { get; }

    public ArtifactViewerService ArtifactViewerService { get; }

    public CommandWorkflowBridge WorkflowBridge { get; }

    public MainMenuWorkflow MainMenuWorkflow { get; }
}

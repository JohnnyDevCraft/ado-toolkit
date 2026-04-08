using AdoToolkit.Commands;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Services;

public sealed class CommandWorkflowBridge
{
    private readonly SetupCommand _setupCommand;
    private readonly ConfigCommands _configCommands;
    private readonly WorkItemCommands _workItemCommands;
    private readonly WorkItemHistoryCommands _workItemHistoryCommands;
    private readonly PullRequestCommands _pullRequestCommands;
    private readonly PullRequestHistoryCommands _pullRequestHistoryCommands;

    public CommandWorkflowBridge(
        SetupCommand setupCommand,
        ConfigCommands configCommands,
        WorkItemCommands workItemCommands,
        WorkItemHistoryCommands workItemHistoryCommands,
        PullRequestCommands pullRequestCommands,
        PullRequestHistoryCommands pullRequestHistoryCommands)
    {
        _setupCommand = setupCommand;
        _configCommands = configCommands;
        _workItemCommands = workItemCommands;
        _workItemHistoryCommands = workItemHistoryCommands;
        _pullRequestCommands = pullRequestCommands;
        _pullRequestHistoryCommands = pullRequestHistoryCommands;
    }

    public Task RunSetupAsync(AppConfig config, CancellationToken cancellationToken = default)
        => _setupCommand.ExecuteAsync(config, cancellationToken);

    public Task SetPatAsync(AppConfig config, string pat, CancellationToken cancellationToken = default)
        => _configCommands.SetPatAsync(config, pat, cancellationToken);

    public Task SetOrganizationAsync(AppConfig config, string? organization, CancellationToken cancellationToken = default)
        => _configCommands.SetOrganizationAsync(config, organization, cancellationToken);

    public Task SetProjectAsync(AppConfig config, string? project, CancellationToken cancellationToken = default)
        => _configCommands.SetProjectAsync(config, project, cancellationToken);

    public Task SetRepositoryAsync(AppConfig config, string repositoryName, string repositoryPath, CancellationToken cancellationToken = default)
        => _configCommands.SetRepositoryAsync(config, repositoryName, repositoryPath, cancellationToken);

    public Task ResetAsync(AppConfig config, CancellationToken cancellationToken = default)
        => _configCommands.ResetAsync(config, cancellationToken);

    public Task<(string JsonPath, string MarkdownPath)> GetWorkItemAsync(AppConfig config, int workItemId, string? outputOverride = null, CancellationToken cancellationToken = default)
        => _workItemCommands.GetAsync(config, workItemId, outputOverride, cancellationToken);

    public IReadOnlyList<WorkItemEntry> ListWorkItems(AppConfig config)
        => _workItemHistoryCommands.List(config);

    public WorkItemEntry GetLastWorkItem(AppConfig config)
        => _workItemHistoryCommands.GetLast(config);

    public Task<string> ViewWorkItemAsync(AppConfig config, int workItemId, CancellationToken cancellationToken = default)
        => _workItemHistoryCommands.ViewAsync(config, workItemId, cancellationToken);

    public Task<IReadOnlyList<AdoPullRequestInfo>> ListActivePullRequestsAsync(AppConfig config, CancellationToken cancellationToken = default)
        => _pullRequestCommands.ListActiveAsync(config, cancellationToken);

    public IReadOnlyList<PullRequestEntry> ListStoredPullRequests(AppConfig config)
        => _pullRequestHistoryCommands.List(config);

    public PullRequestEntry FindStoredPullRequest(AppConfig config, int pullRequestId)
        => _pullRequestHistoryCommands.Find(config, pullRequestId);

    public Task<PullRequestSession> GetPullRequestAsync(AppConfig config, int pullRequestId, CancellationToken cancellationToken = default)
        => _pullRequestCommands.GetAsync(config, pullRequestId, cancellationToken);

    public Task<PullRequestSession> RefreshPullRequestAsync(AppConfig config, PullRequestEntry entry, CancellationToken cancellationToken = default)
        => _pullRequestCommands.RefreshAsync(config, entry, cancellationToken);

    public Task<PullRequestSession> LoadPullRequestSessionAsync(PullRequestEntry entry, CancellationToken cancellationToken = default)
        => _pullRequestHistoryCommands.LoadSessionAsync(entry, cancellationToken);

    public Task SetThreadDecisionAsync(AppConfig config, PullRequestSession session, int threadId, string decision, string? instruction = null, CancellationToken cancellationToken = default)
        => _pullRequestCommands.SetThreadDecisionAsync(config, session, threadId, decision, instruction, cancellationToken);

    public IReadOnlyList<int> GetUnreviewedThreadIds(PullRequestSession session)
        => _pullRequestCommands.GetUnreviewedThreadIds(session);

    public Task<string> GeneratePromptAsync(AppConfig config, PullRequestSession session, CancellationToken cancellationToken = default)
        => _pullRequestCommands.GeneratePromptAsync(config, session, cancellationToken);

    public Task<string> ViewPullRequestAsync(PullRequestEntry entry, CancellationToken cancellationToken = default)
        => _pullRequestCommands.ViewAsync(entry, cancellationToken);
}

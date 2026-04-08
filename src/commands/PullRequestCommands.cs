using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class PullRequestCommands
{
    private readonly PullRequestImportService _importService;
    private readonly PullRequestStorageService _storageService;
    private readonly PullRequestReviewService _reviewService;
    private readonly ArtifactViewerService _artifactViewerService;

    public PullRequestCommands(PullRequestImportService importService, PullRequestStorageService storageService, PullRequestReviewService reviewService, ArtifactViewerService artifactViewerService)
    {
        _importService = importService;
        _storageService = storageService;
        _reviewService = reviewService;
        _artifactViewerService = artifactViewerService;
    }

    public Task<IReadOnlyList<AdoPullRequestInfo>> ListActiveAsync(AppConfig config, CancellationToken cancellationToken = default)
        => _importService.ListActiveAsync(config, cancellationToken);

    public Task<PullRequestSession> GetAsync(AppConfig config, int pullRequestId, CancellationToken cancellationToken = default)
        => _importService.ImportAsync(config, pullRequestId, cancellationToken);

    public Task<PullRequestSession> RefreshAsync(AppConfig config, PullRequestEntry entry, CancellationToken cancellationToken = default)
        => _importService.RefreshAsync(config, entry, cancellationToken);

    public Task SetThreadDecisionAsync(AppConfig config, PullRequestSession session, int threadId, string decision, string? instruction = null, CancellationToken cancellationToken = default)
        => _reviewService.SetDecisionAsync(config, session, threadId, decision, instruction, cancellationToken);

    public IReadOnlyList<int> GetUnreviewedThreadIds(PullRequestSession session)
        => _reviewService.GetUnreviewedThreadIds(session);

    public Task<string> GeneratePromptAsync(AppConfig config, PullRequestSession session, CancellationToken cancellationToken = default)
        => _reviewService.GeneratePromptAsync(config, session, cancellationToken);

    public async Task<string> ViewAsync(PullRequestEntry entry, CancellationToken cancellationToken = default)
    {
        var path = _artifactViewerService.ResolvePreferredArtifactPath(entry);
        return await _artifactViewerService.ReadAsync(path, cancellationToken);
    }
}

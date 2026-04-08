using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class PullRequestHistoryCommands
{
    private readonly AppConfigService _configService;
    private readonly PullRequestStorageService _storageService;
    private readonly ArtifactViewerService _artifactViewerService;

    public PullRequestHistoryCommands(AppConfigService configService, PullRequestStorageService storageService, ArtifactViewerService artifactViewerService)
    {
        _configService = configService;
        _storageService = storageService;
        _artifactViewerService = artifactViewerService;
    }

    public IReadOnlyList<PullRequestEntry> List(AppConfig config)
    {
        return config.PullRequests
            .Where(item => MatchesContext(config, item.Organization, item.Project.Name))
            .OrderByDescending(item => item.UpdatedAt)
            .ToList();
    }

    public PullRequestEntry Find(AppConfig config, int pullRequestId)
    {
        return List(config).FirstOrDefault(item => item.Id == pullRequestId)
               ?? throw new InvalidOperationException($"No stored pull request {pullRequestId} was found for the current context.");
    }

    public async Task<PullRequestSession> LoadSessionAsync(PullRequestEntry entry, CancellationToken cancellationToken = default)
    {
        return await _storageService.LoadSessionAsync(entry, cancellationToken)
               ?? throw new InvalidOperationException($"Stored session for pull request {entry.Id} could not be loaded.");
    }

    public async Task<string> ViewAsync(PullRequestEntry entry, CancellationToken cancellationToken = default)
    {
        return await _artifactViewerService.ReadAsync(_artifactViewerService.ResolvePreferredArtifactPath(entry), cancellationToken);
    }

    private static bool MatchesContext(AppConfig config, string organization, string? projectName)
    {
        if (!string.IsNullOrWhiteSpace(config.CurrentContext.Organization) &&
            !string.Equals(config.CurrentContext.Organization, organization, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(config.CurrentContext.Project?.Name) &&
            !string.Equals(config.CurrentContext.Project.Name, projectName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}


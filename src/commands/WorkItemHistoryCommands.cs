using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class WorkItemHistoryCommands
{
    private readonly ArtifactViewerService _artifactViewerService;

    public WorkItemHistoryCommands(ArtifactViewerService artifactViewerService)
    {
        _artifactViewerService = artifactViewerService;
    }

    public IReadOnlyList<WorkItemEntry> List(AppConfig config)
    {
        return config.WorkItems
            .Where(item => MatchesContext(config, item.Organization, item.Project.Name))
            .OrderByDescending(item => item.UpdatedAt)
            .ToList();
    }

    public WorkItemEntry GetLast(AppConfig config)
    {
        return List(config).FirstOrDefault()
               ?? throw new InvalidOperationException("No stored work items were found for the current context.");
    }

    public async Task<string> ViewAsync(AppConfig config, int workItemId, CancellationToken cancellationToken = default)
    {
        var entry = List(config).FirstOrDefault(item => item.Id == workItemId)
                    ?? throw new InvalidOperationException($"No stored work item {workItemId} was found for the current context.");
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


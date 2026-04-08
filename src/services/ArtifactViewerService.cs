using AdoToolkit.Models;

namespace AdoToolkit.Services;

public sealed class ArtifactViewerService
{
    public async Task<string> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Stored artifact not found at {path}.");
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public string ResolvePreferredArtifactPath(PullRequestEntry entry)
    {
        return entry.LastViewedArtifactPath
               ?? entry.Artifacts.OrderByDescending(artifact => artifact.UpdatedAt ?? artifact.CreatedAt).FirstOrDefault()?.Path
               ?? throw new InvalidOperationException($"No stored artifact was found for pull request {entry.Id}.");
    }

    public string ResolvePreferredArtifactPath(WorkItemEntry entry)
    {
        return entry.LastViewedArtifactPath
               ?? entry.Artifacts.OrderByDescending(artifact => artifact.UpdatedAt ?? artifact.CreatedAt).FirstOrDefault()?.Path
               ?? throw new InvalidOperationException($"No stored artifact was found for work item {entry.Id}.");
    }
}


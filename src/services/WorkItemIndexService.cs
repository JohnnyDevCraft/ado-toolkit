using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Models.WorkItems;

namespace AdoToolkit.Services;

public sealed class WorkItemIndexService
{
    private readonly AppConfigService _configService;

    public WorkItemIndexService(AppConfigService configService)
    {
        _configService = configService;
    }

    public async Task IndexAsync(AppConfig config, RetrievalResult result, string jsonPath, string markdownPath, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var existing = config.WorkItems.FirstOrDefault(item =>
            item.Id == result.RootWorkItem.Id &&
            string.Equals(item.Organization, result.RootWorkItem.Organization, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.Project.Name, result.RootWorkItem.ProjectName, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            existing = new WorkItemEntry
            {
                Id = result.RootWorkItem.Id,
                Title = result.RootWorkItem.Title,
                Organization = result.RootWorkItem.Organization,
                Project = new ProjectRef
                {
                    Id = result.RootWorkItem.ProjectId,
                    Name = result.RootWorkItem.ProjectName
                },
                CreatedAt = now,
                UpdatedAt = now
            };
            config.WorkItems.Add(existing);
        }

        existing.Title = result.RootWorkItem.Title;
        existing.UpdatedAt = now;
        existing.LastViewedArtifactPath = markdownPath;
        existing.Artifacts = UpsertArtifacts(existing.Artifacts, jsonPath, markdownPath, now);

        await _configService.SaveAsync(config, cancellationToken);
    }

    private static List<ArtifactRef> UpsertArtifacts(List<ArtifactRef> artifacts, string jsonPath, string markdownPath, DateTimeOffset timestamp)
    {
        Upsert(artifacts, "work-item-json", jsonPath, "application/json", timestamp);
        Upsert(artifacts, "work-item-markdown", markdownPath, "text/markdown", timestamp);
        return artifacts.OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt).ToList();
    }

    private static void Upsert(List<ArtifactRef> artifacts, string kind, string path, string contentType, DateTimeOffset timestamp)
    {
        var existing = artifacts.FirstOrDefault(item => item.Kind == kind && string.Equals(item.Path, path, StringComparison.Ordinal));
        if (existing is null)
        {
            artifacts.Add(new ArtifactRef
            {
                Kind = kind,
                Path = path,
                ContentType = contentType,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
            return;
        }

        existing.ContentType = contentType;
        existing.UpdatedAt = timestamp;
    }
}


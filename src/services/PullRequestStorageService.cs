using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Services;

public sealed class PullRequestStorageService
{
    private readonly AppConfigService _configService;
    private readonly JsonFileStore _store;
    private readonly AppPaths _paths;
    private readonly PullRequestPromptBuilder _promptBuilder;

    public PullRequestStorageService(AppConfigService configService, JsonFileStore store, AppPaths paths, PullRequestPromptBuilder promptBuilder)
    {
        _configService = configService;
        _store = store;
        _paths = paths;
        _promptBuilder = promptBuilder;
    }

    public string GetPullRequestFolder(string organization, string projectName, string repositoryName)
    {
        return Path.Combine(_paths.GetStorageRoot(), "pull-requests", AppPaths.Slugify(organization, "org"), AppPaths.Slugify(projectName, "project"), AppPaths.Slugify(repositoryName, "repo"));
    }

    public string GetSessionPath(string organization, string projectName, string repositoryName, int pullRequestId)
    {
        return Path.Combine(GetPullRequestFolder(organization, projectName, repositoryName), $"pr-{pullRequestId}.session.json");
    }

    public string GetThreadsPath(string organization, string projectName, string repositoryName, int pullRequestId)
    {
        return Path.Combine(GetPullRequestFolder(organization, projectName, repositoryName), $"pr-{pullRequestId}.threads.json");
    }

    public string GetPromptPath(string projectName, string repositoryName, int pullRequestId)
    {
        return Path.Combine(_paths.GetPromptRoot(), $"{AppPaths.Slugify(projectName, "project")}_{AppPaths.Slugify(repositoryName, "repo")}_{pullRequestId}.txt");
    }

    public async Task SaveSessionAsync(AppConfig config, PullRequestSession session, CancellationToken cancellationToken = default)
    {
        var folder = GetPullRequestFolder(session.Organization, session.Project, session.Repository);
        Directory.CreateDirectory(folder);
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await _store.SaveAsync(GetSessionPath(session.Organization, session.Project, session.Repository, session.PullRequestId), session, cancellationToken);
        await _store.SaveAsync(GetThreadsPath(session.Organization, session.Project, session.Repository, session.PullRequestId), session.Threads, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entry = config.PullRequests.FirstOrDefault(pr =>
            pr.Id == session.PullRequestId &&
            string.Equals(pr.Organization, session.Organization, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(pr.Project.Name, session.Project, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(pr.Repository.Name, session.Repository, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            entry = new PullRequestEntry
            {
                Id = session.PullRequestId,
                Organization = session.Organization,
                Project = new ProjectRef { Name = session.Project },
                Repository = new RepositoryRef { Id = session.RepositoryId, Name = session.Repository, LocalPath = config.CurrentContext.Repository?.LocalPath },
                CreatedAt = now
            };
            config.PullRequests.Add(entry);
        }

        entry.Title = session.PullRequestTitle;
        entry.Project ??= new ProjectRef();
        entry.Project.Name = session.Project;
        entry.Repository ??= new RepositoryRef();
        entry.Repository.Name = session.Repository;
        entry.Repository.Id = session.RepositoryId;
        entry.Repository.LocalPath ??= config.CurrentContext.Repository?.LocalPath;
        entry.ThreadsFilePath = GetThreadsPath(session.Organization, session.Project, session.Repository, session.PullRequestId);
        entry.UpdatedAt = now;
        entry.ReviewState = GetReviewState(session);
        entry.Artifacts = UpsertArtifacts(entry.Artifacts, GetSessionPath(session.Organization, session.Project, session.Repository, session.PullRequestId), "pull-request-session", "application/json", now);
        entry.Artifacts = UpsertArtifacts(entry.Artifacts, entry.ThreadsFilePath, "pull-request-session", "application/json", now);

        await _configService.SaveAsync(config, cancellationToken);
    }

    public async Task<PullRequestSession?> LoadSessionAsync(PullRequestEntry entry, CancellationToken cancellationToken = default)
    {
        var sessionPath = entry.Artifacts.FirstOrDefault(a => a.Kind == "pull-request-session")?.Path;
        if (string.IsNullOrWhiteSpace(sessionPath))
        {
            return null;
        }

        return await _store.LoadAsync<PullRequestSession>(sessionPath, cancellationToken);
    }

    public async Task SavePromptAsync(AppConfig config, PullRequestSession session, CancellationToken cancellationToken = default)
    {
        session.GeneratedPrompt = _promptBuilder.BuildFixPlanPrompt(session);
        var promptPath = GetPromptPath(session.Project, session.Repository, session.PullRequestId);
        Directory.CreateDirectory(Path.GetDirectoryName(promptPath)!);
        await File.WriteAllTextAsync(promptPath, session.GeneratedPrompt, cancellationToken);
        await SaveSessionAsync(config, session, cancellationToken);

        var entry = config.PullRequests.First(pr =>
            pr.Id == session.PullRequestId &&
            string.Equals(pr.Organization, session.Organization, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(pr.Project.Name, session.Project, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(pr.Repository.Name, session.Repository, StringComparison.OrdinalIgnoreCase));

        entry.Artifacts = UpsertArtifacts(entry.Artifacts, promptPath, "pull-request-prompt", "text/plain", DateTimeOffset.UtcNow);
        entry.LastViewedArtifactPath = promptPath;
        entry.ReviewState = "prompt-generated";
        await _configService.SaveAsync(config, cancellationToken);
    }

    private static string GetReviewState(PullRequestSession session)
    {
        if (session.Threads.Count == 0)
        {
            return "not-reviewed";
        }

        return session.Threads.All(thread => !string.Equals(thread.Decision, "unreviewed", StringComparison.OrdinalIgnoreCase))
            ? "reviewed"
            : session.Threads.Any(thread => !string.Equals(thread.Decision, "unreviewed", StringComparison.OrdinalIgnoreCase))
                ? "in-progress"
                : "not-reviewed";
    }

    private static List<ArtifactRef> UpsertArtifacts(List<ArtifactRef> artifacts, string? path, string kind, string contentType, DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return artifacts;
        }

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
        }
        else
        {
            existing.ContentType = contentType;
            existing.UpdatedAt = timestamp;
        }

        return artifacts.OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt).ToList();
    }
}


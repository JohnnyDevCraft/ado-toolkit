using AdoToolkit.Config;
using AdoToolkit.Integrations;
using AdoToolkit.Models;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Services;

public sealed class PullRequestImportService
{
    private readonly IAzureDevOpsPullRequestClient _client;
    private readonly PullRequestStorageService _storageService;
    private readonly PullRequestCodeExcerptService _codeExcerptService;

    public PullRequestImportService(IAzureDevOpsPullRequestClient client, PullRequestStorageService storageService, PullRequestCodeExcerptService codeExcerptService)
    {
        _client = client;
        _storageService = storageService;
        _codeExcerptService = codeExcerptService;
    }

    public async Task<IReadOnlyList<AdoPullRequestInfo>> ListActiveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ValidateContext(config, requireRepository: true);
        var repositoryId = await ResolveRepositoryIdAsync(config, cancellationToken);
        var user = await _client.GetAuthenticatedUserAsync(config.CurrentContext.Organization!, config.Pat!, cancellationToken);
        return await _client.GetActivePullRequestsCreatedByAsync(config.CurrentContext.Organization!, config.CurrentContext.Project!.Name!, repositoryId, config.Pat!, user, cancellationToken);
    }

    public async Task<PullRequestSession> ImportAsync(AppConfig config, int pullRequestId, CancellationToken cancellationToken = default)
    {
        ValidateContext(config, requireRepository: true);
        var repositoryId = await ResolveRepositoryIdAsync(config, cancellationToken);
        var title = await _client.GetPullRequestTitleAsync(config.CurrentContext.Organization!, config.CurrentContext.Project!.Name!, repositoryId, pullRequestId, config.Pat!, cancellationToken)
                    ?? $"PR {pullRequestId}";
        var threads = await _client.GetPullRequestThreadsAsync(config.CurrentContext.Organization!, config.CurrentContext.Project!.Name!, repositoryId, pullRequestId, config.Pat!, cancellationToken);

        var session = new PullRequestSession
        {
            Organization = config.CurrentContext.Organization!,
            Project = config.CurrentContext.Project.Name!,
            Repository = config.CurrentContext.Repository!.Name!,
            RepositoryId = repositoryId,
            PullRequestId = pullRequestId,
            PullRequestTitle = title,
            Threads = threads,
            LastRefreshedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(config.CurrentContext.Repository?.LocalPath))
        {
            _codeExcerptService.PopulateThreadExcerpts(session, config.CurrentContext.Repository.LocalPath!);
        }

        await _storageService.SaveSessionAsync(config, session, cancellationToken);
        return session;
    }

    public async Task<PullRequestSession> RefreshAsync(AppConfig config, PullRequestEntry entry, CancellationToken cancellationToken = default)
    {
        config.CurrentContext.Organization = entry.Organization;
        config.CurrentContext.Project = entry.Project;
        config.CurrentContext.Repository = entry.Repository;
        return await ImportAsync(config, entry.Id, cancellationToken);
    }

    private async Task<string> ResolveRepositoryIdAsync(AppConfig config, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(config.CurrentContext.Repository?.Id))
        {
            return config.CurrentContext.Repository.Id!;
        }

        var repositories = await _client.GetRepositoriesAsync(config.CurrentContext.Organization!, config.CurrentContext.Project!.Name!, config.Pat!, cancellationToken);
        var repository = repositories.FirstOrDefault(repo => string.Equals(repo.Name, config.CurrentContext.Repository!.Name, StringComparison.OrdinalIgnoreCase));
        if (repository is null)
        {
            throw new InvalidOperationException($"Repository '{config.CurrentContext.Repository!.Name}' was not found in project '{config.CurrentContext.Project!.Name}'.");
        }

        config.CurrentContext.Repository!.Id = repository.Id;
        return repository.Id;
    }

    private static void ValidateContext(AppConfig config, bool requireRepository)
    {
        if (string.IsNullOrWhiteSpace(config.Pat) ||
            string.IsNullOrWhiteSpace(config.CurrentContext.Organization) ||
            string.IsNullOrWhiteSpace(config.CurrentContext.Project?.Name))
        {
            throw new InvalidOperationException("PAT, current organization, and current project are required.");
        }

        if (requireRepository && string.IsNullOrWhiteSpace(config.CurrentContext.Repository?.Name))
        {
            throw new InvalidOperationException("Current repository is required for pull request workflows. Use `ado config set-repo <RepoName> <RepoPath>` first.");
        }
    }
}


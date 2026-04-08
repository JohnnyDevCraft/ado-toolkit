using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Integrations;

public interface IAzureDevOpsPullRequestClient
{
    Task<AdoAuthenticatedUser?> GetAuthenticatedUserAsync(string organization, string pat, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdoRepositoryInfo>> GetRepositoriesAsync(string organization, string project, string pat, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdoPullRequestInfo>> GetActivePullRequestsCreatedByAsync(string organization, string project, string repositoryId, string pat, AdoAuthenticatedUser? authenticatedUser, CancellationToken cancellationToken = default);

    Task<string?> GetPullRequestTitleAsync(string organization, string project, string repositoryId, int pullRequestId, string pat, CancellationToken cancellationToken = default);

    Task<List<PullRequestThreadRecord>> GetPullRequestThreadsAsync(string organization, string project, string repositoryId, int pullRequestId, string pat, CancellationToken cancellationToken = default);
}


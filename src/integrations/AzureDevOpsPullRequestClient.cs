using System.Text.Json;
using AdoToolkit.Models.PullRequests;

namespace AdoToolkit.Integrations;

public sealed class AzureDevOpsPullRequestClient : IAzureDevOpsPullRequestClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AzureDevOpsHttpClientFactory _httpClientFactory;

    public AzureDevOpsPullRequestClient(AzureDevOpsHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AdoAuthenticatedUser?> GetAuthenticatedUserAsync(string organization, string pat, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{organization}/_apis/connectionData?connectOptions=IncludeAuthenticatedUser&lastChangeId=-1&lastChangeId64=-1&api-version=7.1-preview.1", cancellationToken);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ConnectionDataResponse>(stream, SerializerOptions, cancellationToken);
        return payload?.AuthenticatedUser is null
            ? null
            : new AdoAuthenticatedUser
            {
                Id = payload.AuthenticatedUser.Id,
                DisplayName = payload.AuthenticatedUser.ProviderDisplayName ?? payload.AuthenticatedUser.DisplayName,
                UniqueName = payload.AuthenticatedUser.UniqueName
            };
    }

    public async Task<IReadOnlyList<AdoRepositoryInfo>> GetRepositoriesAsync(string organization, string project, string pat, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{organization}/{Uri.EscapeDataString(project)}/_apis/git/repositories?api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ListResponse<RepositoryDto>>(stream, SerializerOptions, cancellationToken);
        return payload?.Value?
            .Select(repo => new AdoRepositoryInfo { Id = repo.Id ?? string.Empty, Name = repo.Name ?? string.Empty })
            .Where(repo => !string.IsNullOrWhiteSpace(repo.Name))
            .OrderBy(repo => repo.Name, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
    }

    public async Task<IReadOnlyList<AdoPullRequestInfo>> GetActivePullRequestsCreatedByAsync(string organization, string project, string repositoryId, string pat, AdoAuthenticatedUser? authenticatedUser, CancellationToken cancellationToken = default)
    {
        var uri = $"https://dev.azure.com/{organization}/{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests?searchCriteria.status=active";
        if (!string.IsNullOrWhiteSpace(authenticatedUser?.Id))
        {
            uri += $"&searchCriteria.creatorId={Uri.EscapeDataString(authenticatedUser.Id)}";
        }

        uri += "&api-version=7.1";
        using var client = _httpClientFactory.Create(pat);
        using var response = await client.GetAsync(uri, cancellationToken);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ListResponse<PullRequestDto>>(stream, SerializerOptions, cancellationToken);
        var pullRequests = payload?.Value?.Select(pr => new AdoPullRequestInfo
        {
            Id = pr.PullRequestId,
            Title = pr.Title ?? $"PR {pr.PullRequestId}",
            Status = pr.Status ?? string.Empty,
            CreatedById = pr.CreatedBy?.Id,
            CreatedByDisplayName = pr.CreatedBy?.DisplayName,
            CreatedByUniqueName = pr.CreatedBy?.UniqueName
        }).ToList() ?? [];

        if (!string.IsNullOrWhiteSpace(authenticatedUser?.Id))
        {
            return pullRequests.Where(pr => string.Equals(pr.CreatedById, authenticatedUser.Id, StringComparison.OrdinalIgnoreCase)).OrderBy(pr => pr.Id).ToList();
        }

        return pullRequests.OrderBy(pr => pr.Id).ToList();
    }

    public async Task<string?> GetPullRequestTitleAsync(string organization, string project, string repositoryId, int pullRequestId, string pat, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{organization}/{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests/{pullRequestId}?api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<PullRequestDto>(stream, SerializerOptions, cancellationToken);
        return payload?.Title;
    }

    public async Task<List<PullRequestThreadRecord>> GetPullRequestThreadsAsync(string organization, string project, string repositoryId, int pullRequestId, string pat, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{organization}/{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullRequests/{pullRequestId}/threads?api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ListResponse<ThreadDto>>(stream, SerializerOptions, cancellationToken);

        return payload?.Value?
            .Select(thread => new PullRequestThreadRecord
            {
                ThreadId = thread.Id,
                Status = thread.Status,
                IsDeleted = thread.IsDeleted,
                FilePath = thread.ThreadContext?.FilePath,
                Line = thread.ThreadContext?.RightFileStart?.Line ?? thread.ThreadContext?.LeftFileStart?.Line,
                EndLine = thread.ThreadContext?.RightFileEnd?.Line ?? thread.ThreadContext?.LeftFileEnd?.Line,
                Comments = thread.Comments?
                    .Where(comment => !comment.IsDeleted && !string.IsNullOrWhiteSpace(comment.Content))
                    .Select(comment => new PullRequestCommentRecord
                    {
                        ThreadId = thread.Id,
                        CommentId = comment.Id,
                        Author = comment.Author?.DisplayName,
                        Content = comment.Content ?? string.Empty,
                        CommentType = comment.CommentType,
                        PublishedDate = comment.PublishedDate,
                        IsDeleted = comment.IsDeleted,
                        FilePath = thread.ThreadContext?.FilePath,
                        Line = thread.ThreadContext?.RightFileStart?.Line ?? thread.ThreadContext?.LeftFileStart?.Line,
                        EndLine = thread.ThreadContext?.RightFileEnd?.Line ?? thread.ThreadContext?.LeftFileEnd?.Line
                    })
                    .ToList() ?? []
            })
            .Where(thread => !thread.IsDeleted && thread.Comments.Count > 0)
            .OrderBy(thread => thread.ThreadId)
            .ToList() ?? [];
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"Azure DevOps request failed ({(int)response.StatusCode}): {body}");
    }

    private sealed class ListResponse<T> { public List<T>? Value { get; set; } }
    private sealed class ConnectionDataResponse { public IdentityDto? AuthenticatedUser { get; set; } }
    private sealed class RepositoryDto { public string? Id { get; set; } public string? Name { get; set; } }
    private sealed class PullRequestDto
    {
        public int PullRequestId { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public IdentityDto? CreatedBy { get; set; }
    }
    private sealed class ThreadDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public bool IsDeleted { get; set; }
        public ThreadContextDto? ThreadContext { get; set; }
        public List<CommentDto>? Comments { get; set; }
    }
    private sealed class ThreadContextDto
    {
        public string? FilePath { get; set; }
        public LinePositionDto? LeftFileStart { get; set; }
        public LinePositionDto? LeftFileEnd { get; set; }
        public LinePositionDto? RightFileStart { get; set; }
        public LinePositionDto? RightFileEnd { get; set; }
    }
    private sealed class LinePositionDto { public int? Line { get; set; } }
    private sealed class CommentDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? CommentType { get; set; }
        public IdentityDto? Author { get; set; }
        public DateTimeOffset? PublishedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    private sealed class IdentityDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? ProviderDisplayName { get; set; }
        public string? UniqueName { get; set; }
    }
}


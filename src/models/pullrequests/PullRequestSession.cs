namespace AdoToolkit.Models.PullRequests;

public sealed class PullRequestSession
{
    public string Organization { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string? RepositoryId { get; set; }

    public string PullRequestTitle { get; set; } = string.Empty;

    public int PullRequestId { get; set; }

    public List<PullRequestThreadRecord> Threads { get; set; } = [];

    public string GeneratedPrompt { get; set; } = string.Empty;

    public DateTimeOffset? LastRefreshedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}


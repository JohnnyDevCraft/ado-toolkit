namespace AdoToolkit.Models.PullRequests;

public sealed class AdoPullRequestInfo
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? CreatedById { get; init; }

    public string? CreatedByDisplayName { get; init; }

    public string? CreatedByUniqueName { get; init; }
}


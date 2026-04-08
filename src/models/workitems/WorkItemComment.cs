namespace AdoToolkit.Models.WorkItems;

public sealed class WorkItemComment
{
    public string CommentId { get; init; } = string.Empty;

    public string? AuthoredBy { get; init; }

    public DateTimeOffset? AuthoredAt { get; init; }

    public string RenderedText { get; init; } = string.Empty;
}


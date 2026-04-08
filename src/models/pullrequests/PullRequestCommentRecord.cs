namespace AdoToolkit.Models.PullRequests;

public sealed class PullRequestCommentRecord
{
    public int ThreadId { get; set; }

    public int CommentId { get; set; }

    public string? Author { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? CommentType { get; set; }

    public string? Status { get; set; }

    public string? FilePath { get; set; }

    public int? Line { get; set; }

    public int? EndLine { get; set; }

    public DateTimeOffset? PublishedDate { get; set; }

    public bool IsDeleted { get; set; }
}


namespace AdoToolkit.Models.PullRequests;

public sealed class PullRequestThreadRecord
{
    public int ThreadId { get; set; }

    public string? Status { get; set; }

    public bool IsDeleted { get; set; }

    public string? FilePath { get; set; }

    public int? Line { get; set; }

    public int? EndLine { get; set; }

    public string Decision { get; set; } = "unreviewed";

    public string? DeveloperNotes { get; set; }

    public List<PullRequestCommentRecord> Comments { get; set; } = [];

    public string? CodeExcerpt { get; set; }

    public int? CodeExcerptStartLine { get; set; }

    public string? CodeExcerptLanguage { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }
}


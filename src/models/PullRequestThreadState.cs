namespace AdoToolkit.Models;

public sealed class PullRequestThreadState
{
    public int ThreadId { get; set; }

    public string Decision { get; set; } = "unreviewed";

    public string? FixInstruction { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }
}


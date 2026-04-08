namespace AdoToolkit.Models.WorkItems;

public sealed class WorkItemReference
{
    public int SourceWorkItemId { get; init; }

    public string SourceKind { get; init; } = string.Empty;

    public string SourceLabel { get; init; } = string.Empty;

    public string? SourceCommentId { get; init; }

    public int ReferencedWorkItemId { get; init; }

    public string ReferenceText { get; init; } = string.Empty;

    public string? RelationshipType { get; init; }

    public bool Retrieved { get; set; }
}


namespace AdoToolkit.Models.WorkItems;

public sealed class NormalizedWorkItem
{
    public int Id { get; init; }

    public string ProjectId { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string Organization { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string WorkItemType { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string? AssignedTo { get; init; }

    public string? Tags { get; init; }

    public string? AreaPath { get; init; }

    public string? IterationPath { get; init; }

    public string? Url { get; init; }

    public List<DescriptionField> DescriptionFields { get; init; } = [];

    public List<int> ParentIds { get; init; } = [];

    public List<int> ChildIds { get; init; } = [];

    public List<int> RelatedIds { get; init; } = [];

    public List<WorkItemComment> Comments { get; init; } = [];
}


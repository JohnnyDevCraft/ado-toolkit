namespace AdoToolkit.Models.WorkItems;

public sealed class RelatedWorkItem
{
    public required NormalizedWorkItem WorkItem { get; init; }

    public List<WorkItemReference> References { get; init; } = [];
}


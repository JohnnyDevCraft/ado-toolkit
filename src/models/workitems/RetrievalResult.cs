namespace AdoToolkit.Models.WorkItems;

public sealed class RetrievalResult
{
    public required NormalizedWorkItem RootWorkItem { get; init; }

    public List<NormalizedWorkItem> Parents { get; init; } = [];

    public List<NormalizedWorkItem> Children { get; init; } = [];

    public List<RelatedWorkItem> RelatedWorkItems { get; init; } = [];

    public List<string> Notes { get; init; } = [];
}


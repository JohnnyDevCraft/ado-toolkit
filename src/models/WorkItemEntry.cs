namespace AdoToolkit.Models;

public sealed class WorkItemEntry
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string Organization { get; set; } = string.Empty;

    public ProjectRef Project { get; set; } = new();

    public List<ArtifactRef> Artifacts { get; set; } = [];

    public string? LastViewedArtifactPath { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<string> Tags { get; set; } = [];
}


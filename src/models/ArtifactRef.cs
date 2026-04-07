namespace AdoToolkit.Models;

public sealed class ArtifactRef
{
    public string Kind { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? ContentType { get; set; }
}


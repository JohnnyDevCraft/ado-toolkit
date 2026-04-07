namespace AdoToolkit.Models;

public sealed class CurrentContext
{
    public string? Organization { get; set; }

    public ProjectRef? Project { get; set; }

    public RepositoryRef? Repository { get; set; }
}

